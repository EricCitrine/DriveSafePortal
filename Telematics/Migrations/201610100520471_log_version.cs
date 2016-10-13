namespace Telematics.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class log_version : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Logs",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        lic_no = c.String(),
                        lat = c.Int(nullable: false),
                        lng = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Logs");
        }
    }
}
