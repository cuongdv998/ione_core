using System;
using iOne.Policies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.Policies;

public class PolicyCertificateConfiguration : IEntityTypeConfiguration<PolicyCertificate>
{
    public void Configure(EntityTypeBuilder<PolicyCertificate> builder)
    {
        builder.ToTable("policy_certificate", t =>
        {
            t.HasComment("Giấy chứng nhận bảo hiểm");
        });

        builder.ConfigureByConvention();

        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.PolicyId)
            .HasColumnName("policy_id")
            .IsRequired()
            .HasComment("Tham chiếu đến đơn bảo hiểm gốc");

        builder.Property(x => x.PolicyVersionId)
            .HasColumnName("policy_version_id")
            .IsRequired()
            .HasComment("Tham chiếu đến phiên bản đơn bảo hiểm");

        builder.Property(x => x.CertificateNo)
            .HasColumnName("certificate_no")
            .HasMaxLength(50)
            .HasComment("Số giấy chứng nhận");

        builder.Property(x => x.Url)
            .HasColumnName("url")
            .HasMaxLength(250)
            .HasComment("Đường dẫn file giấy chứng nhận");

        // Audit columns
        builder.Property(x => x.ExtraProperties).HasColumnName("extra_properties");
        builder.Property(x => x.CreationTime).HasColumnName("creation_time");
        builder.Property(x => x.CreatorId).HasColumnName("creator_id");
        builder.Property(x => x.LastModificationTime).HasColumnName("last_modification_time");
        builder.Property(x => x.LastModifierId).HasColumnName("last_modifier_id");
        builder.Property(x => x.DeletionTime).HasColumnName("deletion_time");
        builder.Property(x => x.DeleterId).HasColumnName("deleter_id");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        builder.Property(x => x.ConcurrencyStamp).HasColumnName("concurrency_stamp");

        // Indexes
        builder.HasIndex(e => e.PolicyId, "ix_policy_certificate_policy_id");
        builder.HasIndex(e => e.PolicyVersionId, "ix_policy_certificate_policy_version_id");

        // Foreign Key Relationships
        // Policy
        builder.HasOne(x => x.Policy)
            .WithMany(x => x.PolicyCertificates)
            .HasForeignKey(x => x.PolicyId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_policy_certificate_policy_id");

        // PolicyVersion
        builder.HasOne(x => x.PolicyVersion)
            .WithMany(x => x.PolicyCertificates)
            .HasForeignKey(x => x.PolicyVersionId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_policy_certificate_policy_version_id");
    }
}
