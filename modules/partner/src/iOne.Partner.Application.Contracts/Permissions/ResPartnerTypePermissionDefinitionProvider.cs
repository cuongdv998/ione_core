using iOne.Partner.Localization;
using iOne.Partner.Permissions;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Partner.Permissions;

public class ResPartnerTypePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var resPartnerTypeGroup = context.AddGroup(
            ResPartnerTypePermissions.GroupName,
            L("Permission:ResPartnerType")
        );

        var resPartnerTypePermission = resPartnerTypeGroup.AddPermission(
            ResPartnerTypePermissions.Default,
            L("Permission:ResPartnerType")
        );

        resPartnerTypePermission.AddChild(
            ResPartnerTypePermissions.Create,
            L("Permission:Create")
        );

        resPartnerTypePermission.AddChild(
            ResPartnerTypePermissions.Edit,
            L("Permission:Edit")
        );

        resPartnerTypePermission.AddChild(
            ResPartnerTypePermissions.Delete,
            L("Permission:Delete")
        );

        resPartnerTypePermission.AddChild(
            ResPartnerTypePermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<PartnerResource>(name);
    }
}

