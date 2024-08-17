using AsteelProjectManagement.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AsteelProjectManagement.Controllers
{

   

    public class ModificationController : Controller
    {
         private PrjContext db = new PrjContext();
        // GET: ModificationRequest/Create



        public ActionResult CreateModification()
        {
            string userId = GetUserId(); // Call the GetUserId() method here
            int developerRoleId = 1;
            var projects = db.Projects.Select(p => new { p.ProjectID, p.ProjectName }).ToList();
            var versions = db.Versions.Select(v => new { v.VersionID, v.VersionNumber }).ToList();
            var users = db.Users.Select(u => new { u.UserID, u.Username }).ToList();

            ViewBag.ProjectID = new SelectList(projects, "ProjectID", "ProjectName");
            ViewBag.VersionID = new SelectList(versions, "VersionID", "VersionName");
            ViewBag.RequesterID = new SelectList(users, "UserID", "UserName");
            ViewBag.ReviewedBy = new SelectList(users, "UserID", "UserName");
            int userIdInt;
            if (int.TryParse(userId, out userIdInt))
            {
                bool isDeveloper = db.UserRoleAssignments
                                     .Any(ura => ura.UserID == userIdInt && ura.RoleID == developerRoleId);

                ViewBag.IsDeveloper = isDeveloper;
                System.Diagnostics.Debug.WriteLine($"Is Developer: {isDeveloper}");

            }
            else
            {
                ViewBag.IsDeveloper = false;
            }

            return View();
        }
        public JsonResult GetVersionsByProject(int projectId)
        {
            var versions = db.Versions
                             .Where(v => v.ProjectID == projectId)
                             .Select(v => new { v.VersionID, v.VersionNumber })
                             .ToList();
            return Json(versions, JsonRequestBehavior.AllowGet);
        }

        private string GetUserId()
        {
            // Assuming user ID is stored in an authentication cookie or session
            // Adjust this method according to your authentication system
            return HttpContext.User.Identity.Name; // For Forms Authentication
                                                   // Or, if using Session:
                                                   // return Session["UserId"] as string;
        }
        // POST: ModificationRequest/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateModification([Bind(Include = "ProjectID,VersionID,Description,RequesterID,ReviewedBy")] ModificationRequests modificationRequest)
        {
            if (ModelState.IsValid)
            {
                modificationRequest.Status = "En attente";
                db.ModificationRequests.Add(modificationRequest);
                db.SaveChanges();
                SendNotificationToProjectManager(modificationRequest);

                return RedirectToAction("Index", "Projects"); // Redirect to the project list or details
            }


            return View(modificationRequest);
        }
        private void SendNotificationToProjectManager(ModificationRequests modificationRequest)
        {
            // Find the Project Manager(s)
            var projectManagers = db.Users.Where(u => u.UserRoleAssignments.Any(r => r.RoleID == 2)).ToList(); // Assuming RoleID 2 is for Project Manager

            foreach (var manager in projectManagers)
            {
                Notifications notification = new Notifications
                {
                    UserID = manager.UserID,
                    Message = $"Une nouvelle demande de modification a été soumise pour le projet ID: {modificationRequest.ProjectID}. Cliquez <a href='/Modification/ReviewModification/{modificationRequest.ModificationRequestID}'>ici</a> pour examiner.",
                    IsRead = false,
                    CreatedDate = DateTime.Now,
                    Type = "Demande de modification"
                };

                db.Notifications.Add(notification);
            }

            db.SaveChanges();
        }
      
        public ActionResult ReviewModification(int id)
        {
            var modificationRequest = db.ModificationRequests.Find(id);
            if (modificationRequest == null)
            {
                return HttpNotFound();
            }

            return View(modificationRequest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ReviewModification(int id, string status)
        {
            var modificationRequest = db.ModificationRequests.Find(id);
            if (modificationRequest == null)
            {
                return HttpNotFound();
            }

            modificationRequest.Status = status;
            db.Entry(modificationRequest).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Index", "Projects"); // Redirect to the project list or details
        }

    }
}