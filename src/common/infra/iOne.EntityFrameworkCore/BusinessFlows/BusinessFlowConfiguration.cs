using System;
using iOne.BusinessFlows;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.BusinessFlows;

public class BusinessFlowConfiguration : IEntityTypeConfiguration<BusinessFlow>
{
    public void Configure(EntityTypeBuilder<BusinessFlow> builder)
    {
        builder.ToTable("business_flow", t =>
        {
            t.HasComment("Định nghĩa workflow cho business");
        });

        builder.ConfigureByConvention();

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.OrganizationId).HasColumnName("organization_id")
            .HasComment("Đơn vị (từ hr_department, dept_level = unit)");
        builder.Property(x => x.InsurerId).HasColumnName("insurer_id")
            .HasComment("Công ty bảo hiểm áp dụng");
        builder.Property(x => x.BusinessCode).HasColumnName("business_code")
            .HasMaxLength(50)
            .IsRequired()
            .HasComment("Mã nghiệp vụ liên quan, định nghĩa trong bảng admin_config với code = BUSINESS_CODE và sub_code là các nghiệp vụ tương ứng");
        builder.Property(x => x.WorkflowName).HasColumnName("workflow_name")
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(x => x.WorkflowVersion).HasColumnName("workflow_version")
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(x => x.EffectDate).HasColumnName("effect_date")
            .IsRequired();
        builder.Property(x => x.ExpireDate).HasColumnName("expire_date");
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<BusinessFlowStatus>(v, true))
            .HasMaxLength(10)
            .IsRequired()
            .HasComment("Trạng thái: active - Hoạt động; deactive - Không hoạt động");

        builder.Property(x => x.CreationTime).HasColumnName("creation_time");
        builder.Property(x => x.CreatorId).HasColumnName("creator_id");
        builder.Property(x => x.LastModificationTime).HasColumnName("last_modification_time");
        builder.Property(x => x.LastModifierId).HasColumnName("last_modifier_id");
        builder.Property(x => x.DeletionTime).HasColumnName("deletion_time");
        builder.Property(x => x.DeleterId).HasColumnName("deleter_id");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        builder.Property(x => x.ConcurrencyStamp).HasColumnName("concurrency_stamp");
    }
}
