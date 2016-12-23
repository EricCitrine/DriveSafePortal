using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace Telematics.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }

        public string Company { get; set; }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public System.Data.Entity.DbSet<Telematics.Models.Logs> Logs { get; set; }
        public System.Data.Entity.DbSet<Telematics.Models.Actions> Actions { get; set; }
        public System.Data.Entity.DbSet<Telematics.Models.GPSLocation> GPSLocation { get; set; }
        public System.Data.Entity.DbSet<Telematics.Models.Vehicle> Vehicle { get; set; }
        public System.Data.Entity.DbSet<Telematics.Models.OBD> OBD { get; set; }
        public System.Data.Entity.DbSet<Telematics.Models.DriverScore> DriverScore { get; set; }
        public System.Data.Entity.DbSet<Telematics.Models.Companie> Companie { get; set; }
        public System.Data.Entity.DbSet<Telematics.Models.VehicleStatus> VehicleStatus { get; set; }
        public System.Data.Entity.DbSet<Telematics.Models.Trips> Trips { get; set; }
        public System.Data.Entity.DbSet<Telematics.Models.Drivers_GPSData> Drivers_GPSData { get; set; }

        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
            Database.SetInitializer<ApplicationDbContext>(null);
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        //protected override void OnModelCreating(DbModelBuilder modelBuilder)
        //{
        //    //DONT DO THIS ANYMORE
        //    //base.OnModelCreating(modelBuilder);
        //    //modelBuilder.Entity<Vote>().ToTable("Votes")
        //    modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        //    base.OnModelCreating(modelBuilder);
        //}
    }
}