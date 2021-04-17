using System;
using System.Collections.Concurrent;

namespace ServerBase.ServerTasks
{
    public interface IWaitingNodesContext
    {
        WaitingNode Get(string waitingId);
    }

    public class WaitingNodesContext
    { 
        private readonly ConcurrentDictionary<string, WaitingNode> _waitingNodes = new ConcurrentDictionary<string, WaitingNode>();

        public WaitingNode Get(string waitingId)
        {
            return _waitingNodes.GetOrAdd(waitingId, (key) => new WaitingNode(key));
		}
    }

}
