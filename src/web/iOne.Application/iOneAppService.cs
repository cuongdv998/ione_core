using iOne.Localization;
using Volo.Abp.Application.Services;

namespace iOne;

/* Inherit your application services from this class.
 */
public abstract class iOneAppService : ApplicationService
{
    protected iOneAppService()
    {
        LocalizationResource = typeof(iOneResource);
    }
}
