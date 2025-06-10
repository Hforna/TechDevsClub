using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Domain.Exceptions
{
    public class ExternalServiceException : BaseException
    {
        public List<string> Errors { get; set; } = [];
        public HttpStatusCode StatusCode { get; set; }

        public ExternalServiceException(string error, HttpStatusCode statusCode) : base(error, statusCode)
        {
            Errors.Add(error);
            StatusCode = statusCode;
        }

        public ExternalServiceException(List<string> error, HttpStatusCode statusCode) : base(string.Empty, statusCode)
        {
            Errors = error;
            StatusCode = statusCode;
        }

        public override List<string> GetMessage() => Errors;

        public override HttpStatusCode GetStatusCode() => StatusCode;
    }
}
