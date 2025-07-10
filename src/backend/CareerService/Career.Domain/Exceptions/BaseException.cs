using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Career.Domain.Exceptions
{
    public abstract class BaseException : SystemException
    {
        public abstract List<string> Errors { get; set; }

        public BaseException(string error) : base(error)
        {
            Errors.Add(error);
        }

        public BaseException(List<string> error) : base(string.Empty)
        {
            Errors = error;
        }

        public abstract List<string> GetMessage();
    }
}
