using System;
using System.ComponentModel.DataAnnotations;

namespace iOne.Master.ResBusinessAssignees;

/// <summary>
/// Khi sửa cho phép thay đổi AssigneeId (nhân viên duyệt) và ExpireDate (ngày hết hạn).
/// </summary>
public class UpdateResBusinessAssigneeDto
{
    [Display(Name = "ResBusinessAssignee:AssigneeId")]
    public Guid? AssigneeId { get; set; }

    [Display(Name = "ResBusinessAssignee:ExpireDate")]
    public DateTime? ExpireDate { get; set; }
}
