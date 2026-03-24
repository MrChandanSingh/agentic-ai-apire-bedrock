namespace AspireApp.BedRock.SonetOps.ApiService.Services
{
    public interface IDistributedLock
    {
        Task<ILockResult> AcquireLockAsync(string key, TimeSpan duration);
    }

    public interface ILockResult : IDisposable
    {
        bool IsAcquired { get; }
    }
}