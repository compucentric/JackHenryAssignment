using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using JackHenry2.Models;
using JackHenry2.Services;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace JackHenry2.Helpers
{
    public class RedditApiClient : IRedditApiClient
    {
        private readonly HttpClient _httpClient;
        private const string RedditApiBaseUrl = "https://oauth.reddit.com/r/";
        private Config _config;

            // Rate limit information
        private int _remainingRequests;
        private DateTime _rateLimitResetTime;

        public static int MAX_NUM_OF_REQUESTS_BEFORE_DELAY = 50;

        public int RemainingRequests { get { return _remainingRequests; } }
        public DateTime RateLimitResetTime { get { return _rateLimitResetTime; } }

        private readonly SemaphoreSlim _rateLimitSemaphore = new SemaphoreSlim(1, 1);

        public RedditApiClient(IOptions<Config> configOptions)
        {
            _config = configOptions.Value;

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(_config.UserAgent);
            _httpClient.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(_config.Token);

            _remainingRequests = 1; // Assume at least 1 request is allowed initially
            _rateLimitResetTime = DateTime.UtcNow;
        }
        public async Task<List<Post>> FetchPostsAsync(string subreddit, string after = null)
        {
            var url = $"{RedditApiBaseUrl}{subreddit}/new.json?limit=100";
            if (!string.IsNullOrEmpty(after))
            {
                url += $"&after={after}";
            }

            try
            {
               await _rateLimitSemaphore.WaitAsync();

                try
                {
                    await EnforceRateLimitAsync();

                    var response = await _httpClient.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    UpdateRateLimitInfo(response.Headers);

                    var content = await response.Content.ReadAsStringAsync();
                    var json = JObject.Parse(content);
                    var posts = json["data"]["children"]
                        .Select(child => new Post
                        {
                            Id = child["data"]["id"].ToString(),
                            Title = child["data"]["title"].ToString(),
                            Author = child["data"]["author"].ToString(),
                            UpVotes = (int)child["data"]["ups"]
                        })
                        .ToList();

                    return posts;
                }
                finally
                {
                    _rateLimitSemaphore.Release();
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"An error occurred while fetching posts from Reddit [{subreddit}]: {ex.Message}");
                throw;
            }
        }

        private async Task EnforceRateLimitAsync()
        {
            var now = DateTime.UtcNow;

            if (now >= _rateLimitResetTime)
            {
                // Reset the rate limit if the reset time has passed
                _remainingRequests = 1;
            }

            if (_remainingRequests <= 0)
            {
                var waitTime = _rateLimitResetTime - now;
                Console.WriteLine($"Rate limit exceeded. Waiting for {waitTime.TotalSeconds} seconds.");
                await Task.Delay(waitTime);
            }
            else if (_remainingRequests > MAX_NUM_OF_REQUESTS_BEFORE_DELAY)
            {
                //Console.WriteLine($"No delay since there are still {_remainingRequests} Remaining requests available");
                return;
            }
            else
            {
                // Implement a delay if necessary to avoid hitting the rate limit
                var delay = (_rateLimitResetTime - now).TotalSeconds / (_remainingRequests + 1);
                if (delay > 0)
                {
                    Console.WriteLine($"Delaying next request by {delay} seconds to avoid rate limit.");
                    await Task.Delay(TimeSpan.FromSeconds(delay));
                }
            }
        }

        private void UpdateRateLimitInfo(HttpResponseHeaders headers)
        {
            _remainingRequests = headers.Contains("x-ratelimit-remaining") ?
                (int)decimal.Parse(headers.GetValues("x-ratelimit-remaining").First()) : 1;

            var rateLimitResetSeconds = headers.Contains("x-ratelimit-reset") ?
                int.Parse(headers.GetValues("x-ratelimit-reset").First()) : 60;

            _rateLimitResetTime = DateTime.UtcNow.AddSeconds(rateLimitResetSeconds);
        }
    }

}
