using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.ResPartnerMessageLoggings;

[Table("res_partner_message_logging")]
public class ResPartnerMessageLogging : FullAuditedAggregateRoot<Guid>
{
    [Required]
    [MaxLength(50)]
    public virtual string PartnerCode { get; private set; } = null!;

    public virtual Guid? PolicyId { get; private set; }

    [Required]
    [MaxLength(500)]
    public virtual string ApiUrl { get; private set; } = null!;

    [Required]
    [MaxLength(10)]
    public virtual string HttpMethod { get; private set; } = null!;

    public virtual string? RequestBody { get; private set; }

    public virtual string? ResponseBody { get; private set; }

    public virtual int? HttpStatusCode { get; private set; }

    [Required]
    public virtual bool IsSuccess { get; private set; }

    [MaxLength(2000)]
    public virtual string? ErrorMessage { get; private set; }

    public virtual long? DurationMs { get; private set; }

    protected ResPartnerMessageLogging()
    {
    }

    public ResPartnerMessageLogging(
        Guid id,
        string partnerCode,
        Guid? policyId,
        string apiUrl,
        string httpMethod,
        string? requestBody,
        string? responseBody,
        int? httpStatusCode,
        bool isSuccess,
        string? errorMessage,
        long? durationMs)
        : base(id)
    {
        PartnerCode = partnerCode;
        PolicyId = policyId;
        ApiUrl = apiUrl;
        HttpMethod = httpMethod;
        RequestBody = requestBody;
        ResponseBody = responseBody;
        HttpStatusCode = httpStatusCode;
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        DurationMs = durationMs;
    }
}
