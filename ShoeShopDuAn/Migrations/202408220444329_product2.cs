namespace ShoeShopDuAn.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class product2 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.tb_Product", "Image", c => c.String(maxLength: 255));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.tb_Product", "Image", c => c.String(maxLength: 130));
        }
    }
}
