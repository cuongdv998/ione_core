using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace iOne.Master.ResSequences;

public class GetNextSequenceInput
{
    /// <summary>
    /// Code của cấu hình sequence (ưu tiên dùng Code)
    /// </summary>
    [Display(Name = "ResSequence:Code")]
    public string? Code { get; set; }

    /// <summary>
    /// ID của cấu hình sequence (dùng nếu không có Code)
    /// </summary>
    [Display(Name = "ResSequence:Id")]
    public Guid? Id { get; set; }

    /// <summary>
    /// Dictionary chứa các parameters động để thay thế vào template
    /// Key: tên parameter (ví dụ: "customerCode", "orderId")
    /// Value: giá trị sẽ thay thế (ví dụ: "CUST001", "ORD123")
    /// Template có thể chứa ${param_name} sẽ được thay thế bằng giá trị tương ứng
    /// </summary>
    [Display(Name = "ResSequence:Parameters")]
    public Dictionary<string, string>? Parameters { get; set; }
}

