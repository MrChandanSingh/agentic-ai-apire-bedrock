namespace AspireApp.BedRock.SonetOps.ApiService.Middleware;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly IDictionary<string, string> SecurityHeaders = new Dictionary<string, string>
    {
        { "X-Content-Type-Options", "nosniff" },
        { "X-Frame-Options", "DENY" },
        { "X-XSS-Protection", "1; mode=block" },
        { "Referrer-Policy", "strict-origin-when-cross-origin" },
        { "Content-Security-Policy", "default-src 'self'; frame-ancestors 'none'" },
        { "Permissions-Policy", "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()" },
        { "X-Permitted-Cross-Domain-Policies", "none" },
        { "Cross-Origin-Opener-Policy", "same-origin" },
        { "Cross-Origin-Resource-Policy", "same-origin" },
        { "Cross-Origin-Embedder-Policy", "require-corp" }
    };

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        foreach (var header in SecurityHeaders)
        {
            if (!context.Response.Headers.ContainsKey(header.Key))
            {
                context.Response.Headers.Add(header.Key, header.Value);
            }
        }

        await _next(context);
    }
}