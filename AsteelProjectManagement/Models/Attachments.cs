namespace AsteelProjectManagement.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Attachments
    {
        [Key]
        public int AttachmentID { get; set; }

        public int? ProjectID { get; set; }

        public int? VersionID { get; set; }

        [Required]
        [StringLength(255)]
        public string FilePath { get; set; }

        public string Description { get; set; }

        public DateTime? UploadedDate { get; set; }

        public int UploadedBy { get; set; }

        public virtual Projects Projects { get; set; }

        public virtual Users Users { get; set; }

        public virtual Versions Versions { get; set; }
    }
}
