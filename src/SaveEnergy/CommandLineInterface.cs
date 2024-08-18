using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SaveEnergy;

public class CommandLineInterface : IHostedService
{
    private readonly ILogger<CommandLineInterface> _logger;
    private readonly IHostApplicationLifetime _appLifetime;

    public CommandLineInterface(ILogger<CommandLineInterface> logger, IHostApplicationLifetime appLifetime)
    {
        _logger = logger;
        _appLifetime = appLifetime;
        
        _appLifetime.ApplicationStarted.Register(ProcessCommand);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Process ID {ProcessId}", Environment.ProcessId);
        
        return Task.CompletedTask;
    }

    private void ProcessCommand()
    {
        _logger.LogInformation("Hello, World!");
        _appLifetime.StopApplication();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}