using System.ComponentModel.DataAnnotations;

namespace EaseMyBooking.Api.Dtos;

public class ReviewDto
{
    [Required] public int PlaceId { get; set; }
    [Range(1, 5)] public int Rating { get; set; }
    [Required, MinLength(10)] public string Comment { get; set; } = string.Empty;
}
