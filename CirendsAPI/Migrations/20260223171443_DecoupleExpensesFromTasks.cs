using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CirendsAPI.Migrations
{
    /// <inheritdoc />
    public partial class DecoupleExpensesFromTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_Tasks_TaskId",
                table: "Expenses");

            migrationBuilder.RenameColumn(
                name: "TaskId",
                table: "Expenses",
                newName: "ActivityId");

            migrationBuilder.RenameIndex(
                name: "IX_Expenses_TaskId",
                table: "Expenses",
                newName: "IX_Expenses_ActivityId");

            migrationBuilder.Sql(
                "UPDATE \"Expenses\" e SET \"ActivityId\" = t.\"ActivityId\" FROM \"Tasks\" t WHERE e.\"ActivityId\" = t.\"Id\";");

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_Activities_ActivityId",
                table: "Expenses",
                column: "ActivityId",
                principalTable: "Activities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_Activities_ActivityId",
                table: "Expenses");

            migrationBuilder.RenameColumn(
                name: "ActivityId",
                table: "Expenses",
                newName: "TaskId");

            migrationBuilder.RenameIndex(
                name: "IX_Expenses_ActivityId",
                table: "Expenses",
                newName: "IX_Expenses_TaskId");

            migrationBuilder.Sql(
                "UPDATE \"Expenses\" e SET \"TaskId\" = (SELECT t.\"Id\" FROM \"Tasks\" t WHERE t.\"ActivityId\" = e.\"TaskId\" ORDER BY t.\"Id\" LIMIT 1);");

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_Tasks_TaskId",
                table: "Expenses",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
