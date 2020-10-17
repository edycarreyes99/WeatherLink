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
            // Metodo que verifica si el token proporcionado por la peticion es valido o pertenece a algun usuario en la plataforma
            FirebaseToken decodedToken = await FirebaseAuth.DefaultInstance
                .VerifyIdTokenAsync(jwt);
            return decodedToken.Uid;
        }
    }
}