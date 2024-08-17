using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Web.Mvc;
using AsteelProjectManagement.Models;

namespace AsteelProjectManagement.Controllers
{
    public class HomeController : Controller
    {
        private PrjContext db = new PrjContext();


        // GET: Home/Contact
        public ActionResult Contact()
        {
            // Obtenez le nom d'utilisateur actuel
            var userName = User.Identity.Name;

            // Récupérez l'utilisateur en utilisant le nom d'utilisateur
            var user = db.Users.SingleOrDefault(u => u.Username == userName);
            var userId = user?.UserID;

            // Vérifiez si l'utilisateur a le rôle de chef de projet (RoleID 2)
            bool isProjectManager = db.UserRoleAssignments.Any(ura => ura.UserID == userId && ura.RoleID == 2);

            // Récupérez toutes les notifications
            var notifications = db.Notifications.ToList();

            if (isProjectManager)
            {
                // Filtrer les notifications de type "demande de modification" pour les chefs de projet
                notifications = notifications.Where(n => n.Type == "Request Modification").ToList();
            }
            else
            {
                // Les utilisateurs non-chefs de projet ne voient aucune notification de ce type
                notifications = notifications.Where(n => n.Type != "Request Modification").ToList();
            }

            return View(notifications);
        }





        public ActionResult MarkAsRead(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Notifications notification = db.Notifications.Find(id);
            if (notification == null)
            {
                return HttpNotFound();
            }

            notification.IsRead = true;
            db.Entry(notification).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Contact");
        }
        public ActionResult About () {
            return View();
        }
    }
}
