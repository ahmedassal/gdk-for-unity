using System.Collections.Generic;
using Improbable.Gdk.Core.Commands;

namespace Improbable.Gdk.Core
{
    public class WorldCommandsReceivedStorage : ICommandDiffStorage
        , IDiffCommandResponseStorage<WorldCommands.CreateEntity.ReceivedResponse>
        , IDiffCommandResponseStorage<WorldCommands.DeleteEntity.ReceivedResponse>
        , IDiffCommandResponseStorage<WorldCommands.ReserveEntityIds.ReceivedResponse>
        , IDiffCommandResponseStorage<WorldCommands.EntityQuery.ReceivedResponse>
    {
        private readonly MessageList<WorldCommands.CreateEntity.ReceivedResponse> createEntityResponses =
            new MessageList<WorldCommands.CreateEntity.ReceivedResponse>();

        private readonly MessageList<WorldCommands.DeleteEntity.ReceivedResponse> deleteEntityResponses =
            new MessageList<WorldCommands.DeleteEntity.ReceivedResponse>();

        private readonly MessageList<WorldCommands.ReserveEntityIds.ReceivedResponse> reserveEntityIdsResponses =
            new MessageList<WorldCommands.ReserveEntityIds.ReceivedResponse>();

        private readonly MessageList<WorldCommands.EntityQuery.ReceivedResponse> entityQueryResponses =
            new MessageList<WorldCommands.EntityQuery.ReceivedResponse>();

        private readonly Comparer comparer = new Comparer();

        private bool createEntitySorted;
        private bool deleteEntitySorted;
        private bool reserveEntityIdsSorted;
        private bool entityQueriesSorted;

        public void Clear()
        {
            createEntityResponses.Clear();
            deleteEntityResponses.Clear();
            reserveEntityIdsResponses.Clear();
            entityQueryResponses.Clear();
            createEntitySorted = false;
            deleteEntitySorted = false;
            reserveEntityIdsSorted = false;
            entityQueriesSorted = false;
        }

        public void AddResponse(WorldCommands.CreateEntity.ReceivedResponse response)
        {
            createEntityResponses.Add(response);
        }

        public void AddResponse(WorldCommands.DeleteEntity.ReceivedResponse response)
        {
            deleteEntityResponses.Add(response);
        }

        public void AddResponse(WorldCommands.ReserveEntityIds.ReceivedResponse response)
        {
            reserveEntityIdsResponses.Add(response);
        }

        public void AddResponse(WorldCommands.EntityQuery.ReceivedResponse response)
        {
            entityQueryResponses.Add(response);
        }

        MessagesSpan<WorldCommands.CreateEntity.ReceivedResponse>
            IDiffCommandResponseStorage<WorldCommands.CreateEntity.ReceivedResponse>.GetResponses()
        {
            return createEntityResponses.Slice();
        }

        MessagesSpan<WorldCommands.DeleteEntity.ReceivedResponse>
            IDiffCommandResponseStorage<WorldCommands.DeleteEntity.ReceivedResponse>.GetResponses()
        {
            return deleteEntityResponses.Slice();
        }

        MessagesSpan<WorldCommands.ReserveEntityIds.ReceivedResponse>
            IDiffCommandResponseStorage<WorldCommands.ReserveEntityIds.ReceivedResponse>.GetResponses()
        {
            return reserveEntityIdsResponses.Slice();
        }

        MessagesSpan<WorldCommands.EntityQuery.ReceivedResponse>
            IDiffCommandResponseStorage<WorldCommands.EntityQuery.ReceivedResponse>.GetResponses()
        {
            return entityQueryResponses.Slice();
        }

        WorldCommands.CreateEntity.ReceivedResponse? IDiffCommandResponseStorage<WorldCommands.CreateEntity.ReceivedResponse>.GetResponse(CommandRequestId requestId)
        {
            if (!createEntitySorted)
            {
                createEntityResponses.Sort(comparer);
                createEntitySorted = true;
            }

            var responseIndex = createEntityResponses.GetResponseIndex(requestId);
            return responseIndex.HasValue
                ? createEntityResponses[responseIndex.Value]
                : (WorldCommands.CreateEntity.ReceivedResponse?) null;
        }

        WorldCommands.DeleteEntity.ReceivedResponse?
            IDiffCommandResponseStorage<WorldCommands.DeleteEntity.ReceivedResponse>.GetResponse(CommandRequestId requestId)
        {
            if (!deleteEntitySorted)
            {
                deleteEntityResponses.Sort(comparer);
                deleteEntitySorted = true;
            }

            var responseIndex = deleteEntityResponses.GetResponseIndex(requestId);
            return responseIndex.HasValue
                ? deleteEntityResponses[responseIndex.Value]
                : (WorldCommands.DeleteEntity.ReceivedResponse?) null;
        }

        WorldCommands.ReserveEntityIds.ReceivedResponse?
            IDiffCommandResponseStorage<WorldCommands.ReserveEntityIds.ReceivedResponse>.GetResponse(CommandRequestId requestId)
        {
            if (!reserveEntityIdsSorted)
            {
                reserveEntityIdsResponses.Sort(comparer);
                reserveEntityIdsSorted = true;
            }

            var responseIndex = reserveEntityIdsResponses.GetResponseIndex(requestId);
            return responseIndex.HasValue
                ? reserveEntityIdsResponses[responseIndex.Value]
                : (WorldCommands.ReserveEntityIds.ReceivedResponse?) null;
        }

        WorldCommands.EntityQuery.ReceivedResponse?
            IDiffCommandResponseStorage<WorldCommands.EntityQuery.ReceivedResponse>.GetResponse(CommandRequestId requestId)
        {
            if (!entityQueriesSorted)
            {
                entityQueryResponses.Sort(comparer);
                entityQueriesSorted = true;
            }

            var responseIndex = entityQueryResponses.GetResponseIndex(requestId);
            return responseIndex.HasValue
                ? entityQueryResponses[responseIndex.Value]
                : (WorldCommands.EntityQuery.ReceivedResponse?) null;
        }

        private class Comparer : IComparer<WorldCommands.CreateEntity.ReceivedResponse>,
            IComparer<WorldCommands.DeleteEntity.ReceivedResponse>,
            IComparer<WorldCommands.ReserveEntityIds.ReceivedResponse>,
            IComparer<WorldCommands.EntityQuery.ReceivedResponse>
        {
            public int Compare(WorldCommands.CreateEntity.ReceivedResponse x,
                WorldCommands.CreateEntity.ReceivedResponse y)
            {
                return x.RequestId.CompareTo(y.RequestId);
            }

            public int Compare(WorldCommands.DeleteEntity.ReceivedResponse x,
                WorldCommands.DeleteEntity.ReceivedResponse y)
            {
                return x.RequestId.CompareTo(y.RequestId);
            }

            public int Compare(WorldCommands.ReserveEntityIds.ReceivedResponse x,
                WorldCommands.ReserveEntityIds.ReceivedResponse y)
            {
                return x.RequestId.CompareTo(y.RequestId);
            }

            public int Compare(WorldCommands.EntityQuery.ReceivedResponse x,
                WorldCommands.EntityQuery.ReceivedResponse y)
            {
                return x.RequestId.CompareTo(y.RequestId);
            }
        }
    }
}
