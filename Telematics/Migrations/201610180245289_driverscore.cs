namespace Telematics.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class driverscore : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DriverScores",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        driver_id = c.Int(nullable: false),
                        driver_score = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.DriverScores");
        }
    }
}
