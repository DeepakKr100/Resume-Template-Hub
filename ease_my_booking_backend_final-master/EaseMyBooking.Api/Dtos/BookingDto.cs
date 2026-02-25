using System.ComponentModel.DataAnnotations;

namespace EaseMyBooking.Api.Dtos;

public class BookingDto
{
    [Required]
    public int PlaceId { get; set; }

    // Optional for digital product flow; backend will default to today if null
    public DateTime? VisitDate { get; set; }

    [Range(1, 1000)]
    public int Quantity { get; set; } = 1;
}