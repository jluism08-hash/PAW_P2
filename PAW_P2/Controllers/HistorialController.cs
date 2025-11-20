using System.Web.Mvc;

namespace PAW_P2.Controllers
{
    // Controlador MVC para historial académico
    [Authorize]
    public class HistorialController : Controller
    {
        // GET: Historial
        public ActionResult Index()
        {
            return View();
        }

        // GET: Historial/Estudiante/5
        public ActionResult Estudiante(int id)
        {
            ViewBag.EstudianteId = id;
            return View();
        }

        // GET: Historial/MiHistorial (para estudiantes)
        public ActionResult MiHistorial()
        {
            if (Session["UsuarioId"] != null)
            {
                ViewBag.EstudianteId = Session["UsuarioId"];
            }
            return View("Estudiante");
        }

        // GET: Historial/RegistrarCalificacion
        public ActionResult RegistrarCalificacion(int cursoId, int estudianteId)
        {
            ViewBag.CursoId = cursoId;
            ViewBag.EstudianteId = estudianteId;
            return View();
        }
    }
}
