using Newtonsoft.Json;

namespace SFA.DAS.QnA.Config.Preview.Settings
{
    public class WebConfiguration : IWebConfiguration
    {
        [JsonRequired] public ClientApiAuthentication QnaApiAuthentication { get; set; }
    }
}
