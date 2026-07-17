using iOne.Master.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Master.Permissions;

public class ResMotorClassPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var resMotorClassGroup = context.AddGroup(
            ResMotorClassPermissions.GroupName,
            L("Permission:ResMotorClass")
        );

        var resMotorClassPermission = resMotorClassGroup.AddPermission(
            ResMotorClassPermissions.Default,
            L("Permission:ResMotorClass")
        );

        resMotorClassPermission.AddChild(
            ResMotorClassPermissions.Create,
            L("Permission:Create")
        );

        resMotorClassPermission.AddChild(
            ResMotorClassPermissions.Edit,
            L("Permission:Edit")
        );

        resMotorClassPermission.AddChild(
            ResMotorClassPermissions.Delete,
            L("Permission:Delete")
        );

        resMotorClassPermission.AddChild(
            ResMotorClassPermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MasterResource>(name);
    }
}

