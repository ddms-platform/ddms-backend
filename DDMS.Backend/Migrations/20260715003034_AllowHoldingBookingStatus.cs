using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DDMS.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AllowHoldingBookingStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Cho phép giá trị status = 'holding' (giữ chỗ) trong check constraint.
            // Giữ nguyên các giá trị cũ (gồm 'checked_in' của nhóm).
            migrationBuilder.Sql("ALTER TABLE bookings DROP CHECK chk_booking_status;");
            migrationBuilder.Sql(
                "ALTER TABLE bookings ADD CONSTRAINT chk_booking_status " +
                "CHECK (status IN ('pending','confirmed','paid','completed','cancelled','checked_in','holding'));");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE bookings DROP CHECK chk_booking_status;");
            migrationBuilder.Sql(
                "ALTER TABLE bookings ADD CONSTRAINT chk_booking_status " +
                "CHECK (status IN ('pending','confirmed','paid','completed','cancelled','checked_in'));");
        }
    }
}
