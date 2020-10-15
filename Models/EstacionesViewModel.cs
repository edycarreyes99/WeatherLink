namespace WeatherLink.Models
{
    public class EstacionesViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public bool Active { get; set; }
    }
}