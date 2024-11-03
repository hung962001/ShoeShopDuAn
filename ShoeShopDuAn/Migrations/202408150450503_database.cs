namespace ShoeShopDuAn.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class database : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tb_Product", "ProductId", c => c.Int(nullable: false));
            CreateIndex("dbo.ProductImages", "ProductId");
            AddForeignKey("dbo.ProductImages", "ProductId", "dbo.tb_Product", "Id", cascadeDelete: true);
            DropColumn("dbo.tb_Product", "CategoryId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.tb_Product", "CategoryId", c => c.Int(nullable: false));
            DropForeignKey("dbo.ProductImages", "ProductId", "dbo.tb_Product");
            DropIndex("dbo.ProductImages", new[] { "ProductId" });
            DropColumn("dbo.tb_Product", "ProductId");
        }
    }
}
