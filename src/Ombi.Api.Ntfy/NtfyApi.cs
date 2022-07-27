using Ombi.Api.Ntfy.Models;

namespace Ombi.Api.Ntfy;

public class NtfyApi: INtfyApi
{
    public NtfyApi(IApi api)
    {
        _api = api;
    }

    private readonly IApi _api;
    
    public async Task PushAsync(string endpoint, string authorizationHeader, NtfyNotificationBody body)
    {
        var request = new Request("/", endpoint, HttpMethod.Post);
        if(!String.IsNullOrEmpty(authorizationHeader)) request.AddHeader("Authorization", authorizationHeader);
        request.ApplicationJsonContentType();
        request.AddJsonBody(body);
        
        Console.WriteLine(endpoint);
        Console.WriteLine(request.JsonBody);
        
        await _api.Request(request);
    }
}