using System.Security.Cryptography;
using System.Text;

namespace MeasurementAcquisition.Domain;

/// <summary>
/// SHA-256 hex fingerprint of the raw payload bytes (UTF-8).
/// </summary>
public static class Sha256PayloadFingerprint
{
    /// <summary>
    /// Computes a lowercase hex SHA-256 of the payload.
    /// </summary>
    public static string ComputeHex(string rawPayloadJson)
    {
        ArgumentNullException.ThrowIfNull(rawPayloadJson);
        byte[] bytes = Encoding.UTF8.GetBytes(rawPayloadJson);
        byte[] hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
