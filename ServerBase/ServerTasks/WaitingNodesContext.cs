using System;
using System.Collections.Concurrent;

namespace ServerBase.ServerTasks
{
    public interface IWaitingNodesContext
    {
        WaitingNode Get(string waitingId);
    }

    public class WaitingNodesContext : IWaitingNodesContext
    {
        private readonly ConcurrentDictionary<string, WaitingNode> _waitingNodes = new ConcurrentDictionary<string, WaitingNode>();

        public WaitingNode Get(string waitingId)
        {
            // 아직 지우지 않고 있다
            return _waitingNodes.GetOrAdd(waitingId, (key) => new WaitingNode(key));
        }
    }

}
