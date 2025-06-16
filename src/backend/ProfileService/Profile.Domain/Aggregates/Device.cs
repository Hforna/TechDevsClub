using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Profile.Domain.Entities;
using Profile.Domain.Services.External;
using Profile.Domain.ValueObjects;

namespace Profile.Domain.Aggregates
{
    [Table("devices")]
    public class Device : IEntity
    {
        public DateTime CreatedAt { get; set; }
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Model { get; set; }
        public string OsType { get; set; }
        public DateTime LastAccess { get; set; }
        public DeviceLocation Location { get; set; }
    }
}
