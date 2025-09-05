using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Application.Requests
{
    public class ProfileFilterRequest
    {
        public int Page { get; set; }
        public int PerPage { get; set; }
        public string? UserName { get; set; }
    }
}
