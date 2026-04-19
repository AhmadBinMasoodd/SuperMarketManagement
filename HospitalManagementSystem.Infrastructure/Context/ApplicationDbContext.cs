using HospitalManagementSystem.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace HospitalManagementSystem.Infrastructure.Context
{
    public class ApplicationDbContext: IdentityDbContext<ApplicationUser>
    {
       public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
       {
        
        }
    }
}
