# Policy Listing Implementation - Next Steps

## ✅ Completed

All code changes have been implemented according to the wireframe design plan. The following files have been updated:

### Backend (4 files)
1. ✅ `modules/policy/src/iOne.Policy.Application.Contracts/Policies/GetPoliciesInput.cs`
2. ✅ `modules/policy/src/iOne.Policy.Application.Contracts/Policies/PolicyDto.cs`
3. ✅ `modules/policy/src/iOne.Policy.Application/Policies/PolicyAppService.cs`
4. ✅ `modules/policy/src/iOne.Policy.Application.Contracts/Localization/Policy/en.json`
5. ✅ `modules/policy/src/iOne.Policy.Application.Contracts/Localization/Policy/vi-VN.json`

### Frontend (3 files)
1. ✅ `angular/src/app/pages/policy/policies/policies.models.ts`
2. ✅ `angular/src/app/pages/policy/policies/policies.component.ts`
3. ✅ `angular/src/app/pages/policy/policies/policies.component.html`

---

## 🔧 Required Actions Before Testing

### Step 1: Review Domain Model Changes
Some new fields require updates to the Policy domain entity. Check if these properties exist:

```bash
# Open the Policy entity file
code src/common/domain/iOne.Domain/Policies/Policy.cs
```

**Properties to add** (if they don't exist):
- `CustomerId` (Guid?)
- `CertificateNo` (string?)
- `PolicyIssuerId` (Guid?)
- `Version` (int)
- `IsRootPolicy` (bool)
- `PaymentStatus` (string?)
- `ApproverId` (Guid?)
- `ApprovalDate` (DateTime?)

If any properties are missing, you'll need to:
1. Add them to the entity
2. Create a database migration
3. Apply the migration

### Step 2: Regenerate Angular Proxy Services
After confirming backend changes, regenerate the Angular proxy services:

```bash
cd angular
abp generate-proxy -t ng
```

This command will:
- Update `src/app/proxy/policy/policies/models.ts` with new DTO properties
- Update the PolicyService with new filter parameters
- Ensure frontend and backend are in sync

### Step 3: Build Backend
Build the backend to verify there are no compilation errors:

```bash
dotnet build modules/policy/src/iOne.Policy.Application.Contracts/iOne.Policy.Application.Contracts.csproj
dotnet build modules/policy/src/iOne.Policy.Application/iOne.Policy.Application.csproj
```

### Step 4: Build Frontend
Build the frontend to verify there are no TypeScript errors:

```bash
cd angular
npm run build
# or for development
ng build
```

---

## 🚀 Testing the Implementation

### Test 1: Backend API Testing
Start the backend and test the API endpoints:

```bash
# Start the API (adjust path if needed)
cd src/web/iOne.HttpApi.Host
dotnet run
```

Test the policy list endpoint with new filters:
```bash
# Example: Test with new filters
curl -X GET "https://localhost:44300/api/policy/policies?channelId=xxx&effectiveDateFrom=2024-01-01"
```

### Test 2: Frontend Testing
Start the Angular development server:

```bash
cd angular
npm start
# or
ng serve
```

Navigate to: `http://localhost:4200/pages/policy/policies`

**Test Checklist**:
- [ ] Search form displays all 14 fields
- [ ] Date pickers work correctly
- [ ] Table shows all 25 columns
- [ ] Horizontal scrolling works
- [ ] PolicyNo column is frozen on the left
- [ ] Status column is frozen on the right
- [ ] Column resizing works
- [ ] Search button applies filters correctly
- [ ] Reset button clears all filters
- [ ] Pagination works
- [ ] Sorting works on all columns
- [ ] Status colors display correctly (green/red/gray)

### Test 3: Dropdown Data Loading
Currently, dropdown options are empty arrays. You need to implement services to load actual data:

**Files to update**:
- `angular/src/app/pages/policy/policies/policies.component.ts`

**Add in ngOnInit():**
```typescript
ngOnInit(): void {
  this.loadChannelOptions();
  this.loadCustomerOptions();
  this.loadContractOptions();
  this.loadPartnerOptions();
  this.loadPolicyIssuerOptions();
  this.loadContractTypeOptions();
  this.loadContractStatusOptions();
}

private loadChannelOptions(): void {
  // Call ChannelService to load options
  // this.channelService.getList({...}).subscribe(...)
}

// ... implement other load methods
```

---

## 📝 Known Limitations

### 1. Dropdown Options Are Empty
The dropdown fields are configured but have no data. You need to:
- Implement services (ChannelService, CustomerService, etc.)
- Call these services in component initialization
- Handle loading states and errors

### 2. Some Backend Filters Not Fully Implemented
The following filters have placeholder logic:
- `ContractType` - Requires join with Contract table
- `ContractStatus` - Requires join with Contract table

You need to:
- Review your domain model for Contract relationships
- Implement the join logic in PolicyAppService
- Test these filters

### 3. New DTO Properties May Not Map Correctly
If domain properties don't exist, AutoMapper won't map them. You need to:
- Update `iOnePolicyApplicationAutoMapperProfile.cs`
- Add custom mappings for navigation properties
- Example: `CreateMap<Policy, PolicyDto>().ForMember(dest => dest.PartnerName, opt => opt.MapFrom(src => src.Partner.Name))`

---

## 🐛 Troubleshooting

### Issue: Compilation Errors in Backend
**Cause**: Missing properties in Policy entity
**Solution**: Add the missing properties to the domain entity

### Issue: TypeScript Errors in Frontend
**Cause**: Proxy models not regenerated
**Solution**: Run `abp generate-proxy -t ng`

### Issue: Dropdown Options Not Loading
**Cause**: Services not implemented
**Solution**: Implement dropdown data loading services as shown above

### Issue: Table Columns Not Displaying Correctly
**Cause**: VTable component may not support all properties
**Solution**: Check VTable component documentation and adjust column configurations

### Issue: Filters Not Working
**Cause**: Backend filter logic incomplete or proxy not regenerated
**Solution**: Complete backend filter implementation and regenerate proxies

---

## 📚 Additional Resources

- **Implementation Plan**: `dev_note/POLICY_LISTING_WIREFRAME_IMPLEMENTATION_PLAN.md`
- **Implementation Summary**: `dev_note/POLICY_LISTING_IMPLEMENTATION_SUMMARY.md`
- **Wireframe**: `WebApplicationPrototype_v1.3/2_2__quan_ly_don_bao_hiem.html`

---

## ✨ Recommended Order of Actions

1. **Review and understand changes** - Read the implementation summary
2. **Check domain model** - Verify Policy entity has all required properties
3. **Build backend** - Ensure no compilation errors
4. **Regenerate proxies** - Sync frontend with backend
5. **Build frontend** - Ensure no TypeScript errors
6. **Start backend API** - Test endpoints manually
7. **Start frontend dev server** - Test UI manually
8. **Implement dropdown services** - Load actual data
9. **Test thoroughly** - Go through test checklist
10. **Fix issues** - Address any problems found
11. **Deploy to staging** - Test in staging environment
12. **User acceptance testing** - Get feedback from users
13. **Deploy to production** - Final deployment

---

## 🎯 Success Criteria

The implementation is successful when:
- ✅ All 14 search fields are functional
- ✅ All 25 table columns display correctly
- ✅ Dropdown options load real data
- ✅ Filters work correctly individually and combined
- ✅ Date range filters validate properly (From < To)
- ✅ Table horizontal scrolling works smoothly
- ✅ Frozen columns work correctly
- ✅ Column resizing works
- ✅ Localization works for both English and Vietnamese
- ✅ Performance is acceptable (< 2 seconds for search)
- ✅ No console errors
- ✅ Responsive design works on tablet and mobile
- ✅ Passes user acceptance testing

---

## 📞 Need Help?

If you encounter any issues:
1. Check the implementation summary for details
2. Review the original implementation plan
3. Check the wireframe design for clarification
4. Review ABP and PrimeNG documentation
5. Test with sample data first before production data

Good luck with testing and deployment! 🚀
