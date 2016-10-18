namespace Telematics.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class log : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Actions",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        device_id = c.String(),
                        type = c.String(),
                        value = c.String(),
                        time_stamp = c.DateTime(nullable: false),
                        lat = c.Double(nullable: false),
                        lng = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.GPSLocations",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        vehicle_id = c.String(),
                        driver_id = c.String(),
                        time_stamp = c.DateTime(nullable: false),
                        lat = c.Double(nullable: false),
                        lng = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.GPSLocations");
            DropTable("dbo.Actions");
        }
    }
}
