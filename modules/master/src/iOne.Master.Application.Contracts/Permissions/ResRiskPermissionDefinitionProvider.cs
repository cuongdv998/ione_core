using iOne.Master.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Master.Permissions;

public class ResRiskPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var resRiskGroup = context.AddGroup(
            ResRiskPermissions.GroupName,
            L("Permission:ResRisk")
        );

        var resRiskPermission = resRiskGroup.AddPermission(
            ResRiskPermissions.Default,
            L("Permission:ResRisk")
        );

        resRiskPermission.AddChild(
            ResRiskPermissions.Create,
            L("Permission:Create")
        );

        resRiskPermission.AddChild(
            ResRiskPermissions.Edit,
            L("Permission:Edit")
        );

        resRiskPermission.AddChild(
            ResRiskPermissions.Delete,
            L("Permission:Delete")
        );

        resRiskPermission.AddChild(
            ResRiskPermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MasterResource>(name);
    }
}

