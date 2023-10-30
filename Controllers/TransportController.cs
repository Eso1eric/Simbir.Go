using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Simbir.GO.Contract.Requests;
using Simbir.GO.Data;
using Simbir.GO.Model;

namespace Simbir.GO.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransportController : ControllerBase
{
    private readonly RentContext _context;
    private readonly UserManager<Account> _userManager;

    public TransportController(RentContext context, UserManager<Account> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    [Route("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var transport = await _context.Transports.FindAsync(id);
        return transport != null ? Ok(transport) : NotFound();
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AddTransportRequest request)
    {
        var currUser = HttpContext.User;
        var currUserId = currUser.Claims.First(e => e.Type == "id");
        var owner = await _userManager.FindByIdAsync(currUserId.Value);
        if (owner == null)
            return BadRequest("Something went wrong.");
        
        var transport = new Transport()
        {
            AccountId = owner.Id,
            CanBeRented = request.CanBeRented,
            TransportType = request.TransportType,
            Model = request.Model,
            Color = request.Color,
            Identifier = request.Identifier,
            Description = request.Description,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            MinutePrice = request.MinutePrice,
            DayPrice = request.DayPrice
        };

        await _context.Transports.AddAsync(transport);
        await _context.SaveChangesAsync();
        return Ok("Success!");
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpPut]
    [Route("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTransportRequest request)
    {
        var currUser = HttpContext.User;
        var currUserId = currUser.Claims.First(e => e.Type == "id");
        var owner = await _userManager.FindByIdAsync(currUserId.Value);
        var transport = await _context.Transports.FindAsync(id);
        if (transport == null || transport.AccountId != owner.Id)
            return NotFound("Could not find your transport with specified id.");

        transport.CanBeRented = request.CanBeRented;
        transport.Model = request.Model;
        transport.Color = request.Color;
        transport.Identifier = request.Identifier;
        transport.Description = request.Description;
        transport.Latitude = request.Latitude;
        transport.Longitude = request.Longitude;
        transport.MinutePrice = request.MinutePrice;
        transport.DayPrice = request.DayPrice;

        await _context.SaveChangesAsync();
        return Ok("Success!");
    }
    
    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpDelete]
    [Route("{id:int}")]
    public async Task<IActionResult> Remove(int id)
    {
        var currUser = HttpContext.User;
        var currUserId = currUser.Claims.First(e => e.Type == "id");
        var owner = await _userManager.FindByIdAsync(currUserId.Value);
        var transport = await _context.Transports.FindAsync(id);
        if (transport == null || transport.AccountId != owner.Id)
            return NotFound("Could not find your transport with specified id.");

        _context.Transports.Remove(transport);
        await _context.SaveChangesAsync();
        return Ok("Success!");
    }
}