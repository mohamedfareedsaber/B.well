using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BWell.Loyalty.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVisitFamilyMember : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FamilyMemberId",
                table: "Visits",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Visits_FamilyMemberId",
                table: "Visits",
                column: "FamilyMemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_Visits_FamilyMembers_FamilyMemberId",
                table: "Visits",
                column: "FamilyMemberId",
                principalTable: "FamilyMembers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Visits_FamilyMembers_FamilyMemberId",
                table: "Visits");

            migrationBuilder.DropIndex(
                name: "IX_Visits_FamilyMemberId",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "FamilyMemberId",
                table: "Visits");
        }
    }
}
