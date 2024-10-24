using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourseProject.Models;
using Microsoft.AspNetCore.Identity;

namespace CourseProject.Areas.Identity.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public DateTime LastLogin { get; set; }
    public ICollection<Template>? AccessibleTemplates { get; set; }

}

