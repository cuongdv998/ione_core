using iOne.ClaimFolderEvaluates;
using iOne.ClaimFolders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iOne.EntityFrameworkCore.ClaimFolderEvaluates;

public class ClaimFolderEvaluateConfiguration : IEntityTypeConfiguration<ClaimFolderEvaluate>
{
    public void Configure(EntityTypeBuilder<ClaimFolderEvaluate> builder)
    {
        builder.ToTable("claim_folde_revaluate");

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.ClaimFolderId).HasColumnName("claimfolderid").IsRequired();
        builder.Property(x => x.EmployeeId).HasColumnName("employeeid").IsRequired();
        builder.Property(x => x.Result).HasColumnName("result").HasMaxLength(1).IsRequired();
        builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(250);

        builder.Property(x => x.CreationTime).HasColumnName("creationtime");
        builder.Property(x => x.CreatorId).HasColumnName("creatorid");
        builder.Property(x => x.LastModificationTime).HasColumnName("lastmodificationtime");
        builder.Property(x => x.LastModifierId).HasColumnName("lastmodifierid");

        builder.HasOne<ClaimFolder>()
            .WithMany()
            .HasForeignKey(x => x.ClaimFolderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
