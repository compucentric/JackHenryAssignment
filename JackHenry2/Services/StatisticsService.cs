using System.Collections.Concurrent;
using JackHenry2.Models;

namespace JackHenry2.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly ConcurrentStack<Batch> _batches;
        public StatisticsService()
        {
            _batches = new ConcurrentStack<Batch>();
        }

        public ConcurrentStack<Batch> Batches
        {
            get { return _batches; }
        }

        public void UpdateStatistics(string batchDateTimeStamp, Dictionary<string, List<Post>> readPosts)
        {
            UpdateStatistics(CreateBatch(batchDateTimeStamp, readPosts));
        }
        public void UpdateStatistics(Batch batch)
        {
            _batches.Push(batch);
        }
        public static Batch CreateBatch(string batchDateTimeStamp, Dictionary<string, List<Post>> readPosts)
        {
            List<Post> batchPosts = new List<Post>();
            Batch batch = new Batch(batchDateTimeStamp);

            foreach (var subredditName in readPosts.Keys)
            {
                batchPosts.AddRange(readPosts[subredditName]);
                var statistics = new Statistics();

                //Posts and Upvotes per Subreddit
                statistics.PostUpvotes = readPosts[subredditName].Max(p => p.UpVotes);
                statistics.MostPopularPosts.AddRange(readPosts[subredditName].Where(p => p.UpVotes == statistics.PostUpvotes).ToList());

                //Posts and Upvotes for entire Batch
                if (batch.Statistics.PostUpvotes < statistics.PostUpvotes)
                {
                    batch.Statistics.PostUpvotes = statistics.PostUpvotes;
                    batch.Statistics.MostPopularPosts = statistics.MostPopularPosts;
                }
                else if (batch.Statistics.PostUpvotes == statistics.PostUpvotes)
                    batch.Statistics.MostPopularPosts.AddRange(statistics.MostPopularPosts);

                //Authore and AuthorPosts per Subreddit
                var authorPostCounts = readPosts[subredditName].GroupBy(p => p.Author).Select(g => new { Author = g.Key, Count = g.Count() }).ToList();
                statistics.AuthorPosts = authorPostCounts.Max(x => x.Count);
                statistics.MostPopularAuthors.AddRange(authorPostCounts.Where(p => p.Count == statistics.AuthorPosts).Select(a => a.Author).ToList());
                batch.SubredditStatistics[subredditName] = statistics;
            }
            //Authore and AuthorPosts for entire Batch
            var authorPostCounts2 = batchPosts.GroupBy(p => p.Author).Select(g => new { Author = g.Key, Count = g.Count() }).ToList();
            batch.Statistics.AuthorPosts = authorPostCounts2.Max(x => x.Count);
            batch.Statistics.MostPopularAuthors.AddRange(authorPostCounts2.Where(p => p.Count == batch.Statistics.AuthorPosts).Select(a => a.Author).ToList());

            return batch;
        }

        public Batch GetLastBatch()
        {
            Batch batch = null;
            _batches.TryPeek(out batch);
            return batch;
        }
    }
}
