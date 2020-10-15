using System;
using System.Globalization;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Google.Apis.Util;
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

        // Ruta principal de la aplicacion
        [Route("")]
        [Route("Home")]
        [Route("Home/Index")]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // Metodo que se ejecuta cuando se quiere extraer todas las estaciones
        [Route("Estaciones")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Estaciones()
        {
            return Ok(new
            {
                status = StatusCode(StatusCodes.Status200OK).StatusCode,
                data = await _apiDbContext.Estaciones.ToListAsync()
            });
        }


        // Metodo que se ejecuta cuando se quiere extraer todas las estaciones
        [Route("Estacion")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Estacion(int id)
        {
            if (id == 0)
            {
                return Json(new
                {
                    status = StatusCode(StatusCodes.Status400BadRequest).StatusCode,
                    message = "El Id tiene que ser valido y es un parametro requerido."
                });
            }

            return Ok(new
            {
                status = StatusCode(StatusCodes.Status200OK).StatusCode,
                data = await _apiDbContext.Estaciones.FindAsync(id)
            });
        }

        
    }
}