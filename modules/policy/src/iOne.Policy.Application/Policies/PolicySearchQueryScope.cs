using System;
using System.Collections.Generic;
using System.Linq;
using iOne.Policies;

namespace iOne.Policy.Policies;

/// <summary>
/// Thêm một <c>Where</c> lên query tìm kiếm đơn (AND với filter khác).
/// Nhánh đối tác (viewer có <see cref="iOne.HrEmployees.HrEmployee.PartnerId"/>): so <c>viewer.PartnerId</c> với
/// <see cref="iOne.HrEmployees.HrEmployee.PartnerId"/> của <see cref="iOne.Policies.Policy.Implementer"/> (người khai thác),
/// không dùng <see cref="iOne.Policies.Policy.PartnerId"/> trên đơn vì trường đó là đối tác BH gốc (insurer) trên luồng lưu hiện tại.
/// Không phải quản lý: cùng đối tác kênh của implementer và (Seller hoặc Implementer) là chính mình;
/// quản lý: mọi đơn mà implementer thuộc cùng đối tác kênh.
/// Nhân viên nội bộ (<c>PartnerId</c> null): Seller/Implementer là mình, đồng phòng ban, hoặc (quản lý + cây phòng ban) theo <c>DepartmentId</c>.
/// Không so khớp <c>OrgId</c> trên query (chỉ dùng phòng ban / cây phòng ban).
/// </summary>
public static class PolicySearchQueryScope
{
    /// <summary>
    /// Bản <see cref="Policy"/>.
    /// </summary>
    public static IQueryable<iOne.Policies.Policy> ApplyEmployeeUnitSearchFilter(
        IQueryable<iOne.Policies.Policy> query,
        Guid viewerEmployeeId,
        Guid? viewerPartnerId,
        Guid viewerDepartmentId,
        bool viewerIsManager,
        IReadOnlyList<Guid>? managedSubtreeDepartmentIds)
    {
        if (viewerPartnerId.HasValue)
        {
            var partnerId = viewerPartnerId.Value;
            if (viewerIsManager)
            {
                return query.Where(p =>
                    p.Implementer != null
                    && p.Implementer.PartnerId == partnerId);
            }

            return query.Where(p =>
                p.Implementer != null
                && p.Implementer.PartnerId == partnerId
                && (p.SellerId == viewerEmployeeId || p.ImplementerId == viewerEmployeeId));
        }

        if (viewerIsManager && managedSubtreeDepartmentIds is { Count: > 0 })
        {
            var ids = managedSubtreeDepartmentIds;
            return query.Where(p =>
                p.SellerId == viewerEmployeeId || p.ImplementerId == viewerEmployeeId
                || (p.Seller != null && ids.Contains(p.Seller.DepartmentId))
                || (p.Implementer != null && ids.Contains(p.Implementer.DepartmentId)));
        }

        var deptId = viewerDepartmentId;
        return query.Where(p =>
            p.SellerId == viewerEmployeeId || p.ImplementerId == viewerEmployeeId
            || (p.Seller != null && p.Seller.DepartmentId == deptId)
            || (p.Implementer != null && p.Implementer.DepartmentId == deptId));
    }

    /// <summary>
    /// Bản list theo <see cref="PolicyVersion"/>.
    /// </summary>
    public static IQueryable<PolicyVersion> ApplyEmployeeUnitSearchFilter(
        IQueryable<PolicyVersion> query,
        Guid viewerEmployeeId,
        Guid? viewerPartnerId,
        Guid viewerDepartmentId,
        bool viewerIsManager,
        IReadOnlyList<Guid>? managedSubtreeDepartmentIds)
    {
        query = query.Where(v => v.Policy != null);

        if (viewerPartnerId.HasValue)
        {
            var partnerId = viewerPartnerId.Value;
            if (viewerIsManager)
            {
                return query.Where(v =>
                    v.Policy!.Implementer != null
                    && v.Policy.Implementer.PartnerId == partnerId);
            }

            return query.Where(v =>
                v.Policy!.Implementer != null
                && v.Policy.Implementer.PartnerId == partnerId
                && (v.Policy.SellerId == viewerEmployeeId || v.Policy.ImplementerId == viewerEmployeeId));
        }

        if (viewerIsManager && managedSubtreeDepartmentIds is { Count: > 0 })
        {
            var ids = managedSubtreeDepartmentIds;
            return query.Where(v =>
                v.Policy!.SellerId == viewerEmployeeId || v.Policy.ImplementerId == viewerEmployeeId
                || (v.Policy.Seller != null && ids.Contains(v.Policy.Seller.DepartmentId))
                || (v.Policy.Implementer != null && ids.Contains(v.Policy.Implementer.DepartmentId)));
        }

        var deptId = viewerDepartmentId;
        return query.Where(v =>
            v.Policy!.SellerId == viewerEmployeeId || v.Policy.ImplementerId == viewerEmployeeId
            || (v.Policy.Seller != null && v.Policy.Seller.DepartmentId == deptId)
            || (v.Policy.Implementer != null && v.Policy.Implementer.DepartmentId == deptId));
    }
}
