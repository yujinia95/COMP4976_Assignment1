using System.Net.Http.Json;
using ObituaryBlazorWasm.Models.Auth;

namespace ObituaryBlazorWasm.Services;

/*
    This class handles authentication-related API calls such as signup and login.
*/
public class AuthApiClient
{
    private readonly HttpClient _http;
    private readonly ITokenProvider _tokens;

    /*
        Constructor
    */
    public AuthApiClient(HttpClient http, ITokenProvider tokens)
    {
        _http = http;
        _tokens = tokens;
    }

    /*
        This method handles user signup by sending a POST request to the API.
    */
    public async Task<(bool ok, string? error)> SignupAsync(SignupModel model)
    {
        var response = await _http.PostAsJsonAsync("api/account/register", model);
        
        // If the response indicates success, return true; otherwise, return false with error message
        if (response.IsSuccessStatusCode)
        {
            return (true, null);
        }
        else
        {
            return (false, await response.Content.ReadAsStringAsync());
        }
    }

    /*
        This method handles user login by sending a POST request to the API.
    */
    public async Task<(bool ok, string? error)> LoginAsync(LoginModel model)
    {
        var response = await _http.PostAsJsonAsync("api/account/login", model);

        // If the response indicates failure, return false with error message
        if (!response.IsSuccessStatusCode)
        {
            return (false, await response.Content.ReadAsStringAsync());
        }

        var body = await response.Content.ReadFromJsonAsync<LoginResult>();

        // If the token is not valid, return false with error message
        if (string.IsNullOrWhiteSpace(body?.Token))
        {
            return (false, "Invalid token received");
        }

        await _tokens.SetTokenAsync(body.Token);
        return (true, null);
    }
    
    /*
        This method handles user logout by removing the stored token.
    */
    public async Task LogoutAsync()
    {
        await _tokens.RemoveTokenAsync();
    }
}