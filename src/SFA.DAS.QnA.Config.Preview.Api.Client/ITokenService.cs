using Newtonsoft.Json;
using SFA.DAS.QnA.Config.Preview.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.QnA.Config.Preview.Api.Client
{
    public interface ITokenService
    {
        string GetToken();
    }
}
