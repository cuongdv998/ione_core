using System;

namespace iOne.Master.ResPaymentMethods;

public class ResPaymentMethodSelectDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
}
