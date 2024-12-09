using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Client;
using SFA.DAS.QnA.Config.Preview.Settings;
using System;

namespace SFA.DAS.QnA.Config.Preview.Api.Client
{
    public class TokenService : ITokenService
    {

        private readonly IWebConfiguration _configuration;
        private readonly IHostEnvironment _hostingEnvironment;

        public TokenService(IWebConfiguration configuration, IHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
            _configuration = configuration;
        }

        public string GetToken()
        {
            if (_hostingEnvironment.IsDevelopment())
                return string.Empty;

            var tenantId = _configuration.QnaApiAuthentication.TenantId;
            var clientId = _configuration.QnaApiAuthentication.ClientId;
            var clientSecret = _configuration.QnaApiAuthentication.ClientSecret;
            var scope = new[] { _configuration.QnaApiAuthentication.ResourceId + "/.default" };

            var app = ConfidentialClientApplicationBuilder.Create(clientId)
                .WithClientSecret(clientSecret)
                .WithAuthority(new Uri($"https://login.microsoftonline.com/{tenantId}"))
                .Build();

            var result = app.AcquireTokenForClient(scope).ExecuteAsync().Result;

            return result.AccessToken;
        }
    }
}
