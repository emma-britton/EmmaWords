
using TwitchLib.Client.Models;

namespace Emma.Lib;

/// <summary>
/// Represents a message passed between components, for example in response to a command in Twitch chat.
/// </summary>
/// <param name="username">The username of the viewer who sent the message.</param>
/// <param name="text">The message content, if any.</param>
/// <param name="emotes">The emotes associated with the message.</param>
/// <param name="isBroadcaster">Whether the message is sent by the stream broadcaster.</param>
/// <param name="rewardName">Name of the channel point reward that was redeemed, if any.</param>
public class StreamMessage(string username, string? text, List<Emote>? emotes, bool isBroadcaster = false,
    bool isModerator = false, bool isVIP = false, string? rewardName = null)
{
    /// <summary>
    /// Whether the message is sent by the stream broadcaster.
    /// </summary>
    public bool IsBroadcaster { get; } = isBroadcaster;

    /// <summary>
    /// Whether the message is sent by a moderator.
    /// </summary>
    public bool IsModerator { get; } = isModerator;

    /// <summary>
    /// Whether the message is sent by a VIP.
    /// </summary>
    public bool IsVIP { get; } = isVIP;

    /// <summary>
    /// The emotes associated with the message.
    /// </summary>
    public List<Emote>? Emotes { get; } = emotes;

    /// <summary>
    /// The message content, if any.
    /// </summary>
    public string? Text { get; } = text;

    /// <summary>
    /// The username of the viewer who sent the message.
    /// </summary>
    public string Username { get; } = username;

    /// <summary>
    /// Name of the channel point reward that was redeemed, if any.
    /// </summary>
    public string? RewardName { get; } = rewardName;
}
