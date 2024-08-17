using AsteelProjectManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace AsteelProjectManagement.Controllers
{
    public class NotificationsController : Controller
    {
        // GET: Notifications

        private PrjContext db = new PrjContext();

        public ActionResult Notifications()
        {
         
            return View();
        }

        // GET: Notification/MarkAsRead/5
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

            return RedirectToAction("Notifications");
        }

    }
}