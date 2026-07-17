using System;
using System.Collections.Generic;

namespace iOne.Payment.Payments;

public class MotorbikePaymentInquiryResultDto
{
    public int CandidateCount { get; set; }

    public int PtiInquiryCount { get; set; }

    public int SkippedNonPtiCount { get; set; }

    public int SuccessCount { get; set; }

    public int FailedCount { get; set; }

    public List<Guid> CandidatePolicyIds { get; set; } = new();

    public List<Guid> InquiredPolicyIds { get; set; } = new();
}
