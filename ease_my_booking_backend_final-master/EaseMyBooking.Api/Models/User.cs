using System.Text.Json.Serialization;

namespace EaseMyBooking.Api.Models;

public enum Role { Visitor, Owner, Admin }

public class User
{
    public int UserId { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public Role Role { get; set; } = Role.Visitor;

    [JsonIgnore] public ICollection<Place>? PlacesOwned { get; set; }
    [JsonIgnore] public ICollection<Booking>? Bookings { get; set; }
    [JsonIgnore] public ICollection<Review>? Reviews { get; set; }
}
