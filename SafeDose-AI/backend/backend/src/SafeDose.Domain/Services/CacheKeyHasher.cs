using System.Security.Cryptography;
using System.Text;

namespace SafeDose.Domain.Services;

// Pure utility - builds a deterministic SHA-256 key for de-duplication caching.
// Same drug set + same patient = same key, regardless of selection order.
public class CacheKeyHasher
{
    public string Build(IEnumerable<int> drugIds, int? patientId)
    {
        var sorted = drugIds.Distinct().OrderBy(id => id).ToArray();
        var raw = string.Join(",", sorted) + "|p=" + (patientId?.ToString() ?? "anon");

        using var sha = SHA256.Create();
        return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(raw)))[..32];
    }
}
