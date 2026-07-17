using iOne.Master.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Master.Permissions;

public class ResObjectTypePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var resObjectTypeGroup = context.AddGroup(
            ResObjectTypePermissions.GroupName,
            L("Permission:ResObjectType")
        );

        var resObjectTypePermission = resObjectTypeGroup.AddPermission(
            ResObjectTypePermissions.Default,
            L("Permission:ResObjectType")
        );

        resObjectTypePermission.AddChild(
            ResObjectTypePermissions.Create,
            L("Permission:Create")
        );

        resObjectTypePermission.AddChild(
            ResObjectTypePermissions.Edit,
            L("Permission:Edit")
        );

        resObjectTypePermission.AddChild(
            ResObjectTypePermissions.Delete,
            L("Permission:Delete")
        );

        resObjectTypePermission.AddChild(
            ResObjectTypePermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MasterResource>(name);
    }
}

