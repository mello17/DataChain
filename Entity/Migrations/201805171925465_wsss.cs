namespace Entity.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class wsss : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Transactions", new[] { "Signature" });
            AddColumn("dbo.Transactions", "PubKey", c => c.Binary(nullable: false, maxLength: 300));
            AlterColumn("dbo.Transactions", "Signature", c => c.Binary(nullable: false, maxLength: 750));
            CreateIndex("dbo.Transactions", "Signature");
            CreateIndex("dbo.Transactions", "PubKey");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Transactions", new[] { "PubKey" });
            DropIndex("dbo.Transactions", new[] { "Signature" });
            AlterColumn("dbo.Transactions", "Signature", c => c.Binary(nullable: false, maxLength: 1154));
            DropColumn("dbo.Transactions", "PubKey");
            CreateIndex("dbo.Transactions", "Signature");
        }
    }
}
