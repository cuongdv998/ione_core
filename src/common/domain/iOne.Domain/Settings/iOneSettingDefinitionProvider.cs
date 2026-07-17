using Volo.Abp.Settings;

namespace iOne.Settings;

public class iOneSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(iOneSettings.MySetting1));
    }
}
