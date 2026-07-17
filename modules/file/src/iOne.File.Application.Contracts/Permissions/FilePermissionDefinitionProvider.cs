using iOne.File.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.File.Permissions;

public class FilePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var g = context.AddGroup(FilePermissions.GroupName, L("Permission:FileModule"));
        g.AddPermission(FilePermissions.WordToPdf, L("Permission:WordToPdf"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<FileResource>(name);
    }
}
