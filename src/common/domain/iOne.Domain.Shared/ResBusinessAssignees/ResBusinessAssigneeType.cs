namespace iOne.ResBusinessAssignees;

/// <summary>
/// Phân loại xác định người thực hiện: emp - đích danh; role - theo vai trò; system - hệ thống tự động
/// </summary>
public enum ResBusinessAssigneeType
{
    Emp = 0,    // emp - xác định đích danh người thực hiện
    Role = 1,   // role - xác định người thực hiện theo vai trò
    System = 2  // system - hệ thống tự động thực hiện
}
