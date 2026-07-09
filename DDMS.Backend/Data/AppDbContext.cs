using System;
using System.Collections.Generic;
using DDMS.Backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ai_conversation> ai_conversations { get; set; }

    public virtual DbSet<ai_message> ai_messages { get; set; }

    public virtual DbSet<audit_log> audit_logs { get; set; }

    public virtual DbSet<boat> boats { get; set; }

    public virtual DbSet<boat_cabin> boat_cabins { get; set; }

    public virtual DbSet<boat_image> boat_images { get; set; }

    public virtual DbSet<boat_maintenance> boat_maintenances { get; set; }

    public virtual DbSet<boat_service> boat_services { get; set; }

    public virtual DbSet<booking> bookings { get; set; }

    public virtual DbSet<booking_cabin> booking_cabins { get; set; }

    public virtual DbSet<booking_service> booking_services { get; set; }

    public virtual DbSet<conversation> conversations { get; set; }

    public virtual DbSet<conversation_member> conversation_members { get; set; }

    public virtual DbSet<email_verification_token> email_verification_tokens { get; set; }

    public virtual DbSet<dock> docks { get; set; }

    public virtual DbSet<dock_schedule> dock_schedules { get; set; }

    public virtual DbSet<faq> faqs { get; set; }

    public virtual DbSet<loyalty_point> loyalty_points { get; set; }

    public virtual DbSet<message> messages { get; set; }

    public virtual DbSet<notification> notifications { get; set; }

    public virtual DbSet<notification_recipient> notification_recipients { get; set; }

    public virtual DbSet<owner_profile> owner_profiles { get; set; }

    public virtual DbSet<payment> payments { get; set; }

    public virtual DbSet<owner_payment> owner_payments { get; set; }

    public virtual DbSet<promotion> promotions { get; set; }

    public virtual DbSet<refresh_token> refresh_tokens { get; set; }

    public virtual DbSet<review> reviews { get; set; }

    public virtual DbSet<role> roles { get; set; }

    public virtual DbSet<route> routes { get; set; }

    public virtual DbSet<tour> tours { get; set; }

    public virtual DbSet<tour_image> tour_images { get; set; }

    public virtual DbSet<tour_schedule> tour_schedules { get; set; }

    public virtual DbSet<user> users { get; set; }

    public virtual DbSet<user_role> user_roles { get; set; }

    public virtual DbSet<v_booking_detail> v_booking_details { get; set; }

    public virtual DbSet<v_dashboard> v_dashboards { get; set; }

    public virtual DbSet<v_loyalty_balance> v_loyalty_balances { get; set; }

    public virtual DbSet<v_revenue_stat> v_revenue_stats { get; set; }

    public virtual DbSet<v_top_tour> v_top_tours { get; set; }

    public virtual DbSet<v_unread_message> v_unread_messages { get; set; }

    public virtual DbSet<v_unread_notification> v_unread_notifications { get; set; }

    public virtual DbSet<wishlist> wishlists { get; set; }

    public virtual DbSet<boat_type> boat_types { get; set; }

    public virtual DbSet<port_maintenance_service> port_maintenance_services { get; set; }

    public virtual DbSet<user_wallet> user_wallets { get; set; }

    public virtual DbSet<wallet_withdrawal> wallet_withdrawals { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_unicode_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<boat_type>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");
            entity.HasData(
                new boat_type { id = 1, code = "catamaran", name_vi = "Thuyền hai thân", name_en = "Catamaran" },
                new boat_type { id = 2, code = "fishing_boat", name_vi = "Thuyền đánh cá", name_en = "Fishing Boat" },
                new boat_type { id = 3, code = "speedboat", name_vi = "Cano", name_en = "Speedboat" },
                new boat_type { id = 4, code = "cruiser", name_vi = "Tàu du lịch cỡ vừa", name_en = "Medium Cruiser" },
                new boat_type { id = 5, code = "yacht", name_vi = "Du thuyền", name_en = "Yacht" }
            );
        });

        modelBuilder.Entity<port_maintenance_service>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");
            entity.Property(e => e.created_at)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            
            entity.HasData(
                new port_maintenance_service 
                { 
                    id = Guid.Parse("11111111-1111-1111-1111-111111111111"), 
                    name = "Bảo trì định kỳ", 
                    icon_code = "Settings", 
                    price = 1200000,
                    created_at = new DateTime(2026, 6, 12, 3, 17, 35, 872, DateTimeKind.Utc).AddTicks(1318)
                },
                new port_maintenance_service 
                { 
                    id = Guid.Parse("22222222-2222-2222-2222-222222222222"), 
                    name = "Sửa chữa khẩn cấp", 
                    icon_code = "AlertTriangle", 
                    price = null,
                    created_at = new DateTime(2026, 6, 12, 3, 17, 35, 872, DateTimeKind.Utc).AddTicks(2434)
                },
                new port_maintenance_service 
                { 
                    id = Guid.Parse("33333333-3333-3333-3333-333333333333"), 
                    name = "Vệ sinh thân tàu", 
                    icon_code = "User", // Wait, screenshot uses a Person icon, let's stick to User
                    price = 500000,
                    created_at = new DateTime(2026, 6, 12, 3, 17, 35, 872, DateTimeKind.Utc).AddTicks(2438)
                },
                new port_maintenance_service 
                { 
                    id = Guid.Parse("44444444-4444-4444-4444-444444444444"), 
                    name = "Kiểm tra hệ thống điện", 
                    icon_code = "Zap", 
                    price = 300000,
                    created_at = new DateTime(2026, 6, 12, 3, 17, 35, 872, DateTimeKind.Utc).AddTicks(2440)
                }
            );
        });

        modelBuilder.Entity<ai_conversation>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.HasIndex(e => e.user_id, "idx_ai_conv_user");

            entity.Property(e => e.created_at)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.title).HasMaxLength(255);
            entity.Property(e => e.updated_at)
                .HasMaxLength(6)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            entity.HasOne(d => d.user).WithMany(p => p.ai_conversations)
                .HasForeignKey(d => d.user_id)
                .HasConstraintName("fk_ai_conversations_user");
        });

        modelBuilder.Entity<ai_message>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.HasIndex(e => new { e.ai_conversation_id, e.created_at }, "idx_ai_msg_conv");

            entity.Property(e => e.content).HasColumnType("text");
            entity.Property(e => e.created_at)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.role).HasMaxLength(20);

            entity.HasOne(d => d.ai_conversation).WithMany(p => p.ai_messages)
                .HasForeignKey(d => d.ai_conversation_id)
                .HasConstraintName("fk_ai_messages_conversation");
        });

        modelBuilder.Entity<audit_log>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.HasIndex(e => e.user_id, "fk_audit_user");

            entity.HasIndex(e => new { e.table_name, e.record_id }, "idx_audit_table_record");

            entity.Property(e => e.action).HasMaxLength(20);
            entity.Property(e => e.created_at)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.ip_address).HasMaxLength(45);
            entity.Property(e => e.new_values).HasColumnType("json");
            entity.Property(e => e.old_values).HasColumnType("json");
            entity.Property(e => e.record_id).HasMaxLength(100);
            entity.Property(e => e.table_name).HasMaxLength(100);

            entity.HasOne(d => d.user).WithMany(p => p.audit_logs)
                .HasForeignKey(d => d.user_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_audit_user");
        });

        modelBuilder.Entity<boat>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.HasIndex(e => e.owner_id, "idx_boats_owner");

            entity.Property(e => e.created_at)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.name).HasMaxLength(255);
            entity.Property(e => e.status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'idle'");
            entity.Property(e => e.type).HasMaxLength(100);
            entity.Property(e => e.length).HasPrecision(10, 2);
            entity.Property(e => e.beam).HasPrecision(10, 2);
            entity.Property(e => e.registration_number).HasMaxLength(100);
            entity.Property(e => e.mooring_type).HasMaxLength(50);
            entity.Property(e => e.document_url).HasMaxLength(1000);
            entity.Property(e => e.updated_at)
                .HasMaxLength(6)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            entity.HasOne(d => d.owner).WithMany()
                .HasForeignKey(d => d.owner_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_boats_owner");

            entity.HasQueryFilter(b => !b.is_deleted);
            entity.Property(e => e.is_deleted)
                .HasDefaultValue(false);
        });

        modelBuilder.Entity<boat_cabin>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.HasIndex(e => e.boat_id, "idx_cabins_boat");

            entity.Property(e => e.capacity).HasDefaultValueSql("'2'");
            entity.Property(e => e.created_at)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.description).HasColumnType("text");
            entity.Property(e => e.name).HasMaxLength(100);
            entity.Property(e => e.price).HasPrecision(12, 2);
            entity.Property(e => e.total_rooms).HasDefaultValueSql("'1'");
            entity.Property(e => e.updated_at)
                .HasMaxLength(6)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            entity.HasOne(d => d.boat).WithMany(p => p.boat_cabins)
                .HasForeignKey(d => d.boat_id)
                .HasConstraintName("fk_cabins_boat");
        });

        modelBuilder.Entity<boat_image>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.HasIndex(e => e.boat_id, "idx_boat_images_boat");

            entity.Property(e => e.caption).HasMaxLength(255);
            entity.Property(e => e.created_at)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.image_url).HasColumnType("text");
            entity.Property(e => e.public_id).HasMaxLength(255);

            entity.HasOne(d => d.boat).WithMany(p => p.boat_images)
                .HasForeignKey(d => d.boat_id)
                .HasConstraintName("fk_boat_images_boat");
        });

        modelBuilder.Entity<boat_maintenance>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.HasIndex(e => new { e.boat_id, e.start_time, e.end_time }, "idx_maintenance_boat");

            entity.Property(e => e.created_at)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.end_time).HasMaxLength(6);
            entity.Property(e => e.reason).HasMaxLength(255);
            entity.Property(e => e.start_time).HasMaxLength(6);
            entity.Property(e => e.status).HasMaxLength(20).HasDefaultValueSql("'pending'");

            entity.HasOne(d => d.boat).WithMany(p => p.boat_maintenances)
                .HasForeignKey(d => d.boat_id)
                .HasConstraintName("fk_maintenance_boat");

            entity.HasOne(d => d.port_maintenance_service)
                .WithMany()
                .HasForeignKey(d => d.port_maintenance_service_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_maintenance_port_service");

            entity.HasQueryFilter(m => !m.is_deleted);
            entity.Property(e => e.is_deleted)
                .HasDefaultValue(false);
        });

        modelBuilder.Entity<boat_service>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.HasIndex(e => new { e.boat_id, e.is_active }, "idx_services_active");

            entity.HasIndex(e => e.boat_id, "idx_services_boat");

            entity.Property(e => e.created_at)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.description).HasColumnType("text");
            entity.Property(e => e.is_active)
                .IsRequired()
                .HasDefaultValueSql("'1'");
            entity.Property(e => e.name).HasMaxLength(150);
            entity.Property(e => e.price).HasPrecision(12, 2);
            entity.Property(e => e.updated_at)
                .HasMaxLength(6)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            entity.HasOne(d => d.boat).WithMany(p => p.boat_services)
                .HasForeignKey(d => d.boat_id)
                .HasConstraintName("fk_services_boat");
        });

        modelBuilder.Entity<booking>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.HasIndex(e => e.promotion_id, "fk_bookings_promotion");

            entity.HasIndex(e => e.created_at, "idx_bookings_created").IsDescending();

            entity.HasIndex(e => e.schedule_id, "idx_bookings_sched");

            entity.HasIndex(e => e.status, "idx_bookings_status");

            entity.HasIndex(e => e.user_id, "idx_bookings_user");

            entity.Property(e => e.base_price).HasPrecision(12, 2);
            entity.Property(e => e.cabin_price).HasPrecision(12, 2);
            entity.Property(e => e.cancel_reason).HasColumnType("text");
            entity.Property(e => e.cancelled_at).HasMaxLength(6);
            entity.Property(e => e.hold_expired_at).HasMaxLength(6);
            entity.Property(e => e.hold_reminder_sent).HasDefaultValue(false);
            entity.HasIndex(e => e.hold_expired_at, "idx_bookings_hold_expired");
            entity.Property(e => e.created_at)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.discount_amount).HasPrecision(12, 2);
            entity.Property(e => e.notes).HasColumnType("text");
            entity.Property(e => e.service_price).HasPrecision(12, 2);
            entity.Property(e => e.status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'pending'");
            entity.Property(e => e.total_price).HasPrecision(12, 2);
            entity.Property(e => e.updated_at)
                .HasMaxLength(6)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            entity.HasOne(d => d.promotion).WithMany(p => p.bookings)
                .HasForeignKey(d => d.promotion_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_bookings_promotion");

            entity.HasOne(d => d.schedule).WithMany(p => p.bookings)
                .HasForeignKey(d => d.schedule_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_bookings_schedule");

            entity.HasOne(d => d.user).WithMany(p => p.bookings)
                .HasForeignKey(d => d.user_id)
                .HasConstraintName("fk_bookings_user");
        });

        modelBuilder.Entity<booking_cabin>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.HasIndex(e => e.cabin_id, "fk_booking_cabins_cabin");

            entity.HasIndex(e => e.booking_id, "idx_bk_cabins");

            entity.Property(e => e.created_at)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.quantity).HasDefaultValueSql("'1'");
            entity.Property(e => e.unit_price).HasPrecision(12, 2);

            entity.HasOne(d => d.booking).WithMany(p => p.booking_cabins)
                .HasForeignKey(d => d.booking_id)
                .HasConstraintName("fk_booking_cabins_booking");

            entity.HasOne(d => d.cabin).WithMany(p => p.booking_cabins)
                .HasForeignKey(d => d.cabin_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_booking_cabins_cabin");
        });

        modelBuilder.Entity<booking_service>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.HasIndex(e => e.service_id, "fk_booking_services_service");

            entity.HasIndex(e => e.booking_id, "idx_bk_services");

            entity.Property(e => e.created_at)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.quantity).HasDefaultValueSql("'1'");
            entity.Property(e => e.unit_price).HasPrecision(12, 2);

            entity.HasOne(d => d.booking).WithMany(p => p.booking_services)
                .HasForeignKey(d => d.booking_id)
                .HasConstraintName("fk_booking_services_booking");

            entity.HasOne(d => d.service).WithMany(p => p.booking_services)
                .HasForeignKey(d => d.service_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_booking_services_service");
        });

        modelBuilder.Entity<conversation>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.HasIndex(e => e.booking_id, "idx_conv_booking");

            entity.HasIndex(e => e.created_by, "idx_conv_created_by");

            entity.HasIndex(e => e.schedule_id, "idx_conv_schedule");

            entity.Property(e => e.created_at)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.name).HasMaxLength(255);
            entity.Property(e => e.type)
                .HasMaxLength(20)
                .HasDefaultValueSql("'direct'");
            entity.Property(e => e.updated_at)
                .HasMaxLength(6)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            entity.HasOne(d => d.booking).WithMany(p => p.conversations)
                .HasForeignKey(d => d.booking_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_conversations_booking");

            entity.HasOne(d => d.created_byNavigation).WithMany(p => p.conversations)
                .HasForeignKey(d => d.created_by)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_conversations_created_by");

            entity.HasOne(d => d.schedule).WithMany(p => p.conversations)
                .HasForeignKey(d => d.schedule_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_conversations_schedule");
        });

        modelBuilder.Entity<conversation_member>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.HasIndex(e => e.user_id, "idx_conv_members");

            entity.HasIndex(e => new { e.conversation_id, e.user_id }, "uq_conv_member").IsUnique();

            entity.Property(e => e.joined_at)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.last_read_at).HasMaxLength(6);

            entity.HasOne(d => d.conversation).WithMany(p => p.conversation_members)
                .HasForeignKey(d => d.conversation_id)
                .HasConstraintName("fk_conv_members_conversation");

            entity.HasOne(d => d.user).WithMany(p => p.conversation_members)
                .HasForeignKey(d => d.user_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_conv_members_user");
        });

        modelBuilder.Entity<email_verification_token>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.HasIndex(e => new { e.email, e.purpose }, "idx_email_verify_token_email_purpose");

            entity.HasIndex(e => e.expires_at, "idx_email_verify_token_expires");

            entity.Property(e => e.created_at)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.email).HasMaxLength(255);
            entity.Property(e => e.expires_at).HasMaxLength(6);
            entity.Property(e => e.token_hash).HasMaxLength(64);
            entity.Property(e => e.purpose).HasMaxLength(20);
            entity.Property(e => e.used_at).HasMaxLength(6);
        });

        modelBuilder.Entity<dock>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.created_at)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.location).HasMaxLength(255);
            entity.Property(e => e.max_boats).HasDefaultValueSql("'1'");
            entity.Property(e => e.name).HasMaxLength(255);
            entity.Property(e => e.updated_at)
                .HasMaxLength(6)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        });

        modelBuilder.Entity<dock_schedule>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.HasIndex(e => e.schedule_id, "fk_dock_sched_schedule");

            entity.HasIndex(e => new { e.boat_id, e.start_time, e.end_time }, "idx_dock_sched_boat");

            entity.HasIndex(e => new { e.dock_id, e.start_time, e.end_time }, "idx_dock_sched_dock");

            entity.Property(e => e.created_at)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.end_time).HasMaxLength(6);
            entity.Property(e => e.start_time).HasMaxLength(6);

            entity.HasOne(d => d.boat).WithMany(p => p.dock_schedules)
                .HasForeignKey(d => d.boat_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_dock_sched_boat");

            entity.HasOne(d => d.dock).WithMany(p => p.dock_schedules)
                .HasForeignKey(d => d.dock_id)
                .HasConstraintName("fk_dock_sched_dock");

            entity.HasOne(d => d.schedule).WithMany(p => p.dock_schedules)
                .HasForeignKey(d => d.schedule_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_dock_sched_schedule");
        });

        modelBuilder.Entity<faq>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.HasIndex(e => e.tour_id, "idx_faqs_tour");

            entity.Property(e => e.answer).HasColumnType("text");
            entity.Property(e => e.created_at)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.question).HasColumnType("text");
            entity.Property(e => e.updated_at)
                .HasMaxLength(6)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            entity.HasOne(d => d.tour).WithMany(p => p.faqs)
                .HasForeignKey(d => d.tour_id)
                .HasConstraintName("fk_faqs_tour");
        });

        modelBuilder.Entity<loyalty_point>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.HasIndex(e => e.booking_id, "idx_loyalty_booking");

            entity.HasIndex(e => e.user_id, "idx_loyalty_user");

            entity.Property(e => e.created_at)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.note).HasMaxLength(255);
            entity.Property(e => e.type).HasMaxLength(20);

            entity.HasOne(d => d.booking).WithMany(p => p.loyalty_points)
                .HasForeignKey(d => d.booking_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_loyalty_booking");

            entity.HasOne(d => d.user).WithMany(p => p.loyalty_points)
                .HasForeignKey(d => d.user_id)
                .HasConstraintName("fk_loyalty_user");
        });

        modelBuilder.Entity<message>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.HasIndex(e => new { e.conversation_id, e.created_at }, "idx_messages_conv").IsDescending(false, true);

            entity.HasIndex(e => e.sender_id, "idx_messages_sender");

            entity.Property(e => e.body).HasColumnType("text");
            entity.Property(e => e.created_at)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.updated_at)
                .HasMaxLength(6)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            entity.HasOne(d => d.conversation).WithMany(p => p.messages)
                .HasForeignKey(d => d.conversation_id)
                .HasConstraintName("fk_messages_conversation");

            entity.HasOne(d => d.sender).WithMany(p => p.messages)
                .HasForeignKey(d => d.sender_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_messages_sender");
        });

        modelBuilder.Entity<notification>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.HasIndex(e => e.sender_id, "fk_notifications_sender");

            entity.HasIndex(e => e.created_at, "idx_notif_created").IsDescending();

            entity.Property(e => e.body).HasColumnType("text");
            entity.Property(e => e.created_at)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.data).HasColumnType("text");
            entity.Property(e => e.title).HasMaxLength(255);
            entity.Property(e => e.type).HasMaxLength(20);

            entity.HasOne(d => d.sender).WithMany(p => p.notifications)
                .HasForeignKey(d => d.sender_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_notifications_sender");
        });

        modelBuilder.Entity<notification_recipient>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.HasIndex(e => new { e.user_id, e.is_read }, "idx_notif_recipient");

            entity.HasIndex(e => new { e.notification_id, e.user_id }, "uq_notif_user").IsUnique();

            entity.Property(e => e.created_at)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.read_at).HasMaxLength(6);

            entity.HasOne(d => d.notification).WithMany(p => p.notification_recipients)
                .HasForeignKey(d => d.notification_id)
                .HasConstraintName("fk_notif_recipients_notification");

            entity.HasOne(d => d.user).WithMany(p => p.notification_recipients)
                .HasForeignKey(d => d.user_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_notif_recipients_user");
        });

        modelBuilder.Entity<owner_profile>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.HasIndex(e => e.is_verified, "idx_owner_verified");

            entity.HasIndex(e => e.user_id, "user_id").IsUnique();

            entity.Property(e => e.address).HasColumnType("text");
            entity.Property(e => e.bio).HasColumnType("text");
            entity.Property(e => e.business_name).HasMaxLength(255);
            entity.Property(e => e.created_at)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.license_image).HasColumnType("text");
            entity.Property(e => e.license_number).HasMaxLength(100);
            entity.Property(e => e.status).HasMaxLength(20).HasDefaultValueSql("'Pending'");
            entity.Property(e => e.phone_business).HasMaxLength(20);
            entity.Property(e => e.updated_at)
                .HasMaxLength(6)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.verified_at).HasMaxLength(6);

            entity.HasOne(d => d.user).WithOne(p => p.owner_profile)
                .HasForeignKey<owner_profile>(d => d.user_id)
                .HasConstraintName("fk_owner_profiles_user");
        });

        modelBuilder.Entity<payment>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.HasIndex(e => e.booking_id, "booking_id").IsUnique();

            entity.HasIndex(e => e.status, "idx_payments_status");

            entity.Property(e => e.amount).HasPrecision(12, 2);
            entity.Property(e => e.created_at)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.gateway_response).HasColumnType("text");
            entity.Property(e => e.method).HasMaxLength(20);
            entity.Property(e => e.paid_at).HasMaxLength(6);
            entity.Property(e => e.status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'pending'");
            entity.Property(e => e.transaction_ref).HasMaxLength(255);
            entity.Property(e => e.updated_at)
                .HasMaxLength(6)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            entity.HasOne(d => d.booking).WithOne(p => p.payment)
                .HasForeignKey<payment>(d => d.booking_id)
                .HasConstraintName("fk_payments_booking");
        });

        modelBuilder.Entity<promotion>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.HasIndex(e => e.code, "code").IsUnique();

            entity.HasIndex(e => e.created_by, "fk_promotions_created_by");

            entity.HasIndex(e => new { e.is_active, e.valid_from, e.valid_until }, "idx_promotions_active");

            entity.Property(e => e.code).HasMaxLength(50);
            entity.Property(e => e.created_at)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.description).HasMaxLength(255);
            entity.Property(e => e.discount_type)
                .HasMaxLength(20)
                .HasDefaultValueSql("'percent'");
            entity.Property(e => e.discount_value).HasPrecision(12, 2);
            entity.Property(e => e.is_active)
                .IsRequired()
                .HasDefaultValueSql("'1'");
            entity.Property(e => e.status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'approved'");
            entity.Property(e => e.max_discount).HasPrecision(12, 2);
            entity.Property(e => e.min_order_value).HasPrecision(12, 2);
            entity.Property(e => e.updated_at)
                .HasMaxLength(6)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.valid_from).HasMaxLength(6);
            entity.Property(e => e.valid_until).HasMaxLength(6);

            entity.HasOne(d => d.created_byNavigation).WithMany(p => p.promotions)
                .HasForeignKey(d => d.created_by)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_promotions_created_by");
        });

        modelBuilder.Entity<refresh_token>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.HasIndex(e => new { e.user_id, e.revoked }, "idx_refresh_active");

            entity.HasIndex(e => e.user_id, "idx_refresh_user");

            entity.HasIndex(e => e.token_hash, "token_hash").IsUnique();

            entity.Property(e => e.created_at)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.expires_at).HasMaxLength(6);
            entity.Property(e => e.ip_address).HasMaxLength(45);
            entity.Property(e => e.revoked_at).HasMaxLength(6);
            entity.Property(e => e.user_agent).HasMaxLength(255);

            entity.HasOne(d => d.user).WithMany(p => p.refresh_tokens)
                .HasForeignKey(d => d.user_id)
                .HasConstraintName("fk_refresh_tokens_user");
        });

        modelBuilder.Entity<review>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.HasIndex(e => e.booking_id, "booking_id").IsUnique();

            entity.HasIndex(e => e.tour_id, "idx_reviews_tour");

            entity.HasIndex(e => e.user_id, "idx_reviews_user");

            entity.Property(e => e.comment).HasColumnType("text");
            entity.Property(e => e.image_urls).HasColumnType("json");
            entity.Property(e => e.video_urls).HasColumnType("json");
            entity.Property(e => e.created_at)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.updated_at)
                .HasMaxLength(6)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            entity.HasOne(d => d.booking).WithOne(p => p.review)
                .HasForeignKey<review>(d => d.booking_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_reviews_booking");

            entity.HasOne(d => d.tour).WithMany(p => p.reviews)
                .HasForeignKey(d => d.tour_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_reviews_tour");

            entity.HasOne(d => d.user).WithMany(p => p.reviews)
                .HasForeignKey(d => d.user_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_reviews_user");
        });

        modelBuilder.Entity<role>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.HasIndex(e => e.name, "name").IsUnique();

            entity.Property(e => e.created_at)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.description).HasMaxLength(255);
            entity.Property(e => e.name).HasMaxLength(50);
        });

        modelBuilder.Entity<route>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.HasIndex(e => e.tour_id, "fk_routes_tour");

            entity.Property(e => e.created_at)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.description).HasColumnType("text");
            entity.Property(e => e.end_point).HasMaxLength(255);
            entity.Property(e => e.name).HasMaxLength(255);
            entity.Property(e => e.start_point).HasMaxLength(255);

            entity.HasOne(d => d.tour).WithMany(p => p.routes)
                .HasForeignKey(d => d.tour_id)
                .HasConstraintName("fk_routes_tour");
        });

        modelBuilder.Entity<tour>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.HasIndex(e => e.created_by, "fk_tours_created_by");

            entity.HasIndex(e => e.location, "idx_tours_location");

            entity.HasIndex(e => e.price, "idx_tours_price");

            entity.HasIndex(e => e.avg_rating, "idx_tours_rating").IsDescending();

            entity.HasIndex(e => e.status, "idx_tours_status");

            entity.Property(e => e.avg_rating).HasPrecision(3, 2);
            entity.Property(e => e.cancel_policy)
                .HasMaxLength(20)
                .HasDefaultValueSql("'free'");
            entity.Property(e => e.created_at)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.description).HasColumnType("text");
            entity.Property(e => e.name).HasMaxLength(255);
            entity.Property(e => e.price).HasPrecision(12, 2);
            entity.Property(e => e.status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'active'");
            entity.Property(e => e.updated_at)
                .HasMaxLength(6)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            entity.HasOne(d => d.created_byNavigation).WithMany(p => p.tours)
                .HasForeignKey(d => d.created_by)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_tours_created_by");
        });

        modelBuilder.Entity<tour_image>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.HasIndex(e => e.tour_id, "fk_tour_images_tour");

            entity.Property(e => e.caption).HasMaxLength(255);
            entity.Property(e => e.created_at)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.image_url).HasColumnType("text");
            entity.Property(e => e.public_id).HasMaxLength(255);

            entity.HasOne(d => d.tour).WithMany(p => p.tour_images)
                .HasForeignKey(d => d.tour_id)
                .HasConstraintName("fk_tour_images_tour");
        });

        modelBuilder.Entity<tour_schedule>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.HasIndex(e => e.boat_id, "idx_schedules_boat");

            entity.HasIndex(e => e.dock_id, "idx_schedules_dock");

            entity.HasIndex(e => e.status, "idx_schedules_status");

            entity.HasIndex(e => new { e.start_time, e.end_time }, "idx_schedules_time");

            entity.HasIndex(e => e.tour_id, "idx_schedules_tour");

            entity.Property(e => e.created_at)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.end_time).HasMaxLength(6);
            entity.Property(e => e.start_time).HasMaxLength(6);
            entity.Property(e => e.status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'scheduled'");
            entity.Property(e => e.updated_at)
                .HasMaxLength(6)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            entity.HasOne(d => d.boat).WithMany(p => p.tour_schedules)
                .HasForeignKey(d => d.boat_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_schedules_boat");

            entity.HasOne(d => d.dock).WithMany(p => p.tour_schedules)
                .HasForeignKey(d => d.dock_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_schedules_dock");

            entity.HasOne(d => d.tour).WithMany(p => p.tour_schedules)
                .HasForeignKey(d => d.tour_id)
                .HasConstraintName("fk_schedules_tour");
        });

        modelBuilder.Entity<user>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.HasIndex(e => e.email, "email").IsUnique();

            entity.HasIndex(e => e.google_id, "google_id").IsUnique();

            entity.Property(e => e.avatar_url).HasColumnType("text");
            entity.Property(e => e.created_at)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.full_name).HasMaxLength(150);
            entity.Property(e => e.is_active)
                .IsRequired()
                .HasDefaultValueSql("'1'");
            entity.Property(e => e.email_verified_at).HasMaxLength(6);
            entity.Property(e => e.password_hash).HasMaxLength(255);
            entity.Property(e => e.phone).HasMaxLength(20);
            entity.Property(e => e.address).HasMaxLength(500);
            entity.Property(e => e.updated_at)
                .HasMaxLength(6)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        });

        modelBuilder.Entity<user_role>(entity =>
        {
            entity.HasKey(e => new { e.user_id, e.role_id })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.HasIndex(e => e.role_id, "fk_user_roles_role");

            entity.Property(e => e.assigned_at)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            entity.HasOne(d => d.role).WithMany(p => p.user_roles)
                .HasForeignKey(d => d.role_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_user_roles_role");

            entity.HasOne(d => d.user).WithMany(p => p.user_roles)
                .HasForeignKey(d => d.user_id)
                .HasConstraintName("fk_user_roles_user");
        });

        modelBuilder.Entity<v_booking_detail>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_booking_detail");

            entity.Property(e => e.base_price).HasPrecision(12, 2);
            entity.Property(e => e.boat_name).HasMaxLength(255);
            entity.Property(e => e.booking_id).HasDefaultValueSql("uuid()");
            entity.Property(e => e.booking_status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'pending'");
            entity.Property(e => e.cabin_price).HasPrecision(12, 2);
            entity.Property(e => e.customer_email).HasMaxLength(255);
            entity.Property(e => e.customer_name).HasMaxLength(150);
            entity.Property(e => e.discount_amount).HasPrecision(12, 2);
            entity.Property(e => e.end_time).HasMaxLength(6);
            entity.Property(e => e.payment_method).HasMaxLength(20);
            entity.Property(e => e.payment_status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'pending'");
            entity.Property(e => e.service_price).HasPrecision(12, 2);
            entity.Property(e => e.start_time).HasMaxLength(6);
            entity.Property(e => e.total_price).HasPrecision(12, 2);
            entity.Property(e => e.tour_name).HasMaxLength(255);
        });

        modelBuilder.Entity<v_dashboard>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_dashboard");

            entity.Property(e => e.total_revenue).HasPrecision(34, 2);
        });

        modelBuilder.Entity<v_loyalty_balance>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_loyalty_balance");

            entity.Property(e => e.total_points).HasPrecision(32);
        });

        modelBuilder.Entity<v_revenue_stat>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_revenue_stats");

            entity.Property(e => e.total_revenue).HasPrecision(34, 2);
        });

        modelBuilder.Entity<v_top_tour>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_top_tours");

            entity.Property(e => e.avg_rating).HasPrecision(3, 2);
            entity.Property(e => e.id).HasDefaultValueSql("uuid()");
            entity.Property(e => e.name).HasMaxLength(255);
            entity.Property(e => e.total_revenue).HasPrecision(34, 2);
        });

        modelBuilder.Entity<v_unread_message>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_unread_messages");
        });

        modelBuilder.Entity<v_unread_notification>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_unread_notifications");
        });

        modelBuilder.Entity<wishlist>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.HasIndex(e => e.tour_id, "fk_wishlists_tour");

            entity.HasIndex(e => e.user_id, "idx_wishlists_user");

            entity.HasIndex(e => new { e.user_id, e.tour_id }, "uq_wishlist").IsUnique();

            entity.Property(e => e.created_at)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            entity.HasOne(d => d.tour).WithMany(p => p.wishlists)
                .HasForeignKey(d => d.tour_id)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_wishlists_tour");

            entity.HasOne(d => d.user).WithMany(p => p.wishlists)
                .HasForeignKey(d => d.user_id)
                .HasConstraintName("fk_wishlists_user");
        });

        modelBuilder.Entity<owner_payment>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.owner_id)
                .HasCharSet("utf8mb4")
                .UseCollation("utf8mb4_unicode_ci");
            
            entity.HasOne(d => d.owner)
                .WithMany()
                .HasForeignKey(d => d.owner_id)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_owner_payments_owner");
        });

        modelBuilder.Entity<user_wallet>(entity =>
        {
            entity.HasKey(e => e.id);
            entity.Property(e => e.id)
                .HasColumnType("char(36)")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.user_id)
                .HasColumnType("char(36)")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.balance).HasPrecision(12, 2);
            entity.Property(e => e.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.updated_at).HasDefaultValueSql("CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)");
            entity.HasOne(d => d.user)
                .WithOne()
                .HasForeignKey<user_wallet>(d => d.user_id)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<wallet_withdrawal>(entity =>
        {
            entity.HasKey(e => e.id);
            entity.Property(e => e.id)
                .HasColumnType("char(36)")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.user_id)
                .HasColumnType("char(36)")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.amount).HasPrecision(12, 2);
            entity.Property(e => e.bank_name).HasMaxLength(100);
            entity.Property(e => e.account_number).HasMaxLength(50);
            entity.Property(e => e.account_name).HasMaxLength(100);
            entity.Property(e => e.status).HasMaxLength(20).HasDefaultValueSql("'pending'");
            entity.Property(e => e.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.HasOne(d => d.user)
                .WithMany()
                .HasForeignKey(d => d.user_id)
                .OnDelete(DeleteBehavior.Cascade);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
