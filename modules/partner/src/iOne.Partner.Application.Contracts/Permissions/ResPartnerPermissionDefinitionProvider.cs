using iOne.Partner.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Partner.Permissions;

public class ResPartnerPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var resPartnerGroup = context.AddGroup(
            ResPartnerPermissions.GroupName,
            L("Permission:ResPartner")
        );

        var resPartnerPermission = resPartnerGroup.AddPermission(
            ResPartnerPermissions.Default,
            L("Permission:ResPartner")
        );

        resPartnerPermission.AddChild(
            ResPartnerPermissions.Create,
            L("Permission:Create")
        );

        resPartnerPermission.AddChild(
            ResPartnerPermissions.Edit,
            L("Permission:Edit")
        );

        resPartnerPermission.AddChild(
            ResPartnerPermissions.Delete,
            L("Permission:Delete")
        );

        resPartnerPermission.AddChild(
            ResPartnerPermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<PartnerResource>(name);
    }
}

