using System;

namespace AuthExample;

public class JwtSettings
{
    public Guid Jti { get; set; }
    public Uri SigningKeyUrl { get; set; }
}
