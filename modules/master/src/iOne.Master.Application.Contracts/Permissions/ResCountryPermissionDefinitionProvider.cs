using iOne.Master.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Master.Permissions;

public class ResCountryPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var resCountryGroup = context.AddGroup(
            ResCountryPermissions.GroupName,
            L("Permission:ResCountry")
        );

        var resCountryPermission = resCountryGroup.AddPermission(
            ResCountryPermissions.Default,
            L("Permission:ResCountry")
        );

        resCountryPermission.AddChild(
            ResCountryPermissions.Create,
            L("Permission:Create")
        );

        resCountryPermission.AddChild(
            ResCountryPermissions.Edit,
            L("Permission:Edit")
        );

        resCountryPermission.AddChild(
            ResCountryPermissions.Delete,
            L("Permission:Delete")
        );

        resCountryPermission.AddChild(
            ResCountryPermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MasterResource>(name);
    }
}

