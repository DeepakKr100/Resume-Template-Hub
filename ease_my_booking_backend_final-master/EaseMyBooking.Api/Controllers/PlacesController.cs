using EaseMyBooking.Api.Data;
using EaseMyBooking.Api.Dtos;
using EaseMyBooking.Api.Models;
using EaseMyBooking.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EaseMyBooking.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PlacesController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly CloudinaryService _cloud;

    public PlacesController(ApplicationDbContext db, CloudinaryService cloud)
    {
        _db = db;
        _cloud = cloud;
    }

    // Public list with optional filters (templateType + maxPrice)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetPlaces([FromQuery] string? templateType, [FromQuery] decimal? maxPrice)
    {
        var q = _db.Places.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(templateType))
            q = q.Where(p => EF.Functions.Like(p.Location, $"%{templateType}%")); // compatibility: Location column stores template type

        if (maxPrice.HasValue)
            q = q.Where(p => p.Price <= maxPrice.Value);

        var raw = await (
            from p in q
            select new
            {
                p.PlaceId,
                p.Name,
                p.Description,
                TemplateType = p.Location,
                p.Price,

                FirstImageUrl = _db.PlaceImages
                    .Where(pi => pi.PlaceId == p.PlaceId)
                    .OrderBy(pi => pi.SortOrder)
                    .Select(pi => pi.Url)
                    .FirstOrDefault(),

                LegacyImageUrl = p.ImageUrl,

                Avg = _db.Reviews
                    .Where(r => r.PlaceId == p.PlaceId)
                    .Select(r => (double?)r.Rating)
                    .Average(),

                Cnt = _db.Reviews.Count(r => r.PlaceId == p.PlaceId)
            }
        ).ToListAsync();

        var list = raw.Select(x => new
        {
            x.PlaceId,
            x.Name,
            x.Description,
            templateType = x.TemplateType,
            x.Price,
            imageUrl = string.IsNullOrWhiteSpace(x.FirstImageUrl) ? x.LegacyImageUrl : x.FirstImageUrl,
            averageRating = Math.Round(x.Avg ?? 0.0, 1),
            reviewCount = x.Cnt
        });

        return Ok(list);
    }

    // Public detail (googleDriveUrl exposed only to owner of this template for edit page)
    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetPlace(int id)
    {
        var place = await _db.Places
            .Include(p => p.Owner)
            .Include(p => p.Reviews!).ThenInclude(r => r.User)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.PlaceId == id);

        if (place == null) return NotFound();

        bool canSeeDriveUrl = false;
        if (User?.Identity?.IsAuthenticated == true)
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (role == "Owner" && int.TryParse(idStr, out var requesterId))
            {
                canSeeDriveUrl = requesterId == place.OwnerId;
            }
        }

        var result = new
        {
            place.PlaceId,
            place.Name,
            place.Description,
            templateType = place.Location,     // compatibility
            place.Price,
            place.ImageUrl,

            // Only owner sees this (for edit form). Buyers/public won't get it here.
            googleDriveUrl = canSeeDriveUrl ? place.GoogleMapsUrl : null,

            owner = place.Owner == null ? null : new { place.Owner.UserId, place.Owner.Name },

            reviews = place.Reviews?
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new
                {
                    r.ReviewId,
                    r.Rating,
                    r.Comment,
                    r.CreatedAt,
                    user = new { id = r.UserId, name = r.User!.Name }
                }),

            images = place.Images?
                .OrderBy(i => i.SortOrder)
                .Select(i => new
                {
                    i.PlaceImageId,
                    i.Url,
                    i.SortOrder
                })
        };

        return Ok(result);
    }

    // Owner: my templates
    [HttpGet("my")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> GetMyPlaces()
    {
        int ownerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var myPlaces = await _db.Places
            .AsNoTracking()
            .Where(p => p.OwnerId == ownerId)
            .Select(p => new
            {
                p.PlaceId,
                p.Name,
                p.Description,
                templateType = p.Location,
                p.Price,
                p.ImageUrl,
                googleDriveUrl = p.GoogleMapsUrl,
                p.OwnerId
            })
            .ToListAsync();

        return Ok(myPlaces);
    }

    // Owner: create template
    [HttpPost]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> CreatePlace([FromBody] PlaceDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (!string.IsNullOrWhiteSpace(dto.GoogleDriveUrl) && !IsGoogleDriveUrl(dto.GoogleDriveUrl))
            return BadRequest("Please provide a valid Google Drive / Google Docs link.");

        int ownerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var place = new Place
        {
            Name = dto.Name,
            Description = dto.Description,

            // compatibility mapping
            Location = dto.TemplateType,
            Timings = "N/A",

            Price = dto.Price,
            ImageUrl = dto.ImageUrl ?? "",
            GoogleMapsUrl = dto.GoogleDriveUrl?.Trim(), // compatibility mapping
            OwnerId = ownerId
        };

        _db.Places.Add(place);
        await _db.SaveChangesAsync();
        return Ok(place);
    }

    // Owner: update template
    [HttpPut("{id}")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> UpdatePlace(int id, [FromBody] PlaceDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (!string.IsNullOrWhiteSpace(dto.GoogleDriveUrl) && !IsGoogleDriveUrl(dto.GoogleDriveUrl))
            return BadRequest("Please provide a valid Google Drive / Google Docs link.");

        var place = await _db.Places.FindAsync(id);
        if (place == null) return NotFound();

        int ownerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (place.OwnerId != ownerId) return Forbid("You cannot edit a template you don't own.");

        place.Name = dto.Name;
        place.Description = dto.Description;
        place.Location = dto.TemplateType;      // compatibility mapping
        place.Timings = "N/A";                  // keep existing DB column satisfied
        place.Price = dto.Price;
        place.ImageUrl = dto.ImageUrl ?? "";
        place.GoogleMapsUrl = dto.GoogleDriveUrl?.Trim(); // compatibility mapping

        await _db.SaveChangesAsync();
        return Ok(place);
    }

    [HttpPost("{id}/images")]
    [Authorize(Roles = "Owner")]
    [RequestSizeLimit(50_000_000)]
    public async Task<IActionResult> UploadImages(int id, [FromForm] List<IFormFile> files)
    {
        if (files == null || files.Count == 0) return BadRequest("No files uploaded.");

        var place = await _db.Places.FindAsync(id);
        if (place == null) return NotFound("Template not found.");

        int ownerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (place.OwnerId != ownerId) return Forbid("Not your template.");

        int nextOrder = await _db.PlaceImages
            .Where(pi => pi.PlaceId == id)
            .Select(pi => (int?)pi.SortOrder)
            .MaxAsync() ?? -1;

        var saved = new List<object>();
        foreach (var file in files)
        {
            if (!file.ContentType.StartsWith("image/")) return BadRequest("Only images are allowed.");
            if (file.Length == 0) continue;

            await using var stream = file.OpenReadStream();
            var folder = $"resume-template-market/templates/{id}";
            var (url, publicId) = await _cloud.UploadAsync(stream, file.FileName, folder);

            var pi = new PlaceImage
            {
                PlaceId = id,
                Url = url,
                PublicId = publicId,
                SortOrder = ++nextOrder
            };
            _db.PlaceImages.Add(pi);
            await _db.SaveChangesAsync();

            saved.Add(new { pi.PlaceImageId, pi.Url, pi.SortOrder });
        }

        return Ok(saved);
    }

    [HttpDelete("{id}/images/{imageId}")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> DeleteImage(int id, int imageId)
    {
        var place = await _db.Places.FindAsync(id);
        if (place == null) return NotFound("Template not found.");

        int ownerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (place.OwnerId != ownerId) return Forbid("Not your template.");

        var img = await _db.PlaceImages.FirstOrDefaultAsync(pi => pi.PlaceImageId == imageId && pi.PlaceId == id);
        if (img == null) return NotFound("Image not found.");

        await _cloud.DeleteAsync(img.PublicId);
        _db.PlaceImages.Remove(img);
        await _db.SaveChangesAsync();

        return Ok("Deleted");
    }

    private static bool IsGoogleDriveUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return false;

        var host = uri.Host.ToLowerInvariant();

        return host == "drive.google.com"
            || host == "docs.google.com"
            || host.EndsWith(".googleusercontent.com");
    }
}