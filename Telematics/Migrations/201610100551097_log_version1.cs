namespace Telematics.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class log_version1 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Logs", "lat", c => c.Double(nullable: false));
            AlterColumn("dbo.Logs", "lng", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Logs", "lng", c => c.Int(nullable: false));
            AlterColumn("dbo.Logs", "lat", c => c.Int(nullable: false));
        }
    }
}
