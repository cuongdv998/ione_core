# ResObjectTypeItem Implementation Summary

This document summarizes the complete implementation of the RESOBJECTTYPEITEM entity following the RESCARBRAND pattern.

## Files Created

### 1. Domain Layer

#### Status Enum
- **Path**: `src\common\domain\iOne.Domain.Shared\ResObjectTypeItems\ResObjectTypeItemStatus.cs`
- **Description**: Enum defining Active/Deactive status

#### Domain Entity
- **Path**: `src\common\domain\iOne.Domain\ResObjectTypeItems\ResObjectTypeItem.cs`
- **Features**:
  - Full audit properties (CreationTime, CreatorId, LastModificationTime, LastModifierId, DeletionTime, DeleterId, IsDeleted)
  - Validation for all properties
  - Navigation properties to ResObjectType, ResObjectItemType, and ResUom
  - Immutable Code property
  - Update methods for all mutable properties

#### Repository Interface
- **Path**: `src\common\domain\iOne.Domain\ResObjectTypeItems\IResObjectTypeItemRepository.cs`
- **Methods**:
  - `IsCodeExistsAsync(string code, Guid? excludeId = null)` - Check for duplicate codes

#### Domain Manager
- **Path**: `src\common\domain\iOne.Domain\ResObjectTypeItems\ResObjectTypeItemManager.cs`
- **Methods**:
  - `CreateAsync(ResObjectTypeItem objectTypeItem)` - Creates new entity with duplicate validation
  - `UpdateAsync(...)` - Updates entity properties

### 2. Infrastructure Layer

#### Repository Implementation
- **Path**: `src\common\infra\iOne.EntityFrameworkCore\ResObjectTypeItems\EfCoreResObjectTypeItemRepository.cs`
- **Implementation**: EF Core repository with code existence check

#### Entity Configuration
- **Path**: `src\common\infra\iOne.EntityFrameworkCore\ResObjectTypeItems\ResObjectTypeItemConfiguration.cs`
- **Features**:
  - Table name: `res_object_type_item` (snake_case)
  - Column mappings (all snake_case)
  - Status enum conversion (lowercase in DB)
  - Foreign key relationships with DeleteBehavior.Restrict
  - Unique index on Code
  - Indexes on foreign key columns

### 3. Application Layer - Contracts

#### DTOs
- **Path**: `modules\master\src\iOne.Master.Application.Contracts\ResObjectTypeItems\`
  - `ResObjectTypeItemDto.cs` - Main DTO with all properties
  - `CreateResObjectTypeItemDto.cs` - DTO for creation (with Code)
  - `UpdateResObjectTypeItemDto.cs` - DTO for updates (without Code)
  - `GetResObjectTypeItemsInput.cs` - DTO for filtering and pagination
  - `ImportResObjectTypeItemResultDto.cs` - DTO for import results with error details

#### Application Service Interface
- **Path**: `modules\master\src\iOne.Master.Application.Contracts\ResObjectTypeItems\IResObjectTypeItemAppService.cs`
- **Methods**:
  - CRUD operations (inherited from ICrudAppService)
  - `ImportExcelAsync(byte[] fileBytes)` - Excel import
  - `ExportTemplateAsync()` - Excel template export

### 4. Application Layer - Implementation

#### Application Service
- **Path**: `modules\master\src\iOne.Master.Application\ResObjectTypeItems\ResObjectTypeItemAppService.cs`
- **Features**:
  - **CreateAsync**: Validates code uniqueness and foreign key references
  - **UpdateAsync**: Validates foreign key references (Code immutable)
  - **DeleteAsync**: Soft delete with status update to Deactive
  - **CreateFilteredQueryAsync**: Filtering by Code, Name, ObjectTypeId, ObjectItemType, UomId, Status
  - **ImportExcelAsync**: 
    - Validates required headers (CODE, NAME, OBJECTTYPECODE, UOMCODE)
    - Validates all field lengths and formats
    - Checks for duplicates within file and database
    - Validates foreign key references by code
    - Returns detailed error information per row
  - **ExportTemplateAsync**: Generates Excel template with sample data

### 5. HTTP API Layer

#### Controller
- **Path**: `modules\master\src\iOne.Master.HttpApi\Controllers\ResObjectTypeItemController.cs`
- **Endpoints**:
  - `GET /api/master/object-type-items` - List with pagination/filtering
  - `GET /api/master/object-type-items/{id}` - Get by ID
  - `POST /api/master/object-type-items` - Create
  - `PUT /api/master/object-type-items/{id}` - Update
  - `DELETE /api/master/object-type-items/{id}` - Delete
  - `POST /api/master/object-type-items/import-excel` - Import Excel
  - `GET /api/master/object-type-items/export-template` - Export template

### 6. Permissions

#### Permission Constants
- **Path**: `modules\master\src\iOne.Master.Application.Contracts\Permissions\ResObjectTypeItemPermissions.cs`
- **Permissions**:
  - `MasterResObjectTypeItem` - Default
  - `MasterResObjectTypeItem.Create`
  - `MasterResObjectTypeItem.Edit`
  - `MasterResObjectTypeItem.Delete`
  - `MasterResObjectTypeItem.View`

#### Permission Definition Provider
- **Path**: `modules\master\src\iOne.Master.Application.Contracts\Permissions\ResObjectTypeItemPermissionDefinitionProvider.cs`
- **Description**: Registers permissions in the ABP permission system

### 7. Related Entity Updates

#### Updated Files with Navigation Properties
1. **ResUom** (`src\common\domain\iOne.Domain\ResUoms\ResUom.cs`)
   - Added: `ICollection<ResObjectTypeItem> ObjectTypeItems`

2. **ResObjectType** (`src\common\domain\iOne.Domain\ResObjectTypes\ResObjectType.cs`)
   - Added: `ICollection<ResObjectTypeItem> ObjectTypeItems`

3. **ResObjectItemType** (`src\common\domain\iOne.Domain\ResObjectItemTypes\ResObjectItemType.cs`)
   - Added: `ICollection<ResObjectTypeItem> ObjectTypeItems`

### 8. AutoMapper Configuration

#### Updated File
- **Path**: `modules\master\src\iOne.Master.Application\iOneMasterApplicationAutoMapperProfile.cs`
- **Mappings Added**:
  ```csharp
  CreateMap<ResObjectTypeItem, ResObjectTypeItemDto>();
  CreateMap<CreateResObjectTypeItemDto, ResObjectTypeItem>();
  CreateMap<UpdateResObjectTypeItemDto, ResObjectTypeItem>();
  ```

## Database Schema

```sql
CREATE TABLE res_object_type_item (
    id UUID NOT NULL PRIMARY KEY,
    object_type_id UUID NOT NULL,
    object_item_type UUID NULL,
    code VARCHAR(50) NOT NULL,
    name VARCHAR(250) NOT NULL,
    uom_id UUID NOT NULL,
    description VARCHAR(500) NULL,
    status VARCHAR(15) NOT NULL,
    creation_time TIMESTAMP NOT NULL,
    creator_id UUID NOT NULL,
    last_modification_time TIMESTAMP NULL,
    last_modifier_id UUID NULL,
    deletion_time TIMESTAMP NULL,
    deleter_id UUID NULL,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    concurrency_stamp VARCHAR(40) NULL,
    extra_properties TEXT NULL,
    CONSTRAINT fk_res_object_type_item_uom FOREIGN KEY (uom_id) REFERENCES res_uom(id),
    CONSTRAINT fk_res_object_type_item_object_item_type FOREIGN KEY (object_item_type) REFERENCES res_object_item_type(id),
    CONSTRAINT fk_res_object_type_item_object_type FOREIGN KEY (object_type_id) REFERENCES res_object_type(id)
);

CREATE UNIQUE INDEX ix_res_object_type_item_code ON res_object_type_item(code);
CREATE INDEX ix_res_object_type_item_object_type_id ON res_object_type_item(object_type_id);
CREATE INDEX ix_res_object_type_item_object_item_type ON res_object_type_item(object_item_type);
CREATE INDEX ix_res_object_type_item_uom_id ON res_object_type_item(uom_id);
```

## Import Excel Format

The import function expects an Excel file with the following columns:

| Column | Required | Format | Description |
|--------|----------|--------|-------------|
| CODE | Yes | A-Z, 0-9, _ only (uppercase) | Unique identifier |
| NAME | Yes | Max 250 chars | Display name |
| OBJECTTYPECODE | Yes | Existing code | Reference to ResObjectType |
| OBJECTITEMTYPECODE | No | Existing code | Reference to ResObjectItemType |
| UOMCODE | Yes | Existing code | Reference to ResUom |
| DESCRIPTION | No | Max 500 chars | Optional description |
| STATUS | No | Active/Deactive | Default: Active |

### Import Validation

The import function performs the following validations:
1. **Code**: Required, max 50 chars, A-Z/0-9/_ only, unique in DB and file
2. **Name**: Required, max 250 chars
3. **Description**: Optional, max 500 chars
4. **ObjectTypeCode**: Required, must exist in database
5. **ObjectItemTypeCode**: Optional, must exist in database if provided
6. **UomCode**: Required, must exist in database
7. **Status**: Optional, defaults to Active

## Key Features

1. **Complete CRUD Operations**: All basic operations with proper validation
2. **Foreign Key Validation**: Validates all references before create/update
3. **Import with Validation**: Excel import with detailed error reporting
4. **Export Template**: Generates Excel template for easy data entry
5. **Soft Delete**: Uses ABP's soft delete with status update
6. **Audit Trail**: Full audit logging with CreationTime, CreatorId, etc.
7. **Code Immutability**: Code cannot be changed after creation
8. **Navigation Properties**: Proper EF Core relationships
9. **Permission System**: Integrated with ABP permission system
10. **Localization Ready**: Uses localization keys for all messages

## Next Steps

### Required Actions

1. **Add Localization Entries**: Add the following keys to the localization file:
   - `modules\master\src\iOne.Master.Application.Contracts\Localization\Master\vi-VN.json` (or en.json)
   
   ```json
   "ResObjectTypeItem:Code": "Mã",
   "ResObjectTypeItem:Name": "Tên",
   "ResObjectTypeItem:ObjectTypeId": "Loại đối tượng",
   "ResObjectTypeItem:ObjectItemType": "Loại hạng mục",
   "ResObjectTypeItem:UomId": "Đơn vị tính",
   "ResObjectTypeItem:Description": "Mô tả",
   "ResObjectTypeItem:Status": "Trạng thái",
   "ResObjectTypeItem:CodeRequired": "Mã là bắt buộc",
   "ResObjectTypeItem:CodeMaxLength": "Mã không được vượt quá 50 ký tự",
   "ResObjectTypeItem:CodeInvalid": "Mã chỉ được chứa chữ cái (A-Z), số (0-9) và dấu gạch dưới (_)",
   "ResObjectTypeItem:CodeExists": "Mã {Code} đã tồn tại",
   "ResObjectTypeItem:CodeDuplicateInFile": "Mã {0} bị trùng lặp trong file",
   "ResObjectTypeItem:NameRequired": "Tên là bắt buộc",
   "ResObjectTypeItem:NameMaxLength": "Tên không được vượt quá 250 ký tự",
   "ResObjectTypeItem:DescriptionMaxLength": "Mô tả không được vượt quá 500 ký tự",
   "ResObjectTypeItem:ObjectTypeIdRequired": "Loại đối tượng là bắt buộc",
   "ResObjectTypeItem:UomIdRequired": "Đơn vị tính là bắt buộc",
   "ResObjectTypeItem:StatusRequired": "Trạng thái là bắt buộc",
   "ResObjectTypeItem:ObjectTypeNotFound": "Không tìm thấy loại đối tượng",
   "ResObjectTypeItem:ObjectItemTypeNotFound": "Không tìm thấy loại hạng mục",
   "ResObjectTypeItem:UomNotFound": "Không tìm thấy đơn vị tính",
   "ResObjectTypeItem:ObjectTypeCodeRequired": "Mã loại đối tượng là bắt buộc",
   "ResObjectTypeItem:UomCodeRequired": "Mã đơn vị tính là bắt buộc",
   "ResObjectTypeItem:ExcelFileInvalid": "File Excel không hợp lệ",
   "ResObjectTypeItem:ExcelMissingHeader": "Thiếu cột bắt buộc: {0}",
   "Permission:ResObjectTypeItem": "Quản lý hạng mục đối tượng"
   ```

2. **Run Database Migration**: Create and run a migration to add the `res_object_type_item` table

3. **Test the Implementation**:
   - Test CRUD operations via API
   - Test import with valid/invalid data
   - Test export template
   - Verify foreign key constraints
   - Verify permissions

## Pattern Compliance

This implementation strictly follows the RESCARBRAND pattern:
- ✅ Same folder structure
- ✅ Same naming conventions (snake_case for DB, PascalCase for C#)
- ✅ Same validation approach
- ✅ Same import/export logic
- ✅ Same permission structure
- ✅ Same audit trail implementation
- ✅ Same soft delete behavior
- ✅ Same navigation property patterns
- ✅ Same repository patterns
- ✅ Same AutoMapper configuration
