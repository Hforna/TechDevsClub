using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Domain.Exceptions
{
    public class JsonErrorResponse : SystemException
    {
        public IList<string> Errors { get; set; } = new List<string>();

        public JsonErrorResponse(IList<string> errors) => Errors = errors;

        public JsonErrorResponse(string message) => Errors.Add(message);
    }
}
