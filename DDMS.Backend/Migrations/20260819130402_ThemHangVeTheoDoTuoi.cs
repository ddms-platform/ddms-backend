using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DDMS.Backend.Migrations
{
    /// <inheritdoc />
    public partial class ThemHangVeTheoDoTuoi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "child_price_percent",
                table: "tours",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValueSql: "50.00");

            migrationBuilder.AddColumn<decimal>(
                name: "infant_price_percent",
                table: "tours",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValueSql: "0.00");

            migrationBuilder.AddColumn<int>(
                name: "num_adults",
                table: "bookings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "num_children",
                table: "bookings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "num_infants",
                table: "bookings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Đơn đặt trước tính năng này đều là người lớn. Code vẫn chạy đúng nếu
            // không backfill (PartyComposition.FromCounts có đường lùi về num_people),
            // nhưng để dữ liệu nhất quán thì báo cáo sau này khỏi phải biết tới ngoại lệ đó.
            migrationBuilder.Sql(
                "UPDATE bookings SET num_adults = num_people "
                + "WHERE num_adults = 0 AND num_children = 0 AND num_infants = 0;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "child_price_percent",
                table: "tours");

            migrationBuilder.DropColumn(
                name: "infant_price_percent",
                table: "tours");

            migrationBuilder.DropColumn(
                name: "num_adults",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "num_children",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "num_infants",
                table: "bookings");
        }
    }
}
