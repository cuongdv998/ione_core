using iOne.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace iOne.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class iOneController : AbpControllerBase
{
    protected iOneController()
    {
        LocalizationResource = typeof(iOneResource);
    }
}
