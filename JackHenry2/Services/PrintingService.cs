
using JackHenry2.Helpers;
using JackHenry2.Models;

namespace JackHenry2.Services
{
    public class PrintingService : IPrintingService
    {
        private IStatisticsService _statisticsService;
        private readonly IRedditApiClient _redditApiClient;

        public PrintingService(IStatisticsService statisticsService, IRedditApiClient redditApiClient)
        {
            _statisticsService = statisticsService;
            _redditApiClient = redditApiClient;
        }
        public void PrintAllBatchInfo()
        {
            var numOfBatches = _statisticsService.Batches.Count();

            if (numOfBatches == 0)
            {
                Console.WriteLine($"No Batch Information is available yet.");
                return;
            }

            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Printing Batch Information");
            Console.ForegroundColor = color;

            //var batches = _statisticsService.Batches.ToList();

            Console.WriteLine($"Number of Batches processed: {numOfBatches}");
            Console.WriteLine("");

            for (int i = numOfBatches - 1; i >= 0; i--)
            {
                Console.WriteLine($"Batch:\t{_statisticsService.Batches.ElementAt(i).BatchDateTimeStamp}\tSubredits: [{string.Join(", ", _statisticsService.Batches.ElementAt(i).SubredditStatistics.Keys)}]");
            }
        }
        public void PrintStatistics()
        {
            Batch batch = _statisticsService.GetLastBatch();
            if (batch == null)
            {
                Console.WriteLine("No statistics is available yet.");
                return;

            }
            PrintStatistics(batch);
        }
        public void PrintStatistics(string batchDateTimeStamp)
        {
            Batch batch = _statisticsService.Batches.FirstOrDefault(x => x.BatchDateTimeStamp == batchDateTimeStamp);
            if (batch == null)
            {
                Console.WriteLine($"The specified batch [{batchDateTimeStamp}] does not exist. Please make sure you enter the correct BatchId");
                return;

            }
            PrintStatistics(batch);
        }
        private void PrintStatistics(Batch batch)
        {

            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("");
            Console.WriteLine($"Printing Statistics for Batch [{batch.BatchDateTimeStamp}]");
            Console.ForegroundColor = color;

            Console.WriteLine("");
            color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("**************************************************************************************");
            Console.WriteLine($"Statistics for Batch [batchId: {batch.BatchDateTimeStamp}]");
            Console.WriteLine("**************************************************************************************");
            Console.ForegroundColor = color;
            Console.WriteLine("");

            color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Aggregated Statistics For Subreddits [{string.Join(", ", batch.SubredditStatistics.Keys)}]");
            Console.ForegroundColor = color;
            //Console.WriteLine("-------------------------------------------------------------------------------------");

            PrintStatistics(batch.Statistics);

            foreach (var subredditName in batch.SubredditStatistics.Keys)
            {
                color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Individual Statistics For Subreddit [{subredditName}]");
                Console.ForegroundColor = color;
                //Console.WriteLine("-------------------------------------------------------------------------------------");
                PrintStatistics(batch.SubredditStatistics[subredditName]);
            }
            color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("**************************************************************************************");
            Console.ForegroundColor = color;
            Console.WriteLine("");
        }
        private void PrintStatistics(Statistics statistics)
        {
            Console.WriteLine("");
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"{statistics.MostPopularPosts.Count()} most popular post(s) with {statistics.PostUpvotes} upVotes.");
            Console.ForegroundColor = color;

            foreach (var post in statistics.MostPopularPosts)
            {
                Console.WriteLine("");
                Console.WriteLine($"Id: {post.Id}");
                Console.WriteLine($"Author: {post.Author}");
                Console.WriteLine($"Title: {post.Title}");
            }

            Console.WriteLine("");
            color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"{statistics.MostPopularAuthors.Count()} most popular author(s) with {statistics.AuthorPosts} posts.");
            Console.ForegroundColor = color;
            foreach (var author in statistics.MostPopularAuthors)
            {
                Console.WriteLine("");
                Console.WriteLine($"Author: {author}");
            }
            Console.WriteLine("");
        }
        public void PrintRedditRequestLimits()
        {
            Console.WriteLine($"There are {_redditApiClient.RemainingRequests} remaining request(s); The next refresh will happen in  {(_redditApiClient.RateLimitResetTime - DateTime.UtcNow).TotalSeconds} seconds.");
            Console.WriteLine($"The delay strategy will not start until the the number of remaining requests is greater than {RedditApiClient.MAX_NUM_OF_REQUESTS_BEFORE_DELAY}");
        }
    }

}
