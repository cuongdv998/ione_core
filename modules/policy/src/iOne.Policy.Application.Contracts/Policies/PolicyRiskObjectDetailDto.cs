using System;
using System.Collections.Generic;

namespace iOne.Policy.Policies;

public class PolicyRiskObjectDetailDto
{
    public Guid Id { get; set; } // policy_risk_object.id

    public Guid? ObjectTypeId { get; set; }

    public string? RepName { get; set; }
    public string? RepIdNo { get; set; }
    public string? RepPassport { get; set; }
    public string? RepPhone { get; set; }
    public string? RepEmail { get; set; }
    public Guid? RepProvinceId { get; set; }
    public Guid? RepWardId { get; set; }
    public string? RepAddress { get; set; }
    public string? RepFullAddress { get; set; }

    public Guid? RiskObjectProvinceId { get; set; }
    public Guid? RiskObjectWardId { get; set; }
    public string? RiskObjectAddress { get; set; }
    public string? RiskObjectFullAddress { get; set; }
    public double? RiskObjectLat { get; set; }
    public double? RiskObjectLong { get; set; }

    public List<PolicyRiskObjectDocumentDetailDto>? Documents { get; set; }

    public PolicyRiskMotorDetailDto? RiskObjectMotor { get; set; }
}

public class PolicyRiskObjectDocumentDetailDto
{
    public Guid DocumentId { get; set; }
}