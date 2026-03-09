using LexManagerProMax.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace LexManagerProMax.Infrastructure.Services;

/// <summary>
/// Implementazione di IAIService che chiama le API OpenAI (GPT-4o).
/// Registrare come Scoped nel DI container.
/// </summary>
public sealed class OpenAIService : IAIService, IDisposable
{
    private readonly HttpClient _http;
    private readonly ILogger<OpenAIService> _logger;
    private readonly string _model;
    private const string ApiUrl = "https://api.openai.com/v1/chat/completions";

    public OpenAIService(
        IConfiguration config,
        ILogger<OpenAIService> logger)
    {
        _logger = logger;
        _model = config["OpenAI:Model"] ?? "gpt-4o";

        var apiKey = config["OpenAI:ApiKey"]
            ?? throw new InvalidOperationException(
                "OpenAI:ApiKey non configurata. " +
                "Aggiungere la chiave in appsettings.json o nei secrets.");

        _http = new HttpClient();
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);
        _http.Timeout = TimeSpan.FromMinutes(3);
    }

    // ─────────────────────────────────────────────────────────────
    // Streaming
    // ─────────────────────────────────────────────────────────────

    public async IAsyncEnumerable<string> StreamChatAsync(
        IEnumerable<(string Role, string Content)> messages,
        string systemPrompt,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var payload = BuildPayload(messages, systemPrompt, stream: true);
        var request = BuildRequest(payload);

        HttpResponseMessage response;
        try
        {
            response = await _http.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                ct);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore chiamata OpenAI streaming");
            throw;
        }

        await using var body = await response.Content.ReadAsStreamAsync(ct);
        using var reader = new System.IO.StreamReader(body);

        string? line;
        while ((line = await reader.ReadLineAsync(ct)) is not null
               && !ct.IsCancellationRequested)
        {
            if (!line.StartsWith("data:")) continue;

            var data = line[5..].Trim();
            if (data == "[DONE]") yield break;

            string? chunk = null;
            try
            {
                using var doc = JsonDocument.Parse(data);
                var delta = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("delta");

                if (delta.TryGetProperty("content", out var content))
                    chunk = content.GetString();
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Chunk SSE non parsabile: {Data}", data);
                continue;
            }

            if (!string.IsNullOrEmpty(chunk))
                yield return chunk;
        }
    }

    // ─────────────────────────────────────────────────────────────
    // Non-streaming
    // ─────────────────────────────────────────────────────────────

    public async Task<string> ChatAsync(
        IEnumerable<(string Role, string Content)> messages,
        string systemPrompt,
        CancellationToken ct = default)
    {
        var payload = BuildPayload(messages, systemPrompt, stream: false);
        var request = BuildRequest(payload);

        HttpResponseMessage response;
        try
        {
            response = await _http.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore chiamata OpenAI non-streaming");
            throw;
        }

        var json = await response.Content.ReadAsStringAsync(ct);

        try
        {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore parsing risposta OpenAI: {Json}", json);
            throw;
        }
    }

    // ─────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────

    private object BuildPayload(
        IEnumerable<(string Role, string Content)> messages,
        string systemPrompt,
        bool stream)
    {
        var msgs = new List<object>
        {
            new { role = "system", content = systemPrompt }
        };

        msgs.AddRange(messages.Select(m => new
        {
            role = m.Role,
            content = m.Content
        }));

        return new
        {
            model = _model,
            messages = msgs,
            stream,
            temperature = 0.3,       // bassa per risposte legali precise
            max_tokens = 2048
        };
    }

    private HttpRequestMessage BuildRequest(object payload)
    {
        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        return new HttpRequestMessage(HttpMethod.Post, ApiUrl) { Content = content };
    }

    public void Dispose() => _http.Dispose();
}