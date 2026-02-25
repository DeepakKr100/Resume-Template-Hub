namespace EaseMyBooking.Api.Models;

public class PlaceImage
{
    public int PlaceImageId { get; set; }

    public int PlaceId { get; set; }
    public Place? Place { get; set; }

    public string Url { get; set; } = "";
    public string PublicId { get; set; } = "";

    public int SortOrder { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
