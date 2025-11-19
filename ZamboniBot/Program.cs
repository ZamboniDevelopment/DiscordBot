using Discord;
using Discord.Webhook;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ZamboniBot;

internal class Program
{
    private static BotConfig BotConfig;

    private static DiscordWebhookClient Client;

    private static async Task Main(string[] args)
    {
        InitConfig();
        Client = new DiscordWebhookClient(BotConfig.WebhookUrl);
        await using var timer = new Timer(_ =>
        {
            var a = UpdateDiscordMessage(8080, 1430591387153207388);
            var b = UpdateDiscordMessage(8081, 1440511306338668585);
        }, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));

        await Task.Delay(Timeout.Infinite);
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
            var yaml = serializer.Serialize(BotConfig);
            File.WriteAllText(configFile, yaml);
            return;
        }

        var yamlText = File.ReadAllText(configFile);
        BotConfig = deserializer.Deserialize<BotConfig>(yamlText);
    }

    private static async Task UpdateDiscordMessage(int statusPort, ulong messageId)
    {
        var serverStatus = new ServerStatus();
        await serverStatus.GetStatus(statusPort);
        var embedBuilder = new EmbedBuilder()
            .WithColor(Color.Green)
            .WithFields(new List<EmbedFieldBuilder>
            {
                new() { Name = "Server Version", Value = string.IsNullOrWhiteSpace(serverStatus.ServerVersion) ? "N/A" : serverStatus.ServerVersion, IsInline = true },
                new() { Name = "Online Users Count", Value = serverStatus.OnlineUsersCount.ToString(), IsInline = true },
                new() { Name = "Online Users Names", Value = string.IsNullOrWhiteSpace(serverStatus.OnlineUsers) ? "None" : serverStatus.OnlineUsers, IsInline = true },
                new() { Name = "Queue Count", Value = serverStatus.QueuedUsers.ToString(), IsInline = true },
                new() { Name = "Active Games", Value = serverStatus.ActiveGames.ToString(), IsInline = true }
            });

        var embed = embedBuilder.Build();

        await Client.ModifyMessageAsync(messageId, props =>
        {
            props.Content = "";
            props.Embeds = new List<Embed> { embed };
        });
    }
}