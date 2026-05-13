using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineSinavPortali.API.Migrations
{
    /// <inheritdoc />
    public partial class SinavSilmeDuzeltme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sonuclar_Sinavlar_SinavId",
                table: "Sonuclar");

            migrationBuilder.AddForeignKey(
                name: "FK_Sonuclar_Sinavlar_SinavId",
                table: "Sonuclar",
                column: "SinavId",
                principalTable: "Sinavlar",
                principalColumn: "SinavId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sonuclar_Sinavlar_SinavId",
                table: "Sonuclar");

            migrationBuilder.AddForeignKey(
                name: "FK_Sonuclar_Sinavlar_SinavId",
                table: "Sonuclar",
                column: "SinavId",
                principalTable: "Sinavlar",
                principalColumn: "SinavId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
