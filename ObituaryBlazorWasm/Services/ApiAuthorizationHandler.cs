using System.Net.Http.Headers;

namespace ObituaryBlazorWasm.Services;

/*
    This class is a DelegatingHandler that automatically adds a Bearer token to outgoing HTTP requests.
    Why use DelegatingHandler?
    - To centralize the logic for adding authorization headers to all HTTP requests made by HttpClient.
*/
public class ApiAuthorizationHandler : DelegatingHandler
{
    private readonly ITokenProvider _tokenProvider;

    // Constructor
    public ApiAuthorizationHandler(ITokenProvider tokenProvider)
    {
        _tokenProvider = tokenProvider;
    }

    /*
        Overrides SendAsync to add the Authorization header with Bearer token.
    */
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // If request uri is not null and contains "/api", add the Authorization header. (Just another extra security wise :))
        if (request.RequestUri != null && request.RequestUri.AbsoluteUri.Contains("/api"))
        {
            var token = await _tokenProvider.GetTokenAsync();

            // If token is available, add it to the request headers
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        // Proceed with the HTTP request
        return await base.SendAsync(request, cancellationToken);
    }
}