using System;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using PAW_P2.Models;

namespace PAW_P2.Servicios
{
    // Servicio para manejar la autenticación de usuarios
    public class ServicioAutenticacion
    {
        private readonly SistemaAcademicoContexto _contexto;

        public ServicioAutenticacion()
        {
            _contexto = new SistemaAcademicoContexto();
        }

        public ServicioAutenticacion(SistemaAcademicoContexto contexto)
        {
            _contexto = contexto;
        }

        // Método para autenticar usuario
        public Usuario Autenticar(string correo, string contrasena)
        {
            if (string.IsNullOrEmpty(correo) || string.IsNullOrEmpty(contrasena))
                return null;

            // Hashear la contraseña para comparar
            string contrasenaHash = ObtenerHash(contrasena);

            // Buscar usuario por correo y contraseña
            var usuario = _contexto.Usuarios
                .Include("Rol")
                .Include("Rol.RolPermisos")
                .Include("Rol.RolPermisos.Permiso")
                .FirstOrDefault(u => u.Correo == correo &&
                                     u.Contrasena == contrasenaHash &&
                                     u.Activo);

            if (usuario != null)
            {
                // Actualizar último acceso
                usuario.UltimoAcceso = DateTime.Now;
                _contexto.SaveChanges();
            }

            return usuario;
        }

        // Método para hashear contraseñas con SHA256
        public string ObtenerHash(string texto)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(texto));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // Método para validar si el usuario tiene un permiso específico
        public bool TienePermiso(int usuarioId, string nombrePermiso)
        {
            var usuario = _contexto.Usuarios
                .Include("Rol.RolPermisos.Permiso")
                .FirstOrDefault(u => u.UsuarioId == usuarioId);

            if (usuario == null || usuario.Rol == null)
                return false;

            return usuario.Rol.RolPermisos
                .Any(rp => rp.Permiso.Nombre == nombrePermiso && rp.Permiso.Activo);
        }

        // Método para obtener todos los permisos de un usuario
        public string[] ObtenerPermisos(int usuarioId)
        {
            var usuario = _contexto.Usuarios
                .Include("Rol.RolPermisos.Permiso")
                .FirstOrDefault(u => u.UsuarioId == usuarioId);

            if (usuario == null || usuario.Rol == null)
                return new string[0];

            return usuario.Rol.RolPermisos
                .Where(rp => rp.Permiso.Activo)
                .Select(rp => rp.Permiso.Nombre)
                .ToArray();
        }

        // Método para cambiar contraseña
        public bool CambiarContrasena(int usuarioId, string contrasenaActual, string nuevaContrasena)
        {
            var usuario = _contexto.Usuarios.Find(usuarioId);
            if (usuario == null)
                return false;

            string hashActual = ObtenerHash(contrasenaActual);
            if (usuario.Contrasena != hashActual)
                return false;

            usuario.Contrasena = ObtenerHash(nuevaContrasena);
            _contexto.SaveChanges();
            return true;
        }

        // Método para registrar un nuevo usuario
        public Usuario RegistrarUsuario(Usuario nuevoUsuario, string contrasena)
        {
            // Verificar si el correo ya existe
            if (_contexto.Usuarios.Any(u => u.Correo == nuevoUsuario.Correo))
                return null;

            // Hashear la contraseña
            nuevoUsuario.Contrasena = ObtenerHash(contrasena);
            nuevoUsuario.FechaCreacion = DateTime.Now;
            nuevoUsuario.Activo = true;

            _contexto.Usuarios.Add(nuevoUsuario);
            _contexto.SaveChanges();

            return nuevoUsuario;
        }

        public void Dispose()
        {
            _contexto?.Dispose();
        }
    }
}
