# Plan: Enhance GetAsync API for Policy Detail View/Edit

## Overview

This plan outlines modifications to the `GET /api/policy/policies/{id}` endpoint (`PolicyAppService.GetAsync`) to return comprehensive policy details including all nested entities (`PolicyContract`, `PolicyAmount`, `PolicyVersion`, `PolicyRiskObject`, `PolicyRiskMotor`, `PolicyProduct`, `PolicyCoverage`). This enriched response will enable the frontend `policies.component.html` modal to fully populate the create/update form for editing or viewing existing policies.

---

## 1. Target Response Structure

The `GetAsync` response should mirror the `CreatePolicyDto` input structure so the frontend can directly bind the data to the form:

### Base Fields (from `PolicyDto`)
- Core policy information: `LobId`, `PolicyNo`, `SellType`, `PolicyTypeId`, `PartnerId`, `SellerId`, `ImplementerId`, `CurrencyId`, `Status`, `IssueDate`, `ApprovalStatus`
- Date fields: `OrgEffectDate`, `OrgExpireDate`
- Flags: `IsRenewal`, `IsGift`, `IsBankLoan`
- Financial: `PremiumTotal`, `Premium`, `Vat`, `Discount`, `DiscountRate`
- Insured information: `InsuredName`, `InsuredIdNo`, `InsuredTin`, `InsuredPassport`, `InsuredPhone`, `InsuredEmail`, `InsuredProvinceId`, `InsuredWardId`, `InsuredAddress`, `InsuredFullAddress`, `InsuredOrgType`
- Beneficiary information: `BeneficiaryName`, `BeneficiaryIdNo`, `BeneficiaryTin`, `BeneficiaryPassport`, `BeneficiaryPhone`, `BeneficiaryEmail`, `BeneficiaryProvinceId`, `BeneficiaryWardId`, `BeneficiaryAddress`, `BeneficiaryFullAddress`, `BeneficiaryOrgType`
- Other: `InsurerPolicyNo`, `ChannelId`, `LotImportCode`

### Nested Objects (same shape as `CreatePolicyDto` input)

#### 1.1 `Contract: CreatePolicyContractInputDto?`
**Source:** `policy.Contract` (PolicyContract entity)

**Fields to map:**
- `Type` → `PolicyContractType`
- `CustomerId` → `Guid`
- `EffectDate` → `DateTime`
- `ExpireDate` → `DateTime?`
- `IsReciveInvoice` → `string` ("Y"/"N")
- `LobId` → `Guid?`
- `InsurerId` → `Guid?`
- `InsurerContractCode` → `string?`
- Payer fields: `PayerName`, `PayerEmail`, `PayerPhone`, `PayerProvinceId`, `PayerWardId`, `PayerAddress`, `PayerFullAddress`
- `Description` → `string?`
- `CurrentQuantity` → `decimal?`
- `EmployeeId` → `Guid?`

**Fields to exclude (server-only, not editable on create):**
- `Status`, `CancellationDate`, `TerminationDate`, `CancellationReasonId`, `TerminationReasonId`, `CancellationDescription`, `TerminationDescription`, `QuotationId`
- `Code`, `Name`, `Quantity` (if backend generates these)

#### 1.2 `Version: CreatePolicyVersionInputDto?`
**Source:** Policy version joined via `policy.LastVersionId` (single record, no max version logic)

**Fields to map:**
- `Version` → `decimal`
- `EffectDate` → `DateTime`
- `ExpireDate` → `DateTime`
- `OrgEffectDate` → `DateTime`
- `OrgExpireDate` → `DateTime`
- `InternalNote` → `string?`
- `CustomerNote` → `string?`
- `PremiumTotal` → `decimal`
- `Premium` → `decimal`
- `Vat` → `decimal`
- `Discount` → `decimal?`
- `DiscountRate` → `decimal?`

**Fields to exclude (server-only defaults):**
- `Type` (defaults to "O"), `Status` (defaults to "draft"), `ApprovalDate`, `ApproverId`, `ApprovalStatus`, `InsurerIntegrationStatus`, `InsurerIntegrationDescription`, `EndorsementType`, `EndorsementReasonId`, `EndorsementDescription`

#### 1.3 `Amount: CreatePolicyAmountInputDto?`
**Source:** `policy_amount` table OR derive from `Version` totals

**Option A (simpler):** Derive from `currentVersion`:
- `PremiumTotal`, `Premium`, `Vat`, `Discount`, `DiscountRate` from version

**Option B (more precise):** Query `PolicyAmount` table:
- Query: `PolicyAmount` where `PolicyId == policy.Id && PolicyVersionId == currentVersion.Id`
- If multiple, pick latest by `CreationTime`
- Map: `FeeItemId`, `IssueDate`, `AmountTotal`, `Amount`, `Vat`, `PaymentStatus`, `PaymentMethodId`, `PaymentDate`

**Note:** For edit form, Option A may be sufficient if amount is just a summary. Option B is needed if you want to show/edit individual fee items.

#### 1.4 `Products: List<CreatePolicyProductInputDto>`
**Source:** `currentVersion.PolicyProducts` (PolicyProduct entities)

**For each `PolicyProduct`:**
- `ProductId` → `Guid`
- `PremiumTotal` → `decimal`
- `Premium` → `decimal`
- `Vat` → `decimal`
- `Discount` → `decimal?`
- `DiscountRate` → `decimal?`
- `AmountLiability` → `decimal?`
- `InsurerProductCode` → `string?`

**Nested `Coverages: List<CreatePolicyCoverageInputDto>`:**
**Source:** `PolicyProduct.PolicyCoverages`

**For each `PolicyCoverage`:**
- `CoverageId` → `Guid`
- `CoverageParentId` → `Guid?`
- `UomId` → `Guid?`
- `TaxId` → `Guid?`
- `Quantity` → `decimal`
- `PremiumRate` → `decimal`
- `PremiumTotal` → `decimal`
- `Premium` → `decimal`
- `Vat` → `decimal`
- `InsurerCoverageCode` → `string?`
- `TableRateLineId` → `Guid?`
- `AmountLiability` → `decimal?`
- `NetRate` → `decimal?`
- `BaseRate` → `decimal?`
- `FlatRate` → `decimal?`
- `Loading` → `decimal?`
- `Discount` → `decimal?`
- `DiscountRate` → `decimal?`

**Nested `CoverageLevels: List<CreatePolicyCoverageLevelInputDto>`:**
**Source:** `PolicyCoverage.PolicyCoverageLevels`

**For each `PolicyCoverageLevel`:**
- `CoverageLevelTypeId` → `Guid`
- `CoverageLevelBasisId` → `Guid`
- `AmountType` → `string`
- `FromAmount` → `decimal`
- `ToAmount` → `decimal`
- `ConditionScript` → `string?`
- `ComputeScript` → `string?`

#### 1.5 `RiskObject: CreatePolicyRiskObjectInputDto?`
**Source:** `currentVersion.PolicyRiskObjects` (pick primary one if multiple)

**Fields to map:**
- `ObjectTypeId` → `Guid`
- Representative info: `RepName`, `RepIdNo`, `RepPassport`, `RepPhone`, `RepEmail`, `RepProvinceId`, `RepWardId`, `RepAddress`, `RepFullAddress`
- Risk object location: `RiskObjectProvinceId`, `RiskObjectWardId`, `RiskObjectAddress`, `RiskObjectFullAddress`
- Coordinates: `RiskObjectLat`, `RiskObjectLong`

**Nested `RiskObjectMotor: CreatePolicyRiskMotorInputDto?`**
**Source:** `PolicyRiskObject.PolicyRiskMotors` (pick one if multiple)

**Fields to map:**
- `RiskObjectValue` → `decimal?`
- Car codes: `MotorClassCode`, `CarLineCode`, `CarGroupCode`, `CarTypeCode`, `CarBrandCode`, `CarModelCode`, `CarCategoryCode`
- `CarUsage` → `string?`
- `CarOld` → `decimal?`
- `CarProductionYear` → `DateTime?`
- `CarPlate` → `string?`
- `CarPlateClear` → `string?`
- `CarSeatNumber` → `decimal?`
- `CarVin` → `string?`
- `CarEngineNumber` → `string?`
- `CarPayloadCapacity` → `decimal?`
- `CarColor` → `string?`

#### 1.6 `Documents: List<CreatePolicyDocumentInputDto>?` (optional)
**Source:** `PolicyDocument` entities where `PolicyId == policy.Id`

**For each `PolicyDocument`:**
- `DocumentId` → `Guid?` (link to `ResDocument`)

---

## 2. Backend Implementation Plan

### 2.1 Override `GetAsync` in `PolicyAppService`

**Current behavior:**
- `PolicyController.GetAsync(id)` calls `PolicyAppService.GetAsync(id)`
- Base `CrudAppService.GetAsync` likely does: `Repository.GetAsync(id)` + simple `ObjectMapper.Map<Policy, PolicyDto>` with no includes

**New behavior:**

1. **Override `GetAsync(Guid id)` method:**
   ```csharp
   public override async Task<PolicyDto> GetAsync(Guid id)
   {
       // Load with all required includes
       // Map to enriched PolicyDto
       // Return
   }
   ```

2. **Load Policy with all required navigation properties:**
   ```csharp
   var query = await ReadOnlyRepository.GetQueryableAsync();
   
   query = query
       .Include(p => p.Contract)
           .ThenInclude(c => c.Customer)
       .Include(p => p.PolicyVersions)
           .ThenInclude(v => v.PolicyProducts)
               .ThenInclude(pp => pp.PolicyCoverages)
                   .ThenInclude(pc => pc.PolicyCoverageLevels)
       .Include(p => p.PolicyVersions)
           .ThenInclude(v => v.PolicyRiskObjects)
               .ThenInclude(ro => ro.PolicyRiskMotors)
       .Include(p => p.PolicyCertificates) // If certificate number needed
       .Include(p => p.PolicyDocuments)    // If documents needed
       .Where(p => p.Id == id && !p.IsDeleted);
   
   var entity = await AsyncExecuter.FirstOrDefaultAsync(query);
   if (entity == null)
   {
       throw new EntityNotFoundException(typeof(Policy), id);
   }
   
   // Note: We will filter PolicyVersions to only the one matching LastVersionId (single record)
   ```

3. **Get current version (join to PolicyVersion using LastVersionId - only 1 record):**
   ```csharp
   // Join to PolicyVersion using LastVersionId - filter to get only 1 record
   var currentVersion = entity.PolicyVersions
       .Where(v => v.Id == entity.LastVersionId && !v.IsDeleted)
       .FirstOrDefault();
   
   // If LastVersionId doesn't match any version, currentVersion will be null
   // In that case, version-related nested fields (Version, Products, RiskObject) will be null
   ```

4. **Map base Policy fields:**
   ```csharp
   var dto = ObjectMapper.Map<iOne.Policies.Policy, PolicyDto>(entity);
   ```

5. **Map nested `Contract`:**
   ```csharp
   if (entity.Contract != null)
   {
       dto.Contract = new CreatePolicyContractInputDto
       {
           Type = entity.Contract.Type,
           CustomerId = entity.Contract.CustomerId,
           EffectDate = entity.Contract.EffectDate,
           ExpireDate = entity.Contract.ExpireDate,
           IsReciveInvoice = entity.Contract.IsReciveInvoice,
           LobId = entity.Contract.LobId,
           InsurerId = entity.Contract.InsurerId,
           InsurerContractCode = entity.Contract.InsurerContractCode,
           PayerName = entity.Contract.PayerName,
           PayerEmail = entity.Contract.PayerEmail,
           PayerPhone = entity.Contract.PayerPhone,
           PayerProvinceId = entity.Contract.PayerProvinceId,
           PayerWardId = entity.Contract.PayerWardId,
           PayerAddress = entity.Contract.PayerAddress,
           PayerFullAddress = entity.Contract.PayerFullAddress,
           Description = entity.Contract.Description,
           CurrentQuantity = entity.Contract.CurrentQuantity,
           EmployeeId = entity.Contract.EmployeeId
           // Do NOT include: Code, Name, Quantity, Status, cancellation/termination fields
       };
   }
   ```

6. **Map nested `Version`:**
   ```csharp
   if (currentVersion != null)
   {
       dto.Version = new CreatePolicyVersionInputDto
       {
           Version = currentVersion.Version,
           EffectDate = currentVersion.EffectDate,
           ExpireDate = currentVersion.ExpireDate,
           OrgEffectDate = currentVersion.OrgEffectDate,
           OrgExpireDate = currentVersion.OrgExpireDate,
           InternalNote = currentVersion.InternalNote,
           CustomerNote = currentVersion.CustomerNote,
           PremiumTotal = currentVersion.PremiumTotal,
           Premium = currentVersion.Premium,
           Vat = currentVersion.Vat,
           Discount = currentVersion.Discount,
           DiscountRate = currentVersion.DiscountRate
       };
   }
   ```

7. **Map nested `Amount`:**
   ```csharp
   // Option A: Derive from version
   if (currentVersion != null)
   {
       dto.Amount = new CreatePolicyAmountInputDto
       {
           PremiumTotal = currentVersion.PremiumTotal,
           Premium = currentVersion.Premium,
           Vat = currentVersion.Vat,
           Discount = currentVersion.Discount,
           DiscountRate = currentVersion.DiscountRate
       };
   }
   
   // Option B: Query PolicyAmount table
   // var policyAmount = await PolicyAmountRepository
   //     .FirstOrDefaultAsync(pa => pa.PolicyId == entity.Id 
   //         && pa.PolicyVersionId == currentVersion.Id);
   // if (policyAmount != null) { ... }
   ```

8. **Map nested `Products` + `Coverages` + `CoverageLevels`:**
   ```csharp
   if (currentVersion != null)
   {
       dto.Products = currentVersion.PolicyProducts
           .Where(pp => !pp.IsDeleted)
           .Select(pp => new CreatePolicyProductInputDto
           {
               ProductId = pp.ProductId,
               PremiumTotal = pp.PremiumTotal,
               Premium = pp.Premium,
               Vat = pp.Vat,
               Discount = pp.Discount,
               DiscountRate = pp.DiscountRate,
               AmountLiability = pp.AmountLiability,
               InsurerProductCode = pp.InsurerProductCode,
               Coverages = pp.PolicyCoverages
                   .Where(pc => !pc.IsDeleted)
                   .Select(pc => new CreatePolicyCoverageInputDto
                   {
                       CoverageId = pc.CoverageId,
                       CoverageParentId = pc.CoverageParentId,
                       UomId = pc.UomId,
                       TaxId = pc.TaxId,
                       Quantity = pc.Quantity,
                       PremiumRate = pc.PremiumRate,
                       PremiumTotal = pc.PremiumTotal,
                       Premium = pc.Premium,
                       Vat = pc.Vat,
                       InsurerCoverageCode = pc.InsurerCoverageCode,
                       TableRateLineId = pc.TableRateLineId,
                       AmountLiability = pc.AmountLiability,
                       NetRate = pc.NetRate,
                       BaseRate = pc.BaseRate,
                       FlatRate = pc.FlatRate,
                       Loading = pc.Loading,
                       Discount = pc.Discount,
                       DiscountRate = pc.DiscountRate,
                       CoverageLevels = pc.PolicyCoverageLevels
                           .Where(pcl => !pcl.IsDeleted)
                           .Select(pcl => new CreatePolicyCoverageLevelInputDto
                           {
                               CoverageLevelTypeId = pcl.CoverageLevelTypeId,
                               CoverageLevelBasisId = pcl.CoverageLevelBasisId,
                               AmountType = pcl.AmountType,
                               FromAmount = pcl.FromAmount,
                               ToAmount = pcl.ToAmount,
                               ConditionScript = pcl.ConditionScript,
                               ComputeScript = pcl.ComputeScript
                           })
                           .ToList()
                   })
                   .ToList()
           })
           .ToList();
   }
   ```

9. **Map nested `RiskObject` + `RiskMotor`:**
   ```csharp
   if (currentVersion != null)
   {
       var riskObject = currentVersion.PolicyRiskObjects
           .Where(ro => !ro.IsDeleted)
           .FirstOrDefault(); // Pick primary one if multiple
       
       if (riskObject != null)
       {
           var riskMotor = riskObject.PolicyRiskMotors
               .Where(rm => !rm.IsDeleted)
               .FirstOrDefault(); // Pick one if multiple
           
           dto.RiskObject = new CreatePolicyRiskObjectInputDto
           {
               ObjectTypeId = riskObject.ObjectTypeId,
               RepName = riskObject.RepName,
               RepIdNo = riskObject.RepIdNo,
               RepPassport = riskObject.RepPassport,
               RepPhone = riskObject.RepPhone,
               RepEmail = riskObject.RepEmail,
               RepProvinceId = riskObject.RepProvinceId,
               RepWardId = riskObject.RepWardId,
               RepAddress = riskObject.RepAddress,
               RepFullAddress = riskObject.RepFullAddress,
               RiskObjectProvinceId = riskObject.RiskObjectProvinceId,
               RiskObjectWardId = riskObject.RiskObjectWardId,
               RiskObjectAddress = riskObject.RiskObjectAddress,
               RiskObjectFullAddress = riskObject.RiskObjectFullAddress,
               RiskObjectLat = riskObject.RiskObjectLat,
               RiskObjectLong = riskObject.RiskObjectLong,
               RiskObjectMotor = riskMotor != null ? new CreatePolicyRiskMotorInputDto
               {
                   RiskObjectValue = riskMotor.RiskObjectValue,
                   MotorClassCode = riskMotor.MotorClassCode,
                   CarLineCode = riskMotor.CarLineCode,
                   CarGroupCode = riskMotor.CarGroupCode,
                   CarTypeCode = riskMotor.CarTypeCode,
                   CarBrandCode = riskMotor.CarBrandCode,
                   CarModelCode = riskMotor.CarModelCode,
                   CarCategoryCode = riskMotor.CarCategoryCode,
                   CarUsage = riskMotor.CarUsage,
                   CarOld = riskMotor.CarOld,
                   CarProductionYear = riskMotor.CarProductionYear,
                   CarPlate = riskMotor.CarPlate,
                   CarPlateClear = riskMotor.CarPlateClear,
                   CarSeatNumber = riskMotor.CarSeatNumber,
                   CarVin = riskMotor.CarVin,
                   CarEngineNumber = riskMotor.CarEngineNumber,
                   CarPayloadCapacity = riskMotor.CarPayloadCapacity,
                   CarColor = riskMotor.CarColor
               } : null
           };
       }
   }
   ```

10. **Map `Documents` (optional):**
    ```csharp
    var policyDocuments = await PolicyDocumentRepository
        .GetListAsync(pd => pd.PolicyId == entity.Id && !pd.IsDeleted);
    
    if (policyDocuments.Count > 0)
    {
        dto.Documents = policyDocuments
            .Select(pd => new CreatePolicyDocumentInputDto
            {
                DocumentId = pd.DocumentId
            })
            .ToList();
    }
    ```

11. **Return enriched DTO:**
    ```csharp
    return dto;
    ```

### 2.2 Keep `GetListAsync` Lightweight

- The existing `GetListAsync` (for search grid) should remain lightweight
- It already returns: `ContractNo`, `CustomerId`, `CustomerName`, `ProductName`, `VehiclePlate`, `EngineNumber` (computed fields)
- Do NOT add heavy includes to `GetListAsync` - keep it fast for listing
- Only `GetAsync` (detail view) should load the full graph

### 2.3 Dependencies to Add

**In `PolicyAppService` constructor, ensure these repositories/managers are injected:**
- `IPolicyRepository` (already exists)
- `PolicyContractManager` (already exists)
- `PolicyVersionManager` (already exists)
- `PolicyProductManager` (already exists)
- `PolicyCoverageManager` (already exists)
- `PolicyCoverageLevelManager` (already exists)
- `PolicyRiskObjectManager` (already exists)
- `PolicyRiskMotorManager` (already exists)
- `PolicyDocumentManager` (already exists)
- `IPolicyDocumentRepository` (if not already injected)
- `IPolicyAmountRepository` (if using Option B for Amount mapping)

---

## 3. Frontend Implementation Plan

### 3.1 Update `openEditDialog` Method

**Current behavior:**
- `openEditDialog(policy: PolicyDto)` receives shallow `PolicyDto` from table row
- Manually builds `formData` from limited fields

**New behavior:**

1. **Call `get(id)` API:**
   ```typescript
   openEditDialog(policy: PolicyDto): void {
       this.loading = true;
       
       this.policyService.get(policy.id).subscribe({
           next: (detail: PolicyDto) => {
               // Use enriched detail to populate form
               this.populateFormFromDetail(detail);
               this.dialogMode = 'edit';
               this.selectedPolicy = detail;
               this.dialogVisible = true;
               this.loading = false;
           },
           error: (error) => {
               this.messageService.add({
                   severity: 'error',
                   summary: 'Error',
                   detail: 'Failed to load policy details'
               });
               this.loading = false;
           }
       });
   }
   ```

2. **Create `populateFormFromDetail` method:**
   ```typescript
   private populateFormFromDetail(detail: PolicyDto): void {
       // Map base policy fields
       this.formData = {
           contractId: detail.contractId || null,
           primaryInsurancePartnerId: detail.contract?.insurerId || null,
           lobId: detail.lobId || '',
           contractType: detail.contract?.type || null,
           customerId: detail.contract?.customerId || null,
           contractNo: detail.contractNo || null,
           policyNo: detail.policyNo || '',
           lastVersionId: detail.lastVersionId || '0',
           sellType: detail.sellType ?? PolicySellType.Agency,
           insurerPolicyNo: detail.insurerPolicyNo || null,
           policyTypeId: detail.policyTypeId || '',
           partnerId: detail.partnerId || '',
           sellerId: detail.sellerId || '',
           implementerId: detail.implementerId || '',
           currencyId: detail.currencyId || '',
           exchangeRate: detail.exchangeRate || 1,
           status: detail.status ?? PolicyStatus.Draft,
           issueDate: detail.issueDate || null,
           approvalStatus: detail.approvalStatus || null,
           orgEffectDate: detail.orgEffectDate || '',
           orgExpireDate: detail.orgExpireDate || '',
           isRenewal: detail.isRenewal === 'Y',
           isGift: detail.isGift === 'Y',
           premiumTotal: detail.premiumTotal || 0,
           premium: detail.premium || 0,
           vat: detail.vat || 0,
           discount: detail.discount || null,
           discountRate: detail.discountRate || null,
           isBankLoan: detail.isBankLoan === 'Y',
           channelId: detail.channelId || null,
           certificateNo: detail.certificateNo || null,
           beneficiaryId: detail.beneficiaryId || null,
           payerId: detail.contract?.customerId || null,
           isReceiveInvoice: detail.contract?.isReciveInvoice === 'Y',
           vehicleOwnerId: detail.contract?.customerId || null,
           // ... other fields
           
           // Risk object / motor fields
           carBrandId: null, // Will map from codes
           carModelId: null,
           carLineId: null,
           carGroupId: null,
           vehicleTypeId: null,
           vehiclePlate: detail.riskObject?.riskObjectMotor?.carPlate || null,
           engineNumber: detail.riskObject?.riskObjectMotor?.carEngineNumber || null,
           // ... other car fields from riskObjectMotor
           
           insurancePeriodFrom: detail.version?.orgEffectDate 
               ? new Date(detail.version.orgEffectDate) 
               : this.getDefaultInsurancePeriodFrom(),
           insurancePeriodTo: detail.version?.orgExpireDate 
               ? new Date(detail.version.orgExpireDate) 
               : this.getDefaultInsurancePeriodTo(),
           // ... other fields
       };
       
       // Map car codes to IDs (if you have lookup maps)
       if (detail.riskObject?.riskObjectMotor) {
           const motor = detail.riskObject.riskObjectMotor;
           // Use your existing lookup maps to convert codes to IDs
           // this.formData.carBrandId = this.getCarBrandIdByCode(motor.carBrandCode);
           // etc.
       }
       
       // Initialize selected products and coverages
       this.selectedProducts = {};
       this.productCoverages = {};
       
       if (detail.products && detail.products.length > 0) {
           detail.products.forEach(product => {
               if (product.productId) {
                   // Enable product switch
                   this.selectedProducts[product.productId] = true;
                   
                   // Map coverages
                   if (product.coverages && product.coverages.length > 0) {
                       this.productCoverages[product.productId] = product.coverages.map(coverage => {
                           // Map to CoverageItem interface
                           return {
                               coverageId: coverage.coverageId,
                               coverageParentId: coverage.coverageParentId,
                               uomId: coverage.uomId,
                               taxId: coverage.taxId,
                               insurerCoverageCode: coverage.insurerCoverageCode,
                               benefit: '', // Will need to load from ProCoverage lookup
                               insuranceAmount: coverage.amountLiability || null,
                               quantity: coverage.quantity || 1,
                               taxRate: null, // May need to compute or load
                               premiumRate: coverage.premiumRate || null,
                               premium: coverage.premium || 0,
                               vat: coverage.vat || 0,
                               premiumWithVAT: coverage.premium + coverage.vat,
                               deductible: null,
                               description: null,
                               coverageType: undefined, // Will need to load from ProCoverage
                               selected: coverage.quantity > 0, // Checkbox state
                               isCheckboxDisabled: false, // Will need to check availabilityType
                               availabilityType: undefined // Will need to load from ProProductCoverage
                           };
                       });
                   }
               }
           });
       }
       
       // Load uploaded documents
       if (detail.documents && detail.documents.length > 0) {
           this.uploadedDocumentIds = detail.documents
               .map(d => d.documentId)
               .filter(id => id != null) as string[];
           // Optionally load ResDocument details for display
       }
       
       // Update summary section
       this.updateSummary();
   }
   ```

3. **Handle car code-to-ID mapping:**
   - If backend returns codes (e.g., `carBrandCode`), frontend needs to convert to IDs using existing lookup maps
   - Or backend could return both codes and IDs in a nested structure

### 3.2 View-Only Mode (Future Enhancement)

- Add a `viewMode: boolean` flag
- When `viewMode === true`, disable all form controls in the modal
- Still use the same `get(id)` API call

### 3.3 Update Payload Alignment

- For **update** API call, you can reuse the same nested structure (`Contract`, `Version`, `Amount`, `Products`, `RiskObject`, `Documents`)
- Or keep update limited to specific fields (but detail API still needs full graph for display)

---

## 4. Edge Cases and Validation

### 4.1 No Matching Version
- If `LastVersionId` doesn't match any version in `PolicyVersions`, the filtered Include will return empty collection
- Set `Version`, `Products`, `RiskObject`, `RiskMotor` to `null`/empty
- API should still succeed (policy exists but LastVersionId points to non-existent or deleted version)

### 4.2 Multiple Risk Objects/Motors
- Define business rule: pick first one, or mark one as "primary"
- Document the selection logic

### 4.3 Multiple PolicyAmounts
- If multiple `PolicyAmount` records exist for same `PolicyId + PolicyVersionId`, pick latest by `CreationTime`
- Or aggregate them (if business logic requires)

### 4.4 Soft-Deleted Children
- Filter out `IsDeleted == true` entities when loading children
- Use `.Where(x => !x.IsDeleted)` in LINQ queries

### 4.5 Missing Navigation Properties
- Handle `null` checks for `Contract`, `Customer`, `Version`, `RiskObject`, `RiskMotor`
- Use null-conditional operators (`?.`) in mapping

### 4.6 Performance Considerations
- `GetAsync` will be slower due to heavy includes
- Consider caching if needed
- Monitor query performance and add indexes if necessary

---

## 5. Testing Checklist

### Backend Tests
- [ ] `GetAsync` returns policy with all nested entities populated
- [ ] `GetAsync` handles policy with no versions gracefully
- [ ] `GetAsync` handles policy with no contract gracefully
- [ ] `GetAsync` handles policy with no risk object/motor gracefully
- [ ] `GetAsync` filters out soft-deleted children
- [ ] `GetAsync` joins to version using LastVersionId (single record)
- [ ] `GetAsync` handles multiple products/coverages correctly
- [ ] `GetAsync` handles multiple risk objects/motors (picks first)
- [ ] `GetAsync` performance is acceptable (< 1 second for typical policy)

### Frontend Tests
- [ ] Edit modal populates all fields correctly from `get(id)` response
- [ ] Product switches are enabled for products in response
- [ ] Coverage checkboxes/amounts are populated correctly
- [ ] Summary section shows correct totals
- [ ] Car fields (plate, engine number) are populated
- [ ] Documents are loaded and displayed
- [ ] Form can be submitted for update after loading detail
- [ ] Error handling works if API call fails

---

## 6. Implementation Order

1. **Backend:**
   - Override `GetAsync` in `PolicyAppService`
   - Add includes for all navigation properties
   - Map nested `Contract`
   - Map nested `Version`
   - Map nested `Amount` (Option A first)
   - Map nested `Products` + `Coverages` + `CoverageLevels`
   - Map nested `RiskObject` + `RiskMotor`
   - Map `Documents`
   - Test with existing policies

2. **Frontend:**
   - Update `openEditDialog` to call `get(id)`
   - Create `populateFormFromDetail` method
   - Map base fields to `formData`
   - Initialize `selectedProducts` and `productCoverages`
   - Handle car code-to-ID mapping
   - Test edit flow end-to-end

3. **Refinement:**
   - Handle edge cases
   - Optimize performance if needed
   - Add error handling
   - Add loading states

---

## 7. Notes

- The enriched `PolicyDto` returned by `GetAsync` will have additional nested properties that may not be in the base `PolicyDto` class definition. You may need to extend `PolicyDto` or use a separate `PolicyDetailDto` class if strict typing is required.
- Alternatively, you can add nullable nested properties to `PolicyDto` to support both list and detail views.
- Consider using AutoMapper profiles to simplify the mapping logic if the structure becomes complex.
- The frontend may need to load additional lookup data (e.g., ProCoverage names, availability types) to fully populate the coverage table, but the core structure should come from the API.

---

## 8. Files to Modify

### Backend:
- `/modules/policy/src/iOne.Policy.Application/Policies/PolicyAppService.cs` - Override `GetAsync`
- `/modules/policy/src/iOne.Policy.Application.Contracts/Policies/PolicyDto.cs` - Add nested properties (optional, or use separate DTO)

### Frontend:
- `/angular/src/app/pages/policy/policies/policies.component.ts` - Update `openEditDialog`, add `populateFormFromDetail`
- `/angular/src/app/pages/policy/policies/policies.component.html` - May need minor adjustments for view mode

---

**End of Plan**
