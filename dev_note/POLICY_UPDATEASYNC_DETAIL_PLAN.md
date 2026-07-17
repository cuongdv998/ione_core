# Plan: Implement Policy UpdateAsync (same shape as create, dedicated DTO)

## Goal

Implement `PUT /api/policy/policies/{id}` update logic so the **update payload mirrors create payload** (nested objects), but with these constraints:

- **Do not allow updating Contract**
  - No creating/updating `PolicyContract`
  - No changing `policy.contractId`
  - No changing **contract type**
- **Do not allow updating `policyNo`**
- **Do not allow updating version**
  - No creating a new `PolicyVersion`
  - No changing `policy.lastVersionId`
  - No changing `policy_version.version` (the numeric version)

We will create **new DTOs for update** (do not reuse create DTOs).

---

## 1. API surface

### 1.1 Controller signature

Current:
- `PolicyController.UpdateAsync(Guid id, UpdatePolicyDto input)`

Change to:
- `PolicyController.UpdateAsync(Guid id, UpdatePolicyDetailDto input)`

### 1.2 App service signature

Current `IPolicyAppService` is `ICrudAppService<PolicyDto, Guid, GetPoliciesInput, CreatePolicyDto, UpdatePolicyDto>`.

Plan:
- Either:
  - **Option A (preferred)**: keep CRUD generic but **override** `UpdateAsync(Guid id, UpdatePolicyDetailDto input)` in `PolicyAppService` and expose it in `IPolicyAppService` as an extra method, while leaving CRUD `UpdatePolicyDto` unused (or used by legacy callers).
  - **Option B**: change the CRUD generic update DTO to `UpdatePolicyDetailDto` (bigger breaking change for FE proxies).

Given FE already calls `policyService.update(id, UpdatePolicyDto)`, Option A allows incremental migration.

---

## 2. New Update DTOs (do NOT reuse create DTOs)

Create these files under:
- `modules/policy/src/iOne.Policy.Application.Contracts/Policies/Update/`

### 2.1 `UpdatePolicyDetailDto`

Same “business fields” as create, **excluding**:
- `policyNo` (immutable)
- `contractId` + `contract` object (immutable)
- `version` object and any fields that imply version change
- `lastVersionId` (immutable; derived from existing policy)

Include:
- Base editable policy fields (sellType, insurerPolicyNo, policyTypeId, partnerId, sellerId, implementerId, currencyId, exchangeRate, status, approvalStatus, orgEffectDate, orgExpireDate, isRenewal/isGift/isBankLoan, channelId, lotImportCode, insured + beneficiary fields, etc.)
- Nested:
  - `amount?: UpdatePolicyAmountInputDto`
  - `products?: List<UpdatePolicyProductInputDto>`
  - `riskObject?: UpdatePolicyRiskObjectInputDto`
  - `documents?: List<UpdatePolicyDocumentInputDto>`

### 2.2 `UpdatePolicyAmountInputDto`

Align with what you want editable on update:
- `premiumTotal`, `premium`, `vat`, `discount`, `discountRate`
- (optional later) fee item fields if update screen supports them

### 2.3 `UpdatePolicyProductInputDto`

Required:
- `productId: Guid`

Editable:
- financial totals + insurerProductCode + amountLiability
- `coverages?: List<UpdatePolicyCoverageInputDto>`

### 2.4 `UpdatePolicyCoverageInputDto`

Required:
- `coverageId: Guid`

Editable:
- amountLiability, quantity, premiumRate, premium, vat, taxId, uomId, rates, etc.
- `coverageLevels?: List<UpdatePolicyCoverageLevelInputDto>` (if update form supports editing them)

### 2.5 `UpdatePolicyCoverageLevelInputDto`

Required matching keys:
- Use `(coverageLevelTypeId, coverageLevelBasisId)` as identity, because the UI likely won’t carry `policyCoverageLevelId`.

Editable:
- amountType/from/to/scripts

### 2.6 `UpdatePolicyRiskObjectInputDto` + `UpdatePolicyRiskMotorInputDto`

Editable:
- objectTypeId + rep fields + risk object address + lat/long
- motor codes + plate/vin/engine/value/etc.

### 2.7 `UpdatePolicyDocumentInputDto`

- `documentId: Guid` (ResDocument id)

---

## 3. Update algorithm (server-side)

### 3.1 Load graph (single transaction / UOW)

Load `Policy` with:
- `Contract`
- `PolicyVersions` (+ `PolicyProducts` → `PolicyCoverages` → `PolicyCoverageLevels`)
- `PolicyVersions` (+ `PolicyRiskObjects` → `PolicyRiskMotors`)
- `PolicyDocuments`
- (optional) `PolicyAmounts` (if we store totals there)

Then:
- `currentVersion = policy.PolicyVersions.SingleOrDefault(v => v.Id == policy.LastVersionId && !v.IsDeleted)`
- If missing: return a business error (update requires an existing version).

### 3.2 Enforce immutability rules

Hard rules:
- Ignore/forbid any attempt to change:
  - `policy.PolicyNo`
  - `policy.ContractId`
  - contract itself
  - `policy.LastVersionId`
  - `currentVersion.Version` (numeric version)

Implementation:
- DTO does not contain those fields (preferred).
- Additionally, validate server-side:
  - `policy.ContractId` must remain unchanged
  - `policy.PolicyNo` must remain unchanged

### 3.3 Update policy base fields (allowed fields only)

Update fields from `UpdatePolicyDetailDto`:
- sellType, insurerPolicyNo, policyTypeId, partnerId, sellerId, implementerId
- currencyId, exchangeRate
- status, approvalStatus
- orgEffectDate/orgExpireDate + flags
- insured + beneficiary info, channelId, lotImportCode, etc.

Note:
- `issueDate`: decide if editable; often not editable. If allowed, update; otherwise ignore.

### 3.4 Update `PolicyVersion` totals/notes (without changing version number)

Even though we “don’t update version”, we still need to update **fields on the existing version record** that the UI edits (notes, totals, dates), while keeping `Version` (numeric) unchanged.

Allowed updates on currentVersion:
- Effect/Expire/OrgEffect/OrgExpire (if your UI edits insurance period)
- InternalNote/CustomerNote
- PremiumTotal/Premium/Vat/Discount/DiscountRate (or computed from products)

### 3.5 Update products / coverages / coverage levels (within currentVersion)

Strategy: **Upsert + soft-delete**

#### Products
- Build a map of existing `PolicyProduct` by `ProductId` (only not deleted).
- For each `UpdatePolicyProductInputDto`:
  - If exists: update allowed fields.
  - Else: create new `PolicyProduct` linked to `currentVersion.Id`.
- Any existing product not present in input: soft-delete it (and its coverages/levels).

#### Coverages
For each product:
- Match existing coverages by `CoverageId` (and optionally `CoverageParentId` if required by your model).
- Upsert fields.
- Soft-delete coverages missing from input.

#### CoverageLevels
For each coverage:
- Match existing levels by `(CoverageLevelTypeId, CoverageLevelBasisId)`.
- Upsert fields.
- Soft-delete missing levels.

### 3.6 Update risk object + motor (within currentVersion)

Strategy: “single risk object” (pick first non-deleted).
- If `input.RiskObject` is null:
  - Keep existing unchanged (unless the UI supports clearing it, then soft-delete).
- Else:
  - If an existing riskObject exists: update fields.
  - Else: create new riskObject linked to `currentVersion.Id`.

RiskMotor:
- Similar upsert: update the first motor or create if missing.

### 3.7 Update documents (`policy_document`)

Input: list of `ResDocument` ids.
- Validate all document ids exist in `ResDocument`.
- Compare to existing `PolicyDocument` (not deleted):
  - Add missing links
  - Soft-delete removed links

### 3.8 Update amount (`policy_amount`) (optional but recommended)

If you treat `policy_amount` as the persisted summary:
- Find latest `PolicyAmount` for `(PolicyId, PolicyVersionId)`
  - If exists: update totals
  - Else: create a new record

### 3.9 Return

Return `PolicyDto` (detail shape) by either:
- calling the same mapping used by `GetAsync(id)` logic, or
- returning a fresh `GetAsync(id)` after update.

---

## 4. Data mapping notes (FE → UpdatePolicyDetailDto)

- `contractId` / contract fields: **not sent** on update.
- `policyNo`: **not sent** on update.
- `versionDetail.version`: **not sent**; server keeps existing numeric version.
- Products:
  - include only selected products
  - coverages include selected + main coverages
  - include `amountLiability` from insurance amount input

---

## 5. Files to change (expected)

### Backend
- `modules/policy/src/iOne.Policy.HttpApi/Controllers/PolicyController.cs`
  - Update signature to accept `UpdatePolicyDetailDto` (or add a new endpoint if keeping legacy update).
- `modules/policy/src/iOne.Policy.Application/Policies/PolicyAppService.cs`
  - Implement `UpdateAsync(Guid id, UpdatePolicyDetailDto input)` with above algorithm.
- `modules/policy/src/iOne.Policy.Application.Contracts/Policies/IPolicyAppService.cs`
  - Add new method signature if using Option A.
- `modules/policy/src/iOne.Policy.Application.Contracts/Policies/Update/*`
  - Add all new update DTO files.
- `modules/policy/src/iOne.Policy.Application/iOnePolicyApplicationAutoMapperProfile.cs`
  - Add/ignore mappings as needed (but update likely manual like create).

---

## 6. Open decisions (for your review)

1. **Should update allow changing insurance period dates?**
   - If yes: update both `policy.OrgEffect/OrgExpire` and `currentVersion.OrgEffect/OrgExpire` (and `Effect/Expire` if applicable).
2. **Should update allow changing `status` and `issueDate`?**
3. **When products are removed**, do we soft-delete the old records or keep them?
4. **CoverageLevels**: does UI edit them? If not, we can keep existing levels unchanged unless explicitly provided.

