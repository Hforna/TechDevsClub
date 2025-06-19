
using Profile.Domain.Aggregates;
using Profile.Domain.Repositories;
using Profile.Domain.Services.External;
using System.Runtime.CompilerServices;

namespace Profile.Api.BackgroundJobs
{
    public class CountGithubCommitsBackground : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private Timer _timer;
        private readonly ILogger<CountGithubCommitsBackground> _logger;

        public CountGithubCommitsBackground(IServiceProvider serviceProvider, ILogger<CountGithubCommitsBackground> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(DoWork!, null, TimeSpan.FromDays(2), Timeout.InfiniteTimeSpan);

            return Task.CompletedTask;
        }

        void DoWork(object state)
        {
            Task.Run(async () =>
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var uof = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                        var profiles = await uof.ProfileRepository.GetProfilesWithGithub();

                        if (profiles != null)
                        {
                            var githubService = scope.ServiceProvider.GetRequiredService<IGitHubService>();

                            foreach(var profile in profiles)
                            {
                                var username = profile.GithubMeta.Username;
                                var commits = await githubService.GetTotalProfileCommits(username);
                                var repositories = await githubService.GetPublicProfileRepositories(username);

                                _logger.LogInformation($"Total commits to all {repositories} user repositories: {commits}");

                                profile.GithubMeta.Commits = commits;
                                profile.GithubMeta.Repositories = repositories;
                                uof.GenericRepository.Update<ProfileEntity>(profile);
                            }
                            await uof.Commit();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing background work");
                }
                finally
                {
                    _timer?.Change(TimeSpan.FromDays(2), Timeout.InfiniteTimeSpan);
                }
            }, CancellationToken.None);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }
    }
}
