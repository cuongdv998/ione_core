using iOne.Master.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Master.Permissions;

public class ResDocumentPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var resDocumentGroup = context.AddGroup(
            ResDocumentPermissions.GroupName,
            L("Permission:ResDocument")
        );

        var resDocumentPermission = resDocumentGroup.AddPermission(
            ResDocumentPermissions.Default,
            L("Permission:ResDocument")
        );

        resDocumentPermission.AddChild(
            ResDocumentPermissions.Create,
            L("Permission:Create")
        );

        resDocumentPermission.AddChild(
            ResDocumentPermissions.Edit,
            L("Permission:Edit")
        );

        resDocumentPermission.AddChild(
            ResDocumentPermissions.Delete,
            L("Permission:Delete")
        );

        resDocumentPermission.AddChild(
            ResDocumentPermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MasterResource>(name);
    }
}
