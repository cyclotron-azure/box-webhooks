namespace BoxWebhookShared.Domain.Entities;

/// <summary>
/// Domain entity representing a Box folder.
/// </summary>
public class FolderEntity
{
    public string Id { get; }
    public string Name { get; }
    public DateTime? CreatedAt { get; }

    public FolderEntity(string id, string name, DateTime? createdAt = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Folder ID cannot be empty", nameof(id));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Folder name cannot be empty", nameof(name));

        Id = id;
        Name = name;
        CreatedAt = createdAt;
    }
}
