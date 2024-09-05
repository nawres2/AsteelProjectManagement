using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AsteelProjectManagement.Models
{
    public class EditProfileViewModel
    {


     
            public int UserID { get; set; }
            [Required]
            public string Username { get; set; }
            [Required]
            public string FirstName { get; set; }
            [Required]
            public string LastName { get; set; }
            [Required]
            public string Email { get; set; }
            [Required]
            public string Role { get; set; }
            public DateTime? DateJoined { get; set; }
            public string PasswordHash { get; set; }
        

    }
}