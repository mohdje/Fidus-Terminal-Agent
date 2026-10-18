
namespace Fidus.Models
{
    public sealed record UpdateCheckResult(bool IsUpdateAvailable, Exception? Error = null);
}