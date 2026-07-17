using System.Collections.Generic;

namespace iOne.Navigation;

public class ApplicationMenuDto
{
    public string Name { get; set; } = string.Empty;
    public List<ApplicationMenuItemDto> Items { get; set; } = new();
}

