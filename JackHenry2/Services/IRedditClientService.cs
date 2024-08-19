namespace JackHenry2.Services
{
    public interface IRedditClientService
    {
        Task StartMonitoringAsync(CancellationToken cancellationToken);
    }
}
