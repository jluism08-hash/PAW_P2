using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using PAW_P2.Models;
using PAW_P2.Servicios;

namespace PAW_P2.Controllers
{
    // Controlador para autenticación de usuarios
    public class CuentaController : Controller
    {
        private readonly ServicioAutenticacion _servicioAuth;
        private readonly ServicioBitacora _servicioBitacora;

        public CuentaController()
        {
            _servicioAuth = new ServicioAutenticacion();
            _servicioBitacora = new ServicioBitacora();
        }

        // GET: Cuenta/Login
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: Cuenta/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel modelo, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            var usuario = _servicioAuth.Autenticar(modelo.Correo, modelo.Contrasena);

            if (usuario == null)
            {
                ModelState.AddModelError("", "Correo o contraseña incorrectos");
                return View(modelo);
            }

            // Crear ticket de autenticación
            var authTicket = new FormsAuthenticationTicket(
                1,
                usuario.Correo,
                DateTime.Now,
                DateTime.Now.AddMinutes(60),
                modelo.Recordarme,
                $"{usuario.UsuarioId}|{usuario.Rol.Nombre}|{usuario.NombreCompleto}"
            );

            string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
            var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);

            if (modelo.Recordarme)
            {
                authCookie.Expires = authTicket.Expiration;
            }

            Response.Cookies.Add(authCookie);

            // Guardar datos en sesión
            Session["UsuarioId"] = usuario.UsuarioId;
            Session["NombreUsuario"] = usuario.NombreCompleto;
            Session["RolUsuario"] = usuario.Rol.Nombre;
            Session["Permisos"] = _servicioAuth.ObtenerPermisos(usuario.UsuarioId);

            // Registrar en bitácora
            _servicioBitacora.RegistrarInicioSesion(usuario.UsuarioId, usuario.NombreCompleto);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        // GET: Cuenta/Logout
        public ActionResult Logout()
        {
            if (Session["UsuarioId"] != null)
            {
                int usuarioId = (int)Session["UsuarioId"];
                string nombreUsuario = Session["NombreUsuario"]?.ToString();
                _servicioBitacora.RegistrarCierreSesion(usuarioId, nombreUsuario);
            }

            FormsAuthentication.SignOut();
            Session.Clear();
            Session.Abandon();

            return RedirectToAction("Login");
        }

        // GET: Cuenta/AccesoDenegado
        public ActionResult AccesoDenegado()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _servicioAuth?.Dispose();
                _servicioBitacora?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    // Modelo para el login
    public class LoginModel
    {
        public string Correo { get; set; }
        public string Contrasena { get; set; }
        public bool Recordarme { get; set; }
    }
}
