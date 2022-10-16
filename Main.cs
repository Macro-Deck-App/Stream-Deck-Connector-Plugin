using SuchByte.MacroDeck.Logging;
using SuchByte.MacroDeck.Plugins;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MacroDeck.StreamDeckConnectorPlugin
{
    public class Main : MacroDeckPlugin
    {
        private Process _streamDeckConnectorService;

        public override void Enable()
        {
            AppDomain.CurrentDomain.ProcessExit += Application_ApplicationExit;

            KillIfRunning();

            Task.Run(() =>
            {
                Thread.Sleep(1000 * 7);
                MacroDeckLogger.Info(this, "Starting Stream Deck connector service");
                _streamDeckConnectorService = new Process
                {
                    StartInfo = new ProcessStartInfo(Path.Combine(SuchByte.MacroDeck.MacroDeck.PluginsDirectoryPath, "MacroDeck.StreamDeckConnectorPlugin", "Macro-Deck-Stream-Deck-Connector.exe"), 
                    $"--host 127.0.0.1:{SuchByte.MacroDeck.MacroDeck.Configuration.Host_Port}")
                    {
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                    }
                };
                _streamDeckConnectorService.OutputDataReceived += P_OutputDataReceived;
                _streamDeckConnectorService.Start();
                MacroDeckLogger.Info(this, $"Stream Deck connector service pid: {_streamDeckConnectorService.Id}");
                _streamDeckConnectorService.BeginOutputReadLine();
            });
        }

        private void KillIfRunning()
        {
            var processes = Process.GetProcessesByName("Macro-Deck-Stream-Deck-Connector");
            if (processes.Length == 0) return;
            MacroDeckLogger.Info(this, $"Stream Deck connector service is already running. Killing process...");
            foreach (var process in processes)
            {
                process.Kill();
            }
        }

        private void Application_ApplicationExit(object sender, EventArgs e)
        {
            _streamDeckConnectorService?.Kill();
        }

        private void P_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            MacroDeckLogger.Info(this, e.Data);
        }
    }
}
