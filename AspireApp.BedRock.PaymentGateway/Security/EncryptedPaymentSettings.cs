using System;
using Microsoft.AspNetCore.DataProtection;

namespace AspireApp.BedRock.PaymentGateway.Security
{
    public class EncryptedPaymentSettings
    {
        private readonly IDataProtector _protector;

        public EncryptedPaymentSettings(IDataProtectionProvider provider)
        {
            _protector = provider.CreateProtector("Payment.RazorpayKeys");
        }

        public string EncryptedRazorpayKey { get; set; }
        public string EncryptedRazorpaySecret { get; set; }
        public string CompanyName { get; set; }
        public string Description { get; set; }

        public string GetDecryptedKey() => _protector.Unprotect(EncryptedRazorpayKey);
        public string GetDecryptedSecret() => _protector.Unprotect(EncryptedRazorpaySecret);

        public void EncryptKeys(string razorpayKey, string razorpaySecret)
        {
            EncryptedRazorpayKey = _protector.Protect(razorpayKey);
            EncryptedRazorpaySecret = _protector.Protect(razorpaySecret);
        }
    }
}