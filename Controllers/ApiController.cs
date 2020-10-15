using System;
using System.Net.Mime;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WeatherLink.Controllers
{
    public class ApiController : Controller
    {
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

        // Post
        [HttpPost]
        [Route("AgregarEstacion")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult AgregarEstacion(string nombre, double latitud, double longitud)
        {
            ViewBag.nombre = nombre;
            ViewBag.latitud = latitud;
            ViewBag.longitud = longitud;

            if (string.IsNullOrEmpty(nombre))
            {
                return Json(new
                {
                    status = 400,
                    message = "El nombre tiene que ser valido y es un parametro requerido."
                });
            }

            if (double.IsNaN(latitud) || latitud == 0)
            {
                return BadRequest(Json(new
                {
                    status = 400,
                    message = "La latitud es un parametro requerido y tiene que ser un formato válido."
                }));
            }

            if (double.IsNaN(longitud) || longitud == 0)
            {
                return BadRequest(Json(new
                {
                    status = 400,
                    message = "La longitud es un parametro requerido y tiene que ser un formato válido."
                }));
            }

            return View();
        }
    }
}