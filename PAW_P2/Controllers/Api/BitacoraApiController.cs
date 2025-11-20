using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using PAW_P2.Models;

namespace PAW_P2.Controllers.Api
{
    // Controlador API para consulta de bitácora
    [RoutePrefix("api/bitacora")]
    public class BitacoraApiController : ApiController
    {
        private readonly SistemaAcademicoContexto _contexto;

        public BitacoraApiController()
        {
            _contexto = new SistemaAcademicoContexto();
        }

        // GET api/bitacora
        [HttpGet]
        [Route("")]
        public IHttpActionResult ObtenerTodos(int pagina = 1, int registrosPorPagina = 20)
        {
            var total = _contexto.Bitacoras.Count();

            var registros = _contexto.Bitacoras
                .Include(b => b.Usuario)
                .OrderByDescending(b => b.FechaHora)
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .Select(b => new
                {
                    b.BitacoraId,
                    b.Accion,
                    b.Modulo,
                    b.Descripcion,
                    b.FechaHora,
                    b.DireccionIP,
                    b.EntidadAfectada,
                    b.EntidadId,
                    NombreUsuario = b.Usuario != null ? b.Usuario.NombreCompleto : "Sistema"
                })
                .ToList();

            return Ok(new
            {
                registros = registros,
                paginaActual = pagina,
                totalPaginas = (int)Math.Ceiling((double)total / registrosPorPagina),
                totalRegistros = total
            });
        }

        // GET api/bitacora/buscar
        [HttpGet]
        [Route("buscar")]
        public IHttpActionResult Buscar(string usuario = null, string accion = null,
            string modulo = null, DateTime? fechaInicio = null, DateTime? fechaFin = null,
            int pagina = 1, int registrosPorPagina = 20)
        {
            var query = _contexto.Bitacoras.Include(b => b.Usuario).AsQueryable();

            // Aplicar filtros
            if (!string.IsNullOrEmpty(usuario))
            {
                query = query.Where(b => b.Usuario.NombreCompleto.Contains(usuario) ||
                                         b.Usuario.Correo.Contains(usuario));
            }

            if (!string.IsNullOrEmpty(accion))
            {
                query = query.Where(b => b.Accion.Contains(accion));
            }

            if (!string.IsNullOrEmpty(modulo))
            {
                query = query.Where(b => b.Modulo == modulo);
            }

            if (fechaInicio.HasValue)
            {
                query = query.Where(b => b.FechaHora >= fechaInicio.Value);
            }

            if (fechaFin.HasValue)
            {
                var fechaFinDia = fechaFin.Value.AddDays(1);
                query = query.Where(b => b.FechaHora < fechaFinDia);
            }

            var total = query.Count();

            var registros = query
                .OrderByDescending(b => b.FechaHora)
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .Select(b => new
                {
                    b.BitacoraId,
                    b.Accion,
                    b.Modulo,
                    b.Descripcion,
                    b.FechaHora,
                    b.DireccionIP,
                    b.EntidadAfectada,
                    b.EntidadId,
                    NombreUsuario = b.Usuario != null ? b.Usuario.NombreCompleto : "Sistema"
                })
                .ToList();

            return Ok(new
            {
                registros = registros,
                paginaActual = pagina,
                totalPaginas = (int)Math.Ceiling((double)total / registrosPorPagina),
                totalRegistros = total
            });
        }

        // GET api/bitacora/5
        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult ObtenerPorId(int id)
        {
            var registro = _contexto.Bitacoras
                .Include(b => b.Usuario)
                .Where(b => b.BitacoraId == id)
                .Select(b => new
                {
                    b.BitacoraId,
                    b.Accion,
                    b.Modulo,
                    b.Descripcion,
                    b.FechaHora,
                    b.DireccionIP,
                    b.Navegador,
                    b.EntidadAfectada,
                    b.EntidadId,
                    b.DatosAnteriores,
                    b.DatosNuevos,
                    NombreUsuario = b.Usuario != null ? b.Usuario.NombreCompleto : "Sistema",
                    CorreoUsuario = b.Usuario != null ? b.Usuario.Correo : ""
                })
                .FirstOrDefault();

            if (registro == null)
                return NotFound();

            return Ok(registro);
        }

        // GET api/bitacora/modulos
        [HttpGet]
        [Route("modulos")]
        public IHttpActionResult ObtenerModulos()
        {
            var modulos = _contexto.Bitacoras
                .Select(b => b.Modulo)
                .Distinct()
                .OrderBy(m => m)
                .ToList();

            return Ok(modulos);
        }

        // GET api/bitacora/acciones
        [HttpGet]
        [Route("acciones")]
        public IHttpActionResult ObtenerAcciones()
        {
            var acciones = _contexto.Bitacoras
                .Select(b => b.Accion)
                .Distinct()
                .OrderBy(a => a)
                .ToList();

            return Ok(acciones);
        }

        // GET api/bitacora/estadisticas
        [HttpGet]
        [Route("estadisticas")]
        public IHttpActionResult ObtenerEstadisticas()
        {
            var hoy = DateTime.Today;
            var inicioSemana = hoy.AddDays(-(int)hoy.DayOfWeek);
            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);

            var estadisticas = new
            {
                totalRegistros = _contexto.Bitacoras.Count(),
                registrosHoy = _contexto.Bitacoras.Count(b => DbFunctions.TruncateTime(b.FechaHora) == hoy),
                registrosSemana = _contexto.Bitacoras.Count(b => b.FechaHora >= inicioSemana),
                registrosMes = _contexto.Bitacoras.Count(b => b.FechaHora >= inicioMes),
                porModulo = _contexto.Bitacoras
                    .GroupBy(b => b.Modulo)
                    .Select(g => new { Modulo = g.Key, Cantidad = g.Count() })
                    .ToList(),
                porAccion = _contexto.Bitacoras
                    .GroupBy(b => b.Accion)
                    .Select(g => new { Accion = g.Key, Cantidad = g.Count() })
                    .ToList()
            };

            return Ok(estadisticas);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _contexto?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
