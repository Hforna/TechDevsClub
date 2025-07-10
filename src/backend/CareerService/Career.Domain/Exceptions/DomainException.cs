using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Career.Domain.Exceptions
{
    public class DomainException : BaseException
    {
        public override List<string> Errors { get; set; } = [];

        public DomainException(string error) : base(error)
        {
            Errors.Add(error);
        }

        public DomainException(List<string> errors) : base(errors)
        {
            Errors = errors;
        }

        public override List<string> GetMessage() => Errors;
    }
}
