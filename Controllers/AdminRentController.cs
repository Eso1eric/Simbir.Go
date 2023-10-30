using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Simbir.GO.Contract.Requests;
using Simbir.GO.Data;
using Simbir.GO.Identity;
using Simbir.GO.Model;

namespace Simbir.GO.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = "Bearer", Policy = CustomIdentityConstants.AdminPolicy)]
[Route("api/[controller]")]
public class AdminRentController : ControllerBase
{
    private readonly RentContext _context;
    private readonly UserManager<Account> _userManager;

    public AdminRentController(RentContext context, UserManager<Account> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    [Route("{rentId:guid}")]
    public async Task<IActionResult> Get(Guid rentId)
    {
        var rent = await _context.Rents.FindAsync(rentId);
        
        return rent != null ? Ok(rent) : NotFound();
    }

    [HttpGet]
    [Route("UserHistory/{userId:int}")]
    public async Task<IActionResult> GetUserHistory(int userId)
    {
        var rents = _context.Rents.Where(e => e.AccountId == userId);
        
        return rents.Any() ? Ok(rents) : NotFound();
    }

    [HttpGet]
    [Route("TransportHistory/{transportId:int}")]
    public async Task<IActionResult> GetTransportHistory(int transportId)
    {
        var rents = _context.Rents.Where(e => e.TransportId == transportId);
        
        return rents.Any() ? Ok(rents) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> NewRent([FromBody] AdminRentRequest request)
    {
        if (await _userManager.FindByIdAsync(request.UserId.ToString()) == null)
            return NotFound("Could not find user with specified id!");

        var transport = await _context.Transports.FindAsync(request.TransportId);
        if (transport == null)
            return NotFound("Could not find transport with specified id!");

        transport.CanBeRented = false;
        
        var rent = new Rent()
        {
            Id = Guid.NewGuid(),
            TransportId = request.TransportId,
            AccountId = request.UserId,
            TimeStart = request.TimeStart,
            TimeEnd = request.TimeEnd,
            PriceOfUnit = request.PriceOfUnit,
            PriceType = request.PriceType,
            FinalPrice = request.FinalPrice
        };

        await _context.Rents.AddAsync(rent);
        await _context.SaveChangesAsync();
        
        return Ok($"Success! Id of this rent: {rent.Id}");
    }

    [HttpPost]
    [Route("End/{rentId:guid}")]
    public async Task<IActionResult> EndRent(Guid rentId, [FromHeader] double lat, [FromHeader] double @long)
    {
        var rent = await _context.Rents.FindAsync(rentId);
        if (rent == null)
            return NotFound();

        if (rent.TimeEnd != null)
            return BadRequest("This rent has already ended!");
        
        var user = await _userManager.FindByIdAsync(rent.AccountId.ToString());
        var transport = await _context.Transports.FindAsync(rent.TransportId);

        rent.TimeEnd = DateTime.UtcNow;
        var diff = rent.TimeEnd.Value - rent.TimeStart;
        rent.FinalPrice = rent.PriceOfUnit * (rent.PriceType.ToLower() == "minutes" ? diff.TotalMinutes : diff.TotalDays);
        transport.CanBeRented = true;
        transport.Latitude = lat;
        transport.Longitude = @long;
        user.Balance -= rent.FinalPrice.Value;
        
        await _userManager.UpdateAsync(user);
        await _context.SaveChangesAsync();
        
        return Ok("Rent ended.");
    }

    [HttpPut]
    [Route("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] AdminRentRequest request)
    {
        var rent = await _context.Rents.FindAsync(id);
        if (rent == null)
            return NotFound();
        
        if (await _userManager.FindByIdAsync(request.UserId.ToString()) == null)
            return NotFound("Could not find user with specified id!");

        if (await _context.Transports.FindAsync(request.TransportId) == null)
            return NotFound("Could not find transport with specified id!");

        rent.TransportId = request.TransportId;
        rent.AccountId = request.UserId;
        rent.TimeStart = request.TimeStart;
        rent.TimeEnd = request.TimeEnd;
        rent.PriceOfUnit = request.PriceOfUnit;
        rent.PriceType = request.PriceType;
        rent.FinalPrice = request.FinalPrice;

        await _context.SaveChangesAsync();
        
        return Ok("Success!");
    }

    [HttpDelete]
    [Route("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var rent = await _context.Rents.FindAsync(id);
        if (rent == null)
            return NotFound();

        _context.Rents.Remove(rent);

        await _context.SaveChangesAsync();
        
        return Ok();
    }
}