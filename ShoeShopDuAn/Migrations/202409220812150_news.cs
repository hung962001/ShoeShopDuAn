namespace ShoeShopDuAn.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class news : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.tb_News", "CategoryId", "dbo.Categories");
            DropIndex("dbo.tb_News", new[] { "CategoryId" });
        }
        
        public override void Down()
        {
            CreateIndex("dbo.tb_News", "CategoryId");
            AddForeignKey("dbo.tb_News", "CategoryId", "dbo.Categories", "Id", cascadeDelete: true);
        }
    }
}
