using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ServerBase.ServerTasks
{
    public interface IServerTasksService
    {
        Task RunAsync(params IServerTask[] serverTasks);
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

        public async Task RunAsync(params IServerTask[] serverTasks)
        {
            await RunServerTasksAsync(serverTasks);
        }

        private async Task WaitAsync(string[] waitingIds, Func<Task<object[]>> worker)
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

        private async Task RunServerTasksAsync(IServerTask[] serverTasks)
        {
            var tasks = new Queue<IServerTask>(serverTasks.Length * 2);
            foreach (var serverTask in serverTasks)
            {
                tasks.Enqueue(serverTask);
            }

            try
            {
                while (tasks.Count > 0)
                {
                    var task = tasks.Dequeue();
                    _logger.LogInformation($"{task.GetType().Name} Wait...");

                    var nextTasks = Array.Empty<ServerTask>();
                    await WaitAsync(task.WaitingIds, async () =>
                    {
                        return await task.RunAsync();
                    });

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
        }
    }
}