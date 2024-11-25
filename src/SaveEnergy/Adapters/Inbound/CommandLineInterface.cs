using System.Globalization;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SaveEnergy.Domain;

namespace SaveEnergy.Adapters.Inbound;

public class CommandLineInterface : IHostedService
{
    private readonly ILogger<CommandLineInterface> _logger;
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly IRepositoriesQuery _repositoriesQuery;
    private readonly ICanPresentOutput _outputPresenter;

    public CommandLineInterface(
        ILogger<CommandLineInterface> logger,
        IHostApplicationLifetime appLifetime,
        IRepositoriesQuery repositoriesQuery,
        ICanPresentOutput outputPresenter
    )
    {
        _logger = logger;
        _appLifetime = appLifetime;
        _repositoriesQuery = repositoriesQuery;
        _outputPresenter = outputPresenter;

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

            repositories = repositories.OrderBy(r => r.PushedAt);

            PresentRepositories(repositories);
        }
        catch (FatalErrorException e)
        {
            _outputPresenter.Present(e.Message);
        }
        finally
        {
            _appLifetime.StopApplication();
        }
    }

    private void PresentRepositories(IEnumerable<Repository> repositories)
    {
        _outputPresenter.Present(
            "| Repository name | Last Change | Description | HTML URL | SSH URL | Clone URL |"
        );
        _outputPresenter.Present("| --- | --- | --- | --- | --- | --- |");
        foreach (var repository in repositories)
        {
            _outputPresenter.Present(
                $"| {repository.Name} "
                    + $"| {repository.PushedAt.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture)} "
                    + $"| {repository.Description} "
                    + $"| {repository.HtmlUrl} "
                    + $"| {repository.SshUrl} "
                    + $"| {repository.CloneUrl} |"
            );
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
