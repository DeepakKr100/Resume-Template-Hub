using System.Text.Json.Serialization;

namespace EaseMyBooking.Api.Models;

public class Review
{
    public int ReviewId { get; set; }
    public int PlaceId { get; set; }
    [JsonIgnore] public Place? Place { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public int Rating { get; set; }
    public string Comment { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
