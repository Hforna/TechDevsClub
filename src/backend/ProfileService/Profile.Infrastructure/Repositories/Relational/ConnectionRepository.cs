using Microsoft.EntityFrameworkCore;
using Profile.Domain.Entities;
using Profile.Domain.Repositories;
using Profile.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using X.PagedList;
using X.PagedList.Extensions;

namespace Profile.Infrastructure.Repositories.Relational
{
    public class ConnectionRepository : IConnectionRepository
    {
        private readonly DataContext _context;

        public ConnectionRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<Connection?> ConnectionByConnectedAndConnector(long connectorId, long connectedId)
        {
            return await _context.Connections.SingleOrDefaultAsync(d => d.ConnectedId == connectedId && d.ConnectorId == connectorId);
        }

        public async Task<bool> ConnectionExists(long connectorId, long connectedId)
        {
            return await _context.Connections.AnyAsync(d => d.ConnectorId == connectedId && d.ConnectedId == connectedId);
        }

        public async Task<IPagedList<Connection>> ProfileConnectionsPaged(long profileId, int page, int perPage)
        {
            var connections = await _context.Connections
                .AsNoTracking()
                .Where(d => d.ConnectorId == profileId)
                .ToListAsync();

            return connections.ToPagedList(page, perPage);
        }

        public async Task<bool> UsersAreConnected(long connectorId, long connectedId)
        {
            return await _context.Connections.AnyAsync(d => d.ConnectorId == connectedId && d.ConnectedId == connectorId);
        }
    }
}
