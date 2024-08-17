namespace AsteelProjectManagement.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Versions
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Versions()
        {
            Attachments = new HashSet<Attachments>();
            Comments = new HashSet<Comments>();
            ModificationRequests = new HashSet<ModificationRequests>();
        }

        [Key]
        public int VersionID { get; set; }

        public int ProjectID { get; set; }

        [Required]
        [StringLength(50)]
        public string VersionNumber { get; set; }

        [Required]
        [StringLength(255)]
        public string SourcePath { get; set; }

        [Required]
        [StringLength(255)]
        public string DestinationPath { get; set; }

        public DateTime? ReleaseDate { get; set; }

        public int CreatedBy { get; set; }

        public string Notes { get; set; }

        public bool? IsStable { get; set; }

        public int? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Attachments> Attachments { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Comments> Comments { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ModificationRequests> ModificationRequests { get; set; }

        public virtual Projects Projects { get; set; }

        public virtual Users Users { get; set; }

        public virtual Users Users1 { get; set; }
    }
}
