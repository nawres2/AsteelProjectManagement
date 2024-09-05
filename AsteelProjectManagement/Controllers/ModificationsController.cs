using AsteelProjectManagement.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace AsteelProjectManagement.Controllers
{
   

    public class ModificationsController : Controller
    {
         private PrjContext db = new PrjContext();
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult GetVersionsByProject(int projectId)
        {
            var versions = db.Versions
                             .Where(v => v.ProjectID == projectId)
                             .Select(v => new { v.VersionID, v.VersionNumber }) // Ensure these properties exist and are correctly spelled
                             .ToList();
            return Json(versions, JsonRequestBehavior.AllowGet);
        }
        // GET: ModificationRequest/Create
        public ActionResult Create()
        {
            var projects = db.Projects.Select(p => new { p.ProjectID, p.ProjectName }).ToList();
            var versions = db.Versions.Select(v => new { v.VersionID, v.VersionNumber }).ToList();
            var users = db.Users.Select(u => new { u.UserID, u.Username }).ToList();
         
);

            ViewBag.ProjectID = new SelectList(projects, "ProjectID", "ProjectName");
            ViewBag.VersionID = new SelectList(versions, "VersionID", "VersionName");
            ViewBag.RequesterID = new SelectList(users, "UserID", "UserName");

            return View();
        }

        // POST: ModificationRequest/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProjectID,VersionID,Description")] ModificationRequests modificationRequest)
        {
            if (ModelState.IsValid)
            {
       
   

                var userName = User.Identity.Name;
                var user = db.Users.SingleOrDefault(u => u.Username == userName);
                if (user == null)
                {
                    // Si l'utilisateur n'est pas trouvé, gérer l'erreur
                    return RedirectToAction("Error", "Home");
                }
                var userId = user.UserID;

                // Assigner l'ID de l'utilisateur comme RequesterID
                modificationRequest.RequesterID = userId;
                modificationRequest.RequestedDate = DateTime.Now;
                modificationRequest.Status = "En attente";

                db.ModificationRequests.Add(modificationRequest);
                db.SaveChanges();

                // Créer et envoyer une notification au Chef de Projet et stocker les NotificationIDs
                var notificationIds = SendNotificationToProjectManager(modificationRequest);

                // Stocker les NotificationIDs dans TempData pour une utilisation ultérieure
                TempData["NotificationIds"] = notificationIds;

                return RedirectToAction("Index", "Projects");
            }

            return View(modificationRequest);
        }

        private string GetRequesterName(int requesterID)
        {
            var requester = db.Users.FirstOrDefault(u => u.UserID == requesterID);
            return requester != null ? requester.Username : "Unknown Requester";
        }
        private string GetProjectName(int projectID)
        {
            var project = db.Projects.FirstOrDefault(p => p.ProjectID == projectID);
            return project != null ? project.ProjectName : "Unknown Project";
        }



        private List<int> SendNotificationToProjectManager(ModificationRequests modificationRequest)
        {
            var notificationIds = new List<int>();
            var projectManagers = db.Users.Where(u => u.UserRoleAssignments.Any(r => r.RoleID == 2)).ToList();
            var requesterName = GetRequesterName(modificationRequest.RequesterID); // Obtenez le nom du demandeur
            var projectName = GetProjectName(modificationRequest.ProjectID); // Obtenez le nom du projet

            foreach (var manager in projectManagers)
            {
                Notifications notification = new Notifications
                {
                    UserID = manager.UserID,
                    Message = $"{requesterName} has made a request for the project: {projectName}",
                    IsRead = false,
                    CreatedDate = DateTime.Now,
                    Type = "Request Modification"
                };
                db.Notifications.Add(notification);
                db.SaveChanges(); // Sauvegarde pour obtenir l'ID

                notificationIds.Add(notification.NotificationID); // Stocke l'ID de la notification
            }

            return notificationIds;
        }



        // Other actions...
        [HttpPost]
        public ActionResult AcceptRequest(int requestID, string modificationNotes)
        {
            var request = GetModificationRequestById(requestID);
            if (request != null)
            {
                UpdateRequestStatus(request, "Accepted", modificationNotes);
                SendNotificationToRequester(request, "Accepted", modificationNotes);
                DeleteNotificationForModificationRequest(requestID); // Optionnel si vous voulez supprimer la notification initiale
            }

            return RedirectToAction("Validate");
        }

        // Action pour rejeter une demande de modification
        [HttpPost]
        public ActionResult RejectRequest(int requestID, string modificationNotes)
        {
            var request = GetModificationRequestById(requestID);
            if (request != null)
            {
                UpdateRequestStatus(request, "Rejected", modificationNotes);
                SendNotificationToRequester(request, "Rejected", modificationNotes);
                DeleteNotificationForModificationRequest(requestID); // Optionnel si vous voulez supprimer la notification initiale
            }

            return RedirectToAction("Validate");
        }
        private ModificationRequests GetModificationRequestById(int requestID)
        {
            return db.ModificationRequests
                     .SingleOrDefault(mr => mr.RequestID == requestID);
        }
        private void UpdateRequestStatus(ModificationRequests request, string status, string modificationNotes)
        {

            request.ReviewedBy = GetUserId();
            request.ReviewDate = DateTime.Now;
            request.Status = status;
            request.ModificationNotes = modificationNotes;

            db.SaveChanges();
        

        }


        public ActionResult Validate()
        {
            // Récupère les demandes de modification qui ne sont pas encore validées
            var requests = db.ModificationRequests
                    .Where(mr => mr.Status == "En attente ")
                    .Include(mr => mr.Projects) // Inclut le projet associé si nécessaire
                    .Include(mr => mr.Users) // Inclut l'utilisateur demandeur si nécessaire
                    .ToList();
            System.Diagnostics.Debug.WriteLine("Number of requests: " + requests.Count);

            return View(requests);
        }

        // Action pour valider ou rejeter une demande de modification
        [HttpPost]
        public ActionResult Validate(int requestID, string status, string modificationNotes, int id)
        {
            // Récupère la demande de modification à valider/rejeter

            var request = db.ModificationRequests.SingleOrDefault(mr => mr.RequestID == requestID);
            var notification = db.Notifications.FirstOrDefault(n => n.NotificationID == id);

            var modificationRequest = db.ModificationRequests.Find(id);

            if (request != null)
            {
                // Met à jour les champs de la demande de modification
                request.ReviewedBy = GetUserId(); // Récupère l'ID de l'utilisateur connecté
                request.ReviewDate = DateTime.Now;
                request.Status = status == "Accepted" ? "Accpted" : "Rejected";
                request.ModificationNotes = modificationNotes;
                notification.IsRead = true;


                // Sauvegarde les modifications dans la base de données
                db.SaveChanges();
            }
            DeleteNotificationForModificationRequest(requestID);
          

            return RedirectToAction("Validate");
        }
        private int GetUserId()
        {
            var user = db.Users.SingleOrDefault(u => u.Username == User.Identity.Name);
            return user.UserID; // Assurez-vous que l'ID est de type int

        }


        private void DeleteNotificationForModificationRequest(int requestID)
        {
            var notificationIds = TempData["NotificationIds"] as List<int>;

            if (notificationIds != null && notificationIds.Any())
            {
                var notifications = db.Notifications.Where(n => notificationIds.Contains(n.NotificationID)).ToList();
                if (notifications.Any())
                {
                    db.Notifications.RemoveRange(notifications);
                    db.SaveChanges();
                }
            }
        }
        private void SendNotificationToRequester(ModificationRequests modificationRequest, string status, string modificationNotes)
        {
            // Trouver le demandeur
            var projectName = GetProjectName(modificationRequest.ProjectID); // Obtenez le nom du projet

            var requester = db.Users.SingleOrDefault(u => u.UserID == modificationRequest.RequesterID);

            if (requester != null)
            {
                Notifications notification = new Notifications
                {
                    UserID = requester.UserID,
                    Message = $"Your modification request for: {projectName} has been {status}. Modification notes: {modificationNotes}",
                    IsRead = false,
                    CreatedDate = DateTime.Now,
                    Type = "Request modification"
                };
                db.Notifications.Add(notification);
                db.SaveChanges();
            }
        }
     


    }

}
