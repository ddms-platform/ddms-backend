using DDMS.Backend.Common.Identity;
using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.Wallet;
using DDMS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DDMS.Backend.Controllers;

[ApiController]
[Route("api/wallet")]
[Authorize]
public class WalletController : ControllerBase
{
    private readonly IWalletService _wallet;
    private readonly ICurrentUser _user;

    public WalletController(IWalletService wallet, ICurrentUser user)
    {
        _wallet = wallet;
        _user = user;
    }

    [HttpGet("balance")]
    public async Task<IActionResult> GetBalance(CancellationToken ct) =>
        Ok(ApiResponse<WalletBalanceResponse>.Ok(await _wallet.GetBalanceAsync(_user.Id, ct)));

    [HttpGet("withdrawals")]
    public async Task<IActionResult> GetWithdrawals(CancellationToken ct) =>
        Ok(ApiResponse<List<WithdrawalListItem>>.Ok(await _wallet.GetWithdrawalsAsync(_user.Id, ct)));

    [HttpPost("withdraw")]
    public async Task<IActionResult> RequestWithdraw([FromBody] WithdrawRequest req, CancellationToken ct) =>
        Ok(ApiResponse<WithdrawResult>.Ok(await _wallet.RequestWithdrawAsync(_user.Id, req, ct)));
}
