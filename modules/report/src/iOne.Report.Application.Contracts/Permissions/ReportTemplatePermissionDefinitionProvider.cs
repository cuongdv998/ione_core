using iOne.Report.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Report.Permissions
{
    public class ReportTemplatePermissionDefinitionProvider : PermissionDefinitionProvider
    {
        public override void Define(IPermissionDefinitionContext context)
        {
            var reportTemplateGroup = context.AddGroup(
                ReportTemplatePermissions.GroupName,
                L("Permission:ReportTemplate")
            );

            var reportTemplatePermission = reportTemplateGroup.AddPermission(
                ReportTemplatePermissions.Default,
                L("Permission:ReportTemplate")
            );

            reportTemplatePermission.AddChild(
                ReportTemplatePermissions.Create,
                L("Permission:Create")
            );

            reportTemplatePermission.AddChild(
                ReportTemplatePermissions.Edit,
                L("Permission:Edit")
            );

            reportTemplatePermission.AddChild(
                ReportTemplatePermissions.Delete,
                L("Permission:Delete")
            );

            reportTemplatePermission.AddChild(
                ReportTemplatePermissions.View,
                L("Permission:View")
            );
        }

        private static LocalizableString L(string name)
        {
            return LocalizableString.Create<ReportResource>(name);
        }
    }
}
