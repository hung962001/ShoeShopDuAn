namespace ShoeShopDuAn.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class adv : DbMigration
    {
        public override void Up()
        {
            DropTable("dbo.tb_Adv");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.tb_Adv",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 150),
                        Description = c.String(maxLength: 500),
                        CategoryId = c.Int(nullable: false),
                        Image = c.String(),
                        Link = c.String(maxLength: 500),
                        CreatedBy = c.String(),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(nullable: false),
                        ModifiedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
    }
}
