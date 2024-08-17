using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using AsteelProjectManagement.Models;

namespace AsteelProjectManagement.Controllers
{
    public class AccountController : Controller
    {
        private PrjContext db = new PrjContext();

        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = db.Users.FirstOrDefault(u => u.Username == model.Username && (bool)u.IsActive);

                if (user != null && VerifyPassword(model.Password, user.PasswordHash))
                {
                    FormsAuthentication.SetAuthCookie(user.Username, model.RememberMe);
                    Session["UserID"] = user.UserID; // Définir l'ID utilisateur dans la session
                    Debug.WriteLine($"UserID {user.UserID} set in session.");
                    return RedirectToLocal(returnUrl);
                }
                else
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                }
            }
            Debug.WriteLine(model);
            return View(model);
        }


        [HttpGet]
        
        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Account");
        }

        private bool VerifyPassword(string enteredPassword, string storedHash)
        {
            // Implémentez votre logique de vérification de mot de passe ici
            return enteredPassword == storedHash; // Remplacez cela par une vérification réelle des hachages de mot de passe
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Projects");
        }
    }
}
