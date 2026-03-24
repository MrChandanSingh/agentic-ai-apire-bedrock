using System;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AspireApp.BedRock.PaymentGateway.Logging
{
    public class PaymentAuditLogger
    {
        private readonly ILogger<PaymentAuditLogger> _logger;
        private readonly string _applicationName;

        public PaymentAuditLogger(ILogger<PaymentAuditLogger> logger, string applicationName = "RazorpayGateway")
        {
            _logger = logger;
            _applicationName = applicationName;
        }

        public void LogPaymentAttempt(PaymentAuditEvent auditEvent)
        {
            var logEntry = new
            {
                Timestamp = DateTime.UtcNow,
                Application = _applicationName,
                EventType = "PaymentAttempt",
                UserId = auditEvent.UserId,
                Amount = auditEvent.Amount,
                Currency = auditEvent.Currency,
                OrderId = auditEvent.OrderId,
                IpAddress = auditEvent.IpAddress,
                UserAgent = auditEvent.UserAgent,
                Status = auditEvent.Status
            };

            _logger.LogInformation(
                "Payment {Status}: User={UserId}, Amount={Amount}, Currency={Currency}, IP={IpAddress}",
                auditEvent.Status, auditEvent.UserId, auditEvent.Amount, auditEvent.Currency, auditEvent.IpAddress);

            // Detailed audit log
            _logger.LogInformation("AuditEvent: {AuditData}", JsonSerializer.Serialize(logEntry));
        }

        public void LogSecurityEvent(string userId, string eventType, string details, string ipAddress)
        {
            var logEntry = new
            {
                Timestamp = DateTime.UtcNow,
                Application = _applicationName,
                EventType = eventType,
                UserId = userId,
                IpAddress = ipAddress,
                Details = details
            };

            _logger.LogWarning(
                "Security Event: Type={EventType}, User={UserId}, IP={IpAddress}, Details={Details}",
                eventType, userId, ipAddress, details);

            // Detailed security log
            _logger.LogInformation("SecurityEvent: {AuditData}", JsonSerializer.Serialize(logEntry));
        }
    }

    public class PaymentAuditEvent
    {
        public string UserId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string OrderId { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public string Status { get; set; }
    }
}