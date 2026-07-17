using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.SystemEventNotifies;

[Table("system_event_notify")]
public class SystemEventNotify : FullAuditedAggregateRoot<Guid>
{
    [Required]
    [MaxLength(50)]
    public virtual string EventCode { get; private set; } = null!;

    [Required]
    public virtual Guid AppChannelId { get; private set; }

    [Required]
    [MaxLength(250)]
    public virtual string Title { get; private set; } = null!;

    [Required]
    [MaxLength(500)]
    public virtual string Body { get; private set; } = null!;

    [MaxLength(500)]
    public virtual string? Payload { get; private set; }

    [Required]
    [MaxLength(10)]
    public virtual string RecipientType { get; private set; } = null!;

    [Required]
    public virtual Guid RecipientId { get; private set; }

    [MaxLength(50)]
    public virtual string? Recipient { get; private set; }

    [Required]
    public virtual SystemEventNotifyStatus Status { get; private set; }

    [Required]
    public virtual DateTime ScheduleAt { get; private set; }

    public virtual DateTime? SentAt { get; private set; }

    public virtual DateTime? ReadAt { get; private set; }

    [MaxLength(1000)]
    public virtual string? ErrorMessage { get; private set; }

    [Required]
    [Column("retry_number")]
    public virtual int RetryNumber { get; private set; }

    protected SystemEventNotify()
    {
        // For ORM
    }

    public SystemEventNotify(
        Guid id,
        string eventCode,
        Guid appChannelId,
        string title,
        string body,
        string? payload,
        string recipientType,
        Guid recipientId,
        string? recipient,
        SystemEventNotifyStatus status,
        DateTime scheduleAt,
        DateTime? sentAt,
        DateTime? readAt,
        string? errorMessage,
        int retryNumber = 0)
        : base(id)
    {
        SetEventCode(eventCode);
        SetAppChannelId(appChannelId);
        SetTitle(title);
        SetBody(body);
        SetPayload(payload);
        SetRecipientType(recipientType);
        SetRecipientId(recipientId);
        SetRecipient(recipient);
        SetStatus(status);
        SetScheduleAt(scheduleAt);
        SetSentAt(sentAt);
        SetReadAt(readAt);
        SetErrorMessage(errorMessage);
        SetRetryNumber(retryNumber);
    }

    private void SetEventCode(string eventCode)
    {
        if (string.IsNullOrWhiteSpace(eventCode))
        {
            throw new ArgumentException("EventCode cannot be null or empty.", nameof(eventCode));
        }

        if (eventCode.Length > 50)
        {
            throw new ArgumentException("EventCode cannot exceed 50 characters.", nameof(eventCode));
        }

        EventCode = eventCode;
    }

    private void SetAppChannelId(Guid appChannelId)
    {
        AppChannelId = appChannelId;
    }

    private void SetTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title cannot be null or empty.", nameof(title));
        }

        if (title.Length > 250)
        {
            throw new ArgumentException("Title cannot exceed 250 characters.", nameof(title));
        }

        Title = title;
    }

    private void SetBody(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            throw new ArgumentException("Body cannot be null or empty.", nameof(body));
        }

        if (body.Length > 500)
        {
            throw new ArgumentException("Body cannot exceed 500 characters.", nameof(body));
        }

        Body = body;
    }

    private void SetPayload(string? payload)
    {
        if (payload != null && payload.Length > 500)
        {
            throw new ArgumentException("Payload cannot exceed 500 characters.", nameof(payload));
        }

        Payload = payload;
    }

    private void SetRecipientType(string recipientType)
    {
        if (string.IsNullOrWhiteSpace(recipientType))
        {
            throw new ArgumentException("RecipientType cannot be null or empty.", nameof(recipientType));
        }

        if (recipientType.Length > 10)
        {
            throw new ArgumentException("RecipientType cannot exceed 10 characters.", nameof(recipientType));
        }

        RecipientType = recipientType;
    }

    private void SetRecipientId(Guid recipientId)
    {
        RecipientId = recipientId;
    }

    private void SetRecipient(string? recipient)
    {
        if (recipient != null && recipient.Length > 500)
        {
            throw new ArgumentException("Recipient cannot exceed 50 characters.", nameof(recipient));
        }

        Recipient = recipient;
    }

    private void SetStatus(SystemEventNotifyStatus status)
    {
        Status = status;
    }

    private void SetScheduleAt(DateTime scheduleAt)
    {
        ScheduleAt = scheduleAt;
    }

    private void SetSentAt(DateTime? sentAt)
    {
        SentAt = sentAt;
    }

    private void SetReadAt(DateTime? readAt)
    {
        ReadAt = readAt;
    }

    private void SetErrorMessage(string? errorMessage)
    {
        if (errorMessage != null && errorMessage.Length > 1000)
        {
            throw new ArgumentException("ErrorMessage cannot exceed 1000 characters.", nameof(errorMessage));
        }

        ErrorMessage = errorMessage;
    }

    private void SetRetryNumber(int retryNumber)
    {
        if (retryNumber < 0)
        {
            throw new ArgumentException("RetryNumber cannot be negative.", nameof(retryNumber));
        }

        RetryNumber = retryNumber;
    }

    /// <summary>
    /// Chỉ cho phép sửa trạng thái (status).
    /// </summary>
    public virtual void UpdateStatus(SystemEventNotifyStatus status)
    {
        SetStatus(status);
    }

    /// <summary>
    /// Cập nhật thời gian đọc.
    /// </summary>
    public virtual void UpdateReadAt(DateTime? readAt)
    {
        SetReadAt(readAt);
    }

    /// <summary>
    /// Tăng số lần retry.
    /// </summary>
    public virtual void IncrementRetryNumber()
    {
        RetryNumber++;
    }

    /// <summary>
    /// Cập nhật số lần retry.
    /// </summary>
    public virtual void UpdateRetryNumber(int retryNumber)
    {
        SetRetryNumber(retryNumber);
    }

    /// <summary>
    /// Đánh dấu đã gửi thành công: status = Sent, retry_number + 1, sent_at = now
    /// </summary>
    public virtual void MarkAsSent()
    {
        SetStatus(SystemEventNotifyStatus.Sent);
        SetSentAt(DateTime.Now);
        IncrementRetryNumber();
        SetErrorMessage(null);
    }

    /// <summary>
    /// Đánh dấu gửi thất bại: status = Fail, retry_number + 1, error_message = errorMessage
    /// </summary>
    public virtual void MarkAsFailed(string? errorMessage)
    {
        SetStatus(SystemEventNotifyStatus.Fail);
        IncrementRetryNumber();
        SetErrorMessage(errorMessage);
    }
}
