using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthExample;

internal class CustomValidator : JwtSecurityTokenHandler
{
    private readonly SignatureHelper _signatureHelper;

    public CustomValidator(SignatureHelper signatureHelper)
    {
        _signatureHelper = signatureHelper;
    }

    protected override JwtSecurityToken ValidateSignature(
        string token, TokenValidationParameters validationParameters)
    {
        var jwt = ReadJwtToken(token);

        if (!_verify(jwt))
        {
            throw new Exception();
        }

        return jwt;
    }

    public bool _verify(JwtSecurityToken jwt)
    {
        var headerAndPayload = $"{jwt.RawHeader}.{jwt.RawPayload}";
        var headerAndPayloadBytes = Encoding.UTF8.GetBytes(headerAndPayload);
        var signatureBytes = Base64UrlEncoder.DecodeBytes(jwt.RawSignature);

        return _signatureHelper.Verify(headerAndPayloadBytes, signatureBytes);
    }
}
