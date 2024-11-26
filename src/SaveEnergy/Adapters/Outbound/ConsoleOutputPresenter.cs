using SaveEnergy.Domain;

namespace SaveEnergy.Adapters.Outbound;

public class ConsoleOutputPresenter : ICanPresentOutput
{
    public void Present(string? output)
    {
        Console.WriteLine(output);
    }
}
