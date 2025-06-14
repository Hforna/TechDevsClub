using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Application.Responses
{
    public class ShortProfileResponse
    {
        public string ProfileId { get; set; }
        public string UserName { get; set; }
        public string ProfilePicture { get; set; }
        public bool IsPrivate { get; set; }
    }
}
