namespace AsteelProjectManagement.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Tasks
    {
        [Key]
        public int TaskID { get; set; }

        public int ProjectID { get; set; }

        [Required]
        [StringLength(255)]
        public string TaskName { get; set; }

        public string Description { get; set; }

        public int? AssignedTo { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? DueDate { get; set; }

        [StringLength(50)]
        public string Priority { get; set; }

        public DateTime? CompletedDate { get; set; }

        public virtual Projects Projects { get; set; }

        public virtual Users Users { get; set; }
    }
}
