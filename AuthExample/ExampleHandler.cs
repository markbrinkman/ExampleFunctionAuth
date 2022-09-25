using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace AuthExample;

internal class ExampleHandler : JwtBearerHandler
{
    private readonly JwtSettings _settings;

    public ExampleHandler(
        IOptionsMonitor<JwtBearerOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IOptions<JwtSettings> jwtOptions)
        : base(options, logger, encoder, clock)
    {
        _settings = jwtOptions.Value;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var result = await base.HandleAuthenticateAsync();

        if (!result.Succeeded)
        {
            return result;
        }

        var jwtIdClaim = result.Principal!.FindFirst(JwtRegisteredClaimNames.Jti)!.Value;

        if (!Guid.TryParse(jwtIdClaim, out var jwtId))
        {
            return AuthenticateResult.Fail("JWT ID claim was not found on token.");
        }

        if (jwtId != _settings.Jti)
        {
            return AuthenticateResult.Fail("Token is not registered or revoked.");
        }

        return result;
    }
}
