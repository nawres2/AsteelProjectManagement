namespace AsteelProjectManagement.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Notifications
    {
        [Key]
        public int NotificationID { get; set; }

        public int UserID { get; set; }

        [Required]
        public string Message { get; set; }

        public bool? IsRead { get; set; }

        public DateTime? CreatedDate { get; set; }

        [StringLength(50)]
        public string Type { get; set; }

        public virtual Users Users { get; set; }
    }
}
