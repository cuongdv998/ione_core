using Microsoft.Extensions.Localization;
using iOne.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace iOne;

[Dependency(ReplaceServices = true)]
public class iOneBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<iOneResource> _localizer;

    public iOneBrandingProvider(IStringLocalizer<iOneResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
