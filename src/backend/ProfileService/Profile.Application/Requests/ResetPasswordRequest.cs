using Org.BouncyCastle.Bcpg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Application.Requests
{
    public class ResetPasswordRequest
    {
        public string NewPassword { get; set; }
        public string RepeatPassword { get; set; }
    }
}
