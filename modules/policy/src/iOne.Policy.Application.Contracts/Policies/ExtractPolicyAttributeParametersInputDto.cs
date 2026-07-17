using System.Collections.Generic;

namespace iOne.Policy.Policies;

/// <summary>
/// Request DTO for extracting attribute parameter values from a Policy-like payload.
/// Matches (a subset of) the JSON shape provided by the caller.
/// </summary>
public class ExtractPolicyAttributeParametersInputDto
{
    /// <summary>
    /// Optional amount liability for scripts / attributes that reference it.
    /// </summary>
    public decimal? AmountLiability { get; set; }

    public ExtractPolicyRiskObjectForAttributeInputDto? RiskObject { get; set; }

    public ExtractPolicyProductForAttributeInputDto? Product { get; set; }
}

public class ExtractPolicyRiskObjectForAttributeInputDto
{
    public CreatePolicyRiskMotorInputDto? RiskObjectMotor { get; set; }
}

public class ExtractPolicyProductForAttributeInputDto
{
    // Keep as string to allow empty string in request samples.
    public string? ProductId { get; set; }

    public List<ExtractPolicyAttributeDefinitionInputDto>? Attributes { get; set; }
}

public class ExtractPolicyAttributeDefinitionInputDto
{
    public string? Code { get; set; }

    public string? Name { get; set; }

    /// <summary>
    /// Attribute status. Accepts either string or numeric (enum) values from clients.
    /// </summary>
    public object? Status { get; set; }

    /// <summary>
    /// Attribute spec. Accepts either string or numeric (enum) values from clients.
    /// </summary>
    public object? Spec { get; set; }

    public string? Description { get; set; }

    /// <summary>
    /// Dot-path to the value source, e.g. "riskObject.riskObjectMotor.carVin" or "carVin".
    /// </summary>
    public string? DataPath { get; set; }

    /// <summary>
    /// Attribute data type. Accepts either string or numeric (enum) values from clients.
    /// </summary>
    public object? DataType { get; set; }

    public string? ComputeScript { get; set; }

    public string? ClearDataScript { get; set; }
}

