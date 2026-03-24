using System.ComponentModel.DataAnnotations;

namespace AspireApp.BedRock.PaymentGateway.Models
{
    public class PaymentSettings
    {
        [Required]
        public string RazorpayKey { get; set; }
        [Required]
        public string RazorpaySecret { get; set; }
        [Required]
        public string CompanyName { get; set; }
        [Required]
        public string Description { get; set; }
    }
}