using System.Web.Mvc;

namespace PAW_P2.Controllers
{
    // Controlador MVC para gestión de usuarios
    [Authorize]
    public class UsuariosController : Controller
    {
        // GET: Usuarios
        public ActionResult Index()
        {
            return View();
        }

        // GET: Usuarios/Crear
        public ActionResult Crear()
        {
            return View();
        }

        // GET: Usuarios/Editar/5
        public ActionResult Editar(int id)
        {
            ViewBag.UsuarioId = id;
            return View();
        }

        // GET: Usuarios/Roles
        public ActionResult Roles()
        {
            return View();
        }

        // GET: Usuarios/CrearRol
        public ActionResult CrearRol()
        {
            return View();
        }

        // GET: Usuarios/EditarRol/5
        public ActionResult EditarRol(int id)
        {
            ViewBag.RolId = id;
            return View();
        }
    }
}
