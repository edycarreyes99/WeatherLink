using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WeatherLink.DBContexts;
using WeatherLink.Models;

namespace WeatherLink.Services
{
    public class UpdaterService : IHostedService
    {
        // Variables globales a utilizarse
        private readonly ILogger<UpdaterService> _logger;
        private WeatherService _weatherService;

        // Constructor del servicio
        public UpdaterService(ILogger<UpdaterService> logger, ApiDbContext apiDbContext)
        {
            // Se inicializan las variables
            _logger = logger;
            _weatherService = new WeatherService(apiDbContext);
        }

        // Metodo que se ejecuta al invocar al servicio
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Hosted service starting");

            // Se retorna el thread que se ejecutara cada 5 minutos
            return Task.Factory.StartNew(async () =>
            {
                // loop until a cancalation is requested
                while (!cancellationToken.IsCancellationRequested)
                {
                    Console.WriteLine($"Hosted service executing - {DateTime.Now}");
                    try
                    {
                        // Se invoca al metodo que actualiza la informacion meteorologica de las estaciones en la base de datos
                        await _weatherService.ActualizarEstaciones();
                        await Task.Delay(TimeSpan.FromMinutes(5), cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                }
            }, cancellationToken);
        }

        // Metodo que detiene el thread
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Hosted service stopping");
            return Task.CompletedTask;
        }
    }
}