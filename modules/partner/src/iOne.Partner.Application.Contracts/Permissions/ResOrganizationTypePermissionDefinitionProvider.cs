using iOne.Partner.Localization;
using iOne.Partner.Permissions;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Partner.Permissions;

public class ResOrganizationTypePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var resOrganizationTypeGroup = context.AddGroup(
            ResOrganizationTypePermissions.GroupName,
            L("Permission:ResOrganizationType")
        );

        var resOrganizationTypePermission = resOrganizationTypeGroup.AddPermission(
            ResOrganizationTypePermissions.Default,
            L("Permission:ResOrganizationType")
        );

        resOrganizationTypePermission.AddChild(
            ResOrganizationTypePermissions.Create,
            L("Permission:Create")
        );

        resOrganizationTypePermission.AddChild(
            ResOrganizationTypePermissions.Edit,
            L("Permission:Edit")
        );

        resOrganizationTypePermission.AddChild(
            ResOrganizationTypePermissions.Delete,
            L("Permission:Delete")
        );

        resOrganizationTypePermission.AddChild(
            ResOrganizationTypePermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<PartnerResource>(name);
    }
}

