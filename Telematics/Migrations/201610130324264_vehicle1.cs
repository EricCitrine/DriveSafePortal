namespace Telematics.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class vehicle1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Vehicles", "vehicle_model", c => c.String());
            DropColumn("dbo.Vehicles", "model");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Vehicles", "model", c => c.String());
            DropColumn("dbo.Vehicles", "vehicle_model");
        }
    }
}
