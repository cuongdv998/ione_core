# Plan Lập Trình Backend - Quản lý Đối tác (ResPartner)

## 1. Tổng Quan

### 1.1. Mô Tả
Chức năng quản lý đối tác với các tính năng:
- Tìm kiếm, thêm mới, sửa, xóa đối tác
- Quản lý điều khoản hợp tác (Agreements) trong cùng domain
- Conditional fields dựa trên OrganizationType (CN/TC)
- FullAddress tự động tính từ Address + Ward + Province

### 1.2. Domain Structure
- **Aggregate Root**: `ResPartner`
- **Entity trong Aggregate**: `ResPartnerAgreement`
- **Foreign Keys**:
  - `PartnerTypeId` → `ResPartnerType`
  - `OrganizationTypeId` → `ResOrganizationType`
  - `ProvinceId` → `ResProvince`
  - `WardId` → `ResWard`
  - `ChannelId` → `ResChannel` (nullable)
  - `InvoiceProvinceId` → `ResProvince` (nullable)
  - `InvoiceWardId` → `ResWard` (nullable)
  - `ResPartnerAgreement.AgreementTermId` → `ResAgreementTerm`

### 1.3. Business Rules
1. **Code Validation**: 
   - Unique constraint
   - Chỉ cho phép A-Z, 0-9, _ (uppercase)
   - Immutable khi edit

2. **FullAddress**:
   - Tự động tính: `<Address>, <Ward Name>, <Province Name>`
   - Không cho phép user nhập (computed field)
   - Có thể lưu vào DB hoặc tính toán khi query

3. **PartnerRole**:
   - Lấy từ `AdminConfig` với `code = 'PARTNER_ROLE'`
   - `partner_role = admin_config.sub_code`
   - Giá trị hiển thị trên giao diện cho người dùng chọn sẽ là admin_config.value tương ứng với sub_code, khi insert db thì lấy sub_code

4. **Conditional Fields** (dựa trên `OrganizationType.Type`):
   - **CN (Cá nhân)**: 
     - Hiển thị: `IdNo`
     - Ẩn: `Tin`, `RepName`, `RepEmail`, `RepPhone`, `RepIdNo`, `RepTitle`, `Authorizer`, `AuthorizerPhone`, `AuthorizerEmail`, `AuthorizerNo`, `AuthorizerDate`, `AuthorizerTitle`, `BusinessNo`
   - **TC (Tổ chức)**:
     - Ẩn: `IdNo`
     - Hiển thị: `Tin`, `RepName`, `RepEmail`, `RepPhone`, `RepIdNo`, `RepTitle`, `Authorizer`, `AuthorizerPhone`, `AuthorizerEmail`, `AuthorizerNo`, `AuthorizerDate`, `AuthorizerTitle`, `BusinessNo`

5. **Agreements Validation**:
   - Cùng `AgreementTermId` không được trùng khoảng ngày hiệu lực
   - Validation: `(EffectDate, ExpireDate)` không overlap với các agreement khác cùng `AgreementTermId`

6. **Soft Delete**:
   - Delete trước → Set Status = Deactive sau
   - Đảm bảo ABP audit log được trigger

## 2. Database Schema

### 2.1. Table: `res_partner` (snake_case)

| Column Name (snake_case) | Type | Constraints | Description |
|-------------------------|------|-------------|-------------|
| `id` | UUID | PRIMARY KEY | ID đối tác |
| `channel_id` | UUID | NULL, FK → `res_channel` | Kênh phân phối |
| `partner_type_id` | UUID | NOT NULL, FK → `res_partner_type` | Loại đối tác |
| `partner_role` | VARCHAR(15) | NULL | Vai trò (từ AdminConfig) |
| `code` | VARCHAR(25) | NULL, UNIQUE | Mã đối tác (A-Z, 0-9, _) |
| `name` | VARCHAR(250) | NOT NULL | Tên đối tác |
| `province_id` | UUID | NOT NULL, FK → `res_province` | Tỉnh/thành |
| `ward_id` | UUID | NOT NULL, FK → `res_ward` | Phường/xã |
| `address` | VARCHAR(250) | NOT NULL | Địa chỉ chi tiết |
| `full_address` | VARCHAR(500) | NOT NULL | Địa chỉ đầy đủ (computed) |
| `email` | VARCHAR(50) | NULL | Email |
| `phone` | VARCHAR(15) | NOT NULL | Số điện thoại |
| `note` | VARCHAR(500) | NULL | Ghi chú |
| `status` | VARCHAR(10) | NOT NULL | Trạng thái (active/deactive) |
| `tin` | VARCHAR(50) | NULL | Mã số thuế (TC only) |
| `id_no` | VARCHAR(25) | NULL | Số CCCD (CN only) |
| `rep_name` | VARCHAR(250) | NULL | Tên người đại diện (TC only) |
| `rep_email` | VARCHAR(50) | NULL | Email người đại diện (TC only) |
| `rep_phone` | VARCHAR(15) | NULL | SĐT người đại diện (TC only) |
| `rep_id_no` | VARCHAR(25) | NULL | CCCD người đại diện (TC only) |
| `rep_title` | VARCHAR(250) | NULL | Chức danh người đại diện (TC only) |
| `authorizer` | VARCHAR(50) | NULL | Người ủy quyền (TC only) |
| `authorizer_phone` | VARCHAR(15) | NULL | SĐT người ủy quyền (TC only) |
| `authorizer_email` | VARCHAR(50) | NULL | Email người ủy quyền (TC only) |
| `authorizer_no` | VARCHAR(25) | NULL | Số ủy quyền (TC only) |
| `authorizer_date` | DATE | NULL | Ngày ủy quyền (TC only) |
| `authorizer_title` | VARCHAR(50) | NULL | Chức danh người ủy quyền (TC only) |
| `business_no` | VARCHAR(25) | NULL | Số giấy phép kinh doanh (TC only) |
| `organization_type_id` | UUID | NULL, FK → `res_organization_type` | Loại tổ chức |
| `invoice_province_id` | UUID | NULL, FK → `res_province` | Tỉnh xuất hóa đơn |
| `invoice_ward_id` | UUID | NULL, FK → `res_ward` | Phường/xã xuất hóa đơn |
| `invoice_address` | VARCHAR(250) | NULL | Địa chỉ xuất hóa đơn |
| `invoice_full_address` | VARCHAR(500) | NULL | Địa chỉ đầy đủ xuất hóa đơn (computed) |
| `creation_time` | TIMESTAMP | NOT NULL | Thời gian tạo |
| `creator_id` | UUID | NULL | ID người tạo |
| `last_modification_time` | TIMESTAMP | NULL | Thời gian sửa cuối |
| `last_modifier_id` | UUID | NULL | ID người sửa cuối |
| `is_deleted` | BOOLEAN | NOT NULL, DEFAULT false | Soft delete |
| `deletion_time` | TIMESTAMP | NULL | Thời gian xóa |
| `deleter_id` | UUID | NULL | ID người xóa |
| `concurrency_stamp` | VARCHAR(40) | NULL | Concurrency stamp |

### 2.2. Table: `res_partner_agreement` (snake_case)

| Column Name (snake_case) | Type | Constraints | Description |
|-------------------------|------|-------------|-------------|
| `id` | UUID | PRIMARY KEY | ID điều khoản |
| `partner_id` | UUID | NOT NULL, FK → `res_partner` | ID đối tác |
| `agreement_term_id` | UUID | NOT NULL, FK → `res_agreement_term` | ID điều khoản hợp tác |
| `value` | VARCHAR(50) | NOT NULL | Giá trị thỏa thuận |
| `effect_date` | DATE | NULL | Ngày hiệu lực |
| `expire_date` | DATE | NOT NULL | Ngày hết hiệu lực |
| `creation_time` | TIMESTAMP | NOT NULL | Thời gian tạo |
| `creator_id` | UUID | NULL | ID người tạo |
| `last_modification_time` | TIMESTAMP | NULL | Thời gian sửa cuối |
| `last_modifier_id` | UUID | NULL | ID người sửa cuối |
| `concurrency_stamp` | VARCHAR(40) | NULL | Concurrency stamp |

**Lưu ý**: `res_partner_agreement` KHÔNG có soft delete (không kế thừa `FullAuditedAggregateRoot`)

### 2.3. Indexes
- **Primary Keys**: 
  - `pk_res_partner` (on `id`)
  - `pk_res_partner_agreement` (on `id`)
- **Unique Indexes**:
  - `ix_res_partner_code` (on `code`, unique, where `code IS NOT NULL`)
- **Foreign Key Indexes**:
  - `ix_res_partner_channel_id` (on `channel_id`)
  - `ix_res_partner_partner_type_id` (on `partner_type_id`)
  - `ix_res_partner_organization_type_id` (on `organization_type_id`)
  - `ix_res_partner_province_id` (on `province_id`)
  - `ix_res_partner_ward_id` (on `ward_id`)
  - `ix_res_partner_invoice_province_id` (on `invoice_province_id`)
  - `ix_res_partner_invoice_ward_id` (on `invoice_ward_id`)
  - `ix_res_partner_agreement_partner_id` (on `partner_id`)
  - `ix_res_partner_agreement_agreement_term_id` (on `agreement_term_id`)

## 3. Domain Layer

### 3.1. Enum: `ResPartnerStatus`

**Location**: `src/common/domain/iOne.Domain.Shared/ResPartners/ResPartnerStatus.cs`

```csharp
namespace iOne.ResPartners;

public enum ResPartnerStatus
{
    Active = 0,    // Hoạt động
    Deactive = 1   // Không hoạt động
}
```

### 3.2. Entity: `ResPartner`

**Location**: `src/common/domain/iOne.Domain/ResPartners/ResPartner.cs`

**Đặc điểm**:
- Kế thừa `FullAuditedAggregateRoot<Guid>`
- Table name: `[Table("res_partner")]` (snake_case)
- Properties có private setters
- `Code` immutable (chỉ set trong constructor)
- `FullAddress` và `InvoiceFullAddress` là computed properties (có thể lưu vào DB hoặc tính toán)

**Properties**:
```csharp
public class ResPartner : FullAuditedAggregateRoot<Guid>
{
    // Foreign Keys
    public virtual Guid? ChannelId { get; private set; }
    public virtual Guid PartnerTypeId { get; private set; }
    public virtual string? PartnerRole { get; private set; } // Từ AdminConfig
    public virtual Guid? OrganizationTypeId { get; private set; }
    public virtual Guid ProvinceId { get; private set; }
    public virtual Guid WardId { get; private set; }
    public virtual Guid? InvoiceProvinceId { get; private set; }
    public virtual Guid? InvoiceWardId { get; private set; }
    
    // Basic Info
    public virtual string? Code { get; private set; } // Unique, immutable
    public virtual string Name { get; private set; }
    public virtual string Address { get; private set; }
    public virtual string FullAddress { get; private set; } // Computed
    public virtual string? Email { get; private set; }
    public virtual string Phone { get; private set; }
    public virtual string? Note { get; private set; }
    public virtual ResPartnerStatus Status { get; private set; }
    
    // Invoice Address
    public virtual string? InvoiceAddress { get; private set; }
    public virtual string? InvoiceFullAddress { get; private set; } // Computed
    
    // CN (Cá nhân) fields
    public virtual string? IdNo { get; private set; } // CCCD
    
    // TC (Tổ chức) fields
    public virtual string? Tin { get; private set; } // Mã số thuế
    public virtual string? RepName { get; private set; }
    public virtual string? RepEmail { get; private set; }
    public virtual string? RepPhone { get; private set; }
    public virtual string? RepIdNo { get; private set; }
    public virtual string? RepTitle { get; private set; }
    public virtual string? Authorizer { get; private set; }
    public virtual string? AuthorizerPhone { get; private set; }
    public virtual string? AuthorizerEmail { get; private set; }
    public virtual string? AuthorizerNo { get; private set; }
    public virtual DateTime? AuthorizerDate { get; private set; }
    public virtual string? AuthorizerTitle { get; private set; }
    public virtual string? BusinessNo { get; private set; }
    
    // Navigation Properties
    public virtual ResChannels.ResChannel? Channel { get; set; }
    public virtual ResPartnerTypes.ResPartnerType PartnerType { get; set; }
    public virtual ResOrganizationTypes.ResOrganizationType? OrganizationType { get; set; }
    public virtual ResProvinces.ResProvince Province { get; set; }
    public virtual ResWards.ResWard Ward { get; set; }
    public virtual ResProvinces.ResProvince? InvoiceProvince { get; set; }
    public virtual ResWards.ResWard? InvoiceWard { get; set; }
    
    // Collection
    public virtual ICollection<ResPartnerAgreement> Agreements { get; set; }
    
    // Methods
    public virtual void UpdateName(string name) { ... }
    public virtual void UpdateFullAddress(string fullAddress) { ... } // Computed
    // ... other update methods
    // ⚠️ KHÔNG có UpdateCode() - Code immutable
}
```

### 3.3. Entity: `ResPartnerAgreement`

**Location**: `src/common/domain/iOne.Domain/ResPartners/ResPartnerAgreement.cs`

**Đặc điểm**:
- Kế thừa `AuditedEntity<Guid>` (KHÔNG có soft delete)
- Table name: `[Table("res_partner_agreement")]` (snake_case)
- Là Entity trong Aggregate của `ResPartner`

**Properties**:
```csharp
public class ResPartnerAgreement : AuditedEntity<Guid>
{
    public virtual Guid PartnerId { get; private set; }
    public virtual Guid AgreementTermId { get; private set; }
    public virtual string Value { get; private set; }
    public virtual DateTime? EffectDate { get; private set; }
    public virtual DateTime ExpireDate { get; private set; }
    
    // Navigation Properties
    public virtual ResPartner Partner { get; set; }
    public virtual ResAgreementTerms.ResAgreementTerm AgreementTerm { get; set; }
    
    // Methods
    public virtual void UpdateValue(string value) { ... }
    public virtual void UpdateEffectDate(DateTime? effectDate) { ... }
    public virtual void UpdateExpireDate(DateTime expireDate) { ... }
}
```

### 3.4. Repository Interface: `IResPartnerRepository`

**Location**: `src/common/domain/iOne.Domain/ResPartners/IResPartnerRepository.cs`

```csharp
public interface IResPartnerRepository : IRepository<ResPartner, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null);
    Task<ResPartner?> FindByCodeAsync(string code);
    Task<List<ResPartner>> GetListWithAgreementsAsync(GetResPartnersInput input);
}
```

### 3.5. Manager: `ResPartnerManager`

**Location**: `src/common/domain/iOne.Domain/ResPartners/ResPartnerManager.cs`

**Business Logic**:
- Validate Code uniqueness
- Validate Code format (A-Z, 0-9, _)
- Validate Agreements date ranges (không overlap cùng AgreementTermId)
- Compute FullAddress từ Address + Ward + Province
- Validate conditional fields dựa trên OrganizationType.Type

**Methods**:
```csharp
public class ResPartnerManager : DomainService
{
    public virtual async Task CreateAsync(ResPartner partner) { ... }
    public virtual async Task UpdateAsync(ResPartner partner, ...) { ... }
    public virtual async Task ValidateAgreementsAsync(
        Guid partnerId, 
        List<ResPartnerAgreement> agreements) { ... }
    public virtual string ComputeFullAddress(
        string address, 
        string wardName, 
        string provinceName) { ... }
}
```

## 4. Application Layer

### 4.1. DTOs

**Location**: `modules/partner/src/iOne.Partner.Application.Contracts/ResPartners/`

#### 4.1.1. `ResPartnerDto`
- Kế thừa `FullAuditedEntityDto<Guid>`
- Chứa tất cả fields + navigation properties (Name fields)
- `FullAddress` và `InvoiceFullAddress` (computed)

#### 4.1.2. `CreateResPartnerDto`
- Chứa tất cả fields cần thiết để tạo mới
- **KHÔNG** có `FullAddress` và `InvoiceFullAddress` (tự động tính)
- Có `Agreements` collection (optional)

#### 4.1.3. `UpdateResPartnerDto`
- **KHÔNG** có `Code` (immutable)
- **KHÔNG** có `FullAddress` và `InvoiceFullAddress` (tự động tính)
- Có `Agreements` collection để update

#### 4.1.4. `ResPartnerAgreementDto`
- Kế thừa `AuditedEntityDto<Guid>`
- Chứa: `PartnerId`, `AgreementTermId`, `Value`, `EffectDate`, `ExpireDate`
- Có `AgreementTermName` (navigation property)

#### 4.1.5. `CreateResPartnerAgreementDto`
- Chứa: `AgreementTermId`, `Value`, `EffectDate`, `ExpireDate`

#### 4.1.6. `UpdateResPartnerAgreementDto`
- Chứa: `Value`, `EffectDate`, `ExpireDate`

#### 4.1.7. `GetResPartnersInput`
- Kế thừa `PagedAndSortedResultRequestDto`
- Filters: `Code`, `Name`, `PartnerTypeId`, `OrganizationTypeId`, `ProvinceId`, `WardId`, `Status`, `PartnerRole`

### 4.2. Application Service Interface: `IResPartnerAppService`

**Location**: `modules/partner/src/iOne.Partner.Application.Contracts/ResPartners/IResPartnerAppService.cs`

```csharp
public interface IResPartnerAppService : ICrudAppService<
    ResPartnerDto,
    Guid,
    GetResPartnersInput,
    CreateResPartnerDto,
    UpdateResPartnerDto>
{
    // Có thể thêm methods riêng nếu cần
}
```

### 4.3. Application Service Implementation: `ResPartnerAppService`

**Location**: `modules/partner/src/iOne.Partner.Application/ResPartners/ResPartnerAppService.cs`

**Đặc điểm**:
- Override `CreateAsync`, `UpdateAsync`, `DeleteAsync`, `GetAsync`, `GetListAsync`
- Compute `FullAddress` và `InvoiceFullAddress` trước khi save
- Validate Agreements date ranges
- Load navigation properties (Ward, Province) để compute FullAddress
- Eager load Agreements khi GetAsync

**Methods**:
```csharp
public override async Task<ResPartnerDto> CreateAsync(CreateResPartnerDto input)
{
    // 1. Load navigation properties để compute FullAddress
    // 2. Validate Code uniqueness
    // 3. Validate conditional fields dựa trên OrganizationType
    // 4. Create entity với computed FullAddress
    // 5. Validate và add Agreements
    // 6. Save
}

public override async Task<ResPartnerDto> UpdateAsync(Guid id, UpdateResPartnerDto input)
{
    // 1. Load entity với Agreements
    // 2. Load navigation properties để compute FullAddress
    // 3. Validate conditional fields
    // 4. Update entity (KHÔNG update Code)
    // 5. Update Agreements (validate date ranges)
    // 6. Save
}

protected override async Task<IQueryable<ResPartner>> CreateFilteredQueryAsync(GetResPartnersInput input)
{
    // Filter với eager load navigation properties
    // Include: PartnerType, OrganizationType, Province, Ward, Channel, Agreements
}
```

### 4.4. AutoMapper Configuration

**Location**: `modules/partner/src/iOne.Partner.Application/iOnePartnerApplicationAutoMapperProfile.cs`

**Mappings**:
```csharp
// ResPartner mappings
CreateMap<ResPartner, ResPartnerDto>();
CreateMap<CreateResPartnerDto, ResPartner>();
CreateMap<UpdateResPartnerDto, ResPartner>();

// ResPartnerAgreement mappings
CreateMap<ResPartnerAgreement, ResPartnerAgreementDto>();
CreateMap<CreateResPartnerAgreementDto, ResPartnerAgreement>();
CreateMap<UpdateResPartnerAgreementDto, ResPartnerAgreement>();
```

## 5. Permissions

### 5.1. Permission Constants

**Location**: `modules/partner/src/iOne.Partner.Application.Contracts/Permissions/ResPartnerPermissions.cs`

```csharp
public static class ResPartnerPermissions
{
    public const string GroupName = "PartnerResPartner";
    public const string Default = GroupName;
    public const string Create = Default + ".Create";
    public const string Edit = Default + ".Edit";
    public const string Delete = Default + ".Delete";
    public const string View = Default + ".View";
}
```

### 5.2. Permission Definition Provider

**Location**: `modules/partner/src/iOne.Partner.Application.Contracts/Permissions/ResPartnerPermissionDefinitionProvider.cs`

## 6. Entity Framework Core

### 6.1. Entity Configuration: `ResPartnerConfiguration`

**Location**: `src/common/infra/iOne.EntityFrameworkCore/ResPartners/ResPartnerConfiguration.cs`

**Đặc điểm**:
- Table name: `"res_partner"` (snake_case)
- Tất cả column names: snake_case
- Foreign keys với `OnDelete(DeleteBehavior.Restrict)`
- Unique index trên `code` (where `code IS NOT NULL`)
- Indexes cho tất cả foreign keys

**Configuration**:
```csharp
builder.ToTable("res_partner", t => { t.HasComment("Bảng thông tin đối tác"); });

// Foreign Keys
builder.HasOne(x => x.Channel)
    .WithMany()
    .HasForeignKey(x => x.ChannelId)
    .HasConstraintName("fk_res_partner_res_channel_channel_id")
    .OnDelete(DeleteBehavior.Restrict);

// ... other foreign keys

// Unique Index
builder.HasIndex(e => e.Code, "ix_res_partner_code")
    .IsUnique()
    .HasFilter("\"code\" IS NOT NULL");
```

### 6.2. Entity Configuration: `ResPartnerAgreementConfiguration`

**Location**: `src/common/infra/iOne.EntityFrameworkCore/ResPartners/ResPartnerAgreementConfiguration.cs`

**Đặc điểm**:
- Table name: `"res_partner_agreement"` (snake_case)
- Foreign keys với `OnDelete(DeleteBehavior.Restrict)`
- Indexes cho foreign keys

### 6.3. Repository Implementation: `EfCoreResPartnerRepository`

**Location**: `src/common/infra/iOne.EntityFrameworkCore/ResPartners/EfCoreResPartnerRepository.cs`

**Methods**:
- `IsCodeExistsAsync`
- `FindByCodeAsync`
- `GetListWithAgreementsAsync` (eager load Agreements)

## 7. HTTP API

### 7.1. Controller: `ResPartnerController`

**Location**: `modules/partner/src/iOne.Partner.HttpApi/Controllers/ResPartnerController.cs`

**Endpoints**:
- `GET /api/partner/res-partners` - GetList
- `GET /api/partner/res-partners/{id}` - Get
- `POST /api/partner/res-partners` - Create
- `PUT /api/partner/res-partners/{id}` - Update
- `DELETE /api/partner/res-partners/{id}` - Delete

**Authorization**: Sử dụng `ResPartnerPermissions`

### 7.2. Exclude từ Conventional Controllers

**Location**: `modules/partner/src/iOne.Partner.HttpApi/iOnePartnerHttpApiModule.cs`

Exclude `ResPartnerAppService` khỏi conventional controller generation.

## 8. Localization

### 8.1. Vietnamese (`vi-VN.json`)

**Location**: `modules/partner/src/iOne.Partner.Application.Contracts/Localization/Partner/vi-VN.json`

**Keys cần thêm**:
```json
{
  "Menu:ResPartner": "Danh mục Đối tác",
  "Permission:ResPartner": "Đối tác",
  "ResPartner:Code": "Mã đối tác",
  "ResPartner:Name": "Tên đối tác",
  "ResPartner:PartnerType": "Loại đối tác",
  "ResPartner:PartnerRole": "Vai trò",
  "ResPartner:OrganizationType": "Loại tổ chức",
  "ResPartner:Province": "Tỉnh/Thành",
  "ResPartner:Ward": "Phường/Xã",
  "ResPartner:Address": "Địa chỉ",
  "ResPartner:FullAddress": "Địa chỉ đầy đủ",
  "ResPartner:Email": "Email",
  "ResPartner:Phone": "Số điện thoại",
  "ResPartner:Status": "Trạng thái",
  "ResPartner:IdNo": "Số CCCD",
  "ResPartner:Tin": "Mã số thuế",
  "ResPartner:RepName": "Tên người đại diện",
  "ResPartner:RepEmail": "Email người đại diện",
  "ResPartner:RepPhone": "SĐT người đại diện",
  "ResPartner:RepIdNo": "CCCD người đại diện",
  "ResPartner:RepTitle": "Chức danh người đại diện",
  "ResPartner:Authorizer": "Người ủy quyền",
  "ResPartner:AuthorizerPhone": "SĐT người ủy quyền",
  "ResPartner:AuthorizerEmail": "Email người ủy quyền",
  "ResPartner:AuthorizerNo": "Số ủy quyền",
  "ResPartner:AuthorizerDate": "Ngày ủy quyền",
  "ResPartner:AuthorizerTitle": "Chức danh người ủy quyền",
  "ResPartner:BusinessNo": "Số giấy phép kinh doanh",
  "ResPartner:InvoiceProvince": "Tỉnh xuất hóa đơn",
  "ResPartner:InvoiceWard": "Phường/Xã xuất hóa đơn",
  "ResPartner:InvoiceAddress": "Địa chỉ xuất hóa đơn",
  "ResPartner:InvoiceFullAddress": "Địa chỉ đầy đủ xuất hóa đơn",
  "ResPartner:CodeRequired": "Mã đối tác là bắt buộc",
  "ResPartner:CodeInvalid": "Mã đối tác chỉ được chứa chữ cái (A-Z), số (0-9) và dấu gạch dưới (_)",
  "ResPartner:CodeExists": "Mã đối tác '{Code}' đã tồn tại",
  "ResPartner:CodeCannotBeChanged": "Mã đối tác không được phép thay đổi",
  "ResPartner:AgreementDateOverlap": "Các điều khoản cùng loại không được trùng khoảng ngày hiệu lực",
  "ResPartner:CreatedSuccessfully": "Tạo đối tác thành công",
  "ResPartner:UpdatedSuccessfully": "Cập nhật đối tác thành công",
  "ResPartner:DeletedSuccessfully": "Xóa đối tác thành công"
}
```

### 8.2. English (`en.json`)

Tương tự với bản dịch tiếng Anh.

## 9. Menu Configuration

### 9.1. Menu Contributor

**Location**: `modules/partner/src/iOne.Partner.Application/Navigation/PartnerMenuContributor.cs`

Thêm menu item:
```csharp
partnerMenuItem.AddItem(new ApplicationMenuItem(
    "Partner.ResPartner",
    partnerL["Menu:ResPartner"],
    url: "~/pages/partner/res-partners",
    icon: "pi pi-fw pi-users"
).RequirePermissions(ResPartnerPermissions.Default));
```

## 10. Migration

### 10.1. Migration Script

**Quy tắc**:
- Sử dụng `IF EXISTS`/`IF NOT EXISTS`
- Tất cả names theo snake_case
- Foreign keys với `ON DELETE RESTRICT`
- Unique index trên `code` với filter `WHERE code IS NOT NULL`

## 11. Checklist

### Domain Layer
- [ ] `ResPartnerStatus` enum đã tạo
- [ ] `ResPartner` entity đã tạo với đầy đủ properties
- [ ] `ResPartnerAgreement` entity đã tạo
- [ ] `IResPartnerRepository` interface đã tạo
- [ ] `ResPartnerManager` đã tạo với business logic:
  - [ ] Code uniqueness validation
  - [ ] Code format validation
  - [ ] Agreements date range validation
  - [ ] FullAddress computation
  - [ ] Conditional fields validation

### Application Layer
- [ ] DTOs đã tạo đầy đủ
- [ ] `IResPartnerAppService` interface đã tạo
- [ ] `ResPartnerAppService` implementation:
  - [ ] `CreateAsync` với FullAddress computation
  - [ ] `UpdateAsync` (không update Code)
  - [ ] `DeleteAsync` với soft delete logic
  - [ ] `GetListAsync` với filters
  - [ ] `GetAsync` với eager load Agreements
- [ ] AutoMapper configuration đã thêm

### Permissions
- [ ] `ResPartnerPermissions` constants đã tạo
- [ ] `ResPartnerPermissionDefinitionProvider` đã tạo
- [ ] Permissions đã được sử dụng trong AppService và Controller

### EF Core
- [ ] `ResPartnerConfiguration` đã tạo:
  - [ ] Table name: `"res_partner"` (snake_case)
  - [ ] Tất cả column names: snake_case
  - [ ] Foreign keys với `OnDelete(DeleteBehavior.Restrict)`
  - [ ] Unique index trên `code`
  - [ ] Indexes cho foreign keys
- [ ] `ResPartnerAgreementConfiguration` đã tạo
- [ ] `EfCoreResPartnerRepository` implementation đã tạo
- [ ] Configurations đã được register trong DbContext

### HTTP API
- [ ] `ResPartnerController` đã tạo với đầy đủ endpoints
- [ ] Controller có `[Authorize]` attributes
- [ ] `ResPartnerAppService` đã được exclude khỏi conventional controllers

### Localization
- [ ] Keys đã đầy đủ trong `vi-VN.json`
- [ ] Keys đã đầy đủ trong `en.json`

### Menu
- [ ] Menu item đã được thêm vào `PartnerMenuContributor`

### Migration
- [ ] Migration đã được tạo với IF EXISTS/IF NOT EXISTS
- [ ] Tất cả names theo snake_case
- [ ] Migration đã được test

## 12. Lưu Ý Quan Trọng

1. **FullAddress Computation**:
   - Có thể lưu vào DB (tính toán khi Create/Update)
   - Hoặc tính toán khi query (sử dụng computed column hoặc projection)
   - Recommendation: Lưu vào DB để tối ưu query performance

2. **Agreements Validation**:
   - Validate date range overlap trong Manager
   - Business rule: Cùng `AgreementTermId` không được có date ranges overlap
   - Algorithm: Check `(EffectDate, ExpireDate)` không overlap với các agreement khác

3. **Conditional Fields**:
   - Validation ở Application layer dựa trên `OrganizationType.Type`
   - Frontend sẽ handle UI visibility, backend validate data consistency

4. **PartnerRole**:
   - Lấy từ `AdminConfig` với `code = 'PARTNER_ROLE'`
   - Frontend sẽ query AdminConfig để lấy danh sách options
   - Backend validate `partner_role` có tồn tại trong AdminConfig không

5. **Code Immutability**:
   - Code chỉ set trong constructor
   - Không có `UpdateCode()` method
   - Frontend disable Code field khi edit

6. **Aggregate Pattern**:
   - `ResPartner` là Aggregate Root
   - `ResPartnerAgreement` là Entity trong Aggregate
   - Agreements được quản lý thông qua `ResPartner`
   - Khi delete `ResPartner`, cascade delete `ResPartnerAgreement` (hard delete)

