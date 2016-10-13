namespace Telematics.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class vehicle4 : DbMigration
    {
        public override void Up()
        {
            DropTable("dbo.Vehicles");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Vehicles",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        license_number = c.String(),
                        brand = c.String(),
                        vehicle_model = c.String(),
                        device_id = c.String(),
                    })
                .PrimaryKey(t => t.id);
            
        }
    }
}
