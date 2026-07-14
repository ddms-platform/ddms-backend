using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DDMS.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckedInBookingStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE `bookings` DROP CHECK `chk_booking_status`;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE `bookings`
                ADD CONSTRAINT `chk_booking_status`
                CHECK (`status` IN (
                    'pending',
                    'confirmed',
                    'paid',
                    'completed',
                    'cancelled',
                    'checked_in'
                ));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE `bookings` DROP CHECK `chk_booking_status`;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE `bookings`
                ADD CONSTRAINT `chk_booking_status`
                CHECK (`status` IN (
                    'pending',
                    'confirmed',
                    'paid',
                    'completed',
                    'cancelled'
                ));
                """);
        }
    }
}
