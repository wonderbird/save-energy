using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SaveEnergy.Adapters.Outbound;
using SaveEnergy.Domain;

namespace SaveEnergy.Adapters.Inbound;

public class CommandLineInterface : IHostedService
{
    private readonly ILogger<CommandLineInterface> _logger;
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly IRepositoriesQuery _repositoriesQuery;

    public CommandLineInterface(ILogger<CommandLineInterface> logger, IHostApplicationLifetime appLifetime,
        IRepositoriesQuery repositoriesQuery)
    {
        _logger = logger;
        _appLifetime = appLifetime;
        _repositoriesQuery = repositoriesQuery;

        _appLifetime.ApplicationStarted.Register(ProcessCommand);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Process ID {ProcessId}", Environment.ProcessId);

        return Task.CompletedTask;
    }

    internal async void ProcessCommand()
    {
        try
        {
            var repositories = await _repositoriesQuery.Execute();

            PresentRepositories(repositories);
        }
        catch (FatalErrorException e)
        {
            Console.WriteLine(e.Message);
        }
        finally
        {
            _appLifetime.StopApplication();
        }
    }

    private static void PresentRepositories(IEnumerable<Repository> repositories)
    {
        Console.WriteLine($"| Repository name | URL |");
        Console.WriteLine($"| --- | --- |");
        foreach (var repository in repositories)
        {
            Console.WriteLine($"| {repository.Name} | {repository.HtmlUrl} |");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}