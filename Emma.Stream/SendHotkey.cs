using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Emma.Stream;

internal static class SendHotkey
{
    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(nint hWnd);

    public static void Send(string key)
    {
        var process = Process.GetProcessesByName("obs64").FirstOrDefault();

        if (process != null)
        {
            var windowptr = process.MainWindowHandle;

            if (windowptr != nint.Zero)
            {
                SetForegroundWindow(windowptr);
                SendKeys.SendWait(key);
                SendKeys.Flush();
            }
        }
    }
}
