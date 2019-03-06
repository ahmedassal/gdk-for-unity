// ===========
// DO NOT EDIT - this file is automatically regenerated.
// ===========

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Collections;
using Improbable.Worker.CInterop;
using Improbable.Gdk.Core;
using Improbable.Gdk.ReactiveComponents;

namespace Improbable.Gdk.Tests.ComponentsWithNoFields
{
    public partial class ComponentWithNoFieldsWithCommands
    {
        internal class ReactiveComponentReplicator : IReactiveComponentReplicationHandler
        {
            public uint ComponentId => 1005;

            public EntityArchetypeQuery EventQuery => new EntityArchetypeQuery
            {
                All = new[]
                {
                    ComponentType.ReadOnly<SpatialEntityId>()
                },
                Any = Array.Empty<ComponentType>(),
                None = Array.Empty<ComponentType>(),
            };

            public EntityArchetypeQuery[] CommandQueries => new EntityArchetypeQuery[]
            {
                new EntityArchetypeQuery()
                {
                    All = new[]
                    {
                        ComponentType.Create<Improbable.Gdk.Tests.ComponentsWithNoFields.ComponentWithNoFieldsWithCommands.CommandSenders.Cmd>(),
                        ComponentType.Create<Improbable.Gdk.Tests.ComponentsWithNoFields.ComponentWithNoFieldsWithCommands.CommandResponders.Cmd>(),
                    },
                    Any = Array.Empty<ComponentType>(),
                    None = Array.Empty<ComponentType>(),
                },
            };

            public void SendEvents(ComponentGroup replicationGroup, ComponentSystemBase system, ComponentUpdateSystem componentUpdateSystem)
            {
            }

            public void SendCommands(ComponentGroup commandGroup, ComponentSystemBase system, CommandSystem commandSystem)
            {
                Profiler.BeginSample("ComponentWithNoFieldsWithCommands");
                var entityType = system.GetArchetypeChunkEntityType();
                {
                    var senderType = system.GetArchetypeChunkComponentType<Improbable.Gdk.Tests.ComponentsWithNoFields.ComponentWithNoFieldsWithCommands.CommandSenders.Cmd>(true);
                    var responderType = system.GetArchetypeChunkComponentType<Improbable.Gdk.Tests.ComponentsWithNoFields.ComponentWithNoFieldsWithCommands.CommandResponders.Cmd>(true);

                    var chunks = commandGroup.CreateArchetypeChunkArray(Allocator.TempJob);
                    foreach (var chunk in chunks)
                    {
                        var entities = chunk.GetNativeArray(entityType);
                        var senders = chunk.GetNativeArray(senderType);
                        var responders = chunk.GetNativeArray(responderType);
                        for (var i = 0; i < senders.Length; i++)
                        {
                            var requests = senders[i].RequestsToSend;
                            var responses = responders[i].ResponsesToSend;
                            if (requests.Count > 0)
                            {
                                foreach (var request in requests)
                                {
                                    commandSystem.SendCommand(request, entities[i]);
                                }

                                requests.Clear();
                            }

                            if (responses.Count > 0)
                            {
                                foreach (var response in responses)
                                {
                                    commandSystem.SendResponse(response);
                                }

                                responses.Clear();
                            }
                        }
                    }

                    chunks.Dispose();
                }

                Profiler.EndSample();
            }
        }

        internal class ComponentCleanup : ComponentCleanupHandler
        {
            public override EntityArchetypeQuery CleanupArchetypeQuery => new EntityArchetypeQuery
            {
                All = Array.Empty<ComponentType>(),
                Any = new ComponentType[]
                {
                    ComponentType.Create<ComponentAdded<Improbable.Gdk.Tests.ComponentsWithNoFields.ComponentWithNoFieldsWithCommands.Component>>(),
                    ComponentType.Create<ComponentRemoved<Improbable.Gdk.Tests.ComponentsWithNoFields.ComponentWithNoFieldsWithCommands.Component>>(),
                    ComponentType.Create<Improbable.Gdk.Tests.ComponentsWithNoFields.ComponentWithNoFieldsWithCommands.ReceivedUpdates>(),
                    ComponentType.Create<AuthorityChanges<Improbable.Gdk.Tests.ComponentsWithNoFields.ComponentWithNoFieldsWithCommands.Component>>(),
                    ComponentType.Create<CommandRequests.Cmd>(),
                    ComponentType.Create<CommandResponses.Cmd>(),
                },
                None = Array.Empty<ComponentType>(),
            };

            public override void CleanComponents(ComponentGroup group, ComponentSystemBase system,
                EntityCommandBuffer buffer)
            {
                var entityType = system.GetArchetypeChunkEntityType();
                var componentAddedType = system.GetArchetypeChunkComponentType<ComponentAdded<Improbable.Gdk.Tests.ComponentsWithNoFields.ComponentWithNoFieldsWithCommands.Component>>();
                var componentRemovedType = system.GetArchetypeChunkComponentType<ComponentRemoved<Improbable.Gdk.Tests.ComponentsWithNoFields.ComponentWithNoFieldsWithCommands.Component>>();
                var receivedUpdateType = system.GetArchetypeChunkComponentType<Improbable.Gdk.Tests.ComponentsWithNoFields.ComponentWithNoFieldsWithCommands.ReceivedUpdates>();
                var authorityChangeType = system.GetArchetypeChunkComponentType<AuthorityChanges<Improbable.Gdk.Tests.ComponentsWithNoFields.ComponentWithNoFieldsWithCommands.Component>>();

                var cmdRequestType = system.GetArchetypeChunkComponentType<CommandRequests.Cmd>();
                var cmdResponseType = system.GetArchetypeChunkComponentType<CommandResponses.Cmd>();

                var chunkArray = group.CreateArchetypeChunkArray(Allocator.TempJob);

                foreach (var chunk in chunkArray)
                {
                    var entities = chunk.GetNativeArray(entityType);

                    // Updates
                    if (chunk.Has(receivedUpdateType))
                    {
                        var updateArray = chunk.GetNativeArray(receivedUpdateType);
                        for (int i = 0; i < entities.Length; ++i)
                        {
                            buffer.RemoveComponent<Improbable.Gdk.Tests.ComponentsWithNoFields.ComponentWithNoFieldsWithCommands.ReceivedUpdates>(entities[i]);
                            var updateList = updateArray[i].Updates;

                            // Pool update lists to avoid excessive allocation
                            updateList.Clear();
                            Improbable.Gdk.Tests.ComponentsWithNoFields.ComponentWithNoFieldsWithCommands.Update.Pool.Push(updateList);

                            ReferenceTypeProviders.UpdatesProvider.Free(updateArray[i].handle);
                        }
                    }

                    // Component Added
                    if (chunk.Has(componentAddedType))
                    {
                        for (int i = 0; i < entities.Length; ++i)
                        {
                            buffer.RemoveComponent<ComponentAdded<Improbable.Gdk.Tests.ComponentsWithNoFields.ComponentWithNoFieldsWithCommands.Component>>(entities[i]);
                        }
                    }

                    // Component Removed
                    if (chunk.Has(componentRemovedType))
                    {
                        for (int i = 0; i < entities.Length; ++i)
                        {
                            buffer.RemoveComponent<ComponentRemoved<Improbable.Gdk.Tests.ComponentsWithNoFields.ComponentWithNoFieldsWithCommands.Component>>(entities[i]);
                        }
                    }

                    // Authority
                    if (chunk.Has(authorityChangeType))
                    {
                        var authorityChangeArray = chunk.GetNativeArray(authorityChangeType);
                        for (int i = 0; i < entities.Length; ++i)
                        {
                            buffer.RemoveComponent<AuthorityChanges<Improbable.Gdk.Tests.ComponentsWithNoFields.ComponentWithNoFieldsWithCommands.Component>>(entities[i]);
                            AuthorityChangesProvider.Free(authorityChangeArray[i].Handle);
                        }
                    }

                    // Cmd Command
                    if (chunk.Has(cmdRequestType))
                    {
                            var cmdRequestArray = chunk.GetNativeArray(cmdRequestType);
                        for (int i = 0; i < entities.Length; ++i)
                        {
                            buffer.RemoveComponent<CommandRequests.Cmd>(entities[i]);
                            ReferenceTypeProviders.CmdRequestsProvider.Free(cmdRequestArray[i].CommandListHandle);
                        }
                    }

                    if (chunk.Has(cmdResponseType))
                    {
                        var cmdResponseArray = chunk.GetNativeArray(cmdResponseType);
                        for (int i = 0; i < entities.Length; ++i)
                        {
                            buffer.RemoveComponent<CommandResponses.Cmd>(entities[i]);
                            ReferenceTypeProviders.CmdResponsesProvider.Free(cmdResponseArray[i].CommandListHandle);
                        }
                    }
                }

                chunkArray.Dispose();
            }
        }

        internal class AcknowledgeAuthorityLossHandler : AbstractAcknowledgeAuthorityLossHandler
       {
            public override EntityArchetypeQuery Query => new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    ComponentType.ReadOnly<AuthorityLossImminent<Improbable.Gdk.Tests.ComponentsWithNoFields.ComponentWithNoFieldsWithCommands.Component>>(),
                    ComponentType.ReadOnly<SpatialEntityId>()
                },
                Any = Array.Empty<ComponentType>(),
                None = Array.Empty<ComponentType>()
            };

            public override void AcknowledgeAuthorityLoss(ComponentGroup group, ComponentSystemBase system,
                ComponentUpdateSystem updateSystem)
            {
                var authorityLossType = system.GetArchetypeChunkComponentType<AuthorityLossImminent<Improbable.Gdk.Tests.ComponentsWithNoFields.ComponentWithNoFieldsWithCommands.Component>>();
                var spatialEntityType = system.GetArchetypeChunkComponentType<SpatialEntityId>();

                var chunkArray = group.CreateArchetypeChunkArray(Allocator.TempJob);

                foreach (var chunk in chunkArray)
                {
                    var authorityArray = chunk.GetNativeArray(authorityLossType);
                    var spatialEntityIdArray = chunk.GetNativeArray(spatialEntityType);

                    for (int i = 0; i < authorityArray.Length; ++i)
                    {
                        if (authorityArray[i].AcknowledgeAuthorityLoss)
                        {
                            updateSystem.AcknowledgeAuthorityLoss(spatialEntityIdArray[i].EntityId,
                                1005);
                        }
                    }
                }

                chunkArray.Dispose();
            }
        }
    }
}
