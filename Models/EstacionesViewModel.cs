using System;

namespace WeatherLink.Models
{
    public class EstacionesViewModel
    {
        // Propiedad para el identificador unico de la estacion
        public int Id { get; set; }

        // Propiedad para el nombre de la estacion
        public string Name { get; set; }

        // Propiedad para la latitud de la estacion
        public double Latitude { get; set; }

        // Propiedad para el longitud de la estacion
        public double Longitude { get; set; }

        public double Temperatura { get; set; }

        public double Humedad { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}