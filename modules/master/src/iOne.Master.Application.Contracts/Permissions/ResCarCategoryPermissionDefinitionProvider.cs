using iOne.Master.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Master.Permissions;

public class ResCarCategoryPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var resCarCategoryGroup = context.AddGroup(
            ResCarCategoryPermissions.GroupName,
            L("Permission:ResCarCategory")
        );

        var resCarCategoryPermission = resCarCategoryGroup.AddPermission(
            ResCarCategoryPermissions.Default,
            L("Permission:ResCarCategory")
        );

        resCarCategoryPermission.AddChild(
            ResCarCategoryPermissions.Create,
            L("Permission:Create")
        );

        resCarCategoryPermission.AddChild(
            ResCarCategoryPermissions.Edit,
            L("Permission:Edit")
        );

        resCarCategoryPermission.AddChild(
            ResCarCategoryPermissions.Delete,
            L("Permission:Delete")
        );

        resCarCategoryPermission.AddChild(
            ResCarCategoryPermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MasterResource>(name);
    }
}


