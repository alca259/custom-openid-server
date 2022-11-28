using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthServer.Migrations
{
    public partial class UpdateIdentity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_Role_Role_Role_Id",
                schema: "Identity",
                table: "User_Role");

            migrationBuilder.DropForeignKey(
                name: "FK_User_Role_User_User_Id",
                schema: "Identity",
                table: "User_Role");

            migrationBuilder.RenameColumn(
                name: "Role_Id",
                schema: "Identity",
                table: "User_Role",
                newName: "RoleId");

            migrationBuilder.RenameColumn(
                name: "User_Id",
                schema: "Identity",
                table: "User_Role",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_User_Role_Role_Id",
                schema: "Identity",
                table: "User_Role",
                newName: "IX_User_Role_RoleId");

            migrationBuilder.RenameColumn(
                name: "Normalized_Name",
                schema: "Identity",
                table: "Role",
                newName: "NormalizedName");

            migrationBuilder.RenameColumn(
                name: "Concurrency_Stamp",
                schema: "Identity",
                table: "Role",
                newName: "ConcurrencyStamp");

            migrationBuilder.CreateTable(
                name: "User_Claim",
                schema: "Identity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    User_Id = table.Column<long>(type: "bigint", nullable: false),
                    Claim_Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Claim_Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User_Claim", x => x.Id);
                    table.ForeignKey(
                        name: "FK_User_Claim_User_User_Id",
                        column: x => x.User_Id,
                        principalSchema: "Identity",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "User_Token",
                schema: "Identity",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User_Token", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_User_Token_User_UserId",
                        column: x => x.UserId,
                        principalSchema: "Identity",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_User_Claim_User_Id",
                schema: "Identity",
                table: "User_Claim",
                column: "User_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_User_Role_Role_RoleId",
                schema: "Identity",
                table: "User_Role",
                column: "RoleId",
                principalSchema: "Identity",
                principalTable: "Role",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_User_Role_User_UserId",
                schema: "Identity",
                table: "User_Role",
                column: "UserId",
                principalSchema: "Identity",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_Role_Role_RoleId",
                schema: "Identity",
                table: "User_Role");

            migrationBuilder.DropForeignKey(
                name: "FK_User_Role_User_UserId",
                schema: "Identity",
                table: "User_Role");

            migrationBuilder.DropTable(
                name: "User_Claim",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "User_Token",
                schema: "Identity");

            migrationBuilder.RenameColumn(
                name: "RoleId",
                schema: "Identity",
                table: "User_Role",
                newName: "Role_Id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                schema: "Identity",
                table: "User_Role",
                newName: "User_Id");

            migrationBuilder.RenameIndex(
                name: "IX_User_Role_RoleId",
                schema: "Identity",
                table: "User_Role",
                newName: "IX_User_Role_Role_Id");

            migrationBuilder.RenameColumn(
                name: "NormalizedName",
                schema: "Identity",
                table: "Role",
                newName: "Normalized_Name");

            migrationBuilder.RenameColumn(
                name: "ConcurrencyStamp",
                schema: "Identity",
                table: "Role",
                newName: "Concurrency_Stamp");

            migrationBuilder.AddForeignKey(
                name: "FK_User_Role_Role_Role_Id",
                schema: "Identity",
                table: "User_Role",
                column: "Role_Id",
                principalSchema: "Identity",
                principalTable: "Role",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_User_Role_User_User_Id",
                schema: "Identity",
                table: "User_Role",
                column: "User_Id",
                principalSchema: "Identity",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
