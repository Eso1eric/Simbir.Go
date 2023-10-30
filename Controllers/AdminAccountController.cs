using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Simbir.GO.Contract.Requests;
using Simbir.GO.Model;
using Simbir.GO.Contract.Responses;
using Simbir.GO.Identity;

namespace Simbir.GO.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = "Bearer", Policy = CustomIdentityConstants.AdminPolicy)]
[Route("api/[controller]")]
public class AdminAccountController : ControllerBase
{
    private readonly ILogger<AccountController> _logger;
    private readonly UserManager<Account> _userManager;

    public AdminAccountController(ILogger<AccountController> logger, UserManager<Account> userManager)
    {
        _logger = logger;
        _userManager = userManager;
    }

    [HttpGet]
    [Route("Account")]
    public async Task<List<AccountDataResponse>> GetAccounts([FromHeader] int start, [FromHeader] int count)
    {
        var users = start == 0 && count == 0
            ? _userManager.Users
            : _userManager.Users.Skip(Math.Max(start, count == 0 ? count : await _userManager.Users.CountAsync() - count));
        
        return users.Select(e => new AccountDataResponse()
        {
            Username = e.UserName,
            Password = "*****",
            CreateDate = e.CreateDate,
            Balance = e.Balance
        }).ToList();
    }

    [HttpGet]
    [Route("Account/{id}")]
    public async Task<IActionResult> GetAccount(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        return user != null
            ? Ok(new AccountDataResponse()
            {
                Username = user.UserName,
                Password = "*****",
                CreateDate = user.CreateDate,
                Balance = user.Balance
            })
            : NotFound();
    }
    
    [HttpPost]
    [Route("Account")]
    public async Task<IActionResult> CreateAccount([FromBody] ManageAccountRequest request)
    {
        if (await _userManager.FindByNameAsync(request.Username) != null)
            return BadRequest("User with specified username already exists!");

        var newUser = new Account()
        {
            UserName = request.Username,
            NormalizedUserName = _userManager.NormalizeName(request.Username),
            CreateDate = DateTime.UtcNow,
            Balance = request.Balance
        };

        var user = await _userManager.CreateAsync(newUser, request.Password);

        return user.Succeeded
            ? Ok("Success!")
            : BadRequest($"Could not create account with provided username and password.\n{string.Join("\n", user.Errors.Select(e => e.Description))}");
    }
    
    [HttpPut]
    [Route("Account/{id}")]
    public async Task<IActionResult> UpdateAccount(string id, [FromBody] ManageAccountRequest request)
    {
        var account = await _userManager.FindByIdAsync(id);
        if (account == null)
            return NotFound();

        var errors = new List<string>();
        if (request.Username != account.UserName)
        {
            account.UserName = request.Username;
            await _userManager.UpdateNormalizedUserNameAsync(account);
            var usernameChangeResult = await _userManager.UpdateAsync(account);
            if (!usernameChangeResult.Succeeded)
                errors = errors.Union(usernameChangeResult.Errors.Select(e => e.Description)).ToList();
        }

        account.Balance = request.Balance;
        account.IsAdmin = request.IsAdmin;
        var passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(account);
        var passwordChangeResult = await _userManager.ResetPasswordAsync(account, passwordResetToken, request.Password);
        if (!passwordChangeResult.Succeeded)
            errors = errors.Union(passwordChangeResult.Errors.Select(e => e.Description)).ToList();
        
        return errors.Any() ? BadRequest(string.Join("\n", errors)) : Ok("Success!");
    }

    [HttpDelete]
    [Route("Account/{id}")]
    public async Task<IActionResult> DeleteAccount(string id)
    {
        var account = await _userManager.FindByIdAsync(id);
        if (account == null)
            return NotFound();

        var result = await _userManager.DeleteAsync(account);
        return result.Succeeded
            ? Ok("Success!")
            : BadRequest(string.Join("\n", result.Errors.Select(e => e.Description)));
    }
}