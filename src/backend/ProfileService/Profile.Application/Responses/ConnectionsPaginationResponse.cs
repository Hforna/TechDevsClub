using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Application.Responses
{
    public class ConnectionsPaginationResponse
    {
        public int PageNumber { get; set; }
        public int Count { get; set; }
        public bool IsFirstPage { get; set; }
        public bool HasNextPage { get; set; }
        public List<ConnectionResponse> Connections { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool IsLastPage { get; set; }
    }
}
