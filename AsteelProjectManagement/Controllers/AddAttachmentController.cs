using AsteelProjectManagement.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace AsteelProjectManagement.Controllers
{
    public class AddAttachmentController : Controller
    {
        // GET: AddAttachment
        private PrjContext db = new PrjContext();

        // GET: Attachments/AddAttachment
        public ActionResult AddAttachment(int? projectId, int? versionId)
        {
            if (projectId == null && versionId == null)
            {
                // Handle case where neither projectId nor versionId is specified
            }

            ViewBag.Projects = new SelectList(db.Projects, "ProjectID", "ProjectName", projectId);
            ViewBag.Versions = new SelectList(db.Versions, "VersionID", "VersionName", versionId);
            return PartialView("AddAttachment", new Attachments { ProjectID = projectId, VersionID = versionId });
        }

        // POST: Attachments/AddAttachment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddAttachment(Attachments attachment, HttpPostedFileBase uploadFile)
        {
            if (ModelState.IsValid)
            {
                if (uploadFile != null && uploadFile.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(uploadFile.FileName);
                    string filePath = Path.Combine(Server.MapPath("~/Uploads"), fileName);
                    uploadFile.SaveAs(filePath);

                    attachment.FilePath = filePath;
                    attachment.UploadedDate = DateTime.Now;
                    // Set UploadedBy, possibly from the current user

                    db.Attachments.Add(attachment);
                    db.SaveChanges();
                    return RedirectToAction("Index", "Home");
                }
            }

            ViewBag.Projects = new SelectList(db.Projects, "ProjectID", "ProjectName", attachment.ProjectID);
            ViewBag.Versions = new SelectList(db.Versions, "VersionID", "VersionName", attachment.VersionID);
            return PartialView("AddAttachment", attachment);
        }

    }
}