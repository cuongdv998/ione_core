# Plan Xây Dựng Backend: Quản lý Khách hàng (ResCustomer)

## 📋 Tổng Quan

**Module**: Customer (Quản lý khách hàng)  
**Entity**: ResCustomer (Khách hàng)  
**Table Name**: `res_customer` (snake_case - PostgreSQL convention)  
**DDL Table Name**: `RESCUSTOMER` (sẽ convert sang `res_customer`)

---

## 🎯 Yêu Cầu Chức Năng

1. ✅ **CRUD Operations**: Tìm kiếm, Thêm, Sửa, Xóa
2. ✅ **Validation Code**: Mã khách hàng chỉ cho phép A-Z, 0-9, _ (uppercase)
3. ✅ **Code Immutable**: Khi sửa không được phép sửa mã khách hàng (disable trên UI)
4. ✅ **Code Unique**: Mã khách hàng là duy nhất
5. ✅ **FullAddress Auto**: FullAddress được tự động nối từ `<address>`, `<tên phường xã>`, `<tên tỉnh thành>`. Người dùng không được phép nhập (disable trên UI)
6. ✅ **Conditional Fields**: 
   - Nếu `organization_type.Type = "CN"` (Cá nhân): 
     - ✅ Hiển thị: `IdNo` (CCCD)
     - ❌ Ẩn: `Tin`, `RepName`, `RepEmail`, `RepPhone`, `RepIdNo`, `RepTitle`, `Authorizer`, `AuthorizerPhone`, `AuthorizerEmail`, `AuthorizerNo`, `AuthorizerDate`, `AuthorizerTitle`, `BusinessNo`
   - Nếu `organization_type.Type = "TC"` (Tổ chức):
     - ❌ Ẩn: `IdNo` (CCCD)
     - ✅ Hiển thị: `Tin`, `RepName`, `RepEmail`, `RepPhone`, `RepIdNo`, `RepTitle`, `Authorizer`, `AuthorizerPhone`, `AuthorizerEmail`, `AuthorizerNo`, `AuthorizerDate`, `AuthorizerTitle`, `BusinessNo`
7. ✅ **Soft Delete**: Khi xóa → soft delete (ABP audit log)
8. ✅ **Đa ngôn ngữ**: vi-VN và en
9. ✅ **Phân quyền**: View, Create, Edit, Delete

---

## 📊 Database Schema

### Table: `res_customer`

| Column Name (snake_case) | Type | Constraints | Description |
|-------------------------|------|-------------|-------------|
| `id` | UUID | PRIMARY KEY | ID khách hàng |
| `code` | VARCHAR(25) | NULL, UNIQUE | Mã khách hàng (A-Z, 0-9, _) |
| `name` | VARCHAR(250) | NOT NULL | Tên khách hàng |
| `industry_id` | UUID | NULL, FK | ID ngành nghề (FK → res_industry) |
| `province_id` | UUID | NOT NULL, FK | ID tỉnh/thành (FK → res_province) |
| `ward_id` | UUID | NOT NULL, FK | ID phường/xã (FK → res_ward) |
| `address` | VARCHAR(250) | NOT NULL | Địa chỉ chi tiết |
| `full_address` | VARCHAR(500) | NOT NULL | Địa chỉ đầy đủ (auto-generated) |
| `email` | VARCHAR(50) | NULL | Email |
| `phone` | VARCHAR(15) | NOT NULL | Số điện thoại |
| `note` | VARCHAR(500) | NULL | Ghi chú |
| `status` | VARCHAR(10) | NOT NULL | Trạng thái: "active" hoặc "deactive" |
| `tin` | VARCHAR(50) | NULL | Mã số thuế (chỉ hiển thị khi organization_type = TC) |
| `id_no` | VARCHAR(25) | NULL | Số CCCD (chỉ hiển thị khi organization_type = CN) |
| `passport_no` | VARCHAR(25) | NULL | Số hộ chiếu |
| `dob` | DATE | NULL | Ngày sinh |
| `sex` | VARCHAR(15) | NULL | Giới tính: "M" (Nam) hoặc "F" (Nữ) |
| `rep_name` | VARCHAR(250) | NULL | Tên người đại diện (chỉ hiển thị khi organization_type = TC) |
| `rep_email` | VARCHAR(50) | NULL | Email người đại diện (chỉ hiển thị khi organization_type = TC) |
| `rep_phone` | VARCHAR(15) | NULL | Số điện thoại người đại diện (chỉ hiển thị khi organization_type = TC) |
| `rep_id_no` | VARCHAR(25) | NULL | Số CCCD người đại diện (chỉ hiển thị khi organization_type = TC) |
| `rep_title` | VARCHAR(250) | NULL | Chức danh người đại diện (chỉ hiển thị khi organization_type = TC) |
| `authorizer` | VARCHAR(50) | NULL | Người ủy quyền (chỉ hiển thị khi organization_type = TC) |
| `authorizer_phone` | VARCHAR(15) | NULL | Số điện thoại người ủy quyền (chỉ hiển thị khi organization_type = TC) |
| `authorizer_email` | VARCHAR(50) | NULL | Email người ủy quyền (chỉ hiển thị khi organization_type = TC) |
| `authorizer_no` | VARCHAR(25) | NULL | Số ủy quyền (chỉ hiển thị khi organization_type = TC) |
| `authorizer_date` | DATE | NULL | Ngày ủy quyền (chỉ hiển thị khi organization_type = TC) |
| `authorizer_title` | VARCHAR(50) | NULL | Chức danh người ủy quyền (chỉ hiển thị khi organization_type = TC) |
| `business_no` | VARCHAR(25) | NULL | Số giấy phép kinh doanh (chỉ hiển thị khi organization_type = TC) |
| `organization_type_id` | UUID | NULL, FK | ID loại tổ chức (FK → res_organization_type) |
| `invoice_province_id` | UUID | NULL, FK | ID tỉnh (theo địa chỉ xuất hóa đơn) (FK → res_province) |
| `invoice_ward_id` | UUID | NULL, FK | ID phường/xã (theo địa chỉ xuất hóa đơn) (FK → res_ward) |
| `invoice_address` | VARCHAR(250) | NULL | Địa chỉ xuất hóa đơn |
| `invoice_full_address` | VARCHAR(500) | NULL | Địa chỉ đầy đủ xuất hóa đơn (auto-generated) |
| `sale_id` | UUID | NULL, FK | ID nhân viên quản lý khách hàng (FK → hr_employee) |
| `creation_time` | TIMESTAMP | NOT NULL | Thời gian tạo |
| `creator_id` | UUID | NOT NULL | ID người tạo |
| `last_modification_time` | TIMESTAMP | NULL | Thời gian sửa cuối |
| `last_modifier_id` | UUID | NULL | ID người sửa cuối |
| `is_deleted` | BOOLEAN | NOT NULL, DEFAULT false | Đánh dấu xóa (soft delete) |
| `deletion_time` | TIMESTAMP | NULL | Thời gian xóa |
| `deleter_id` | UUID | NULL | ID người xóa |
| `concurrency_stamp` | VARCHAR(40) | NULL | Concurrency stamp |
| `tenant_id` | UUID | NULL | Tenant ID (multi-tenancy) |

### Foreign Keys

- `fk_res_customer_industry_id` → `res_industry(id)` (ON DELETE RESTRICT, ON UPDATE RESTRICT)
- `fk_res_customer_province_id` → `res_province(id)` (ON DELETE RESTRICT, ON UPDATE RESTRICT)
- `fk_res_customer_ward_id` → `res_ward(id)` (ON DELETE RESTRICT, ON UPDATE RESTRICT)
- `fk_res_customer_organization_type_id` → `res_organization_type(id)` (ON DELETE RESTRICT, ON UPDATE RESTRICT)
- `fk_res_customer_invoice_province_id` → `res_province(id)` (ON DELETE RESTRICT, ON UPDATE RESTRICT)
- `fk_res_customer_invoice_ward_id` → `res_ward(id)` (ON DELETE RESTRICT, ON UPDATE RESTRICT)
- `fk_res_customer_sale_id` → `hr_employee(id)` (ON DELETE RESTRICT, ON UPDATE RESTRICT)

### Indexes

- **Primary Key**: `pk_res_customer` (on `id`)
- **Unique Index**: `ix_res_customer_code` (on `code`, unique, nullable)

---

## 📁 Cấu Trúc Files Cần Tạo

### 1. Domain Layer

#### 1.1. Enum Status
**Location**: `src/common/domain/iOne.Domain.Shared/ResCustomers/ResCustomerStatus.cs`

```csharp
namespace iOne.ResCustomers;

public enum ResCustomerStatus
{
    Active = 0,    // Hoạt động
    Deactive = 1   // Không hoạt động
}
```

**Lưu ý**: 
- Enum values sẽ được convert sang string trong database: "active", "deactive"
- MaxLength(10) trong EF Core configuration

---

#### 1.2. Enum Sex
**Location**: `src/common/domain/iOne.Domain.Shared/ResCustomers/ResCustomerSex.cs`

```csharp
namespace iOne.ResCustomers;

public enum ResCustomerSex
{
    Male = 0,      // Nam (M)
    Female = 1     // Nữ (F)
}
```

**Lưu ý**: 
- Enum values sẽ được convert sang string trong database: "M", "F"
- MaxLength(15) trong EF Core configuration

---

#### 1.3. Entity: ResCustomer
**Location**: `src/common/domain/iOne.Domain/ResCustomers/ResCustomer.cs`

**Yêu cầu**:
- Kế thừa từ `FullAuditedAggregateRoot<Guid>`
- Table name: `[Table("res_customer")]` (snake_case)
- Properties với private setters
- Constructor với validation
- Methods để set properties (KHÔNG có `SetCode()` sau khi tạo)
- Method `UpdateFullAddress()` để tự động nối address + ward name + province name
- Validation Code: Regex `^[A-Z0-9_]+$` (uppercase)

**Properties chính**:
- `Code` (string, max 25, nullable, unique, immutable sau khi tạo)
- `Name` (string, max 250, required)
- `IndustryId` (Guid?, optional, FK to ResIndustry)
- `ProvinceId` (Guid, required, FK to ResProvince)
- `WardId` (Guid, required, FK to ResWard)
- `Address` (string, max 250, required)
- `FullAddress` (string, max 500, required, auto-generated)
- `Email` (string, max 50, optional)
- `Phone` (string, max 15, required)
- `Note` (string, max 500, optional)
- `Status` (ResCustomerStatus enum, required)
- `Tin` (string, max 50, optional) - chỉ hiển thị khi organization_type = TC
- `IdNo` (string, max 25, optional) - chỉ hiển thị khi organization_type = CN
- `PassportNo` (string, max 25, optional)
- `Dob` (DateTime?, optional)
- `Sex` (ResCustomerSex enum, optional)
- `RepName` (string, max 250, optional) - chỉ hiển thị khi organization_type = TC
- `RepEmail` (string, max 50, optional) - chỉ hiển thị khi organization_type = TC
- `RepPhone` (string, max 15, optional) - chỉ hiển thị khi organization_type = TC
- `RepIdNo` (string, max 25, optional) - chỉ hiển thị khi organization_type = TC
- `RepTitle` (string, max 250, optional) - chỉ hiển thị khi organization_type = TC
- `Authorizer` (string, max 50, optional) - chỉ hiển thị khi organization_type = TC
- `AuthorizerPhone` (string, max 15, optional) - chỉ hiển thị khi organization_type = TC
- `AuthorizerEmail` (string, max 50, optional) - chỉ hiển thị khi organization_type = TC
- `AuthorizerNo` (string, max 25, optional) - chỉ hiển thị khi organization_type = TC
- `AuthorizerDate` (DateTime?, optional) - chỉ hiển thị khi organization_type = TC
- `AuthorizerTitle` (string, max 50, optional) - chỉ hiển thị khi organization_type = TC
- `BusinessNo` (string, max 25, optional) - chỉ hiển thị khi organization_type = TC
- `OrganizationTypeId` (Guid?, optional, FK to ResOrganizationType)
- `InvoiceProvinceId` (Guid?, optional, FK to ResProvince)
- `InvoiceWardId` (Guid?, optional, FK to ResWard)
- `InvoiceAddress` (string, max 250, optional)
- `InvoiceFullAddress` (string, max 500, optional, auto-generated)
- `SaleId` (Guid?, optional, FK to HrEmployee)

**Navigation Properties**:
- `Industry` (ResIndustry?)
- `Province` (ResProvince?)
- `Ward` (ResWard?)
- `OrganizationType` (ResOrganizationType?)
- `InvoiceProvince` (ResProvince?)
- `InvoiceWard` (ResWard?)
- `Sale` (HrEmployee?)

**Methods**:
- `SetCode(string code)` - chỉ gọi trong constructor
- `UpdateName(string name)`
- `UpdateFullAddress(string? wardName, string? provinceName)` - tự động nối address + ward name + province name
- `UpdateInvoiceFullAddress(string? wardName, string? provinceName)` - tự động nối invoice address + ward name + province name
- Các methods khác để update properties

---

#### 1.4. Repository Interface
**Location**: `src/common/domain/iOne.Domain/ResCustomers/IResCustomerRepository.cs`

```csharp
using Volo.Abp.Domain.Repositories;

namespace iOne.ResCustomers;

public interface IResCustomerRepository : IRepository<ResCustomer, Guid>
{
    // Có thể thêm custom queries nếu cần
}
```

---

### 2. Application Layer

#### 2.1. DTOs
**Location**: `modules/customer/src/iOne.Customer.Application.Contracts/ResCustomers/`

##### 2.1.1. ResCustomerDto.cs
```csharp
public class ResCustomerDto : FullAuditedEntityDto<Guid>
{
    public string? Code { get; set; }
    public string Name { get; set; } = null!;
    public Guid? IndustryId { get; set; }
    public string? IndustryName { get; set; }
    public Guid ProvinceId { get; set; }
    public string? ProvinceName { get; set; }
    public Guid WardId { get; set; }
    public string? WardName { get; set; }
    public string Address { get; set; } = null!;
    public string FullAddress { get; set; } = null!;
    public string? Email { get; set; }
    public string Phone { get; set; } = null!;
    public string? Note { get; set; }
    public ResCustomerStatus Status { get; set; }
    public string? Tin { get; set; }
    public string? IdNo { get; set; }
    public string? PassportNo { get; set; }
    public DateTime? Dob { get; set; }
    public ResCustomerSex? Sex { get; set; }
    public string? RepName { get; set; }
    public string? RepEmail { get; set; }
    public string? RepPhone { get; set; }
    public string? RepIdNo { get; set; }
    public string? RepTitle { get; set; }
    public string? Authorizer { get; set; }
    public string? AuthorizerPhone { get; set; }
    public string? AuthorizerEmail { get; set; }
    public string? AuthorizerNo { get; set; }
    public DateTime? AuthorizerDate { get; set; }
    public string? AuthorizerTitle { get; set; }
    public string? BusinessNo { get; set; }
    public Guid? OrganizationTypeId { get; set; }
    public string? OrganizationTypeName { get; set; }
    public string? OrganizationTypeType { get; set; } // "CN" hoặc "TC"
    public Guid? InvoiceProvinceId { get; set; }
    public string? InvoiceProvinceName { get; set; }
    public Guid? InvoiceWardId { get; set; }
    public string? InvoiceWardName { get; set; }
    public string? InvoiceAddress { get; set; }
    public string? InvoiceFullAddress { get; set; }
    public Guid? SaleId { get; set; }
    public string? SaleName { get; set; }
}
```

##### 2.1.2. CreateResCustomerDto.cs
```csharp
public class CreateResCustomerDto
{
    [Required]
    [MaxLength(25)]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "Code can only contain uppercase letters (A-Z), numbers (0-9) and underscore (_)")]
    public string? Code { get; set; }
    
    [Required]
    [MaxLength(250)]
    public string Name { get; set; } = null!;
    
    public Guid? IndustryId { get; set; }
    
    [Required]
    public Guid ProvinceId { get; set; }
    
    [Required]
    public Guid WardId { get; set; }
    
    [Required]
    [MaxLength(250)]
    public string Address { get; set; } = null!;
    
    [MaxLength(50)]
    [EmailAddress]
    public string? Email { get; set; }
    
    [Required]
    [MaxLength(15)]
    public string Phone { get; set; } = null!;
    
    [MaxLength(500)]
    public string? Note { get; set; }
    
    [Required]
    public ResCustomerStatus Status { get; set; }
    
    [MaxLength(50)]
    public string? Tin { get; set; }
    
    [MaxLength(25)]
    public string? IdNo { get; set; }
    
    [MaxLength(25)]
    public string? PassportNo { get; set; }
    
    public DateTime? Dob { get; set; }
    
    public ResCustomerSex? Sex { get; set; }
    
    [MaxLength(250)]
    public string? RepName { get; set; }
    
    [MaxLength(50)]
    [EmailAddress]
    public string? RepEmail { get; set; }
    
    [MaxLength(15)]
    public string? RepPhone { get; set; }
    
    [MaxLength(25)]
    public string? RepIdNo { get; set; }
    
    [MaxLength(250)]
    public string? RepTitle { get; set; }
    
    [MaxLength(50)]
    public string? Authorizer { get; set; }
    
    [MaxLength(15)]
    public string? AuthorizerPhone { get; set; }
    
    [MaxLength(50)]
    [EmailAddress]
    public string? AuthorizerEmail { get; set; }
    
    [MaxLength(25)]
    public string? AuthorizerNo { get; set; }
    
    public DateTime? AuthorizerDate { get; set; }
    
    [MaxLength(50)]
    public string? AuthorizerTitle { get; set; }
    
    [MaxLength(25)]
    public string? BusinessNo { get; set; }
    
    public Guid? OrganizationTypeId { get; set; }
    
    public Guid? InvoiceProvinceId { get; set; }
    
    public Guid? InvoiceWardId { get; set; }
    
    [MaxLength(250)]
    public string? InvoiceAddress { get; set; }
    
    public Guid? SaleId { get; set; }
}
```

##### 2.1.3. UpdateResCustomerDto.cs
```csharp
public class UpdateResCustomerDto
{
    // KHÔNG có Code (immutable)
    
    [Required]
    [MaxLength(250)]
    public string Name { get; set; } = null!;
    
    public Guid? IndustryId { get; set; }
    
    [Required]
    public Guid ProvinceId { get; set; }
    
    [Required]
    public Guid WardId { get; set; }
    
    [Required]
    [MaxLength(250)]
    public string Address { get; set; } = null!;
    
    [MaxLength(50)]
    [EmailAddress]
    public string? Email { get; set; }
    
    [Required]
    [MaxLength(15)]
    public string Phone { get; set; } = null!;
    
    [MaxLength(500)]
    public string? Note { get; set; }
    
    [Required]
    public ResCustomerStatus Status { get; set; }
    
    [MaxLength(50)]
    public string? Tin { get; set; }
    
    [MaxLength(25)]
    public string? IdNo { get; set; }
    
    [MaxLength(25)]
    public string? PassportNo { get; set; }
    
    public DateTime? Dob { get; set; }
    
    public ResCustomerSex? Sex { get; set; }
    
    [MaxLength(250)]
    public string? RepName { get; set; }
    
    [MaxLength(50)]
    [EmailAddress]
    public string? RepEmail { get; set; }
    
    [MaxLength(15)]
    public string? RepPhone { get; set; }
    
    [MaxLength(25)]
    public string? RepIdNo { get; set; }
    
    [MaxLength(250)]
    public string? RepTitle { get; set; }
    
    [MaxLength(50)]
    public string? Authorizer { get; set; }
    
    [MaxLength(15)]
    public string? AuthorizerPhone { get; set; }
    
    [MaxLength(50)]
    [EmailAddress]
    public string? AuthorizerEmail { get; set; }
    
    [MaxLength(25)]
    public string? AuthorizerNo { get; set; }
    
    public DateTime? AuthorizerDate { get; set; }
    
    [MaxLength(50)]
    public string? AuthorizerTitle { get; set; }
    
    [MaxLength(25)]
    public string? BusinessNo { get; set; }
    
    public Guid? OrganizationTypeId { get; set; }
    
    public Guid? InvoiceProvinceId { get; set; }
    
    public Guid? InvoiceWardId { get; set; }
    
    [MaxLength(250)]
    public string? InvoiceAddress { get; set; }
    
    public Guid? SaleId { get; set; }
}
```

##### 2.1.4. GetResCustomersInput.cs
```csharp
public class GetResCustomersInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public Guid? IndustryId { get; set; }
    public Guid? ProvinceId { get; set; }
    public Guid? WardId { get; set; }
    public Guid? OrganizationTypeId { get; set; }
    public Guid? SaleId { get; set; }
    public ResCustomerStatus? Status { get; set; }
}
```

---

#### 2.2. Application Service Interface
**Location**: `modules/customer/src/iOne.Customer.Application.Contracts/ResCustomers/IResCustomerAppService.cs`

```csharp
using Volo.Abp.Application.Services;

namespace iOne.Customer.ResCustomers;

public interface IResCustomerAppService : ICrudAppService<
    ResCustomerDto,
    Guid,
    GetResCustomersInput,
    CreateResCustomerDto,
    UpdateResCustomerDto>
{
}
```

---

#### 2.3. Application Service Implementation
**Location**: `modules/customer/src/iOne.Customer.Application/ResCustomers/ResCustomerAppService.cs`

**Yêu cầu**:
- Inject `IResCustomerRepository`, `IResIndustryRepository`, `IResProvinceRepository`, `IResWardRepository`, `IResOrganizationTypeRepository`, `IHrEmployeeRepository`
- Inject `ObjectMapper`, `ILocalizer`
- Implement CRUD operations
- Validation:
  - Code unique (nếu có)
  - Code format: A-Z, 0-9, _
  - Foreign keys tồn tại
- Tự động tính FullAddress và InvoiceFullAddress khi create/update
- Load navigation properties để lấy tên (IndustryName, ProvinceName, WardName, etc.)

**Logic FullAddress**:
```csharp
// Trong CreateAsync và UpdateAsync
var province = await _provinceRepository.GetAsync(input.ProvinceId);
var ward = await _wardRepository.GetAsync(input.WardId);
var fullAddress = $"{input.Address}, {ward.Name}, {province.Name}";
entity.UpdateFullAddress(ward.Name, province.Name);
```

**Logic InvoiceFullAddress** (nếu có):
```csharp
if (input.InvoiceProvinceId.HasValue && input.InvoiceWardId.HasValue && !string.IsNullOrWhiteSpace(input.InvoiceAddress))
{
    var invoiceProvince = await _provinceRepository.GetAsync(input.InvoiceProvinceId.Value);
    var invoiceWard = await _wardRepository.GetAsync(input.InvoiceWardId.Value);
    var invoiceFullAddress = $"{input.InvoiceAddress}, {invoiceWard.Name}, {invoiceProvince.Name}";
    entity.UpdateInvoiceFullAddress(invoiceWard.Name, invoiceProvince.Name);
}
```

---

#### 2.4. AutoMapper Configuration
**Location**: `modules/customer/src/iOne.Customer.Application/iOneCustomerApplicationAutoMapperProfile.cs`

**QUAN TRỌNG**: Phải thêm mapping configuration:

```csharp
CreateMap<ResCustomer, ResCustomerDto>();
CreateMap<CreateResCustomerDto, ResCustomer>();
CreateMap<UpdateResCustomerDto, ResCustomer>();
```

---

### 3. Permissions

#### 3.1. Permission Constants
**Location**: `modules/customer/src/iOne.Customer.Application.Contracts/Permissions/ResCustomerPermissions.cs`

```csharp
namespace iOne.Customer.Permissions;

public static class ResCustomerPermissions
{
    public const string GroupName = "CustomerResCustomer";

    public const string Default = GroupName;
    public const string Create = Default + ".Create";
    public const string Edit = Default + ".Edit";
    public const string Delete = Default + ".Delete";
    public const string View = Default + ".View";
}
```

---

#### 3.2. Permission Definition Provider
**Location**: `modules/customer/src/iOne.Customer.Application.Contracts/Permissions/ResCustomerPermissionDefinitionProvider.cs`

```csharp
using iOne.Customer.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Customer.Permissions;

public class ResCustomerPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var resCustomerGroup = context.AddGroup(
            ResCustomerPermissions.GroupName,
            L("Permission:ResCustomer")
        );

        var resCustomerPermission = resCustomerGroup.AddPermission(
            ResCustomerPermissions.Default,
            L("Permission:ResCustomer")
        );

        resCustomerPermission.AddChild(
            ResCustomerPermissions.Create,
            L("Permission:Create")
        );

        resCustomerPermission.AddChild(
            ResCustomerPermissions.Edit,
            L("Permission:Edit")
        );

        resCustomerPermission.AddChild(
            ResCustomerPermissions.Delete,
            L("Permission:Delete")
        );

        resCustomerPermission.AddChild(
            ResCustomerPermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<CustomerResource>(name);
    }
}
```

---

### 4. Entity Framework Core

#### 4.1. Entity Configuration
**Location**: `src/common/infra/iOne.EntityFrameworkCore/ResCustomers/ResCustomerConfiguration.cs`

**Yêu cầu**:
- Table name: `"res_customer"` (snake_case)
- Tất cả column names: snake_case
- Configure foreign keys với tên snake_case: `fk_res_customer_xxx`
- Configure indexes: `ix_res_customer_code` (unique, nullable)
- Enum conversions:
  - Status: `"active"`, `"deactive"` (lowercase)
  - Sex: `"M"`, `"F"` (uppercase)

**Ví dụ**:
```csharp
public class ResCustomerConfiguration : IEntityTypeConfiguration<ResCustomer>
{
    public void Configure(EntityTypeBuilder<ResCustomer> builder)
    {
        builder.ToTable("res_customer", t =>
        {
            t.HasComment("Bảng lưu thông tin khách hàng");
        });

        builder.ConfigureByConvention();

        // Column names: snake_case
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Code).HasColumnName("code").HasMaxLength(25);
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(250).IsRequired();
        // ... các columns khác

        // Status: Enum to string (lowercase)
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<ResCustomerStatus>(v, true)
            )
            .HasMaxLength(10)
            .IsRequired();

        // Sex: Enum to string (uppercase: M, F)
        builder.Property(x => x.Sex)
            .HasColumnName("sex")
            .HasConversion<string>(
                v => v.HasValue ? (v.Value == ResCustomerSex.Male ? "M" : "F") : null,
                v => string.IsNullOrEmpty(v) ? null : (v == "M" ? ResCustomerSex.Male : ResCustomerSex.Female)
            )
            .HasMaxLength(15);

        // Foreign Keys
        builder.HasOne(x => x.Industry)
            .WithMany()
            .HasForeignKey(x => x.IndustryId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_res_customer_industry_id");

        // ... các foreign keys khác

        // Indexes
        builder.HasIndex(x => x.Code, "ix_res_customer_code")
            .IsUnique();
    }
}
```

---

#### 4.2. Repository Implementation
**Location**: `src/common/infra/iOne.EntityFrameworkCore/ResCustomers/EfCoreResCustomerRepository.cs`

```csharp
using iOne.ResCustomers;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace iOne.EntityFrameworkCore.ResCustomers;

public class EfCoreResCustomerRepository : EfCoreRepository<iOneDbContext, ResCustomer, Guid>, IResCustomerRepository
{
    public EfCoreResCustomerRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }
}
```

---

#### 4.3. Register Repository
**Location**: `src/common/infra/iOne.EntityFrameworkCore/EntityFrameworkCore/iOneDbContext.cs`

Thêm vào `OnModelCreating`:
```csharp
builder.Entity<ResCustomer>(b =>
{
    b.ToTable("res_customer");
    b.ConfigureByConvention();
});
```

Hoặc sử dụng:
```csharp
modelBuilder.ApplyConfiguration(new ResCustomerConfiguration());
```

---

#### 4.4. Database Migration

**Quy tắc QUAN TRỌNG**:
- **LUÔN** sử dụng `IF EXISTS` khi DROP
- **LUÔN** sử dụng `IF NOT EXISTS` khi CREATE
- Table và column names: **snake_case**
- Index names: **snake_case** với prefix

**Ví dụ**:
```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""res_customer"";");
    
    migrationBuilder.CreateTable(
        name: "res_customer",
        columns: table => new
        {
            id = table.Column<Guid>(type: "uuid", nullable: false),
            code = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true),
            name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
            // ... các columns khác
            creation_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
            // ... audit columns
        },
        constraints: table =>
        {
            table.PrimaryKey("pk_res_customer", x => x.id);
            table.ForeignKey(
                name: "fk_res_customer_industry_id",
                column: x => x.industry_id,
                principalTable: "res_industry",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
            // ... các foreign keys khác
        },
        comment: "Bảng lưu thông tin khách hàng");

    migrationBuilder.CreateIndex(
        name: "ix_res_customer_code",
        table: "res_customer",
        column: "code",
        unique: true);
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""ix_res_customer_code"";");
    migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""res_customer"";");
}
```

---

### 5. Localization

#### 5.1. Localization Files
**Location**: 
- `modules/customer/src/iOne.Customer.Application.Contracts/Localization/Customer/vi-VN.json`
- `modules/customer/src/iOne.Customer.Application.Contracts/Localization/Customer/en.json`

**Keys cần thêm**:
```json
{
  "Menu:Customer": "Quản lý khách hàng",
  "Menu:ResCustomer": "Danh sách khách hàng",
  "Permission:ResCustomer": "Quản lý khách hàng",
  "Permission:Create": "Tạo mới",
  "Permission:Edit": "Sửa",
  "Permission:Delete": "Xóa",
  "Permission:View": "Xem",
  "ResCustomer:Code": "Mã khách hàng",
  "ResCustomer:Name": "Tên khách hàng",
  "ResCustomer:CodeExists": "Mã khách hàng {Code} đã tồn tại",
  "ResCustomer:CodeInvalid": "Mã khách hàng chỉ được chứa chữ cái in hoa (A-Z), số (0-9) và dấu gạch dưới (_)",
  "ResCustomer:CreatedSuccessfully": "Tạo khách hàng thành công",
  "ResCustomer:UpdatedSuccessfully": "Cập nhật khách hàng thành công",
  "ResCustomer:DeletedSuccessfully": "Xóa khách hàng thành công"
}
```

---

### 6. HTTP API Controllers

#### 6.1. Controller
**Location**: `modules/customer/src/iOne.Customer.HttpApi/Controllers/ResCustomerController.cs`

```csharp
using iOne.Customer;
using iOne.Customer.Permissions;
using iOne.Customer.ResCustomers;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.AspNetCore.Mvc;

namespace iOne.Customer.Controllers;

[RemoteService(Name = CustomerRemoteServiceConsts.RemoteServiceName)]
[Area(CustomerRemoteServiceConsts.ModuleName)]
[Route("api/customer/res-customers")]
[Authorize]
public class ResCustomerController : AbpControllerBase
{
    protected IResCustomerAppService AppService { get; }

    public ResCustomerController(IResCustomerAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(ResCustomerPermissions.View)]
    public virtual Task<PagedResultDto<ResCustomerDto>> GetListAsync(GetResCustomersInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(ResCustomerPermissions.View)]
    public virtual Task<ResCustomerDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(ResCustomerPermissions.Create)]
    public virtual Task<ResCustomerDto> CreateAsync(CreateResCustomerDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(ResCustomerPermissions.Edit)]
    public virtual Task<ResCustomerDto> UpdateAsync(Guid id, UpdateResCustomerDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(ResCustomerPermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }
}
```

---

#### 6.2. Exclude từ Conventional Controllers
**Location**: `modules/customer/src/iOne.Customer.HttpApi/iOneCustomerHttpApiModule.cs`

**QUAN TRỌNG**: Phải exclude `ResCustomerAppService` khỏi conventional controller generation:

```csharp
Configure<AbpAspNetCoreMvcOptions>(options =>
{
    options.ConventionalControllers.Create(
        typeof(iOneCustomerApplicationModule).Assembly,
        opts =>
        {
            opts.TypePredicate = type => 
                type.Name != "ResIndustryAppService" &&
                type.Name != "ResCustomerAppService"; // Exclude ResCustomerAppService
        });
});
```

---

### 7. Menu Configuration

#### 7.1. Menu Contributor
**Location**: `modules/customer/src/iOne.Customer.Application/Navigation/CustomerMenuContributor.cs`

**Cập nhật** để thêm menu item cho ResCustomer:

```csharp
resCustomerMenuItem.AddItem(new ApplicationMenuItem(
    "Customer.ResCustomer",
    customerL["Menu:ResCustomer"],
    url: "~/pages/customer/res-customers",
    icon: "pi pi-fw pi-users"
).RequirePermissions(ResCustomerPermissions.Default));
```

---

## ✅ Checklist Trước Khi Hoàn Thành

- [ ] **Domain Layer**
  - [ ] Enum `ResCustomerStatus` đã tạo
  - [ ] Enum `ResCustomerSex` đã tạo
  - [ ] Entity `ResCustomer` đã tạo với đầy đủ properties
  - [ ] Table name sử dụng `[Table("res_customer")]` (snake_case)
  - [ ] Validation Code: Regex `^[A-Z0-9_]+$`
  - [ ] Method `UpdateFullAddress()` để tự động nối address
  - [ ] Method `UpdateInvoiceFullAddress()` để tự động nối invoice address
  - [ ] Repository interface `IResCustomerRepository` đã tạo

- [ ] **Application Layer**
  - [ ] DTOs đã tạo: `ResCustomerDto`, `CreateResCustomerDto`, `UpdateResCustomerDto`, `GetResCustomersInput`
  - [ ] `UpdateResCustomerDto` KHÔNG có field `Code` (immutable)
  - [ ] Application Service interface `IResCustomerAppService` đã tạo
  - [ ] Application Service implementation `ResCustomerAppService` đã tạo
  - [ ] Logic tự động tính FullAddress trong CreateAsync và UpdateAsync
  - [ ] Logic tự động tính InvoiceFullAddress trong CreateAsync và UpdateAsync
  - [ ] Validation Code unique (nếu có)
  - [ ] Validation Foreign keys tồn tại
  - [ ] **AutoMapper configuration đã được thêm**

- [ ] **Permissions**
  - [ ] `ResCustomerPermissions` đã tạo
  - [ ] `ResCustomerPermissionDefinitionProvider` đã tạo
  - [ ] Permissions đã được sử dụng trong Controller và AppService

- [ ] **EF Core Configuration**
  - [ ] `ResCustomerConfiguration` đã tạo
  - [ ] Table name: `"res_customer"` (snake_case)
  - [ ] Tất cả column names: snake_case
  - [ ] Status enum conversion: lowercase ("active", "deactive")
  - [ ] Sex enum conversion: uppercase ("M", "F")
  - [ ] Foreign keys với tên snake_case: `fk_res_customer_xxx`
  - [ ] Index `ix_res_customer_code` (unique, nullable)
  - [ ] Repository implementation `EfCoreResCustomerRepository` đã tạo
  - [ ] Đã register trong `iOneDbContext`

- [ ] **Migration**
  - [ ] Migration đã được tạo
  - [ ] Sử dụng `IF EXISTS`/`IF NOT EXISTS`
  - [ ] Table và column names: snake_case
  - [ ] Index names: snake_case với prefix

- [ ] **Localization**
  - [ ] Keys đã thêm vào `vi-VN.json`
  - [ ] Keys đã thêm vào `en.json`
  - [ ] Keys phải match nhau

- [ ] **HTTP API**
  - [ ] Controller `ResCustomerController` đã tạo
  - [ ] Route: `"api/customer/res-customers"`
  - [ ] Có `[RemoteService]` và `[Area]` attributes
  - [ ] Có `[Authorize]` với permissions
  - [ ] **Đã exclude `ResCustomerAppService` khỏi conventional controllers**

- [ ] **Menu**
  - [ ] Menu item đã được thêm vào `CustomerMenuContributor`
  - [ ] Menu item có permission check

- [ ] **Testing**
  - [ ] Build solution thành công
  - [ ] Test API endpoints thành công
  - [ ] Test FullAddress tự động tính đúng
  - [ ] Test Code validation (A-Z, 0-9, _)
  - [ ] Test Code unique
  - [ ] Test Code immutable khi update
  - [ ] Test conditional fields dựa trên organization_type

---

## 🔍 Lưu Ý Quan Trọng

1. **Code Immutable**: 
   - `UpdateResCustomerDto` KHÔNG có field `Code`
   - Trong `UpdateAsync`, không được phép update Code
   - Frontend phải disable field Code khi edit

2. **FullAddress Auto-Generated**:
   - FullAddress = `"{Address}, {WardName}, {ProvinceName}"`
   - InvoiceFullAddress = `"{InvoiceAddress}, {InvoiceWardName}, {InvoiceProvinceName}"` (nếu có)
   - Frontend phải disable field FullAddress và InvoiceFullAddress
   - Logic tính FullAddress phải được thực hiện trong AppService (CreateAsync và UpdateAsync)

3. **Conditional Fields**:
   - Logic hiển thị/ẩn field dựa trên `organization_type.Type` phải được xử lý ở **Frontend**
   - Backend chỉ cần validate và lưu dữ liệu
   - DTO phải có field `OrganizationTypeType` để frontend biết loại tổ chức

4. **Foreign Keys**:
   - Tất cả foreign keys phải có validation trong AppService
   - Sử dụng `ON DELETE RESTRICT` để đảm bảo data integrity

5. **Enum Conversions**:
   - Status: lowercase ("active", "deactive")
   - Sex: uppercase ("M", "F")

6. **Database Naming**:
   - Tất cả database objects (tables, columns, indexes, constraints) phải theo **snake_case**

---

## 📝 Ghi Chú Implementation

### Logic FullAddress trong AppService

```csharp
public async Task<ResCustomerDto> CreateAsync(CreateResCustomerDto input)
{
    // Validation
    if (!string.IsNullOrWhiteSpace(input.Code) && await _repository.AnyAsync(x => x.Code == input.Code))
    {
        throw new UserFriendlyException(
            _localizer["ResCustomer:CodeExists", new { Code = input.Code }]
        );
    }

    // Load navigation properties để lấy tên
    var province = await _provinceRepository.GetAsync(input.ProvinceId);
    var ward = await _wardRepository.GetAsync(input.WardId);
    
    // Tạo entity
    var entity = new ResCustomer(
        GuidGenerator.Create(),
        input.Code,
        input.Name,
        input.ProvinceId,
        input.WardId,
        input.Address,
        input.Phone,
        input.Status
        // ... các properties khác
    );
    
    // Tự động tính FullAddress
    entity.UpdateFullAddress(ward.Name, province.Name);
    
    // Tự động tính InvoiceFullAddress (nếu có)
    if (input.InvoiceProvinceId.HasValue && input.InvoiceWardId.HasValue && !string.IsNullOrWhiteSpace(input.InvoiceAddress))
    {
        var invoiceProvince = await _provinceRepository.GetAsync(input.InvoiceProvinceId.Value);
        var invoiceWard = await _wardRepository.GetAsync(input.InvoiceWardId.Value);
        entity.UpdateInvoiceFullAddress(invoiceWard.Name, invoiceProvince.Name);
    }
    
    await _repository.InsertAsync(entity);
    
    // Map to DTO và load navigation properties
    var dto = ObjectMapper.Map<ResCustomer, ResCustomerDto>(entity);
    dto.ProvinceName = province.Name;
    dto.WardName = ward.Name;
    // ... load các navigation properties khác
    
    return dto;
}
```

---

**Kết thúc Plan**

