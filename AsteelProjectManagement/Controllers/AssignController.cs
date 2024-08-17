using AsteelProjectManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AsteelProjectManagement.Controllers
{
    public class AssignController : Controller
    {
        // GET: Assign
        private PrjContext db = new PrjContext();
        public ActionResult Assign()
        {
            var tasks = db.Tasks.Where(t => t.AssignedTo == null).ToList(); // Tâches non assignées
            var employees = db.Users.ToList();
            ViewBag.Tasks = new SelectList(tasks, "TaskID", "TaskName");
            ViewBag.Employees = new SelectList(employees, "EmployeeID", "Name");
            return View();
        }

        // POST: Tasks/Assign
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Assign(int taskId, int employeeId)
        {
            var task = db.Tasks.Find(taskId);
            if (task != null)
            {
                task.AssignedTo = employeeId;
                db.SaveChanges();
                return RedirectToAction("Assign");
            }
            // Recharger les données pour l'affichage
            ViewBag.Tasks = new SelectList(db.Tasks.ToList(), "TaskID", "TaskName");
            ViewBag.Employees = new SelectList(db.Users.ToList(), "EmployeeID", "Name");
            return View();
        }
    }
}