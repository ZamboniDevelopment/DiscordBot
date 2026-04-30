using Discord;
using Discord.Webhook;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ZamboniBot;

internal class Program
{
    private static BotConfig BotConfig = null!;
    private static DiscordWebhookClient Client = null!;
    private static readonly HttpClient Http = new HttpClient();
    private static readonly Dictionary<ulong, ServerStatus> LastStatus = new();

    private static async Task Main(string[] args)
    {
        InitConfig();
        Client = new DiscordWebhookClient(BotConfig.WebhookUrl);
        _ = Task.Run(UpdateLoop);
        await Task.Delay(Timeout.Infinite);
    }

    private static async Task UpdateLoop()
    {
        while (true)
        {
            try
            {
                await UpdateDiscordMessage(8080, "nhl10", 1430591387153207388);
                await UpdateDiscordMessage(8081, "nhl11", 1440511306338668585);
                await UpdateDiscordMessage(8082, "nhl14", 1495916489721778217);
                await UpdateDiscordMessage(8083, "nhllegacy", 1495916751815180298);
                await UpdateDiscordMessage(8085, "nhl12", 1454739955619463229);
                await UpdateDiscordMessage(8086, "nhl13", 1454739965312372757);
                await UpdateDiscordMessage(8087, "nhl15", 1495916644013314089);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Loop error: " + ex.Message);
            }

            await Task.Delay(TimeSpan.FromSeconds(10));
        }
    }

    private static void InitConfig()
    {
        const string configFile = "bot-config.yml";

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .Build();

        var serializer = new SerializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .Build();

        if (!File.Exists(configFile))
        {
            BotConfig = new BotConfig();
            File.WriteAllText(configFile, serializer.Serialize(BotConfig));
            return;
        }

        BotConfig = deserializer.Deserialize<BotConfig>(File.ReadAllText(configFile));
    }

    private static async Task UpdateDiscordMessage(int statusPort, string game, ulong messageId)
    {
        var status = await ServerStatus.GetStatus(Http, statusPort, game);
        
        if (LastStatus.TryGetValue(messageId, out var prev) && prev.Equals(status))
            return;
        LastStatus[messageId] = status;

        var embed = new EmbedBuilder()
            .WithColor(Color.Green)
            .AddField("Server Version", status.ServerVersion ?? "N/A", true)
            .AddField("Online Users Count", status.OnlineUsersCount.ToString(), true)
            .AddField("Online Users Names", FormatUserList(status.OnlineUsers), true)
            .AddField("Queue Count", status.QueuedUsers.ToString(), true)
            .AddField("Active Games", status.ActiveGames.ToString(), true)
            .Build();

        await Client.ModifyMessageAsync(messageId, props =>
        {
            props.Content = "";
            props.Embeds = new[] { embed };
        });
    }
    
    // remove this?
    private static void EmptyMsg()
    {
        Client.SendMessageAsync("emptymessage");
    }

    private static string FormatUserList(string? users, int max = 40)
    {
        if (string.IsNullOrWhiteSpace(users))
            return "None";
        var split = users.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (split.Length <= max)
            return string.Join(", ", split);
        var shown = split.Take(max);
        var remaining = split.Length - max;
        return $"{string.Join(", ", shown)} and {remaining} more";
    }
}
