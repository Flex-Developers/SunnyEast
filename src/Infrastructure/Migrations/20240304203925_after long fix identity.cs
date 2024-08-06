using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class afterlongfixidentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShopOrders_Products_ProductId",
                table: "ShopOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_ShopOrders_Shops_ShopId",
                table: "ShopOrders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ShopOrders",
                table: "ShopOrders");

            migrationBuilder.RenameTable(
                name: "ShopOrders",
                newName: "ShopOrder");

            migrationBuilder.RenameIndex(
                name: "IX_ShopOrders_ShopId",
                table: "ShopOrder",
                newName: "IX_ShopOrder_ShopId");

            migrationBuilder.RenameIndex(
                name: "IX_ShopOrders_ProductId",
                table: "ShopOrder",
                newName: "IX_ShopOrder_ProductId");

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Orders",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ShopOrder",
                table: "ShopOrder",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ShopOrder_Products_ProductId",
                table: "ShopOrder",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ShopOrder_Shops_ShopId",
                table: "ShopOrder",
                column: "ShopId",
                principalTable: "Shops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShopOrder_Products_ProductId",
                table: "ShopOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_ShopOrder_Shops_ShopId",
                table: "ShopOrder");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ShopOrder",
                table: "ShopOrder");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Orders");

            migrationBuilder.RenameTable(
                name: "ShopOrder",
                newName: "ShopOrders");

            migrationBuilder.RenameIndex(
                name: "IX_ShopOrder_ShopId",
                table: "ShopOrders",
                newName: "IX_ShopOrders_ShopId");

            migrationBuilder.RenameIndex(
                name: "IX_ShopOrder_ProductId",
                table: "ShopOrders",
                newName: "IX_ShopOrders_ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ShopOrders",
                table: "ShopOrders",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ShopOrders_Products_ProductId",
                table: "ShopOrders",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ShopOrders_Shops_ShopId",
                table: "ShopOrders",
                column: "ShopId",
                principalTable: "Shops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
