using Box.Sdk.Gen;
using BoxWebhookDemo.Domain.Interfaces;
using BoxWebhookDemo.Infrastructure.OAuth;
using DotNetEnv;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace BoxWebhookDemo.Presentation.ConsoleUI;

/// <summary>
/// Handles authentication flow in the console UI.
/// Follows Single Responsibility principle (SRP).
/// </summary>
public class AuthenticationHandler
{
    private readonly IBoxClientFactory _clientFactory;
    private readonly IConsoleIO _console;

    public AuthenticationHandler(IBoxClientFactory clientFactory, IConsoleIO console)
    {
        _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
        _console = console ?? throw new ArgumentNullException(nameof(console));
    }

    public async Task<BoxClient> AuthenticateAsync()
    {
        _console.WriteLine("Select authentication method:");
        _console.WriteLine("1. Developer Token (for testing)");
        _console.WriteLine("2. Client Credentials Grant (CCG) - requires Enterprise account");
        _console.WriteLine("3. JWT Authentication - requires Enterprise account");
        _console.WriteLine("4. OAuth 2.0 (User Authentication) - works with Personal accounts");
        _console.Write("\nChoice: ");

        var choice = _console.ReadLine();

        return choice switch
        {
            "1" => AuthenticateWithDeveloperToken(),
            "2" => AuthenticateWithCCG(),
            "3" => await AuthenticateWithJWTAsync(),
            "4" => await AuthenticateWithOAuthAsync(),
            _ => throw new ArgumentException("Invalid authentication choice")
        };
    }

    private BoxClient AuthenticateWithDeveloperToken()
    {
        _console.Write("Enter your Developer Token: ");
        var token = _console.ReadLine();

        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Developer token cannot be empty");

        var client = _clientFactory.CreateWithDeveloperToken(token);
        _console.WriteLine("✓ Authenticated with Developer Token");
        return client;
    }

    private BoxClient AuthenticateWithCCG()
    {
        // Load .env file if it exists
        Env.TraversePath().Load();

        var clientId = Env.GetString("BOX_CLIENT_ID");
        var clientSecret = Env.GetString("BOX_CLIENT_SECRET");
        var enterpriseId = Env.GetString("BOX_ENTERPRISE_ID"); // Optional - can be null

        // Validate required credentials
        if (string.IsNullOrEmpty(clientId))
        {
            _console.Write("Enter Client ID: ");
            clientId = _console.ReadLine();
        }

        if (string.IsNullOrEmpty(clientSecret))
        {
            _console.Write("Enter Client Secret: ");
            clientSecret = _console.ReadLine();
        }

        // Enterprise ID is optional - only prompt if user wants to provide it
        if (string.IsNullOrEmpty(enterpriseId))
        {
            _console.Write("Enter Enterprise ID (press Enter to skip if not available): ");
            var input = _console.ReadLine();
            enterpriseId = string.IsNullOrWhiteSpace(input) ? null : input;
        }

        if (string.IsNullOrWhiteSpace(clientId))
            throw new ArgumentException("Error: BOX_CLIENT_ID is required and cannot be empty.");

        if (string.IsNullOrWhiteSpace(clientSecret))
            throw new ArgumentException("Error: BOX_CLIENT_SECRET is required and cannot be empty.");

        var client = _clientFactory.CreateWithCcg(clientId, clientSecret, enterpriseId);
        
        if (string.IsNullOrEmpty(enterpriseId))
            _console.WriteLine("✓ Authenticated with CCG (user-level)");
        else
            _console.WriteLine("✓ Authenticated with CCG (enterprise-level)");
        
        return client;
    }

    private async Task<BoxClient> AuthenticateWithJWTAsync()
    {
        _console.Write("Enter path to JWT config file (or base64 encoded config): ");
        var input = _console.ReadLine();

        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("JWT config path or base64 string is required");

        BoxClient client;

        if (File.Exists(input))
        {
            client = await _clientFactory.CreateWithJwtAsync(input);
        }
        else
        {
            client = await _clientFactory.CreateWithJwtFromBase64Async(input);
        }

        _console.WriteLine("✓ Authenticated with JWT");
        return client;
    }

    private async Task<BoxClient> AuthenticateWithOAuthAsync()
    {
        // Load .env file if it exists
        Env.TraversePath().Load();

        var clientId = Env.GetString("BOX_CLIENT_ID");
        var clientSecret = Env.GetString("BOX_CLIENT_SECRET");
        var redirectUri = Env.GetString("BOX_REDIRECT_URI") ?? "http://localhost:8080/callback";

        // Validate required credentials
        if (string.IsNullOrEmpty(clientId))
        {
            _console.Write("Enter Client ID: ");
            clientId = _console.ReadLine();
        }

        if (string.IsNullOrEmpty(clientSecret))
        {
            _console.Write("Enter Client Secret: ");
            clientSecret = _console.ReadLine();
        }

        if (string.IsNullOrWhiteSpace(clientId))
            throw new ArgumentException("Error: BOX_CLIENT_ID is required and cannot be empty.");

        if (string.IsNullOrWhiteSpace(clientSecret))
            throw new ArgumentException("Error: BOX_CLIENT_SECRET is required and cannot be empty.");

        // Get authorization URL
        var authorizeUrl = _clientFactory.GetOAuthAuthorizeUrl(clientId, clientSecret, redirectUri);

        _console.WriteLine("\n╔══════════════════════════════════════════════════════════════════╗");
        _console.WriteLine("║                    OAuth 2.0 Authentication                      ║");
        _console.WriteLine("╠══════════════════════════════════════════════════════════════════╣");
        _console.WriteLine("║  Choose authentication mode:                                     ║");
        _console.WriteLine("║  1. Automatic (starts local server, opens browser)               ║");
        _console.WriteLine("║  2. Manual (copy URL, paste authorization code)                  ║");
        _console.WriteLine("╚══════════════════════════════════════════════════════════════════╝");
        _console.Write("\nChoice (1 or 2): ");
        
        var modeChoice = _console.ReadLine();
        
        string authorizationCode;
        
        if (modeChoice == "1")
        {
            authorizationCode = await AuthenticateWithOAuthAutomaticAsync(authorizeUrl, redirectUri);
        }
        else
        {
            authorizationCode = AuthenticateWithOAuthManual(authorizeUrl);
        }

        var client = await _clientFactory.CreateWithOAuthAsync(clientId, clientSecret, authorizationCode);
        _console.WriteLine("✓ Authenticated with OAuth 2.0");
        return client;
    }

    private async Task<string> AuthenticateWithOAuthAutomaticAsync(string authorizeUrl, string redirectUri)
    {
        _console.WriteLine("\n🚀 Starting local callback server...");
        
        using var callbackServer = new OAuthCallbackServer(redirectUri);
        
        _console.WriteLine($"✓ Listening on: {redirectUri}");
        _console.WriteLine("\n📱 Opening browser for authorization...\n");
        
        // Open browser
        OpenBrowser(authorizeUrl);
        
        _console.WriteLine("⏳ Waiting for authorization (press Ctrl+C to cancel)...\n");
        
        try
        {
            var code = await callbackServer.WaitForAuthorizationCodeAsync();
            _console.WriteLine("\n✓ Authorization code received!");
            return code;
        }
        catch (Exception ex)
        {
            _console.WriteLine($"\n✗ Error: {ex.Message}");
            throw;
        }
    }

    private string AuthenticateWithOAuthManual(string authorizeUrl)
    {
        _console.WriteLine("\n📋 Manual OAuth 2.0 Flow:");
        _console.WriteLine("─────────────────────────────────────────────────────────────────");
        _console.WriteLine("1. Open this URL in your browser:\n");
        _console.WriteLine($"   {authorizeUrl}\n");
        _console.WriteLine("2. Log in to Box and authorize the application");
        _console.WriteLine("3. Copy the 'code' parameter from the redirect URL");
        _console.WriteLine("   Example: http://localhost:8080/callback?code=AUTHORIZATION_CODE");
        _console.WriteLine("─────────────────────────────────────────────────────────────────\n");

        _console.Write("Enter Authorization Code: ");
        var authorizationCode = _console.ReadLine();

        if (string.IsNullOrWhiteSpace(authorizationCode))
            throw new ArgumentException("Authorization code cannot be empty");

        return authorizationCode;
    }

    private void OpenBrowser(string url)
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
            else
            {
                _console.WriteLine($"Please open this URL manually: {url}");
            }
        }
        catch
        {
            _console.WriteLine($"Could not open browser. Please open this URL manually:\n{url}");
        }
    }
}
