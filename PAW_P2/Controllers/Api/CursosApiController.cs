using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using Newtonsoft.Json;
using PAW_P2.Models;
using PAW_P2.Servicios;

namespace PAW_P2.Controllers.Api
{
    // Controlador API para gestión de cursos
    [RoutePrefix("api/cursos")]
    public class CursosApiController : ApiController
    {
        private readonly SistemaAcademicoContexto _contexto;
        private readonly ServicioBitacora _servicioBitacora;

        public CursosApiController()
        {
            _contexto = new SistemaAcademicoContexto();
            _servicioBitacora = new ServicioBitacora();
        }

        // GET api/cursos
        [HttpGet]
        [Route("")]
        public IHttpActionResult ObtenerTodos()
        {
            var cursos = _contexto.Cursos
                .Where(c => c.Activo)
                .Select(c => new
                {
                    c.CursoId,
                    c.Codigo,
                    c.Nombre,
                    c.Descripcion,
                    c.Creditos,
                    c.Cuatrimestre,
                    c.FechaCreacion,
                    CantidadDocentes = c.DocentesAsignados.Count(d => d.Activo),
                    CantidadEstudiantes = c.Inscripciones.Count(i => i.Estado == "Activo")
                })
                .OrderBy(c => c.Codigo)
                .ToList();

            return Ok(cursos);
        }

        // GET api/cursos/5
        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult ObtenerPorId(int id)
        {
            var curso = _contexto.Cursos
                .Include(c => c.DocentesAsignados.Select(d => d.Docente))
                .Where(c => c.CursoId == id)
                .Select(c => new
                {
                    c.CursoId,
                    c.Codigo,
                    c.Nombre,
                    c.Descripcion,
                    c.Creditos,
                    c.Cuatrimestre,
                    c.FechaCreacion,
                    c.FechaModificacion,
                    Docentes = c.DocentesAsignados.Where(d => d.Activo).Select(d => new
                    {
                        d.CursoDocenteId,
                        d.DocenteId,
                        d.Docente.NombreCompleto,
                        d.Horario,
                        d.FechaAsignacion
                    })
                })
                .FirstOrDefault();

            if (curso == null)
                return NotFound();

            return Ok(curso);
        }

        // POST api/cursos
        [HttpPost]
        [Route("")]
        public IHttpActionResult Crear([FromBody] CursoCrearViewModel modelo)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Verificar si el código ya existe
            if (_contexto.Cursos.Any(c => c.Codigo == modelo.Codigo))
                return BadRequest("Ya existe un curso con ese código");

            var curso = new Curso
            {
                Codigo = modelo.Codigo,
                Nombre = modelo.Nombre,
                Descripcion = modelo.Descripcion,
                Creditos = modelo.Creditos,
                Cuatrimestre = modelo.Cuatrimestre,
                FechaCreacion = DateTime.Now,
                CreadoPorId = modelo.UsuarioCreadorId,
                Activo = true
            };

            _contexto.Cursos.Add(curso);
            _contexto.SaveChanges();

            // Registrar en bitácora
            _servicioBitacora.RegistrarCreacion(
                modelo.UsuarioCreadorId,
                "Cursos",
                "Curso",
                curso.CursoId,
                $"Se creó el curso {curso.Codigo} - {curso.Nombre}",
                JsonConvert.SerializeObject(new { curso.Codigo, curso.Nombre, curso.Creditos })
            );

            return Ok(new { mensaje = "Curso creado exitosamente", id = curso.CursoId });
        }

        // PUT api/cursos/5
        [HttpPut]
        [Route("{id:int}")]
        public IHttpActionResult Actualizar(int id, [FromBody] CursoEditarViewModel modelo)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var curso = _contexto.Cursos.Find(id);
            if (curso == null)
                return NotFound();

            // Verificar si se puede cambiar el código
            bool tieneEstudiantes = _contexto.Inscripciones.Any(i => i.CursoId == id);
            if (tieneEstudiantes && curso.Codigo != modelo.Codigo)
                return BadRequest("No se puede cambiar el código del curso porque tiene estudiantes inscritos");

            // Guardar datos anteriores
            var datosAnteriores = JsonConvert.SerializeObject(new
            {
                curso.Codigo,
                curso.Nombre,
                curso.Descripcion,
                curso.Creditos,
                curso.Cuatrimestre
            });

            curso.Codigo = modelo.Codigo;
            curso.Nombre = modelo.Nombre;
            curso.Descripcion = modelo.Descripcion;
            curso.Creditos = modelo.Creditos;
            curso.Cuatrimestre = modelo.Cuatrimestre;
            curso.FechaModificacion = DateTime.Now;
            curso.ModificadoPorId = modelo.UsuarioModificadorId;

            _contexto.SaveChanges();

            // Registrar en bitácora
            _servicioBitacora.RegistrarModificacion(
                modelo.UsuarioModificadorId,
                "Cursos",
                "Curso",
                id,
                $"Se actualizó el curso {curso.Codigo}",
                datosAnteriores,
                JsonConvert.SerializeObject(new { curso.Codigo, curso.Nombre, curso.Creditos })
            );

            return Ok(new { mensaje = "Curso actualizado exitosamente" });
        }

        // DELETE api/cursos/5
        [HttpDelete]
        [Route("{id:int}")]
        public IHttpActionResult Eliminar(int id, [FromUri] int usuarioEliminadorId = 0)
        {
            var curso = _contexto.Cursos.Find(id);
            if (curso == null)
                return NotFound();

            // Verificar si tiene estudiantes inscritos
            if (_contexto.Inscripciones.Any(i => i.CursoId == id && i.Estado == "Activo"))
                return BadRequest("No se puede eliminar el curso porque tiene estudiantes inscritos");

            // Guardar datos para bitácora
            var datosAnteriores = JsonConvert.SerializeObject(new
            {
                curso.Codigo,
                curso.Nombre
            });

            // Eliminación lógica
            curso.Activo = false;
            _contexto.SaveChanges();

            // Registrar en bitácora
            _servicioBitacora.RegistrarEliminacion(
                usuarioEliminadorId,
                "Cursos",
                "Curso",
                id,
                $"Se eliminó el curso {curso.Codigo} - {curso.Nombre}",
                datosAnteriores
            );

            return Ok(new { mensaje = "Curso eliminado exitosamente" });
        }

        // POST api/cursos/5/asignar-docente
        [HttpPost]
        [Route("{id:int}/asignar-docente")]
        public IHttpActionResult AsignarDocente(int id, [FromBody] AsignarDocenteViewModel modelo)
        {
            var curso = _contexto.Cursos.Find(id);
            if (curso == null)
                return NotFound();

            // Verificar si el docente existe
            var docente = _contexto.Usuarios.Find(modelo.DocenteId);
            if (docente == null)
                return BadRequest("El docente no existe");

            // Verificar si ya está asignado
            if (_contexto.CursoDocentes.Any(cd => cd.CursoId == id && cd.DocenteId == modelo.DocenteId && cd.Activo))
                return BadRequest("El docente ya está asignado a este curso");

            // Verificar conflicto de horario (simplificado)
            if (!string.IsNullOrEmpty(modelo.Horario))
            {
                var conflicto = _contexto.CursoDocentes
                    .Any(cd => cd.DocenteId == modelo.DocenteId &&
                               cd.Horario == modelo.Horario &&
                               cd.Activo &&
                               cd.CursoId != id);

                if (conflicto)
                    return BadRequest("El docente tiene un conflicto de horario");
            }

            var asignacion = new CursoDocente
            {
                CursoId = id,
                DocenteId = modelo.DocenteId,
                Horario = modelo.Horario,
                FechaAsignacion = DateTime.Now,
                Activo = true
            };

            _contexto.CursoDocentes.Add(asignacion);
            _contexto.SaveChanges();

            // Registrar en bitácora
            _servicioBitacora.RegistrarCreacion(
                modelo.UsuarioAsignadorId,
                "Cursos",
                "AsignacionDocente",
                asignacion.CursoDocenteId,
                $"Se asignó el docente {docente.NombreCompleto} al curso {curso.Codigo}"
            );

            return Ok(new { mensaje = "Docente asignado exitosamente" });
        }

        // DELETE api/cursos/5/remover-docente/3
        [HttpDelete]
        [Route("{cursoId:int}/remover-docente/{docenteId:int}")]
        public IHttpActionResult RemoverDocente(int cursoId, int docenteId, [FromUri] int usuarioId = 0)
        {
            var asignacion = _contexto.CursoDocentes
                .FirstOrDefault(cd => cd.CursoId == cursoId && cd.DocenteId == docenteId && cd.Activo);

            if (asignacion == null)
                return NotFound();

            asignacion.Activo = false;
            _contexto.SaveChanges();

            // Registrar en bitácora
            _servicioBitacora.RegistrarEliminacion(
                usuarioId,
                "Cursos",
                "AsignacionDocente",
                asignacion.CursoDocenteId,
                $"Se removió la asignación del docente del curso"
            );

            return Ok(new { mensaje = "Docente removido exitosamente" });
        }

        // POST api/cursos/5/matricular
        [HttpPost]
        [Route("{id:int}/matricular")]
        public IHttpActionResult Matricular(int id, [FromBody] MatricularViewModel modelo)
        {
            var curso = _contexto.Cursos.Find(id);
            if (curso == null || !curso.Activo)
                return NotFound();

            // Verificar si el estudiante existe
            var estudiante = _contexto.Usuarios.Find(modelo.EstudianteId);
            if (estudiante == null)
                return BadRequest("El estudiante no existe");

            // Verificar si ya está matriculado
            var yaMatriculado = _contexto.Inscripciones
                .Any(i => i.CursoId == id && i.EstudianteId == modelo.EstudianteId && i.Estado == "Activo");

            if (yaMatriculado)
                return BadRequest("El estudiante ya está matriculado en este curso");

            var inscripcion = new Inscripcion
            {
                CursoId = id,
                EstudianteId = modelo.EstudianteId,
                FechaInscripcion = DateTime.Now,
                Estado = "Activo"
            };

            _contexto.Inscripciones.Add(inscripcion);
            _contexto.SaveChanges();

            // Registrar en bitácora
            _servicioBitacora.RegistrarCreacion(
                modelo.EstudianteId,
                "Cursos",
                "Inscripcion",
                inscripcion.InscripcionId,
                $"El estudiante {estudiante.NombreCompleto} se matriculó en el curso {curso.Codigo}"
            );

            return Ok(new { mensaje = "Matrícula realizada exitosamente", id = inscripcion.InscripcionId });
        }

        // DELETE api/cursos/5/retirar/3
        [HttpDelete]
        [Route("{cursoId:int}/retirar/{estudianteId:int}")]
        public IHttpActionResult Retirar(int cursoId, int estudianteId)
        {
            var inscripcion = _contexto.Inscripciones
                .FirstOrDefault(i => i.CursoId == cursoId && i.EstudianteId == estudianteId && i.Estado == "Activo");

            if (inscripcion == null)
                return NotFound();

            inscripcion.Estado = "Retirado";
            _contexto.SaveChanges();

            var curso = _contexto.Cursos.Find(cursoId);
            var estudiante = _contexto.Usuarios.Find(estudianteId);

            // Registrar en bitácora
            _servicioBitacora.RegistrarEliminacion(
                estudianteId,
                "Cursos",
                "Inscripcion",
                inscripcion.InscripcionId,
                $"El estudiante {estudiante?.NombreCompleto} se retiró del curso {curso?.Codigo}"
            );

            return Ok(new { mensaje = "Retiro realizado exitosamente" });
        }

        // GET api/cursos/mis-cursos/5
        [HttpGet]
        [Route("mis-cursos/{estudianteId:int}")]
        public IHttpActionResult ObtenerMisCursos(int estudianteId)
        {
            var cursos = _contexto.Inscripciones
                .Include(i => i.Curso)
                .Where(i => i.EstudianteId == estudianteId && i.Estado == "Activo")
                .Select(i => new
                {
                    i.InscripcionId,
                    i.CursoId,
                    i.Curso.Codigo,
                    i.Curso.Nombre,
                    i.Curso.Creditos,
                    i.Curso.Cuatrimestre,
                    i.FechaInscripcion,
                    Docentes = i.Curso.DocentesAsignados.Where(d => d.Activo).Select(d => d.Docente.NombreCompleto)
                })
                .ToList();

            return Ok(cursos);
        }

        // GET api/cursos/disponibles/5
        [HttpGet]
        [Route("disponibles/{estudianteId:int}")]
        public IHttpActionResult ObtenerCursosDisponibles(int estudianteId)
        {
            // Obtener IDs de cursos donde el estudiante ya está matriculado
            var cursosMatriculados = _contexto.Inscripciones
                .Where(i => i.EstudianteId == estudianteId && i.Estado == "Activo")
                .Select(i => i.CursoId)
                .ToList();

            var cursosDisponibles = _contexto.Cursos
                .Where(c => c.Activo && !cursosMatriculados.Contains(c.CursoId))
                .Select(c => new
                {
                    c.CursoId,
                    c.Codigo,
                    c.Nombre,
                    c.Descripcion,
                    c.Creditos,
                    c.Cuatrimestre,
                    CantidadDocentes = c.DocentesAsignados.Count(d => d.Activo),
                    CantidadEstudiantes = c.Inscripciones.Count(i => i.Estado == "Activo")
                })
                .OrderBy(c => c.Codigo)
                .ToList();

            return Ok(cursosDisponibles);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _contexto?.Dispose();
                _servicioBitacora?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    // ViewModels para cursos
    public class CursoCrearViewModel
    {
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int Creditos { get; set; }
        public string Cuatrimestre { get; set; }
        public int UsuarioCreadorId { get; set; }
    }

    public class CursoEditarViewModel
    {
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int Creditos { get; set; }
        public string Cuatrimestre { get; set; }
        public int UsuarioModificadorId { get; set; }
    }

    public class AsignarDocenteViewModel
    {
        public int DocenteId { get; set; }
        public string Horario { get; set; }
        public int UsuarioAsignadorId { get; set; }
    }

    public class MatricularViewModel
    {
        public int EstudianteId { get; set; }
    }
}
