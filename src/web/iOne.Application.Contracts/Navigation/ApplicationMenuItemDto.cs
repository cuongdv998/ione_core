using System.Collections.Generic;

namespace iOne.Navigation;

public class ApplicationMenuItemDto
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Url { get; set; }
    public string? Icon { get; set; }
    public int Order { get; set; }
    public string? RequiredPermissionName { get; set; }
    public List<ApplicationMenuItemDto>? Items { get; set; }
}

