using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AsteelProjectManagement.Models
{
    public class ProjectViewModel
    {
        public Projects Projects { get; set; }
        public List<Links> Links { get; set; } = new List<Links>();
        public List<Attachments> Attachments { get; set; } = new List<Attachments>();
        public List<Versions> Versions { get; set; } = new List<Versions>();
        public List<Comments> Comments { get; set; } = new List<Comments>();
        public string CreatedByUsername { get; set; }
        public string ProjectManagerUsername { get; set; }




    }
}