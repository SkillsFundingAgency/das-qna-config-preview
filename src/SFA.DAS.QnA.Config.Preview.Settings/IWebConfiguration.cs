using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.QnA.Config.Preview.Settings
{
    public interface IWebConfiguration
    {
        ClientApiAuthentication QnaApiAuthentication { get; set; }
        string QnaSqlConnectionString { get; set; }
    }
}
