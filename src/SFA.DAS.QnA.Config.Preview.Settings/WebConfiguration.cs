using Newtonsoft.Json;

namespace SFA.DAS.QnA.Config.Preview.Settings
{
    public class WebConfiguration : IWebConfiguration
    {
        [JsonRequired] public QnaApiClientConfiguration QnaApiAuthentication { get; set; }
    }
}
