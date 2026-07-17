namespace iOne.ResObjectTypes;

/// <summary>
/// Helper class for converting between ResObjectGroup enum and string values (M/P)
/// </summary>
public static class ResObjectGroupHelper
{
    /// <summary>
    /// Converts ResObjectGroup enum to string representation
    /// Person -> "M", Property -> "P", null -> null
    /// </summary>
    public static string? ToString(ResObjectGroup? objectGroup)
    {
        if (!objectGroup.HasValue)
        {
            return null;
        }

        return objectGroup.Value switch
        {
            ResObjectGroup.Person => "M",
            ResObjectGroup.Property => "P",
            _ => null
        };
    }

    /// <summary>
    /// Converts string representation to ResObjectGroup enum
    /// "M" -> Person, "P" -> Property, null/empty/other -> null
    /// </summary>
    public static ResObjectGroup? FromString(string? objectGroupString)
    {
        if (string.IsNullOrWhiteSpace(objectGroupString))
        {
            return null;
        }

        var normalized = objectGroupString.Trim().ToUpperInvariant();
        return normalized switch
        {
            "M" => ResObjectGroup.Person,
            "P" => ResObjectGroup.Property,
            _ => null
        };
    }
}

