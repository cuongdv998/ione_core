namespace iOne.AppMobile.Permissions;

public static class FirebasePermissions
{
    public const string GroupName = "AppMobileFirebase";

    public const string Default = GroupName;
    public const string SendNotification = Default + ".SendNotification";
    public const string SendToMultiple = Default + ".SendToMultiple";
    public const string SendToTopic = Default + ".SendToTopic";
    public const string SubscribeToTopic = Default + ".SubscribeToTopic";
    public const string UnsubscribeFromTopic = Default + ".UnsubscribeFromTopic";
}
