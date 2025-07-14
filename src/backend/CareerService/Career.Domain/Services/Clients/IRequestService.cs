using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Domain.Services.Clients
{
    public interface IRequestService
    {
        public string? GetBearerToken();
    }
}
