using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Profile.Domain.Exceptions;
using Profile.Domain.Services.External;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Profile.Infrastructure.Services.External
{
    public class GitHubService : IGitHubService
    {
        private readonly IHttpClientFactory _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _signKey;

        public GitHubService(IHttpClientFactory httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _signKey = _configuration.GetValue<string>("services:gitHub:signKey")!;
        }

        public async Task<int> GetPublicProfileRepositories(string userName)
        {
            var client = _httpClient.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _signKey);
            client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
            client.DefaultRequestHeaders.Add("User-Agent", "techdev");

            var response = await client.GetAsync($"https://api.github.com/users/{userName}");
            var content = await response.Content.ReadAsStringAsync();

            if(response.IsSuccessStatusCode)
            {
                var toJson = JsonDocument.Parse(content);

                var repositories = toJson.RootElement.GetProperty("public_repos").GetInt32();

                return repositories;
            }
            throw new ExternalServiceException(content, response.StatusCode);
        }

        public async Task<int> GetTotalProfileCommits(string userName)
        {
            throw new NotImplementedException();
        }
    }
}
