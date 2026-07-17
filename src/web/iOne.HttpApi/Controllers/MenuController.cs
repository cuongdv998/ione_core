using System.Threading.Tasks;
using iOne.Navigation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace iOne.Controllers;

[Route("api/app/menu")]
[Authorize]
public class MenuController : iOneController
{
    private readonly IMenuAppService _menuAppService;

    public MenuController(IMenuAppService menuAppService)
    {
        _menuAppService = menuAppService;
    }

    /// <summary>
    /// Lấy danh sách menu động từ backend theo phân quyền của user hiện tại
    /// </summary>
    /// <param name="menuName">Tên menu (mặc định: "Main")</param>
    /// <returns>ApplicationMenuDto chứa danh sách menu items đã được filter theo permissions</returns>
    [HttpGet]
    public virtual async Task<ApplicationMenuDto> GetMenuAsync([FromQuery] string menuName = "Main")
    {
        return await _menuAppService.GetMenuAsync(menuName);
    }
}

