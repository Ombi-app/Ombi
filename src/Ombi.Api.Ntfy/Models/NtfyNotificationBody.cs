using Newtonsoft.Json;

namespace Ombi.Api.Ntfy.Models;

public class NtfyNotificationBody
{
    [JsonConstructor]
    public NtfyNotificationBody()
    {
    }

    public string topic { get; set; }
    public string message { get; set; }
    public string title { get; set; }
    public List<string> tags { get; set; }
    public sbyte priority { get; set; }
    public string click { get; set; }
    public string attach { get; set; }
    public string filename { get; set; }
    public string delay { get; set; }
}