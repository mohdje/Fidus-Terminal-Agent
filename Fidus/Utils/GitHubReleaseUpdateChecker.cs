using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fidus.Utils
{
    public sealed class GitHubRelease
    {
        [JsonPropertyName("tag_name")]
        public string? TagName { get; init; }

        [JsonPropertyName("draft")]
        public bool Draft { get; init; }

        [JsonPropertyName("prerelease")]
        public bool Prerelease { get; init; }
    }

    public sealed class GitHubReleaseUpdateChecker
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public static readonly Uri ReleasesUri = new("https://api.github.com/repos/mohdje/Fidus-Terminal-Agent/releases");

        private readonly HttpClient httpClient;

        public GitHubReleaseUpdateChecker(HttpClient? httpClient = null)
        {
            this.httpClient = httpClient ?? new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(5)
            };
        }

        public async Task<bool> CheckForUpdateAsync(CancellationToken cancellationToken = default)
        {
            var installedVersion = AppInfo.GetInstalledVersion();
            if (installedVersion is null)
                return false;

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, ReleasesUri);
                request.Headers.UserAgent.ParseAdd("Fidus");

                using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                    return false;

                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                var releases = JsonSerializer.Deserialize<List<GitHubRelease>>(responseBody, SerializerOptions) ?? [];
                var latestStableVersion = GetLatestStableVersion(releases);

                return latestStableVersion is not null && latestStableVersion > installedVersion;
            }
            catch (HttpRequestException)
            {
                return false;
            }
            catch (TaskCanceledException)
            {
                return false;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        public static Version? GetLatestStableVersion(IEnumerable<GitHubRelease>? releases)
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

        public static Version? ParseVersion(string? tagName)
        {
            var normalizedVersion = NormalizeTag(tagName);
            if (string.IsNullOrWhiteSpace(normalizedVersion))
                return null;

            return Version.TryParse(normalizedVersion, out var version) ? version : null;
        }

        public static string? NormalizeTag(string? tagName)
        {
            if (string.IsNullOrWhiteSpace(tagName))
                return null;

            var normalizedVersion = tagName.Trim();
            if (normalizedVersion.Length > 0 && (normalizedVersion[0] == 'v' || normalizedVersion[0] == 'V'))
                normalizedVersion = normalizedVersion[1..];

            return normalizedVersion;
        }
    }
}
