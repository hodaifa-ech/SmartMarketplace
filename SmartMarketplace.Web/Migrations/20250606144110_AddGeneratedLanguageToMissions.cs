using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartMarketplace.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddGeneratedLanguageToMissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GeneratedLanguage",
                table: "Missions",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GeneratedLanguage",
                table: "Missions");
        }
    }
}
