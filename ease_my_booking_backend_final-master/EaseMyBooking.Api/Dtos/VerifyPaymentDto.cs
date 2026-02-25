using System.ComponentModel.DataAnnotations;

namespace EaseMyBooking.Api.Dtos;

public class VerifyPaymentDto
{
    [Required] public int BookingId { get; set; }
    [Required] public string OrderId { get; set; } = string.Empty;     // razorpay_order_id
    [Required] public string PaymentId { get; set; } = string.Empty;   // razorpay_payment_id
    [Required] public string Signature { get; set; } = string.Empty;   // razorpay_signature
}
