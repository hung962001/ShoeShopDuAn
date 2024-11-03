namespace ShoeShopDuAn.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class product1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tb_Product", "CategoryId", c => c.Int(nullable: false));
            AlterColumn("dbo.tb_Product", "ProductCode", c => c.String(maxLength: 50));
            AlterColumn("dbo.tb_Product", "PriceSale", c => c.Decimal(precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.tb_Product", "PriceSale", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.tb_Product", "ProductCode", c => c.String());
            DropColumn("dbo.tb_Product", "CategoryId");
        }
    }
}
