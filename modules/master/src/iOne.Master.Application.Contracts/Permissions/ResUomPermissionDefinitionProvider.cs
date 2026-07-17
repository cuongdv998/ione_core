using iOne.Master.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Master.Permissions;

public class ResUomPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var resUomGroup = context.AddGroup(
            ResUomPermissions.GroupName,
            L("Permission:ResUom")
        );

        var resUomPermission = resUomGroup.AddPermission(
            ResUomPermissions.Default,
            L("Permission:ResUom")
        );

        resUomPermission.AddChild(
            ResUomPermissions.Create,
            L("Permission:Create")
        );

        resUomPermission.AddChild(
            ResUomPermissions.Edit,
            L("Permission:Edit")
        );

        resUomPermission.AddChild(
            ResUomPermissions.Delete,
            L("Permission:Delete")
        );

        resUomPermission.AddChild(
            ResUomPermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MasterResource>(name);
    }
}
