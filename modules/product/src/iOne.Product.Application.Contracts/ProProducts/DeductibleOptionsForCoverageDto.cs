using System;
using System.Collections.Generic;

namespace iOne.Product.ProProducts;

/// <summary>
/// Deductible options for one product coverage (one row in the coverage grid).
/// </summary>
public class DeductibleOptionsForCoverageDto
{
    public Guid ProductId { get; set; }
    public Guid ProductCoverageId { get; set; }
    public Guid CoverageId { get; set; }

    /// <summary>
    /// Options for dropdown: value = fromAmount (string), label = formatted.
    /// </summary>
    public List<DeductibleOptionItemDto> Options { get; set; } = new();

    /// <summary>
    /// Term metadata keyed by fromAmount (string). Used when saving policy_coverage_level.
    /// </summary>
    public Dictionary<string, DeductibleTermMetaDto> TermByValue { get; set; } = new();
}
