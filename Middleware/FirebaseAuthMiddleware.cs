using FionetixAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace FionetixAPI.Middleware;

public class FirebaseAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<FirebaseAuthMiddleware> _logger;
    private readonly bool _isDevelopment;

    public FirebaseAuthMiddleware(RequestDelegate next, ILogger<FirebaseAuthMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _isDevelopment = env.IsDevelopment();
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext db)
    {
        // Skip auth for OpenAPI/Swagger endpoints
        var path = context.Request.Path.Value ?? "";
        if (path.StartsWith("/openapi"))
        {
            await _next(context);
            return;
        }

        // Development bypass: X-Dev-Email header for testing without Firebase
        if (_isDevelopment && context.Request.Headers.TryGetValue("X-Dev-Email", out var devEmail))
        {
            var devUser = await db.AppUsers.FirstOrDefaultAsync(u => u.Email == devEmail.ToString());
            if (devUser != null)
            {
                context.Items["UserId"] = devUser.Id;
                context.Items["UserEmail"] = devUser.Email;
                context.Items["UserRole"] = devUser.Role;
                await _next(context);
                return;
            }
        }

        // Extract Bearer token
        var authHeader = context.Request.Headers.Authorization.ToString();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Missing or invalid authorization token." });
            return;
        }

        var token = authHeader["Bearer ".Length..];

        try
        {
            // Verify Firebase token
            var decodedToken = await FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(token);
            var uid = decodedToken.Uid;
            var email = decodedToken.Claims.TryGetValue("email", out var emailClaim) ? emailClaim.ToString() : null;

            // Lookup user by Firebase UID or email
            var user = await db.AppUsers.FirstOrDefaultAsync(u => u.FirebaseUid == uid || u.Email == email);

            if (user == null)
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsJsonAsync(new { error = "User not registered in the system." });
                return;
            }

            // Update FirebaseUid if it was a placeholder
            if (user.FirebaseUid != uid)
            {
                user.FirebaseUid = uid;
                await db.SaveChangesAsync();
            }

            context.Items["UserId"] = user.Id;
            context.Items["UserEmail"] = user.Email;
            context.Items["UserRole"] = user.Role;
            await _next(context);
        }
        catch (FirebaseAdmin.Auth.FirebaseAuthException)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid or expired token." });
        }
        catch (Exception ex) when (_isDevelopment)
        {
            // In dev, if Firebase is not initialized, allow X-Dev-Email fallback only
            _logger.LogWarning("Firebase auth failed: {Message}. Use X-Dev-Email header for dev testing.", ex.Message);
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Authentication failed. In development, use X-Dev-Email header." });
        }
    }
}

public static class FirebaseAuthMiddlewareExtensions
{
    public static IApplicationBuilder UseFirebaseAuth(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<FirebaseAuthMiddleware>();
    }
}
