using System.ComponentModel.DataAnnotations;

namespace AspireApp.BedRock.PaymentGateway.Models;

public class DeliveryFormModel
{
    [Required(ErrorMessage = "Full name is required")]
    [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Address line 1 is required")]
    [StringLength(100, ErrorMessage = "Address line 1 cannot exceed 100 characters")]
    public string AddressLine1 { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Address line 2 cannot exceed 100 characters")]
    public string? AddressLine2 { get; set; }

    [Required(ErrorMessage = "City is required")]
    [StringLength(50, ErrorMessage = "City cannot exceed 50 characters")]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "State is required")]
    [StringLength(50, ErrorMessage = "State cannot exceed 50 characters")]
    public string State { get; set; } = string.Empty;

    [Required(ErrorMessage = "ZIP code is required")]
    [RegularExpression(@"^\d{5}(-\d{4})?$", ErrorMessage = "Invalid ZIP code format")]
    public string ZipCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required")]
    [RegularExpression(@"^\+?(\d[\d-. ]+)?(\([\d-. ]+\))?[\d-. ]+\d$", ErrorMessage = "Invalid phone number format")]
    public string Phone { get; set; } = string.Empty;
}