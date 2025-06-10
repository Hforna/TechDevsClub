using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Application.Responses
{
    public class GithubMetadataResponse
    {
        public string Username { get; set; }
        public int Repositories { get; set; }
        public int Commits { get; set; }
    }
}
