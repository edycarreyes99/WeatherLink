using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Google.Apis.Http;
using Microsoft.EntityFrameworkCore;
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
        // Variables globales a utilizarse
        private readonly ConfigurableHttpClient _httpClient;
        private readonly ApiDbContext _apiDbContext;

        // Constructor del servicio
        public WeatherService(ApiDbContext apiDbContext)
        {
            // Se inicializan las variables globales
            _httpClient = new HttpClientFactory().CreateHttpClient(new CreateHttpClientArgs());
            _apiDbContext = apiDbContext;
        }

        // Metodo que se ejecuta para extraer el clima de una estacion mediante su id
        public async Task<object> ClimaPorEstacion(int id)
        {
            // Se crea una variable que almacena el contenido de la estacion a partir del id proporcionado
            var estacion = _apiDbContext.Estaciones.First(e => e.Id.Equals(id));

            // Url de la api de clima
            var url =
                $"http://api.openweathermap.org/data/2.5/weather?lat={estacion.Latitude}&lon={estacion.Longitude}&appid=7e0e4e6cdce20a9faf2b59da4e37dcc2&units=metric";

            // Se realiza la peticion a la api
            var response = await _httpClient.GetAsync(url);

            // Se parsea el contenido de la respuesta a un archivo json
            var jsonResponse = JObject.Parse(response.Content.ReadAsStringAsync().Result);


            // Se retorna la informacion obtenida de la peticion
            return new
            {
                id,
                temperatura = jsonResponse["main"]["temp"],
                humedad = jsonResponse["main"]["humidity"]
            };
        }

        // Metodo que se ejecuta para extraer el clima de una estacion mediante su longitud y latitud
        public async Task<object> ClimaPorLngLat(double lat, double lng)
        {
            // Url de la api de clima
            var url =
                $"http://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lng}&appid=7e0e4e6cdce20a9faf2b59da4e37dcc2&units=metric";

            // Se realiza la peticion a la api
            var response = await _httpClient.GetAsync(url);

            // Se parsea el contenido de la respuesta a un archivo json
            var jsonResponse = JObject.Parse(response.Content.ReadAsStringAsync().Result);


            // Se retorna la informacion obtenida de la peticion
            return new
            {
                temperatura = jsonResponse["main"]["temp"],
                humedad = jsonResponse["main"]["humidity"]
            };
        }

        // ToDo Terminar la implementacion del metodo
        // Metodo que se ejecuta en la paticion para generar los datos del graficos de temperatura
        public async Task<object> GenerarDatosParaGraficoDeTemperatura()
        {
            // Se extraen todas las estaciones de la base de datos y se guardan en una variable
            var estaciones = await _apiDbContext.Estaciones.ToListAsync();

            // Se determina si existen estaciones en la base de datos
            if (estaciones.Count != 0)
            {
                var series = new List<Dictionary<string, object>>();
                var categories = new List<string>();
                string[] diasSemana =
                {
                    "Lunes",
                    "Martes",
                    "Miércoles",
                    "Jueves",
                    "Viernes",
                    "Sábado",
                    "Domingo"
                };

                string[] meses =
                {
                    "Enero",
                    "Febrero",
                    "Marzo",
                    "Abril",
                    "Mayo",
                    "Junio",
                    "Julio",
                    "Agosto",
                    "Septiembre",
                    "Octubre",
                    "Noviembre",
                    "Diciembre"
                };
                var i = 0;
                // Se recorre la lista de estaciones
                foreach (var estacion in estaciones)
                {
                    var listaClimaPorEstacion = new List<JToken>();

                    // Url de la api de clima
                    var url =
                        $"http://api.openweathermap.org/data/2.5/forecast?lat={estacion.Latitude}&lon={estacion.Longitude}&appid=7e0e4e6cdce20a9faf2b59da4e37dcc2&units=metric";

                    // Se realiza la peticion a la api
                    var response = await _httpClient.GetAsync(url);

                    // Se parsea el contenido de la respuesta a un archivo json
                    var jsonResponse = JObject.Parse(response.Content.ReadAsStringAsync().Result);

                    listaClimaPorEstacion = jsonResponse["list"].ToList();

                    var promediosTemperaturaPorDia = new Dictionary<string, double>();

                    var contadorHorasPorDia = new Dictionary<string, int>();

                    var dataSeries = new List<double>();
                    foreach (var dateClimaPorHora in listaClimaPorEstacion)
                    {
                        CultureInfo ci = new CultureInfo("Es-Es");
                        var climaPorHora =
                            DateTime.ParseExact(dateClimaPorHora["dt_txt"].ToString(), "yyyy-MM-dd HH:mm:ss",
                                ci);

                        double temperatura;

                        double.TryParse(dateClimaPorHora["main"]["temp"].ToString(), out temperatura);

                        var dayOfWeek = ci.DateTimeFormat.GetDayName(climaPorHora.DayOfWeek);

                        if (!promediosTemperaturaPorDia.ContainsKey(
                            $"{dayOfWeek} {climaPorHora.Day} {meses[climaPorHora.Month].Substring(0, 3)}")
                        )
                        {
                            promediosTemperaturaPorDia[
                                $"{dayOfWeek} {climaPorHora.Day} {meses[climaPorHora.Month].Substring(0, 3)}"
                            ] = 0;
                            contadorHorasPorDia[
                                $"{dayOfWeek} {climaPorHora.Day} {meses[climaPorHora.Month].Substring(0, 3)}"
                            ] = 0;
                        }

                        promediosTemperaturaPorDia[
                            $"{dayOfWeek} {climaPorHora.Day} {meses[climaPorHora.Month].Substring(0, 3)}"
                        ] += temperatura;

                        contadorHorasPorDia[
                            $"{dayOfWeek} {climaPorHora.Day} {meses[climaPorHora.Month].Substring(0, 3)}"
                        ] += 1;
                    }

                    Console.WriteLine($"Estacion {estacion.Name}:");
                    promediosTemperaturaPorDia.Remove(promediosTemperaturaPorDia.Keys.Last());
                    foreach (var key in promediosTemperaturaPorDia.Keys.ToList())
                    {
                        promediosTemperaturaPorDia[key] /= contadorHorasPorDia[key];
                        promediosTemperaturaPorDia[key] = Math.Round(promediosTemperaturaPorDia[key], 2);
                        if (i == 0)
                        {
                            categories.Add(key);
                        }

                        dataSeries.Add(promediosTemperaturaPorDia[key]);

                        Console.WriteLine($"{key}: {promediosTemperaturaPorDia[key]}");
                    }

                    var nuevaSerie = new Dictionary<string, object>();
                    nuevaSerie["name"] = estacion.Name;
                    nuevaSerie["data"] = dataSeries;
                    series.Add(nuevaSerie);

                    i++;
                }

                return new
                {
                    categories,
                    series
                };
            }

            return new { };
        }

        // Metodo que se ejecuta para actualizar los datos del clima para cada estacion
        public async Task ActualizarEstaciones()
        {
            // Se extraen todas las estaciones y se guardan en una variable local
            var estaciones = await _apiDbContext.Estaciones.ToListAsync();

            // Se determina si existen estaciones en la base de datos
            if (estaciones.Count != 0)
            {
                // Se recorre la lita de estaciones
                foreach (var estacion in estaciones)
                {
                    // se invoca al metodo para obtener la informacion del clima de daca estacion
                    var clima = JObject.FromObject(await ClimaPorEstacion(estacion.Id));

                    // Se actualizan los datos de las estaciones
                    estacion.Humedad = (double) clima["humedad"];
                    estacion.Temperatura = (double) clima["temperatura"];
                    estacion.UpdatedAt = DateTime.Now;
                }

                // Se guardan en la base de datos
                await _apiDbContext.SaveChangesAsync();
            }
        }
    }
}