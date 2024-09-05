using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Diagnostics;
using System.Web.Mvc;
using AsteelProjectManagement.Models;
using System.Data.Entity.Validation;
using System.Net.Mail;
using System.IO;
using Microsoft.AspNet.Identity;


namespace AsteelProjectManagement.Controllers
{
    public class ProjectsController : Controller
    {
        private PrjContext db = new PrjContext();

        // GET: Projects
        // GET: Projects
        public ActionResult Index()
        {
            // Assume the user is authenticated and their ID is stored in the session
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = (int)Session["UserID"];

            // Retrieve the user's role ID from the UserRoleAssignment table
            var userRole = db.UserRoleAssignments
                             .Where(ura => ura.UserID == userId)
                             .Select(ura => ura.RoleID)
                             .FirstOrDefault();

            // Retrieve projects including related users
            var projects = db.Projects.Include(p => p.Users).Include(p => p.Users1).ToList();

            // Pass user role to the view
            ViewBag.UserRoleId = userRole;

            return View(projects);
        }

        public ActionResult Stepper()
        {
            var viewModel = new StepperViewModel
            {
                Project = new Projects(),
                Link = new Links(),
                Version = new Versions(),
                Attachment = new Attachments(),
                Comment = new Comments()
            };
            return View(viewModel);
        }
        public ActionResult GetProjectManagers()
        {
            var projectManagers = db.Users // Remplacez `Users` par votre modèle de gestionnaires de projet
                .Select(u => new { u.UserID, u.Username }) // Remplacez `UserID` et `UserName` par les propriétés appropriées
                .ToList();

            return Json(projectManagers, JsonRequestBehavior.AllowGet);
        }

        private int GetCurrentUserId()
        {
            // Remplacez cette méthode pour obtenir l'ID de l'utilisateur actuellement connecté
            var userName = User.Identity.Name;
            var user = db.Users.SingleOrDefault(u => u.Username == userName);
            return user != null ? user.UserID : 0; // Assurez-vous que UserID est la clé primaire
        }


        [HttpPost]
        public ActionResult SaveStep1(Projects project)
        {
            // Retrieve the authenticated user's ID
            /* var userIdString = User.Identity.GetUserId(); // Assuming you're using Identity framework

             if (userIdString == null)
             {
                 return Json(new { success = false, message = "User not authenticated" });
             }
             int userId;
             if (!int.TryParse(userIdString, out userId))
             {
                 return Json(new { success = false, message = "Invalid User ID" });
             }*/


        

            if (ModelState.IsValid)
            {


                // Set the UserID for the project
                // Set the UserID for the project
                project.CreatedBy = GetCurrentUserId();
                project.CreatedDate = DateTime.Now;
                project.ProjectManagerID = 1; // Assurez-vous que cet ID existe dans votre base de données
                project.LastModifiedDate = DateTime.Now;
                project.Status = project.Status;
                project.Priority = project.Priority;
                db.Projects.Add(project);
                db.SaveChanges();

                // Return the ProjectID for the next steps
                return Json(new { success = true, projectId = project.ProjectID });
            }

            return Json(new { success = false });
        }


        [HttpPost]
        public ActionResult SaveStep2( Links link, int projectId)
        {
           /* var userIdString = User.Identity.GetUserId(); // Assuming you're using Identity framework

            if (userIdString == null)
            {
                return Json(new { success = false, message = "User not authenticated" });
            }
            int userId;
            if (!int.TryParse(userIdString, out userId))
            {
                return Json(new { success = false, message = "Invalid User ID" });
            }*/
            if (ModelState.IsValid)
            {
                link.ProjectID = projectId; // Link to the project created in Step 1
                link.CreatedBy = GetCurrentUserId();
                link.CreatedDate = DateTime.Now;
                link.Category=link.Category;
                db.Links.Add(link);
                db.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public ActionResult SaveStep3(Versions version, int projectId)
        {
            if (ModelState.IsValid)
            {
                version.CreatedBy = GetCurrentUserId();
                version.ReleaseDate = DateTime.Now;
                version.IsStable = true;
                Random random = new Random();
                version.VersionNumber = random.Next(1, 100).ToString(); // génère un numéro entre 1 et 100
                version.ProjectID = projectId; // Associe le projet créé dans l'étape 1
                version.ModifiedDate= DateTime.Now; 
                db.Versions.Add(version);
                db.SaveChanges();

                return Json(new { success = true });
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors);
            var errorMessages = string.Join(", ", errors.Select(e => e.ErrorMessage));
            return Json(new { success = false, message = errorMessages });
        }

        [HttpPost]
        public ActionResult SaveStep4(Attachments attachment, int projectId)
        {
           
            if (ModelState.IsValid)
            {
                attachment.UploadedBy =  int.Parse(User.Identity.GetUserId());
                ;
                attachment.UploadedDate = DateTime.Now;
                attachment.ProjectID = projectId; // Link to the project created in Step 1
                db.Attachments.Add(attachment);
                db.SaveChanges();

                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public ActionResult SaveStep5(Comments comment, int projectId)
        {
            if (ModelState.IsValid)
            {
                comment.CreatedDate = DateTime.Now;
                comment.UserID = GetCurrentUserId();
                ;
                comment.ProjectID = projectId; // Link to the project created in Step 1
                db.Comments.Add(comment);
                db.SaveChanges();

                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
        [HttpPost]
        public JsonResult FinalizeProcess(int projectId)
        {
            try
            {
                CompleteProject(projectId);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        private void CompleteProject(int projectId)
        {
            // Logique pour marquer le projet comme terminé
            var project = db.Projects.Find(projectId);
            if (project != null)
            {
                db.SaveChanges();
            }
        }



        public ActionResult Details(int id)
        {
            var project = db.Projects.Include(p => p.Links)
                                     .Include(p => p.Attachments)
                                     .Include(p => p.Versions)
                                     .Include(p => p.Comments)
                                     .FirstOrDefault(p => p.ProjectID == id);

            if (project == null)
            {
                return HttpNotFound();
            }

            var createdByUser = db.Users.FirstOrDefault(u => u.UserID == project.CreatedBy);
            var projectManager = db.Users.FirstOrDefault(u => u.UserID == project.ProjectManagerID);

            var projectViewModel = new ProjectViewModel
            {
                Projects = project,
                Links = project.Links.ToList(),
                Attachments = project.Attachments.ToList(),
                Versions = project.Versions.ToList(),
                Comments = project.Comments.ToList(),
                CreatedByUsername = createdByUser != null ? createdByUser.Username : "Unknown",
                 ProjectManagerUsername = projectManager != null ? projectManager.Username : "Unknown",

            };

            return View(projectViewModel);
        }
 


        public ActionResult Comments(int projectId)
        {
            ViewData["ProjectID"] = projectId;

            var model = new Comments { ProjectID = projectId };
            return PartialView("Comments", model);
        }

        public ActionResult AddLink(int projectId)
        {
            System.Diagnostics.Debug.WriteLine("AddLink GET called with projectId: " + projectId);
            ViewData["ProjectID"] = projectId;

            var model = new Links { ProjectID = projectId };
            return PartialView("AddLink", model);
        }

        // POST: Projects/AddLink
       

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Projects projects = db.Projects.Find(id);
            if (projects == null)
            {
                return HttpNotFound();
            }
            bool isModificationValidated = IsModificationRequestValidated(projects.ProjectID);

            if (!isModificationValidated)
            {
                // Redirect to the "ModificationRequest" action if the status is "En attente"
                return Json(new { redirect = Url.Action("Create", "Modifications", new { id = projects.ProjectID }), showModal = false }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { showModal = true, html = RenderPartialViewToString("Delete", projects) }, JsonRequestBehavior.AllowGet);
        }
        private string RenderPartialViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                return sw.GetStringBuilder().ToString();
            }
        }
        public ActionResult LoadDeleteModalContent(int id)
        {
            var project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            return PartialView("Delete", project);
        }

        // POST: Projects/Delete/5
        // POST: Projects/Delete/5
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            // Vérifier si l'ID du projet est valide
            if (id <= 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Trouver le projet
            var project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }

            // Trouver et supprimer toutes les versions associées à ce projet
            var versions = db.Versions.Where(v => v.ProjectID == id).ToList();
            foreach (var version in versions)
            {
                db.Versions.Remove(version);
            }

            // Trouver et supprimer tous les liens associés à ce projet
            var links = db.Links.Where(l => l.ProjectID == id).ToList();
            foreach (var link in links)
            {
                db.Links.Remove(link);
            }
            // Trouver et supprimer toutes les demandes de modification associées à ce projet
            var modificationRequests = db.ModificationRequests.Where(mr => mr.ProjectID == id).ToList();
            foreach (var request in modificationRequests)
            {
                db.ModificationRequests.Remove(request);
            }

            // Trouver et supprimer toutes les pièces jointes associées à ce projet
            var attachments = db.Attachments.Where(a => a.ProjectID == id).ToList();
            foreach (var attachment in attachments)
            {
                db.Attachments.Remove(attachment);
            }

            var tasks = db.Tasks.Where(t => t.ProjectID == id).ToList();
            foreach (var task in tasks)
            {
                db.Tasks.Remove(task);
            }

            // Trouver et supprimer tous les commentaires associés à ce projet
            var comments = db.Comments.Where(c => c.ProjectID == id).ToList();
            foreach (var comment in comments)
            {
                db.Comments.Remove(comment);
            }

            // Supprimer le projet lui-même
            db.Projects.Remove(project);

            // Enregistrer tous les changements dans la base de données
            db.SaveChanges();

            // Rediriger vers l'action Index
            return RedirectToAction("Index");
        }


        [HttpPost]

        public ActionResult AddAttachment(int projectId)
        {
        
            Debug.WriteLine("AddAttachment GET called with projectId: " + projectId);

            var model = new Attachments { ProjectID = projectId };

            ViewBag.UploadedBy = new SelectList(db.Users, "UserID", "Username");
            Debug.WriteLine("Entrée dans la méthode Index");


            return PartialView("AddAttachment", model); // Utilisez le nom de la vue partielle correct
        }

        public JsonResult CheckModificationRequestStatus(int id)
        {
            // Check if the most recent modification request is validated
            bool isModificationValidated = IsModificationRequestValidated(id);

            // Return the result as JSON
            return Json(isModificationValidated, JsonRequestBehavior.AllowGet);
        }
      
       /* [HttpPost]
        public ActionResult CreateEdit(Projects project)
        {
            if (ModelState.IsValid)
            {
                if (project.ProjectID == 0)
                {
                    project.CreatedDate = DateTime.Now;
                    db.Projects.Add(project);
                }
                else
                {
                    var existingProject = db.Projects.Find(project.ProjectID);
                    if (existingProject == null)
                    {
                        return HttpNotFound();
                    }

                    existingProject.ProjectName = project.ProjectName;
                    existingProject.Description = project.Description;
                    existingProject.LastModifiedDate = DateTime.Now;
                    existingProject.Status = project.Status;
                    existingProject.Priority = project.Priority;

                    db.Entry(existingProject).State = System.Data.Entity.EntityState.Modified;
                }

                db.SaveChanges();
                return Json(new { success = true, projectId = project.ProjectID });
            }

            return PartialView("~/Views/Projects/CreateEdit.cshtml", project);
        }*/

        public ActionResult Edit(int id)
         {
             var project = db.Projects.Find(id);

             if (project == null)
             {
                 return HttpNotFound();
             }



             bool isModificationValidated = IsModificationRequestValidated(project.ProjectID);

             if (!isModificationValidated)
             {
                 // Redirect to the "ModificationRequest" action if the status is "En attente"
                 return RedirectToAction("Create", "Modifications", new { id = project.ProjectID });
             }
             var link = db.Links.FirstOrDefault(l => l.ProjectID == id);
             var version = db.Versions.FirstOrDefault(v => v.ProjectID == id);
             var attachment = db.Attachments.FirstOrDefault(a => a.ProjectID == id);
             var comment = db.Comments.FirstOrDefault(c => c.ProjectID == id);

             var viewModel = new StepperViewModel
             {
                 Project = project ,
                 Link = link ?? new Links(), // Initialize with a new instance if null
                 Version = version ?? new Versions(), // Initialize with a new instance if null
                 Attachment = attachment ?? new Attachments(), // Initialize with a new instance if null
                 Comment = comment ?? new Comments(), // Initialize with a new instance if null
             };

             ViewBag.ProjectID = project.ProjectID;

             return View
                (viewModel); // Assurez-vous que le nom de la vue est correct
            }
        [HttpPost]
        public JsonResult CreateEdit(Projects model)
        {
            // Vérifiez si le modèle est valide
            if (!ModelState.IsValid)
            {
                // Obtenez les messages d'erreur de ModelState
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();

                return Json(new { success = false, message = string.Join("; ", errors) });
            }

            // Vérifiez si model.Project est nul
            if (model == null)
            {
                return Json(new { success = false, message = "Les détails du projet sont manquants" });
            }

            Projects project;

            // Si le ProjectID existe, effectuer une mise à jour du projet
            project = db.Projects.Find(model.ProjectID);

            if (project != null)
            {
                project.ProjectName = model.ProjectName;
                project.Description = model.Description;
                project.Status = model.Status;
                project.Priority = model.Priority;
                project.LastModifiedDate = DateTime.Now; // Mettre à jour la date de modification

                db.SaveChanges(); // Enregistrer les modifications

                return Json(new { success = true, projectId = project.ProjectID, message = "Project updated successfully" });
            }
            else
            {
                return Json(new { success = false, message = "Project not found" });
            }
        }





        [HttpPost]
        public ActionResult UpdateStep2(Links link, int projectId)
         {
             if (ModelState.IsValid)
             {
                 var existingLink = db.Links.FirstOrDefault(l => l.ProjectID == projectId);
                 if (existingLink != null)
                 {
                     existingLink.URL = link.URL;
                     existingLink.Category = link.Category;
                     existingLink.Description = link.Description;

                     db.Entry(existingLink).State = System.Data.Entity.EntityState.Modified;
                     db.SaveChanges();
                     return Json(new { success = true });
                 }
                 return Json(new { success = false, message = "Link not found" });
             }

             return Json(new { success = false, message = "Model validation failed." });
         }

         [HttpPost]
         public ActionResult UpdateStep3(Versions version, int projectId)
         {
             if (ModelState.IsValid)
             {
                 var existingVersion = db.Versions.FirstOrDefault(v => v.ProjectID == projectId);
                 if (existingVersion != null)
                 {
                     existingVersion.SourcePath = version.SourcePath;
                     existingVersion.DestinationPath=version.DestinationPath;
                     existingVersion.Notes= version.Notes;
                     existingVersion.ModifiedDate = DateTime.Now;
                    existingVersion.ModifiedBy = int.Parse(User.Identity.GetUserId());

                    db.Entry(existingVersion).State = System.Data.Entity.EntityState.Modified;
                     db.SaveChanges();
                     return Json(new { success = true });
                 }
                 return Json(new { success = false, message = "Version not found" });
             }

             return Json(new { success = false, message = "Model validation failed." });
         }

         [HttpPost]
         public ActionResult UpdateStep4(Attachments attachment, int projectId)
         {
             if (ModelState.IsValid)
             {
                 var existingAttachment = db.Attachments.FirstOrDefault(a => a.ProjectID == projectId);
                 if (existingAttachment != null)
                 {
                     existingAttachment.FilePath = attachment.FilePath;
                     existingAttachment.Description = attachment.Description;
                     db.Entry(existingAttachment).State = System.Data.Entity.EntityState.Modified;
                     db.SaveChanges();
                     return Json(new { success = true });
                 }
                 return Json(new { success = false, message = "Attachment not found" });
             }

             return Json(new { success = false, message = "Model validation failed." });
         }

         [HttpPost]
         public ActionResult UpdateStep5(Comments comment, int projectId)
         {
             if (ModelState.IsValid)
             {
                 var existingComment = db.Comments.FirstOrDefault(c => c.ProjectID == projectId);
                 if (existingComment != null)
                 {
                     existingComment.Content = comment.Content;

                     db.Entry(existingComment).State = System.Data.Entity.EntityState.Modified;
                     db.SaveChanges();
                     return Json(new { success = true });
                 }
                 return Json(new { success = false, message = "Comment not found" });
             }

             return Json(new { success = false, message = "Model validation failed." });
         }

         [HttpPost]
         public JsonResult FinalizeUpdateProcess(int projectId)
         {
             try
             {
                 var project = db.Projects.Find(projectId);
                 if (project != null)
                 {
                     // Logique pour finaliser la modification du projet
                     db.SaveChanges();
                     return Json(new { success = true });
                 }
                 return Json(new { success = false, message = "Project not found" });
             }
             catch (Exception ex)
             {
                 return Json(new { success = false, message = ex.Message });
             }
         }


         


        public JsonResult GetProjectData(int id)
        {
            var projectData = db.Projects
                .Where(p => p.ProjectID == id)
                .Select(p => new
                {
                    Project = new
                    {
                        p.ProjectID,
                        p.ProjectName,
                        p.Description,
                        // Excluez les propriétés que vous ne souhaitez pas inclure
                    },
                    Links = p.Links.Select(l => new
                    {
                        l.LinkID,
                        l.ProjectID
                        // Excluez `l.SomeProperty` si vous ne voulez pas la retourner
                    }).ToList(),
                    Versions = p.Versions.Select(v => new
                    {
                        v.VersionID,
                        v.ProjectID
                        // Excluez `v.SomeProperty` si vous ne voulez pas la retourner
                    }).ToList(),
                    Attachments = p.Attachments.Select(a => new
                    {
                        a.AttachmentID,
                        a.ProjectID
                        // Excluez `a.SomeProperty` si vous ne voulez pas la retourner
                    }).ToList(),
                    Comments = p.Comments.Select(c => new
                    {
                        c.CommentID,
                        c.ProjectID
                    }).ToList()
                })
                .FirstOrDefault();

            return Json(projectData, JsonRequestBehavior.AllowGet);
        }
        

        [HttpPost]
        public JsonResult Finish(StepperViewModel model)
        {
            try
            {
                // You can add any finalization logic here if needed
                return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
       

        public bool IsModificationRequestValidated(int projectId)
        {
            // Récupérer toutes les demandes de modification associées au projet
            var requests = db.ModificationRequests
                            .Where(mr => mr.ProjectID == projectId)
                            .ToList();

            // Trouver la demande de modification validée la plus récente
            var validatedRequest = requests
                .Where(mr => mr.Status == "Accepted")
                .OrderByDescending(mr => mr.RequestedDate) // Assuming you want the most recent validated request
                .FirstOrDefault();

            // Vérifier si une demande validée existe
            return validatedRequest != null;
        }











        [HttpPost]

        [ValidateAntiForgeryToken]

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
