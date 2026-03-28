using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Simulation.GatewayCli;

internal static class GatewayHttp
{
    internal static readonly JsonSerializerOptions JsonWriteOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    internal static readonly JsonSerializerOptions JsonReadOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    internal static void TraceHttpRequest(HttpClient client, HttpRequestMessage request, bool traceHttp)
    {
        if (!traceHttp) return;
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(request);
        Uri? requestUri = request.RequestUri;
        Uri resolved = requestUri is null
            ? client.BaseAddress!
            : requestUri.IsAbsoluteUri
                ? requestUri
                : new Uri(client.BaseAddress!, requestUri);
        Console.Error.WriteLine($"[simulate-gateway] {request.Method} {resolved}");
    }

    internal static void TraceHttpPostRelative(HttpClient client, string relativePath, bool traceHttp)
    {
        if (!traceHttp) return;
        ArgumentNullException.ThrowIfNull(client);
        Uri resolved = new(client.BaseAddress!, relativePath);
        Console.Error.WriteLine($"[simulate-gateway] POST {resolved} (application/json)");
    }

    internal async static Task<HttpResponseMessage> SendPostWithoutBodyAsync(
        HttpClient client,
        string relativePath,
        bool traceHttp,
        CancellationToken cancellationToken)
    {
        using var msg = new HttpRequestMessage(HttpMethod.Post, relativePath);
        TraceHttpRequest(client, msg, traceHttp);
        return await client.SendAsync(msg, cancellationToken).ConfigureAwait(false);
    }

    internal async static Task<HttpResponseMessage> GetAsync(
        HttpClient client,
        string relativePath,
        CancellationToken cancellationToken,
        bool traceHttp = false)
    {
        using var msg = new HttpRequestMessage(HttpMethod.Get, relativePath);
        TraceHttpRequest(client, msg, traceHttp);
        return await client.SendAsync(msg, cancellationToken).ConfigureAwait(false);
    }

    internal static HttpClient CreateClient(
        Uri baseUri,
        string? tenantId,
        string? bearerToken,
        string? correlationId,
        TimeSpan httpClientTimeout)
    {
        var client = new HttpClient { BaseAddress = baseUri, Timeout = httpClientTimeout };
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        if (!string.IsNullOrWhiteSpace(tenantId)) _ = client.DefaultRequestHeaders.TryAddWithoutValidation("X-Tenant-Id", tenantId.Trim());

        if (!string.IsNullOrWhiteSpace(bearerToken)) client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken.Trim());

        if (!string.IsNullOrWhiteSpace(correlationId)) _ = client.DefaultRequestHeaders.TryAddWithoutValidation("X-Correlation-Id", correlationId.Trim());

        return client;
    }

    internal async static Task<HttpResponseMessage> PostJsonAsync(
        HttpClient client,
        string relativePath,
        object body,
        CancellationToken cancellationToken,
        bool traceHttp = false)
    {
        TraceHttpPostRelative(client, relativePath, traceHttp);
        string json = JsonSerializer.Serialize(body, JsonWriteOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await client.PostAsync(relativePath, content, cancellationToken).ConfigureAwait(false);
    }

    internal async static Task WriteResultAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken,
        bool writeSuccessBodyToStdout = true)
    {
        string body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            string detail = FormatHttpErrorBody(body, maxLength: 2048);
            string reason = response.ReasonPhrase?.Trim() ?? "";
            string statusLine = string.IsNullOrEmpty(reason)
                ? $"{(int)response.StatusCode}"
                : $"{(int)response.StatusCode} {reason}";
            string msg = $"HTTP {statusLine}: {detail}";
            await Console.Error.WriteLineAsync(msg).ConfigureAwait(false);
            throw new InvalidOperationException(msg);
        }

        if (writeSuccessBodyToStdout && body.Length > 0) Console.WriteLine(body);
    }

    private static string FormatHttpErrorBody(string body, int maxLength)
    {
        if (string.IsNullOrEmpty(body)) return "(empty response body)";
        if (body.Length <= maxLength) return body;
        return string.Concat(body.AsSpan(0, maxLength), "…");
    }

    internal static Uri NormalizeGatewayBase(string raw)
    {
        string trimmed = raw.Trim();
        if (trimmed.Length == 0) throw new ArgumentException("Gateway base URL is empty.", nameof(raw));

        return new Uri(trimmed.EndsWith('/') ? trimmed : trimmed + "/", UriKind.Absolute);
    }
}
