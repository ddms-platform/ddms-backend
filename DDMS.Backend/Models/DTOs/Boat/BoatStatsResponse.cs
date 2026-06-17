namespace DDMS.Backend.Models.DTOs.Boat;

public class MonthlyProfit
{
    public string Month { get; init; }
    public decimal Profit { get; init; }
    public int Year { get; init; }
}

public class BoatStatsResponse
{
    public int total { get; init; }
    public int idle { get; init; }
    public int running { get; init; }
    public int totalCabins { get; init; }
    public List<MonthlyProfit> monthlyProfits { get; init; } = new();
}
