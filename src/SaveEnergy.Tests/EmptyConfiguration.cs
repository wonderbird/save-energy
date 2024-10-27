using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace SaveEnergy.Tests;

public class EmptyConfiguration : IConfiguration
{
    public IConfigurationSection GetSection(string key) => null!;

    public IEnumerable<IConfigurationSection> GetChildren() => null!;

    public IChangeToken GetReloadToken() => null!;

    public string? this[string key]
    {
        get => null;
        set { /* for this fake configuration the setter is intentionally empty */ }
    }
}
