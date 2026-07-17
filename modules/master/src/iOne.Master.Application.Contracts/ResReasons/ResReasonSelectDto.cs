using System;
using System.ComponentModel.DataAnnotations;

namespace iOne.Master.ResReasons;

public class ResReasonSelectDto
{
    public Guid Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;
}
