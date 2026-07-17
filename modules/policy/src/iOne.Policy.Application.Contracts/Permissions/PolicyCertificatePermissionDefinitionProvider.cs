using iOne.Policy.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Policy.Permissions;

public class PolicyCertificatePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var policyCertificateGroup = context.AddGroup(
            PolicyCertificatePermissions.GroupName,
            L("Permission:PolicyCertificate")
        );

        var policyCertificatePermission = policyCertificateGroup.AddPermission(
            PolicyCertificatePermissions.Default,
            L("Permission:PolicyCertificate")
        );

        policyCertificatePermission.AddChild(
            PolicyCertificatePermissions.Create,
            L("Permission:Create")
        );

        policyCertificatePermission.AddChild(
            PolicyCertificatePermissions.Edit,
            L("Permission:Edit")
        );

        policyCertificatePermission.AddChild(
            PolicyCertificatePermissions.Delete,
            L("Permission:Delete")
        );

        policyCertificatePermission.AddChild(
            PolicyCertificatePermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<PolicyResource>(name);
    }
}
