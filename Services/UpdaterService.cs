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
        private readonly ILogger<UpdaterService> _logger;
        private ApiDbContext _apiDbContext;
        private WeatherService _weatherService;

        // inject a logger
        public UpdaterService(ILogger<UpdaterService> logger, ApiDbContext apiDbContext)
        {
            _logger = logger;
            _apiDbContext = apiDbContext;
            _weatherService = new WeatherService(_apiDbContext);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Hosted service starting");
            var estacion = _apiDbContext.Estaciones.First();
            return Task.Factory.StartNew(async () =>
            {
                // loop until a cancalation is requested
                while (!cancellationToken.IsCancellationRequested)
                {
                    Console.WriteLine($"Hosted service executing - {DateTime.Now}");
                    try
                    {
                        // wait for 3 seconds
                        await _weatherService.ActualizarEstaciones();
                        await Task.Delay(TimeSpan.FromMinutes(5), cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                }
            }, cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Hosted service stopping");
            return Task.CompletedTask;
        }
    }
}