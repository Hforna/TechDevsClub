using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Domain.Exceptions
{
    public abstract class BaseException : SystemException
    {
        public List<string> Errors { get; set; } = [];
        public HttpStatusCode StatusCode { get; set; }

        public BaseException(string error, HttpStatusCode statusCode) : base(error)
        {
            Errors.Add(error);
            StatusCode = statusCode;
        }

        public BaseException(List<string> error, HttpStatusCode statusCode) : base(string.Empty)
        {
            Errors = error;
            StatusCode = statusCode;
        }

        public abstract HttpStatusCode GetStatusCode();
        public abstract List<string> GetMessage();
    }
}
