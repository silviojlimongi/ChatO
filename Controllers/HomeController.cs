using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ChatO.Controllers                 
{
    public class HomeController : Controller // Controlador MVC básico (hereda de Controller)
    {
        public ActionResult Index()          // Acción GET para la página principal (Home/Index)
        {
            return View();                   // Renderiza la vista ~/Views/Home/Index.cshtml
        }

        public ActionResult About()          // Acción GET para la página "About"
        {
            ViewBag.Message = "Descripción de la Pagina."; // Pasa un texto a la vista vía ViewBag

            return View();                   // Renderiza ~/Views/Home/About.cshtml
        }

        public ActionResult Contact()        // Acción GET para la página "Contact"
        {
            ViewBag.Message = "Pagina de Contactos."; // Mensaje que la vista mostrará (About/Contact usan h3 en el template)

            return View();                   // Renderiza ~/Views/Home/Contact.cshtml
        }
    }
}
