using AutoMapper;
using Profile.Application.Responses;
using Profile.Domain.Aggregates;
using Profile.Domain.Entities;
using Profile.Domain.Exceptions;
using Profile.Domain.Repositories;
using Profile.Domain.Services.Security;
using Sqids;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Application.ApplicationServices
{
    public interface IConnectionService
    {
        public Task<ConnectionResponse> CreateConnection(long profileId);
        public Task<ConnectionResponse> AcceptConnection(long connectionId);
    }

    public class ConnectionService : IConnectionService
    {
        private readonly IRequestToken _requestToken;
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _uof;
        private readonly IMapper _mapper;
        private readonly Guid _userUid;

        public ConnectionService(IRequestToken requestToken, ITokenService tokenService, 
            IUnitOfWork uof, IMapper mapper)
        {
            _requestToken = requestToken;
            _tokenService = tokenService;
            _uof = uof;
            _mapper = mapper;

            _userUid = _tokenService.GetUserIdentifierByToken(_requestToken.GetToken());
        }

        public async Task<ConnectionResponse> AcceptConnection(long connectionId)
        {
            var user = await _uof.UserRepository.UserByIdentifier(_userUid);
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
            var user = await _uof.UserRepository.UserByIdentifier(_userUid);
            var profile = await _uof.ProfileRepository.ProfileByUser(user!);

            if (profile.Id == profileId) throw new ContextException(ResourceExceptMessages.CONNECT_WITH_YOURSELF, System.Net.HttpStatusCode.Unauthorized);

            var profileConnected = await _uof.GenericRepository.GetById<ProfileEntity>(profileId);

            if (profileConnected is null)
                throw new ContextException(ResourceExceptMessages.PROFILE_NOT_EXISTS, System.Net.HttpStatusCode.NotFound);

            var connectionExists = _uof.ConnectionRepository.ConnectionExists(profile.Id, profileConnected.Id);

            if (connectionExists is null)
                throw new ContextException(ResourceExceptMessages.ALREADY_CONNECTED, System.Net.HttpStatusCode.Unauthorized);

            var connection = new Connection()
            {
                ConnectorId = profile.Id,
                ConnectedId = profileConnected.Id
            };

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
    }
}
