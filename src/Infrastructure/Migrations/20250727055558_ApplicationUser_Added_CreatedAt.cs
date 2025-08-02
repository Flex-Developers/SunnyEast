using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ApplicationUser_Added_CreatedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "AspNetUsers",
                type: "datetime(6)",
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AspNetUsers");

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
    }
}
