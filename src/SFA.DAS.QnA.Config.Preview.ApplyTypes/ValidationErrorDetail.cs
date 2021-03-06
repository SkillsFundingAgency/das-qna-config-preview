﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.QnA.Config.Preview.ApplyTypes
{
    public class ValidationErrorDetail
    {
        public ValidationErrorDetail()
        {
        }

        public ValidationErrorDetail(string field, string errorMessage)
        {
            Field = field;
            ErrorMessage = errorMessage;
        }

        public ValidationErrorDetail(string field, string errorMessage, ValidationStatusCode statusCode)
        {
            Field = field;
            ErrorMessage = errorMessage;
            ValidationStatusCode = statusCode;
        }

        public ValidationErrorDetail(string errorMessage, ValidationStatusCode statusCode)
        {
            ErrorMessage = errorMessage;
            ValidationStatusCode = statusCode;
        }

        public string Field { get; set; }
        public string ErrorMessage { get; set; }
        public ValidationStatusCode ValidationStatusCode { get; set; }


        public string StatusCode => ValidationStatusCode.ToString();
    }
}
