namespace ShoeShopDuAn.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class products : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.tb_Product", "ProductId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.tb_Product", "ProductId", c => c.Int(nullable: false));
        }
    }
}
