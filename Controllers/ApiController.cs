using System;
using System.Diagnostics;
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
        [Route("Api")]
        [Route("Api/Index")]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
        
        [Route("Api/Error")]
        [Route("Error")]
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }

        // Metodo que se ejecuta cuando se quiere extraer todas las estaciones
        [Route("Estaciones")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
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
        [ProducesResponseType(StatusCodes.Status200OK)]
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

        // Metodo que se ejecuta cuando se quiere agregar una nueva estacion
        [HttpPost]
        [Route("AgregarEstacion")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AgregarEstacion(string nombre, double latitud, double longitud)
        {
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

            EstacionesViewModel nuevaEstacion = new EstacionesViewModel();

            nuevaEstacion.Name = nombre;
            nuevaEstacion.Latitude = latitud;
            nuevaEstacion.Longitude = longitud;

            if (_apiDbContext.Estaciones.Where(e => e.Name.Trim().ToLower().Equals(nombre.Trim().ToLower())).ToList()
                .Count > 0)
            {
                return BadRequest(Json(new
                {
                    status = StatusCode(StatusCodes.Status409Conflict).StatusCode,
                    message = "Otra estacion con el mismo nombre ya existe."
                }));
            }

            EstacionesViewModel nuevaEstacionGuardada = _apiDbContext.Estaciones.Add(nuevaEstacion).Entity;

            var response = await _apiDbContext.SaveChangesAsync();

            Console.WriteLine($"La respuesta a la peticion fue: {response}");

            if (response == 0)
            {
                return BadRequest(Json(new
                {
                    status = StatusCode(StatusCodes.Status409Conflict).StatusCode,
                    message = "No se pudo guardar la estacion correctamente."
                }));
            }

            return Ok(new
            {
                status = StatusCode(StatusCodes.Status201Created).StatusCode,
                message =
                    $"La estacion {nuevaEstacionGuardada.Name} ha sido guardada correctamente con el Id: {nuevaEstacionGuardada.Id}",
                data = nuevaEstacionGuardada
            });
        }

        [HttpPut]
        [Route("ActualizarEstacion")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ActualizarEstacion(int id, string nombre)
        {
            if (string.IsNullOrEmpty(nombre))
            {
                return Json(new
                {
                    status = StatusCode(StatusCodes.Status400BadRequest).StatusCode,
                    message = "El nombre tiene que ser valido y es un parametro requerido."
                });
            }

            if (id == 0)
            {
                return Json(new
                {
                    status = StatusCode(StatusCodes.Status400BadRequest).StatusCode,
                    message = "El Id tiene que ser valido y es un parametro requerido."
                });
            }

            if (_apiDbContext.Estaciones.Where(e => e.Id.Equals(id)).ToList().Count == 0)
            {
                return Json(new
                {
                    status = StatusCode(StatusCodes.Status400BadRequest).StatusCode,
                    message = "El id ingresado no pertenece a ninguna estacion."
                });
            }

            EstacionesViewModel estacionActualizar = _apiDbContext.Estaciones.First(e => e.Id.Equals(id));

            estacionActualizar.Name = nombre;

            _apiDbContext.Update(estacionActualizar);

            await _apiDbContext.SaveChangesAsync();

            return Ok(new
            {
                status = StatusCode(StatusCodes.Status200OK).StatusCode,
                message = $"La estacion con el id {id} se ha actualizado correctamente.",
                data = estacionActualizar
            });
        }

        [HttpDelete]
        [Route("EliminarEstacion")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult EliminarEstacion(int id)
        {
            if (id == 0)
            {
                return Json(new
                {
                    status = StatusCode(StatusCodes.Status400BadRequest).StatusCode,
                    message = "El id tiene que tener un formato valido y es requerido para ejecutar esta accion."
                });
            }

            if (_apiDbContext.Estaciones.Where(e => e.Id.Equals(id)).ToList().Count == 0)
            {
                return Json(new
                {
                    status = StatusCode(StatusCodes.Status400BadRequest).StatusCode,
                    message = "El id ingresado no pertenece a ninguna estacion."
                });
            }

            EstacionesViewModel estacionEliminar = _apiDbContext.Estaciones.First(e => e.Id.Equals(id));

            _apiDbContext.Remove(estacionEliminar);

            return Ok(new
            {
                status = StatusCode(StatusCodes.Status200OK).StatusCode,
                message = $"La estacion con el id {id} se ha eliminado correctamente.",
                data = estacionEliminar
            });
        }
    }
}