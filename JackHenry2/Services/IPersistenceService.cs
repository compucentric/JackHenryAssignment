using JackHenry2.Models;

namespace JackHenry2.Services
{
    public interface IPersistenceService
    {
        Task SaveBatchAsync();
        Task SaveBatchAsync(Batch batch);
        Task LoadBatchesAsync();
    }
}
