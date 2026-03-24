using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace AspireApp.BedRock.PaymentGateway.Security
{
    public static class PaymentSecurityMiddleware
    {
        public static IServiceCollection AddPaymentSecurity(this IServiceCollection services)
        {
            // Add Data Protection
            services.AddDataProtection();

            // Add Rate Limiting
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                // Global rate limit
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 100,
                            Window = TimeSpan.FromMinutes(1)
                        }
                    )
                );

                // Payment endpoint specific limits
                options.AddPolicy("payment", context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 10,
                            Window = TimeSpan.FromMinutes(1)
                        }
                    )
                );
            });

            return services;
        }

        public static IApplicationBuilder UsePaymentSecurity(this IApplicationBuilder app)
        {
            // Add security headers
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Add("X-Frame-Options", "DENY");
                context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
                context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
                context.Response.Headers.Add("Permissions-Policy", "payment=self");
                
                // Content Security Policy
                context.Response.Headers.Add("Content-Security-Policy", 
                    "default-src 'self'; " +
                    "script-src 'self' https://checkout.razorpay.com; " +
                    "frame-src 'self' https://api.razorpay.com; " +
                    "img-src 'self' https://rzp-logo.s3.amazonaws.com; " +
                    "style-src 'self' 'unsafe-inline'; " + // Required for Blazor
                    "connect-src 'self' https://api.razorpay.com; " +
                    "form-action 'self';");

                await next();
            });

            // Enable rate limiting
            app.UseRateLimiter();

            return app;
        }
    }
}