using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecruitmentAgency.Data.Migrations
{
    /// <inheritdoc />
    public partial class ModifiResume : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DesiredSalary",
                table: "Resumes");

            migrationBuilder.RenameColumn(
                name: "FullName",
                table: "Resumes",
                newName: "Title");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "Resumes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "Resumes");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Resumes",
                newName: "FullName");

            migrationBuilder.AddColumn<decimal>(
                name: "DesiredSalary",
                table: "Resumes",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
