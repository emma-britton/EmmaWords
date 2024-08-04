using Emma.Lib;
using TwitchLib.Api;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using TwitchLib.PubSub;
using TwitchLib.PubSub.Events;

namespace Emma.IsBot;

public class TwitchBot
{
    public event EventHandler<StreamMessage>? Message;
    public event EventHandler<EventArgs>? ChatCleared;
    public event EventHandler<EventArgs>? Alert;

    private readonly TwitchClient Client;
    private readonly TwitchAPI API;
    private readonly CommandParser CommandParser;
    private readonly TwitchPubSub PubSub;

    private readonly string CommandPrefix;
    private readonly string BotUsername;
    private readonly string BotAccessToken;
    private readonly string ChannelAccessToken;
    private readonly string ChannelName;
    private readonly string ChannelId;


    public TwitchBot(
        CommandParser commandParser,
        string commandPrefix,
        string clientID,
        string botAccessToken,
        string botUsername,
        string channelAccessToken,
        string channelName,
        string channelId)
    {
        API = new TwitchAPI();
        API.Settings.ClientId = clientID;
        API.Settings.AccessToken = botAccessToken;

        var clientOptions = new ClientOptions
        {
            MessagesAllowedInPeriod = 750,
            ThrottlingPeriod = TimeSpan.FromSeconds(30)
        };

        var wsClient = new WebSocketClient(clientOptions);
        Client = new TwitchClient(wsClient);

        CommandParser = commandParser;
        CommandPrefix = commandPrefix;
        BotUsername = botUsername;
        BotAccessToken = botAccessToken;
        ChannelAccessToken = channelAccessToken;
        ChannelName = channelName;
        ChannelId = channelId;

        PubSub = new TwitchPubSub();
    }

    private void Pubsub_OnChannelPointsRewardRedeemed(object? sender, OnChannelPointsRewardRedeemedArgs e)
    {
        string rewardName = e.RewardRedeemed.Redemption.Reward.Title;
        string userName = e.RewardRedeemed.Redemption.User.Login;

        var message = new StreamMessage(userName, null, null, userName.Equals(ChannelName, StringComparison.OrdinalIgnoreCase),
            rewardName: rewardName);

        Message?.Invoke(this, message);

        string? result = CommandParser.InterpretReward(message);

        if (result != null)
        {
            Client.SendMessage(ChannelName, result);
        }
    }


    private void Pubsub_OnPubSubServiceConnected(object? sender, EventArgs e)
    {
        //PubSub.ListenToBitsEventsV2(ChannelId);
        //PubSub.ListenToFollows(ChannelId);
        //PubSub.ListenToRaid(ChannelId);
        //PubSub.ListenToSubscriptions(ChannelId);
        PubSub.ListenToChannelPoints(ChannelId);
        PubSub.SendTopics(ChannelAccessToken);
    }


    public void Run()
    {
        var credentials = new ConnectionCredentials(BotUsername, BotAccessToken);

        Client.Initialize(credentials, ChannelName);

        Client.OnLog += Client_OnLog;
        Client.OnMessageReceived += Client_OnMessageReceived;
        Client.OnConnected += Client_OnConnected;
        Client.OnChatCleared += Client_OnChatCleared;
        Client.OnNewSubscriber += Client_OnNewSubscriber;
        Client.OnCommunitySubscription += Client_OnCommunitySubscription;
        Client.OnReSubscriber += Client_OnReSubscriber;
        Client.OnContinuedGiftedSubscription += Client_OnContinuedGiftedSubscription;
        Client.OnGiftedSubscription += Client_OnGiftedSubscription;
        Client.OnRaidNotification += Client_OnRaidNotification;
        Client.OnPrimePaidSubscriber += Client_OnPrimePaidSubscriber;

        Client.Connect();

        if (PubSub != null)
        {
            PubSub.OnPubSubServiceError += (s, e) =>
            {
                Console.WriteLine(e.Exception.Message);
            };

            PubSub.OnLog += PubSub_OnLog;
            PubSub.OnPubSubServiceConnected += Pubsub_OnPubSubServiceConnected;
            PubSub.OnChannelPointsRewardRedeemed += Pubsub_OnChannelPointsRewardRedeemed;
            PubSub.OnBitsReceivedV2 += PubSub_OnBitsReceivedV2;
            PubSub.OnChannelSubscription += PubSub_OnChannelSubscription;
            PubSub.OnFollow += PubSub_OnFollow;

            PubSub.Connect();
        }
    }


    private void PubSub_OnLog(object? sender, TwitchLib.PubSub.Events.OnLogArgs e)
    {
        //Console.WriteLine("PubSub: " + e.Data);
    }


    private void Client_OnLog(object? sender, TwitchLib.Client.Events.OnLogArgs e)
    {
        //Console.WriteLine("Client: " + e.Data);
    }


    private void PubSub_OnFollow(object? sender, OnFollowArgs e)
    {
        Alert?.Invoke(sender, e);
    }


    private void PubSub_OnChannelSubscription(object? sender, OnChannelSubscriptionArgs e)
    {
    }

    private void PubSub_OnBitsReceivedV2(object? sender, OnBitsReceivedV2Args e)
    {
    }

    private void Client_OnPrimePaidSubscriber(object? sender, OnPrimePaidSubscriberArgs e)
    {
    }

    private void Client_OnRaidNotification(object? sender, OnRaidNotificationArgs e)
    {
    }

    private void Client_OnGiftedSubscription(object? sender, OnGiftedSubscriptionArgs e)
    {
    }

    private void Client_OnContinuedGiftedSubscription(object? sender, OnContinuedGiftedSubscriptionArgs e)
    {
    }

    private void Client_OnReSubscriber(object? sender, OnReSubscriberArgs e)
    {
    }

    private void Client_OnCommunitySubscription(object? sender, OnCommunitySubscriptionArgs e)
    {
    }

    private void Client_OnNewSubscriber(object? sender, OnNewSubscriberArgs e)
    {
    }

    public string GetProfilePicUrl(string name)
    {
        return API.Helix.Users.GetUsersAsync(logins: new List<string> { name }).Result.Users[0].ProfileImageUrl;
    }


    private void Client_OnChatCleared(object? sender, OnChatClearedArgs e)
    {
        ChatCleared?.Invoke(this, EventArgs.Empty);
    }


    private void Client_OnConnected(object? sender, OnConnectedArgs e)
    {
        Console.WriteLine("Connected to channel: " + ChannelName);

        if (!string.IsNullOrWhiteSpace(Program.Config["JoinMessage"]))
        {
            Client.SendMessage(ChannelName, Program.Config["JoinMessage"]);
        }
    }


    private void Client_OnMessageReceived(object? sender, OnMessageReceivedArgs e)
    {
        if (e.ChatMessage.Username.Equals(Program.Config["TwitchUsername"], StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var message = new StreamMessage(e.ChatMessage.Username, e.ChatMessage.Message, e.ChatMessage.EmoteSet.Emotes,
            e.ChatMessage.IsBroadcaster, e.ChatMessage.IsModerator, e.ChatMessage.IsVip);

        Message?.Invoke(this, message);
        string command = e.ChatMessage.Message;

        if (string.IsNullOrWhiteSpace(CommandPrefix) || command.StartsWith(CommandPrefix, StringComparison.OrdinalIgnoreCase))
        {
            command = command[CommandPrefix.Length..].Trim();

            if (command.StartsWith("emma ", StringComparison.OrdinalIgnoreCase))
            {
                command = command[5..].Trim();
            }

            string? result = CommandParser.InterpretCommand(message, command);

            if (result != null)
            {
                Client.SendMessage(e.ChatMessage.Channel, result);
                Thread.Sleep(1000);
            }
        }
    }


    public void SendMessage(string message)
    {
        Client.SendMessage(ChannelName, message);
        Thread.Sleep(1000);
    }
}