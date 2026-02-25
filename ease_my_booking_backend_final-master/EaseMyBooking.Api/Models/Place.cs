using System.Text.Json.Serialization;

namespace EaseMyBooking.Api.Models;

public class Place
{
    public int PlaceId { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Location { get; set; } = "";
    public string Timings { get; set; } = "";
    public decimal Price { get; set; }
    public string ImageUrl { get; set; } = ""; // legacy single image

    // NEW: Google Maps link for the place (owner-provided)
    public string? GoogleMapsUrl { get; set; } = null;

    public int OwnerId { get; set; }
    public User? Owner { get; set; }

    [JsonIgnore] public ICollection<Booking>? Bookings { get; set; }
    public ICollection<Review>? Reviews { get; set; }

    // NEW: multiple images
    public ICollection<PlaceImage>? Images { get; set; }
}
