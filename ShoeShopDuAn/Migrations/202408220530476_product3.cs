namespace ShoeShopDuAn.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class product3 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.tb_Product", "IsHome");
        }
        
        public override void Down()
        {
            AddColumn("dbo.tb_Product", "IsHome", c => c.Boolean(nullable: false));
        }
    }
}
