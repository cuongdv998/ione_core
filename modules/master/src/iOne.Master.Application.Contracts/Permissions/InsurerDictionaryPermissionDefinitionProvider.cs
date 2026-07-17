using iOne.Master.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Master.Permissions;

public class InsurerDictionaryPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var insurerDictionaryGroup = context.AddGroup(
            InsurerDictionaryPermissions.GroupName,
            L("Permission:InsurerDictionary")
        );

        var insurerDictionaryPermission = insurerDictionaryGroup.AddPermission(
            InsurerDictionaryPermissions.Default,
            L("Permission:InsurerDictionary")
        );

        insurerDictionaryPermission.AddChild(
            InsurerDictionaryPermissions.Create,
            L("Permission:Create")
        );

        insurerDictionaryPermission.AddChild(
            InsurerDictionaryPermissions.Edit,
            L("Permission:Edit")
        );

        insurerDictionaryPermission.AddChild(
            InsurerDictionaryPermissions.Delete,
            L("Permission:Delete")
        );

        insurerDictionaryPermission.AddChild(
            InsurerDictionaryPermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MasterResource>(name);
    }
}
