using System.Threading.Tasks;

namespace WeatherLink.Interfaces
{
    public interface IAuthService
    {
        Task<string> CheckJwt(string jwt);
    }
}