using System;
using System.Collections.Generic;

namespace DDMS.Backend.Models.Entities;

public partial class audit_log
{
    public Guid id { get; set; }

    public Guid? user_id { get; set; }

    public string table_name { get; set; } = null!;

    public string record_id { get; set; } = null!;

    public string action { get; set; } = null!;

    public string? old_values { get; set; }

    public string? new_values { get; set; }

    public string? ip_address { get; set; }

    public DateTime created_at { get; set; }

    public virtual user? user { get; set; }
}
