using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ClassAssistantBot.Migrations
{
    public partial class AddGuilds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "GuildId",
                table: "Students",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Guilds",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ClassRoomId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guilds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Guilds_ClassRooms_ClassRoomId",
                        column: x => x.ClassRoomId,
                        principalTable: "ClassRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Students_GuildId",
                table: "Students",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_Guilds_ClassRoomId",
                table: "Guilds",
                column: "ClassRoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Guilds_GuildId",
                table: "Students",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Students_Guilds_GuildId",
                table: "Students");

            migrationBuilder.DropTable(
                name: "Guilds");

            migrationBuilder.DropIndex(
                name: "IX_Students_GuildId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "Students");
        }
    }
}
