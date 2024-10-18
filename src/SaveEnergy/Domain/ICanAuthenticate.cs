namespace SaveEnergy.Domain;

public interface ICanAuthenticate
{
    Task<AccessToken> RequestAccessToken();
}