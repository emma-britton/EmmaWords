using Emma.Lib;
using TwitchLib.Api;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

namespace Emma.IsBot;

public class TwitchBot
{
    public event EventHandler<StreamMessage>? Message;
    public event EventHandler<EventArgs>? ChatCleared;

    private readonly TwitchClient Client;
    private readonly TwitchAPI API;
    private readonly CommandParser CommandParser;

    private readonly string CommandPrefix;
    private readonly string BotUsername;
    private readonly string OAuthToken;
    private readonly string Channel;


    public TwitchBot(
        CommandParser commandParser,
        string commandPrefix,
        string clientID, 
        string accessToken, 
        string botUsername, 
        string oAuthToken, 
        string channel)
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

        CommandParser = commandParser;
        CommandPrefix = commandPrefix;
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
        ChatCleared?.Invoke(this, EventArgs.Empty);
    }


    private void Client_OnConnected(object? sender, OnConnectedArgs e)
    {
        Console.WriteLine("Connected to channel: " + Channel);

        if (!string.IsNullOrWhiteSpace(Program.Config["JoinMessage"]))
        {
            Client.SendMessage(Channel, Program.Config["JoinMessage"]);
        }
    }


    private void Client_OnMessageReceived(object? sender, OnMessageReceivedArgs e)
    {
        if (e.ChatMessage.Username.Equals(Program.Config["TwitchUsername"], StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var message = new StreamMessage(e.ChatMessage.Username, e.ChatMessage.Message, e.ChatMessage.EmoteSet.Emotes, e.ChatMessage.IsBroadcaster);

        Message?.Invoke(this, message);
        string command = message.Text;

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
            }
        }
    }


    public void SendMessage(string message)
    {
        Client.SendMessage(Channel, message);
    }
}