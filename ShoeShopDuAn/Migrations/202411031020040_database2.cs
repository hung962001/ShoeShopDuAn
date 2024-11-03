namespace ShoeShopDuAn.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class database2 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.tb_OrderDetail", "Quantity", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.tb_OrderDetail", "Quantity", c => c.Int(nullable: false));
        }
    }
}
