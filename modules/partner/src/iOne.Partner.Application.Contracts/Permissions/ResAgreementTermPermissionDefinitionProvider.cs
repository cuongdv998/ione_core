using iOne.Partner.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Partner.Permissions;

public class ResAgreementTermPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var resAgreementTermGroup = context.AddGroup(
            ResAgreementTermPermissions.GroupName,
            L("Permission:ResAgreementTerm")
        );

        var resAgreementTermPermission = resAgreementTermGroup.AddPermission(
            ResAgreementTermPermissions.Default,
            L("Permission:ResAgreementTerm")
        );

        resAgreementTermPermission.AddChild(
            ResAgreementTermPermissions.Create,
            L("Permission:Create")
        );

        resAgreementTermPermission.AddChild(
            ResAgreementTermPermissions.Edit,
            L("Permission:Edit")
        );

        resAgreementTermPermission.AddChild(
            ResAgreementTermPermissions.Delete,
            L("Permission:Delete")
        );

        resAgreementTermPermission.AddChild(
            ResAgreementTermPermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<PartnerResource>(name);
    }
}

