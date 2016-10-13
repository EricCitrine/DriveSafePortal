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
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
            
            // Create Roles
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

            // Add sample vehicle
            try
            {
                List<Vehicle> vehicles = new List<Vehicle>()
                {
                    new Vehicle { id = 1, lic_no = "S123A", brand = "Toyota", v_model = "Swift", device_no = "123" },
                    new Vehicle { id = 2, lic_no = "S456B", brand = "Mercedes", v_model = "Benz", device_no = "456" },
                    new Vehicle { id = 3, lic_no = "S789C", brand = "Honda", v_model = "Civic", device_no = "789" }
                };
                vehicles.ForEach(s => context.Vehicle.AddOrUpdate(p => p.id, s));
                context.SaveChanges();
            }
            catch (Exception e)
            {
                throw new Exception("Error creating log " + e.Message);
            }

            // Add sample log
            //try
            //{
            //    List<Logs> logs = new List<Logs>()
            //    {
            //        new Logs { id = 1, lic_no = "S999G", lat = 1.285485, lng = 103.802524 },
            //        new Logs { id = 2, lic_no = "S999G", lat = 1.308339, lng = 103.775796 },
            //        new Logs { id = 3, lic_no = "S999G", lat = 1.334327, lng = 103.811529 }
            //    };
            //    logs.ForEach(s => context.Logs.AddOrUpdate(p => p.id, s));
            //    context.SaveChanges();
            //} catch(Exception e) {
            //    throw new Exception("Error creating log "+e.Message);
            //}
        }
    }
}
