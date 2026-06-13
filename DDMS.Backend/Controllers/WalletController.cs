using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Common.Responses;
using DDMS.Backend.Data;
using DDMS.Backend.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;

namespace DDMS.Backend.Controllers;

[ApiController]
[Route("api/wallet")]
[Authorize]
public class WalletController : ControllerBase
{
    private readonly AppDbContext _context;

    public WalletController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("balance")]
    public async Task<IActionResult> GetBalance()
    {
        var userId = GetCurrentUserId();
        var wallet = await _context.user_wallets.FirstOrDefaultAsync(w => w.user_id == userId);
        var balance = wallet?.balance ?? 0m;
        return Ok(ApiResponse<object>.Ok(new { balance }));
    }

    [HttpGet("withdrawals")]
    public async Task<IActionResult> GetWithdrawals()
    {
        var userId = GetCurrentUserId();
        var withdrawals = await _context.wallet_withdrawals
            .Where(w => w.user_id == userId)
            .OrderByDescending(w => w.created_at)
            .Select(w => new {
                w.id,
                w.amount,
                w.bank_name,
                w.account_number,
                w.account_name,
                w.status,
                w.created_at,
                w.processed_at
            })
            .ToListAsync();
        return Ok(ApiResponse<object>.Ok(withdrawals));
    }

    [HttpPost("withdraw")]
    public async Task<IActionResult> RequestWithdraw([FromBody] WithdrawRequest req)
    {
        var userId = GetCurrentUserId();

        if (req.Amount <= 0)
        {
            return BadRequest(new ApiErrorResponse
            {
                code = ErrorCode.UncategorizedError,
                message = "Số tiền rút phải lớn hơn 0."
            });
        }

        if (string.IsNullOrWhiteSpace(req.BankName) || 
            string.IsNullOrWhiteSpace(req.AccountNumber) || 
            string.IsNullOrWhiteSpace(req.AccountName))
        {
            return BadRequest(new ApiErrorResponse
            {
                code = ErrorCode.UncategorizedError,
                message = "Vui lòng nhập đầy đủ thông tin ngân hàng."
            });
        }

        var wallet = await _context.user_wallets.FirstOrDefaultAsync(w => w.user_id == userId);
        if (wallet == null || wallet.balance < req.Amount)
        {
            return BadRequest(new ApiErrorResponse
            {
                code = ErrorCode.UncategorizedError,
                message = "Số dư ví không đủ để thực hiện giao dịch."
            });
        }

        // Create withdrawal request and deduct immediately
        wallet.balance -= req.Amount;
        wallet.updated_at = DateTime.UtcNow;

        var withdrawal = new wallet_withdrawal
        {
            id = Guid.NewGuid(),
            user_id = userId,
            amount = req.Amount,
            bank_name = req.BankName.Trim(),
            account_number = req.AccountNumber.Trim(),
            account_name = req.AccountName.Trim(),
            status = "pending",
            created_at = DateTime.UtcNow
        };

        _context.wallet_withdrawals.Add(withdrawal);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { success = true, newBalance = wallet.balance }));
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedException();
        return userId;
    }
}

public class WithdrawRequest
{
    public decimal Amount { get; set; }
    public string BankName { get; set; } = null!;
    public string AccountNumber { get; set; } = null!;
    public string AccountName { get; set; } = null!;
}
