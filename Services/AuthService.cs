using System.Security.Principal;
using System.Threading.Tasks;
using FirebaseAdmin.Auth;
using WeatherLink.Interfaces;

namespace WeatherLink.Services
{
    public class AuthService : IAuthService
    {
        public async Task<string> CheckJwt(string jwt)
        {
            FirebaseToken decodedToken = await FirebaseAuth.DefaultInstance
                .VerifyIdTokenAsync(jwt);
            return decodedToken.Uid;
        }
    }
}