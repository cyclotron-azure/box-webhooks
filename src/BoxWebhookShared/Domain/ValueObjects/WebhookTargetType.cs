namespace BoxWebhookShared.Domain.Entities;

/// <summary>
/// Value object representing the type of webhook target.
/// </summary>
public sealed class WebhookTargetType : IEquatable<WebhookTargetType>
{
    public string Value { get; }

    private WebhookTargetType(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public static WebhookTargetType Folder => new("folder");
    public static WebhookTargetType File => new("file");

    public static WebhookTargetType FromString(string value)
    {
        return value?.ToLowerInvariant() switch
        {
            "folder" => Folder,
            "file" => File,
            _ => new WebhookTargetType(value ?? "unknown")
        };
    }

    public override string ToString() => Value;

    public override bool Equals(object? obj) => Equals(obj as WebhookTargetType);

    public bool Equals(WebhookTargetType? other) =>
        other is not null && Value == other.Value;

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(WebhookTargetType? left, WebhookTargetType? right) =>
        left?.Equals(right) ?? right is null;

    public static bool operator !=(WebhookTargetType? left, WebhookTargetType? right) =>
        !(left == right);
}
