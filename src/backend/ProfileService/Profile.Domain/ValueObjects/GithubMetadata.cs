using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Domain.ValueObjects
{
    [Owned]
    public sealed record GithubMetadata
    {
        public GithubMetadata(string username, string githubProfile, int repositories, int commits)
        {
            Username = username;
            GithubProfile = githubProfile;
            Repositories = repositories;
            Commits = commits;
        }

        public string Username { get; set; }
        public string GithubProfile { get; set; }
        public int Repositories { get; set; }
        public int Commits { get; set; }
    }
}
