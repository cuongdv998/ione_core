using iOne.Report.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Report.Permissions
{
    public class ReportTemplateParamerterDefinitionProvider : PermissionDefinitionProvider
    {
        public override void Define(IPermissionDefinitionContext context)
        {
            //var reportTemplateParameterGroup = context.AddGroup(
            //    ReportTemplateParameterPermissions.GroupName,
            //    L("Permission:ReportTemplateParameter")
            //);

            //var reportTemplateParameterPermission = reportTemplateParameterGroup.AddPermission(
            //    ReportTemplateParameterPermissions.Default,
            //    L("Permission:ReportTemplateParameter")
            //);

            //reportTemplateParameterPermission.AddChild(
            //    ReportTemplateParameterPermissions.Create,
            //    L("Permission:Create")
            //);

            //reportTemplateParameterPermission.AddChild(
            //    ReportTemplateParameterPermissions.Edit,
            //    L("Permission:Edit")
            //);

            //reportTemplateParameterPermission.AddChild(
            //    ReportTemplateParameterPermissions.Delete,
            //    L("Permission:Delete")
            //);

            //reportTemplateParameterPermission.AddChild(
            //    ReportTemplateParameterPermissions.View,
            //    L("Permission:View")
            //);
        }

        private static LocalizableString L(string name)
        {
            return LocalizableString.Create<ReportResource>(name);
        }
    }
}
