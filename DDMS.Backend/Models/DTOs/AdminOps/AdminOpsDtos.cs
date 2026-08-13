namespace DDMS.Backend.Models.DTOs.AdminOps;

public class OpsBriefingResponse
{
    /// <summary>Ngày báo cáo (UTC).</summary>
    public DateTime GeneratedAt { get; set; }

    /// <summary>Đoạn tóm tắt tự nhiên do AI viết.</summary>
    public string Narrative { get; set; } = string.Empty;

    /// <summary>Raw signals — FE có thể render dạng chip/badge riêng.</summary>
    public OpsBriefingSignals Signals { get; set; } = new();
}

public class OpsBriefingSignals
{
    public int ToursToday { get; set; }
    public int GuestsExpected { get; set; }
    public decimal RevenueForecast { get; set; }
    public int BoatsInMaintenance { get; set; }
    public int PendingOwnerVerifications { get; set; }
    public int PendingTourApprovals { get; set; }
    public List<DockLoadItem> DockPeaks { get; set; } = new();
    public List<AlertItem> Alerts { get; set; } = new();
    public string? WeatherSummary { get; set; }
}

public class DockLoadItem
{
    public string DockName { get; set; } = string.Empty;
    public int ToursInWindow { get; set; }
    public int MaxBoats { get; set; }
    public int UtilizationPercent { get; set; }
    public string WindowLabel { get; set; } = string.Empty;
}

public class AlertItem
{
    /// <summary>"warning" | "info" | "critical"</summary>
    public string Severity { get; set; } = "info";
    public string Title { get; set; } = string.Empty;
    public string? Detail { get; set; }
}

public class AdminOpsChatRequest
{
    public string Question { get; set; } = string.Empty;
    public Guid? ConversationId { get; set; }
}

public class AdminOpsChatResponse
{
    public Guid ConversationId { get; set; }
    public string Answer { get; set; } = string.Empty;
    /// <summary>Optional data table returned alongside the narrative answer.</summary>
    public List<Dictionary<string, object?>>? DataTable { get; set; }
}

public class WhatIfSimRequest
{
    /// <summary>"close_dock" | "bad_weather" | "add_boats"</summary>
    public string Scenario { get; set; } = string.Empty;
    /// <summary>For close_dock: dockId. For add_boats: number of new boats.</summary>
    public Guid? DockId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? Number { get; set; }
}

public class WhatIfSimResponse
{
    public string Scenario { get; set; } = string.Empty;
    /// <summary>Human-friendly summary of impact.</summary>
    public string Summary { get; set; } = string.Empty;
    public int AffectedBookings { get; set; }
    public int AffectedGuests { get; set; }
    public decimal PotentialRefundVnd { get; set; }
    public List<AlertItem> Suggestions { get; set; } = new();
}
