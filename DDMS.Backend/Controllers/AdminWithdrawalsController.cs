using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using DDMS.Backend.Data;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Common.Responses;
using DDMS.Backend.Services.Interfaces;

namespace DDMS.Backend.Controllers;

[Authorize(Roles = "admin")]
[ApiController]
[Route("api/admin/withdrawals")]
public class AdminWithdrawalsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IEmailSender _emailSender;

    public AdminWithdrawalsController(AppDbContext context, IEmailSender emailSender)
    {
        _context = context;
        _emailSender = emailSender;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllWithdrawals()
    {
        try
        {
            var list = await _context.wallet_withdrawals
                .Include(w => w.user)
                .OrderByDescending(w => w.created_at)
                .Select(w => new
                {
                    id = w.id,
                    userId = w.user_id,
                    userFullName = w.user.full_name,
                    userEmail = w.user.email,
                    amount = w.amount,
                    bankName = w.bank_name,
                    accountNumber = w.account_number,
                    accountName = w.account_name,
                    status = w.status,
                    createdAt = w.created_at,
                    processedAt = w.processed_at
                })
                .ToListAsync();

            return Ok(ApiResponse<object>.Ok(list));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                IsSuccess = false,
                Message = "Lỗi khi lấy danh sách yêu cầu rút tiền",
                Error = ex.Message
            });
        }
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> ApproveWithdrawal(Guid id)
    {
        try
        {
            var withdrawal = await _context.wallet_withdrawals
                .Include(w => w.user)
                .FirstOrDefaultAsync(w => w.id == id);

            if (withdrawal == null)
                return NotFound(new { message = "Không tìm thấy yêu cầu rút tiền." });

            if (withdrawal.status != "pending")
                return BadRequest(new { message = "Yêu cầu rút tiền này đã được xử lý trước đó." });

            withdrawal.status = "approved";
            withdrawal.processed_at = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Send notification email to user
            if (withdrawal.user != null)
            {
                try
                {
                    await _emailSender.SendWithdrawalStatusEmailAsync(
                        withdrawal.user.email,
                        withdrawal.user.full_name,
                        withdrawal.amount,
                        withdrawal.bank_name,
                        withdrawal.account_number,
                        "approved"
                    );
                }
                catch (Exception mailEx)
                {
                    // Log email sending error
                }
            }

            return Ok(ApiResponse<object>.Ok(new { success = true }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> RejectWithdrawal(Guid id)
    {
        try
        {
            var withdrawal = await _context.wallet_withdrawals
                .Include(w => w.user)
                .FirstOrDefaultAsync(w => w.id == id);

            if (withdrawal == null)
                return NotFound(new { message = "Không tìm thấy yêu cầu rút tiền." });

            if (withdrawal.status != "pending")
                return BadRequest(new { message = "Yêu cầu rút tiền này đã được xử lý trước đó." });

            withdrawal.status = "rejected";
            withdrawal.processed_at = DateTime.UtcNow;

            // Refund the withdrawal amount back to user's wallet
            var wallet = await _context.user_wallets.FirstOrDefaultAsync(w => w.user_id == withdrawal.user_id);
            if (wallet != null)
            {
                wallet.balance += withdrawal.amount;
                wallet.updated_at = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            // Send notification email to user
            if (withdrawal.user != null)
            {
                try
                {
                    await _emailSender.SendWithdrawalStatusEmailAsync(
                        withdrawal.user.email,
                        withdrawal.user.full_name,
                        withdrawal.amount,
                        withdrawal.bank_name,
                        withdrawal.account_number,
                        "rejected"
                    );
                }
                catch (Exception mailEx)
                {
                    // Log email sending error
                }
            }

            return Ok(ApiResponse<object>.Ok(new { success = true }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}
