using System.Linq;
using System.Web.Mvc;
using AsteelProjectManagement.Models;
using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;

namespace AsteelProjectManagement.Controllers
{
    public class EditProfilController : Controller
    {
        private PrjContext _context = new PrjContext();

        [HttpGet]
        public ActionResult EditProfil()
        {
            // Vérifier si l'utilisateur est connecté
            if (Session["UserID"] == null)
            {
                Debug.WriteLine("Session UserID is null. Redirecting to login.");
                return RedirectToAction("Login", "Account");
            }

            int userId = (int)Session["UserID"];
            Users user = _context.Users.Find(userId);

            if (user == null)
            {
                Debug.WriteLine($"User with ID {userId} not found.");
                return HttpNotFound();
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProfil(Users updatedUser)
        {
            // Vérifier si l'utilisateur est connecté
            if (Session["UserID"] == null)
            {
                Debug.WriteLine("Session UserID is null. Redirecting to login.");
                return RedirectToAction("Login", "Account");
            }

            int userId = (int)Session["UserID"];
            Users user = _context.Users.Find(userId);

            if (user != null)
            {
                if (ModelState.IsValid)
                {
                    user.Username = updatedUser.Username;
                    user.FirstName = updatedUser.FirstName;
                    user.LastName = updatedUser.LastName;
                    user.Email = updatedUser.Email;
                    user.Role = updatedUser.Role;
                    user.DateJoined = updatedUser.DateJoined;

                    if (!string.IsNullOrEmpty(updatedUser.PasswordHash))
                    {
                        user.PasswordHash = HashPassword(updatedUser.PasswordHash);
                    }

                        return RedirectToAction("Profil", "Profil");
                    

                   
                }
            }
            else
            {
                Debug.WriteLine($"User with ID {userId} not found.");
            }
            return View(updatedUser);
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
