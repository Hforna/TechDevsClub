using Profile.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using X.PagedList;

namespace Profile.Domain.Repositories
{
    public interface IConnectionRepository
    {
        public Task<IPagedList<Connection>> ProfileConnectionsPaged(long profileId, int page, int perPage);
        public Task<bool> UsersAreConnected(long connectorId, long connectedId);
        public Task<bool> ConnectionExists(long connectorId, long connectedId);
        public Task<Connection?> ConnectionByConnectedAndConnector(long connectorId, long connectedId);
    }
}
