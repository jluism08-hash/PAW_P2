using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using Newtonsoft.Json;
using PAW_P2.Models;
using PAW_P2.Servicios;

namespace PAW_P2.Controllers.Api
{
    // Controlador API para gestión de usuarios
    [RoutePrefix("api/usuarios")]
    public class UsuariosApiController : ApiController
    {
        private readonly SistemaAcademicoContexto _contexto;
        private readonly ServicioBitacora _servicioBitacora;
        private readonly ServicioAutenticacion _servicioAuth;

        public UsuariosApiController()
        {
            _contexto = new SistemaAcademicoContexto();
            _servicioBitacora = new ServicioBitacora();
            _servicioAuth = new ServicioAutenticacion();
        }

        // GET api/usuarios
        [HttpGet]
        [Route("")]
        public IHttpActionResult ObtenerTodos()
        {
            var usuarios = _contexto.Usuarios
                .Include(u => u.Rol)
                .Where(u => u.Activo)
                .Select(u => new
                {
                    u.UsuarioId,
                    u.NombreCompleto,
                    u.Correo,
                    u.Identificacion,
                    u.FechaCreacion,
                    u.UltimoAcceso,
                    u.Activo,
                    Rol = u.Rol.Nombre,
                    u.RolId
                })
                .ToList();

            return Ok(usuarios);
        }

        // GET api/usuarios/5
        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult ObtenerPorId(int id)
        {
            var usuario = _contexto.Usuarios
                .Include(u => u.Rol)
                .Where(u => u.UsuarioId == id)
                .Select(u => new
                {
                    u.UsuarioId,
                    u.NombreCompleto,
                    u.Correo,
                    u.Identificacion,
                    u.FechaCreacion,
                    u.UltimoAcceso,
                    u.Activo,
                    Rol = u.Rol.Nombre,
                    u.RolId
                })
                .FirstOrDefault();

            if (usuario == null)
                return NotFound();

            return Ok(usuario);
        }

        // GET api/usuarios/docentes
        [HttpGet]
        [Route("docentes")]
        public IHttpActionResult ObtenerDocentes()
        {
            var docentes = _contexto.Usuarios
                .Include(u => u.Rol)
                .Where(u => u.Activo && u.Rol.Nombre == "Docente")
                .Select(u => new
                {
                    u.UsuarioId,
                    u.NombreCompleto,
                    u.Correo
                })
                .ToList();

            return Ok(docentes);
        }

        // GET api/usuarios/estudiantes
        [HttpGet]
        [Route("estudiantes")]
        public IHttpActionResult ObtenerEstudiantes()
        {
            var estudiantes = _contexto.Usuarios
                .Include(u => u.Rol)
                .Where(u => u.Activo && u.Rol.Nombre == "Estudiante")
                .Select(u => new
                {
                    u.UsuarioId,
                    u.NombreCompleto,
                    u.Correo,
                    u.Identificacion
                })
                .ToList();

            return Ok(estudiantes);
        }

        // POST api/usuarios
        [HttpPost]
        [Route("")]
        public IHttpActionResult Crear([FromBody] UsuarioCrearViewModel modelo)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Verificar si el correo ya existe
            if (_contexto.Usuarios.Any(u => u.Correo == modelo.Correo))
                return BadRequest("El correo electrónico ya está registrado");

            // Verificar si la identificación ya existe
            if (!string.IsNullOrEmpty(modelo.Identificacion) &&
                _contexto.Usuarios.Any(u => u.Identificacion == modelo.Identificacion))
                return BadRequest("La identificación ya está registrada");

            var usuario = new Usuario
            {
                NombreCompleto = modelo.NombreCompleto,
                Correo = modelo.Correo,
                Identificacion = modelo.Identificacion,
                RolId = modelo.RolId,
                Contrasena = _servicioAuth.ObtenerHash(modelo.Contrasena),
                FechaCreacion = DateTime.Now,
                Activo = true
            };

            _contexto.Usuarios.Add(usuario);
            _contexto.SaveChanges();

            // Registrar en bitácora
            _servicioBitacora.RegistrarCreacion(
                modelo.UsuarioCreadorId,
                "Usuarios",
                "Usuario",
                usuario.UsuarioId,
                $"Se creó el usuario {usuario.NombreCompleto}",
                JsonConvert.SerializeObject(new { usuario.NombreCompleto, usuario.Correo, usuario.RolId })
            );

            return Ok(new { mensaje = "Usuario creado exitosamente", id = usuario.UsuarioId });
        }

        // PUT api/usuarios/5
        [HttpPut]
        [Route("{id:int}")]
        public IHttpActionResult Actualizar(int id, [FromBody] UsuarioEditarViewModel modelo)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var usuario = _contexto.Usuarios.Find(id);
            if (usuario == null)
                return NotFound();

            // Guardar datos anteriores para bitácora
            var datosAnteriores = JsonConvert.SerializeObject(new
            {
                usuario.NombreCompleto,
                usuario.Correo,
                usuario.RolId,
                usuario.Identificacion
            });

            // Verificar si el nuevo correo ya existe
            if (_contexto.Usuarios.Any(u => u.Correo == modelo.Correo && u.UsuarioId != id))
                return BadRequest("El correo electrónico ya está registrado");

            // Obtener rol anterior para bitácora
            var rolAnterior = _contexto.Roles.Find(usuario.RolId)?.Nombre;
            var rolNuevo = _contexto.Roles.Find(modelo.RolId)?.Nombre;

            usuario.NombreCompleto = modelo.NombreCompleto;
            usuario.Correo = modelo.Correo;
            usuario.Identificacion = modelo.Identificacion;

            // Si cambió el rol, registrar el cambio
            if (usuario.RolId != modelo.RolId)
            {
                usuario.RolId = modelo.RolId;
                _servicioBitacora.RegistrarCambioRol(
                    modelo.UsuarioModificadorId,
                    id,
                    rolAnterior,
                    rolNuevo
                );
            }

            _contexto.SaveChanges();

            // Registrar modificación en bitácora
            _servicioBitacora.RegistrarModificacion(
                modelo.UsuarioModificadorId,
                "Usuarios",
                "Usuario",
                id,
                $"Se actualizó el usuario {usuario.NombreCompleto}",
                datosAnteriores,
                JsonConvert.SerializeObject(new { usuario.NombreCompleto, usuario.Correo, usuario.RolId })
            );

            return Ok(new { mensaje = "Usuario actualizado exitosamente" });
        }

        // DELETE api/usuarios/5
        [HttpDelete]
        [Route("{id:int}")]
        public IHttpActionResult Eliminar(int id, [FromUri] int usuarioEliminadorId = 0)
        {
            var usuario = _contexto.Usuarios.Find(id);
            if (usuario == null)
                return NotFound();

            // Guardar datos para bitácora
            var datosAnteriores = JsonConvert.SerializeObject(new
            {
                usuario.NombreCompleto,
                usuario.Correo,
                usuario.RolId
            });

            // Eliminación lógica
            usuario.Activo = false;
            _contexto.SaveChanges();

            // Registrar en bitácora
            _servicioBitacora.RegistrarEliminacion(
                usuarioEliminadorId,
                "Usuarios",
                "Usuario",
                id,
                $"Se eliminó el usuario {usuario.NombreCompleto}",
                datosAnteriores
            );

            return Ok(new { mensaje = "Usuario eliminado exitosamente" });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _contexto?.Dispose();
                _servicioBitacora?.Dispose();
                _servicioAuth?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    // ViewModels para usuarios
    public class UsuarioCrearViewModel
    {
        public string NombreCompleto { get; set; }
        public string Correo { get; set; }
        public string Contrasena { get; set; }
        public string Identificacion { get; set; }
        public int RolId { get; set; }
        public int UsuarioCreadorId { get; set; }
    }

    public class UsuarioEditarViewModel
    {
        public string NombreCompleto { get; set; }
        public string Correo { get; set; }
        public string Identificacion { get; set; }
        public int RolId { get; set; }
        public int UsuarioModificadorId { get; set; }
    }
}
