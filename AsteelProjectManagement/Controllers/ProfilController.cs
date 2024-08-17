using AsteelProjectManagement.Models;
using System.Linq;
using System.Web.Mvc;

namespace AsteelProjectManagement.Controllers
{
    // [Authorize]
    public class ProfilController : Controller
    {
        private PrjContext _context = new PrjContext();

        // GET: /User/Profile
        public ActionResult Profil()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            string username = User.Identity.Name;

            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _context.Users.SingleOrDefault(u => u.Username == username);
            if (user == null)
            {
                return HttpNotFound();
            }

            return View(user);
        }
    }
}
