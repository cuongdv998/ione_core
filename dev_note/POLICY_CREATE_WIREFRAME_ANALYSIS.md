# Policy Creation Screen Wireframe Analysis & Implementation Plan

## Overview
This document analyzes the differences between the current policy creation dialog and the wireframe design at `2_2_1__them_moi_don_bao_hiem.html`, and provides a detailed implementation plan.

## Wireframe Structure Analysis

Based on the wireframe HTML analysis, the form is organized into the following sections:

### Section 1: Thông tin hợp đồng (Contract Information)
1. **Đối tác BH gốc** (Primary Insurance Partner) * - Dropdown
2. **Nghiệp vụ BH** (Line of Business) * - Dropdown  
3. **Loại hợp đồng** (Contract Type) - Dropdown (HĐ lẻ / HĐ bao)
4. **Khách hàng** (Customer) * - Dropdown/Input
5. **Người thanh toán** (Payer) - Input
6. **Thời hạn bảo hiểm (từ)** (Insurance Period From) * - Date Picker
7. **Thời hạn bảo hiểm (đến)** (Insurance Period To) * - Date Picker
8. **Số hợp đồng** (Contract Number) - Text Input
9. **Số đơn BH** (Policy Number) - Text Input
10. **Số đơn BH gốc** (Root Policy Number) - Text Input

### Section 2: Thông tin đơn BH (Policy Information)
- Additional policy-specific fields
- **Tổng phí BH (có VAT)** (Total Premium with VAT) - Number Input

### Section 3: Thông tin người được BH (Insured Person Information)
1. **Tên người được BH** (Insured Name) - Text Input
2. **Số CMND/CCCD** (ID Number) - Text Input
3. **Số điện thoại** (Phone) * - Text Input
4. **Email** - Email Input
5. **Địa chỉ** (Address) - Textarea
6. **Địa chỉ đầy đủ** (Full Address) - Textarea (read-only/computed)
7. Additional fields for organization type (if applicable):
   - **Email người đại diện** (Representative Email)
   - **Email người ủy quyền** (Authorizer Email)

### Section 4: Thông tin người thụ hưởng (Beneficiary Information)
1. **Tên người thụ hưởng** (Beneficiary Name) - Text Input
2. **Số CMND/CCCD** (ID Number) - Text Input
3. **Số điện thoại** (Phone) * - Text Input
4. **Email** - Email Input
5. **Địa chỉ** (Address) - Textarea
6. **Địa chỉ đầy đủ** (Full Address) - Textarea (read-only/computed)
7. Additional fields for organization type (if applicable)

## Current Implementation Analysis

### Current Dialog Structure
The current implementation (`policies.component.html` lines 319-547) has:
- Simple single-column form layout
- Basic fields: PolicyNo, InsurerPolicyNo, PolicyTypeId, PartnerId, Status, OrgEffectDate, OrgExpireDate, PremiumTotal
- Insured person fields: Name, IDNo, Phone, Email, Address
- Beneficiary fields: Name, IDNo, Phone, Email, Address
- No section grouping
- No tabs or accordion structure
- Missing many fields from wireframe

## Key Differences Identified

### 1. Form Structure & Organization
- **Wireframe**: Organized into clear sections with headers
- **Current**: Flat list of fields without grouping
- **Change Required**: Add section headers and organize fields into logical groups

### 2. Missing Fields in Current Implementation
- **Contract Information Section**:
  - Primary Insurance Partner (Đối tác BH gốc) - Dropdown
  - Line of Business (Nghiệp vụ BH) - Dropdown
  - Contract Type (Loại hợp đồng) - Dropdown
  - Customer (Khách hàng) - Dropdown
  - Payer (Người thanh toán) - Text Input
  - Contract Number (Số hợp đồng) - Text Input
  - Root Policy Number (Số đơn BH gốc) - Text Input

- **Policy Information**:
  - Premium breakdown (Premium, VAT, Discount, Discount Rate)
  - Bank Loan indicator
  - Renewal indicator
  - Gift indicator
  - Currency and Exchange Rate
  - Channel
  - Seller/Implementer

- **Insured/Beneficiary Information**:
  - Full Address (computed field)
  - Organization-specific fields (Representative, Authorizer info)

### 3. Field Type Changes
- **Date Fields**: Wireframe uses date pickers with calendar icon
- **Dropdowns**: Wireframe shows proper dropdowns for Partner, LOB, Contract Type, Customer
- **Text Fields**: Some fields should be textareas (Address fields)

### 4. Layout & Styling
- **Wireframe**: Multi-column grid layout (likely 2-3 columns)
- **Current**: Single column layout
- **Change Required**: Implement responsive grid layout

### 5. Required Field Indicators
- **Wireframe**: Shows asterisk (*) for required fields
- **Current**: Has some asterisks but inconsistent
- **Change Required**: Ensure all required fields are marked

## Implementation Plan

### Phase 1: Backend Updates (If Needed)
**Estimated Time: 2-4 hours**

1. **Review DTOs** (`CreatePolicyDto`, `UpdatePolicyDto`)
   - Verify all fields from wireframe are present
   - Add missing fields if needed
   - Ensure proper validation attributes

2. **Review Domain Model** (`Policy` entity)
   - Verify all properties exist
   - Check navigation properties for dropdowns

3. **Update Application Service** (`PolicyAppService`)
   - Ensure Create/Update methods handle all new fields
   - Add validation logic if needed

### Phase 2: Frontend Model Updates
**Estimated Time: 1-2 hours**

1. **Update `PolicyFormData` interface** (`policies.models.ts`)
   - Add missing fields:
     - `primaryInsurancePartnerId: string`
     - `lobId: string` (Line of Business)
     - `contractType: PolicyContractType | null`
     - `customerId: string`
     - `payerName: string`
     - `contractNo: string`
     - `rootPolicyNo: string`
     - `premium: number`
     - `vat: number`
     - `discount: number`
     - `discountRate: number`
     - `isBankLoan: boolean`
     - `isRenewal: boolean`
     - `isGift: boolean`
     - `currencyId: string`
     - `exchangeRate: number`
     - `channelId: string`
     - `sellerId: string`
     - `implementerId: string`
     - `insuredFullAddress: string` (computed)
     - `beneficiaryFullAddress: string` (computed)
     - Organization fields (if needed)

2. **Update `getEmptyForm()` method**
   - Initialize all new fields with default values

### Phase 3: Component Logic Updates
**Estimated Time: 3-4 hours**

1. **Add Dropdown Options Arrays**
   - `primaryInsurancePartnerOptions` (already exists)
   - `lobOptions` (Line of Business)
   - `contractTypeOptions` (already exists)
   - `customerOptions`
   - `currencyOptions`
   - `channelOptions`
   - `sellerOptions`
   - `implementerOptions`

2. **Add Methods to Load Dropdown Data**
   - `loadLobOptions()` - Load from ProLineOfBusiness service
   - `loadCustomerOptions()` - Load from ResCustomer service
   - `loadCurrencyOptions()` - Load from ResCurrency service
   - `loadChannelOptions()` - Load from ResChannel service
   - `loadSellerOptions()` - Load from HrEmployee service (filter by role)
   - `loadImplementerOptions()` - Load from HrEmployee service (filter by role)

3. **Update `openCreateDialog()` method**
   - Call all dropdown loading methods
   - Reset form with all new fields

4. **Update `openEditDialog()` method**
   - Map all new fields from DTO to form data

5. **Update `save()` method**
   - Map all new form fields to CreatePolicyDto/UpdatePolicyDto

6. **Add Address Computation Logic**
   - Method to compute full address from address + ward + province
   - Update when address, ward, or province changes

### Phase 4: HTML Template Restructure
**Estimated Time: 4-6 hours**

1. **Add Section Headers**
   - Wrap fields in sections with headers:
     - "Thông tin hợp đồng" (Contract Information)
     - "Thông tin đơn BH" (Policy Information)
     - "Thông tin người được BH" (Insured Person Information)
     - "Thông tin người thụ hưởng" (Beneficiary Information)

2. **Implement Grid Layout**
   - Use Tailwind CSS grid: `grid grid-cols-12 gap-4`
   - Organize fields in 2-3 column layout
   - Responsive: `col-span-12 md:col-span-6 lg:col-span-4`

3. **Add Contract Information Section**
   ```html
   <!-- Section: Contract Information -->
   <div class="col-span-12">
     <h3 class="text-lg font-semibold mb-4 underline">Thông tin hợp đồng</h3>
   </div>
   <!-- Fields: Primary Insurance Partner, LOB, Contract Type, Customer, Payer, Dates, Contract No, Policy No, Root Policy No -->
   ```

4. **Add Policy Information Section**
   ```html
   <!-- Section: Policy Information -->
   <div class="col-span-12">
     <h3 class="text-lg font-semibold mb-4">Thông tin đơn BH</h3>
   </div>
   <!-- Fields: Premium breakdown, Currency, Exchange Rate, Channel, Seller, Implementer, Flags -->
   ```

5. **Update Insured Person Section**
   - Add section header
   - Add Full Address field (read-only/computed)
   - Add organization-specific fields (conditional display)

6. **Update Beneficiary Section**
   - Add section header
   - Add Full Address field (read-only/computed)
   - Add organization-specific fields (conditional display)

7. **Replace Input Types**
   - Change date inputs to `p-datepicker` components
   - Change text inputs to dropdowns where needed
   - Change address inputs to `p-textarea`

8. **Add Required Field Indicators**
   - Ensure all required fields have `<span class="text-red-500">*</span>`

### Phase 5: Localization Updates
**Estimated Time: 1-2 hours**

1. **Add Missing Localization Keys** (`en.json` and `vi-VN.json`)
   - Contract Information section:
     - `Policy:PrimaryInsurancePartner`
     - `Policy:LineOfBusiness`
     - `Policy:ContractType`
     - `Policy:Customer`
     - `Policy:Payer`
     - `Policy:ContractNo`
     - `Policy:RootPolicyNo`
   - Policy Information section:
     - `Policy:Premium`
     - `Policy:VAT`
     - `Policy:Discount`
     - `Policy:DiscountRate`
     - `Policy:IsBankLoan`
     - `Policy:IsRenewal`
     - `Policy:IsGift`
     - `Policy:Currency`
     - `Policy:ExchangeRate`
   - Address fields:
     - `Policy:FullAddress`
   - Section headers:
     - `Policy:ContractInformation`
     - `Policy:PolicyInformation`
     - `Policy:InsuredPersonInformation`
     - `Policy:BeneficiaryInformation`

### Phase 6: Validation & Error Handling
**Estimated Time: 2-3 hours**

1. **Add Form Validation**
   - Required field validation
   - Date range validation (from < to)
   - Email format validation
   - Phone number format validation
   - Number field validation (min/max)

2. **Update Error Messages**
   - Ensure all validation errors are localized
   - Display errors inline with fields

### Phase 7: Testing & Refinement
**Estimated Time: 2-3 hours**

1. **Functional Testing**
   - Test all dropdowns load correctly
   - Test form submission with all fields
   - Test validation
   - Test address computation

2. **UI/UX Testing**
   - Verify layout matches wireframe
   - Test responsive behavior
   - Verify all fields are accessible
   - Check field alignment and spacing

3. **Integration Testing**
   - Test create operation
   - Test update operation
   - Verify data persistence

## Estimated Total Time
**15-25 hours** (approximately 2-3 days)

## Priority Order
1. **High Priority**: Contract Information section, Policy Information section, Dropdown implementations
2. **Medium Priority**: Address computation, Organization-specific fields, Validation
3. **Low Priority**: UI polish, Advanced validation, Conditional field display

## Notes
- The wireframe appears to use tabs or accordion for sections (based on Dynamic Panel structure)
- Consider using PrimeNG `p-tabView` or `p-accordion` for better UX
- Some fields may be conditional based on Contract Type or Organization Type
- Full Address fields should be computed automatically from Address + Ward + Province
- Organization-specific fields should only show when Organization Type is "TC" (Tổ chức)

## Dependencies
- Backend DTOs must support all new fields
- Dropdown services must be available (LOB, Customer, Currency, Channel, Employee services)
- Address computation logic may need backend support or frontend calculation
