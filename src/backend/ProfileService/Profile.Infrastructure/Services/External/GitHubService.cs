using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<GitHubService> _logger;

        public GitHubService(IHttpClientFactory httpClient, IConfiguration configuration, ILogger<GitHubService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
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
            var client = _httpClient.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _signKey);
            client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
            client.DefaultRequestHeaders.Add("User-Agent", "techdev");

            var repositories = await GetRepositoryUserNames(userName);

            var totalCommits = 0;
            
            foreach(var repos in repositories)
            {
                var page = 1;
                var hasPage = true;
                var perPage = 100;

                while(hasPage)
                {
                    var request = await client.GetAsync($"https://api.github.com/repos/{userName}/{repos}/commits?page={page}&per_page={perPage}");
                    var content = await request.Content.ReadAsStringAsync();

                    if (!request.IsSuccessStatusCode)
                        throw new ExternalException(content, (int)request.StatusCode);

                    var toJson = JsonDocument.Parse(content);

                    var enumarateJson = toJson.RootElement.EnumerateArray();
                    var countEnumerate = enumarateJson.Count();
                    totalCommits += countEnumerate;

                    hasPage = perPage == countEnumerate;
                    page++;

                    _logger.LogInformation($"Repository {repos} commits: {enumarateJson.Count()}");
                }
            }

            return totalCommits;
        }

        async Task<List<string>> GetRepositoryUserNames(string name)
        {
            var client = _httpClient.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _signKey);
            client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
            client.DefaultRequestHeaders.Add("User-Agent", "techdev");

            var request = await client.GetAsync($"https://api.github.com/users/{name}/repos");

            var content = await request.Content.ReadAsStringAsync();

            if(request.IsSuccessStatusCode)
            {
                var toJson = JsonDocument.Parse(content);

                var names = new List<string>();

                var list = toJson.RootElement.EnumerateArray();
                foreach(var rep in list)
                {
                    var repoName = rep.GetProperty("name").GetString();
                    names.Add(repoName!);
                }
                return names;
            }
            throw new ExternalException(content, (int)request.StatusCode);
        }
    }
}
