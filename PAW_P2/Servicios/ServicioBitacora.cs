using System;
using System.Web;
using PAW_P2.Models;

namespace PAW_P2.Servicios
{
    // Servicio para registrar acciones en la bitácora del sistema
    public class ServicioBitacora
    {
        private readonly SistemaAcademicoContexto _contexto;

        public ServicioBitacora()
        {
            _contexto = new SistemaAcademicoContexto();
        }

        public ServicioBitacora(SistemaAcademicoContexto contexto)
        {
            _contexto = contexto;
        }

        // Método principal para registrar una acción
        public void RegistrarAccion(int? usuarioId, string accion, string modulo,
            string descripcion, string entidadAfectada = null, int? entidadId = null,
            string datosAnteriores = null, string datosNuevos = null)
        {
            var bitacora = new Bitacora
            {
                UsuarioId = usuarioId,
                Accion = accion,
                Modulo = modulo,
                Descripcion = descripcion,
                FechaHora = DateTime.Now,
                DireccionIP = ObtenerDireccionIP(),
                Navegador = ObtenerNavegador(),
                EntidadAfectada = entidadAfectada,
                EntidadId = entidadId,
                DatosAnteriores = datosAnteriores,
                DatosNuevos = datosNuevos
            };

            _contexto.Bitacoras.Add(bitacora);
            _contexto.SaveChanges();
        }

        // Registrar inicio de sesión
        public void RegistrarInicioSesion(int usuarioId, string nombreUsuario)
        {
            RegistrarAccion(usuarioId, "Inicio de Sesión", "Autenticación",
                $"El usuario {nombreUsuario} inició sesión en el sistema");
        }

        // Registrar cierre de sesión
        public void RegistrarCierreSesion(int usuarioId, string nombreUsuario)
        {
            RegistrarAccion(usuarioId, "Cierre de Sesión", "Autenticación",
                $"El usuario {nombreUsuario} cerró sesión en el sistema");
        }

        // Registrar creación de entidad
        public void RegistrarCreacion(int usuarioId, string modulo, string entidad,
            int entidadId, string descripcion, string datosNuevos = null)
        {
            RegistrarAccion(usuarioId, "Creación", modulo, descripcion,
                entidad, entidadId, null, datosNuevos);
        }

        // Registrar modificación de entidad
        public void RegistrarModificacion(int usuarioId, string modulo, string entidad,
            int entidadId, string descripcion, string datosAnteriores = null, string datosNuevos = null)
        {
            RegistrarAccion(usuarioId, "Modificación", modulo, descripcion,
                entidad, entidadId, datosAnteriores, datosNuevos);
        }

        // Registrar eliminación de entidad
        public void RegistrarEliminacion(int usuarioId, string modulo, string entidad,
            int entidadId, string descripcion, string datosAnteriores = null)
        {
            RegistrarAccion(usuarioId, "Eliminación", modulo, descripcion,
                entidad, entidadId, datosAnteriores, null);
        }

        // Registrar cambio de rol
        public void RegistrarCambioRol(int usuarioId, int usuarioAfectadoId,
            string rolAnterior, string rolNuevo)
        {
            RegistrarAccion(usuarioId, "Cambio de Rol", "Usuarios",
                $"Se cambió el rol del usuario {usuarioAfectadoId} de '{rolAnterior}' a '{rolNuevo}'",
                "Usuario", usuarioAfectadoId, rolAnterior, rolNuevo);
        }

        // Obtener dirección IP del cliente
        private string ObtenerDireccionIP()
        {
            try
            {
                if (HttpContext.Current != null)
                {
                    string ip = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                    if (string.IsNullOrEmpty(ip))
                    {
                        ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                    }
                    return ip ?? "Desconocida";
                }
                return "No disponible";
            }
            catch
            {
                return "Error al obtener IP";
            }
        }

        // Obtener información del navegador
        private string ObtenerNavegador()
        {
            try
            {
                if (HttpContext.Current != null)
                {
                    return HttpContext.Current.Request.UserAgent ?? "Desconocido";
                }
                return "No disponible";
            }
            catch
            {
                return "Error al obtener navegador";
            }
        }

        public void Dispose()
        {
            _contexto?.Dispose();
        }
    }
}
