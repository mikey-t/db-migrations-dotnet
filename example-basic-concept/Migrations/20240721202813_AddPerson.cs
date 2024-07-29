using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace example_basic_concept.Migrations
{
    /// <inheritdoc />
    public partial class AddPerson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RunScript("AddPerson.sql");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RunScript("AddPerson_Down.sql");
        }
    }
}
