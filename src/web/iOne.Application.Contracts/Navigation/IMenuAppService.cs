using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Navigation;

public interface IMenuAppService : IApplicationService
{
    /// <summary>
    /// Lấy danh sách menu động từ backend theo phân quyền của user hiện tại
    /// </summary>
    /// <param name="menuName">Tên menu (mặc định: "Main")</param>
    /// <returns>ApplicationMenuDto chứa danh sách menu items đã được filter theo permissions</returns>
    Task<ApplicationMenuDto> GetMenuAsync(string menuName = "Main");
}

