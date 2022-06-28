using System.Globalization;

namespace Netstream.Nws.Client.Daemon;

public class AzureConfig
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string TenantId { get; set; }
    public string Instance { get; set; }
    /// <summary>
    /// URL of the authority
    /// </summary>
    public string Authority => String.Format(CultureInfo.InvariantCulture, Instance, TenantId);
}