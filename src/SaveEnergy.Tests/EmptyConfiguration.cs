using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace SaveEnergy.Tests;

public class EmptyConfiguration : IConfiguration
{
    public IConfigurationSection GetSection(string key)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IConfigurationSection> GetChildren()
    {
        throw new NotImplementedException();
    }

    public IChangeToken GetReloadToken()
    {
        throw new NotImplementedException();
    }

    public string? this[string key]
    {
        get => null;
        set => throw new NotImplementedException();
    }
}