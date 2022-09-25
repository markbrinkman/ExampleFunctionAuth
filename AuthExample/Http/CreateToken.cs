using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Threading;

namespace AuthExample.Http
{
    public class CreateToken
    {
        private readonly JwtSettings _settings;
        private readonly SignatureHelper _signatureHelper;

        public CreateToken(IOptions<JwtSettings> options, SignatureHelper signatureHelper)
        {
            _settings = options.Value;
            _signatureHelper = signatureHelper;
        }

        [FunctionName(nameof(CreateToken))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log,
            CancellationToken cancellationToken)
        {
            var jwtId = _settings.Jti;

            var header = _makeHeader();
            var payload = _makePayload(DateTime.UtcNow.AddDays(5), jwtId);
            var signature = await _makeSignatureAsync($"{header}.{payload}", cancellationToken);

            return new OkObjectResult($"{header}.{payload}.{signature}");
        }

        private string _makeHeader()
        {
            var headerJson = JsonConvert.SerializeObject(new Dictionary<string, string>
            {
                [JwtHeaderParameterNames.Typ] = "JWT",
                [JwtHeaderParameterNames.Alg] = SignatureHelper.Algorithm.ToString(),
                [JwtHeaderParameterNames.Kid] = _signatureHelper.KeyId
            });

            return Base64UrlEncoder.Encode(headerJson);
        }

        private string _makePayload(DateTime expires, Guid jwtId)
        {
            var claims = new Claim[]
            {
                new(JwtRegisteredClaimNames.Jti, jwtId.ToString()),
            };

            return new JwtSecurityToken("Test", claims: claims, expires: expires)
                .EncodedPayload;
        }

        private async Task<string> _makeSignatureAsync(
            string headerAndPayload,
            CancellationToken cancellationToken)
        {
            var signature = await _signatureHelper.SignAsync(
                Encoding.UTF8.GetBytes(headerAndPayload),
                cancellationToken);

            return Base64UrlEncoder.Encode(signature);
        }
    }
}
