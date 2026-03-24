using System.Text.RegularExpressions;

namespace AspireApp.BedRock.PaymentGateway.Utilities
{
    public static class LogMasker
    {
        public static string MaskSensitiveData(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Mask payment IDs (keep first 4 and last 4 characters)
            input = Regex.Replace(input, @"(pay_\w{4})\w+(\w{4})", "$1****$2");
            
            // Mask order IDs (keep first 4 and last 4 characters)
            input = Regex.Replace(input, @"(order_\w{4})\w+(\w{4})", "$1****$2");
            
            // Mask any key-like strings (alphanumeric strings longer than 20 characters)
            input = Regex.Replace(input, @"([a-zA-Z0-9]{4})[a-zA-Z0-9]{16,}([a-zA-Z0-9]{4})", "$1****$2");
            
            return input;
        }
    }
}