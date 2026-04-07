using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecruitmentAgency.Data.Migrations
{
    /// <inheritdoc />
    public partial class VacancyImg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "Vacancies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ViewsCount",
                table: "Vacancies",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "Vacancies");

            migrationBuilder.DropColumn(
                name: "ViewsCount",
                table: "Vacancies");
        }
    }
}
