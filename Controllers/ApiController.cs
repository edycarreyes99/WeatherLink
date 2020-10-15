using System;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WeatherLink.Models;

namespace WeatherLink.Controllers
{
    public class ApiController : Controller
    {
        private readonly ApiDbContext _apiDbContext;

        public ApiController(ApiDbContext apiDbContext)
        {
            _apiDbContext = apiDbContext;
        }

        // GET
        public IActionResult Index()
        {
            return View();
        }

        // Get
        [HttpGet]
        [Route("Estaciones")]
        public IActionResult Estaciones()
        {
            return View();
        }

        // Metodo que se ejecuta cuando se quiere agregar una nueva estacion
        [HttpPost]
        [Route("AgregarEstacion")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AgregarEstacion(string nombre, double latitud, double longitud)
        {
            ViewBag.nombre = nombre;
            ViewBag.latitud = latitud;
            ViewBag.longitud = longitud;

            if (string.IsNullOrEmpty(nombre))
            {
                return Json(new
                {
                    status = StatusCode(StatusCodes.Status400BadRequest).StatusCode,
                    message = "El nombre tiene que ser valido y es un parametro requerido."
                });
            }

            if (double.IsNaN(latitud) || latitud == 0)
            {
                return BadRequest(Json(new
                {
                    status = StatusCode(StatusCodes.Status400BadRequest).StatusCode,
                    message = "La latitud es un parametro requerido y tiene que ser un formato válido."
                }));
            }

            if (double.IsNaN(longitud) || longitud == 0)
            {
                return BadRequest(Json(new
                {
                    status = StatusCode(StatusCodes.Status400BadRequest).StatusCode,
                    message = "La longitud es un parametro requerido y tiene que ser un formato válido."
                }));
            }

            return Ok(new
            {
                status = StatusCode(StatusCodes.Status200OK).StatusCode,
                data = await _apiDbContext.Estaciones.ToListAsync()
            });
        }
    }
}