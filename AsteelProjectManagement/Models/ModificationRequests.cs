namespace AsteelProjectManagement.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ModificationRequests
    {
        [Key]
        public int RequestID { get; set; }

        public int ProjectID { get; set; }

        public int VersionID { get; set; }

        public int RequesterID { get; set; }

        [Required]
        public string Description { get; set; }

        public DateTime? RequestedDate { get; set; }

        [StringLength(50)]
        public string Status { get; set; }

        public int? ReviewedBy { get; set; }

        public DateTime? ReviewDate { get; set; }

        public string ModificationNotes { get; set; }

        public string Modification { get; set; }


        public virtual Projects Projects { get; set; }

        public virtual Users Users { get; set; }

        public virtual Users Users1 { get; set; }

        public virtual Versions Versions { get; set; }
    }
}
