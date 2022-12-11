
using TwitchLib.Client.Models;

namespace EmmaWords;

internal interface IGameUI
{
    public void HandleMessage(ChatMessage message);

    public void Render(GdiAnimation animation, Graphics gfx, Rectangle area);

    public void HandleKey(KeyEventArgs e);

    public void HandleMouse(MouseEventArgs e);
}
