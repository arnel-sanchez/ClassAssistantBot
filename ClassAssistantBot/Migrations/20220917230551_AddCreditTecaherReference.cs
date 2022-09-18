using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassAssistantBot.Migrations
{
    public partial class AddCreditTecaherReference : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Code",
                table: "Credits",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "TeacherId",
                table: "Credits",
                type: "bigint",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_Credits_TeacherId",
                table: "Credits",
                column: "TeacherId");

            migrationBuilder.AddForeignKey(
                name: "FK_Credits_Users_TeacherId",
                table: "Credits",
                column: "TeacherId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Credits_Users_TeacherId",
                table: "Credits");

            migrationBuilder.DropIndex(
                name: "IX_Credits_TeacherId",
                table: "Credits");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Credits");

            migrationBuilder.DropColumn(
                name: "TeacherId",
                table: "Credits");
        }
    }
}
