using iOne.Master.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Master.Permissions;

public class ResDocumentTypePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var resDocumentTypeGroup = context.AddGroup(
            ResDocumentTypePermissions.GroupName,
            L("Permission:ResDocumentType")
        );

        var resDocumentTypePermission = resDocumentTypeGroup.AddPermission(
            ResDocumentTypePermissions.Default,
            L("Permission:ResDocumentType")
        );

        resDocumentTypePermission.AddChild(
            ResDocumentTypePermissions.Create,
            L("Permission:Create")
        );

        resDocumentTypePermission.AddChild(
            ResDocumentTypePermissions.Edit,
            L("Permission:Edit")
        );

        resDocumentTypePermission.AddChild(
            ResDocumentTypePermissions.Delete,
            L("Permission:Delete")
        );

        resDocumentTypePermission.AddChild(
            ResDocumentTypePermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MasterResource>(name);
    }
}

