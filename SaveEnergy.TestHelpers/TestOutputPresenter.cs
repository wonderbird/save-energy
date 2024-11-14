using SaveEnergy.Domain;
using Xunit.Abstractions;

namespace SaveEnergy.TestHelpers;

public class TestOutputPresenter : ICanPresentOutput
{
    private readonly ITestOutputHelper _testOutputHelper;
    public string RecordedOutput { get; private set; } = "";

    public TestOutputPresenter(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public void Present(string output)
    {
        RecordedOutput += output + "\n";
        _testOutputHelper.WriteLine(output);
    }
}