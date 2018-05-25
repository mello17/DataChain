namespace Entity.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class wss : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Transactions", new[] { "Signature" });
            AlterColumn("dbo.Transactions", "Signature", c => c.Binary(nullable: false, maxLength: 1154));
            CreateIndex("dbo.Transactions", "Signature");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Transactions", new[] { "Signature" });
            AlterColumn("dbo.Transactions", "Signature", c => c.Binary(nullable: false, maxLength: 1124));
            CreateIndex("dbo.Transactions", "Signature");
        }
    }
}
