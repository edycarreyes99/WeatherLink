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

        private readonly string[] _meses =
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

        // Metodo que se ejecuta en la paticion para generar los datos del graficos de temperatura y humedad
        public async Task<object> GenerarDatosParaGraficos()
        {
            // Se extraen todas las estaciones de la base de datos y se guardan en una variable
            var estaciones = await _apiDbContext.Estaciones.ToListAsync();

            // Se determina si existen estaciones en la base de datos
            if (estaciones.Count != 0)
            {
                // Variables a utilizarse
                var seriesTemperatura = new List<Dictionary<string, object>>();
                var seriesHumedad = new List<Dictionary<string, object>>();
                var categories = new List<string>();
                var i = 0;

                // Se recorre la lista de estaciones
                foreach (var estacion in estaciones)
                {
                    // Variable para almacenar la respuesta desde la api de OPEN WEATHER
                    var listaClimaPorEstacion = new List<JToken>();

                    // Url de la api de clima
                    var url =
                        $"http://api.openweathermap.org/data/2.5/forecast?lat={estacion.Latitude}&lon={estacion.Longitude}&appid=7e0e4e6cdce20a9faf2b59da4e37dcc2&units=metric";

                    // Se realiza la peticion a la api
                    var response = await _httpClient.GetAsync(url);

                    // Se parsea el contenido de la respuesta a un archivo json
                    var jsonResponse = JObject.Parse(response.Content.ReadAsStringAsync().Result);

                    // Se almacena la respuesta de la api en la variable local
                    listaClimaPorEstacion = jsonResponse["list"].ToList();
                    var promediosTemperaturaPorDia = new Dictionary<string, double>();
                    var dataSeriesTemperatura = new List<double>();
                    var promediosHumedadesPorDia = new Dictionary<string, double>();
                    var dataSeriesHumedad = new List<double>();
                    var contadorHorasPorDia = new Dictionary<string, int>();

                    // Se recorre cada valor retornado por la api
                    foreach (var dateClimaPorHora in listaClimaPorEstacion)
                    {
                        // Se establece una variable para la cultura español del parsing de la varibale date
                        var ci = new CultureInfo("Es-Es");
                        var climaPorHora =
                            DateTime.ParseExact(dateClimaPorHora["dt_txt"].ToString(), "yyyy-MM-dd HH:mm:ss",
                                ci);
                        double temperatura;
                        double humedad;

                        // Se parsea el valor de la temperatura a la variable local
                        double.TryParse(dateClimaPorHora["main"]["temp"].ToString(), out temperatura);
                        double.TryParse(dateClimaPorHora["main"]["humidity"].ToString(), out humedad);

                        // Se obtiene el nombre del dia de la semana en español
                        var dayOfWeek = ci.DateTimeFormat.GetDayName(climaPorHora.DayOfWeek);

                        // Se verifica si el dia ya etaba almacenado en el diccionaro para inicializar el contador a 0
                        if (!promediosTemperaturaPorDia.ContainsKey(
                            $"{dayOfWeek} {climaPorHora.Day} {_meses[climaPorHora.Month].Substring(0, 3)}")
                        )
                        {
                            promediosTemperaturaPorDia[
                                $"{dayOfWeek} {climaPorHora.Day} {_meses[climaPorHora.Month].Substring(0, 3)}"
                            ] = 0;
                            promediosHumedadesPorDia[
                                $"{dayOfWeek} {climaPorHora.Day} {_meses[climaPorHora.Month].Substring(0, 3)}"
                            ] = 0;
                            contadorHorasPorDia[
                                $"{dayOfWeek} {climaPorHora.Day} {_meses[climaPorHora.Month].Substring(0, 3)}"
                            ] = 0;
                        }

                        // Se suma el valor actual de los promedio con el nuevo valor de la temperatura
                        promediosTemperaturaPorDia[
                            $"{dayOfWeek} {climaPorHora.Day} {_meses[climaPorHora.Month].Substring(0, 3)}"
                        ] += temperatura;

                        if (contadorHorasPorDia[
                            $"{dayOfWeek} {climaPorHora.Day} {_meses[climaPorHora.Month].Substring(0, 3)}"
                        ] < 3)
                        {
                            promediosHumedadesPorDia[
                                $"{dayOfWeek} {climaPorHora.Day} {_meses[climaPorHora.Month].Substring(0, 3)}"
                            ] += humedad;
                        }

                        // Se incrementa el contador de las horas por dia para cada estacion
                        contadorHorasPorDia[
                            $"{dayOfWeek} {climaPorHora.Day} {_meses[climaPorHora.Month].Substring(0, 3)}"
                        ] += 1;
                    }

                    // Console.WriteLine($"Estacion {estacion.Name}:");
                    promediosTemperaturaPorDia.Remove(promediosTemperaturaPorDia.Keys.Last());
                    // Console.WriteLine("Temperaturas:");
                    // Se recorre todos los promedios almacenados hasta el momento por cada estacion
                    foreach (var key in promediosTemperaturaPorDia.Keys.ToList())
                    {
                        // Se divide entre la cantidad de horas por dia para determinar el promedio de temperatura por dia de cada estacion
                        promediosTemperaturaPorDia[key] /= contadorHorasPorDia[key];

                        // Se redondea a unicamente 2 decimales
                        promediosTemperaturaPorDia[key] = Math.Round(promediosTemperaturaPorDia[key], 2);

                        if (i == 0)
                        {
                            categories.Add(key);
                        }

                        // Se añade el promedio del dia a la variable que contendra todos los datos de las series para el grafico
                        dataSeriesTemperatura.Add(promediosTemperaturaPorDia[key]);

                        // Console.WriteLine($"{key}: {promediosTemperaturaPorDia[key]}");
                    }

                    promediosHumedadesPorDia.Remove(promediosHumedadesPorDia.Keys.Last());
                    // Console.WriteLine("Humedades:");
                    foreach (var key in promediosHumedadesPorDia.Keys.ToList())
                    {
                        promediosHumedadesPorDia[key] /= 3;
                        promediosHumedadesPorDia[key] = Math.Round(promediosHumedadesPorDia[key], 2);
                        dataSeriesHumedad.Add(promediosHumedadesPorDia[key]);
                        // Console.WriteLine($"{key}: {promediosHumedadesPorDia[key]}");
                    }

                    // Se crea un nuevo diccionaro para retornar los valores de las series por cada estacion
                    var nuevaSerieTemperatura = new Dictionary<string, object>();
                    nuevaSerieTemperatura["name"] = estacion.Name;
                    nuevaSerieTemperatura["data"] = dataSeriesTemperatura;
                    seriesTemperatura.Add(nuevaSerieTemperatura);

                    var nuevaSerieHumedad = new Dictionary<string, object>();
                    nuevaSerieHumedad["name"] = estacion.Name;
                    nuevaSerieHumedad["data"] = dataSeriesHumedad;
                    seriesHumedad.Add(nuevaSerieHumedad);

                    i++;
                }

                // Se retornan los valores de las series y las cateogorias para el grafico
                return new
                {
                    categories,
                    seriesHumedad,
                    seriesTemperatura
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