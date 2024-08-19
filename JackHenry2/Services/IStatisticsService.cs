using System.Collections.Concurrent;
using JackHenry2.Models;

namespace JackHenry2.Services
{
    public interface IStatisticsService
    {
        ConcurrentStack<Batch> Batches { get; }
        void UpdateStatistics(string batchDateTimeStamp, Dictionary<string, List<Post>> readPosts);
        void UpdateStatistics(Batch batch);
        Batch GetLastBatch();
    }
}
