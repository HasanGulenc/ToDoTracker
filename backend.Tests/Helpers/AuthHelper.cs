using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace TaskManager.Tests.Helpers;

public record AuthResponse(string Token, string Email);

public static class AuthHelper
{
    public static async Task<string> RegisterAndGetToken(
        HttpClient client,
        string? email = null,
        string password = "Password123!")
    {
        email ??= $"user-{Guid.NewGuid()}@test.com";
        var response = await client.PostAsJsonAsync("/api/auth/register", new { email, password });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return body!.Token;
    }

    public static HttpClient WithToken(this HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
