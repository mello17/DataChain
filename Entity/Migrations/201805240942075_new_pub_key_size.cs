namespace Entity.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class new_pub_key_size : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Transactions", new[] { "PubKey" });
            AlterColumn("dbo.Transactions", "PubKey", c => c.Binary(nullable: false, maxLength: 450));
            CreateIndex("dbo.Transactions", "PubKey");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Transactions", new[] { "PubKey" });
            AlterColumn("dbo.Transactions", "PubKey", c => c.Binary(nullable: false, maxLength: 300));
            CreateIndex("dbo.Transactions", "PubKey");
        }
    }
}
