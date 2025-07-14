using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Domain.Exceptions
{
    public class ClientException : BaseException
    {
        public override List<string> Errors { get; set; } = [];

        public ClientException(string error) : base(error)
        {
            Errors.Add(error);
        }

        public ClientException(List<string> errors) : base(errors)
        {
            Errors = errors;
        }

        public override List<string> GetMessage() => Errors;
    }
}
