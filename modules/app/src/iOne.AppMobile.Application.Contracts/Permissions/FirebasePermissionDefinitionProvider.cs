using iOne.AppMobile.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.AppMobile.Permissions;

public class FirebasePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        //var firebaseGroup = context.AddGroup(
        //    FirebasePermissions.GroupName,
        //    L("Permission:Firebase")
        //);

        //var firebasePermission = firebaseGroup.AddPermission(
        //    FirebasePermissions.Default,
        //    L("Permission:Firebase")
        //);

        //firebasePermission.AddChild(
        //    FirebasePermissions.SendNotification,
        //    L("Permission:Firebase:SendNotification")
        //);

        //firebasePermission.AddChild(
        //    FirebasePermissions.SendToMultiple,
        //    L("Permission:Firebase:SendToMultiple")
        //);

        //firebasePermission.AddChild(
        //    FirebasePermissions.SendToTopic,
        //    L("Permission:Firebase:SendToTopic")
        //);

        //firebasePermission.AddChild(
        //    FirebasePermissions.SubscribeToTopic,
        //    L("Permission:Firebase:SubscribeToTopic")
        //);

        //firebasePermission.AddChild(
        //    FirebasePermissions.UnsubscribeFromTopic,
        //    L("Permission:Firebase:UnsubscribeFromTopic")
        //);
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<AppMobileResource>(name);
    }
}
