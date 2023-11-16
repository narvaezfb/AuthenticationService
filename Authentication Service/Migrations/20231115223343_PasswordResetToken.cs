using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Authentication_Service.Migrations
{
    /// <inheritdoc />
    public partial class PasswordResetToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResetPasswordToken",
                table: "User",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResetPasswordTokenExpiry",
                table: "User",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResetPasswordToken",
                table: "User");

            migrationBuilder.DropColumn(
                name: "ResetPasswordTokenExpiry",
                table: "User");
        }
    }
}
