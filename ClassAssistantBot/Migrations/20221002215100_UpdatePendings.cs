using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassAssistantBot.Migrations
{
    public partial class UpdatePendings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ClassRoomId",
                table: "StatusPhrases",
                type: "bigint",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<long>(
                name: "ClassRoomId",
                table: "RectificationToTheTeachers",
                type: "bigint",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<long>(
                name: "ClassRoomId",
                table: "Memes",
                type: "bigint",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<long>(
                name: "ClassRoomId",
                table: "Jokes",
                type: "bigint",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<long>(
                name: "ClassRoomId",
                table: "Dailies",
                type: "bigint",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_StatusPhrases_ClassRoomId",
                table: "StatusPhrases",
                column: "ClassRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_RectificationToTheTeachers_ClassRoomId",
                table: "RectificationToTheTeachers",
                column: "ClassRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Memes_ClassRoomId",
                table: "Memes",
                column: "ClassRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Jokes_ClassRoomId",
                table: "Jokes",
                column: "ClassRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Dailies_ClassRoomId",
                table: "Dailies",
                column: "ClassRoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_Dailies_ClassRooms_ClassRoomId",
                table: "Dailies",
                column: "ClassRoomId",
                principalTable: "ClassRooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Jokes_ClassRooms_ClassRoomId",
                table: "Jokes",
                column: "ClassRoomId",
                principalTable: "ClassRooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Memes_ClassRooms_ClassRoomId",
                table: "Memes",
                column: "ClassRoomId",
                principalTable: "ClassRooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RectificationToTheTeachers_ClassRooms_ClassRoomId",
                table: "RectificationToTheTeachers",
                column: "ClassRoomId",
                principalTable: "ClassRooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StatusPhrases_ClassRooms_ClassRoomId",
                table: "StatusPhrases",
                column: "ClassRoomId",
                principalTable: "ClassRooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Dailies_ClassRooms_ClassRoomId",
                table: "Dailies");

            migrationBuilder.DropForeignKey(
                name: "FK_Jokes_ClassRooms_ClassRoomId",
                table: "Jokes");

            migrationBuilder.DropForeignKey(
                name: "FK_Memes_ClassRooms_ClassRoomId",
                table: "Memes");

            migrationBuilder.DropForeignKey(
                name: "FK_RectificationToTheTeachers_ClassRooms_ClassRoomId",
                table: "RectificationToTheTeachers");

            migrationBuilder.DropForeignKey(
                name: "FK_StatusPhrases_ClassRooms_ClassRoomId",
                table: "StatusPhrases");

            migrationBuilder.DropIndex(
                name: "IX_StatusPhrases_ClassRoomId",
                table: "StatusPhrases");

            migrationBuilder.DropIndex(
                name: "IX_RectificationToTheTeachers_ClassRoomId",
                table: "RectificationToTheTeachers");

            migrationBuilder.DropIndex(
                name: "IX_Memes_ClassRoomId",
                table: "Memes");

            migrationBuilder.DropIndex(
                name: "IX_Jokes_ClassRoomId",
                table: "Jokes");

            migrationBuilder.DropIndex(
                name: "IX_Dailies_ClassRoomId",
                table: "Dailies");

            migrationBuilder.DropColumn(
                name: "ClassRoomId",
                table: "StatusPhrases");

            migrationBuilder.DropColumn(
                name: "ClassRoomId",
                table: "RectificationToTheTeachers");

            migrationBuilder.DropColumn(
                name: "ClassRoomId",
                table: "Memes");

            migrationBuilder.DropColumn(
                name: "ClassRoomId",
                table: "Jokes");

            migrationBuilder.DropColumn(
                name: "ClassRoomId",
                table: "Dailies");
        }
    }
}
