using System.Threading.Tasks;
using iOne.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Emailing;
using Volo.Abp.SettingManagement;
using Volo.Abp.Settings;

namespace iOne.Setting;

public class AppSettingAppService : iOne.iOneAppService
{
    private readonly ISettingManager _settingManager;
    private readonly ISettingProvider _settingProvider;
    private readonly IEmailSender _emailSender;

    public AppSettingAppService(
        ISettingManager settingManager,
        ISettingProvider settingProvider,
        IEmailSender emailSender)
    {
        _settingManager = settingManager;
        _settingProvider = settingProvider;
        _emailSender = emailSender;
    }

    [Authorize(iOnePermissions.AppSetting.View)]
    public async Task<string?> GetAsync(string name)
    {
        return await _settingProvider.GetOrNullAsync(name);
    }

    [Authorize(iOnePermissions.AppSetting.Manage)]
    public async Task SetGlobalAsync(string name, string value)
    {
        await _settingManager.SetGlobalAsync(name, value);
    }

    [Authorize(iOnePermissions.AppSetting.Manage)]
    public async Task SendTestEmailAsync(SendTestEmailInput input)
    {
        await _emailSender.SendAsync(
            input.To.Trim(),
            L["SystemSettings:TestEmailSubject"],
            L["SystemSettings:TestEmailBody"]
        );
    }
}