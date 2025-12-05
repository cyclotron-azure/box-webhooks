namespace BoxWebhookDemo.Domain.Entities;

/// <summary>
/// Value object representing webhook trigger types.
/// </summary>
public sealed class WebhookTrigger : IEquatable<WebhookTrigger>
{
    public string Value { get; }

    private WebhookTrigger(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    // File triggers
    public static WebhookTrigger FileUploaded => new("FILE.UPLOADED");
    public static WebhookTrigger FileDownloaded => new("FILE.DOWNLOADED");
    public static WebhookTrigger FilePreviewed => new("FILE.PREVIEWED");
    public static WebhookTrigger FileTrashed => new("FILE.TRASHED");
    public static WebhookTrigger FileDeleted => new("FILE.DELETED");
    public static WebhookTrigger FileRestored => new("FILE.RESTORED");
    public static WebhookTrigger FileCopied => new("FILE.COPIED");
    public static WebhookTrigger FileMoved => new("FILE.MOVED");
    public static WebhookTrigger FileLocked => new("FILE.LOCKED");
    public static WebhookTrigger FileUnlocked => new("FILE.UNLOCKED");
    public static WebhookTrigger FileRenamed => new("FILE.RENAMED");

    // Folder triggers
    public static WebhookTrigger FolderCreated => new("FOLDER.CREATED");
    public static WebhookTrigger FolderDownloaded => new("FOLDER.DOWNLOADED");
    public static WebhookTrigger FolderTrashed => new("FOLDER.TRASHED");
    public static WebhookTrigger FolderDeleted => new("FOLDER.DELETED");
    public static WebhookTrigger FolderRestored => new("FOLDER.RESTORED");
    public static WebhookTrigger FolderCopied => new("FOLDER.COPIED");
    public static WebhookTrigger FolderMoved => new("FOLDER.MOVED");
    public static WebhookTrigger FolderRenamed => new("FOLDER.RENAMED");

    // Common trigger sets
    public static IReadOnlyList<WebhookTrigger> AllFileUploadEvents =>
        new[] { FileUploaded };

    public static IReadOnlyList<WebhookTrigger> AllFileAccessEvents =>
        new[] { FileUploaded, FileDownloaded, FilePreviewed };

    public static IReadOnlyList<WebhookTrigger> AllFileLifecycleEvents =>
        new[] { FileUploaded, FileTrashed, FileDeleted, FileRestored, FileCopied, FileMoved };

    public static WebhookTrigger FromString(string value)
    {
        return new WebhookTrigger(value.ToUpperInvariant());
    }

    public override string ToString() => Value;

    public override bool Equals(object? obj) => Equals(obj as WebhookTrigger);

    public bool Equals(WebhookTrigger? other) =>
        other is not null && Value == other.Value;

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(WebhookTrigger? left, WebhookTrigger? right) =>
        left?.Equals(right) ?? right is null;

    public static bool operator !=(WebhookTrigger? left, WebhookTrigger? right) =>
        !(left == right);
}
