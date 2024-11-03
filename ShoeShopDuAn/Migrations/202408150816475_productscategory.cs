namespace ShoeShopDuAn.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class productscategory : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tb_ProductCategory", "Alias", c => c.String());
            AddColumn("dbo.tb_ProductCategory", "IsDelete", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.tb_ProductCategory", "IsDelete");
            DropColumn("dbo.tb_ProductCategory", "Alias");
        }
    }
}
