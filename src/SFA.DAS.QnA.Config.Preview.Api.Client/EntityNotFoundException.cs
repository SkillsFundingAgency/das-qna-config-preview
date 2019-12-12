using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.QnA.Config.Preview.Api.Client
{
    public class EntityNotFoundException : ApplicationException
    {
        public EntityNotFoundException()
        {

        }

        public EntityNotFoundException(string message) : base(message)
        {

        }
        public EntityNotFoundException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
