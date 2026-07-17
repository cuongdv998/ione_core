using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using iOne.ResAppChannels;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.ResEvents;

[Table("res_event_notify_template")]
public class ResEventNotifyTemplate : FullAuditedEntity<Guid>
{
    [Required]
    public virtual Guid EventId { get; private set; }

    [Required]
    public virtual Guid AppChannelId { get; private set; }

    [Required]
    public virtual int RetryNumber { get; private set; }

    [Required]
    [MaxLength(250)]
    public virtual string Title { get; private set; } = null!;

    [Required]
    [MaxLength(500)]
    public virtual string Body { get; private set; } = null!;

    [MaxLength(1000)]
    public virtual string? Data { get; private set; }

    // Navigation properties
    public virtual ResEvent? Event { get; protected set; }
    public virtual ResAppChannel? AppChannel { get; protected set; }

    protected ResEventNotifyTemplate()
    {
        // For ORM
    }

    public ResEventNotifyTemplate(
        Guid id,
        Guid eventId,
        Guid appChannelId,
        int retryNumber,
        string title,
        string body,
        string? data)
        : base(id)
    {
        SetEventId(eventId);
        SetAppChannelId(appChannelId);
        SetRetryNumber(retryNumber);
        SetTitle(title);
        SetBody(body);
        SetData(data);
    }

    private void SetEventId(Guid eventId)
    {
        if (eventId == Guid.Empty)
        {
            throw new ArgumentException("EventId cannot be empty.", nameof(eventId));
        }

        EventId = eventId;
    }

    private void SetAppChannelId(Guid appChannelId)
    {
        if (appChannelId == Guid.Empty)
        {
            throw new ArgumentException("AppChannelId cannot be empty.", nameof(appChannelId));
        }

        AppChannelId = appChannelId;
    }

    private void SetRetryNumber(int retryNumber)
    {
        if (retryNumber < 0)
        {
            throw new ArgumentException("RetryNumber cannot be negative.", nameof(retryNumber));
        }

        if (retryNumber > 99)
        {
            throw new ArgumentException("RetryNumber cannot exceed 99.", nameof(retryNumber));
        }

        RetryNumber = retryNumber;
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

    private void SetData(string? data)
    {
        if (data != null && data.Length > 1000)
        {
            throw new ArgumentException("Data cannot exceed 1000 characters.", nameof(data));
        }

        Data = data;
    }

    public virtual void UpdateAppChannelId(Guid appChannelId)
    {
        SetAppChannelId(appChannelId);
    }

    public virtual void UpdateRetryNumber(int retryNumber)
    {
        SetRetryNumber(retryNumber);
    }

    public virtual void UpdateTitle(string title)
    {
        SetTitle(title);
    }

    public virtual void UpdateBody(string body)
    {
        SetBody(body);
    }

    public virtual void UpdateData(string? data)
    {
        SetData(data);
    }
}

