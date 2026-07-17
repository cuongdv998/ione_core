using System;
using System.ComponentModel.DataAnnotations;

namespace iOne.Master.AdminConfigs;

/// <summary>
/// DTO cho dropdown AdminConfig (theo code, ví dụ PARTY_IN_RELATIONSHIP, DRIVER_LICENSE_LEVEL).
/// </summary>
public class AdminConfigSelectDto
{
    public Guid Id { get; set; }

    public string SubCode { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Value { get; set; } = null!;
}
