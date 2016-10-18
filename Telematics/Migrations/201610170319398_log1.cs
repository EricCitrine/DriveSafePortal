namespace Telematics.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class log1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.OBDs",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        device_id = c.String(),
                        lic_no = c.String(),
                        time_stamp = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.id);
            
            AddColumn("dbo.Actions", "alarm_msg", c => c.String());
            AddColumn("dbo.Actions", "alarm_type", c => c.String());
            AddColumn("dbo.Actions", "location", c => c.String());
            DropColumn("dbo.Actions", "value");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Actions", "value", c => c.String());
            DropColumn("dbo.Actions", "location");
            DropColumn("dbo.Actions", "alarm_type");
            DropColumn("dbo.Actions", "alarm_msg");
            DropTable("dbo.OBDs");
        }
    }
}
