using EmmaWords.Properties;
using TwitchLib.Api;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

namespace EmmaWords;


class TwitchBot
{
    private readonly TwitchClient Client;
    private readonly WordService WordService;
    private readonly TwitchAPI API;
    private readonly string CommandPrefix;
    private readonly string BotUsername;
    private readonly string OAuthToken;
    private readonly string Channel;


    public bool ClearChat { get; set; }


    public TwitchBot(string commandPrefix, WordService wordService, 
        string clientID, string accessToken, string botUsername, string oAuthToken, string channel)
    {
        API = new TwitchAPI();
        API.Settings.ClientId = clientID;
        API.Settings.AccessToken = accessToken;

        var clientOptions = new ClientOptions
        {
            MessagesAllowedInPeriod = 750,
            ThrottlingPeriod = TimeSpan.FromSeconds(30)
        };

        var wsClient = new WebSocketClient(clientOptions);
        Client = new TwitchClient(wsClient);

        CommandPrefix = commandPrefix;
        WordService = wordService;
        BotUsername = botUsername;
        OAuthToken = oAuthToken;
        Channel = channel;
    }


    public void Run()
    {
        var credentials = new ConnectionCredentials(BotUsername, OAuthToken);

        Client.Initialize(credentials, Channel);
        Client.OnMessageReceived += Client_OnMessageReceived;
        Client.OnConnected += Client_OnConnected;
        Client.OnChatCleared += Client_OnChatCleared;
        Client.OnLog += Client_OnLog;
        Client.Connect();
    }

    private void Client_OnLog(object? sender, OnLogArgs e)
    {
        //Console.WriteLine(e.Data);
    }

    public string GetProfilePicUrl(string name)
    {
        return API.Helix.Users.GetUsersAsync(logins: new List<string> { name }).Result.Users[0].ProfileImageUrl;
    }


    private void Client_OnChatCleared(object? sender, OnChatClearedArgs e)
    {
        ClearChat = true;
    }


    private void Client_OnConnected(object? sender, OnConnectedArgs e)
    {
        Console.WriteLine("Connected to channel: " + Channel);
        Client.SendMessage(Channel, "sorry i'm late");
    }


    private void Client_OnMessageReceived(object? sender, OnMessageReceivedArgs e)
    {
        if (e.ChatMessage.Username.Equals("emma_is_bot", StringComparison.OrdinalIgnoreCase)) return;
        WordService.CurrentUser = e.ChatMessage.Username;

        Program.ChatMessages.Enqueue(e.ChatMessage);
        string command = e.ChatMessage.Message;
        string actualCommand;

        string[] channelsWithShortPrefix = [ "gurchy", "abbyws", "cloiss" ];

        if (command.StartsWith(CommandPrefix, StringComparison.OrdinalIgnoreCase))
        {
            actualCommand = command[CommandPrefix.Length..].Trim();
        }
        else if (channelsWithShortPrefix.Contains(Channel.ToLower()) && command.StartsWith(Settings.Default.ShortCommandPrefix, StringComparison.OrdinalIgnoreCase))
        {
            actualCommand = command[Settings.Default.ShortCommandPrefix.Length..].Trim();
        }
        else
        {
            return;
        }

        string? result =  WordService.InterpretCommand(actualCommand);

        if (result != null)
        {
            Client.SendMessage(e.ChatMessage.Channel, result);
        }
    }


    public void SendMessage(string message)
    {
        Client.SendMessage(Channel, message);
    }
}