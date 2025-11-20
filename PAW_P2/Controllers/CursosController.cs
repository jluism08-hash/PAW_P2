using System.Web.Mvc;

namespace PAW_P2.Controllers
{
    // Controlador MVC para gestión de cursos
    [Authorize]
    public class CursosController : Controller
    {
        // GET: Cursos
        public ActionResult Index()
        {
            return View();
        }

        // GET: Cursos/Crear
        public ActionResult Crear()
        {
            return View();
        }

        // GET: Cursos/Editar/5
        public ActionResult Editar(int id)
        {
            ViewBag.CursoId = id;
            return View();
        }

        // GET: Cursos/Detalle/5
        public ActionResult Detalle(int id)
        {
            ViewBag.CursoId = id;
            return View();
        }

        // GET: Cursos/AsignarDocente/5
        public ActionResult AsignarDocente(int id)
        {
            ViewBag.CursoId = id;
            return View();
        }

        // GET: Cursos/MisCursos
        public ActionResult MisCursos()
        {
            return View();
        }

        // GET: Cursos/Disponibles
        public ActionResult Disponibles()
        {
            return View();
        }
    }
}
