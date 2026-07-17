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
    public
        class ReportTemplateSqlPermissionDefinitionProvider : PermissionDefinitionProvider
    {
        public override void Define(IPermissionDefinitionContext context)
        {
            //var reportTemplateSqlGroup = context.AddGroup(
            //    ReportTemplateSqlPermissions.GroupName,
            //    L("Permission:ReportTemplateSql")
            //);

            //var reportTemplateSqlPermission = reportTemplateSqlGroup.AddPermission(
            //    ReportTemplateSqlPermissions.Default,
            //    L("Permission:ReportTemplateSql")
            //);

            //reportTemplateSqlPermission.AddChild(
            //    ReportTemplateSqlPermissions.Create,
            //    L("Permission:Create")
            //);

            //reportTemplateSqlPermission.AddChild(
            //    ReportTemplateSqlPermissions.Edit,
            //    L("Permission:Edit")
            //);

            //reportTemplateSqlPermission.AddChild(
            //    ReportTemplateSqlPermissions.Delete,
            //    L("Permission:Delete")
            //);

            //reportTemplateSqlPermission.AddChild(
            //    ReportTemplateSqlPermissions.View,
            //    L("Permission:View")
            //);
        }

        private static LocalizableString L(string name)
        {
            return LocalizableString.Create<ReportResource>(name);
        }
    }
}
