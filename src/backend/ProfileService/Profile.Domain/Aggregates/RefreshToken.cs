using Profile.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Domain.Aggregates
{
    public class RefreshToken : IEntity
    {
        public long Id { get; set; }
        public required string Value { get; set; }
        public required long UserId { get; set; }
        public required long DeviceId { get; set; }
        public DateTime RefreshTokenExpiration { get; set; }
    }
}
