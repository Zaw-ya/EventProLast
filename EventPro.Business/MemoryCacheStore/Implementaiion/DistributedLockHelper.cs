using Microsoft.Extensions.Logging;
using RedLockNet.SERedis;

namespace EventPro.Business.MemoryCacheStore.Implementaiion
{
    public class DistributedLockHelper
    {
        private readonly RedLockFactory _redlockFactory;
        private readonly ILogger<DistributedLockHelper> _logger;

        public DistributedLockHelper(RedLockFactory redlockFactory, ILogger<DistributedLockHelper> logger)
        {
            _redlockFactory = redlockFactory;
            _logger = logger;
        }

        public async Task<bool> RunWithLockAsync(
            string resourceKey,
            Func<Task> action,
            TimeSpan? expiry = null,
            TimeSpan? wait = null,
            TimeSpan? retry = null)
        {
            expiry ??= TimeSpan.FromSeconds(300);
            wait ??= TimeSpan.FromSeconds(300);
            retry ??= TimeSpan.FromMilliseconds(300);

            if (_redlockFactory == null)
            {
                _logger.LogWarning("RedLockFactory is null (Redis offline). Executing {ResourceKey} without distributed lock.", resourceKey);
                await action();
                return true;
            }

            using (var redLock = await _redlockFactory.CreateLockAsync(resourceKey, expiry.Value, wait.Value, retry.Value))
            {
                if (!redLock.IsAcquired)
                {
                    _logger.LogWarning("Could not acquire lock for {ResourceKey}", resourceKey);
                    return false;
                }

                try
                {
                    await action();
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing action under lock {ResourceKey}", resourceKey);
                    throw;
                }
            }
        }
    }
}
