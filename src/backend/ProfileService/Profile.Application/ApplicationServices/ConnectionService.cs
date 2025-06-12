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
using System.Text;
using System.Threading.Tasks;

namespace Profile.Application.ApplicationServices
{
    public interface IConnectionService
    {
        public Task<ConnectionResponse> CreateConnection(long profileId);
    }

    public class ConnectionService : IConnectionService
    {
        private readonly IRequestToken _requestToken;
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _uof;
        private readonly SqidsEncoder<long> _sqids;
        private readonly IMapper _mapper;

        public ConnectionService(IRequestToken requestToken, ITokenService tokenService, 
            IUnitOfWork uof, SqidsEncoder<long> sqids, IMapper mapper)
        {
            _requestToken = requestToken;
            _tokenService = tokenService;
            _uof = uof;
            _sqids = sqids;
            _mapper = mapper;
        }

        public async Task<ConnectionResponse> CreateConnection(long profileId)
        {
            var uid = _tokenService.GetUserIdentifierByToken(_requestToken.GetToken());
            var user = await _uof.UserRepository.UserByIdentifier(uid);
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
