using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.UI.Navigation;
using Volo.Abp.Authorization.Permissions;

namespace iOne.Navigation;

public class MenuAppService : iOneAppService, IMenuAppService
{
    private readonly IMenuManager _menuManager;
    private readonly IPermissionChecker _permissionChecker;

    public MenuAppService(
        IMenuManager menuManager,
        IPermissionChecker permissionChecker)
    {
        _menuManager = menuManager;
        _permissionChecker = permissionChecker;
    }

    public async Task<ApplicationMenuDto> GetMenuAsync(string menuName = "Main")
    {
        // Lấy menu từ ABP MenuManager
        var menu = await _menuManager.GetAsync(menuName);

        // Convert ApplicationMenu sang DTO và filter theo permissions
        var menuDto = new ApplicationMenuDto
        {
            Name = menu.Name,
            Items = await ConvertMenuItemsAsync(menu.Items)
        };

        return menuDto;
    }

    private List<ApplicationMenuItem> GetAllMenuItems(IList<ApplicationMenuItem> items)
    {
        var result = new List<ApplicationMenuItem>();
        foreach (var item in items)
        {
            result.Add(item);
            if (item.Items != null && item.Items.Any())
            {
                result.AddRange(GetAllMenuItems(item.Items));
            }
        }
        return result;
    }

    private async Task<List<ApplicationMenuItemDto>> ConvertMenuItemsAsync(
        IList<ApplicationMenuItem> items)
    {
        var result = new List<ApplicationMenuItemDto>();

        foreach (var item in items.OrderBy(x => x.Order))
        {
            // ABP MenuManager.GetAsync đã tự động filter menu theo permissions của user hiện tại
            // Nên các items trả về đã được filter, không cần check lại permissions
            // Chỉ cần convert sang DTO

            var itemDto = new ApplicationMenuItemDto
            {
                Name = item.Name,
                DisplayName = item.DisplayName,
                Url = item.Url,
                Icon = item.Icon,
                Order = item.Order,
                // Lưu permission name để frontend có thể sử dụng (nếu cần). RequiredPermissionName obsolete: dùng RequirePermissions khi tạo menu.
#pragma warning disable CS0618 // Type or member is obsolete
                RequiredPermissionName = item.RequiredPermissionName
#pragma warning restore CS0618
            };

            // Convert sub-items đệ quy
            if (item.Items != null && item.Items.Any())
            {
                itemDto.Items = await ConvertMenuItemsAsync(item.Items);
                
                // Nếu sau khi filter, không còn sub-items nào và item không có URL, bỏ qua parent item
                if ((itemDto.Items == null || !itemDto.Items.Any()) && string.IsNullOrEmpty(itemDto.Url))
                {
                    continue; // Bỏ qua parent item nếu không còn sub-items và không có URL
                }
            }

            result.Add(itemDto);
        }

        return result;
    }
}

