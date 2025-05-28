using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Domain.Exceptions
{
    public class DomainException : BaseException
    {
        public override List<string> Errors { get; set; }
        public override HttpStatusCode StatusCode { get; set; }

        public DomainException(string error, HttpStatusCode statusCode) : base(error, statusCode)
        {
            Errors.Add(error);
            StatusCode = statusCode;
        }

        public DomainException(List<string> error, HttpStatusCode statusCode) : base(string.Empty, statusCode)
        {
            Errors = error;
            StatusCode = statusCode;
        }

        public override List<string> GetMessage() => [Message];

        public override HttpStatusCode GetStatusCode() => StatusCode;
    }
}
