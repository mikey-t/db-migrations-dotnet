using Microsoft.EntityFrameworkCore.Migrations;
using MikeyT.DbMigrations;

#nullable disable

namespace DbMigrations.Migrations.TestDbContextMigrations
{
    public partial class AddPerson : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            MigrationScriptRunner.RunScript(migrationBuilder, "Test/AddPerson.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            MigrationScriptRunner.RunScript(migrationBuilder, "Test/AddPerson_Down.sql");
        }
    }
}
