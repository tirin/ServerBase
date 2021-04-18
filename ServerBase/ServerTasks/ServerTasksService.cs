using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ServerBase.ServerTasks
{
    public interface IServerTasksService
    {
        Task<object[]> RunAsync(params IServerTask[] serverTasks);
    }

    public class ServerTasksService : IServerTasksService
    {
        private readonly ILogger<ServerTasksService> _logger;
        private readonly IWaitingNodesContext _waitingNodes;

        public ServerTasksService(ILogger<ServerTasksService> logger, IWaitingNodesContext waitingNodes)
        {
            _logger = logger;
            _waitingNodes = waitingNodes;
        }

        public async Task<object[]> RunAsync(params IServerTask[] serverTasks)
        {
            return await RunServerTasksAsync(serverTasks);
        }

        private async Task<object[]> WaitAsync(string[] waitingIds, Func<Task<object[]>> worker)
        {
            var waitingNodes = new List<WaitingNode>(waitingIds.Length);
            foreach (var waitingId in waitingIds)
            {
                var waitingNode = _waitingNodes.Get(waitingId);
                waitingNodes.Add(waitingNode);
            }

            var work = new SerializedWork(waitingNodes.ToArray(), worker);
            return await work.WaitAsync();
        }

        private async Task<object[]> RunServerTasksAsync(IServerTask[] serverTasks)
        {
            var tasks = new Queue<IServerTask>(serverTasks.Length * 2);
            foreach (var serverTask in serverTasks)
            {
                tasks.Enqueue(serverTask);
            }

            var ret = new List<object>(serverTasks.Length);
            try
            {
                while (tasks.Count > 0)
                {
                    var task = tasks.Dequeue();
                    _logger.LogInformation($"{task.GetType().Name} Wait...");

                    await task.AssignAsync();

                    var nextTasks = Array.Empty<ServerTask>();
                    ret.AddRange(await WaitAsync(task.WaitingIds.ToArray(), async () =>
                    {
                        return await task.RunAsync();
                    }));

                    _logger.LogInformation($"{task.GetType().Name} Complete");
                    foreach (var nextTask in task.NextTasks)
                    {
                        tasks.Enqueue(nextTask);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return ret.ToArray();
        }
    }
}