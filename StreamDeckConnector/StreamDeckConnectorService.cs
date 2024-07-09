using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MacroDeck.StreamDeckConnectorPlugin.GitHub;
using SuchByte.MacroDeck.Logging;
using SuchByte.MacroDeck.Startup;
using SuchByte.MacroDeck.Utils;

namespace MacroDeck.StreamDeckConnectorPlugin.StreamDeckConnector;

public static class StreamDeckConnectorService
{
    private static string ExecutablePath => Path.Combine(ApplicationPaths.PluginsDirectoryPath,
        "MacroDeck.StreamDeckConnectorPlugin", "Macro-Deck-Stream-Deck-Connector.exe");
    
    private const string ProcessName = "Macro-Deck-Stream-Deck-Connector";
    
    private static readonly string Host =  $"--host 127.0.0.1:{SuchByte.MacroDeck.MacroDeck.Configuration.HostPort}";
    
    private static Process? _streamDeckConnector;

    public static async Task Start()
    {
        KillIfRunning();
        if (GetInstalledVersion() is null || await IsNewerVersionAvailable())
        {
            try
            {
                await DownloadExecutable();
            }
            catch (Exception ex)
            {
                MacroDeckLogger.Error(Main.Instance,
                    $"Error while downloading StreamDeckConnector executable: {ex.Message}");
            }
        }

        if (!File.Exists(ExecutablePath))
        {
            MacroDeckLogger.Error(Main.Instance, $"StreamDeckConnector executable not found at {ExecutablePath}");
            return;
        }

        try
        {
            await RunProcess();
        } catch (Exception ex)
        {
            MacroDeckLogger.Error(Main.Instance, $"Failed to start StreamDeckConnector: {ex.Message}");
        }
    }

    public static void Stop()
    {
        KillIfRunning();
    }

    private static async Task DownloadExecutable()
    {
        var latestGitHubRelease = await GitHubService.GetLatestRelease();
        var asset = GetAsset(latestGitHubRelease);
        if (asset?.DownloadUrl is null)
        {
            return;
        }

        MacroDeckLogger.Info(Main.Instance, "Downloading StreamDeckConnector executable...");
        await Task.Run(() => FileDownloader.DownloadFileAsync(asset.DownloadUrl, ExecutablePath));
        MacroDeckLogger.Info(Main.Instance, "Downloaded StreamDeckConnector executable");
    }

    private static GitHubAsset? GetAsset(GitHubRelease? release)
    {
        return release?.Assets?.FirstOrDefault(x => x.Name != null && x.Name.EndsWith(".exe"));
    }

    private static async Task<bool> IsNewerVersionAvailable()
    {
        var installedVersion = GetInstalledVersion();
        long installedSize = new FileInfo(ExecutablePath).Length;
        var latestGitHubRelease = await GitHubService.GetLatestRelease();
        if (installedVersion is null || latestGitHubRelease?.Name is null || GetAsset(latestGitHubRelease)?.Size is null)
        {
            return false;
        }

        return installedVersion != latestGitHubRelease.Name || GetAsset(latestGitHubRelease)!.Size != installedSize;
    }

    private static string? GetInstalledVersion()
    {
        if (!File.Exists(ExecutablePath))
        {
            return null;
        }
        
        var versionInfo = FileVersionInfo.GetVersionInfo(ExecutablePath);
        return versionInfo.ProductVersion;
    }
    
    private static async Task RunProcess()
    {
        // Wait a few seconds to ensure Macro Deck is fully started
        await Task.Delay(TimeSpan.FromSeconds(6));
 
        _streamDeckConnector = new Process
        {
            StartInfo = new ProcessStartInfo(ExecutablePath, Host)
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
            }
        };
        _streamDeckConnector.OutputDataReceived += OutputDataReceived;
        _streamDeckConnector.Start();
        _streamDeckConnector.BeginOutputReadLine();
    }

    private static void KillIfRunning()
    {
        var processes = Process.GetProcessesByName(ProcessName);
        if (processes.Length == 0)
        {
            return;
        }
        
        foreach (var process in processes)
        {
            process.Kill();
        }
    }
    
    private static void OutputDataReceived(object? sender, DataReceivedEventArgs e)
    {
        if (e.Data is { } dataString)
        {
            MacroDeckLogger.Info(Main.Instance, dataString);   
        }
    }
}