using JackHenry2.Models;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JackHenry2.Services
{
    public class PersistenceService : IPersistenceService
    {
        private readonly IStatisticsService _statisticsService;
        private readonly IRedditClientService _redditClientService;
        private readonly Config _config;
        public PersistenceService(IOptions<Config> configOptions, IStatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
            _config = configOptions.Value;
        }
        public async Task SaveBatchAsync()
        {
            await SaveBatchAsync(_statisticsService.GetLastBatch());
        }

        public async Task SaveBatchAsync(Batch batch)
        {
            var json = JsonSerializer.Serialize(batch);
            await File.AppendAllTextAsync(_config.FilePath, json + Environment.NewLine);
        }

        public async Task LoadBatchesAsync()
        {
            if (!File.Exists(_config.FilePath))
                return;

            var json = await File.ReadAllLinesAsync(_config.FilePath);
            foreach (var jsonBatch in json)
            {
                var batch = JsonSerializer.Deserialize<Batch>(jsonBatch);
                if (batch != null)
                    _statisticsService.Batches.Push(batch);
            }
        }
    }
}
