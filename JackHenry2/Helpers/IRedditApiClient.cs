using JackHenry2.Models;

namespace JackHenry2
{
    public interface IRedditApiClient
    {
        Task<List<Post>> FetchPostsAsync(string subredditName, string after);
        public int RemainingRequests { get; } 
        public DateTime RateLimitResetTime { get;  }

    }
}
