namespace SCE___FINAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class courseReq : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Courses", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.Courses", "Class", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Courses", "Class", c => c.String());
            AlterColumn("dbo.Courses", "Name", c => c.String());
        }
    }
}
