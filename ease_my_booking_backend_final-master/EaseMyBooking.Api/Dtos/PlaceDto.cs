using System.ComponentModel.DataAnnotations;

namespace EaseMyBooking.Api.Dtos;

public class PlaceDto
{
    [Required, StringLength(150, MinimumLength = 2)]
    public string Name { get; set; } = "";

    [Required, StringLength(4000, MinimumLength = 10)]
    public string Description { get; set; } = "";

    // Compatibility mapping: stored in Place.Location
    [Required, StringLength(150)]
    public string TemplateType { get; set; } = "";

    [Range(0, 10000000)]
    public decimal Price { get; set; }

    [Url]
    public string? ImageUrl { get; set; }

    // Compatibility mapping: stored in Place.GoogleMapsUrl
    [Url]
    public string? GoogleDriveUrl { get; set; }
}