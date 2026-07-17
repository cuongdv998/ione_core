using Volo.Abp.Authorization.Permissions;
using Volo.Abp.AuditLogging.Localization;
using Volo.Abp.Localization;

namespace Volo.Abp.AuditLogging;

public class AuditLoggingPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var auditLoggingGroup = context.AddGroup(
            AuditLoggingPermissions.GroupName,
            L("Permission:AuditLogging")
        );

        var auditLogsPermission = auditLoggingGroup.AddPermission(
            AuditLoggingPermissions.AuditLogs.Default,
            L("Permission:AuditLogs")
        );
        
        auditLogsPermission.AddChild(
            AuditLoggingPermissions.AuditLogs.View,
            L("Permission:View")
        );
        
        auditLogsPermission.AddChild(
            AuditLoggingPermissions.AuditLogs.ViewDetails,
            L("Permission:ViewDetails")
        );
        
        auditLogsPermission.AddChild(
            AuditLoggingPermissions.AuditLogs.ViewEntityChanges,
            L("Permission:ViewEntityChanges")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<AuditLoggingResource>(name);
    }
}

