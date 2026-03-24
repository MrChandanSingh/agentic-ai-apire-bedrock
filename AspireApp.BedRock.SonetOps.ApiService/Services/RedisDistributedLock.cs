using StackExchange.Redis;
using Microsoft.Extensions.Logging;

namespace AspireApp.BedRock.SonetOps.ApiService.Services
{
    public class RedisDistributedLock : IDistributedLock
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<RedisDistributedLock> _logger;

        public RedisDistributedLock(IConnectionMultiplexer redis, ILogger<RedisDistributedLock> logger)
        {
            _redis = redis;
            _logger = logger;
        }

        public async Task<ILockResult> AcquireLockAsync(string key, TimeSpan duration)
        {
            var db = _redis.GetDatabase();
            var value = Guid.NewGuid().ToString();
            var acquired = await db.StringSetAsync(key, value, duration, When.NotExists);

            return new RedisLockResult(acquired, db, key, value, _logger);
        }

        private class RedisLockResult : ILockResult
        {
            private readonly IDatabase _db;
            private readonly string _key;
            private readonly string _value;
            private readonly ILogger _logger;
            private bool _disposed;

            public RedisLockResult(bool isAcquired, IDatabase db, string key, string value, ILogger logger)
            {
                IsAcquired = isAcquired;
                _db = db;
                _key = key;
                _value = value;
                _logger = logger;
            }

            public bool IsAcquired { get; }

            public void Dispose()
            {
                if (_disposed) return;

                if (IsAcquired)
                {
                    try
                    {
                        var script = @"
                            if redis.call('get', KEYS[1]) == ARGV[1] then
                                return redis.call('del', KEYS[1])
                            else
                                return 0
                            end";

                        _db.ScriptEvaluate(script, new RedisKey[] { _key }, new RedisValue[] { _value });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error releasing Redis lock for key {Key}", _key);
                    }
                }

                _disposed = true;
            }
        }
    }
}