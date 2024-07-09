using SuchByte.MacroDeck.Plugins;
using System;
using System.Threading.Tasks;
using MacroDeck.StreamDeckConnectorPlugin.StreamDeckConnector;

namespace MacroDeck.StreamDeckConnectorPlugin;

public class Main : MacroDeckPlugin
{
    public static Main Instance = null!;

    public Main()
    {
        Instance = this;
    }

    public override void Enable()
    {
        AppDomain.CurrentDomain.ProcessExit += Application_ApplicationExit;

        Task.Run(async () =>
        {
            await StreamDeckConnectorService.Start();
        });
    }

    private void Application_ApplicationExit(object? sender, EventArgs e)
    {
        StreamDeckConnectorService.Stop();
    }
}