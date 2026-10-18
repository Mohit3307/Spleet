using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spleet.Migrations
{
    /// <inheritdoc />
    public partial class AddExpenseContributions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExpenseContributions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExpenseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseContributions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpenseContributions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExpenseContributions_Expenses_ExpenseId",
                        column: x => x.ExpenseId,
                        principalTable: "Expenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseContributions_ExpenseId",
                table: "ExpenseContributions",
                column: "ExpenseId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseContributions_UserId",
                table: "ExpenseContributions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExpenseContributions");
        }
    }
}
