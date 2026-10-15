using System.Reflection;

namespace Fidus.Utils
{
    public static class AppInfo
    {
        public static string GetInstalledVersionText()
        {
            var informationalVersion = typeof(AppInfo).Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion;

            return informationalVersion ?? typeof(AppInfo).Assembly.GetName().Version?.ToString() ?? "unknown";
        }

        internal static Version? GetInstalledVersion()
        {
            var versionText = GetInstalledVersionText();
            var candidate = versionText.Split('+')[0].Split('-')[0];

            if (candidate.StartsWith("v", StringComparison.OrdinalIgnoreCase))
                candidate = candidate[1..];

            return Version.TryParse(candidate, out var version) ? version : typeof(AppInfo).Assembly.GetName().Version;
        }
    }
}
