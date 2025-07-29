using Azure.Core;
using Career.Domain.Dtos;
using Career.Domain.Exceptions;
using Career.Domain.Services.Clients;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Career.Infrastructure.Services.Clients
{
    public class ProfileServiceClient : IProfileServiceClient
    {
        private readonly IHttpClientFactory _httpClient;
        private readonly ILogger<ProfileServiceClient> _logger;

        public ProfileServiceClient(IHttpClientFactory httpClient, ILogger<ProfileServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<UserInfosDto> GetUserInfos(string accessToken)
        {
            using var client = _httpClient.CreateClient("profile.api");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.GetAsync("api/users");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            try
            {
                var deserialize = JsonSerializer.Deserialize<UserInfosDto>(content);

                if (deserialize! is null)
                    throw new SerializationException(ResourceExceptMessages.INVALID_SERIALIZER_TYPE);

                return deserialize!;
            }
            catch (SerializationException ex)
            {
                _logger.LogError(ex, $"Error while trying to deserialize user infos response: {ex.Message}");

                throw new ClientException(ResourceExceptMessages.INVALID_SERIALIZER_TYPE);
            }catch(Exception ex)
            {
                _logger.LogError(ex, $"Unexpectadly error occured {ex.Message}");

                throw new ClientException(content);
            }
        }

        public async Task<UserInfosWithRolesDto> GetUserInfosById(string userId)
        {
            using var client = _httpClient.CreateClient("profile.api");

            var response = await client.GetAsync($"api/users/infos/{userId}");

            if(response.IsSuccessStatusCode == false)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    throw new DomainException(ResourceExceptMessages.USER_NOT_EXISTS);
            }

            var content = await response.Content.ReadAsStringAsync();
            try
            {
                var deserialize = JsonSerializer.Deserialize<UserInfosWithRolesDto>(content);

                if (deserialize! is null)
                    throw new SerializationException(ResourceExceptMessages.INVALID_SERIALIZER_TYPE);

                return deserialize!;
            }
            catch (SerializationException ex)
            {
                _logger.LogError(ex, $"Error while trying to deserialize user infos response: {ex.Message}");

                throw new ClientException(ResourceExceptMessages.INVALID_SERIALIZER_TYPE);
            }
        }

        public async Task<UserRolesDto> GetUserRoles(string accessToken)
        {
            using var client = _httpClient.CreateClient("profile.api");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.GetAsync("api/users/roles");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            try
            {
                var deserialize = JsonSerializer.Deserialize<UserRolesDto>(content);

                return deserialize!;
            }
            catch (SerializationException ex)
            {
                _logger.LogError(ex, $"Error while trying to deserialize user infos response: {ex.Message}");

                throw new ClientException(ResourceExceptMessages.INVALID_SERIALIZER_TYPE);
            }
        }
    }
}
