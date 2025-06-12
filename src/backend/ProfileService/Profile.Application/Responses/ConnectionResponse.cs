using Profile.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Application.Responses
{
    public class ConnectionResponse
    {
        public string Id { get; set; }
        public string ConnectorId { get; set; }
        public string ConnectedId { get; set; }
        public ConnectionStatus Statuts { get; set; }
        public bool AreConnected { get; set; }
    }
}
