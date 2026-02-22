using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project2EmailNight.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIsStatus_AddBody_ToMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsStatus",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "MessageDetail",
                table: "Messages");

            migrationBuilder.RenameColumn(
                name: "RecieverEmail",
                table: "Messages",
                newName: "ReceiverEmail");

            migrationBuilder.AddColumn<string>(
                name: "Body",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Body",
                table: "Messages");

            migrationBuilder.RenameColumn(
                name: "ReceiverEmail",
                table: "Messages",
                newName: "RecieverEmail");

            migrationBuilder.AddColumn<bool>(
                name: "IsStatus",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MessageDetail",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
