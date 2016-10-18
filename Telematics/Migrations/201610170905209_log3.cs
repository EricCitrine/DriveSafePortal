namespace Telematics.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class log3 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Actions", "lat", c => c.String());
            AlterColumn("dbo.Actions", "lng", c => c.String());
            AlterColumn("dbo.GPSLocations", "lat", c => c.String());
            AlterColumn("dbo.GPSLocations", "lng", c => c.String());
            AlterColumn("dbo.Logs", "lat", c => c.String());
            AlterColumn("dbo.Logs", "lng", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Logs", "lng", c => c.Double(nullable: false));
            AlterColumn("dbo.Logs", "lat", c => c.Double(nullable: false));
            AlterColumn("dbo.GPSLocations", "lng", c => c.Double(nullable: false));
            AlterColumn("dbo.GPSLocations", "lat", c => c.Double(nullable: false));
            AlterColumn("dbo.Actions", "lng", c => c.Double(nullable: false));
            AlterColumn("dbo.Actions", "lat", c => c.Double(nullable: false));
        }
    }
}
