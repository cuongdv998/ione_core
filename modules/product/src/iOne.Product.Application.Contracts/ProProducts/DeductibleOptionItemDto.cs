namespace iOne.Product.ProProducts;

/// <summary>
/// Single option for deductible dropdown (value = fromAmount string, label = formatted).
/// </summary>
public class DeductibleOptionItemDto
{
    public string Value { get; set; } = null!;
    public string Label { get; set; } = null!;

    /// <summary>
    /// Kết quả eval condition_script cho mức miễn thường này (true/false). Có script thì mới eval; không có script = true.
    /// </summary>
    public bool ConditionEvalResult { get; set; }
}
