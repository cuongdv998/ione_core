using System;
using iOne.ResBusinessAssignees;
using iOne.ResTaskCategories;
using iOne.WorkInstances;
using iOne.WorkTasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.WorkTasks;

public class WorkTaskConfiguration : IEntityTypeConfiguration<WorkTask>
{
    public void Configure(EntityTypeBuilder<WorkTask> builder)
    {
        builder.ToTable("work_task", t =>
        {
            t.HasComment("Bảng lưu các task thực hiện");
        });

        builder.ConfigureByConvention();

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.WorkInstanceId).HasColumnName("work_instance_id");
        builder.Property(x => x.BusinessCode).HasColumnName("business_code")
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(x => x.BusinessName).HasColumnName("business_name")
            .HasMaxLength(50)
            .IsRequired()
            .HasComment("Tên bảng liên quan đến task");
        builder.Property(x => x.BusinessKey).HasColumnName("business_key")
            .IsRequired()
            .HasComment("ID của bản ghi liên quan đến task");
        builder.Property(x => x.FormKey).HasColumnName("form_key")
            .HasMaxLength(250)
            .HasComment("Link đến form nghiệp vụ tương ứng");
        builder.Property(x => x.EventName).HasColumnName("event_name")
            .HasMaxLength(50)
            .HasComment("Event name");
        builder.Property(x => x.Code).HasColumnName("code")
            .HasMaxLength(50)
            .IsRequired()
            .HasComment("Mã task");
        builder.Property(x => x.Name).HasColumnName("name")
            .HasMaxLength(250)
            .IsRequired()
            .HasComment("Tên Task");
        builder.Property(x => x.Description).HasColumnName("description")
            .HasMaxLength(250);
        builder.Property(x => x.ReporterId).HasColumnName("reporter_id")
            .IsRequired()
            .HasComment("Người gán việc");
        builder.Property(x => x.BusinessAuthorityCode).HasColumnName("business_authority_code")
            .HasMaxLength(50)
            .HasComment("Mã phân cấp duyệt");
        builder.Property(x => x.AssigneeId).HasColumnName("assignee_id")
            .HasComment("Người nhận việc");
        builder.Property(x => x.AssigneeDepartmentId).HasColumnName("assignee_department_id")
            .HasComment("Đơn vị của người nhận việc");
        builder.Property(x => x.AssigneeOrganizationId).HasColumnName("assignee_organization_id");
        builder.Property(x => x.ResBusinessAssigneeId).HasColumnName("res_business_assignee_id")
            .HasComment("Cấu hình phân cấp duyệt áp dụng khi tạo task");
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<WorkTaskStatus>(v, true))
            .HasMaxLength(15)
            .IsRequired()
            .HasComment("Trạng thái: new, inprogress, completed, accepted, rejected, cancelled, wait_approve, approved, pending, return");
        builder.Property(x => x.Priority)
            .HasColumnName("priority")
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<WorkTaskPriority>(v, true))
            .HasMaxLength(15)
            .IsRequired()
            .HasComment("Mức ưu tiên: high, medium, low");
        builder.Property(x => x.TaskCategoryId).HasColumnName("task_category_id")
            .IsRequired();
        builder.Property(x => x.StartDate).HasColumnName("start_date")
            .IsRequired()
            .HasComment("Ngày dự kiến bắt đầu task");
        builder.Property(x => x.EndDate).HasColumnName("end_date")
            .IsRequired()
            .HasComment("Ngày dự kiến kết thúc task");
        builder.Property(x => x.ActualStartDate).HasColumnName("actual_start_date")
            .HasComment("Ngày bắt đầu thực tế task");
        builder.Property(x => x.ActualEndDate).HasColumnName("actual_end_date")
            .HasComment("Ngày kết thúc task");
        builder.Property(x => x.ReasonId).HasColumnName("reason_id")
            .HasComment("Lý do (nếu từ chối)");
        builder.Property(x => x.ReasonDescription).HasColumnName("reason_description")
            .HasMaxLength(250)
            .HasComment("Mô tả lý do");
        builder.Property(x => x.CreationTime).HasColumnName("creation_time");
        builder.Property(x => x.CreatorId).HasColumnName("creator_id");
        builder.Property(x => x.LastModificationTime).HasColumnName("last_modification_time");
        builder.Property(x => x.LastModifierId).HasColumnName("last_modifier_id");
        builder.Property(x => x.DeletionTime).HasColumnName("deletion_time");
        builder.Property(x => x.DeleterId).HasColumnName("deleter_id");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        builder.Property(x => x.ConcurrencyStamp).HasColumnName("concurrency_stamp");

        builder.HasOne<WorkInstance>()
            .WithMany()
            .HasForeignKey(x => x.WorkInstanceId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ResTaskCategory>()
            .WithMany()
            .HasForeignKey(x => x.TaskCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ResBusinessAssignee>()
            .WithMany()
            .HasForeignKey(x => x.ResBusinessAssigneeId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
