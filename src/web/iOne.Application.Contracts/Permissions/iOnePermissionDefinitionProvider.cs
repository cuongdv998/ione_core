using iOne.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace iOne.Permissions;

public class iOnePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(iOnePermissions.GroupName);

        var appSetting = myGroup.AddPermission(iOnePermissions.AppSetting.Default, L("Permission:AppSetting"));
        appSetting.AddChild(iOnePermissions.AppSetting.View, L("Permission:AppSetting.View"));
        appSetting.AddChild(iOnePermissions.AppSetting.Manage, L("Permission:AppSetting.Manage"));

        var webviewAuth = myGroup.AddPermission(iOnePermissions.WebviewAuth.Default, L("Permission:WebviewAuth"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<iOneResource>(name);
    }
}
