namespace Telematics.Migrations
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Telematics.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(Telematics.Models.ApplicationDbContext context)
        {
            try
            {
                List<string> myRoles = new List<string>(new string[] { "Super", "Admin", "Manager", "Driver" });
                var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
                for (int i = 0; i < myRoles.Count(); i++)
                {
                    if (RoleManager.RoleExists(myRoles[i]) == false)
                    {
                        RoleManager.Create(new IdentityRole(myRoles[i]));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error Create Roles: " + ex.Message);
            }
        }
    }
}
