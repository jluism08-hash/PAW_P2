using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using PAW_P2.Models;

namespace PAW_P2.Controllers.Api
{
    // Controlador API para historial académico
    [RoutePrefix("api/historial")]
    public class HistorialApiController : ApiController
    {
        private readonly SistemaAcademicoContexto _contexto;

        public HistorialApiController()
        {
            _contexto = new SistemaAcademicoContexto();
        }

        // GET api/historial/estudiante/5
        [HttpGet]
        [Route("estudiante/{estudianteId:int}")]
        public IHttpActionResult ObtenerHistorialEstudiante(int estudianteId)
        {
            var estudiante = _contexto.Usuarios
                .Where(u => u.UsuarioId == estudianteId)
                .Select(u => new
                {
                    u.UsuarioId,
                    u.NombreCompleto,
                    u.Identificacion,
                    u.Correo
                })
                .FirstOrDefault();

            if (estudiante == null)
                return NotFound();

            var historial = _contexto.Inscripciones
                .Include(i => i.Curso)
                .Where(i => i.EstudianteId == estudianteId)
                .OrderByDescending(i => i.Curso.Cuatrimestre)
                .Select(i => new
                {
                    i.InscripcionId,
                    i.Curso.Codigo,
                    i.Curso.Nombre,
                    i.Curso.Creditos,
                    i.Curso.Cuatrimestre,
                    i.NotaFinal,
                    i.Estado,
                    i.Aprobado,
                    i.FechaInscripcion,
                    i.FechaFinalizacion
                })
                .ToList();

            return Ok(new
            {
                estudiante = estudiante,
                historial = historial
            });
        }

        // GET api/historial/buscar?termino=Juan
        [HttpGet]
        [Route("buscar")]
        public IHttpActionResult BuscarEstudiantes(string termino)
        {
            if (string.IsNullOrEmpty(termino))
                return BadRequest("Debe proporcionar un término de búsqueda");

            var estudiantes = _contexto.Usuarios
                .Include(u => u.Rol)
                .Where(u => u.Activo &&
                            u.Rol.Nombre == "Estudiante" &&
                            (u.NombreCompleto.Contains(termino) ||
                             u.Identificacion.Contains(termino) ||
                             u.Correo.Contains(termino)))
                .Select(u => new
                {
                    u.UsuarioId,
                    u.NombreCompleto,
                    u.Identificacion,
                    u.Correo
                })
                .Take(20)
                .ToList();

            return Ok(estudiantes);
        }

        // GET api/historial/estudiante/5/calificaciones
        [HttpGet]
        [Route("estudiante/{estudianteId:int}/calificaciones")]
        public IHttpActionResult ObtenerCalificacionesEstudiante(int estudianteId)
        {
            var calificaciones = _contexto.Calificaciones
                .Include(c => c.Curso)
                .Where(c => c.EstudianteId == estudianteId)
                .OrderByDescending(c => c.FechaRegistro)
                .Select(c => new
                {
                    c.CalificacionId,
                    c.Curso.Codigo,
                    c.Curso.Nombre,
                    NombreCurso = c.Curso.Nombre,
                    c.TipoEvaluacion,
                    c.Descripcion,
                    c.Nota,
                    c.Porcentaje,
                    c.FechaRegistro,
                    c.Observaciones
                })
                .ToList();

            return Ok(calificaciones);
        }

        // GET api/historial/estudiante/5/grafico
        [HttpGet]
        [Route("estudiante/{estudianteId:int}/grafico")]
        public IHttpActionResult ObtenerDatosGrafico(int estudianteId)
        {
            // Obtener promedios por cuatrimestre
            var promediosPorCuatrimestre = _contexto.Inscripciones
                .Include(i => i.Curso)
                .Where(i => i.EstudianteId == estudianteId && i.NotaFinal.HasValue)
                .GroupBy(i => i.Curso.Cuatrimestre)
                .Select(g => new
                {
                    Cuatrimestre = g.Key,
                    Promedio = g.Average(i => i.NotaFinal.Value),
                    CantidadCursos = g.Count(),
                    Aprobados = g.Count(i => i.Aprobado == true),
                    Reprobados = g.Count(i => i.Aprobado == false)
                })
                .OrderBy(x => x.Cuatrimestre)
                .ToList();

            // Obtener notas por tipo de evaluación
            var notasPorTipo = _contexto.Calificaciones
                .Where(c => c.EstudianteId == estudianteId)
                .GroupBy(c => c.TipoEvaluacion)
                .Select(g => new
                {
                    TipoEvaluacion = g.Key,
                    Promedio = g.Average(c => c.Nota),
                    Cantidad = g.Count()
                })
                .ToList();

            // Calcular promedio general
            var promedioGeneral = _contexto.Inscripciones
                .Where(i => i.EstudianteId == estudianteId && i.NotaFinal.HasValue)
                .Average(i => (decimal?)i.NotaFinal.Value) ?? 0;

            return Ok(new
            {
                promediosPorCuatrimestre = promediosPorCuatrimestre,
                notasPorTipo = notasPorTipo,
                promedioGeneral = promedioGeneral
            });
        }

        // POST api/historial/calificacion
        [HttpPost]
        [Route("calificacion")]
        public IHttpActionResult RegistrarCalificacion([FromBody] CalificacionViewModel modelo)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Verificar que el estudiante esté inscrito en el curso
            var inscripcion = _contexto.Inscripciones
                .FirstOrDefault(i => i.EstudianteId == modelo.EstudianteId &&
                                     i.CursoId == modelo.CursoId &&
                                     i.Estado == "Activo");

            if (inscripcion == null)
                return BadRequest("El estudiante no está inscrito en este curso");

            var calificacion = new Calificacion
            {
                EstudianteId = modelo.EstudianteId,
                CursoId = modelo.CursoId,
                TipoEvaluacion = modelo.TipoEvaluacion,
                Descripcion = modelo.Descripcion,
                Nota = modelo.Nota,
                Porcentaje = modelo.Porcentaje,
                Observaciones = modelo.Observaciones,
                FechaRegistro = DateTime.Now
            };

            _contexto.Calificaciones.Add(calificacion);
            _contexto.SaveChanges();

            // Actualizar nota final del estudiante en el curso
            ActualizarNotaFinal(modelo.EstudianteId, modelo.CursoId);

            return Ok(new { mensaje = "Calificación registrada exitosamente", id = calificacion.CalificacionId });
        }

        // Método para actualizar la nota final del estudiante
        private void ActualizarNotaFinal(int estudianteId, int cursoId)
        {
            var calificaciones = _contexto.Calificaciones
                .Where(c => c.EstudianteId == estudianteId && c.CursoId == cursoId)
                .ToList();

            if (calificaciones.Any())
            {
                // Calcular nota final ponderada
                decimal totalPorcentaje = calificaciones.Sum(c => c.Porcentaje);
                decimal notaFinal = 0;

                if (totalPorcentaje > 0)
                {
                    notaFinal = calificaciones.Sum(c => (c.Nota * c.Porcentaje) / 100);
                }

                // Actualizar inscripción
                var inscripcion = _contexto.Inscripciones
                    .FirstOrDefault(i => i.EstudianteId == estudianteId && i.CursoId == cursoId);

                if (inscripcion != null)
                {
                    inscripcion.NotaFinal = notaFinal;
                    inscripcion.Aprobado = notaFinal >= 70; // Nota mínima de aprobación
                    _contexto.SaveChanges();
                }
            }
        }

        // GET api/historial/curso/5/estudiantes
        [HttpGet]
        [Route("curso/{cursoId:int}/estudiantes")]
        public IHttpActionResult ObtenerEstudiantesCurso(int cursoId)
        {
            var estudiantes = _contexto.Inscripciones
                .Include(i => i.Estudiante)
                .Where(i => i.CursoId == cursoId && i.Estado == "Activo")
                .Select(i => new
                {
                    i.InscripcionId,
                    i.EstudianteId,
                    i.Estudiante.NombreCompleto,
                    i.Estudiante.Identificacion,
                    i.NotaFinal,
                    i.Aprobado,
                    i.FechaInscripcion
                })
                .OrderBy(e => e.NombreCompleto)
                .ToList();

            return Ok(estudiantes);
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

    // ViewModels para historial
    public class CalificacionViewModel
    {
        public int EstudianteId { get; set; }
        public int CursoId { get; set; }
        public string TipoEvaluacion { get; set; }
        public string Descripcion { get; set; }
        public decimal Nota { get; set; }
        public decimal Porcentaje { get; set; }
        public string Observaciones { get; set; }
    }
}
