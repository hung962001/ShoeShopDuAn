namespace ShoeShopDuAn.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class database1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tb_order", "Email", c => c.String());
            AddColumn("dbo.tb_order", "TypePayment", c => c.Int(nullable: false));
            AlterColumn("dbo.tb_order", "TotalAmount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.tb_order", "TotalAmount", c => c.String());
            DropColumn("dbo.tb_order", "TypePayment");
            DropColumn("dbo.tb_order", "Email");
        }
    }
}
