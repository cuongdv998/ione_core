using iOne.Master.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Master.Permissions;

public class ResPaymentTypePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var resPaymentTypeGroup = context.AddGroup(
            ResPaymentTypePermissions.GroupName,
            L("Permission:ResPaymentType")
        );

        var resPaymentTypePermission = resPaymentTypeGroup.AddPermission(
            ResPaymentTypePermissions.Default,
            L("Permission:ResPaymentType")
        );

        resPaymentTypePermission.AddChild(
            ResPaymentTypePermissions.Create,
            L("Permission:Create")
        );

        resPaymentTypePermission.AddChild(
            ResPaymentTypePermissions.Edit,
            L("Permission:Edit")
        );

        resPaymentTypePermission.AddChild(
            ResPaymentTypePermissions.Delete,
            L("Permission:Delete")
        );

        resPaymentTypePermission.AddChild(
            ResPaymentTypePermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MasterResource>(name);
    }
}
