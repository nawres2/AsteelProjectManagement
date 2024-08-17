namespace AsteelProjectManagement.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Comments
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Comments()
        {
            Comments1 = new HashSet<Comments>();
        }

        [Key]
        public int CommentID { get; set; }

        public int? ProjectID { get; set; }

        public int? VersionID { get; set; }

        public int UserID { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime? CreatedDate { get; set; }

        public int? ParentCommentID { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Comments> Comments1 { get; set; }

        public virtual Comments Comments2 { get; set; }

        public virtual Projects Projects { get; set; }

        public virtual Users Users { get; set; }

        public virtual Versions Versions { get; set; }
    }
}
