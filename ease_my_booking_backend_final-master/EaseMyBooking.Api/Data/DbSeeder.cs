using System.Security.Cryptography;
using System.Text;
using EaseMyBooking.Api.Models;

namespace EaseMyBooking.Api.Data;

public static class DbSeeder
{
    public static void Seed(ApplicationDbContext db)
    {
        if (db.Users.Any()) return;

        static string Hash(string value)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(value));
            return Convert.ToBase64String(bytes);
        }

        var admin = new User { Name = "Admin User", Email = "admin@easebook.com", PasswordHash = Hash("Admin@123"), Role = Role.Admin };
        var owner = new User { Name = "Alice Owner", Email = "alice@easebook.com", PasswordHash = Hash("Owner@123"), Role = Role.Owner };
        var visitor = new User { Name = "Bob Visitor", Email = "bob@easebook.com", PasswordHash = Hash("Visitor@123"), Role = Role.Visitor };
        db.Users.AddRange(admin, owner, visitor);
        db.SaveChanges();

        var museum = new Place
        {
            Name = "City Museum",
            Description = "A historical city museum with ancient artifacts.",
            Location = "Downtown",
            Timings = "9 AM - 5 PM",
            Price = 50.00m,
            ImageUrl = "",
            OwnerId = owner.UserId
        };
        var zoo = new Place
        {
            Name = "Wildlife Zoo",
            Description = "Zoo with a wide variety of animals.",
            Location = "Uptown",
            Timings = "8 AM - 6 PM",
            Price = 30.00m,
            ImageUrl = "",
            OwnerId = owner.UserId
        };
        db.Places.AddRange(museum, zoo);
        db.SaveChanges();

        db.Bookings.Add(new Booking
        {
            PlaceId = museum.PlaceId,
            UserId = visitor.UserId,
            VisitDate = DateTime.Today.AddDays(7),
            Quantity = 2,
            PaymentConfirmed = false
        });
        db.SaveChanges();
    }
}
