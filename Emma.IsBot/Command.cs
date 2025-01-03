
using Emma.Lib;

namespace Emma.IsBot;

class Command
{
    public string Name { get; }
    public Func<string[], string?> Action { get; }
    public string Help { get; }
    public Permission Permission { get; }


    public Command(string name, Func<string[], string?> action, string help, Permission permission)
    {
        Name = name;
        Action = action;
        Help = help;
        Permission = permission;
    }


    public bool HasPermission(StreamMessage message)
    {
        return Permission == Permission.Anyone || message.Username == "gurchy" ||
               (Permission == Permission.VIP && (message.IsVIP || message.IsModerator || message.IsBroadcaster)) ||
               (Permission == Permission.Moderator && (message.IsModerator || message.IsBroadcaster)) ||
               (Permission == Permission.Broadcaster && message.IsBroadcaster);
    }
}
