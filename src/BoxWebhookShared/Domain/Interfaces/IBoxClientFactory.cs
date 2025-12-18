using Box.Sdk.Gen;

namespace BoxWebhookShared.Domain.Interfaces;

/// <summary>
/// Factory interface for creating authenticated Box clients.
/// Follows the Factory pattern and Dependency Inversion principle.
/// </summary>
public interface IBoxClientFactory
{
    /// <summary>
    /// Creates a Box client using a developer token.
    /// </summary>
    BoxClient CreateWithDeveloperToken(string token);

    /// <summary>
    /// Creates a Box client using Client Credentials Grant.
    /// </summary>
    BoxClient CreateWithCcg(string clientId, string clientSecret, string? enterpriseId = null);

    /// <summary>
    /// Creates a Box client using JWT authentication.
    /// </summary>
    Task<BoxClient> CreateWithJwtAsync(string configPath);

    /// <summary>
    /// Creates a Box client using JWT from base64 encoded config.
    /// </summary>
    Task<BoxClient> CreateWithJwtFromBase64Async(string base64Config);

    /// <summary>
    /// Gets the OAuth 2.0 authorization URL for user authentication.
    /// Works with personal/free Box accounts.
    /// </summary>
    string GetOAuthAuthorizeUrl(string clientId, string clientSecret, string redirectUri);

    /// <summary>
    /// Creates a Box client using OAuth 2.0 authorization code.
    /// Works with personal/free Box accounts.
    /// </summary>
    Task<BoxClient> CreateWithOAuthAsync(string clientId, string clientSecret, string authorizationCode);
}
