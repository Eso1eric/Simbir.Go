using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Simbir.GO.Contract.Requests;
using Simbir.GO.Contract.Responses;
using Simbir.GO.Data;
using Simbir.GO.Model;

namespace Simbir.GO.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RentController : ControllerBase
{
    private readonly RentContext _context;
    private readonly UserManager<Account> _userManager;

    public RentController(RentContext context, UserManager<Account> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    [Route("Transport")]
    public async Task<IActionResult> FindTransportInRange([FromHeader] double lat, [FromHeader] double @long, [FromHeader] double radius, [FromHeader] string type)
    {
        var transport = _context.Transports.Where(e =>
            e.CanBeRented && e.TransportType == type &&
            (Math.Pow(e.Latitude - lat, 2) + Math.Pow(e.Longitude - @long, 2) <
             Math.Pow(radius, 2)));

        return transport.Any()
            ? Ok(transport.Select(e => new TransportInRangeResponse()
            {
                CanBeRented = e.CanBeRented,
                TransportType = e.TransportType,
                Model = e.Model,
                Color = e.Color,
                Identifier = e.Identifier,
                Description = e.Description,
                Latitude = e.Latitude,
                Longitude = e.Longitude,
                MinutePrice = e.MinutePrice,
                DayPrice = e.DayPrice
            }))
            : NotFound("Could not find any transport in this range.");
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpGet]
    [Route("{rentId:guid}")]
    public async Task<IActionResult> Get(Guid rentId)
    {
        var currUser = HttpContext.User;
        var currUserId = currUser.Claims.First(e => e.Type == "id");
        var user = await _userManager.FindByIdAsync(currUserId.Value);
        var rent = await _context.Rents.FindAsync(rentId);
        if (rent != null && rent.AccountId != user.Id)
            return NotFound();
            
        return Ok(rent);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpGet]
    [Route("MyHistory")]
    public async Task<IActionResult> GetHistory()
    {
        var currUser = HttpContext.User;
        var currUserId = currUser.Claims.First(e => e.Type == "id");
        var user = await _userManager.FindByIdAsync(currUserId.Value);
        var rents = _context.Rents.Where(e => e.AccountId == user.Id);
        
        return Ok(rents);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpGet]
    [Route("TransportHistory/{transportId:int}")]
    public async Task<IActionResult> GetTransportHistory(int transportId)
    {
        var currUser = HttpContext.User;
        var currUserId = currUser.Claims.First(e => e.Type == "id");
        var user = await _userManager.FindByIdAsync(currUserId.Value);
        var transport = _context.Rents.FirstOrDefault(e => e.TransportId == transportId && e.AccountId == user.Id);

        return transport != null ? Ok(transport) : NotFound();
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpPost]
    [Route("New/{transportId:int}")]
    public async Task<IActionResult> StartNewRent(int transportId, [FromHeader] string rentType)
    {
        var currUser = HttpContext.User;
        var currUserId = currUser.Claims.First(e => e.Type == "id");
        var user = await _userManager.FindByIdAsync(currUserId.Value);
        var transport = _context.Transports.FirstOrDefault(e => e.Id == transportId && e.AccountId != user.Id);
        if (transport == null)
            return BadRequest("Could not rent transport with specified id.");

        var rent = new Rent()
        {
            TransportId = transportId,
            AccountId = user.Id,
            TimeStart = DateTime.UtcNow,
            TimeEnd = null,
            PriceOfUnit = rentType.ToLower() == "minutes" ? transport.MinutePrice.Value : transport.DayPrice.Value,
            PriceType = rentType,
            FinalPrice = null
        };

        transport.CanBeRented = false;

        await _context.Rents.AddAsync(rent);
        await _context.SaveChangesAsync();
        
        return Ok("Success!");
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpPost]
    [Route("End/{rentId:guid}")]
    public async Task<IActionResult> EndRent(Guid rentId, [FromHeader] double lat, [FromHeader] double @long)
    {
        var currUser = HttpContext.User;
        var currUserId = currUser.Claims.First(e => e.Type == "id");
        var user = await _userManager.FindByIdAsync(currUserId.Value);
        var rent = await _context.Rents.FindAsync(rentId);
        if (rent.AccountId != user.Id)
            return NotFound();

        var transport = await _context.Transports.FindAsync(rent.TransportId);
        
        rent.TimeEnd = DateTime.UtcNow;
        var diff = rent.TimeEnd.Value - rent.TimeStart;
        rent.FinalPrice = rent.PriceOfUnit * (rent.PriceType.ToLower() == "minutes" ? diff.TotalMinutes : diff.TotalDays);
        transport.CanBeRented = true;
        transport.Latitude = lat;
        transport.Longitude = @long;
        
        await _context.SaveChangesAsync();
        
        return Ok("Rent ended.");
    }
}