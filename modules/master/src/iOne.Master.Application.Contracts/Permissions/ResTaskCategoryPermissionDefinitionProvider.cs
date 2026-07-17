using iOne.Master.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Master.Permissions;

public class ResTaskCategoryPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var resTaskCategoryGroup = context.AddGroup(
            ResTaskCategoryPermissions.GroupName,
            L("Permission:ResTaskCategory")
        );

        var resTaskCategoryPermission = resTaskCategoryGroup.AddPermission(
            ResTaskCategoryPermissions.Default,
            L("Permission:ResTaskCategory")
        );

        resTaskCategoryPermission.AddChild(
            ResTaskCategoryPermissions.Create,
            L("Permission:Create")
        );

        resTaskCategoryPermission.AddChild(
            ResTaskCategoryPermissions.Edit,
            L("Permission:Edit")
        );

        resTaskCategoryPermission.AddChild(
            ResTaskCategoryPermissions.Delete,
            L("Permission:Delete")
        );

        resTaskCategoryPermission.AddChild(
            ResTaskCategoryPermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MasterResource>(name);
    }
}
