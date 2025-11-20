using System.Web.Mvc;

namespace PAW_P2.Controllers
{
    // Controlador MVC para consulta de bitácora
    [Authorize]
    public class BitacoraController : Controller
    {
        // GET: Bitacora
        public ActionResult Index()
        {
            return View();
        }

        // GET: Bitacora/Detalle/5
        public ActionResult Detalle(int id)
        {
            ViewBag.BitacoraId = id;
            return View();
        }
    }
}
