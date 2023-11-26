using Microsoft.EntityFrameworkCore.Migrations;
using MikeyT.DbMigrations;

#nullable disable

namespace DbMigrations.Migrations.TestDbContextMigrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            MigrationScriptRunner.RunScript(migrationBuilder, "Test/Initial.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            MigrationScriptRunner.RunScript(migrationBuilder, "Test/Initial_Down.sql");
        }
    }
}
