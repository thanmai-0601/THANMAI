using Application.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class MaturityBackgroundService : BackgroundService
{
    private readonly ILogger<MaturityBackgroundService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public MaturityBackgroundService(
        ILogger<MaturityBackgroundService> logger, 
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Maturity Background Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Maturity Background Service is doing background work.");

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var maturityService = scope.ServiceProvider.GetRequiredService<IAutomatedMaturityService>();

                int processed = await maturityService.ProcessMaturitiesAsync();
                
                if (processed > 0)
                {
                    _logger.LogInformation($"Processed {processed} matured policies.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing Maturity processing job.");
            }

            // Run once every 24 hours
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }

        _logger.LogInformation("Maturity Background Service has stopped.");
    }
}
