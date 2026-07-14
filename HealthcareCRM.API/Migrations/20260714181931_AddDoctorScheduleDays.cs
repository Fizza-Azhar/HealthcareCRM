using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthcareCRM.API.Migrations
{
    /// <inheritdoc />
    public partial class AddDoctorScheduleDays : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ScheduleDays",
                table: "Doctors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ScheduleDays",
                table: "Doctors");
        }
    }
}
