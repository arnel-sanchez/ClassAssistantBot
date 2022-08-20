using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassAssistantBot.Migrations
{
    public partial class UpdateClassIntervention : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassInterventions_ClassRooms_ClassRoomId",
                table: "ClassInterventions");

            migrationBuilder.DropColumn(
                name: "DateTime",
                table: "ClassInterventions");

            migrationBuilder.RenameColumn(
                name: "ClassRoomId",
                table: "ClassInterventions",
                newName: "ClassId");

            migrationBuilder.RenameIndex(
                name: "IX_ClassInterventions_ClassRoomId",
                table: "ClassInterventions",
                newName: "IX_ClassInterventions_ClassId");

            migrationBuilder.AddColumn<bool>(
                name: "Finished",
                table: "ClassInterventions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassInterventions_Classes_ClassId",
                table: "ClassInterventions",
                column: "ClassId",
                principalTable: "Classes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassInterventions_Classes_ClassId",
                table: "ClassInterventions");

            migrationBuilder.DropColumn(
                name: "Finished",
                table: "ClassInterventions");

            migrationBuilder.RenameColumn(
                name: "ClassId",
                table: "ClassInterventions",
                newName: "ClassRoomId");

            migrationBuilder.RenameIndex(
                name: "IX_ClassInterventions_ClassId",
                table: "ClassInterventions",
                newName: "IX_ClassInterventions_ClassRoomId");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateTime",
                table: "ClassInterventions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddForeignKey(
                name: "FK_ClassInterventions_ClassRooms_ClassRoomId",
                table: "ClassInterventions",
                column: "ClassRoomId",
                principalTable: "ClassRooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
