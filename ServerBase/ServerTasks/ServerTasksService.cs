using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerBase.ServerTasks
{
    public class ServerTasksService
    {
        private readonly IWaitingNodesContext _waitingNodes;

        public ServerTasksService(IWaitingNodesContext waitingNodes)
        {
            _waitingNodes = waitingNodes;
        }

        public async Task AddWaitingTasksAsync(params ServerTask[] serverTasks)
        {
            await RunServerTaskAsync(serverTasks);
        }

        private async Task AwaitAsync(string[] waitingIds, Func<Task<object[]>> worker)
        {
            var waitingNodes = new List<WaitingNode>(waitingIds.Length);
            foreach (var waitingId in waitingIds)
            {
                var waitingNode = _waitingNodes.Get(waitingId);
                waitingNodes.Add(waitingNode);
            }

            var work = new SerializedWork(waitingNodes.ToArray(), worker);
            await work.AwaitAsync();
        }

        private async Task RunServerTaskAsync(ServerTask[] serverTasks)
        {
            var tasks = new Queue<ServerTask>(serverTasks.Length * 2);
            foreach (var serverTask in serverTasks)
            {
                tasks.Enqueue(serverTask);
            }

            try
            {
                //Logger.WriteDebugLine($"{serverTask.GetType().Name} Wait...");

                while (tasks.Count > 0)
                {
                    var task = tasks.Dequeue();

                    var nextTasks = Array.Empty<ServerTask>();
                    await AwaitAsync(task.WaitingIds, async () =>
                    {
                        return await task.RunAsync();
                    });
                    foreach (var nextTask in task.NextTasks)
                    {
                        tasks.Enqueue(nextTask);
                    }
                }
                //Logger.WriteDebugLine($"{serverTask.GetType().Name} Complete");
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}