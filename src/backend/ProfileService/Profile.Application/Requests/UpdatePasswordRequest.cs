using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Application.Requests
{
    public class UpdatePasswordRequest
    {
        public string Password { get; set; }
        public string NewPassword { get; set; }
        public string RepeatPassword { get; set; }
    }
}
