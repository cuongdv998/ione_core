using iOne.Report.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Report.Permissions
{
    public class CertificatePermissionDefinitionProvider : PermissionDefinitionProvider
    {
        public override void Define(IPermissionDefinitionContext context)
        {
            var certificateGroup = context.AddGroup(
                CertificatePermissions.GroupName,
                L("Permission:Certificate")
            );

            var certificatePermission = certificateGroup.AddPermission(
                CertificatePermissions.Default,
                L("Permission:Certificate")
            );

            certificatePermission.AddChild(
                CertificatePermissions.Generate,
                L("Permission:Generate")
            );
        }

        private static LocalizableString L(string name)
        {
            return LocalizableString.Create<ReportResource>(name);
        }
    }
}
