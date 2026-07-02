using Microsoft.AspNetCore.Mvc;

namespace LisAeroGest.Controllers
{
    public class DashboardController : Controller
    {
        /// <summary>
        /// Redireciona para o dashboard correto consoante a role do utilizador autenticado.
        /// </summary>
        /// <returns>
        /// <see cref="IActionResult"/> com a view do dashboard
        /// correspondente à role do utilizador autenticado.
        /// </returns>
        [HttpGet]
        public IActionResult Index()
        {
            if (User.IsInRole("Admin"))
                return View("AdminDashboard");

            if (User.IsInRole("Employee"))
                return View("EmployeeDashboard");

            return RedirectToAction("Index", "Home");
        }
    }
}
