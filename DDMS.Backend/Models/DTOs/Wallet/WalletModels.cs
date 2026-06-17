namespace DDMS.Backend.Models.DTOs.Wallet;

public class WithdrawRequest
{
    public decimal Amount { get; set; }
    public string BankName { get; set; } = null!;
    public string AccountNumber { get; set; } = null!;
    public string AccountName { get; set; } = null!;
}

public class WalletBalanceResponse
{
    public decimal Balance { get; set; }
}

public class WithdrawalListItem
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string BankName { get; set; } = null!;
    public string AccountNumber { get; set; } = null!;
    public string AccountName { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}

public class WithdrawResult
{
    public bool Success { get; set; } = true;
    public decimal NewBalance { get; set; }
}
