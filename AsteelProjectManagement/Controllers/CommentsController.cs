using AsteelProjectManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AsteelProjectManagement.Controllers
{
    public class CommentsController : Controller
    {
        private PrjContext db = new PrjContext();

        public ActionResult AddComment(Comments comment)

        // GET: Comments
        {
            if (ModelState.IsValid)
        {
            db.Comments.Add(comment);
            db.SaveChanges();
            return Json(new { success = true });
        }

return Json(new { success = false });
    }
    }
}