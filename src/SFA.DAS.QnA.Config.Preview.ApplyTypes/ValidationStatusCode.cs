using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace SFA.DAS.QnA.Config.Preview.ApplyTypes
{
    public enum ValidationStatusCode
    {
        [EnumMember(Value = "BadRequest")]
        BadRequest,
        [EnumMember(Value = "AlreadyExists")]
        AlreadyExists,
        [EnumMember(Value = "NotFound")]
        NotFound
    }
}
