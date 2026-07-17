using System;
using System.Collections.Generic;

namespace iOne.Product.ProProducts;

/// <summary>
/// Input for loading deductible (mức miễn thường) options per product coverage.
/// </summary>
public class GetDeductibleOptionsInput
{
    /// <summary>
    /// Product IDs to load deductible options for (typically selected products).
    /// </summary>
    public List<Guid> ProductIds { get; set; } = new();

    /// <summary>
    /// Optional issue date used to filter effective coverage levels (EffectDate &lt;= issueDate &lt;= ExpireDate).
    /// If null, current date is used.
    /// </summary>
    public DateTime? IssueDate { get; set; }

    /// <summary>
    /// Nhóm xe (car group) - used by condition_script when evaluating deductible options.
    /// </summary>
    public string? CarGroup { get; set; }

    /// <summary>
    /// Loại xe (car type) - used by condition_script when evaluating deductible options.
    /// </summary>
    public string? CarType { get; set; }

    /// <summary>
    /// Số chỗ ngồi (seating capacity) - used by condition_script when evaluating deductible options.
    /// </summary>
    public int? SeatingCapacity { get; set; }

    /// <summary>
    /// Trọng tải (weight, tons) - used by condition_script when evaluating deductible options.
    /// </summary>
    public decimal? Weight { get; set; }

    /// <summary>
    /// Mục đích kinh doanh (car purpose) - e.g. KDVT, KKDVT. Used by condition_script when evaluating deductible options.
    /// </summary>
    public string? CarPurpose { get; set; }
}
