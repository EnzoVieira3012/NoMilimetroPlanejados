using Microsoft.Extensions.Caching.Memory;

namespace Backend.Application.Services;

public class LoginAttemptService
{
    private readonly IMemoryCache _cache;
    private const int MaxAttempts = 5;
    private const int BlockDurationMinutes = 15;

    public LoginAttemptService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public bool IsBlocked(string ipAddress)
    {
        return _cache.TryGetValue(GetBlockedKey(ipAddress), out _);
    }

    public void RegisterFailedAttempt(string ipAddress)
    {
        var key = GetAttemptKey(ipAddress);

        if (_cache.TryGetValue<int>(key, out var attempts))
        {
            attempts++;

            if (attempts >= MaxAttempts)
            {
                BlockIp(ipAddress);
                _cache.Remove(key);
            }
            else
            {
                _cache.Set(key, attempts, TimeSpan.FromMinutes(BlockDurationMinutes));
            }
        }
        else
        {
            _cache.Set(key, 1, TimeSpan.FromMinutes(BlockDurationMinutes));
        }
    }

    private void BlockIp(string ipAddress)
    {
        var key = GetBlockedKey(ipAddress);
        _cache.Set(key, true, TimeSpan.FromMinutes(BlockDurationMinutes));
    }

    private static string GetAttemptKey(string ipAddress)
    {
        return $"{ipAddress}:attempts";
    }

    private static string GetBlockedKey(string ipAddress)
    {
        return $"{ipAddress}:blocked";
    }
}