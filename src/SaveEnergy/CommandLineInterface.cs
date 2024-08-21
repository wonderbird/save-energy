using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SaveEnergy;

public class CommandLineInterface : IHostedService
{
    private readonly ILogger<CommandLineInterface> _logger;
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly IConfiguration _configuration;

    public CommandLineInterface(ILogger<CommandLineInterface> logger, IConfiguration configuration,
        IHostApplicationLifetime appLifetime)
    {
        _logger = logger;
        _configuration = configuration;
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
        _logger.LogInformation("Client ID: {ClientId}", _configuration["GitHub:ClientId"]);
        _appLifetime.StopApplication();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}