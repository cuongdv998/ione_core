using System;
using iOne.BusinessFlows;
using iOne.WorkInstances;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.WorkInstances;

public class WorkInstanceConfiguration : IEntityTypeConfiguration<WorkInstance>
{
    public void Configure(EntityTypeBuilder<WorkInstance> builder)
    {
        builder.ToTable("work_instance", t =>
        {
            t.HasComment("Bảng lưu thông tin WorkFlow xử lý");
        });

        builder.ConfigureByConvention();

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.WorkflowInstanceId).HasColumnName("workflow_instance_id")
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(x => x.BusinessFlowId).HasColumnName("business_flow_id")
            .IsRequired();
        builder.Property(x => x.BusinessCode).HasColumnName("business_code")
            .HasMaxLength(36);
        builder.Property(x => x.BusinessName).HasColumnName("business_name")
            .HasMaxLength(50);
        builder.Property(x => x.BusinessKey).HasColumnName("business_key");
        builder.Property(x => x.StartDate).HasColumnName("start_date");
        builder.Property(x => x.EndDate).HasColumnName("end_date");
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<WorkInstanceStatus>(v, true))
            .HasMaxLength(15)
            .IsRequired()
            .HasComment("Trạng thái: new, inprogress, completed, accepted, rejected, cancelled, wait_approve, approved, pending, return");
        builder.Property(x => x.CreationTime).HasColumnName("creation_time");
        builder.Property(x => x.CreatorId).HasColumnName("creator_id");
        builder.Property(x => x.ConcurrencyStamp).HasColumnName("concurrency_stamp");

        builder.HasOne<BusinessFlow>()
            .WithMany()
            .HasForeignKey(x => x.BusinessFlowId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
