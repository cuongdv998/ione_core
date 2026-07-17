using iOne.Master.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Master.Permissions;

public class ResSequencePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var resSequenceGroup = context.AddGroup(
            ResSequencePermissions.GroupName,
            L("Permission:ResSequence")
        );

        var resSequencePermission = resSequenceGroup.AddPermission(
            ResSequencePermissions.Default,
            L("Permission:ResSequence")
        );

        resSequencePermission.AddChild(
            ResSequencePermissions.Create,
            L("Permission:Create")
        );

        resSequencePermission.AddChild(
            ResSequencePermissions.Edit,
            L("Permission:Edit")
        );

        resSequencePermission.AddChild(
            ResSequencePermissions.Delete,
            L("Permission:Delete")
        );

        resSequencePermission.AddChild(
            ResSequencePermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MasterResource>(name);
    }
}

