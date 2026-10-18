using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text.Json;
using Fidus.Enums;
using Fidus.Models;
using Fidus.Utils;

namespace Fidus.Manager
{
    public sealed class UpdateManager(
        ConsoleHelper consoleHelper,
        HttpClient httpClient,
        IUpdateInstallerLauncher? installerLauncher = null)
    {
        public static readonly Uri ReleasesUri = new("https://api.github.com/repos/mohdje/Fidus-Terminal-Agent/releases");

        private static readonly Uri WindowsInstallerUri = new("https://github.com/mohdje/Fidus-Terminal-Agent/releases/latest/download/fidus_cli_windows_install.ps1");
        private static readonly Uri LinuxInstallerUri = new("https://github.com/mohdje/Fidus-Terminal-Agent/releases/latest/download/fidus_cli_linux_install.sh");
        private static readonly Uri MacOSInstallerUri = new("https://github.com/mohdje/Fidus-Terminal-Agent/releases/latest/download/fidus_cli_macos_install.sh");

        private readonly HttpClient httpClient = httpClient;
        private readonly IUpdateInstallerLauncher installerLauncher = installerLauncher ?? new UpdateInstallerLauncher();
        private readonly ConsoleHelper consoleHelper = consoleHelper;

        public UpdateManager(ConsoleHelper consoleHelper)
            : this(consoleHelper, CreateHttpClient(), null)
        {
        }

        public async Task<bool> CheckForUpdateAsync(CancellationToken cancellationToken = default)
        {
            var result = await CheckForUpdateResultAsync(cancellationToken).ConfigureAwait(false);
            return result.IsUpdateAvailable;
        }

        public async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            InstallerPlatform platform;
            Uri installerUri;
            try
            {
                (platform, installerUri) = GetInstaller();
            }
            catch (PlatformNotSupportedException ex)
            {
                consoleHelper.RenderError(ex.Message);
                return 1;
            }

            var checkTaskId = await consoleHelper.RenderTaskProcessingAsync("Check for update");

            var checkResult = await CheckForUpdateResultAsync(cancellationToken).ConfigureAwait(false);

            await consoleHelper.StopRenderTaskProcessingAsync(checkTaskId);

            if (checkResult.Error is not null)
            {
                consoleHelper.RenderError($"Could not check for updates: {checkResult.Error.Message}");
                return 1;
            }

            if (!checkResult.IsUpdateAvailable)
            {
                Console.WriteLine("Fidus CLI is up to date.");
                return 0;
            }

            var temporaryDirectory = Path.Combine(Path.GetTempPath(), $"fidus-update-{Guid.NewGuid():N}");
            var scriptPath = Path.Combine(temporaryDirectory, platform == InstallerPlatform.Windows ? "installer.ps1" : "installer.sh");

            try
            {
                Directory.CreateDirectory(temporaryDirectory);

                var downloadUpdateId = await consoleHelper.RenderTaskProcessingAsync("Download update");

                using var response = await httpClient.GetAsync(installerUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

                await consoleHelper.StopRenderTaskProcessingAsync(downloadUpdateId);

                response.EnsureSuccessStatusCode();

                await using (var file = File.Create(scriptPath))
                    await response.Content.CopyToAsync(file, cancellationToken).ConfigureAwait(false);

                await consoleHelper.StopRenderTaskProcessingAsync(checkTaskId);

                installerLauncher.Start(platform, scriptPath, temporaryDirectory);
                Console.WriteLine("Starting the Fidus CLI installer. The installer will continue after this command exits.");
                return 0;
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or IOException or UnauthorizedAccessException or InvalidOperationException or Win32Exception)
            {
                consoleHelper.RenderError($"Could not download or start the Fidus installer: {ex.Message}");
                try
                {
                    if (Directory.Exists(temporaryDirectory))
                        Directory.Delete(temporaryDirectory, recursive: true);
                }
                catch (IOException cleanupException)
                {
                    consoleHelper.RenderError($"Could not clean up the temporary installer files: {cleanupException.Message}");
                }
                catch (UnauthorizedAccessException cleanupException)
                {
                    consoleHelper.RenderError($"Could not clean up the temporary installer files: {cleanupException.Message}");
                }

                return 1;
            }
        }

        private (InstallerPlatform Platform, Uri InstallerUri) GetInstaller()
        {
            if (OperatingSystem.IsWindows())
                return (InstallerPlatform.Windows, GetInstallerUri(InstallerPlatform.Windows));
            if (OperatingSystem.IsLinux())
                return (InstallerPlatform.Linux, GetInstallerUri(InstallerPlatform.Linux));
            if (OperatingSystem.IsMacOS())
                return (InstallerPlatform.MacOS, GetInstallerUri(InstallerPlatform.MacOS));

            throw new PlatformNotSupportedException($"Fidus updates are not supported on {RuntimeInformation.OSDescription}.");
        }

        private Uri GetInstallerUri(InstallerPlatform platform) =>
            platform switch
            {
                InstallerPlatform.Windows => WindowsInstallerUri,
                InstallerPlatform.Linux => LinuxInstallerUri,
                InstallerPlatform.MacOS => MacOSInstallerUri,
                _ => throw new PlatformNotSupportedException($"Installer platform '{platform}' is not supported.")
            };

        private async Task<UpdateCheckResult> CheckForUpdateResultAsync(CancellationToken cancellationToken)
        {
            var installedVersion = AppInfo.GetInstalledVersion();
            if (installedVersion is null)
                return new UpdateCheckResult(false, new InvalidOperationException("The installed Fidus version could not be determined."));

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, ReleasesUri);
                request.Headers.UserAgent.ParseAdd("Fidus");

                using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                var serializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                var releases = JsonSerializer.Deserialize<List<GitHubRelease>>(responseBody, serializerOptions);
                if (releases is null)
                    return new UpdateCheckResult(false, new JsonException("The GitHub releases response was empty or invalid."));

                var latestStableVersion = GetLatestStableVersion(releases);
                return new UpdateCheckResult(latestStableVersion is not null && latestStableVersion > installedVersion);
            }
            catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException or IOException or JsonException)
            {
                return new UpdateCheckResult(false, ex);
            }
        }

        private static Version? GetLatestStableVersion(IEnumerable<GitHubRelease>? releases)
        {
            if (releases is null)
                return null;

            var versions = releases
                .Where(release => !release.Draft && !release.Prerelease)
                .Select(release => ParseVersion(release.TagName))
                .OfType<Version>()
                .ToList();

            return versions.Count == 0 ? null : versions.Max();
        }

        private static Version? ParseVersion(string? tagName)
        {
            if (string.IsNullOrWhiteSpace(tagName))
                return null;

            var normalizedVersion = tagName.Trim();
            if (normalizedVersion.Length > 0 && (normalizedVersion[0] == 'v' || normalizedVersion[0] == 'V'))
                normalizedVersion = normalizedVersion[1..];

            return Version.TryParse(normalizedVersion, out var version) ? version : null;
        }

        private static HttpClient CreateHttpClient() => new()
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
    }
}
