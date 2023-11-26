using Microsoft.EntityFrameworkCore.Migrations;
using MikeyT.DbMigrations;

#nullable disable

namespace DbMigrations.Migrations.MainDbContextMigrations
{
    public partial class AddPerson : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            MigrationScriptRunner.RunScript(migrationBuilder, "Main/AddPerson.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            MigrationScriptRunner.RunScript(migrationBuilder, "Main/AddPerson_Down.sql");
        }
    }
}
