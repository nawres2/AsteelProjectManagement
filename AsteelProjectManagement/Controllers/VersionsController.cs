using AsteelProjectManagement.Models;
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace AsteelProjectManagement.Controllers
{
    public class VersionsController : Controller
    {
        private PrjContext db = new PrjContext();

        public ActionResult Versions(int projectId)
        {
            var versions = db.Versions.Where(v => v.ProjectID == projectId).ToList();
            return View(versions);
        }


        public ActionResult VersionsDetails(int? id)
        {
            var versions = db.Versions.Where(v => v.ProjectID == id).ToList();
            ViewBag.ProjectID = id; // Pass the ProjectID to the view
            return View(versions);
        }

        public ActionResult Create()
        {
           // Debug.WriteLine("Create Action GET - ProjectID: " + projectId);
           
            ViewBag.CreatedBy = new SelectList(db.Users, "UserID", "UserName");
            ViewBag.ModifiedBy = new SelectList(db.Users, "UserID", "UserName");
            ViewBag.Projects = new SelectList(db.Projects, "ProjectID", "ProjectName");
            

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "VersionID,VersionNumber,SourcePath,DestinationPath,ReleaseDate,CreatedBy,Notes,ModifiedBy,ModifiedDate,ProjectID")] Versions version)
        {
            Debug.WriteLine("Create Action POST - ProjectID: " + version.ProjectID);

            // Check if ProjectID is valid
            var projectExists = db.Projects.Any(p => p.ProjectID == version.ProjectID);
            if (!projectExists)
            {
                ModelState.AddModelError("ProjectID", "Invalid ProjectID. The project does not exist.");
                ViewBag.ProjectID = new SelectList(db.Projects, "ProjectID", "ProjectName", version.ProjectID);
                ViewBag.CreatedBy = new SelectList(db.Users, "UserID", "Username", version.CreatedBy);
                ViewBag.ModifiedBy = new SelectList(db.Users, "UserID", "Username", version.ModifiedBy);
                return View(version);
            }

            try
            {
                db.Versions.Add(version);
                db.SaveChanges();
                return RedirectToAction("Versions", new { projectId = version.ProjectID });
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Debug.WriteLine($"Property: {validationError.PropertyName}, Error: {validationError.ErrorMessage}");
                    }
                }
                // Optionally, rethrow the exception or handle it in another way
                throw;
            }
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Versions versions = db.Versions.Find(id);
            if (versions == null)
            {
                return HttpNotFound();
            }

            // Populate dropdown lists for CreatedBy and ProjectManagerID
           

            ViewBag.CreatedBy = new SelectList(db.Users, "UserID", "Username", versions.CreatedBy);
            ViewBag.ModifiedBy = new SelectList(db.Users, "UserID", "Username", versions.ModifiedBy);

            // Return the partial view for editing the project
            return PartialView("Edit", versions);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

       public ActionResult Edit([Bind(Include = "VersionID,VersionNumber,SourcePath,DestinationPath,ReleaseDate,CreatedBy,Notes,IsStable,ModifiedBy,ModifiedDate,ProjectID")] Versions version)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Update all versions related to this project
                    var versionsToUpdate = db.Versions.Where(v => v.ProjectID == version.ProjectID).ToList();

                    foreach (var v in versionsToUpdate)
                    {
                        v.VersionNumber = version.VersionNumber;
                        v.SourcePath = version.SourcePath;
                        v.DestinationPath = version.DestinationPath;
                        v.ReleaseDate = version.ReleaseDate;
                        v.CreatedBy = version.CreatedBy;
                        v.Notes = version.Notes;
                        v.IsStable = version.IsStable;
                        v.ModifiedBy = version.ModifiedBy;
                        v.ModifiedDate = version.ModifiedDate;

                        // Update each version entity state
                        db.Entry(v).State = EntityState.Modified;
                    }

                    // Save changes
                    db.SaveChanges();

                    return RedirectToAction("Versions", new { projectId = version.ProjectID });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while updating versions. Please try again.");
                    Debug.WriteLine(ex.Message);
                }
            }

            // If model state is not valid or an error occurred, re-populate dropdown lists and return to the edit view
            ViewBag.CreatedBy = new SelectList(db.Users, "UserID", "Username", version.CreatedBy);
            ViewBag.ModifiedBy = new SelectList(db.Users, "UserID", "Username", version.ModifiedBy);

            return PartialView("Edit", version);
        }







        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Versions versions = db.Versions.Find(id);
            if (versions == null)
            {
                return HttpNotFound();
            }
            return PartialView("Delete", versions); // Retourne une vue partielle pour la confirmation modale
        }

        // POST: Versions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Versions versions = db.Versions.Find(id);
            if (versions == null)
            {
                return HttpNotFound();
            }

            db.Versions.Remove(versions);
            db.SaveChanges();

            // Récupérez le projectId de la version supprimée
            int projectId = versions.ProjectID;

            // Redirigez vers l'action "Versions" avec le projectId
            return RedirectToAction("Versions", new { projectId = projectId });
        }



        public ActionResult AddAttachment(int projectId)
        {
            System.Diagnostics.Debug.WriteLine("AddAttachment GET called with projectId: " + projectId);

            var model = new Attachments { ProjectID = projectId };
            return PartialView("AddAttachment", model);
        }

        // POST: Projects/AddLink
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddAttachment([Bind(Include = "AttachmentID,ProjectID,VersionID,FilePath,Description,UploadedDate,UploadedBy")] Attachments attachment)
        {
            System.Diagnostics.Debug.WriteLine("AddAttachment POST called with attachment: " + attachment.ProjectID);

            if (ModelState.IsValid)
            {
                // Sauvegarde de l'attachement dans la base de données ou autre traitement nécessaire
                attachment.UploadedDate = DateTime.Now;


                db.Attachments.Add(attachment);
                db.SaveChanges();

                return Json(new { success = true });
            }

            System.Diagnostics.Debug.WriteLine("Model is invalid.");
            return PartialView("AddAttachment", attachment);
        }



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
