using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using SFA.DAS.QnA.Config.Preview.Settings;

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
            var appKey = _configuration.QnaApiAuthentication.ClientSecret;
            var resourceId = _configuration.QnaApiAuthentication.ResourceId;

            var authority = $"https://login.microsoftonline.com/{tenantId}";
            var clientCredential = new ClientCredential(clientId, appKey);
            var context = new AuthenticationContext(authority, true);
            var result = context.AcquireTokenAsync(resourceId, clientCredential).Result;

            return result.AccessToken;
        }
    }
}
