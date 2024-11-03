namespace ShoeShopDuAn.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ImageProduct : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ProductImages", "Image", c => c.String(maxLength: 255));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ProductImages", "Image", c => c.String());
        }
    }
}
