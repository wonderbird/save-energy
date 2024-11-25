using FluentAssertions;
using Meziantou.Extensions.Logging.Xunit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SaveEnergy.Adapters.Inbound;
using SaveEnergy.Domain;
using SaveEnergy.TestHelpers;
using Xunit.Abstractions;

namespace SaveEnergy.Tests.Adapters.Inbound;

public class CommandLineInterfaceTest
{
    private readonly ILogger<CommandLineInterface> _logger;
    private readonly ICanPresentOutput _outputPresenter;

    public CommandLineInterfaceTest(ITestOutputHelper testOutputHelper)
    {
        _logger = XUnitLogger.CreateLogger<CommandLineInterface>(testOutputHelper);
        _outputPresenter = new TestOutputPresenter(testOutputHelper);
    }

    [Fact]
    public void ProcessCommand_QueryThrowsException_DoesNotThrow()
    {
        IHostApplicationLifetime appLifetime = new ApplicationLifetime(
            NullLogger<ApplicationLifetime>.Instance
        );
        IRepositoriesQuery exceptionThrowingQuery = new ExceptionThrowingQuery();

        var cli = new CommandLineInterface(
            _logger,
            appLifetime,
            exceptionThrowingQuery,
            _outputPresenter
        );

        var act = () => cli.ProcessCommand();

        act.Should().NotThrow();
    }

    private class ExceptionThrowingQuery : IRepositoriesQuery
    {
        public Task<IEnumerable<Repository>> Execute()
        {
            throw new FatalErrorException();
        }
    }
}
