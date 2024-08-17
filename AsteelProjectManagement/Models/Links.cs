namespace AsteelProjectManagement.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Links
    {
        [Key]
        public int LinkID { get; set; }

        public int ProjectID { get; set; }

        [Required]
        [StringLength(255)]
        public string URL { get; set; }

        public string Description { get; set; }

        public DateTime? CreatedDate { get; set; }

        public int CreatedBy { get; set; }

        [StringLength(50)]
        public string Category { get; set; }

        public virtual Users Users { get; set; }

        public virtual Projects Projects { get; set; }
    }
}
