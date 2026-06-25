using System.Collections.Concurrent;

namespace SafeDose.Infrastructure.ExternalServices;

// Simple in-memory cache for recent Paymob webhook bodies to aid debugging in dev.
// Not intended for production persistence.
public static class PaymobWebhookCache
{
    private static readonly ConcurrentDictionary<string, string> _cache = new();

    public static void Store(string orderId, string body)
    {
        if (string.IsNullOrWhiteSpace(orderId)) return;
        _cache[orderId] = body ?? string.Empty;
    }

    public static bool TryGet(string orderId, out string body) => _cache.TryGetValue(orderId ?? string.Empty, out body);
}
