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
        public GithubMetadata(string username, int repositories, int commits)
        {
            Username = username;
            Repositories = repositories;
            Commits = commits;
        }

        public string Username { get; set; }
        public int Repositories { get; set; }
        public int Commits { get; set; }
    }
}
