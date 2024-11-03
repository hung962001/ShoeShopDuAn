namespace ShoeShopDuAn.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class news1 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.tb_News", "CategoryId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.tb_News", "CategoryId", c => c.Int(nullable: false));
        }
    }
}
