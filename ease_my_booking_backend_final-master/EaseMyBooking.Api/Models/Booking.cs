namespace EaseMyBooking.Api.Models;

public class Booking
{
    public int BookingId { get; set; }
    public int PlaceId { get; set; }
    public Place? Place { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public DateTime VisitDate { get; set; }
    public int Quantity { get; set; }
    public DateTime BookingTime { get; set; } = DateTime.UtcNow;
    public bool PaymentConfirmed { get; set; }
}
