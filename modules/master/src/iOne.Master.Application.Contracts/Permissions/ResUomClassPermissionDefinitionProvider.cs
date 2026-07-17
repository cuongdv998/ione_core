using iOne.Master.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Master.Permissions;

public class ResUomClassPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var resUomClassGroup = context.AddGroup(
            ResUomClassPermissions.GroupName,
            L("Permission:ResUomClass")
        );

        var resUomClassPermission = resUomClassGroup.AddPermission(
            ResUomClassPermissions.Default,
            L("Permission:ResUomClass")
        );

        resUomClassPermission.AddChild(
            ResUomClassPermissions.Create,
            L("Permission:Create")
        );

        resUomClassPermission.AddChild(
            ResUomClassPermissions.Edit,
            L("Permission:Edit")
        );

        resUomClassPermission.AddChild(
            ResUomClassPermissions.Delete,
            L("Permission:Delete")
        );

        resUomClassPermission.AddChild(
            ResUomClassPermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MasterResource>(name);
    }
}

