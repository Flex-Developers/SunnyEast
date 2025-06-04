using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CartItemSelectedProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "SummaryPrice",
                table: "CartItems");

            migrationBuilder.AddColumn<int>(
                name: "FiveSelectedCount",
                table: "CartItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Gr1000SelectedCount",
                table: "CartItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Gr100SelectedCount",
                table: "CartItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Gr2000SelectedCount",
                table: "CartItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Gr3000SelectedCount",
                table: "CartItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Gr300SelectedCount",
                table: "CartItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Gr5000SelectedCount",
                table: "CartItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Gr500SelectedCount",
                table: "CartItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Ml1000SelectedCount",
                table: "CartItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Ml100SelectedCount",
                table: "CartItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Ml2000SelectedCount",
                table: "CartItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Ml3000SelectedCount",
                table: "CartItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Ml300SelectedCount",
                table: "CartItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Ml5000SelectedCount",
                table: "CartItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Ml500SelectedCount",
                table: "CartItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OneSelectedCount",
                table: "CartItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ThreeSelectedCount",
                table: "CartItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TwoSelectedCount",
                table: "CartItems",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FiveSelectedCount",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "Gr1000SelectedCount",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "Gr100SelectedCount",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "Gr2000SelectedCount",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "Gr3000SelectedCount",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "Gr300SelectedCount",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "Gr5000SelectedCount",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "Gr500SelectedCount",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "Ml1000SelectedCount",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "Ml100SelectedCount",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "Ml2000SelectedCount",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "Ml3000SelectedCount",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "Ml300SelectedCount",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "Ml5000SelectedCount",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "Ml500SelectedCount",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "OneSelectedCount",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "ThreeSelectedCount",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "TwoSelectedCount",
                table: "CartItems");

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "CartItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "SummaryPrice",
                table: "CartItems",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
