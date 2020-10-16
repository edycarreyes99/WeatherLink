using System;
using System.Threading.Tasks;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WeatherLink.DBContexts;
using WeatherLink.Interfaces;
using WeatherLink.Middlewares;
using WeatherLink.Services;

namespace WeatherLink
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile("firebaseConfig.json")
            });
        }

        // Metodo para crear el Thread de que actualice el contenido de las estaciones cada 5 minutos
        private void HiloParaEstaciones()
        {
            var startTimeSpan = TimeSpan.Zero;
            var periodTimeSpan = TimeSpan.FromSeconds(5);

            var timer = new System.Threading.Timer(async (e) =>
            {
                /*var httpClient =
                    new HttpClientFactory().CreateHttpClient(new CreateHttpClientArgs());

                var url =
                    $"https://localhost:5001/ActualizarEstaciones/";

                var response = await httpClient.GetAsync(url);

                Console.WriteLine(response.Content.ReadAsStringAsync().Result);*/

                Console.WriteLine("Hilo comenzado");
            }, null, startTimeSpan, periodTimeSpan);
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
            // Se añade el contexto de la base de datos a los servicios generales de la aplicacion y se inicializa
            services.AddDbContext<ApiDbContext>(options =>
                options.UseMySql(Configuration["WeatherLink:MYSQL_CONNECTION_STRING"]));

            // configure basic authentication 
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddScheme<AuthenticationSchemeOptions, FirebaseJwtMiddleware>(JwtBearerDefaults.AuthenticationScheme,
                    null);

            // se configura el DI para la aplicacion de servicios
            services.AddScoped<IAuthService, AuthService>();


            services.AddSingleton<IHostedService>(new UpdaterService(new Logger<UpdaterService>(new LoggerFactory()),
                new ApiDbContext(new DbContextOptionsBuilder<ApiDbContext>()
                    .UseMySql(Configuration["WeatherLink:MYSQL_CONNECTION_STRING"]).Options)));

            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Api/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // politica de cors global
            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                // Ruta para extraer una sola estacion
                endpoints.MapControllers();
            });

            // HiloParaEstaciones();
        }
    }
}