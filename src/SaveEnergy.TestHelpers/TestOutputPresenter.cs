using SaveEnergy.Domain;
using Xunit.Abstractions;

namespace SaveEnergy.TestHelpers;

public class TestOutputPresenter : ICanPresentOutput
{
    private readonly ITestOutputHelper _testOutputHelper;

    private readonly List<string> _recordedOutput = [];

    public IEnumerable<string> RecordedOutput => _recordedOutput;

    public TestOutputPresenter(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public void Present(string output)
    {
        _recordedOutput.Add(output);
        _testOutputHelper.WriteLine(output);
    }
}