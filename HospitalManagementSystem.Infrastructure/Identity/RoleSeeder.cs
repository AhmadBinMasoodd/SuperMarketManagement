using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace HospitalManagementSystem.Infrastructure.Identity.Seed
{
    public class RoleSeeder
    {
        public static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole("Admin"));

            if (!await roleManager.RoleExistsAsync("Doctor"))
                await roleManager.CreateAsync(new IdentityRole("Doctor"));

            if (!await roleManager.RoleExistsAsync("Receptionist"))
                await roleManager.CreateAsync(new IdentityRole("Receptionist"));
        }
    }
}
