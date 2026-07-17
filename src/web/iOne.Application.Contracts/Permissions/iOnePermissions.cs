 namespace iOne.Permissions;

public static class iOnePermissions
{
    public const string GroupName = "iOne";

    public static class AppSetting
    {
        public const string Default = GroupName + ".AppSetting";
        public const string View = Default + ".View";
        public const string Manage = Default + ".Manage";
    }

    public static class WebviewAuth
    {
        public const string Default = GroupName + ".WebviewAuth";
    }
}
