using System.Text.Json;
using System.Text.Json.Serialization;

namespace ZamboniBot;

public class ServerStatus
{
    [JsonPropertyName("serverVersion")] public string ServerVersion { get; set; }

    [JsonPropertyName("onlineUsersCount")] public int OnlineUsersCount { get; set; }

    [JsonPropertyName("onlineUsers")] public string OnlineUsers { get; set; }

    [JsonPropertyName("queuedUsers")] public int QueuedUsers { get; set; }

    [JsonPropertyName("activeGames")] public int ActiveGames { get; set; }

    public async Task GetStatus(int port, string game)
    {
        using var client = new HttpClient();
        try
        {
            var url = "http://127.0.0.1:" + port + "/" + game + "/status";
            var json = await client.GetStringAsync(url);

            var status = JsonSerializer.Deserialize<ServerStatus>(json);

            if (status != null)
            {
                ServerVersion = status.ServerVersion;
                OnlineUsersCount = status.OnlineUsersCount;
                OnlineUsers = status.OnlineUsers;
                QueuedUsers = status.QueuedUsers;
                ActiveGames = status.ActiveGames;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}