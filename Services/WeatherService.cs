using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Google.Apis.Http;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Newtonsoft.Json.Linq;
using WeatherLink.DBContexts;
using WeatherLink.Interfaces;
using WeatherLink.Models;
using IHttpClientFactory = Google.Apis.Http.IHttpClientFactory;

namespace WeatherLink.Services
{
    public class WeatherService : IWeatherService
    {
        private readonly ConfigurableHttpClient _httpClient;
        private readonly ApiDbContext _apiDbContext;

        public WeatherService(ApiDbContext apiDbContext)
        {
            _httpClient = new HttpClientFactory().CreateHttpClient(new CreateHttpClientArgs());
            _apiDbContext = apiDbContext;
        }

        public async Task<object> ClimaPorEstacion(int id)
        {
            var estacion = _apiDbContext.Estaciones.First(e => e.Id.Equals(id));
            var url =
                $"http://api.openweathermap.org/data/2.5/weather?lat={estacion.Latitude}&lon={estacion.Longitude}&appid=7e0e4e6cdce20a9faf2b59da4e37dcc2&units=metric";

            var response = await _httpClient.GetAsync(url);

            var jsonResponse = JObject.Parse(response.Content.ReadAsStringAsync().Result);


            return new
            {
                id,
                temperatura = jsonResponse["main"]["temp"],
                humedad = jsonResponse["main"]["humidity"]
            };
        }

        public async Task<object> ClimaPorLngLat(double lat, double lng)
        {
            var url =
                $"http://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lng}&appid=7e0e4e6cdce20a9faf2b59da4e37dcc2&units=metric";

            var response = await _httpClient.GetAsync(url);

            var jsonResponse = JObject.Parse(response.Content.ReadAsStringAsync().Result);


            return new
            {
                temperatura = jsonResponse["main"]["temp"],
                humedad = jsonResponse["main"]["humidity"]
            };
        }

        public async Task<object> DatosDeGraficosGeneralDeTemperatura()
        {
            var estaciones = _apiDbContext.Estaciones.ToList();

            var jsonResponse = new JObject();

            estaciones.ForEach(async estacion =>
            {
                var url =
                    $"http://api.openweathermap.org/data/2.5/forecast?lat={estacion.Latitude}&lon={estacion.Longitude}&appid=7e0e4e6cdce20a9faf2b59da4e37dcc2&units=metric";

                var response = await _httpClient.GetAsync(url);

                var estacionResponse = JObject.Parse(response.Content.ReadAsStringAsync().Result);

                var fechasGenerales = new List<DateTime>();

                foreach (var clima in estacionResponse["list"])
                {
                    var date = DateTime.ParseExact(clima["dt_txt"].ToString(), "yyyy-MM-dd HH:mm:ss",
                        System.Globalization.CultureInfo.InvariantCulture).Date;
                    fechasGenerales.Add(date);
                }

                List<List<DateTime>> fechasAgrupadasPorDia = fechasGenerales
                    .GroupBy(x => x.Date.Day)
                    .Select(g => g.ToList())
                    .ToList();
                Console.WriteLine($"Dias para {estacion.Name}");
                fechasAgrupadasPorDia.ForEach(fecha =>
                {
                    fecha.ForEach(fechaInterna => { Console.WriteLine(fechaInterna); });
                });
            });

            return new
            {
                data = jsonResponse
            };
        }
        
        public async Task ActualizarEstaciones()
        {
            var estaciones = _apiDbContext.Estaciones;
            if (estaciones.ToList().Count != 0)
            {
                estaciones.ToList().ForEach(async estacion =>
                {
                    var estacionActualizar = estacion;
                    var clima = JObject.FromObject(await ClimaPorEstacion(estacion.Id));

                    estacion.Humedad = (double) clima["humedad"];

                    estacion.Temperatura = (double) clima["temperatura"];

                    _apiDbContext.Update(estacionActualizar);
                });
            }
            
            await _apiDbContext.SaveChangesAsync();
        }
    }
}