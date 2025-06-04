using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Application.Responses
{
    public sealed record LoginResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
