namespace WeatherLink.Interfaces
{
    public interface IAuthService
    {
        bool CheckJwt(string jwt);
    }
}