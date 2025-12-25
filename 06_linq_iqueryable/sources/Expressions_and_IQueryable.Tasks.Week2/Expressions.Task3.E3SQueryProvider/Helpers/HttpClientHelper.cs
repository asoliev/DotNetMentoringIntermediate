using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Expressions.Task3.E3SQueryProvider.Helpers;

public static class HttpClientHelper
{
    public static HttpClient CreateClient(string userName, string password)
    {
        var handler = new HttpClientHandler
        {
            AllowAutoRedirect = true,
            PreAuthenticate = true
        };

        HttpClient client = new(handler, disposeHandler: true);

        string credentials = $"{userName}:{password}";
        byte[] credentialsBytes = Encoding.ASCII.GetBytes(credentials);
        string base64Credentials = Convert.ToBase64String(credentialsBytes);
        AuthenticationHeaderValue authHeader = new("Basic", base64Credentials);
        client.DefaultRequestHeaders.Authorization = authHeader;

        return client;
    }
}