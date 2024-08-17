using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AsteelProjectManagement.Models
{
    public class Modifications
    {
        public Projects Projects { get; set; }
        public Versions Versions { get; set; }
        public Users Users { get; set; }
        public ModificationRequests Modification { get; set; }
    }
}