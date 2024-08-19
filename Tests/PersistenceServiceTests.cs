using JackHenry2.Models;
using JackHenry2.Services;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace JackHenry2.Tests
{
    [TestFixture]
    public class PersistenceServiceTests
    {
        private IOptions<Config> _configOptions;
        private IPersistenceService _persistenceService;
        private IStatisticsService _statisticsService;

        [SetUp]
        public void SetUp()
        {
            var config = new Config
            {
                FilePath = Path.Combine(Path.GetTempPath(), "test.txt"),
                UserAgent = "JackHenry",
                Token = "your_token_here",
                Subreddits = new List<string> { "news", "technology", "funny" }
            };
            _configOptions = Options.Create(config);
            _statisticsService = new StatisticsService();
            _persistenceService = new PersistenceService(_configOptions, _statisticsService);
        }

        private Batch CreateBatch()
        {
            var readPosts = new Dictionary<string, List<Post>>
        {
            {
                "SubredditName1",
                new List<Post>()
                {
                    new Post() { Id = "11", Author = "A11", Title = "T11", UpVotes = 11 },
                    new Post() { Id = "12", Author = "A12", Title = "T12", UpVotes = 12 },
                }
            },
            {
                "SubredditName2",
                new List<Post>()
                {
                    new Post() { Id = "21", Author = "A21", Title = "T21", UpVotes = 21 },
                    new Post() { Id = "21", Author = "A21", Title = "T22", UpVotes = 22 },
                }
           }

        };

            Batch batch = StatisticsService.CreateBatch(DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss"), readPosts);
            return batch;
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(_configOptions.Value.FilePath))
            {
                File.Delete(_configOptions.Value.FilePath);
            }
        }

        [Test]
        public async Task SaveBatchAsync_ShouldSaveBatchToFile()
        {
            // Arrange
            _statisticsService.UpdateStatistics(CreateBatch());
            Batch batch = _statisticsService.GetLastBatch();

            // Act
            await _persistenceService.SaveBatchAsync();

            // Assert
            string fileContent = await File.ReadAllTextAsync(_configOptions.Value.FilePath);
            Assert.IsFalse(string.IsNullOrEmpty(fileContent));

            var savedBatch = JsonSerializer.Deserialize<Batch>(fileContent.Trim());

            //Assert that 2 objest are the same
            Assert.That(JsonSerializer.Serialize(savedBatch), Is.EqualTo(JsonSerializer.Serialize(batch)));
        }

        [Test]
        public async Task LoadBatchesAsync_ShouldLoadBatchesFromFile()
        {
            // Arrange

            //Create Batches and persist them without updating _statisticsService.Batches
            Batch batch1 = CreateBatch();
            await _persistenceService.SaveBatchAsync(batch1);

            Batch batch2 = CreateBatch();
            await _persistenceService.SaveBatchAsync(batch2);

            Assert.IsTrue(_statisticsService.Batches.Count() == 0);

            // Act
            await _persistenceService.LoadBatchesAsync();

            // Assert
            Assert.That(_statisticsService.Batches.Count, Is.EqualTo(2));
            Assert.That(JsonSerializer.Serialize(batch1), Is.EqualTo(JsonSerializer.Serialize(_statisticsService.Batches.FirstOrDefault(x => x.BatchDateTimeStamp == batch1.BatchDateTimeStamp))));
            Assert.That(JsonSerializer.Serialize(batch2), Is.EqualTo(JsonSerializer.Serialize(_statisticsService.Batches.FirstOrDefault(x => x.BatchDateTimeStamp == batch2.BatchDateTimeStamp))));
        }
    }
}