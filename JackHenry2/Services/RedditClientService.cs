using JackHenry2.Models;
using Microsoft.Extensions.Options;

namespace JackHenry2.Services
{
    public class RedditClientService : IRedditClientService
    {
        private readonly IRedditApiClient _redditApiClient;
        private readonly Config _config;
        private readonly IStatisticsService _statisticsService;
        private readonly IPersistenceService _persistenceService;

        public RedditClientService(IOptions<Config> configOptions, IStatisticsService statisticsService, IPersistenceService persistenceService, IRedditApiClient redditApiClient)
        {
            _config = configOptions.Value;
            _statisticsService = statisticsService;
            _persistenceService = persistenceService;
            _redditApiClient = redditApiClient;
        }

        public async Task StartMonitoringAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var tasks = new List<Task<Dictionary<string, List<Post>>>>();

                foreach (var subredditName in _config.Subreddits)
                {
                    tasks.Add(MonitorSubredditAsync(subredditName, cancellationToken));
                }

                // Wait for all subreddit monitoring tasks to complete
                var results = await Task.WhenAll(tasks);

                //then process the batch
                Dictionary<string, List<Post>> readPosts = new Dictionary<string, List<Post>>();
                foreach (var r in results)
                {
                    readPosts[r.Keys.First()] = r.Values.First();
                }

                _statisticsService.UpdateStatistics(DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss"), readPosts);
                await _persistenceService.SaveBatchAsync();

                Console.WriteLine();
                Console.WriteLine($"Just got a new batch [{_statisticsService.GetLastBatch().BatchDateTimeStamp}]");
                Console.WriteLine("");
            }        }

        private async Task<Dictionary<string, List<Post>>> MonitorSubredditAsync(string subredditName, CancellationToken cancellationToken)
        {
            var subredditPosts = new Dictionary<string, List<Post>>();
            var posts = new List<Post>();
            string after = null;

            try
            {
                do
                {
                    var readPosts = await _redditApiClient.FetchPostsAsync(subredditName, after);

                    if (readPosts != null && readPosts.Any())
                    {
                        posts.AddRange(readPosts);

                        // Update the "after" token to fetch the next batch
                        string lastBatchPostId = readPosts.LastOrDefault()?.Id;
                        after = lastBatchPostId == null ? null : "t3_" + lastBatchPostId;
                    }
                    else
                    {
                        after = null;
                    }

                } while (after != null && !cancellationToken.IsCancellationRequested);

                subredditPosts[subredditName] = posts;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"Monitoring subreddit '{subredditName}' was canceled.");
                throw;
            }
            catch (HttpRequestException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred while monitoring subreddit '{subredditName}'. Erro Msg: {ex.Message}");
                throw;
            }

            return subredditPosts;
        }

    }
}


