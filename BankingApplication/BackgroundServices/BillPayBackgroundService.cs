using BankingApplication.Interfaces;

namespace BankingApplication.BackgroundServices;

public class BillPayBackgroundService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<BillPayBackgroundService> _logger;

    public BillPayBackgroundService(IServiceProvider services, ILogger<BillPayBackgroundService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Bill Pay Background Service is running.");

        while(!cancellationToken.IsCancellationRequested)
        {
            await DoWorkAsync(cancellationToken);

            await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
        }
    }

    private async Task DoWorkAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Bill Pay Background Service is working.");

        using var scope = _services.CreateScope();
        var billPayService = scope.ServiceProvider.GetRequiredService<IBillPayService>();

        try
        {
            await billPayService.ProcessScheduledBillPaymentsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process scheduled bill payments");
        }

        _logger.LogInformation("Bill Pay Background Service work complete.");
    }
}
