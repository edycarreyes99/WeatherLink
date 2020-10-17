using System;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WeatherLink.Interfaces;
using WeatherLink.Services;

namespace WeatherLink.Middlewares
{
    public class FirebaseJwtMiddleware : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        // Variables globales a utilizarse
        private readonly AuthService _authService = new AuthService();

        // Constructor del middleware
        public FirebaseJwtMiddleware(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        // Metodo que se ejecuta cada vez que se realiza una peticion al servidor
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Se omite la validacion si el endpoint o controladdor posee el atributo [AllowAnonymous]
            var endpoint = Context.GetEndpoint();
            if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
                return AuthenticateResult.NoResult();

            // Se verifica que la peticion contenga el header de autorizacion
            if (!Request.Headers.ContainsKey("Authorization"))
                return AuthenticateResult.Fail("No se encuentra el header de 'Authorization'.");

            String jwtCorrecto;
            string jwt;

            try
            {
                var authHeader = Request.Headers["Authorization"];
                jwt = authHeader.ToString().Replace("Bearer ", "");

                // Se invoca al metodo de verificacion del jwt
                jwtCorrecto = await _authService.CheckJwt(jwt);
            }
            catch
            {
                return AuthenticateResult.Fail("El valor del 'Authorization' ingresado no es valido.");
            }

            // Se verifica que el jwt le pertenezca a un usuario
            if (jwtCorrecto == null)
                return AuthenticateResult.Fail(
                    "Los valores de authorizacion ingresados no corresponden a ningun usuario.");

            var claims = new[]
            {
                new Claim(ClaimTypes.Authentication, jwt)
            };

            var identidadClaims = new ClaimsIdentity(claims, Scheme.Name);
            var claimsPrincipal = new ClaimsPrincipal(identidadClaims);
            var authenticationTicket = new AuthenticationTicket(claimsPrincipal, Scheme.Name);

            // Se retorna la autenticacion correcta
            return AuthenticateResult.Success(authenticationTicket);
        }
    }
}