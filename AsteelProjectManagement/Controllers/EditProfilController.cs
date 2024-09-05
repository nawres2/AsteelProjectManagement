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
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = (int)Session["UserID"];
            Users user = _context.Users.Find(userId);

            if (user == null)
            {
                return HttpNotFound();
            }

            var model = new EditProfileViewModel
            {
                UserID = user.UserID,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role,
                DateJoined = user.DateJoined,
                PasswordHash = user.PasswordHash
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProfil(EditProfileViewModel updatedUser)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = (int)Session["UserID"];
            Users user = _context.Users.Find(userId);

            if (user != null && ModelState.IsValid)
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

                _context.SaveChanges();
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
