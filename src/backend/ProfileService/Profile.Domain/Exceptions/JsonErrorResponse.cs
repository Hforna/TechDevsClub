using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Domain.Exceptions
{
    public class JsonErrorResponse : BaseException
    {
        public List<string> Errors { get; set; } = [];
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.BadRequest;

        public JsonErrorResponse(List<string> errors, HttpStatusCode statusCode) : base(string.Empty, statusCode)
        {
            Errors = errors;
            StatusCode = (HttpStatusCode)statusCode;
        }

        public JsonErrorResponse(string error, HttpStatusCode statusCode) : base(error, statusCode)
        {
            Errors.Add(error);
            statusCode = (HttpStatusCode)statusCode;
        }

        public override HttpStatusCode GetStatusCode() => StatusCode;
        public override List<string> GetMessage() => [Message];
    }
}
