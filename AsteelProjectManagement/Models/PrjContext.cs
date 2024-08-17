using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace AsteelProjectManagement.Models
{
    public partial class PrjContext : DbContext
    {
        public PrjContext()
            : base("name=PrjContext")
        {
        }

        public virtual DbSet<Attachments> Attachments { get; set; }
        public virtual DbSet<Comments> Comments { get; set; }
        public virtual DbSet<Links> Links { get; set; }
        public virtual DbSet<ModificationRequests> ModificationRequests { get; set; }
        public virtual DbSet<Notifications> Notifications { get; set; }
        public virtual DbSet<Projects> Projects { get; set; }
        public virtual DbSet<sysdiagrams> sysdiagrams { get; set; }
        public virtual DbSet<Tasks> Tasks { get; set; }
        public virtual DbSet<UserRoleAssignments> UserRoleAssignments { get; set; }
        public virtual DbSet<UserRoles> UserRoles { get; set; }
        public virtual DbSet<Users> Users { get; set; }
        public virtual DbSet<Versions> Versions { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Comments>()
                .HasMany(e => e.Comments1)
                .WithOptional(e => e.Comments2)
                .HasForeignKey(e => e.ParentCommentID);

            modelBuilder.Entity<Projects>()
                .HasMany(e => e.Links)
                .WithRequired(e => e.Projects)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Projects>()
                .HasMany(e => e.ModificationRequests)
                .WithRequired(e => e.Projects)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Projects>()
                .HasMany(e => e.Tasks)
                .WithRequired(e => e.Projects)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Projects>()
                .HasMany(e => e.Versions)
                .WithRequired(e => e.Projects)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<UserRoles>()
                .HasMany(e => e.UserRoleAssignments)
                .WithRequired(e => e.UserRoles)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Users>()
                .HasMany(e => e.Attachments)
                .WithRequired(e => e.Users)
                .HasForeignKey(e => e.UploadedBy)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Users>()
                .HasMany(e => e.Comments)
                .WithRequired(e => e.Users)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Users>()
                .HasMany(e => e.Links)
                .WithRequired(e => e.Users)
                .HasForeignKey(e => e.CreatedBy)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Users>()
                .HasMany(e => e.ModificationRequests)
                .WithRequired(e => e.Users)
                .HasForeignKey(e => e.RequesterID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Users>()
                .HasMany(e => e.ModificationRequests1)
                .WithOptional(e => e.Users1)
                .HasForeignKey(e => e.ReviewedBy);

            modelBuilder.Entity<Users>()
                .HasMany(e => e.Notifications)
                .WithRequired(e => e.Users)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Users>()
                .HasMany(e => e.Projects)
                .WithRequired(e => e.Users)
                .HasForeignKey(e => e.CreatedBy)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Users>()
                .HasMany(e => e.Projects1)
                .WithOptional(e => e.Users1)
                .HasForeignKey(e => e.ProjectManagerID);

            modelBuilder.Entity<Users>()
                .HasMany(e => e.Tasks)
                .WithOptional(e => e.Users)
                .HasForeignKey(e => e.AssignedTo);

            modelBuilder.Entity<Users>()
                .HasMany(e => e.UserRoleAssignments)
                .WithRequired(e => e.Users)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Users>()
                .HasMany(e => e.UserRoles)
                .WithOptional(e => e.Users)
                .HasForeignKey(e => e.CreatedBy);

            modelBuilder.Entity<Users>()
                .HasMany(e => e.Versions)
                .WithRequired(e => e.Users)
                .HasForeignKey(e => e.CreatedBy)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Users>()
                .HasMany(e => e.Versions1)
                .WithOptional(e => e.Users1)
                .HasForeignKey(e => e.ModifiedBy);

            modelBuilder.Entity<Versions>()
                .HasMany(e => e.ModificationRequests)
                .WithRequired(e => e.Versions)
                .WillCascadeOnDelete(false);
        }
    }
}
