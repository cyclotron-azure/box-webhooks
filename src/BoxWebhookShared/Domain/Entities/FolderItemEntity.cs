namespace BoxWebhookShared.Domain.Entities;

/// <summary>
/// Domain entity representing an item within a Box folder.
/// </summary>
public class FolderItemEntity
{
    public string Id { get; }
    public string Name { get; }
    public FolderItemType ItemType { get; }

    public FolderItemEntity(string id, string name, FolderItemType itemType)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Item ID cannot be empty", nameof(id));

        Id = id;
        Name = name ?? string.Empty;
        ItemType = itemType;
    }
}

/// <summary>
/// Types of items that can exist in a folder.
/// </summary>
public enum FolderItemType
{
    Folder,
    File,
    WebLink
}
