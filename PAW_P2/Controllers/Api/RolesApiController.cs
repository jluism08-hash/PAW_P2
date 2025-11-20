using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using Newtonsoft.Json;
using PAW_P2.Models;
using PAW_P2.Servicios;

namespace PAW_P2.Controllers.Api
{
    // Controlador API para gestión de roles y permisos
    [RoutePrefix("api/roles")]
    public class RolesApiController : ApiController
    {
        private readonly SistemaAcademicoContexto _contexto;
        private readonly ServicioBitacora _servicioBitacora;

        public RolesApiController()
        {
            _contexto = new SistemaAcademicoContexto();
            _servicioBitacora = new ServicioBitacora();
        }

        // GET api/roles
        [HttpGet]
        [Route("")]
        public IHttpActionResult ObtenerTodos()
        {
            var roles = _contexto.Roles
                .Where(r => r.Activo)
                .Select(r => new
                {
                    r.RolId,
                    r.Nombre,
                    r.Descripcion,
                    r.FechaCreacion,
                    CantidadUsuarios = r.Usuarios.Count(u => u.Activo)
                })
                .ToList();

            return Ok(roles);
        }

        // GET api/roles/5
        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult ObtenerPorId(int id)
        {
            var rol = _contexto.Roles
                .Include(r => r.RolPermisos.Select(rp => rp.Permiso))
                .Where(r => r.RolId == id)
                .Select(r => new
                {
                    r.RolId,
                    r.Nombre,
                    r.Descripcion,
                    r.FechaCreacion,
                    Permisos = r.RolPermisos.Select(rp => new
                    {
                        rp.Permiso.PermisoId,
                        rp.Permiso.Nombre,
                        rp.Permiso.Modulo
                    })
                })
                .FirstOrDefault();

            if (rol == null)
                return NotFound();

            return Ok(rol);
        }

        // GET api/roles/permisos
        [HttpGet]
        [Route("permisos")]
        public IHttpActionResult ObtenerPermisos()
        {
            var permisos = _contexto.Permisos
                .Where(p => p.Activo)
                .OrderBy(p => p.Modulo)
                .ThenBy(p => p.Nombre)
                .Select(p => new
                {
                    p.PermisoId,
                    p.Nombre,
                    p.Descripcion,
                    p.Modulo
                })
                .ToList();

            return Ok(permisos);
        }

        // POST api/roles
        [HttpPost]
        [Route("")]
        public IHttpActionResult Crear([FromBody] RolCrearViewModel modelo)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Verificar si el nombre ya existe
            if (_contexto.Roles.Any(r => r.Nombre == modelo.Nombre && r.Activo))
                return BadRequest("Ya existe un rol con ese nombre");

            var rol = new Rol
            {
                Nombre = modelo.Nombre,
                Descripcion = modelo.Descripcion,
                FechaCreacion = DateTime.Now,
                Activo = true
            };

            _contexto.Roles.Add(rol);
            _contexto.SaveChanges();

            // Asignar permisos
            if (modelo.PermisosIds != null && modelo.PermisosIds.Length > 0)
            {
                foreach (var permisoId in modelo.PermisosIds)
                {
                    _contexto.RolPermisos.Add(new RolPermiso
                    {
                        RolId = rol.RolId,
                        PermisoId = permisoId
                    });
                }
                _contexto.SaveChanges();
            }

            // Registrar en bitácora
            _servicioBitacora.RegistrarCreacion(
                modelo.UsuarioCreadorId,
                "Usuarios",
                "Rol",
                rol.RolId,
                $"Se creó el rol {rol.Nombre}",
                JsonConvert.SerializeObject(new { rol.Nombre, modelo.PermisosIds })
            );

            return Ok(new { mensaje = "Rol creado exitosamente", id = rol.RolId });
        }

        // PUT api/roles/5
        [HttpPut]
        [Route("{id:int}")]
        public IHttpActionResult Actualizar(int id, [FromBody] RolEditarViewModel modelo)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var rol = _contexto.Roles.Find(id);
            if (rol == null)
                return NotFound();

            // Guardar datos anteriores
            var permisosAnteriores = _contexto.RolPermisos
                .Where(rp => rp.RolId == id)
                .Select(rp => rp.PermisoId)
                .ToArray();

            var datosAnteriores = JsonConvert.SerializeObject(new
            {
                rol.Nombre,
                rol.Descripcion,
                Permisos = permisosAnteriores
            });

            rol.Nombre = modelo.Nombre;
            rol.Descripcion = modelo.Descripcion;

            // Actualizar permisos
            var permisosActuales = _contexto.RolPermisos.Where(rp => rp.RolId == id);
            _contexto.RolPermisos.RemoveRange(permisosActuales);

            if (modelo.PermisosIds != null && modelo.PermisosIds.Length > 0)
            {
                foreach (var permisoId in modelo.PermisosIds)
                {
                    _contexto.RolPermisos.Add(new RolPermiso
                    {
                        RolId = rol.RolId,
                        PermisoId = permisoId
                    });
                }
            }

            _contexto.SaveChanges();

            // Registrar en bitácora
            _servicioBitacora.RegistrarModificacion(
                modelo.UsuarioModificadorId,
                "Usuarios",
                "Rol",
                id,
                $"Se actualizó el rol {rol.Nombre}",
                datosAnteriores,
                JsonConvert.SerializeObject(new { rol.Nombre, modelo.PermisosIds })
            );

            return Ok(new { mensaje = "Rol actualizado exitosamente" });
        }

        // DELETE api/roles/5
        [HttpDelete]
        [Route("{id:int}")]
        public IHttpActionResult Eliminar(int id, [FromUri] int usuarioEliminadorId = 0)
        {
            var rol = _contexto.Roles.Find(id);
            if (rol == null)
                return NotFound();

            // Verificar si hay usuarios con este rol
            if (_contexto.Usuarios.Any(u => u.RolId == id && u.Activo))
                return BadRequest("No se puede eliminar el rol porque tiene usuarios asignados");

            // Eliminación lógica
            rol.Activo = false;
            _contexto.SaveChanges();

            // Registrar en bitácora
            _servicioBitacora.RegistrarEliminacion(
                usuarioEliminadorId,
                "Usuarios",
                "Rol",
                id,
                $"Se eliminó el rol {rol.Nombre}"
            );

            return Ok(new { mensaje = "Rol eliminado exitosamente" });
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

    // ViewModels para roles
    public class RolCrearViewModel
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int[] PermisosIds { get; set; }
        public int UsuarioCreadorId { get; set; }
    }

    public class RolEditarViewModel
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int[] PermisosIds { get; set; }
        public int UsuarioModificadorId { get; set; }
    }
}
