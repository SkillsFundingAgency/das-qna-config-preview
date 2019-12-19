using Newtonsoft.Json;
using System;

namespace SFA.DAS.QnA.Config.Preview.Settings
{
    public class WebConfiguration : IWebConfiguration
    {
        [JsonRequired] public ClientApiAuthentication QnaApiAuthentication { get; set; }
        [JsonRequired] public string QnaSqlConnectionString { get; set; }
    }
}
