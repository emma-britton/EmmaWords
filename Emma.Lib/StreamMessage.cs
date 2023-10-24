
using TwitchLib.Client.Models;

namespace Emma.Lib;

/// <summary>
/// Represents a message passed between components, for example in response to a command in Twitch chat.
/// </summary>
public class StreamMessage
{
    /// <summary>
    /// Whether the message is sent by the stream broadcaster.
    /// </summary>
    public bool IsBroadcaster { get; }

    /// <summary>
    /// The emotes associated with the message.
    /// </summary>
    public List<Emote>? Emotes { get; }

    /// <summary>
    /// The message content.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// The username of the viewer who sent the message.
    /// </summary>
    public string Username { get; }


    /// <summary>
    /// Creates a new message.
    /// </summary>
    /// <param name="username">The username of the viewer who sent the message.</param>
    /// <param name="text">The message content.</param>
    /// <param name="emotes">The emotes associated with the message.</param>
    public StreamMessage(string username, string text, List<Emote>? emotes, bool isBroadcaster)
    {
        Username = username;
        Text = text;
        Emotes = emotes;
        IsBroadcaster = isBroadcaster;
    }
}
