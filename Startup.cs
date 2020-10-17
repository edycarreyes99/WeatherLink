using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
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
using WeatherLink.InputFormatters;
using WeatherLink.Interfaces;
using WeatherLink.Middlewares;
using WeatherLink.Services;

namespace WeatherLink
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            // Se inicializa la plataforma de firebase en el proyecto
            Configuration = configuration;
            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile("firebaseConfig.json")
            });
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();

            // Se añade el contexto de la base de datos a los servicios generales de la aplicacion y se inicializa
            services.AddDbContext<ApiDbContext>(options =>
                options.UseMySql(Configuration["WeatherLink:MYSQL_CONNECTION_STRING"]));

            // Configuracion para la autenticacion basica mediante JWT 
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddScheme<AuthenticationSchemeOptions, FirebaseJwtMiddleware>(JwtBearerDefaults.AuthenticationScheme,
                    null);

            // Se configura el DI para la aplicacion de servicios
            services.AddScoped<IAuthService, AuthService>();


            // Se añade el servicio de background para el THREAD para extraer y actualizar la informacion meteoroligica de cada estacion cada 5 minutos
            services.AddSingleton<IHostedService>(new UpdaterService(new Logger<UpdaterService>(new LoggerFactory()),
                new ApiDbContext(new DbContextOptionsBuilder<ApiDbContext>()
                    .UseMySql(Configuration["WeatherLink:MYSQL_CONNECTION_STRING"]).Options)));

            // Se añade el formateador para aceptar peticiones con json
            services.AddMvc(options => { options.InputFormatters.Insert(0, new RawJsonBodyInputFormatter()); });

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
        }
    }
}