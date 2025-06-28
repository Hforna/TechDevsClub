using AutoMapper;
using Profile.Application.Responses;
using Profile.Domain.Aggregates;
using Profile.Domain.Entities;
using Profile.Domain.Exceptions;
using Profile.Domain.Repositories;
using Profile.Domain.Services;
using Profile.Domain.Services.Security;
using Sqids;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Profile.Application.ApplicationServices
{
    public interface IConnectionService
    {
        public Task<ConnectionsPaginationResponse> ProfileConnectionsPagination(long profileId, int page, int perPage);
        public Task<ConnectionResponse> CreateConnection(long profileId);
        public Task<ConnectionResponse> AcceptConnection(long connectionId);
        public Task RejectConnection(long connectionId);

    }

    public class ConnectionService : IConnectionService
    {
        private readonly IRequestService _requestService;
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _uof;
        private readonly IMapper _mapper;

        public ConnectionService(IRequestService requestService, ITokenService tokenService, 
            IUnitOfWork uof, IMapper mapper)
        {
            _requestService = requestService;
            _tokenService = tokenService;
            _uof = uof;
            _mapper = mapper;
        }

        public async Task<ConnectionResponse> AcceptConnection(long connectionId)
        {
            var user = await _tokenService.GetUserByToken();
            var profile = await _uof.ProfileRepository.ProfileByUser(user);

            var connection = await _uof.GenericRepository.GetById<Connection>(connectionId);

            if (connection is null)
                throw new ContextException(ResourceExceptMessages.CONNECTION_BY_ID_NOT_EXISTS, System.Net.HttpStatusCode.NotFound);

            var reverseConnetion = await _uof.ConnectionRepository.ConnectionByConnectedAndConnector(connection.ConnectedId, connection.ConnectorId);

            if(reverseConnetion is not null)
            {
                connection.AreConnected = true;
                reverseConnetion.AreConnected = true;
                _uof.GenericRepository.Update<Connection>(reverseConnetion);
            }

            connection.Accept();

            _uof.GenericRepository.Update<Connection>(connection);
            await _uof.Commit();

            return _mapper.Map<ConnectionResponse>(connection);
        }

        public async Task<ConnectionResponse> CreateConnection(long profileId)
        {
            var user = await _tokenService.GetUserByToken();
            var profile = await _uof.ProfileRepository.ProfileByUser(user!);

            if (profile.Id == profileId) throw new ContextException(ResourceExceptMessages.CONNECT_WITH_YOURSELF, System.Net.HttpStatusCode.Unauthorized);

            var profileConnected = await _uof.GenericRepository.GetById<ProfileEntity>(profileId);

            if (profileConnected is null)
                throw new ContextException(ResourceExceptMessages.PROFILE_NOT_EXISTS, System.Net.HttpStatusCode.NotFound);

            var connection = await CreateConnectionIfNotExists(profile.Id, profileConnected.Id);

            if (profileConnected.IsPrivate == false)
            {
                connection.Accept();

                var reverseConnection = await _uof.ConnectionRepository.ConnectionByConnectedAndConnector(profileConnected.Id, profile.Id);

                if (reverseConnection is not null && reverseConnection.Status == ConnectionStatus.Approved)
                {
                    connection.AreConnected = true;
                    reverseConnection.AreConnected = true;

                    _uof.GenericRepository.Update<Connection>(reverseConnection);
                }
            }
            await _uof.GenericRepository.Add<Connection>(connection);
            await _uof.Commit();    

            return _mapper.Map<ConnectionResponse>(connection);            
        }

        public async Task<ConnectionsPaginationResponse> ProfileConnectionsPagination(long profileId, int page, int perPage)
        {
            if (perPage > 100) throw new ValidationException(ResourceExceptMessages.OUT_OF_RANGE_PER_PAGE_MAX_100, System.Net.HttpStatusCode.BadRequest);

            var profile = await _uof.ProfileRepository.ProfileById(profileId);

            if (profile is null)
                throw new ContextException(ResourceExceptMessages.PROFILE_NOT_EXISTS, System.Net.HttpStatusCode.NotFound);

            var connections = await _uof.ConnectionRepository.ProfileConnectionsPaged(profileId, page, perPage);

            var response = new ConnectionsPaginationResponse()
            {
                HasNextPage = connections.HasNextPage,
                HasPreviousPage = connections.HasPreviousPage,
                IsFirstPage = connections.IsFirstPage,
                Count = connections.Count,
                IsLastPage = connections.IsLastPage,
                PageNumber = connections.PageNumber,
            };
            response.Connections = connections
                .Select(connection => _mapper.Map<ConnectionResponse>(connection))
                .ToList();

            return response;
        }

        public async Task RejectConnection(long connectionId)
        {
            var user = await _tokenService.GetUserByToken();
            var profile = await _uof.ProfileRepository.ProfileByUser(user!);

            var connection = await _uof.GenericRepository.GetById<Connection>(connectionId);

            if (connection is null)
                throw new ContextException(ResourceExceptMessages.CONNECTION_BY_ID_NOT_EXISTS, System.Net.HttpStatusCode.BadRequest);

            _uof.GenericRepository.Delete<Connection>(connection);
            await _uof.Commit();
        }

        private async Task<Connection> CreateConnectionIfNotExists(long profileId, long profileConnectedId)
        {
            var connectionExists = await _uof.ConnectionRepository.ConnectionExists(profileId, profileConnectedId);

            if (!connectionExists)
                throw new ContextException(ResourceExceptMessages.ALREADY_CONNECTED, System.Net.HttpStatusCode.Unauthorized);

            var connection = new Connection()
            {
                ConnectorId = profileId,
                ConnectedId = profileConnectedId
            };

            return connection;
        }
    }
}
