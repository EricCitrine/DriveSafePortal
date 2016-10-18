namespace Telematics.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class log2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Actions", "driver_id", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Actions", "driver_id");
        }
    }
}
