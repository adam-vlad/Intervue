using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Intervue.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPromptProfileToInterview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PromptProfile",
                table: "interviews",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PromptProfile",
                table: "interviews");
        }
    }
}
