using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcademiaTennisDAL.Migrations
{
    /// <inheritdoc />
    public partial class AgregaUserIdAProfesor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Profesores",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Profesores_UserId",
                table: "Profesores",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Profesores_AspNetUsers_UserId",
                table: "Profesores",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Profesores_AspNetUsers_UserId",
                table: "Profesores");

            migrationBuilder.DropIndex(
                name: "IX_Profesores_UserId",
                table: "Profesores");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Profesores");
        }
    }
}
