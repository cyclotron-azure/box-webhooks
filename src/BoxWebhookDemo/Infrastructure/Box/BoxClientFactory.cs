using Box.Sdk.Gen;
using BoxWebhookDemo.Domain.Interfaces;

namespace BoxWebhookDemo.Infrastructure.Box;

/// <summary>
/// Factory for creating authenticated Box clients.
/// Follows Open/Closed principle (OCP) - open for extension, closed for modification.
/// </summary>
public class BoxClientFactory : IBoxClientFactory
{
    public BoxClient CreateWithDeveloperToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Developer token cannot be empty", nameof(token));

        var auth = new BoxDeveloperTokenAuth(token: token);
        return new BoxClient(auth: auth);
    }

    public BoxClient CreateWithCcg(string clientId, string clientSecret, string? enterpriseId = null)
    {
        if (string.IsNullOrWhiteSpace(clientId))
            throw new ArgumentException("Client ID cannot be empty", nameof(clientId));
        if (string.IsNullOrWhiteSpace(clientSecret))
            throw new ArgumentException("Client Secret cannot be empty", nameof(clientSecret));

        var config = new CcgConfig(
            clientId: clientId,
            clientSecret: clientSecret)
        {
            EnterpriseId = enterpriseId
        };

        var auth = new BoxCcgAuth(config: config);
        return new BoxClient(auth: auth);
    }

    public async Task<BoxClient> CreateWithJwtAsync(string configPath)
    {
        if (string.IsNullOrWhiteSpace(configPath))
            throw new ArgumentException("Config path cannot be empty", nameof(configPath));

        if (!File.Exists(configPath))
            throw new FileNotFoundException("JWT config file not found", configPath);

        var configJson = await File.ReadAllTextAsync(configPath);
        var jwtConfig = JwtConfig.FromConfigJsonString(configJsonString: configJson);

        var auth = new BoxJwtAuth(config: jwtConfig);
        return new BoxClient(auth: auth);
    }

    public Task<BoxClient> CreateWithJwtFromBase64Async(string base64Config)
    {
        if (string.IsNullOrWhiteSpace(base64Config))
            throw new ArgumentException("Base64 config cannot be empty", nameof(base64Config));

        try
        {
            var decodedConfig = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(base64Config));
            var jwtConfig = JwtConfig.FromConfigJsonString(configJsonString: decodedConfig);

            var auth = new BoxJwtAuth(config: jwtConfig);
            return Task.FromResult(new BoxClient(auth: auth));
        }
        catch (FormatException)
        {
            throw new ArgumentException("Invalid base64 encoded JWT config", nameof(base64Config));
        }
    }

    public string GetOAuthAuthorizeUrl(string clientId, string clientSecret, string redirectUri)
    {
        if (string.IsNullOrWhiteSpace(clientId))
            throw new ArgumentException("Client ID cannot be empty", nameof(clientId));
        if (string.IsNullOrWhiteSpace(clientSecret))
            throw new ArgumentException("Client Secret cannot be empty", nameof(clientSecret));
        if (string.IsNullOrWhiteSpace(redirectUri))
            throw new ArgumentException("Redirect URI cannot be empty", nameof(redirectUri));

        var config = new OAuthConfig(clientId: clientId, clientSecret: clientSecret);
        var auth = new BoxOAuth(config: config);

        return auth.GetAuthorizeUrl(new GetAuthorizeUrlOptions { RedirectUri = redirectUri });
    }

    public async Task<BoxClient> CreateWithOAuthAsync(string clientId, string clientSecret, string authorizationCode)
    {
        if (string.IsNullOrWhiteSpace(clientId))
            throw new ArgumentException("Client ID cannot be empty", nameof(clientId));
        if (string.IsNullOrWhiteSpace(clientSecret))
            throw new ArgumentException("Client Secret cannot be empty", nameof(clientSecret));
        if (string.IsNullOrWhiteSpace(authorizationCode))
            throw new ArgumentException("Authorization code cannot be empty", nameof(authorizationCode));

        var config = new OAuthConfig(clientId: clientId, clientSecret: clientSecret);
        var auth = new BoxOAuth(config: config);

        await auth.GetTokensAuthorizationCodeGrantAsync(authorizationCode);
        return new BoxClient(auth: auth);
    }
}
