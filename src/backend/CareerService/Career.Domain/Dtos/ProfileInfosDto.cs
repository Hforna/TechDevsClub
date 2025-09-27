using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Domain.Dtos
{
    public sealed record ProfileInfosDto
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public bool IsPrivate { get; set; }
    }
}
