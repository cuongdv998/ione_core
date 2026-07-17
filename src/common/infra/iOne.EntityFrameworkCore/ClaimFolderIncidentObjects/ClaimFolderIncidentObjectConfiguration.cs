using iOne.ClaimFolderIncidentObjects;
using iOne.ClaimFolders;
using iOne.ResObjectTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ClaimFolderIncidentObjects;

public class ClaimFolderIncidentObjectConfiguration : IEntityTypeConfiguration<ClaimFolderIncidentObject>
{
    public void Configure(EntityTypeBuilder<ClaimFolderIncidentObject> builder)
    {
        builder.ToTable("claim_folder_incident_object", t =>
        {
            t.HasComment("Đối tượng tổn thất");
        });

        builder.ConfigureByConvention();

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.ClaimFolderId).HasColumnName("claimfolderid").IsRequired();
        builder.Property(x => x.ObjectTypeId).HasColumnName("objecttypeid").IsRequired();
        builder.Property(x => x.CarPlate).HasColumnName("carplate").HasMaxLength(50);
        builder.Property(x => x.CarEngineNumber).HasColumnName("carenginenumber").HasMaxLength(50);
        builder.Property(x => x.CarVin).HasColumnName("carvin").HasMaxLength(50);
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(250);
        builder.Property(x => x.IdNo).HasColumnName("idno").HasMaxLength(15);
        builder.Property(x => x.PassportNo).HasColumnName("passportno").HasMaxLength(25);
        builder.Property(x => x.ExitDate).HasColumnName("exitdate");
        builder.Property(x => x.ProfileNo).HasColumnName("profileno").HasMaxLength(50);

        builder.Property(x => x.CreationTime).HasColumnName("creationtime");
        builder.Property(x => x.CreatorId).HasColumnName("creatorid");
        builder.Property(x => x.LastModificationTime).HasColumnName("lastmodificationtime");
        builder.Property(x => x.LastModifierId).HasColumnName("lastmodifierid");

        builder.HasOne(x => x.ClaimFolder)
            .WithMany()
            .HasForeignKey(x => x.ClaimFolderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ObjectType)
            .WithMany()
            .HasForeignKey(x => x.ObjectTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

