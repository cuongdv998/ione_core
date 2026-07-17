# Policy Listing Screen - Wireframe Implementation Plan

## 1. Overview
This document outlines the differences between the current policy listing screen and the wireframe design, and provides a detailed implementation plan to align the application with the wireframe specifications.

**Current Screen**: `http://localhost:4200/pages/policy/policies`
**Wireframe**: `WebApplicationPrototype_v1.3/2_2__quan_ly_don_bao_hiem.html`

---

## 2. Comparison Analysis

### 2.1 Search Form Fields

#### Current Implementation (5 fields)
1. **Số đơn** (Policy No) - Text input
2. **Mã hợp đồng** (Contract ID) - Text input
3. **Mã loại đơn** (Policy Type ID) - Text input
4. **Mã đối tác** (Partner ID) - Text input
5. **Trạng thái** (Status) - Dropdown

#### Wireframe Design (14 fields)
1. **Kênh khai thác** (Channel) - Dropdown ✨ NEW
2. **Khách hàng** (Customer) - Dropdown ✨ NEW
3. **Hợp đồng** (Contract) - Dropdown (Changed from text to dropdown)
4. **Loại hợp đồng** (Contract Type) - Dropdown ✨ NEW
5. **Trạng thái hợp đồng** (Contract Status) - Dropdown ✨ NEW
6. **Số Giấy chứng nhận** (Certificate Number) - Text input ✨ NEW
7. **Số đơn BH** (Policy Number) - Text input (Same, renamed)
8. **Trạng thái đơn BH** (Policy Status) - Dropdown (Same, renamed)
9. **Đối tác khai thác** (Partner) - Dropdown (Changed from text to dropdown)
10. **Người cấp đơn** (Policy Issuer) - Dropdown ✨ NEW
11. **Ngày hiệu lực (từ)** (Effective Date From) - Date picker ✨ NEW
12. **Ngày hết hạn (từ)** (Expiry Date From) - Date picker ✨ NEW
13. **Ngày hiệu lực (đến)** (Effective Date To) - Date picker ✨ NEW
14. **Ngày hết hạn (đến)** (Expiry Date To) - Date picker ✨ NEW

**❌ Removed**: Mã loại đơn (Policy Type ID as text input)

### 2.2 Table Columns

#### Current Implementation (9 columns)
1. **STT** (No)
2. **Số đơn** (Policy No)
3. **Số đơn nhà bảo hiểm** (Insurer Policy No)
4. **Tên người được bảo hiểm** (Insured Name)
5. **Tổng phí bảo hiểm** (Premium Total)
6. **Ngày hiệu lực** (Effective Date)
7. **Ngày hết hạn** (Expiry Date)
8. **Thời gian tạo** (Creation Time)
9. **Trạng thái** (Status)

#### Wireframe Design (25 columns)
1. **STT** (No)
2. **Số hợp đồng** (Contract Number) ✨ NEW
3. **Số đơn BH** (Policy Number) (Same)
4. **Số GCN** (Certificate Number) ✨ NEW
5. **Sản phẩm BH** (Insurance Product) ✨ NEW
6. **BH gốc** (Root Insurance) ✨ NEW
7. **Khách hàng** (Customer) ✨ NEW
8. **Ngày hiệu lực** (Effective Date) (Same)
9. **Ngày hết hạn** (Expiry Date) (Same)
10. **Biển số xe** (Vehicle Plate) ✨ NEW
11. **Số khung/số máy** (Chassis/Engine Number) ✨ NEW
12. **Tổng phí BH** (Total Premium) (Same)
13. **Kênh khai thác** (Channel) ✨ NEW
14. **Đối tác khai thác** (Partner) ✨ NEW
15. **Người khai thác** (Implementer) ✨ NEW
16. **Người cấp đơn** (Policy Issuer) ✨ NEW
17. **Phiên bản** (Version) ✨ NEW
18. **Là đơn gốc** (Is Root Policy) ✨ NEW
19. **Trạng thái** (Status) (Same)
20. **Trạng thái TT** (Payment Status) ✨ NEW
21. **Trạng thái duyệt đơn** (Approval Status) (Exists but not shown)
22. **Ngày duyệt** (Approval Date) ✨ NEW
23. **Người duyệt** (Approver) ✨ NEW
24. **Ngày tạo** (Creation Date) (Same as Creation Time)
25. **Người tạo** (Creator) ✨ NEW

**❌ Removed**: Số đơn nhà bảo hiểm (Insurer Policy No), Tên người được bảo hiểm (Insured Name)

### 2.3 Summary of Changes

#### Search Form
- **9 New Fields** (Channel, Customer, Contract Type, Contract Status, Certificate Number, Policy Issuer, 4 Date range filters)
- **3 Changed Fields** (Contract, Partner from text to dropdown; Policy Type from text to dropdown)
- **1 Removed Field** (Policy Type ID as text)

#### Table Columns
- **18 New Columns** (Contract Number, Certificate Number, Product, Root Insurance, Customer, Vehicle Plate, Chassis/Engine, Channel, Partner, Implementer, Policy Issuer, Version, Is Root Policy, Payment Status, Approval Date, Approver, Creator)
- **2 Removed Columns** (Insurer Policy No, Insured Name)
- **16 Columns with additional details or reformatting needed**

---

## 3. Implementation Plan

### Phase 1: Backend Updates

#### 3.1 Update DTOs and Input Models
**File**: `modules/policy/src/iOne.Policy.Application.Contracts/Policies/GetPoliciesInput.cs`

Add new search filter properties:
```csharp
public string? ChannelId { get; set; }
public string? CustomerId { get; set; }
public string? ContractTypeId { get; set; }
public string? ContractStatus { get; set; }
public string? CertificateNo { get; set; }
public string? PolicyIssuerId { get; set; }
public DateTime? EffectiveDateFrom { get; set; }
public DateTime? EffectiveDateTo { get; set; }
public DateTime? ExpiryDateFrom { get; set; }
public DateTime? ExpiryDateTo { get; set; }
```

#### 3.2 Update PolicyDto
**File**: `modules/policy/src/iOne.Policy.Application.Contracts/Policies/PolicyDto.cs`

Add new properties for display:
```csharp
// Contract & Product Info
public string ContractNo { get; set; }
public string CertificateNo { get; set; }
public string ProductName { get; set; }
public string RootInsuranceName { get; set; }

// Customer Info
public string CustomerName { get; set; }
public string CustomerId { get; set; }

// Vehicle Info (for car insurance)
public string VehiclePlate { get; set; }
public string ChassisNumber { get; set; }
public string EngineNumber { get; set; }

// Channel & Partner Info
public string ChannelName { get; set; }
public string ChannelId { get; set; }
public string PartnerName { get; set; }

// Personnel Info
public string ImplementerName { get; set; }
public string ImplementerId { get; set; }
public string PolicyIssuerName { get; set; }
public string PolicyIssuerId { get; set; }

// Version & Root Policy
public int Version { get; set; }
public bool IsRootPolicy { get; set; }

// Status Info
public string PaymentStatus { get; set; }
public DateTime? ApprovalDate { get; set; }
public string ApproverName { get; set; }
public string ApproverId { get; set; }

// Creator Info
public string CreatorName { get; set; }
```

#### 3.3 Update Application Service
**File**: `modules/policy/src/iOne.Policy.Application/Policies/PolicyAppService.cs`

Update the `GetListAsync` method to:
1. Apply new filter conditions
2. Include related entities (Contract, Customer, Channel, Partner, Product, etc.)
3. Map new properties to the DTO

#### 3.4 Update Domain Repository (if needed)
**File**: `modules/policy/src/iOne.Policy.Domain/Policies/IPolicyRepository.cs`

Add method for complex queries with all the new filters and includes.

### Phase 2: Frontend Updates

#### 3.5 Update Frontend Models
**File**: `angular/src/app/pages/policy/policies/policies.models.ts`

```typescript
export interface PolicySearchForm {
  // Existing
  policyNo: string | null;
  status: PolicyStatus | null;
  
  // Changed to dropdown
  contractId: string | null;
  partnerId: string | null;
  
  // New dropdown fields
  channelId: string | null;
  customerId: string | null;
  contractTypeId: string | null;
  contractStatus: string | null;
  policyIssuerId: string | null;
  
  // New text fields
  certificateNo: string | null;
  
  // New date range fields
  effectiveDateFrom: string | null;
  effectiveDateTo: string | null;
  expiryDateFrom: string | null;
  expiryDateTo: string | null;
}
```

#### 3.6 Update Component TypeScript
**File**: `angular/src/app/pages/policy/policies/policies.component.ts`

Changes needed:
1. **Import PrimeNG DatePicker (Calendar)**: Add `CalendarModule` to imports
2. **Add new dropdown options arrays**:
   - `channelOptions`
   - `customerOptions`
   - `contractOptions`
   - `contractTypeOptions`
   - `contractStatusOptions`
   - `partnerOptions`
   - `policyIssuerOptions`
3. **Update searchForm initialization** with new fields
4. **Update initializeColumns()** to include all 25 columns
5. **Add services to load dropdown options**:
   - Load channels from ChannelService
   - Load customers from CustomerService
   - Load contracts from ContractService
   - Load partners from PartnerService
   - Load users for policy issuer
6. **Update loadData()** to pass new filter parameters
7. **Add formatters** for new column types (boolean for Is Root Policy, etc.)

#### 3.7 Update Component HTML Template
**File**: `angular/src/app/pages/policy/policies/policies.component.html`

Changes needed:
1. **Expand search form grid** to accommodate 14 fields (use a 3-column grid layout)
2. **Add new search fields**:
   - Channel dropdown (p-select)
   - Customer dropdown (p-select with search)
   - Contract dropdown (p-select with search)
   - Contract Type dropdown (p-select)
   - Contract Status dropdown (p-select)
   - Certificate Number input
   - Partner dropdown (p-select with search)
   - Policy Issuer dropdown (p-select with search)
   - Effective Date From (p-calendar)
   - Effective Date To (p-calendar)
   - Expiry Date From (p-calendar)
   - Expiry Date To (p-calendar)
3. **Update table columns** to show all 25 columns
4. **Add horizontal scroll** to table (enable scrollable mode)
5. **Add column freezing** for important columns (STT, Policy No, Status)
6. **Add column resizing** capability
7. **Add column visibility toggle** for user customization

#### 3.8 Update Proxy Services
**Files**: 
- `angular/src/app/proxy/policy/policies/models.ts`
- `angular/src/app/proxy/policy/controllers/policy.service.ts`

Regenerate proxy services using ABP CLI to sync with backend DTOs:
```bash
cd angular
abp generate-proxy -t ng
```

### Phase 3: Localization

#### 3.9 Add Localization Keys
**File**: Backend localization files

Add Vietnamese and English keys for all new fields:
```json
{
  "Policy::Policy:Channel": "Kênh khai thác",
  "Policy::Policy:ChannelId": "Mã kênh",
  "Policy::Policy:Customer": "Khách hàng",
  "Policy::Policy:CustomerId": "Mã khách hàng",
  "Policy::Policy:ContractType": "Loại hợp đồng",
  "Policy::Policy:ContractStatus": "Trạng thái hợp đồng",
  "Policy::Policy:CertificateNo": "Số Giấy chứng nhận",
  "Policy::Policy:PolicyIssuer": "Người cấp đơn",
  "Policy::Policy:EffectiveDateFrom": "Ngày hiệu lực (từ)",
  "Policy::Policy:EffectiveDateTo": "Ngày hiệu lực (đến)",
  "Policy::Policy:ExpiryDateFrom": "Ngày hết hạn (từ)",
  "Policy::Policy:ExpiryDateTo": "Ngày hết hạn (đến)",
  "Policy::Policy:ContractNo": "Số hợp đồng",
  "Policy::Policy:Product": "Sản phẩm BH",
  "Policy::Policy:RootInsurance": "BH gốc",
  "Policy::Policy:VehiclePlate": "Biển số xe",
  "Policy::Policy:ChassisEngine": "Số khung/số máy",
  "Policy::Policy:Implementer": "Người khai thác",
  "Policy::Policy:Version": "Phiên bản",
  "Policy::Policy:IsRootPolicy": "Là đơn gốc",
  "Policy::Policy:PaymentStatus": "Trạng thái TT",
  "Policy::Policy:ApprovalDate": "Ngày duyệt",
  "Policy::Policy:Approver": "Người duyệt",
  "Policy::Policy:Creator": "Người tạo"
}
```

### Phase 4: Services Integration

#### 3.10 Add/Update Related Services
Ensure the following services exist and are properly integrated:
1. **ChannelService** - for loading channel options
2. **CustomerService** - for loading customer options
3. **ContractService** - for loading contract options
4. **PartnerService** - for loading partner options (may already exist)
5. **UserService** - for loading policy issuer/implementer options

### Phase 5: UI/UX Enhancements

#### 3.11 Table Enhancements
1. **Enable horizontal scrolling** with fixed column widths
2. **Freeze key columns**: STT, Policy No, Status (on the left)
3. **Add column reordering** capability
4. **Add column visibility toggle** (show/hide columns)
5. **Add export to Excel** functionality for the full dataset
6. **Implement responsive design** for mobile/tablet views
7. **Add quick filters** (buttons for common status filters)
8. **Add batch operations** if needed (bulk update status, etc.)

#### 3.12 Search Form Enhancements
1. **Use accordion/collapsible panel** for search form (already implemented)
2. **Add "Save Filter"** functionality to save common search criteria
3. **Add "Advanced Search"** toggle to show/hide additional filters
4. **Add field validation** for date ranges (From must be before To)
5. **Add auto-search** option (search as user types with debounce)
6. **Add clear individual field** buttons (X icon on each input)

---

## 4. Implementation Steps

### Step 1: Backend Development
1. ✅ Update `GetPoliciesInput.cs` with new filter properties
2. ✅ Update `PolicyDto.cs` with new display properties
3. ✅ Update `PolicyAppService.cs` with new query logic
4. ✅ Add/update repository methods if needed
5. ✅ Run and test API endpoints with new filters
6. ✅ Add localization keys to backend resources

### Step 2: Frontend Service Layer
1. ✅ Regenerate Angular proxy services using ABP CLI
2. ✅ Verify proxy models match backend DTOs
3. ✅ Test service methods with new parameters

### Step 3: Frontend Component Development
1. ✅ Update `policies.models.ts` with new interfaces
2. ✅ Update `policies.component.ts`:
   - Add new imports (CalendarModule, etc.)
   - Add dropdown option properties
   - Initialize dropdown options in ngOnInit
   - Update searchForm initialization
   - Update table columns definition
   - Add formatters for new columns
   - Update loadData method
3. ✅ Update `policies.component.html`:
   - Add new search fields to form
   - Update table columns
   - Add horizontal scroll configuration
   - Add column freeze configuration
4. ✅ Test component functionality

### Step 4: Integration & Testing
1. ✅ Integration testing with backend
2. ✅ Test all search filters individually
3. ✅ Test combined filters
4. ✅ Test table sorting on new columns
5. ✅ Test table pagination
6. ✅ Test responsive design
7. ✅ Cross-browser testing

### Step 5: Final Touches
1. ✅ Add loading states for dropdown options
2. ✅ Add error handling for failed service calls
3. ✅ Add tooltips for complex fields
4. ✅ Optimize performance (lazy loading, virtual scrolling)
5. ✅ Documentation updates
6. ✅ User acceptance testing

---

## 5. Technical Considerations

### 5.1 Performance
- **Large Dataset**: With 25 columns, consider implementing:
  - Virtual scrolling for table rows
  - Column virtualization for many columns
  - Server-side pagination (already implemented)
  - Debounced search for text inputs
  - Lazy loading for dropdown options

### 5.2 Data Relationships
- Policy → Contract (1:1)
- Policy → Customer (1:1)
- Policy → Partner (1:1)
- Policy → Channel (1:1)
- Policy → Product (1:1)
- Policy → PolicyIssuer/User (1:1)
- Policy → Implementer/User (1:1)
- Policy → Approver/User (1:1)
- Policy → Creator/User (1:1)

Ensure all these relationships are properly defined in the domain model and EF Core configurations.

### 5.3 Database Indexes
Add indexes for frequently filtered/sorted columns:
- `PolicyNo`
- `ContractId`
- `CertificateNo`
- `Status`
- `OrgEffectDate`
- `OrgExpireDate`
- `CreationTime`
- `CustomerId`
- `PartnerId`
- `ChannelId`

### 5.4 Authorization
Ensure proper permission checks for:
- Viewing policies
- Filtering by sensitive data (customer info, etc.)
- Exporting data

---

## 6. Testing Checklist

### Functional Testing
- [ ] All search fields work correctly
- [ ] Date range filters validate properly (From < To)
- [ ] Dropdown options load correctly
- [ ] Table displays all 25 columns
- [ ] Table sorting works on all sortable columns
- [ ] Table pagination works correctly
- [ ] Search + Sort + Pagination work together
- [ ] Export functionality works
- [ ] Responsive design works on mobile/tablet
- [ ] Column freeze/unfreeze works
- [ ] Column show/hide works

### Integration Testing
- [ ] Backend filters apply correctly
- [ ] Backend returns correct data shape
- [ ] Frontend displays backend data correctly
- [ ] Search results match backend query
- [ ] Pagination counts are accurate

### Performance Testing
- [ ] Large dataset (1000+ rows) loads in reasonable time
- [ ] Search response time < 2 seconds
- [ ] Table scrolling is smooth
- [ ] Dropdown loading is fast

### Security Testing
- [ ] Unauthorized users cannot access data
- [ ] SQL injection prevented
- [ ] XSS prevention in place
- [ ] CSRF protection active

---

## 7. Potential Challenges & Solutions

### Challenge 1: Too Many Table Columns
**Issue**: 25 columns may cause horizontal scrolling issues and poor UX
**Solutions**:
- Implement column visibility toggle (users can show/hide columns)
- Freeze important columns (STT, Policy No, Status)
- Use column groups/headers for better organization
- Consider a master-detail view (show summary in table, details in expandable row)

### Challenge 2: Dropdown Performance
**Issue**: Loading many dropdown options can be slow
**Solutions**:
- Implement server-side search/filtering for dropdowns
- Use PrimeNG's p-select with lazy loading
- Cache dropdown options in frontend
- Implement pagination for dropdown options

### Challenge 3: Complex Query Performance
**Issue**: Many filters + joins can slow down queries
**Solutions**:
- Optimize database queries with proper indexes
- Use projection (select only needed fields)
- Implement query result caching
- Consider using database views for complex joins

### Challenge 4: Date Range Validation
**Issue**: Users might enter invalid date ranges
**Solutions**:
- Add client-side validation (From < To)
- Add server-side validation as well
- Show clear error messages
- Disable invalid date selections in date picker

---

## 8. Estimated Effort

| Phase | Task | Estimated Time |
|-------|------|----------------|
| 1 | Backend DTOs & Models | 2-3 hours |
| 1 | Backend Service Logic | 4-6 hours |
| 1 | Backend Testing | 2-3 hours |
| 2 | Frontend Models | 1 hour |
| 2 | Component TypeScript | 6-8 hours |
| 2 | Component HTML | 4-6 hours |
| 2 | Frontend Services | 2-3 hours |
| 3 | Localization | 2 hours |
| 4 | Integration Testing | 4-6 hours |
| 5 | UI/UX Polish | 3-4 hours |
| 5 | Documentation | 2 hours |
| **TOTAL** | | **32-44 hours** (4-5.5 days) |

---

## 9. Architecture Consistency

This implementation maintains consistency with the existing ABP Angular architecture:

✅ **ABP Framework Patterns**:
- Uses ABP's LocalizationService for all text
- Uses ABP's PermissionService for authorization
- Follows ABP's DTO patterns
- Uses ABP's Application Service patterns
- Implements ABP's pagination standards

✅ **Angular Best Practices**:
- Standalone components
- Reactive forms where appropriate
- Service injection via constructor
- Proper module imports
- Type safety with TypeScript interfaces

✅ **PrimeNG UI Components**:
- Uses p-panel for sections
- Uses p-select for dropdowns
- Uses p-calendar for date pickers
- Uses p-table (via VTable wrapper) for data grid
- Uses p-button for actions
- Uses p-toast for notifications

✅ **Project Code Patterns**:
- Matches existing component structure
- Uses VTable custom component
- Follows naming conventions
- Implements lazy loading pattern
- Uses existing service patterns

---

## 10. Next Steps

1. **Review & Approval**: Get stakeholder approval on this plan
2. **Sprint Planning**: Break down into sprint-sized tasks
3. **Backend Development**: Start with Phase 1 (Backend)
4. **Frontend Development**: Proceed to Phase 2 & 3 (Frontend)
5. **Testing**: Comprehensive testing (Phase 4)
6. **Deployment**: Deploy to test environment
7. **UAT**: User Acceptance Testing
8. **Production Deployment**: Deploy to production

---

## 11. Additional Notes

- Consider creating a **feature flag** to toggle between old and new UI during transition
- Plan for **data migration** if database schema changes
- Schedule **user training** for the new interface
- Prepare **user documentation** with screenshots
- Set up **monitoring** for performance metrics post-deployment

---

**Document Version**: 1.0
**Created**: 2026-01-14
**Last Updated**: 2026-01-14
**Author**: AI Assistant
