using EaseMyBooking.Api.Data;
using EaseMyBooking.Api.Dtos;
using EaseMyBooking.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Razorpay.Api;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace EaseMyBooking.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _config;
    public BookingsController(ApplicationDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    [HttpPost]
    public async Task<IActionResult> CreateBooking([FromBody] BookingDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var place = await _db.Places.FindAsync(dto.PlaceId);
        if (place == null) return NotFound("Template not found");

        if ((User.FindFirstValue(ClaimTypes.Role) ?? "") == "Owner" && place.OwnerId == userId)
            return BadRequest("Sellers cannot buy their own template.");

        var effectiveDate = (dto.VisitDate ?? DateTime.UtcNow.Date).Date;

        var booking = new Booking
        {
            PlaceId = dto.PlaceId,
            UserId = userId,
            VisitDate = effectiveDate, // kept for compatibility
            Quantity = dto.Quantity,
            PaymentConfirmed = false
        };

        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();

        decimal total = place.Price * booking.Quantity;
        int amountPaise = (int)(total * 100);
        if (amountPaise <= 0)
            return BadRequest("Invalid Order Amount.");

        string key = _config["PaymentSettings:RazorpayKey"] ?? throw new Exception("Missing RazorpayKey");
        string secret = _config["PaymentSettings:RazorpaySecret"] ?? throw new Exception("Missing RazorpaySecret");

        var client = new RazorpayClient(key, secret);
        var options = new Dictionary<string, object>
        {
            { "amount", amountPaise },
            { "currency", "INR" },
            { "receipt", $"booking_{booking.BookingId}" }
        };

        Order order;
        try
        {
            order = client.Order.Create(options);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Payment order creation failed: {ex.Message}");
        }

        string orderId = order["id"].ToString();
        return Ok(new
        {
            bookingId = booking.BookingId,
            orderId,
            amount = amountPaise,
            currency = "INR",
            message = "Order created. Pending payment."
        });
    }

    [HttpPost("verifyPayment")]
    public async Task<IActionResult> VerifyPayment([FromBody] VerifyPaymentDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var booking = await _db.Bookings.FindAsync(dto.BookingId);
        if (booking == null || booking.UserId != userId)
            return NotFound("Order not found");

        string secret = _config["PaymentSettings:RazorpaySecret"] ?? throw new Exception("Missing RazorpaySecret");
        string payload = $"{dto.OrderId}|{dto.PaymentId}";

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        string computedSignature = string.Concat(hash.Select(b => b.ToString("x2")));

        if (!computedSignature.Equals(dto.Signature, StringComparison.OrdinalIgnoreCase))
            return BadRequest("Invalid payment signature.");

        booking.PaymentConfirmed = true;
        await _db.SaveChangesAsync();

        return Ok("Payment verified and purchase confirmed.");
    }

    [HttpGet("my")]
    [Authorize(Roles = "Visitor")]
    public async Task<IActionResult> GetMyBookings()
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var list = await _db.Bookings
            .AsNoTracking()
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.BookingTime)
            .Select(b => new
            {
                b.BookingId,
                b.PlaceId,
                b.VisitDate,
                b.Quantity,
                b.BookingTime,
                b.PaymentConfirmed,

                place = new
                {
                    b.Place.PlaceId,
                    b.Place.Name,
                    b.Place.Price,
                    templateType = b.Place.Location
                    // IMPORTANT: do not expose google drive link here
                },

                // Expose file link only after payment is confirmed
                deliveryUrl = b.PaymentConfirmed ? b.Place.GoogleMapsUrl : null,

                placeThumbUrl = _db.PlaceImages
                    .Where(pi => pi.PlaceId == b.PlaceId)
                    .OrderBy(pi => pi.SortOrder)
                    .Select(pi => pi.Url)
                    .FirstOrDefault() ?? b.Place.ImageUrl
            })
            .ToListAsync();

        return Ok(list);
    }

    [HttpGet("place/{placeId}")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> GetBookingsForPlace(int placeId)
    {
        int ownerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var place = await _db.Places.FindAsync(placeId);
        if (place == null) return NotFound();
        if (place.OwnerId != ownerId) return Forbid("Not your template");

        var placeBookings = await _db.Bookings
            .Include(b => b.User)
            .Where(b => b.PlaceId == placeId)
            .OrderByDescending(b => b.BookingTime)
            .ToListAsync();

        return Ok(placeBookings);
    }

    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllBookings()
    {
        var allBookings = await _db.Bookings
            .Include(b => b.Place)
            .Include(b => b.User)
            .OrderByDescending(b => b.BookingTime)
            .ToListAsync();

        return Ok(allBookings);
    }
}