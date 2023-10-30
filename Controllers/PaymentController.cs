using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Simbir.GO.Model;

namespace Simbir.GO.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly UserManager<Account> _userManager;

    public PaymentController(UserManager<Account> userManager)
    {
        _userManager = userManager;
    }

    [HttpPost]
    [Route("Hesoyam/{accountId:int?}")]
    public async Task<IActionResult> AddMoney(int? accountId)
    {
        var currUser = HttpContext.User;
        var currUserId = currUser.Claims.First(e => e.Type == "id");
        var user = await _userManager.FindByIdAsync(currUserId.Value);
        if (user.IsAdmin)
        {
            if (accountId == null)
                await _userManager.Users.ExecuteUpdateAsync(e =>
                    e.SetProperty(a => a.Balance, a => a.Balance + 250000));
            else
            {
                var account = await _userManager.FindByIdAsync(accountId.Value.ToString());
                if (account == null)
                    return NotFound();

                account.Balance += 250000;
                await _userManager.UpdateAsync(account);
            }
        }
        else
        {
            if (accountId != null && accountId != user.Id)
                return BadRequest("You can add money only to yourself!");
                
            user.Balance += 250000;
            await _userManager.UpdateAsync(user);
        }
        
        return Ok("Cheat activated");
    }
}