using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AtomicLmsCore.IntegrationTests.Common;

public class TestAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Check if this is a test that should be unauthenticated
        // If no test-specific headers are present, fail authentication
        var hasTestRole = Context.Request.Headers.ContainsKey("X-Test-Role");
        var hasTestPermissions = Context.Request.Headers.ContainsKey("X-Test-Permissions");
        var hasTestTenant = Context.Request.Headers.ContainsKey("X-Test-Tenant");
        var hasTestAuth = Context.Request.Headers.ContainsKey("X-Test-Auth");

        if (!hasTestRole && !hasTestPermissions && !hasTestTenant && !hasTestAuth)
        {
            return Task.FromResult(AuthenticateResult.Fail("No test authentication headers provided"));
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "Test User"),
            new(ClaimTypes.NameIdentifier, "test-user-id"),
            new(ClaimTypes.Email, "test@example.com"),
        };

        if (Context.Request.Headers.TryGetValue("X-Test-Role", out var roleValue))
        {
            var roles = roleValue.ToString().Split(',');
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role.Trim())));
        }

        if (Context.Request.Headers.TryGetValue("X-Test-Permissions", out var permissionValue))
        {
            var permissions = permissionValue.ToString().Split(',');
            claims.AddRange(permissions.Select(permission => new Claim("permission", permission.Trim())));
        }

        if (Context.Request.Headers.TryGetValue("X-Test-Tenant", out var tenantValue))
        {
            var tenantIds = tenantValue.ToString().Split(',');
            claims.AddRange(tenantIds.Select(tenantId => new Claim("tenant_id", tenantId.Trim())));
        }

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
