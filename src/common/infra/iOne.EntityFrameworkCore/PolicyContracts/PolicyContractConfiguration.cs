using System;
using iOne.PolicyContracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.PolicyContracts;

public class PolicyContractConfiguration : IEntityTypeConfiguration<PolicyContract>
{
    public void Configure(EntityTypeBuilder<PolicyContract> builder)
    {
        builder.ToTable("policy_contract", t =>
        {
            t.HasComment("Hợp đồng bảo hiểm");
        });

        builder.ConfigureByConvention();

        builder.Property(x => x.Id).HasColumnName("id");

        // Basic Info
        builder.Property(x => x.InsurerId)
            .HasColumnName("insurer_id")
            .HasComment("Doanh nghiệp bảo hiểm");
        builder.Property(x => x.InsurerContractCode)
            .HasColumnName("insurer_contract_code")
            .HasMaxLength(50)
            .HasComment("Mã hợp đồng phía công ty bảo hiểm");
        builder.Property(x => x.LobId)
            .HasColumnName("lob_id")
            .HasComment("Nghiệp vụ bảo hiểm");
        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(250)
            .IsRequired();
        builder.Property(x => x.Type)
            .HasColumnName("type")
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<PolicyContractType>(v, true)
            )
            .HasMaxLength(15)
            .IsRequired()
            .HasComment("Loại hợp đồng:\n- individual: hợp đồng lẻ\n- group: hợp đồng nhóm");
        builder.Property(x => x.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired()
            .HasComment("Khách hàng ký hợp đồng");

        // Payer Info
        builder.Property(x => x.PayerName)
            .HasColumnName("payer_name")
            .HasMaxLength(250);
        builder.Property(x => x.PayerEmail)
            .HasColumnName("payer_email")
            .HasMaxLength(50);
        builder.Property(x => x.PayerPhone)
            .HasColumnName("payer_phone")
            .HasMaxLength(15);
        builder.Property(x => x.PayerProvinceId).HasColumnName("payer_province_id");
        builder.Property(x => x.PayerWardId).HasColumnName("payer_ward_id");
        builder.Property(x => x.PayerAddress)
            .HasColumnName("payer_address")
            .HasMaxLength(250);
        builder.Property(x => x.PayerFullAddress)
            .HasColumnName("payer_full_address")
            .HasMaxLength(500);
        builder.Property(x => x.PayerTin)
            .HasColumnName("payer_tin")
            .HasMaxLength(50);

        // Description
        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        // Dates
        builder.Property(x => x.EffectDate)
            .HasColumnName("effect_date")
            .IsRequired();
        builder.Property(x => x.ExpireDate).HasColumnName("expire_date");

        // Quantity
        builder.Property(x => x.Quantity)
            .HasColumnName("quantity")
            .HasColumnType("NUMERIC(6)")
            .IsRequired();
        builder.Property(x => x.CurrentQuantity)
            .HasColumnName("current_quantity")
            .HasColumnType("NUMERIC(6)");

        // Status
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<PolicyContractStatus>(v, true)
            )
            .HasMaxLength(15)
            .IsRequired()
            .HasComment("Trạng thái: active (hoạt động), deactive (không hoạt động)");

        // Employee
        builder.Property(x => x.EmployeeId).HasColumnName("employee_id");

        // Cancellation & Termination
        builder.Property(x => x.CancellationDate).HasColumnName("cancellation_date");
        builder.Property(x => x.TerminationDate).HasColumnName("termination_date");
        builder.Property(x => x.CancellationReasonId).HasColumnName("cancellation_reason_id");
        builder.Property(x => x.TerminationReasonId).HasColumnName("termination_reason_id");
        builder.Property(x => x.CancellationDescription)
            .HasColumnName("cancellation_description")
            .HasMaxLength(500);
        builder.Property(x => x.TerminationDescription)
            .HasColumnName("termination_description")
            .HasMaxLength(500);

        // Flags
        builder.Property(x => x.IsReciveInvoice)
            .HasColumnName("is_recive_invoice")
            .HasMaxLength(1)
            .IsRequired()
            .HasDefaultValue("N")
            .HasComment("Có nhận hóa đơn hay không");

        // Quotation
        builder.Property(x => x.QuotationId).HasColumnName("quotation_id");

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

        builder.HasIndex(e => e.InsurerId, "ix_policy_contract_insurer_id");
        builder.HasIndex(e => e.CustomerId, "ix_policy_contract_customer_id");
        builder.HasIndex(e => e.LobId, "ix_policy_contract_lob_id");
        builder.HasIndex(e => e.EmployeeId, "ix_policy_contract_employee_id");

        // Foreign Key Relationships
        // Insurer (ResPartner)
        builder.HasOne(x => x.Insurer)
            .WithMany(x => x.PolicyContracts)
            .HasForeignKey(x => x.InsurerId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_policy_contract_insurer_id");

        // Customer (ResCustomer)
        builder.HasOne(x => x.Customer)
            .WithMany(x => x.PolicyContracts)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_policy_contract_customer_id");

        // LOB (ProLineOfBusiness)
        builder.HasOne(x => x.Lob)
            .WithMany()
            .HasForeignKey(x => x.LobId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_policy_contract_lob_id");

        // Policies (one-to-many relationship)
        builder.HasMany(x => x.Policies)
            .WithOne(x => x.Contract)
            .HasForeignKey(x => x.ContractId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_policy_contract_id");

        // Employee (HrEmployee)
        builder.HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_policy_contract_employee_id");
    }
}
