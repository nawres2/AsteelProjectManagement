namespace AsteelProjectManagement.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class UserRoleAssignments
    {
        [Key]
        public int UserRoleID { get; set; }

        public int UserID { get; set; }

        public int RoleID { get; set; }

        public DateTime? AssignedDate { get; set; }

        public virtual UserRoles UserRoles { get; set; }

        public virtual Users Users { get; set; }
    }
}
