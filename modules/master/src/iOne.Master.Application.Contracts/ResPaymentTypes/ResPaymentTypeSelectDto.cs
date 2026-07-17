using System;

namespace iOne.Master.ResPaymentTypes;

public class ResPaymentTypeSelectDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
}
