using System.ComponentModel.DataAnnotations;

namespace AspireApp.BedRock.PaymentGateway.Models
{
    public class PaymentOrderRequest
    {
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
        public decimal Amount { get; set; }
        
        [Required]
        [RegularExpression("^[A-Z]{3}$", ErrorMessage = "Invalid currency code")]
        public string Currency { get; set; }
        
        [Required]
        [RegularExpression("^[a-zA-Z0-9-_]+$", ErrorMessage = "Invalid receipt ID format")]
        public string ReceiptId { get; set; }
    }
}