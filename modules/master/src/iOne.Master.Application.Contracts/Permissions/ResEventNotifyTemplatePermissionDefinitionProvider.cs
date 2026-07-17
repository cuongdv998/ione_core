using iOne.Master.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Master.Permissions;

public class ResEventNotifyTemplatePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var resEventNotifyTemplateGroup = context.AddGroup(
            ResEventNotifyTemplatePermissions.GroupName,
            L("Permission:ResEventNotifyTemplate")
        );

        var resEventNotifyTemplatePermission = resEventNotifyTemplateGroup.AddPermission(
            ResEventNotifyTemplatePermissions.Default,
            L("Permission:ResEventNotifyTemplate")
        );

        resEventNotifyTemplatePermission.AddChild(
            ResEventNotifyTemplatePermissions.Create,
            L("Permission:Create")
        );

        resEventNotifyTemplatePermission.AddChild(
            ResEventNotifyTemplatePermissions.Edit,
            L("Permission:Edit")
        );

        resEventNotifyTemplatePermission.AddChild(
            ResEventNotifyTemplatePermissions.Delete,
            L("Permission:Delete")
        );

        resEventNotifyTemplatePermission.AddChild(
            ResEventNotifyTemplatePermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MasterResource>(name);
    }
}

