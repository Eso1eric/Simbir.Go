using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Simbir.GO.Contract.Requests;
using Simbir.GO.Data;
using Simbir.GO.Identity;
using Simbir.GO.Model;

namespace Simbir.GO.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = "Bearer", Policy = CustomIdentityConstants.AdminPolicy)]
[Route("api/[controller]")]
public class AdminTransportController : ControllerBase
{
    private readonly RentContext _context;

    public AdminTransportController(RentContext context)
    {
        _context = context;
    }
    
    [HttpGet]
    public async Task<List<Transport>> GetTransports([FromHeader] int start, [FromHeader] int count, [FromHeader] string? transportType)
    {
        var transports = start == 0 && count == 0
            ? _context.Transports
            : _context.Transports.Skip(Math.Max(start,
                count == 0 ? count : await _context.Transports.CountAsync() - count));

        return transportType == null
            ? transports.ToList()
            : transports.Where(e => e.TransportType == transportType).ToList();
    }

    [HttpGet]
    [Route("{id:long}")]
    public async Task<IActionResult> GetTransport(int id)
    {
        var transport = await _context.Transports.FindAsync(id);
        
        return transport != null ? Ok(transport) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> AddTransport([FromBody] AdminTransportRequest request)
    {
        if (await _context.Accounts.FindAsync(request.OwnerId) == null)
            return BadRequest("Could not find user with specified owner id.");
        
        var transport = new Transport()
        {
            AccountId = request.OwnerId,
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

    [HttpPut]
    [Route("{id:int}")]
    public async Task<IActionResult> UpdateTransport(int id, [FromBody] AdminTransportRequest request)
    {
        var transport = await _context.Transports.FindAsync(id);
        if (transport == null)
            return NotFound();

        transport.AccountId = request.OwnerId;
        transport.CanBeRented = request.CanBeRented;
        transport.TransportType = request.TransportType;
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

    [HttpDelete]
    [Route("{id:int}")]
    public async Task<IActionResult> RemoveTransport(int id)
    {
        var transport = await _context.Transports.FindAsync(id);
        if (transport == null)
            return NotFound();
        
        _context.Transports.Remove(transport);
        await _context.SaveChangesAsync();
        return Ok("Success!");
    }
}