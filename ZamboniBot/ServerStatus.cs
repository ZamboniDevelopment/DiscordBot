using System.Text.Json;
using System.Text.Json.Serialization;

namespace ZamboniBot;

public class ServerStatus
{
    [JsonPropertyName("serverVersion")] public string? ServerVersion { get; set; }
    [JsonPropertyName("onlineUsersCount")] public int OnlineUsersCount { get; set; }
    [JsonPropertyName("onlineUsers")] public string? OnlineUsers { get; set; }
    [JsonPropertyName("queuedUsers")] public int QueuedUsers { get; set; }
    [JsonPropertyName("activeGames")] public int ActiveGames { get; set; }

    public static async Task<ServerStatus> GetStatus(HttpClient client, int port, string game)
    {
        try
        {
            var url = $"http://127.0.0.1:{port}/{game}/status";
            var json = await client.GetStringAsync(url);

            return JsonSerializer.Deserialize<ServerStatus>(json)
                   ?? new ServerStatus();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching status: {ex.Message}");
            return new ServerStatus();
        }
    }
}