using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace ServerBase.Services
{
    public class Initializer : IHostedService
    {
        private readonly ILogger<Initializer> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public Initializer(ILogger<Initializer> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                using var context = scope.ServiceProvider.GetRequiredService<ServerDbContext>();
                var database = context.Database;

                await database.MigrateAsync(cancellationToken);
            }

            _logger.LogInformation("Start");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            _logger.LogInformation("Stop");
        }
    }
}