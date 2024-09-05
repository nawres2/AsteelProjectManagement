using AsteelProjectManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using System.Data.Entity;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNet.Identity;

namespace AsteelProjectManagement.Controllers
{
    public class TasksController : Controller
    {
        // GET: Tasks
        private PrjContext db = new PrjContext();

        public ActionResult Tasks()
        {
            // Assurez-vous que l'utilisateur est authentifié
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = (int)Session["UserID"];

            // Récupération du rôle de l'utilisateur depuis la table UserRoleAssignments
            var userRole = db.UserRoleAssignments
                             .Where(ura => ura.UserID == userId)
                             .Select(ura => ura.RoleID)
                             .FirstOrDefault();

            // Récupération des tâches et des utilisateurs associés
            var tasks = db.Tasks.Include(t => t.Projects).Include(t => t.Users).ToList();
            var users = db.Users.ToList(); // Assurez-vous que la liste des utilisateurs est récupérée

            // Passer les données à la vue
            ViewBag.UserRoleId = userRole;
            ViewBag.Users = users;

            return View(tasks);
        }



        public ActionResult CreateTask()
        {
            var model = new Tasks();
            ViewBag.ProjectList = db.Projects.Select(p => new SelectListItem
            {
                Value = p.ProjectID.ToString(),
                Text = p.ProjectName
            }).ToList();
            model.CreatedDate = DateTime.Now;
            var statusList = new SelectList(new[] {
        new { Value = "Not Started", Text = "Not Started" },
        new { Value = "In Progress", Text = "In Progress" },
        new { Value = "Completed", Text = "Completed" }
    }, "Value", "Text");
            var priorityList = new SelectList(new[] {
        new { Value = "Low", Text = "Low" },
        new { Value = "Meduim", Text = "Meduim" },
        new { Value = "High", Text = "High" }
    }, "Value", "Text");
            ViewBag.StatusList = statusList;
            ViewBag.PriorityList = priorityList;


            return PartialView("CreateTask", model);
        }

        // POST: Tasks/CreateTask
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateTask(Tasks model)
        {
            if (ModelState.IsValid)
            {
                model.CreatedDate = DateTime.Now; // Ensure the creation date is set

                db.Tasks.Add(model);
                db.SaveChanges();

                return Json(new { success = true, redirectUrl = Url.Action("Tasks", "Tasks") }); // Return success response with redirect URL

            }

            // Return validation errors
            var errorMessages = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return Json(new { success = false, errorMessage = "Validation failed.", errors = errorMessages });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int taskId)
        {
            // Récupérer la tâche à supprimer en utilisant taskId
            var task = db.Tasks.Find(taskId);
            if (task != null)
            {
                db.Tasks.Remove(task);
                db.SaveChanges();
            }

            return RedirectToAction("Tasks"); // Rediriger vers une page après suppression

        }

        public ActionResult EditTask(int id)
        {
            var task = db.Tasks.Find(id);
            {
                // Fetch projects from the database


                // Convert the projects to a SelectList


                // Set up SelectList for Status and Priority
                var statusList = new SelectList(new[] {
        new { Value = "Not Started", Text = "Not Started" },
        new { Value = "In Progress", Text = "In Progress" },
        new { Value = "Completed", Text = "Completed" }
    }, "Value", "Text");
                var priorityList = new SelectList(new[] {
        new { Value = "Low", Text = "Low" },
        new { Value = "Meduim", Text = "Meduim" },
        new { Value = "High", Text = "High" }
    }, "Value", "Text");

                ViewBag.StatusList = statusList;
                ViewBag.PriorityList = priorityList;

                return PartialView("EditTask", task);
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditTask(Tasks model)
        {
            if (ModelState.IsValid)
            {
                var task = db.Tasks.Find(model.TaskID);
                if (task != null)
                {
                    task.TaskName = model.TaskName;
                    task.Description = model.Description;
                    task.Status = model.Status;
                    task.DueDate = model.DueDate;
                    task.Priority = model.Priority;
                    task.CompletedDate = model.CompletedDate;
                    task.AssignedTo = model.AssignedTo;

                    db.Entry(task).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Tasks"); // Rediriger vers une page après suppression


                    
                }
                else
                {
                    return Json(new { success = false, errorMessage = "Task not found." });
                }
            }
            else
            {
                var errorMessages = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, errorMessage = "Validation failed.", errors = errorMessages });
            }
        }
        [CustomAuthorize(2)] // Vérifie si le rôle a RoleID = 2 (Chef de Projet)

        public ActionResult Assign()
        {
            // Obtenir l'ID de l'utilisateur actuellement connecté
            int userId = GetCurrentUserId();

            // Vérifiez si l'utilisateur est un Chef de Projet
            bool isProjectManager = db.UserRoleAssignments
                .Any(ra => ra.UserID == userId && ra.RoleID == 2); // RoleID 2 pour Chef de Projet

            if (!isProjectManager)
            {
                // Optionnel: Rediriger ou afficher un message si l'utilisateur n'est pas un Chef de Projet
                return RedirectToAction("Index", "Home");
            }

            // Récupérer les tâches
            var tasks = db.Tasks.ToList();

            // Récupérer les utilisateurs qui ont le rôle de développeur (RoleID 1)
            var developerRoleId = 1;
            var users = db.UserRoleAssignments
                           .Where(ura => ura.RoleID == developerRoleId)
                           .Select(ura => ura.Users)
                           .ToList();

            // Préparer les listes pour les vues
            ViewBag.Tasks = new SelectList(tasks, "TaskID", "TaskName");
            ViewBag.Users = new SelectList(users, "UserID", "UserName");

            return View();
        }


        // POST: Tasks/Assign
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Assign(int taskId, int userId)
        {
            // Obtenir l'ID de l'utilisateur actuellement connecté
            int currentUserId = GetCurrentUserId();

            // Vérifiez si l'utilisateur est un Chef de Projet
            bool isProjectManager = db.UserRoleAssignments
                .Any(ra => ra.UserID == currentUserId && ra.RoleID == 2); // RoleID 2 pour Chef de Projet

            if (!isProjectManager)
            {
                return RedirectToAction("Tasks", "Tasks"); // Redirigez vers la page d'accès refusé
            }

            var task = db.Tasks.Find(taskId);
            if (task != null)
            {
                // Assigner la tâche à l'utilisateur
                task.AssignedTo = userId;
                db.SaveChanges();

                // Créer une notification pour l'utilisateur assigné
                var notification = new Notifications
                {
                    UserID = userId,
                    Message = $"You are assigned to the task '{task.TaskName}'.",
                    IsRead = false,
                    CreatedDate = DateTime.Now,
                    Type = "Assignment"
                };

                db.Notifications.Add(notification);
                db.SaveChanges();

                return RedirectToAction("Tasks");
            }

            // Recharger les données pour l'affichage
            ViewBag.Tasks = new SelectList(db.Tasks.ToList(), "TaskID", "TaskName");
            ViewBag.Users = new SelectList(db.Users.ToList(), "UserID", "UserName");

            return View();
        }


        private int GetCurrentUserId()
        {
            // Remplacez cette méthode pour obtenir l'ID de l'utilisateur actuellement connecté
            var userName = User.Identity.Name;
            var user = db.Users.SingleOrDefault(u => u.Username == userName);
            return user != null ? user.UserID : 0; // Assurez-vous que UserID est la clé primaire
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }







   






        public ActionResult AccessDenied()
        {
            return View(); // Retourne la vue AccessDenied.cshtml
        }
    }
}