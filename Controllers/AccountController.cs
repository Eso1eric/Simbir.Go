using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Simbir.GO.Model;
using Simbir.GO.Contract.Requests;
using Simbir.GO.Contract.Responses;
using Simbir.GO.Extensions;

namespace Simbir.GO.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly ILogger<AccountController> _logger;
    private readonly IConfiguration _configuration;
    private readonly UserManager<Account> _userManager;

    public AccountController(ILogger<AccountController> logger, IConfiguration configuration, UserManager<Account> userManager)
    {
        _logger = logger;
        _configuration = configuration;
        _userManager = userManager;
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpGet]
    [Route("Me")]
    public async Task<AccountDataResponse> GetMe()
    {
        var currUser = HttpContext.User;
        var currUserId = currUser.Claims.First(e => e.Type == "id");
        var user = await _userManager.FindByIdAsync(currUserId.Value);
        
        return new AccountDataResponse
        {
            Username = user.UserName,
            Password = "*****",
            CreateDate = user.CreateDate,
            Balance = user.Balance
        };
    }

    [HttpPost]
    [Route("SignIn")]
    public async Task<IActionResult> SignIn([FromBody] AuthorizationRequest request)
    {
        if (HttpContext.Request.Cookies["USER_SESSION"] != null)
            return BadRequest();
            
        var user = await _userManager.FindByNameAsync(request.Username);
        if (user != null && await _userManager.CheckPasswordAsync(user, request.Password))
        {
            var token = new JwtSecurityTokenHandler().GenerateJwtToken(user, _configuration);
            HttpContext.Response.Cookies.Append("USER_SESSION", token, new CookieOptions() { MaxAge = TimeSpan.FromMinutes(15)});
            return Ok(new { token });
        }

        return Unauthorized("Username or password is incorrect.");
    }

    [HttpPost]
    [Route("SignUp")]
    public async Task<IActionResult> SignUp([FromBody] AuthorizationRequest request)
    {
        if (await _userManager.FindByNameAsync(request.Username) != null)
            return BadRequest($"User with username \"{request.Username}\" already exists!");

        var newUser = new Account()
        {
            UserName = request.Username,
            CreateDate = DateTime.UtcNow,
            NormalizedUserName = _userManager.NormalizeName(request.Username),
            Balance = 0,
            IsAdmin = false
        };

        var user = await _userManager.CreateAsync(newUser, request.Password);
        
        return user.Succeeded
            ? Ok("Success! Use your username and password to get authentication token.")
            : BadRequest($"Could not create account with provided username and password.\n{string.Join("\n", user.Errors.Select(e => e.Description))}");
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpPost]
    [Route("SignOut")]
    public async new Task<IActionResult> SignOut()
    {
        if (HttpContext.Request.Cookies["USER_SESSION"] != null)
        {
            HttpContext.Response.Cookies.Delete("USER_SESSION");
            HttpContext.Session.Clear();
        }

        return Ok();
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpPut]
    [Route("Update")]
    public async Task<IActionResult> Update([FromBody] AuthorizationRequest request)
    {
        if (await _userManager.FindByNameAsync(request.Username) != null)
            return BadRequest($"Username \"{request.Username}\" has already been taken!");

        var errors = new List<string>();
        var currUser = HttpContext.User;
        var currUserId = currUser.Claims.First(e => e.Type == "id");
        var account = await _userManager.FindByIdAsync(currUserId.Value);
        if (request.Username != account.UserName)
        {
            account.UserName = request.Username;
            await _userManager.UpdateNormalizedUserNameAsync(account);
            var usernameChangeResult = await _userManager.UpdateAsync(account);
            if (!usernameChangeResult.Succeeded)
                errors = errors.Union(usernameChangeResult.Errors.Select(e => e.Description)).ToList();
        }
        
        var passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(account);
        var passwordChangeResult = await _userManager.ResetPasswordAsync(account, passwordResetToken, request.Password);
        if (!passwordChangeResult.Succeeded)
            errors = errors.Union(passwordChangeResult.Errors.Select(e => e.Description)).ToList();
        
        return errors.Any() ? BadRequest(string.Join("\n", errors)) : Ok("Success!");
    }
}