using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace AsteelProjectManagement.Models
{
    public class StepperViewModel
    {
        public bool SelectOnFocus { get; set; }
        public Projects Project { get; set; }
        public Links Link { get; set; }

        public Attachments Attachment { get; set; }
        public Versions Version { get; set; }
        public Comments Comment { get; set; }
        public IEnumerable<SelectListItem> Categories { get; set; } // List of categories for the dropdown



    }
}
