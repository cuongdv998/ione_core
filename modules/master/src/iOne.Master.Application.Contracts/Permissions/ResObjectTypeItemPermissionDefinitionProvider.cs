using iOne.Master.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Master.Permissions;

public class ResObjectTypeItemPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var resObjectTypeItemGroup = context.AddGroup(
            ResObjectTypeItemPermissions.GroupName,
            L("Permission:ResObjectTypeItem")
        );

        var resObjectTypeItemPermission = resObjectTypeItemGroup.AddPermission(
            ResObjectTypeItemPermissions.Default,
            L("Permission:ResObjectTypeItem")
        );

        resObjectTypeItemPermission.AddChild(
            ResObjectTypeItemPermissions.Create,
            L("Permission:Create")
        );

        resObjectTypeItemPermission.AddChild(
            ResObjectTypeItemPermissions.Edit,
            L("Permission:Edit")
        );

        resObjectTypeItemPermission.AddChild(
            ResObjectTypeItemPermissions.Delete,
            L("Permission:Delete")
        );

        resObjectTypeItemPermission.AddChild(
            ResObjectTypeItemPermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MasterResource>(name);
    }
}
