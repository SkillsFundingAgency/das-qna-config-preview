using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.QnA.Config.Preview.Settings
{
    public class ClientApiAuthentication: IClientApiAuthentication
    {
        [JsonRequired] public string Instance { get; set; }

        [JsonRequired] public string TenantId { get; set; }

        [JsonRequired] public string ClientId { get; set; }

        [JsonRequired] public string ClientSecret { get; set; }

        [JsonRequired] public string ResourceId { get; set; }

        [JsonRequired] public string ApiBaseAddress { get; set; }
    }
}
