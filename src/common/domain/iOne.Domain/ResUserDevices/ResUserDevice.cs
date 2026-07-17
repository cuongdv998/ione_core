using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.ResUserDevices;

[Table("res_user_device")]
public class ResUserDevice : FullAuditedAggregateRoot<Guid>
{
    [Required]
    [MaxLength(50)]
    public virtual string UserName { get; private set; } = null!;

    [Required]
    [MaxLength(250)]
    public virtual string DeviceUid { get; private set; } = null!;

    [Required]
    [MaxLength(500)]
    public virtual string DeviceToken { get; private set; } = null!;

    [Required]
    [MaxLength(50)]
    public virtual string AppChannelCode { get; private set; } = null!;

    [Required]
    public virtual DateTime EffectDate { get; private set; }

    public virtual DateTime? ExpirDate { get; private set; }

    [Required]
    public virtual ResUserDeviceStatus Status { get; private set; }

    [MaxLength(50)]
    public virtual string? Os { get; private set; }

    [MaxLength(250)]
    public virtual string? DeviceName { get; private set; }

    protected ResUserDevice()
    {
        // For ORM
    }

    public ResUserDevice(
        Guid id,
        string userName,
        string deviceUid,
        string deviceToken,
        string appChannelCode,
        DateTime effectDate,
        DateTime? expirDate,
        ResUserDeviceStatus status,
        string? os,
        string? deviceName)
        : base(id)
    {
        SetUserName(userName);
        SetDeviceUid(deviceUid);
        SetDeviceToken(deviceToken);
        SetAppChannelCode(appChannelCode);
        SetEffectDate(effectDate);
        SetExpirDate(expirDate);
        SetStatus(status);
        SetOs(os);
        SetDeviceName(deviceName);
    }

    private void SetUserName(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            throw new ArgumentException("UserName cannot be null or empty.", nameof(userName));
        }

        if (userName.Length > 50)
        {
            throw new ArgumentException("UserName cannot exceed 50 characters.", nameof(userName));
        }

        UserName = userName;
    }

    private void SetDeviceUid(string deviceUid)
    {
        if (string.IsNullOrWhiteSpace(deviceUid))
        {
            throw new ArgumentException("DeviceUid cannot be null or empty.", nameof(deviceUid));
        }

        if (deviceUid.Length > 250)
        {
            throw new ArgumentException("DeviceUid cannot exceed 250 characters.", nameof(deviceUid));
        }

        DeviceUid = deviceUid;
    }

    private void SetDeviceToken(string deviceToken)
    {
        if (string.IsNullOrWhiteSpace(deviceToken))
        {
            throw new ArgumentException("DeviceToken cannot be null or empty.", nameof(deviceToken));
        }

        if (deviceToken.Length > 500)
        {
            throw new ArgumentException("DeviceToken cannot exceed 500 characters.", nameof(deviceToken));
        }

        DeviceToken = deviceToken;
    }

    private void SetAppChannelCode(string appChannelCode)
    {
        if (string.IsNullOrWhiteSpace(appChannelCode))
        {
            throw new ArgumentException("AppChannelCode cannot be null or empty.", nameof(appChannelCode));
        }

        if (appChannelCode.Length > 50)
        {
            throw new ArgumentException("AppChannelCode cannot exceed 50 characters.", nameof(appChannelCode));
        }

        AppChannelCode = appChannelCode;
    }

    private void SetEffectDate(DateTime effectDate)
    {
        EffectDate = effectDate.Date; // Only date part
    }

    private void SetExpirDate(DateTime? expirDate)
    {
        ExpirDate = expirDate?.Date; // Only date part
    }

    private void SetStatus(ResUserDeviceStatus status)
    {
        Status = status;
    }

    private void SetOs(string? os)
    {
        if (os != null && os.Length > 50)
        {
            throw new ArgumentException("Os cannot exceed 50 characters.", nameof(os));
        }

        Os = os;
    }

    private void SetDeviceName(string? deviceName)
    {
        if (deviceName != null && deviceName.Length > 250)
        {
            throw new ArgumentException("DeviceName cannot exceed 250 characters.", nameof(deviceName));
        }

        DeviceName = deviceName;
    }

    // ⚠️ QUAN TRỌNG: Không có method update cho UserName, DeviceUid, AppChannelCode, EffectDate
    // Các trường này không được phép sửa sau khi tạo

    public virtual void UpdateDeviceToken(string deviceToken)
    {
        SetDeviceToken(deviceToken);
    }

    public virtual void UpdateExpirDate(DateTime? expirDate)
    {
        SetExpirDate(expirDate);
    }

    public virtual void UpdateStatus(ResUserDeviceStatus status)
    {
        SetStatus(status);
    }

    public virtual void UpdateOs(string? os)
    {
        SetOs(os);
    }

    public virtual void UpdateDeviceName(string? deviceName)
    {
        SetDeviceName(deviceName);
    }
}
