using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DDMS.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddBlogPosts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_blog_posts",
                table: "blog_posts");

            migrationBuilder.AddPrimaryKey(
                name: "PRIMARY",
                table: "blog_posts",
                column: "id");

            migrationBuilder.CreateTable(
                name: "booking_payment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    booking_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    amount_paid = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    payos_order_code = table.Column<long>(type: "bigint", nullable: false),
                    description = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    checkout_url = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    paid_at = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                    table.ForeignKey(
                        name: "fk_booking_payments_booking",
                        column: x => x.booking_id,
                        principalTable: "bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.CreateIndex(
                name: "IX_blog_posts_slug",
                table: "blog_posts",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_blog_posts_source_hash",
                table: "blog_posts",
                column: "source_hash");

            migrationBuilder.CreateIndex(
                name: "IX_blog_posts_status_published_at",
                table: "blog_posts",
                columns: new[] { "status", "published_at" });

            migrationBuilder.CreateIndex(
                name: "IX_booking_payment_booking_id",
                table: "booking_payment",
                column: "booking_id");

            migrationBuilder.CreateIndex(
                name: "IX_booking_payment_payos_order_code",
                table: "booking_payment",
                column: "payos_order_code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "booking_payment");

            migrationBuilder.DropPrimaryKey(
                name: "PRIMARY",
                table: "blog_posts");

            migrationBuilder.DropIndex(
                name: "IX_blog_posts_slug",
                table: "blog_posts");

            migrationBuilder.DropIndex(
                name: "IX_blog_posts_source_hash",
                table: "blog_posts");

            migrationBuilder.DropIndex(
                name: "IX_blog_posts_status_published_at",
                table: "blog_posts");

            migrationBuilder.AddPrimaryKey(
                name: "PK_blog_posts",
                table: "blog_posts",
                column: "id");
        }
    }
}
