using System.Net;
using System.Text;
using System.Web;

namespace BoxWebhookDemo.Infrastructure.OAuth;

/// <summary>
/// A temporary HTTP server that listens for OAuth 2.0 callback redirects.
/// Captures the authorization code from the callback URL.
/// </summary>
public class OAuthCallbackServer : IDisposable
{
    private readonly HttpListener _listener;
    private readonly string _redirectUri;
    private bool _disposed;

    public OAuthCallbackServer(string redirectUri = "http://localhost:8080/callback/")
    {
        // Ensure URI ends with /
        _redirectUri = redirectUri.EndsWith("/") ? redirectUri : redirectUri + "/";

        _listener = new HttpListener();
        _listener.Prefixes.Add(_redirectUri);
    }

    /// <summary>
    /// Starts the callback server and waits for the authorization code.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to stop waiting</param>
    /// <returns>The authorization code from the callback</returns>
    public async Task<string> WaitForAuthorizationCodeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _listener.Start();
            
            // Wait for the callback request
            var contextTask = _listener.GetContextAsync();
            
            // Wait for either the context or cancellation
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var completedTask = await Task.WhenAny(
                contextTask,
                Task.Delay(Timeout.Infinite, cts.Token)
            );

            if (completedTask != contextTask)
            {
                throw new OperationCanceledException("OAuth callback wait was cancelled");
            }

            var context = await contextTask;
            var request = context.Request;
            var response = context.Response;

            // Parse the query string for the code
            var query = request.Url?.Query;
            string? code = null;
            string? error = null;
            string? errorDescription = null;

            if (!string.IsNullOrEmpty(query))
            {
                var queryParams = HttpUtility.ParseQueryString(query);
                code = queryParams["code"];
                error = queryParams["error"];
                errorDescription = queryParams["error_description"];
            }

            // Send response to browser
            string responseHtml;
            if (!string.IsNullOrEmpty(code))
            {
                responseHtml = GetSuccessHtml();
                response.StatusCode = 200;
            }
            else if (!string.IsNullOrEmpty(error))
            {
                responseHtml = GetErrorHtml(error, errorDescription);
                response.StatusCode = 400;
            }
            else
            {
                responseHtml = GetErrorHtml("missing_code", "No authorization code received");
                response.StatusCode = 400;
            }

            var buffer = Encoding.UTF8.GetBytes(responseHtml);
            response.ContentLength64 = buffer.Length;
            response.ContentType = "text/html; charset=utf-8";
            await response.OutputStream.WriteAsync(buffer, cancellationToken);
            response.Close();

            if (string.IsNullOrEmpty(code))
            {
                throw new InvalidOperationException(
                    string.Format("OAuth authorization failed: {0} - {1}", 
                        error ?? "unknown error", 
                        errorDescription ?? "No description"));
            }

            return code;
        }
        finally
        {
            Stop();
        }
    }

    /// <summary>
    /// Stops the HTTP listener.
    /// </summary>
    public void Stop()
    {
        if (_listener.IsListening)
        {
            _listener.Stop();
        }
    }

    private static string GetSuccessHtml()
    {
        return @"<!DOCTYPE html>
<html>
<head>
    <title>Authorization Successful</title>
    <style>
        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            display: flex;
            justify-content: center;
            align-items: center;
            height: 100vh;
            margin: 0;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
        }
        .container {
            text-align: center;
            background: white;
            padding: 40px 60px;
            border-radius: 16px;
            box-shadow: 0 20px 60px rgba(0,0,0,0.3);
        }
        .checkmark { font-size: 64px; margin-bottom: 20px; }
        h1 { color: #22c55e; margin: 0 0 10px 0; }
        p { color: #666; margin: 0; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='checkmark'>✅</div>
        <h1>Authorization Successful!</h1>
        <p>You can close this window and return to the application.</p>
    </div>
</body>
</html>";
    }

    private static string GetErrorHtml(string error, string? description)
    {
        var safeError = WebUtility.HtmlEncode(error);
        var safeDescription = WebUtility.HtmlEncode(description ?? "An error occurred during authorization");
        
        return string.Format(@"<!DOCTYPE html>
<html>
<head>
    <title>Authorization Failed</title>
    <style>
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            display: flex;
            justify-content: center;
            align-items: center;
            height: 100vh;
            margin: 0;
            background: linear-gradient(135deg, #ff6b6b 0%, #ee5a5a 100%);
        }}
        .container {{
            text-align: center;
            background: white;
            padding: 40px 60px;
            border-radius: 16px;
            box-shadow: 0 20px 60px rgba(0,0,0,0.3);
        }}
        .icon {{ font-size: 64px; margin-bottom: 20px; }}
        h1 {{ color: #ef4444; margin: 0 0 10px 0; }}
        p {{ color: #666; margin: 0; }}
        .error {{ color: #999; font-size: 14px; margin-top: 15px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='icon'>❌</div>
        <h1>Authorization Failed</h1>
        <p>{0}</p>
        <p class='error'>Error: {1}</p>
    </div>
</body>
</html>", safeDescription, safeError);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Stop();
            _listener.Close();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
