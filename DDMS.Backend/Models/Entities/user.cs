using System;
using System.Collections.Generic;

namespace DDMS.Backend.Models.Entities;

public partial class user
{
    public Guid id { get; set; }

    public string full_name { get; set; } = null!;

    public string email { get; set; } = null!;

    public string? password_hash { get; set; }

    public string? phone { get; set; }

    public string? address { get; set; }

    public string? avatar_url { get; set; }

    public bool? is_active { get; set; }

    public string? google_id { get; set; }

    public DateTime? email_verified_at { get; set; }

    public DateTime created_at { get; set; }

    public DateTime updated_at { get; set; }

    public virtual ICollection<ai_conversation> ai_conversations { get; set; } = new List<ai_conversation>();

    public virtual ICollection<audit_log> audit_logs { get; set; } = new List<audit_log>();

    public virtual ICollection<booking> bookings { get; set; } = new List<booking>();

    public virtual ICollection<conversation_member> conversation_members { get; set; } = new List<conversation_member>();

    public virtual ICollection<conversation> conversations { get; set; } = new List<conversation>();

    public virtual ICollection<loyalty_point> loyalty_points { get; set; } = new List<loyalty_point>();

    public virtual ICollection<message> messages { get; set; } = new List<message>();

    public virtual ICollection<notification_recipient> notification_recipients { get; set; } = new List<notification_recipient>();

    public virtual ICollection<notification> notifications { get; set; } = new List<notification>();

    public virtual owner_profile? owner_profile { get; set; }

    public virtual ICollection<promotion> promotions { get; set; } = new List<promotion>();

    public virtual ICollection<refresh_token> refresh_tokens { get; set; } = new List<refresh_token>();

    public virtual ICollection<review> reviews { get; set; } = new List<review>();

    public virtual ICollection<tour> tours { get; set; } = new List<tour>();

    public virtual ICollection<user_role> user_roles { get; set; } = new List<user_role>();

    public virtual ICollection<wishlist> wishlists { get; set; } = new List<wishlist>();
}
