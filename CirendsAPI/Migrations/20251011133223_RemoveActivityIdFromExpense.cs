using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CirendsAPI.Migrations
{
    /// <inheritdoc />
    public partial class RemoveActivityIdFromExpense : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_Activities_ActivityId",
                table: "Expenses");

            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_Tasks_TaskId",
                table: "Expenses");

            migrationBuilder.DropIndex(
                name: "IX_Expenses_ActivityId",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "ActivityId",
                table: "Expenses");

            migrationBuilder.AlterColumn<int>(
                name: "TaskId",
                table: "Expenses",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_Tasks_TaskId",
                table: "Expenses",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_Tasks_TaskId",
                table: "Expenses");

            migrationBuilder.AlterColumn<int>(
                name: "TaskId",
                table: "Expenses",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "ActivityId",
                table: "Expenses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_ActivityId",
                table: "Expenses",
                column: "ActivityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_Activities_ActivityId",
                table: "Expenses",
                column: "ActivityId",
                principalTable: "Activities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_Tasks_TaskId",
                table: "Expenses",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}