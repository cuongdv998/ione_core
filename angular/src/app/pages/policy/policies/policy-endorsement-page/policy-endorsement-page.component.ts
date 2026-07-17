import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { DatePickerModule } from 'primeng/datepicker';
import { ToastModule } from 'primeng/toast';
import { DialogModule } from 'primeng/dialog';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TextareaModule } from 'primeng/textarea';
import { InputNumberModule } from 'primeng/inputnumber';
import { FileUploadModule } from 'primeng/fileupload';
import { TableModule } from 'primeng/table';
import { TooltipModule } from 'primeng/tooltip';
import { CheckboxModule } from 'primeng/checkbox';

import { PoliciesComponent } from '../policies.component';
import type { PolicyDto, UpdatePolicyDetailDto } from '@/proxy/policy/policies/models';
import { PolicyContractType } from '@/proxy/policy-contracts/policy-contract-type.enum';

import { Router } from '@angular/router';
import { MessageService, ConfirmationService } from 'primeng/api';
import { PermissionService } from '@/core/services/permission.service';
import { LocalizationService } from '@/core/services/localization.service';
import { ConfigStateService } from '@abp/ng.core';
import { RestService } from '@abp/ng.core';
import { PolicyService } from '@/proxy/policy/controllers/policy.service';
import { ResPartnerService } from '@/proxy/partner/controllers/res-partner.service';
import { ProLineOfBusinessService } from '@/proxy/product/controllers/pro-line-of-business.service';
import { ResCustomerService } from '@/proxy/customer/controllers/res-customer.service';
import { ResCurrencyService } from '@/proxy/master/controllers/res-currency.service';
import { ResChannelService } from '@/proxy/partner/controllers/res-channel.service';
import { HrEmployeeService } from '@/proxy/hr/controllers/hr-employee.service';
import { ResProvinceService } from '@/proxy/master/controllers/res-province.service';
import { ResWardService } from '@/proxy/master/controllers/res-ward.service';
import { PolicyTypeService } from '@/proxy/policy/controllers/policy-type.service';
import { PolicyContractService } from '@/proxy/policy/controllers/policy-contract.service';
import { PolicyCertificateService } from '@/proxy/policy/controllers/policy-certificate.service';
import { ResCarBrandService } from '@/proxy/master/controllers/res-car-brand.service';
import { ResCarCategoryService } from '@/proxy/master/controllers/res-car-category.service';
import { ResCarLineService } from '@/proxy/master/controllers/res-car-line.service';
import { ResCarGroupService } from '@/proxy/master/controllers/res-car-group.service';
import { ResCarTypeService } from '@/proxy/master/controllers/res-car-type.service';
import { AdminConfigService } from '@/proxy/master/controllers/admin-config.service';
import { AdminConfigStatus } from '@/proxy/admin-configs/admin-config-status.enum';
import { ResOrganizationTypeService } from '@/proxy/partner/controllers/res-organization-type.service';
import { ResIndustryService } from '@/proxy/customer/controllers/res-industry.service';
import { ProProductService } from '@/proxy/product/pro-products/pro-product.service';
import { ResDocumentService } from '@/proxy/master/controllers/res-document.service';
import { ResDocumentTypeService } from '@/proxy/master/controllers/res-document-type.service';
import { ResObjectTypeService } from '@/proxy/master/controllers/res-object-type.service';
import { ResReasonService } from '@/proxy/master/controllers/res-reason.service';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { ReportTemplateService } from '@/proxy/report/controllers/report-template.service';
import { MulticolumnComboboxComponent } from '@/shared/components/multicolumn-combobox/multicolumn-combobox.component';

@Component({
  selector: 'app-policy-endorsement-page',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    PanelModule,
    ButtonModule,
    InputTextModule,
    SelectModule,
    DatePickerModule,
    ToastModule,
    DialogModule,
    ConfirmDialogModule,
    TextareaModule,
    InputNumberModule,
    FileUploadModule,
    TableModule,
    TooltipModule,
    CheckboxModule,
    TranslatePipe,
    MulticolumnComboboxComponent
  ],
  templateUrl: './policy-endorsement-page.component.html',
  styleUrl: './policy-endorsement-page.component.scss',
  providers: [MessageService, ConfirmationService]
})
export class PolicyEndorsementPageComponent extends PoliciesComponent implements OnInit {
  /** Endorsement section: reason, type, description (only on this page) */
  endorsementReasonId: string | null = null;
  endorsementTypeId: string | null = null;
  endorsementDescription: string | null = null;
  endorsementReasonOptions: Array<{ label: string; value: string }> = [];
  /** value = config.id (Guid) for API; code = config.value for section editability logic */
  endorsementTypeOptions: Array<{ label: string; value: string; code: string }> = [];

  /** Section editability flags set by onEndorsementTypeChange (single source of truth). */
  isContractInfoEditable = false;
  isGeneralInfoEditable = false;
  isEffectDateEditable = false;
  isInsuredObjectEditable = false;
  isCoverageEditable = false;
  isAllInforEditable = false;

  constructor(
    private route: ActivatedRoute,
    policyService: PolicyService,
    messageService: MessageService,
    confirmationService: ConfirmationService,
    reportTemplateService: ReportTemplateService,
    permissionService: PermissionService,
    localizationService: LocalizationService,
    router: Router,
    partnerService: ResPartnerService,
    lobService: ProLineOfBusinessService,
    customerService: ResCustomerService,
    currencyService: ResCurrencyService,
    channelService: ResChannelService,
    employeeService: HrEmployeeService,
    provinceService: ResProvinceService,
    wardService: ResWardService,
    policyTypeService: PolicyTypeService,
    policyContractService: PolicyContractService,
    carBrandService: ResCarBrandService,
    carCategoryService: ResCarCategoryService,
    carLineService: ResCarLineService,
    carGroupService: ResCarGroupService,
    carTypeService: ResCarTypeService,
    adminConfigService: AdminConfigService,
    organizationTypeService: ResOrganizationTypeService,
    industryService: ResIndustryService,
    productService: ProProductService,
    restService: RestService,
    configState: ConfigStateService,
    resDocumentService: ResDocumentService,
    resDocumentTypeService: ResDocumentTypeService,
    resObjectTypeService: ResObjectTypeService,
    policyCertificateService: PolicyCertificateService,
    private reasonService: ResReasonService,
    cdr: ChangeDetectorRef
  ) {
    super(
      policyService,
      messageService,
      confirmationService,
      reportTemplateService,
      permissionService,
      localizationService,
      router,
      partnerService,
      lobService,
      customerService,
      currencyService,
      channelService,
      employeeService,
      provinceService,
      wardService,
      policyTypeService,
      policyContractService,
      carBrandService,
      carCategoryService,
      carLineService,
      carGroupService,
      carTypeService,
      adminConfigService,
      organizationTypeService,
      industryService,
      productService,
      restService,
      configState,
      resDocumentService,
      resDocumentTypeService,
      resObjectTypeService,
      policyCertificateService,
      cdr
    );
  }

  /** Endorsement type values from admin_config (code = ENDORSEMENT_POLICY_TYPE). */
  static readonly ENDORSEMENT_TYPE_RISK_OBJECT = 'RISK_OBJECT_ENDORSEMENT';
  static readonly ENDORSEMENT_TYPE_EFFECT_DATE = 'POLICY_EFFECT_DATE_ENDORSEMENT';
  static readonly ENDORSEMENT_TYPE_COVERAGE = 'POLICY_COVERAGE_ENDORSEMENT';
  static readonly ENDORSEMENT_TYPE_COMMON_INFOR = 'POLICY_COMMON_INFOR_ENDORSEMENT';
  static readonly ENDORSEMENT_TYPE_ALL_INFOR = 'POLICY_ALL_INFOR_ENDORSEMENT';

  /** View-only mode when opened via "xem chi tiết" for endorsement (query view=1). */
  endorsementViewMode = false;

  /** Record History modal state */
  recordHistoryModalVisible = false;
  workTaskHistoryList: any[] = [];
  recordHistoryLoading = false;

  /** True so the top endorsement section (reason, type, description) stays editable; false when view-only. */
  get isEndorsementSectionEditable(): boolean {
    return !this.endorsementViewMode;
  }

  /**
   * On endorsement page, section editability is driven by endorsementTypeId;
   * onEndorsementTypeChange updates the section flags used by the template.
   */

  override ngOnInit(): void {
    super.ngOnInit();

    this.loading = false;

    const id = (this.route.snapshot.paramMap.get('id') || '').trim();
    const versionId = (this.route.snapshot.queryParamMap.get('versionId') || '').trim() || undefined;
    const viewParam = (this.route.snapshot.queryParamMap.get('view') || '').trim();
    this.endorsementViewMode = viewParam === '1';

    if (!id) {
      this.backToList();
      return;
    }

    // Load endorsement type options from admin_config (code = ENDORSEMENT_POLICY_TYPE)
    this.loadEndorsementTypeOptions();
    // Load endorsement reason options from res_reason (group code = ENDORSEMENT_POLICY_REASON)
    this.loadEndorsementReasonOptions();

    const stub = { id, lastVersionId: versionId } as PolicyDto;
    if (this.endorsementViewMode) {
      // Xem chi tiết đơn SĐBS: mở read-only (giống nút sửa nhưng chỉ xem)
      this.openViewDialog(stub);
    } else {
      this.openEditDialog(stub);
    }
    // Apply initial section editability (view mode => all false; edit => from endorsement type)
    this.onEndorsementTypeChange(this.endorsementTypeId);
  }

  /**
   * CSS classes for policy approval status in footer (expose for template).
   */
  override getPolicyApprovalStatusFooterClass(approvalStatus: string | null | undefined): string {
    return super.getPolicyApprovalStatusFooterClass(approvalStatus);
  }

  /**
   * Giá trị tổng tăng/giảm phí hiển thị:
   * - Đang sửa SĐBS: luôn tính từ form (coverage premiumChange + SP bỏ chọn) để khớp khi thêm/bớt sản phẩm.
   * - Chỉ xem (view=1): ưu tiên policy_amount.endorsementAdjustmentAmount từ API; không có thì tính từ form.
   */
  get displayTotalPremiumChange(): number {
    if (!this.endorsementViewMode) {
      return this.calculateTotalPremiumChange();
    }
    const fromAmount = this.currentPolicyDetail?.amount?.endorsementAdjustmentAmount;
    if (fromAmount != null && typeof fromAmount === 'number') {
      return fromAmount;
    }
    return this.calculateTotalPremiumChange();
  }

  /**
   * Calculate total Tăng/Giảm phí (premiumChange) from:
   * - Selected products: sum of coverage.premiumChange
   * - Deselected products: -(premiumOrigin + vatOrigin) for each coverage (phản ánh giảm phí do bỏ sản phẩm)
   */
  protected calculateTotalPremiumChange(): number {
    const selectedProductIds = this.getSelectedProductIds();
    const selectedSet = new Set(selectedProductIds);
    let total = 0;
    // Selected: sum premiumChange
    for (const productId of selectedProductIds) {
      const coverages = this.getAllCoveragesWithSections(productId);
      for (const item of coverages) {
        if (!item.isSectionHeader && item.premiumChange != null) {
          total += item.premiumChange;
        }
      }
    }
    // Deselected: subtract full original premium (giảm phí do bỏ sản phẩm)
    const allProductIds = Object.keys(this.productCoverages || {});
    for (const productId of allProductIds) {
      if (selectedSet.has(productId)) continue;
      const coverages = this.getAllCoveragesWithSections(productId);
      for (const item of coverages) {
        if (item.isSectionHeader) continue;
        const originVat = (item.premiumOrigin ?? 0) + (item.vatOrigin ?? 0);
        total -= originVat;
      }
    }
    return total;
  }

  /**
   * Build list of per-coverage endorsement changes (Tăng/Giảm phí) for the payload.
   * Each item has coverageId and changeAmount (premiumVat - premiumVat snapshot). Only non-zero changes.
   */
  protected getEndorsementCoverageChangesList(): Array<{ coverageId: string; changeAmount: number }> {
    const list: Array<{ coverageId: string; changeAmount: number }> = [];
    const selectedProductIds = this.getSelectedProductIds();
    for (const productId of selectedProductIds) {
      const coverages = this.getAllCoveragesWithSections(productId);
      for (const item of coverages) {
        if (item.isSectionHeader) continue;
        if (item.premiumChange == null || item.premiumChange === undefined) continue;
        if (item.premiumChange === 0) continue;
        const coverageId = item.coverageId;
        if (!coverageId) continue;
        list.push({ coverageId: String(coverageId), changeAmount: item.premiumChange });
      }
    }
    return list;
  }

  override populateFormFromDetail(detail: any): void {
    super.populateFormFromDetail(detail);
    const v = detail?.versionDetail;
    if (v) {
      this.endorsementTypeId = v.endorsementType ?? null;
      this.endorsementReasonId = v.endorsementReasonId ?? null;
      this.endorsementDescription = v.endorsementDescription ?? null;
      this.onEndorsementTypeChange(this.endorsementTypeId);
    }
  }

  /**
   * Load Endorsement Type options from admin_config with code = ENDORSEMENT_POLICY_TYPE
   */
  private loadEndorsementTypeOptions(): void {
    this.adminConfigService.getList({
      code: 'ENDORSEMENT_POLICY_TYPE',
      status: AdminConfigStatus.Active,
      maxResultCount: 1000,
      skipCount: 0,
      sorting: 'name asc'
    }).subscribe({
      next: (result) => {
        // value = config.id (Guid) for saving to policy_version.endorsement_type; code = config.value for onEndorsementTypeChange logic
        const options = (result.items || []).map(config => ({
          label: config.name || '',
          value: config.id || '',
          code: config.value || ''
        }));
        this.endorsementTypeOptions = options.sort((a, b) => (a.label || '').localeCompare(b.label || '', undefined, { sensitivity: 'base' }));
        // Re-apply section editability (e.g. when options load after policy detail already set endorsementTypeId)
        this.onEndorsementTypeChange(this.endorsementTypeId);
      },
      error: () => {
        // Silently fail
      }
    });
  }

  /**
   * Load Endorsement Reason options from res_reason using group code = ENDORSEMENT_POLICY_REASON
   */
  private loadEndorsementReasonOptions(): void {
    this.reasonService.getSelectListByGroupCode('ENDORSEMENT_POLICY_REASON').subscribe({
      next: (result: any) => {
        this.endorsementReasonOptions = (result || []).map((reason: any) => ({
          label: reason.name || '',
          value: reason.id || ''
        }));
      },
      error: () => {
        // Silently fail
        this.endorsementReasonOptions = [];
      }
    });
  }

  /** Trả về versionId hợp lệ khi đang sửa version cụ thể (để backend dùng đúng policy_version, tránh PolicyProduct:NotFound). */
  private getValidVersionIdForEndorsement(): string | undefined {
    const v = (this.formData.lastVersionId || this.currentPolicyDetail?.lastVersionId)?.trim();
    if (!v || v === '0') return undefined;
    return v;
  }

  /**
   * Handle endorsement type change - updates form section editability.
   * Dropdown value = config.id (Guid); we resolve config.value (code) from options to drive section logic.
   * Contract Info: enabled for COMMON_INFOR or ALL_INFOR
   * General Info: enabled for COMMON_INFOR or ALL_INFOR
   * Effect Date: enabled for EFFECT_DATE or ALL_INFOR
   * Insured Object: enabled for RISK_OBJECT or ALL_INFOR
   * Coverage: enabled for COVERAGE or ALL_INFOR
   * Notes/Attachments: enabled only for ALL_INFOR
   */
  onEndorsementTypeChange(newTypeId: string | null): void {
    if (this.endorsementViewMode) {
      this.isContractInfoEditable = false;
      this.isGeneralInfoEditable = false;
      this.isEffectDateEditable = false;
      this.isInsuredObjectEditable = false;
      this.isCoverageEditable = false;
      this.isAllInforEditable = false;
      return;
    }
    const id = (newTypeId ?? '').trim();
    const option = this.endorsementTypeOptions.find(opt => opt.value === id);
    const t = option?.code?.trim() ?? '';
    const { ENDORSEMENT_TYPE_COMMON_INFOR, ENDORSEMENT_TYPE_ALL_INFOR, ENDORSEMENT_TYPE_EFFECT_DATE,
      ENDORSEMENT_TYPE_RISK_OBJECT, ENDORSEMENT_TYPE_COVERAGE } = PolicyEndorsementPageComponent;

    this.isContractInfoEditable =
      t === ENDORSEMENT_TYPE_COMMON_INFOR || t === ENDORSEMENT_TYPE_ALL_INFOR;
    this.isGeneralInfoEditable =
      t === ENDORSEMENT_TYPE_COMMON_INFOR || t === ENDORSEMENT_TYPE_ALL_INFOR;
    this.isEffectDateEditable =
      t === ENDORSEMENT_TYPE_EFFECT_DATE || t === ENDORSEMENT_TYPE_ALL_INFOR;
    this.isInsuredObjectEditable =
      t === ENDORSEMENT_TYPE_RISK_OBJECT || t === ENDORSEMENT_TYPE_ALL_INFOR;
    this.isCoverageEditable =
      t === ENDORSEMENT_TYPE_COVERAGE || t === ENDORSEMENT_TYPE_ALL_INFOR;
    this.isAllInforEditable = t === ENDORSEMENT_TYPE_ALL_INFOR;
  }

  openRecordHistoryModal(): void {
    const policyVersionId = (
      this.currentPolicyDetail?.lastVersionId ??
      this.formData?.lastVersionId ??
      this.selectedPolicy?.lastVersionId
    ) as string | undefined;
    const versionId = (policyVersionId ?? '').toString().trim();
    if (!versionId || versionId === '0') return;
    this.recordHistoryModalVisible = true;
    this.recordHistoryLoading = true;
    this.workTaskHistoryList = [];
    this.policyService.getWorkTaskHistory(versionId).subscribe({
      next: (list) => {
        this.workTaskHistoryList = list ?? [];
        this.recordHistoryLoading = false;
      },
      error: (err) => {
        this.recordHistoryLoading = false;
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: err?.error?.error?.message ?? this.localizationService.localize('AbpUi::Error')
        });
      }
    });
  }

  backToList(): void {
    this.navigateToPolicyList();
  }

  confirmCancel(): void {
    this.confirmationService.confirm({
      message: this.localizationService.localize('Policy::Policy:UnsavedExitConfirm'),
      header: this.localizationService.localize('Policy::Warning'),
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: this.localizationService.localize('AbpUi::Yes'),
      rejectLabel: this.localizationService.localize('AbpUi::No'),
      rejectButtonStyleClass: 'p-button-secondary',
      accept: () => this.backToList()
    });
  }

  protected override afterSaveSuccess(): void {
    this.backToList();
  }

  /** Lưu nháp: lưu endorsement không gửi duyệt Elsa */
  async saveDraft(): Promise<void> {
    if (!this.validateForm()) return;
    this.loading = true;
    try {
      await this.flushSelectedProductsPremiumRecalc();
    } catch {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Policy::Error'),
        detail: 'Failed to calculate premium'
      });
      this.loading = false;
      return;
    }
    this.submitEndorsement(false);
  }

  /** Lưu và Duyệt: lưu endorsement và gửi duyệt Elsa */
  override saveAndApprove(): void {
    if (!this.validateForm()) return;
    this.confirmationService.confirm({
      message: this.localizationService.localize('Policy::PolicyEndorsement:SaveAndApproveConfirm'),
      header: this.localizationService.localize('AbpUi::Warning'),
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: this.localizationService.localize('AbpUi::Yes'),
      rejectLabel: this.localizationService.localize('AbpUi::No'),
      accept: () => {
        this.loading = true;
        this.flushSelectedProductsPremiumRecalc().then(() => {
          this.submitEndorsement(true);
        }).catch(() => {
          this.messageService.add({
            severity: 'error',
            summary: this.localizationService.localize('Policy::Error'),
            detail: 'Failed to calculate premium'
          });
          this.loading = false;
        });
      }
    });
  }

  /**
   * Endorsement update - similar to update() but calls endorsement API endpoint.
   * @param submitForApproval true = Lưu và Duyệt (gọi Elsa), false = Lưu nháp (không gọi Elsa)
   */
  private submitEndorsement(submitForApproval: boolean): void {
    if (!this.selectedPolicy || !this.selectedPolicy.id) {
      return;
    }

    this.loading = true;

    // For snapshot editing, we rely on the formData names directly
    const insurancePeriodFrom = this.ensureDate(this.formData.insurancePeriodFrom);
    const insurancePeriodTo = this.ensureDate(this.formData.insurancePeriodTo);

    const selectedProductIds = this.getSelectedProductIds();
    const totals = this.getSelectedProductsTotals(selectedProductIds);

    // Build products payload from UI, then attach policy_product/policy_coverage IDs from loaded detail (if present)
    const detail: any = this.currentPolicyDetail;
    const productIdToPolicyProductId: Record<string, string> = {};
    const coverageIdToPolicyCoverageIdByProductId: Record<string, Record<string, string>> = {};
    const coverageIdToCoverageLevelsByProductId: Record<string, Record<string, any[]>> = {};

    if (detail?.products?.length) {
      detail.products.forEach((p: any) => {
        if (p?.productId && p?.id) {
          productIdToPolicyProductId[p.productId] = p.id;
        }
        const covMap: Record<string, string> = {};
        const lvlMap: Record<string, any[]> = {};
        (p?.coverages || []).forEach((c: any) => {
          if (c?.coverageId && c?.id) {
            covMap[c.coverageId] = c.id;
          }
          if (c?.coverageId && Array.isArray(c?.coverageLevels)) {
            lvlMap[c.coverageId] = c.coverageLevels;
          }
        });
        if (p?.productId) {
          coverageIdToPolicyCoverageIdByProductId[p.productId] = covMap;
          coverageIdToCoverageLevelsByProductId[p.productId] = lvlMap;
        }
      });
    }

    const productsPayload = selectedProductIds.map(productId => {
      const payload = this.buildPolicyProductPayload(productId);
      payload.id = productIdToPolicyProductId[productId] || undefined;

      const covIdMap = coverageIdToPolicyCoverageIdByProductId[productId] || {};
      const lvlMap = coverageIdToCoverageLevelsByProductId[productId] || {};
      payload.coverages = (payload.coverages || []).map((c: any) => ({
        ...c,
        id: covIdMap[c.coverageId] || undefined,
        // Preserve existing coverageLevels, but upsert deductible level based on current UI selection.
        coverageLevels: this.mergeCoverageLevels(
          Array.isArray(lvlMap[c.coverageId]) ? lvlMap[c.coverageId] : [],
          Array.isArray(c.coverageLevels) ? c.coverageLevels : []
        ),
      }));

      return payload;
    });

    const riskMotorPayload = {
      ...this.buildRiskMotorPayload(),
      id: detail?.riskObject?.riskObjectMotor?.id || undefined,
    };

    const riskObjectPayload = {
      id: detail?.riskObject?.id || undefined,
      objectTypeId: this.carObjectTypeId,
      // Update RiskObject with vehicle owner data from form (snapshot)
      repName: this.formData.vehicleOwnerName || undefined,
      repIdNo: this.formData.vehicleOwnerIdNo || undefined,
      repPassport: undefined,
      repPhone: this.formData.vehicleOwnerPhone || undefined,
      repEmail: this.formData.vehicleOwnerEmail || undefined,
      repProvinceId: this.formData.vehicleOwnerProvinceId || undefined,
      repWardId: this.formData.vehicleOwnerWardId || undefined,
      repAddress: this.formData.vehicleOwnerAddress || undefined,
      repFullAddress: this.formData.vehicleOwnerFullAddress || undefined,
      riskObjectProvinceId: this.formData.vehicleOwnerProvinceId || undefined,
      riskObjectWardId: this.formData.vehicleOwnerWardId || undefined,
      riskObjectAddress: this.formData.vehicleOwnerAddress || undefined,
      riskObjectFullAddress: this.formData.vehicleOwnerFullAddress || undefined,
      riskObjectLat: detail?.riskObject?.riskObjectLat ?? undefined,
      riskObjectLong: detail?.riskObject?.riskObjectLong ?? undefined,
      // Always send current list so backend can sync Ảnh xe (add new, remove deleted).
      documents: this.getCarPhotoDocumentIds().map(id => ({ documentId: id })),
      riskObjectMotor: riskMotorPayload,
    };

    const isIndividualContract = this.formData.contractType === PolicyContractType.Individual;
    const hasContract = !!(this.formData.contractId?.trim());
    let contractPayload: any = undefined;
    if (isIndividualContract) {
      contractPayload = {
        code: this.formData.contractNo,
        type: this.formData.contractType,
        name: this.formData.contractName || undefined,
        insurerId: this.formData.primaryInsurancePartnerId?.trim() || null,
        lobId: this.formData.lobId?.trim() || null,
        customerId: this.formData.customerId?.trim() || null,
        effectDate: this.toLocalIsoDateTime(insurancePeriodFrom) || null,
        expireDate: this.toLocalIsoDateTime(insurancePeriodTo) || null,
        isReciveInvoice: this.formData.isReceiveInvoice ? 'Y' : 'N',
        payerName: this.formData.payerName || undefined,
        payerEmail: this.formData.payerEmail || undefined,
        payerPhone: this.formData.payerPhone || undefined,
        payerProvinceId: this.formData.payerProvinceId || undefined,
        payerWardId: this.formData.payerWardId || undefined,
        payerAddress: this.formData.payerAddress || undefined,
        payerFullAddress: this.formData.payerFullAddress || undefined,
        documents: this.uploadedContractDocumentIds.map(id => ({ documentId: id })),
      };
    } else if (hasContract || this.uploadedContractDocumentIds.length > 0) {
      contractPayload = {
        documents: this.uploadedContractDocumentIds.map(id => ({ documentId: id }))
      };
    }

    // Build endorsement DTO - similar to update but includes endorsement fields in version
    const endorsementDto: any = {
      contractId: this.formData.contractId?.trim() || undefined,
      contract: contractPayload,
      lobId: this.formData.lobId?.trim() || null,
      sellType: this.formData.sellType,
      insurerPolicyNo: this.formData.insurerPolicyNo?.trim() || undefined,
      certificateNo: this.formData.certificateNo?.trim() || undefined,
      policyTypeId: this.formData.policyTypeId?.trim() || undefined,
      partnerId: this.formData.primaryInsurancePartnerId?.trim() || undefined,
      sellerId: this.formData.sellerId?.trim() || null,
      implementerId: this.formData.implementerId?.trim() || null,
      currencyId: this.formData.currencyId?.trim() || null,
      exchangeRate: this.formData.exchangeRate,
      status: this.formData.status,
      issueDate: this.formData.issueDate?.trim() || undefined,
      approvalStatus: this.formData.approvalStatus?.trim() || undefined,
      orgEffectDate: this.toLocalIsoDateTime(insurancePeriodFrom) || null,
      orgExpireDate: this.toLocalIsoDateTime(insurancePeriodTo) || null,
      isRenewal: this.formData.isRenewal ? 'Y' : 'N',
      isGift: this.formData.isGift || 'N',
      premiumTotal: totals.premiumTotal,
      premium: totals.premium,
      vat: totals.vat,
      discount: 0,
      discountRate: 0,
      markup: 0,
      isBankLoan: this.formData.isBankLoan ? 'Y' : 'N',
      channelId: this.formData.channelId?.trim() || undefined,

      insuredName: this.formData.insuredName?.trim() || undefined,
      insuredIdNo: this.formData.insuredIdNo?.trim() || undefined,
      insuredPhone: this.formData.insuredPhone?.trim() || undefined,
      insuredEmail: this.formData.insuredEmail?.trim() || undefined,
      insuredProvinceId: this.formData.insuredProvinceId?.trim() || undefined,
      insuredWardId: this.formData.insuredWardId?.trim() || undefined,
      insuredAddress: this.formData.insuredAddress?.trim() || undefined,
      insuredFullAddress: this.formData.insuredFullAddress?.trim() || undefined,

      beneficiaryName: this.formData.beneficiaryName?.trim() || undefined,
      beneficiaryIdNo: this.formData.beneficiaryIdNo?.trim() || undefined,
      beneficiaryPhone: this.formData.beneficiaryPhone?.trim() || undefined,
      beneficiaryEmail: this.formData.beneficiaryEmail?.trim() || undefined,
      beneficiaryProvinceId: this.formData.beneficiaryProvinceId?.trim() || undefined,
      beneficiaryWardId: this.formData.beneficiaryWardId?.trim() || undefined,
      beneficiaryAddress: this.formData.beneficiaryAddress?.trim() || undefined,
      beneficiaryFullAddress: this.formData.beneficiaryFullAddress?.trim() || undefined,

      version: {
        effectDate: this.toLocalIsoDateTime(insurancePeriodFrom) || undefined,
        expireDate: this.toLocalIsoDateTime(insurancePeriodTo) || undefined,
        orgEffectDate: this.toLocalIsoDateTime(insurancePeriodFrom) || undefined,
        orgExpireDate: this.toLocalIsoDateTime(insurancePeriodTo) || undefined,
        internalNote: this.formData.internalNotes || undefined,
        customerNote: this.formData.customerNotes || undefined,
        premiumTotal: totals.premiumTotal,
        premium: totals.premium,
        vat: totals.vat,
        discount: 0,
        discountRate: 0,
        markup: 0,
        // Add endorsement-specific fields
        endorsementType: this.endorsementTypeId?.trim() || undefined,
        endorsementReasonId: this.endorsementReasonId?.trim() || undefined,
        endorsementDescription: this.endorsementDescription?.trim() || undefined,
        endorsementTypeCode: this.endorsementTypeOptions.find(o => o.value === this.endorsementTypeId)?.code || undefined,
      },

      products: productsPayload,
      riskObject: riskObjectPayload,
      documents: (this.uploadedDocumentIds || []).map(id => ({ documentId: id })),
      amount: {
        premiumTotal: totals.premiumTotal,
        premium: totals.premium,
        vat: totals.vat,
        discount: 0,
        discountRate: 0,
        markup: 0,
        // Tổng tăng/giảm phí — nguồn chính cho BE (khớp footer; gồm bỏ SP)
        endorsementAdjustmentAmount: this.calculateTotalPremiumChange(),
        // Fallback nếu BE không nhận tổng: từng quyền có premiumChange ≠ 0
        endorsementCoverageChanges: this.getEndorsementCoverageChangesList(),
      },
      submitForApproval,
      // Chỉ định policy_version đang sửa khi update-in-place (tránh PolicyProduct:NotFound)
      versionId: this.getValidVersionIdForEndorsement(),
    };

    this.policyService.endorsement(this.selectedPolicy!.id as string, endorsementDto as UpdatePolicyDetailDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Policy::Success'),
          detail: this.localizationService.localize('Policy::Policy:EndorsementSavedSuccessfully') || 'Endorsement saved successfully'
        });
        this.loading = false;
        this.afterSaveSuccess();
      },
      error: (error) => {
        const errorMessage = this.getFriendlyErrorMessage(error);
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Policy::Error'),
          detail: errorMessage
        });
        this.loading = false;
      }
    });
  }
}
