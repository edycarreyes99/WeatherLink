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
        private readonly AuthService _authService = new AuthService();

        public FirebaseJwtMiddleware(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // skip authentication if endpoint has [AllowAnonymous] attribute
            var endpoint = Context.GetEndpoint();
            if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
                return AuthenticateResult.NoResult();

            if (!Request.Headers.ContainsKey("Authorization"))
                return AuthenticateResult.Fail("No se encuentra el header de 'Authorization'.");

            String jwtCorrecto;
            string jwt;
            try
            {
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
                jwt = Encoding.UTF8.GetString(credentialBytes).Replace("Bearer ", "");
                jwtCorrecto = await _authService.CheckJwt(jwt);
            }
            catch
            {
                return AuthenticateResult.Fail("El valor del 'Authorization' ingresado no es valido.");
            }

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

            return AuthenticateResult.Success(authenticationTicket);
        }
    }
}