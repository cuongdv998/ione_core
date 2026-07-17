using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.Reports
{
    public class ReportTemplateSqlConfiguration : IEntityTypeConfiguration<ReportTemplateSql>
    {
        public void Configure(EntityTypeBuilder<ReportTemplateSql> builder)
        {
            builder.ToTable("report_template_sql");

            builder.ConfigureByConvention();

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.ReportTemplateId)
                .HasColumnName("report_template_id")
                .IsRequired();

            builder.Property(x => x.SqlText)
                .HasColumnName("sql_text")
                .IsRequired()
                .HasColumnType("text");

            builder.Property(x => x.VarName)
                .HasColumnName("var_name")
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Status)
                .HasColumnName("status")
                .IsRequired()
                .HasMaxLength(15)
                .HasDefaultValue("active");

            builder.Property(x => x.IsSingle)
                .HasColumnName("is_single")
                .IsRequired()
                .HasDefaultValue(false);

            // ===== Audited (snake_case) =====
            builder.Property(x => x.CreationTime).HasColumnName("creation_time");
            builder.Property(x => x.CreatorId).HasColumnName("creator_id");
            builder.Property(x => x.LastModificationTime).HasColumnName("last_modification_time");
            builder.Property(x => x.LastModifierId).HasColumnName("last_modifier_id");
            builder.Property(x => x.DeletionTime).HasColumnName("deletion_time");
            builder.Property(x => x.DeleterId).HasColumnName("deleter_id");
            builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
            builder.Property(x => x.ConcurrencyStamp).HasColumnName("concurrency_stamp");
            builder.Property(x => x.ExtraProperties).HasColumnName("extra_properties");
            // ===== FK =====
            builder.HasOne(x => x.ReportTemplate)
                .WithMany()
                .HasForeignKey(x => x.ReportTemplateId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
