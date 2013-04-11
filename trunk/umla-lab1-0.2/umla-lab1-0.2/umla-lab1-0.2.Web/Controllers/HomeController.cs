namespace umla_lab1_0._2.Web.Controllers
{
    using System.Web.Mvc;

    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            this.ViewBag.Message = "Home";

            return this.View();
        }
    }
}
