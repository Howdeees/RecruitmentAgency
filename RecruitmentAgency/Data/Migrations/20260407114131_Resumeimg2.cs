using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecruitmentAgency.Data.Migrations
{
    /// <inheritdoc />
    public partial class Resumeimg2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "Vacancies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "Vacancies");
        }
    }
}
