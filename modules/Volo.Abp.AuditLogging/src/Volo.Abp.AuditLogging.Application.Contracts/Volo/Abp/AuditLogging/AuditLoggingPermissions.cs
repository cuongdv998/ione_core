using Volo.Abp.Reflection;

namespace Volo.Abp.AuditLogging;

public static class AuditLoggingPermissions
{
    public const string GroupName = "AbpAuditLogging";

    public static class AuditLogs
    {
        public const string Default = GroupName + ".AuditLogs";
        public const string View = Default + ".View";
        public const string ViewDetails = Default + ".ViewDetails";
        public const string ViewEntityChanges = Default + ".ViewEntityChanges";
    }

    public static string[] GetAll()
    {
        return ReflectionHelper.GetPublicConstantsRecursively(typeof(AuditLoggingPermissions));
    }
}

