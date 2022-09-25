using Azure.Core;
using Azure.Security.KeyVault.Keys.Cryptography;
using Microsoft.Extensions.Options;
using System.Threading;
using System.Threading.Tasks;

namespace AuthExample;

public class SignatureHelper
{
    public static SignatureAlgorithm Algorithm => SignatureAlgorithm.RS256;

    public string KeyId => _cryptographyClient.KeyId;

    private readonly CryptographyClient _cryptographyClient;

    public SignatureHelper(IOptions<JwtSettings> settings, TokenCredential credential)
    {
        _cryptographyClient = new CryptographyClient(settings.Value.SigningKeyUrl, credential);
    }

    public async Task<byte[]> SignAsync(byte[] data, CancellationToken cancellationToken)
    {
        return (await _cryptographyClient
                .SignDataAsync(Algorithm, data, cancellationToken))
            .Signature;
    }

    public bool Verify(byte[] data, byte[] signature)
    {
        return _cryptographyClient.VerifyData(Algorithm, data, signature).IsValid;
    }
}
