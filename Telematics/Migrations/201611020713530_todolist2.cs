namespace Telematics.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class todolist2 : DbMigration
    {
        public override void Up()
        {
            DropTable("dbo.TodoItems");
        }
        
        public override void Down()
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
    }
}
