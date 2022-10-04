using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassAssistantBot.Migrations
{
    public partial class AddChannels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClassInterventionChannel",
                table: "ClassRooms",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ClassTitleChannel",
                table: "ClassRooms",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DiaryChannel",
                table: "ClassRooms",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "JokesChannel",
                table: "ClassRooms",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RectificationToTheTeacherChannel",
                table: "ClassRooms",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StatusPhraseChannel",
                table: "ClassRooms",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClassInterventionChannel",
                table: "ClassRooms");

            migrationBuilder.DropColumn(
                name: "ClassTitleChannel",
                table: "ClassRooms");

            migrationBuilder.DropColumn(
                name: "DiaryChannel",
                table: "ClassRooms");

            migrationBuilder.DropColumn(
                name: "JokesChannel",
                table: "ClassRooms");

            migrationBuilder.DropColumn(
                name: "RectificationToTheTeacherChannel",
                table: "ClassRooms");

            migrationBuilder.DropColumn(
                name: "StatusPhraseChannel",
                table: "ClassRooms");
        }
    }
}
