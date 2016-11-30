namespace Telematics.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class todolist1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TodoItems",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        Text = c.String(),
                    })
                .PrimaryKey(t => t.id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.TodoItems");
        }
    }
}
