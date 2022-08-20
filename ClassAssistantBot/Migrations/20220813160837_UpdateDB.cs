using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassAssistantBot.Migrations
{
    public partial class UpdateDB : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassInterventions_Teachers_TeacherId",
                table: "ClassInterventions");

            migrationBuilder.DropIndex(
                name: "IX_ClassInterventions_TeacherId",
                table: "ClassInterventions");

            migrationBuilder.DropColumn(
                name: "TeacherId",
                table: "ClassInterventions");

            migrationBuilder.AddColumn<long>(
                name: "ClassRoomId",
                table: "ClassInterventions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_ClassInterventions_ClassRoomId",
                table: "ClassInterventions",
                column: "ClassRoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClassInterventions_ClassRooms_ClassRoomId",
                table: "ClassInterventions",
                column: "ClassRoomId",
                principalTable: "ClassRooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassInterventions_ClassRooms_ClassRoomId",
                table: "ClassInterventions");

            migrationBuilder.DropIndex(
                name: "IX_ClassInterventions_ClassRoomId",
                table: "ClassInterventions");

            migrationBuilder.DropColumn(
                name: "ClassRoomId",
                table: "ClassInterventions");

            migrationBuilder.AddColumn<string>(
                name: "TeacherId",
                table: "ClassInterventions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ClassInterventions_TeacherId",
                table: "ClassInterventions",
                column: "TeacherId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClassInterventions_Teachers_TeacherId",
                table: "ClassInterventions",
                column: "TeacherId",
                principalTable: "Teachers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
