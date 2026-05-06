using FreeRedis;

namespace BuildingBlocks.DistributedLocking.Redis.Internal;

internal sealed class RedisDistributedLockStore : IRedisDistributedLockStore
{
    private const string ReleaseScript = """
        if redis.call('get', KEYS[1]) == ARGV[1] then
            return redis.call('del', KEYS[1])
        end
        return 0
        """;

    private const string RenewScript = """
        if redis.call('get', KEYS[1]) == ARGV[1] then
            return redis.call('pexpire', KEYS[1], ARGV[2])
        end
        return 0
        """;

    private readonly RedisClient _redisClient;

    public RedisDistributedLockStore(RedisClient redisClient)
    {
        _redisClient = redisClient;
    }

    public Task<bool> TryAcquireAsync(
        string resource,
        string ownerToken,
        TimeSpan leaseTtl,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return _redisClient.SetNxAsync(resource, ownerToken, leaseTtl);
    }

    public async Task<bool> ReleaseAsync(
        string resource,
        string ownerToken,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = await _redisClient.EvalAsync(
            ReleaseScript,
            [resource],
            [ownerToken]);
        return ConvertEvalResultToBoolean(result);
    }

    public async Task<bool> RenewAsync(
        string resource,
        string ownerToken,
        TimeSpan leaseTtl,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = await _redisClient.EvalAsync(
            RenewScript,
            [resource],
            [ownerToken, (long)Math.Ceiling(leaseTtl.TotalMilliseconds)]);
        return ConvertEvalResultToBoolean(result);
    }

    private static bool ConvertEvalResultToBoolean(object? result)
    {
        return result switch
        {
            null => false,
            bool booleanResult => booleanResult,
            byte byteResult => byteResult != 0,
            short shortResult => shortResult != 0,
            int intResult => intResult != 0,
            long longResult => longResult != 0,
            string stringResult when long.TryParse(stringResult, out var parsedLong) => parsedLong != 0,
            string stringResult when bool.TryParse(stringResult, out var parsedBoolean) => parsedBoolean,
            _ => false
        };
    }
}

