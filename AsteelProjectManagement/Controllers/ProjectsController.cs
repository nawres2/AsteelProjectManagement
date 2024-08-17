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


namespace AsteelProjectManagement.Controllers
{
    public class ProjectsController : Controller
    {
        private PrjContext db = new PrjContext();

        // GET: Projects
        public ActionResult Index()
        {
            var projects = db.Projects.Include(p => p.Users).Include(p => p.Users1);
            return View(projects.ToList());
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

        // POST: Projects/Steppersave
        [HttpPost]
        public async Task<ActionResult> Steppersave(StepperViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Validation failed." });
            }

            // Get the current user's username
            var username = User.Identity.Name;

            // Assuming you have a Users table where you can get the user's ID by their username
            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }
            var userId = user.UserID; // Assuming 'UserID' is the primary key of your User table

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // Save Project
                    if (model.Project != null)
                    {
                        model.Project.CreatedDate = DateTime.Now; // Set CreatedDate to current date and time
                        model.Project.CreatedBy = userId; // Set CreatedBy to the current user's ID
                        model.Project.ProjectManagerID = userId; // Set ProjectManagerID to the current user's ID
                        db.Projects.Add(model.Project);
                        await db.SaveChangesAsync();
                        // Log project save

                        // Update related entities with the ProjectID
                        model.Link.ProjectID = model.Project.ProjectID;
                        model.Attachment.ProjectID = model.Project.ProjectID;
                        model.Version.ProjectID = model.Project.ProjectID;
                        model.Comment.ProjectID = model.Project.ProjectID;
                    }

                    // Save Link
                    if (model.Link != null && model.Link.ProjectID != 0)
                    {
                        model.Link.CreatedDate = DateTime.Now;
                        model.Link.CreatedBy = userId; // Set CreatedBy to the current user's ID
                        db.Links.Add(model.Link);
                    }

                    // Save Version
                    if (model.Version != null && model.Version.ProjectID != 0)
                    {
                        model.Version.ReleaseDate = DateTime.Now;
                        model.Version.CreatedBy = userId; // Set CreatedBy to the current user's ID
                        db.Versions.Add(model.Version);
                    }

                    // Save Attachment
                    if (model.Attachment != null && model.Attachment.ProjectID != 0)
                    {
                        model.Attachment.UploadedDate = DateTime.Now;
                        model.Attachment.UploadedBy = userId;
                        db.Attachments.Add(model.Attachment);
                    }

                    // Save Comment
                    if (model.Comment != null && model.Comment.ProjectID != 0)
                    {
                        model.Comment.CreatedDate = DateTime.Now;
                        db.Comments.Add(model.Comment);
                    }

                    await db.SaveChangesAsync();
                    transaction.Commit();

                    return Json(new { success = true, message = "Data saved successfully." });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    // Log the exception (ex)
                    return Json(new { success = false, message = "An error occurred while saving the data." });
                }
            }
        }





        // GET: Finish

        // GET: Projects/Create
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
            return PartialView("Delete", projects); // Return partial view for the modal
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            // Find all versions that reference this project
            var versions = db.Versions.Where(v => v.ProjectID == id).ToList();

            // Remove all found versions
            foreach (var version in versions)
            {
                db.Versions.Remove(version);
            }

            // Remove the project
            Projects project = db.Projects.Find(id);
            db.Projects.Remove(project);
            db.SaveChanges();

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


        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }

            if (!IsModificationRequestValidated(project.ProjectID))
            {
                return RedirectToAction("Create", "Modifications");
            }

            var viewModel = new StepperViewModel
            {
                Project = project,
                Link = db.Links.FirstOrDefault(l => l.ProjectID == id) ?? new Links(),
                Version = db.Versions.FirstOrDefault(v => v.ProjectID == id) ?? new Versions(),
                Attachment = db.Attachments.FirstOrDefault(a => a.ProjectID == id) ?? new Attachments(),
                Comment = db.Comments.FirstOrDefault(c => c.ProjectID == id) ?? new Comments()
            };

            return PartialView("Edit", viewModel); // Assurez-vous que le nom de la vue est correct
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(StepperViewModel viewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    throw new InvalidOperationException("Invalid model state.");
                }
               
                    var project = db.Projects.Find(viewModel.Project.ProjectID);
                    if (project == null)
                    {
                        return HttpNotFound();
                    }

                    project.ProjectName = viewModel.Project.ProjectName;
                    project.Description = viewModel.Project.Description;
                    project.LastModifiedDate = viewModel.Project.LastModifiedDate;
                    project.Status = viewModel.Project.Status;
                    project.Priority = viewModel.Project.Priority;

                    db.Entry(project).State = EntityState.Modified;
                    db.SaveChanges();

                    // Mettre à jour les autres entités
                    var link = db.Links.FirstOrDefault(l => l.ProjectID == viewModel.Link.ProjectID);
                    if (link != null)
                    {
                        link.URL = viewModel.Link.URL;
                        link.Description = viewModel.Link.Description;
                        link.Category = viewModel.Link.Category;

                        db.Entry(link).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    var version = db.Versions.FirstOrDefault(v => v.ProjectID == viewModel.Version.ProjectID);
                    if (version != null)
                    {
                        version.SourcePath = viewModel.Version.SourcePath;
                        version.DestinationPath = viewModel.Version.DestinationPath;
                        version.Notes = viewModel.Version.Notes;
                        version.ModifiedBy = viewModel.Version.ModifiedBy;
                        version.ModifiedDate = viewModel.Version.ModifiedDate;

                        db.Entry(version).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    var attachment = db.Attachments.FirstOrDefault(a => a.ProjectID == viewModel.Attachment.ProjectID);
                    if (attachment != null)
                    {
                        attachment.FilePath = viewModel.Attachment.FilePath;
                        attachment.Description = viewModel.Attachment.Description;

                        db.Entry(attachment).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    var comment = db.Comments.FirstOrDefault(c => c.ProjectID == viewModel.Comment.ProjectID);
                    if (comment != null)
                    {
                        comment.Content = viewModel.Comment.Content;

                        db.Entry(comment).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    return Json(new { success = true });
                
            }
            catch (Exception ex)
            {

                return Json(new { success = false, message = ex.Message });
            }
        }


        private bool IsModificationRequestValidated(int projectId)
        {
            // Récupérer toutes les demandes de modification associées au projet
            var requests = db.ModificationRequests
                            .Where(mr => mr.ProjectID == projectId)
                            .ToList();

            // Trouver la demande de modification validée la plus récente
            var validatedRequest = requests
                .Where(mr => mr.Status == "Validée")
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
