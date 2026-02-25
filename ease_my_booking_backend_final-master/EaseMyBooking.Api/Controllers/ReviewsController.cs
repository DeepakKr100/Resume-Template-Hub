using EaseMyBooking.Api.Data;
using EaseMyBooking.Api.Dtos;
using EaseMyBooking.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EaseMyBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public ReviewsController(ApplicationDbContext db) => _db = db;

    /// <summary>
    /// Add a review for a template. Only allowed if user has a PAID purchase for that template.
    /// Body: { placeId, rating(1..5), comment(min 10 chars) }
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Visitor")]
    public async Task<IActionResult> Create([FromBody] ReviewDto dto)
    {
        if (dto == null) return BadRequest("Invalid payload.");
        if (dto.Rating < 1 || dto.Rating > 5) return BadRequest("Rating must be between 1 and 5.");
        if (string.IsNullOrWhiteSpace(dto.Comment) || dto.Comment.Trim().Length < 10)
            return BadRequest("Comment must be at least 10 characters.");

        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        bool eligible = await _db.Bookings
            .AnyAsync(b => b.UserId == userId
                        && b.PlaceId == dto.PlaceId
                        && b.PaymentConfirmed);

        if (!eligible)
            return BadRequest("You can only review templates you have purchased.");

        var review = new Review
        {
            PlaceId = dto.PlaceId,
            UserId = userId,
            Rating = dto.Rating,
            Comment = dto.Comment.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _db.Reviews.Add(review);
        await _db.SaveChangesAsync();

        var user = await _db.Users.Where(u => u.UserId == userId).Select(u => u.Name).FirstAsync();
        return Ok(new
        {
            reviewId = review.ReviewId,
            rating = review.Rating,
            comment = review.Comment,
            createdAt = review.CreatedAt,
            userName = user
        });
    }

    [HttpGet("place/{placeId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> ForPlace(int placeId)
    {
        var items = await _db.Reviews
            .Where(r => r.PlaceId == placeId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new
            {
                r.ReviewId,
                r.Rating,
                r.Comment,
                r.CreatedAt,
                user = new { name = r.User!.Name, id = r.UserId }
            })
            .ToListAsync();

        return Ok(items);
    }
}