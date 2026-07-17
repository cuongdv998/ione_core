using iOne.Master.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Master.Permissions;

public class ResPaymentMethodPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var resPaymentMethodGroup = context.AddGroup(
            ResPaymentMethodPermissions.GroupName,
            L("Permission:ResPaymentMethod")
        );

        var resPaymentMethodPermission = resPaymentMethodGroup.AddPermission(
            ResPaymentMethodPermissions.Default,
            L("Permission:ResPaymentMethod")
        );

        resPaymentMethodPermission.AddChild(
            ResPaymentMethodPermissions.Create,
            L("Permission:Create")
        );

        resPaymentMethodPermission.AddChild(
            ResPaymentMethodPermissions.Edit,
            L("Permission:Edit")
        );

        resPaymentMethodPermission.AddChild(
            ResPaymentMethodPermissions.Delete,
            L("Permission:Delete")
        );

        resPaymentMethodPermission.AddChild(
            ResPaymentMethodPermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MasterResource>(name);
    }
}
