using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace ObituaryBlazorWasm.Services;

/*
    This interface defines methods for managing JWT tokens in a Blazor WebAssembly application.
*/
public interface ITokenProvider
{
    Task SetTokenAsync(string token);
    Task<string?> GetTokenAsync();
    Task RemoveTokenAsync();
}

/*
    This class implements the ITokenProvider interface using browser's local storage via JavaScript interop.
    Why triying to store token in browser local storage?
    - To persist the authentication token across browser sessions.
*/
public class BrowserTokenProvider : ITokenProvider
{
    private const string Key = "auth_token";
    private readonly IJSRuntime _js;

    // Constructor to initialize JS runtime
    public BrowserTokenProvider(IJSRuntime js)
    {
        _js = js;
    }

    /*
        Sets the JWT token in browser's local storage.
    */
    public async Task SetTokenAsync(string token)
    {
        await _js.InvokeVoidAsync("localStorage.setItem", Key, token);
    }

    /*
        Retrieves the JWT token from browser's local storage.
    */
    public async Task<string?> GetTokenAsync()
    {
        return await _js.InvokeAsync<string>("localStorage.getItem", Key);
    }

    /*
        Removes the JWT token from browser's local storage.
    */
    public async Task RemoveTokenAsync()
    {
        await _js.InvokeVoidAsync("localStorage.removeItem", Key);
    }
}