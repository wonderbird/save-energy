using SaveEnergy.Domain;

namespace SaveEnergy.Adapters.Outbound;

public interface ICanAuthenticate
{
    Task<AccessToken> RequestAccessToken();
}