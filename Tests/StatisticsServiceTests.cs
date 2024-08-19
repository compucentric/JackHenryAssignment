using JackHenry2.Models;
using JackHenry2.Services;

namespace JackHenry2.Tests
{
    [TestFixture]
    public class StatisticsServiceTests
    {
        private StatisticsService _statisticsService;

        [SetUp]
        public void Setup()
        {
            _statisticsService = new StatisticsService();
        }

        [Test]
        public void UpdateStatistics_ShouldAddBatchToStack()
        {
            // Arrange
            var posts = new List<Post>
            {
                new Post { Author = "Author1", UpVotes = 100 },
                new Post { Author = "Author2", UpVotes = 150 }
            };
            var readPosts = new Dictionary<string, List<Post>>
            {
                { "subreddit1", posts }
            };
            string batchDateTimeStamp = "2024-08-17T12:00:00Z";

            // Act
            _statisticsService.UpdateStatistics(batchDateTimeStamp, readPosts);

            // Assert
            Assert.That(_statisticsService.Batches.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetLastBatch_ShouldReturnMostRecentBatch()
        {
            // Arrange
            var posts1 = new List<Post>
            {
                new Post { Author = "Author1", UpVotes = 100 },
                new Post { Author = "Author2", UpVotes = 200 }
            };
            var posts2 = new List<Post>
            {
                new Post { Author = "Author3", UpVotes = 150 },
                new Post { Author = "Author4", UpVotes = 300 }
            };

            var readPosts1 = new Dictionary<string, List<Post>> { { "subreddit1", posts1 } };
            var readPosts2 = new Dictionary<string, List<Post>> { { "subreddit2", posts2 } };

            _statisticsService.UpdateStatistics("2024-08-17 07:00:00", readPosts1);
            _statisticsService.UpdateStatistics("2024-08-17 08:05:00", readPosts2);

            // Act
            var lastBatch = _statisticsService.GetLastBatch();

            // Assert
            Assert.That(lastBatch.BatchDateTimeStamp, Is.EqualTo("2024-08-17 08:05:00"));
            Assert.That(lastBatch.Statistics.PostUpvotes, Is.EqualTo(300));
        }

        [Test]
        public void CreateBatch_ShouldCalculateStatisticsCorrectly()
        {
            // Arrange
            var posts = new List<Post>
            {
                new Post { Author = "Author1", UpVotes = 50 },
                new Post { Author = "Author1", UpVotes = 100 },
                new Post { Author = "Author2", UpVotes = 100 },
                new Post { Author = "Author3", UpVotes = 75 }
            };

            var readPosts = new Dictionary<string, List<Post>>
            {
                { "subreddit1", posts }
            };

            string batchDateTimeStamp = "2024-08-17 08:05:00";

            // Act
            var batch = StatisticsService.CreateBatch(batchDateTimeStamp, readPosts);

            // Assert
            Assert.That(batch.Statistics.PostUpvotes, Is.EqualTo(100));
            Assert.That(batch.Statistics.MostPopularPosts.Count, Is.EqualTo(2)); // Two posts with 100 upvotes
            Assert.That(batch.Statistics.AuthorPosts, Is.EqualTo(2)); // Author1 has 2 posts
            Assert.That(batch.Statistics.MostPopularAuthors.Count, Is.EqualTo(1)); // Author1 is the most popular author
        }

        [Test]
        public void UpdateStatistics_ShouldUpdateBatchWhenCalledWithBatchObject()
        {
            // Arrange
            var posts = new List<Post>
            {
                new Post { Author = "Author1", UpVotes = 50 },
                new Post { Author = "Author2", UpVotes = 150 }
            };
            var readPosts = new Dictionary<string, List<Post>>
            {
                { "subreddit1", posts }
            };

            var batch = StatisticsService.CreateBatch("2024-08-17T12:00:00Z", readPosts);

            // Act
            _statisticsService.UpdateStatistics(batch);

            // Assert
            Assert.That(_statisticsService.Batches.Count, Is.EqualTo(1));
            var lastBatch = _statisticsService.GetLastBatch();
            Assert.That(lastBatch, Is.EqualTo(batch));
        }
    }
}
