namespace Entity.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class gf : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.AccountModels", "Role", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.AccountModels", "Role", c => c.Byte(nullable: false));
        }
    }
}
