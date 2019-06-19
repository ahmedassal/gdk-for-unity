using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Improbable.Gdk.Tools;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Improbable.Gdk.Mobile
{
    public static class iOSLaunchUtils
    {
        private static readonly string XCodeProjectPath = Path.GetFullPath(Path.Combine(Common.BuildScratchDirectory, "MobileClient@iOS", "MobileClient@iOS"));
        private static readonly string DerivedDataPath = Path.GetFullPath(Path.Combine(Common.BuildScratchDirectory, "ios-build"));
        private static readonly string XCodeProjectFile = "Unity-iPhone.xcodeproj";

        private static readonly Regex nameRegex = new Regex("^[a-z|A-Z|\\s|0-9]+");
        private static readonly Regex simulatorUIDRegex = new Regex("\\[([A-Z]|[0-9]|-)+\\]");
        private static readonly Regex deviceUIDRegex = new Regex("\\[([a-z]|[0-9])+\\]");
        
        public static Dictionary<string, string> RetrieveAvailableiOSSimulators()
        {
            var availableSimulators = new Dictionary<string, string>();

            // Check if we have a physical device connected
            var exitCode = RedirectedProcess.Command("instruments")
                .WithArgs("-s", "devices")
                .AddOutputProcessing(message =>
                {
                    // get all simulators
                    if (message.Contains("iPhone") || message.Contains("iPad"))
                    {
                        if (simulatorUIDRegex.IsMatch(message))
                        {
                            var simulatorUID = simulatorUIDRegex.Match(message).Value.Trim('[', ']');
                            availableSimulators[nameRegex.Match(message).Value] = simulatorUID;
                        }
                    }
                })
                .RedirectOutputOptions(OutputRedirectBehaviour.None)
                .Run();

            if (exitCode != 0)
            {
                Debug.LogError("Failed to find iOS Simulators. Make sure you have the Command line tools for XCode (https://developer.apple.com/download/more/) installed and check the logs.");
            }

            return availableSimulators;
        }

        public static Dictionary<string, string> RetrieveAvailableiOSDevices()
        {
            var availableDevices = new Dictionary<string, string>();
            var exitCode = RedirectedProcess.Command("instruments")
                .WithArgs("-s", "devices")
                .AddOutputProcessing(message =>
                {
                    if (deviceUIDRegex.IsMatch(message))
                    {
                        var deviceUID = deviceUIDRegex.Match(message).Value.Trim('[', ']');
                        availableDevices[nameRegex.Match(message).Value] = deviceUID;
                    }
                })
                .RedirectOutputOptions(OutputRedirectBehaviour.None)
                .Run();

            if (exitCode != 0)
            {
                Debug.LogError("Failed to find connected iOS devices. Make sure you have the Command line tools for XCode (https://developer.apple.com/download/more/) installed and check the logs.");
            }
            
            return availableDevices;
        }
        
        public static void Build(string developmentTeamId)
        {
            try
            {
                EditorUtility.DisplayProgressBar("Preparing your Mobile Client", "Building your XCode project", 0f);

                if (!Directory.Exists(XCodeProjectPath))
                {
                    Debug.LogError("Was not able to find an XCode project. Did you build your iOS worker?");
                    return;
                }

                if (string.IsNullOrEmpty(developmentTeamId))
                {
                    Debug.LogError("Development Team Id was not specified. Unable to build the XCode project.");
                    return;
                }

                if (!TryBuildXCodeProject(developmentTeamId))
                {
                    Debug.LogError(
                        $"Failed to build your XCode project. Make sure you have the Command line tools for XCode (https://developer.apple.com/download/more/) installed and check the logs.");
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }
        
        public static void Launch(bool shouldConnectLocally, string deviceId, string runtimeIp, bool useSimulator)
        {
            try 
            {
                EditorUtility.DisplayProgressBar("Preparing your Mobile Client", "Preparing launch arguments", 0.0f);

                if (!TryGetXCTestRunPath(useSimulator, out var xcTestRunPath))
                {
                    Debug.LogError(
                        "Unable to find a xctestrun file for the correct architecture. Did you build your client using the correct Target SDK? " +
                        "Go to Project Settings > Player > iOS > Other Settings > Target SDK to select the correct one before building your iOS worker.");
                    return;
                }

                var arguments = MobileLaunchUtils.PrepareArguments(shouldConnectLocally, runtimeIp);

                if (!TryModifyEnvironmentVariables(xcTestRunPath, arguments))
                {
                    Debug.LogError($"Was unable to read and modify {xcTestRunPath}.");
                    return;
                }
                
                if (useSimulator)
                {
                    EditorUtility.DisplayProgressBar("Launching Mobile Client", "Start iOS Simulator", 0.5f);

                    // Start simulator
                    if (RedirectedProcess.Command("xcrun")
                        .WithArgs("instruments", "-w", deviceId, "-t", "Blank")
                        .Run() != 0)
                    {
                        Debug.LogError("Was unable to start iOS Simulator.");
                        return;
                    }
                }

                EditorUtility.DisplayProgressBar("Launching Mobile Client", "Installing your app", 0.7f);

                if (!TryLaunchApplication(deviceId, xcTestRunPath))
                {
                    Debug.LogError("Failed to start app on iOS device.");
                }

                EditorUtility.DisplayProgressBar("Launching Mobile Client", "Done", 1.0f);
            }
            finally
            {
                var traceDirectories = Directory
                    .GetDirectories(Path.Combine(Application.dataPath, ".."), "*.trace")
                    .Where(s => s.EndsWith(".trace"));
                foreach (var directory in traceDirectories)
                {
                    Directory.Delete(directory, true);
                }
                
                EditorUtility.ClearProgressBar();
            }
        }
        
        private static bool TryBuildXCodeProject(string developmentTeamId)
        {
            return RedirectedProcess.Command("xcodebuild")
                .WithArgs("build-for-testing",
                    "-project", Path.Combine(XCodeProjectPath, XCodeProjectFile),
                    "-derivedDataPath", DerivedDataPath,
                    "-scheme", "Unity-iPhone",
                    $"DEVELOPMENT_TEAM={developmentTeamId}",
                    "-allowProvisioningUpdates")
                .Run() == 0;
        }

        private static bool TryLaunchApplication(string deviceId, string filePath)
        {
            var command = "osascript";
            var commandArgs = $@"-e 'tell application ""Terminal""
                                     activate
                                     do script ""xcodebuild test-without-building -destination 'id={deviceId}' -xctestrun {filePath}""
                                     end tell'";

            var processInfo = new ProcessStartInfo(command, commandArgs)
            {
                CreateNoWindow = false,
                UseShellExecute = true,
                WorkingDirectory = Common.SpatialProjectRootDir
            };

            var process = Process.Start(processInfo);

            return process != null;
        }

        private static bool TryGetXCTestRunPath(bool useSimulator, out string xctestrunPath)
        {
            if (!Directory.Exists(DerivedDataPath))
            {
                xctestrunPath = string.Empty;
                return false;
            }
            
            var files = Directory.GetFiles(DerivedDataPath, "*.xctestrun", SearchOption.AllDirectories);
            xctestrunPath = useSimulator
                ? files.FirstOrDefault(file => file.Contains("iphonesimulator")) 
                : files.FirstOrDefault(file => file.Contains("iphoneos"));

            return !string.IsNullOrEmpty(xctestrunPath);
        }

        private static bool TryModifyEnvironmentVariables(string filePath, string arguments)
        { 
            /*
             * How to add SpatialOS arguments to the game as iOS environment variables
             * The xctestrun file contains the launch arguments for your game
             * However it is structured slightly different from most XML docs by only using names like
             * "key", "dict", "string" as their node names.
             * We need to iterate through the XML file and find the node that contains "EnvironmentVariables" as a text
             * and then add the values to the next node, containing the actual environment variables.
             *
             * <key>EnvironmentVariables</key>
             * <dict>
             *     <key>OS_ACTIVITY_DT_MODE</key>
             *     <string>YES</string>
             *     <key>SQLITE_ENABLE_THREAD_ASSERTIONS</key>
             *     <string>1</string>
             *     <key>SPATIALOS_ARGUMENTS</key>
             *     <string>+environment local +receptionistHost 192.168.0.10 </string>
             * </dict>
             */
            
            try
            {
                var doc = new XmlDocument();
                doc.Load(filePath);
                // Navigate to the <dict> node containing all the parameters to launch the client
                var rootNode = doc.DocumentElement.ChildNodes[0].ChildNodes[1];
                var envKeyNode = rootNode.ChildNodes.Cast<XmlNode>().FirstOrDefault(node => node.InnerText == "EnvironmentVariables");
                var envValueNode = envKeyNode.NextSibling;
                var spatialKeyNode = envValueNode.ChildNodes.Cast<XmlNode>()
                    .FirstOrDefault(node => node.InnerText == LaunchArguments.iOSEnvironmentKey);

                if (spatialKeyNode != null)
                {
                    var spatialValueNode = spatialKeyNode.NextSibling;
                    spatialValueNode.InnerText = arguments;
                }
                else
                {
                    spatialKeyNode = doc.CreateNode("element", "key", string.Empty);
                    spatialKeyNode.InnerText = LaunchArguments.iOSEnvironmentKey;
                    var spatialValueNode = doc.CreateNode("element", "string", string.Empty);
                    spatialValueNode.InnerText = arguments;
                    envValueNode.AppendChild(spatialKeyNode);
                    envValueNode.AppendChild(spatialValueNode);
                }

                doc.Save(filePath);

                // UTY-2068: We currently get invalid XML using the XMLDocument object, we need to identify why this happens and fix it.
                var text = File.ReadAllText(filePath);
                text = text.Replace("[]>", ">");
                File.WriteAllText(filePath, text);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return false;
            }
        }
    }
}
