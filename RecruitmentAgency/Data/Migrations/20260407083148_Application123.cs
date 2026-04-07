using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecruitmentAgency.Data.Migrations
{
    /// <inheritdoc />
    public partial class Application123 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RecruiterNotes",
                table: "Applications",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecruiterNotes",
                table: "Applications");
        }
    }
}
