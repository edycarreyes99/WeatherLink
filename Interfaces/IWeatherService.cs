using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Newtonsoft.Json.Linq;

namespace WeatherLink.Interfaces
{
    public interface IWeatherService
    {
        Task<object> ClimaPorEstacion(int id);

        Task<object> GenerarDatosParaGraficos();
        
        Task<object> ClimaPorLngLat(double lat, double lng);

        Task ActualizarEstaciones();
    }
}