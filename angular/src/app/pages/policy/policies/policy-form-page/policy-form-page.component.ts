import { ChangeDetectorRef, Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PolicyRequestApprovalService } from '@/proxy/policy/controllers/policy-request-approval.service';
import { WorkTaskStatus } from '@/proxy/work-tasks/work-task-status.enum';
import { PolicyStatus } from '@/proxy/policies/policy-status.enum';
import { ResReasonService } from '@/proxy/master/controllers/res-reason.service';
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
import type { PolicyDto, PolicyWorkTaskHistoryItemDto, CalculateRefundAmountBatchInput, DistributeRefundInput } from '@/proxy/policy/policies/models';

// Same services as PoliciesComponent constructor
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
import { ResOrganizationTypeService } from '@/proxy/partner/controllers/res-organization-type.service';
import { ResIndustryService } from '@/proxy/customer/controllers/res-industry.service';
import { ProProductService } from '@/proxy/product/pro-products/pro-product.service';
import { ResDocumentService } from '@/proxy/master/controllers/res-document.service';
import { ResDocumentTypeService } from '@/proxy/master/controllers/res-document-type.service';
import { ResObjectTypeService } from '@/proxy/master/controllers/res-object-type.service';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { PermissionPipe } from '@/core/pipes/permission.pipe';
import { MulticolumnComboboxComponent } from '@/shared/components/multicolumn-combobox/multicolumn-combobox.component';
import { ReportTemplateService } from '@/proxy/report/controllers/report-template.service';
import { Observable, of, tap } from 'rxjs';

@Component({
  selector: 'app-policy-form-page',
  standalone: true,
  // Import everything needed by the form template
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
    PermissionPipe,
    MulticolumnComboboxComponent
  ],
  templateUrl: './policy-form-page.component.html',
  styleUrl: './policy-form-page.component.scss',
  providers: [MessageService, ConfirmationService]
})
export class PolicyFormPageComponent extends PoliciesComponent implements OnInit {
  override restrictCustomerListToCurrentSale = true;

  /** When true, this component is embedded (e.g. in policy-request-approval-detail); use policyIdForEmbed and hide header. */
  @Input() embeddedViewMode = false;
  /** Policy id to load when embeddedViewMode is true (instead of reading from route). */
  @Input() policyIdForEmbed: string | null = null;
  /** Version id để load chi tiết theo version (từ màn phê duyệt); nếu có thì get policy với versionId thay vì version mới nhất. */
  @Input() versionIdForEmbed: string | null = null;
  /** When set in embeddedViewMode, Back navigates to this path. */
  @Input() returnToListPath: string | null = null;
  /** When true, hide the page header (title and back button). Used when embedded. */
  @Input() hideHeader = false;
  /** When set (e.g. from policy-request-approval-detail), footer shows this workflow status instead of policy approval status, with localized label and color. */
  @Input() workTaskStatusForFooter: WorkTaskStatus | number | undefined;
  /** When true, show refund amount column in coverage table and total refund in summary (used in termination approval view). */
  @Input() isTerminationView = false;
  /** Emits the full policy detail object after it has been loaded and applied. Used by parent to get termination reason data. */
  @Output() policyDetailLoaded = new EventEmitter<any>();

  totalRefundAmount = 0;

  /** When true, view was opened from request-approval list; show Approve/Reject/Close and return to request-approval on close. */
  fromApprovalContext = false;
  /** When true, view was opened from terminate-approval list; show Approve/Reject/Close and return to terminate-approval on close. */
  fromTerminateApprovalContext = false;
  approvalActionLoading = false;
  rejectReasonModalVisible = false;
  rejectReasonId: string | null = null;
  rejectReasonDescription: string | null = null;
  rejectReasonOptions: Array<{ label: string; value: string }> = [];
  recordHistoryModalVisible = false;
  workTaskHistoryList: PolicyWorkTaskHistoryItemDto[] = [];
  recordHistoryLoading = false;
  readonly APPROVAL_PERMISSION_CREATE = 'PolicyPolicy.RequestApproval.Approve';
  readonly APPROVAL_PERMISSION_TERMINATE = 'PolicyPolicy.TerminateApproval.Approve';
  readonly BUSINESS_CODE_TERMINATE = 'TERMINATE_POLICY_APPROVAL';
  private static readonly REASON_GROUP_CREATE = 'APPROVAL_POLICY_REASON';
  private static readonly REASON_GROUP_TERMINATE = 'TERMINATE_POLICY_REASON';

  /** Permission required for Approve/Reject in current context. */
  get approvalPermission(): string {
    return this.fromTerminateApprovalContext ? this.APPROVAL_PERMISSION_TERMINATE : this.APPROVAL_PERMISSION_CREATE;
  }

  /** (1) Draft + (empty or rejected): show Attach, History, Save draft, Save & Submit, Back. */
  get detailCaseDraftNotPending(): boolean {
    return this.formData?.status === PolicyStatus.Draft &&
      (this.formData?.approvalStatus ?? '').toString().trim().toLowerCase() !== 'pending';
  }

  /** Show Attach in footer for (1),(2),(3),(5): Draft or Active. Hide for (4),(6): Cancelled, Terminated, Expired. */
  get detailShowAttach(): boolean {
    const s = this.formData?.status;
    return s === PolicyStatus.Draft || s === PolicyStatus.Active;
  }

  /** (4) or (6): only History and Back. */
  get detailShowOnlyHistoryAndBack(): boolean {
    const s = this.formData?.status;
    return s === PolicyStatus.Cancelled || s === PolicyStatus.Terminated || s === PolicyStatus.Expired;
  }

  /** Localized label for work task status in footer (when shown instead of policy approval status). */
  formatWorkTaskStatusForFooter(status: WorkTaskStatus | number | undefined): string {
    if (status === undefined || status === null) return '-';
    const n = typeof status === 'number' ? status : -1;
    const keyByStatus: Record<number, string> = {
      [WorkTaskStatus.New]: 'Policy::WorkTaskStatus:New',
      [WorkTaskStatus.InProgress]: 'Policy::WorkTaskStatus:InProgress',
      [WorkTaskStatus.Completed]: 'Policy::WorkTaskStatus:Completed',
      [WorkTaskStatus.Accepted]: 'Policy::WorkTaskStatus:Accepted',
      [WorkTaskStatus.Rejected]: 'Policy::WorkTaskStatus:Rejected',
      [WorkTaskStatus.Cancelled]: 'Policy::WorkTaskStatus:Cancelled',
      [WorkTaskStatus.WaitApprove]: 'Policy::WorkTaskStatus:WaitApprove',
      [WorkTaskStatus.Approved]: 'Policy::WorkTaskStatus:Approved',
      [WorkTaskStatus.Pending]: 'Policy::WorkTaskStatus:Pending',
      [WorkTaskStatus.Return]: 'Policy::WorkTaskStatus:Return'
    };
    const key = keyByStatus[n];
    return key ? this.localizationService.localize(key) : '-';
  }

  /** CSS classes for work task status badge in footer: green approved, red rejected, amber wait, gray other. */
  getWorkTaskStatusFooterClass(status: WorkTaskStatus | number | undefined): string {
    if (status === undefined || status === null) return 'text-gray-700';
    const n = typeof status === 'number' ? status : -1;
    if (n === WorkTaskStatus.Approved) return 'px-2 py-0.5 rounded text-sm font-semibold bg-green-100 text-green-800';
    if (n === WorkTaskStatus.Rejected) return 'px-2 py-0.5 rounded text-sm font-semibold bg-red-100 text-red-800';
    if (n === WorkTaskStatus.WaitApprove) return 'px-2 py-0.5 rounded text-sm font-semibold bg-amber-100 text-amber-800';
    return 'px-2 py-0.5 rounded text-sm font-semibold bg-gray-100 text-gray-800';
  }

  /** CSS classes for policy approval status value in footer: green approved, red rejected, amber pending, gray other. */
  override getPolicyApprovalStatusFooterClass(approvalStatus: string | null | undefined): string {
    const s = (approvalStatus ?? '').toString().trim().toLowerCase();
    if (s === 'approved') return 'px-2 py-0.5 rounded text-sm font-semibold bg-green-100 text-green-800';
    if (s === 'rejected') return 'px-2 py-0.5 rounded text-sm font-semibold bg-red-100 text-red-800';
    if (s === 'pending') return 'px-2 py-0.5 rounded text-sm font-semibold bg-amber-100 text-amber-800';
    return 'px-2 py-0.5 rounded text-sm font-semibold bg-gray-100 text-gray-800';
  }

  constructor(
    private route: ActivatedRoute,
    private policyRequestApprovalService: PolicyRequestApprovalService,
    private reasonService: ResReasonService,
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

  override ngOnInit(): void {
    super.ngOnInit();

    // PoliciesComponent uses `loading` primarily for the listing table.
    // On this route-based form page we don't trigger table lazy loading,
    // so ensure `loading` starts as false (edit/view will set it to true while fetching detail).
    this.loading = false;

    // Embedded mode: used by policy-request-approval-detail; load policy by id (và versionId nếu có) and show view content only.
    if (this.embeddedViewMode && this.policyIdForEmbed) {
      const versionId = (this.versionIdForEmbed || '').trim() || undefined;
      this.openViewDialog({ id: this.policyIdForEmbed, lastVersionId: versionId } as PolicyDto);
      return;
    }

    const mode = (this.route.snapshot.data?.['mode'] || '').toString();
    const id = (this.route.snapshot.paramMap.get('id') || '').trim();
    const fromApproval = (this.route.snapshot.queryParamMap.get('fromApproval') || '').trim();
    const fromTerminateApproval = (this.route.snapshot.queryParamMap.get('fromTerminateApproval') || '').trim();
    this.fromApprovalContext = (mode === 'view' && fromApproval === '1');
    this.fromTerminateApprovalContext = (mode === 'view' && fromTerminateApproval === '1');
    if (this.fromApprovalContext || this.fromTerminateApprovalContext) {
      this.loadRejectReasonOptions();
    }

    if (mode === 'create') {
      const renewFrom = (this.route.snapshot.queryParamMap.get('renewFrom') || '').trim() || null;
      if (renewFrom) {
        this.openCreateForRenewal(renewFrom);
        return;
      }
      const contractId = (this.route.snapshot.queryParamMap.get('contractId') || '').trim() || null;
      this.openCreateDialog(contractId);
      return;
    }

    if (!id) {
      this.backToList();
      return;
    }

    const versionId = (this.route.snapshot.queryParamMap.get('versionId') || '').trim() || undefined;
    const stub = { id, lastVersionId: versionId } as PolicyDto;
    if (mode === 'edit') {
      this.openEditDialog(stub);
      return;
    }

    // default view (optionally for a specific version when versionId is in query)
    this.openViewDialog(stub);
  }

  /**
   * Re-fetch embedded policy detail (same pipeline as openViewDialog). Used after approve on request-approval detail.
   */
  reloadEmbeddedPolicyDetail(): Observable<unknown> {
    if (!this.embeddedViewMode || !this.policyIdForEmbed) {
      return of(void 0);
    }
    const id = this.policyIdForEmbed.trim();
    if (!id) {
      return of(void 0);
    }
    const versionId = (this.versionIdForEmbed || '').trim() || undefined;
    const stub = { id, lastVersionId: versionId } as PolicyDto;

    this.dialogMode = 'view';
    this.selectedPolicy = stub;
    this.currentPolicyDetail = null;
    this.loadDialogOptions();
    this.uploadedDocuments = [];
    this.uploadedDocumentIds = [];

    this.loading = true;
    return this.policyService.get(id, versionId).pipe(
      tap({
        next: (detail) => {
          this.currentPolicyDetail = detail as any;
          this.populateFormFromDetail(detail as any);
          this.dialogVisible = true;
          this.loading = false;
        },
        error: () => {
          this.loading = false;
        }
      })
    );
  }

  /**
   * Open create form pre-filled from policy detail (renewal): fetch detail by id, then populate renewal period (start from source end or now; end +1y).
   * User saves to call create API (no policy id).
   */
  private openCreateForRenewal(renewFromId: string): void {
    this.dialogMode = 'create';
    this.loadDialogOptions();
    this.loading = true;
    this.policyService.get(renewFromId).subscribe({
      next: (detail) => {
        this.populateFormFromDetailForRenewal(detail as any);
        this.loading = false;
      },
      error: (err) => {
        console.error('Failed to load policy detail for renewal:', err);
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: err?.error?.error?.message || this.localizationService.localize('Policy::Policy:LoadDetailFailed') || 'Failed to load policy details'
        });
        this.loading = false;
      }
    });
  }

  backToList(): void {
    if (this.embeddedViewMode && this.returnToListPath) {
      this.router.navigateByUrl(this.returnToListPath);
      return;
    }
    if (this.fromTerminateApprovalContext) {
      this.router.navigate(['/pages/policy/terminate-approval']);
    } else if (this.fromApprovalContext) {
      this.router.navigate(['/pages/policy/request-approval']);
    } else {
      this.navigateToPolicyList();
    }
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

  private loadRejectReasonOptions(): void {
    const groupCode = this.fromTerminateApprovalContext
      ? PolicyFormPageComponent.REASON_GROUP_TERMINATE
      : PolicyFormPageComponent.REASON_GROUP_CREATE;
    this.reasonService.getSelectListByGroupCode(groupCode).subscribe({
      next: (result: any) => {
        this.rejectReasonOptions = (result || []).map((r: any) => ({ label: r.name || '', value: r.id || '' }));
      },
      error: () => {
        this.rejectReasonOptions = [];
        this.messageService.add({ severity: 'warn', summary: this.localizationService.localize('AbpUi::Warning'), detail: this.localizationService.localize('Policy::PolicyRequestApproval:RejectReasonLoadFailed') });
      }
    });
  }

  approveFromView(): void {
    const id = (this.route.snapshot.paramMap.get('id') || '').trim();
    if (!id) return;
    this.confirmationService.confirm({
      message: this.localizationService.localize('Policy::PolicyRequestApproval:ConfirmApprove'),
      header: this.localizationService.localize('Policy::Policy:Approve'),
      icon: 'pi pi-check',
      accept: () => {
        this.approvalActionLoading = true;
        this.policyRequestApprovalService.approve(id).subscribe({
          next: () => {
            this.approvalActionLoading = false;
            this.messageService.add({ severity: 'success', summary: this.localizationService.localize('AbpUi::Success'), detail: this.localizationService.localize('Policy::PolicyRequestApproval:ApprovedSuccess') });
            this.backToList();
          },
          error: () => {
            this.approvalActionLoading = false;
            this.messageService.add({ severity: 'error', summary: this.localizationService.localize('AbpUi::Error'), detail: this.localizationService.localize('Policy::PolicyRequestApproval:ApproveFailed') });
          }
        });
      }
    });
  }

  openRejectModalFromView(): void {
    this.rejectReasonId = null;
    this.rejectReasonDescription = null;
    this.rejectReasonModalVisible = true;
  }

  cancelRejectFromView(): void {
    this.rejectReasonModalVisible = false;
  }

  openRecordHistoryModal(): void {
    // Version ID of the policy version currently displayed (from loaded detail), not necessarily the policy's "last" version
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

  confirmRejectFromView(): void {
    if (!this.rejectReasonId) {
      this.messageService.add({ severity: 'warn', summary: this.localizationService.localize('AbpUi::Warning'), detail: this.localizationService.localize('Policy::PolicyRequestApproval:SelectReason') });
      return;
    }
    const trimmedDescription = (this.rejectReasonDescription || '').trim();
    if (!trimmedDescription) {
      this.messageService.add({ severity: 'warn', summary: this.localizationService.localize('AbpUi::Warning'), detail: this.localizationService.localize('Policy::PolicyRequestApproval:ReasonDescriptionRequired') });
      return;
    }
    this.rejectReasonModalVisible = false;
    this.confirmationService.confirm({
      message: this.localizationService.localize('Policy::PolicyRequestApproval:ConfirmReject'),
      header: this.localizationService.localize('Policy::Policy:Reject'),
      icon: 'pi pi-times',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => this.doRejectFromView()
    });
  }

  private doRejectFromView(): void {
    const id = (this.route.snapshot.paramMap.get('id') || '').trim();
    if (!id) return;
    this.approvalActionLoading = true;
    const reasonId = this.rejectReasonId || undefined;
    const reasonDescription = (this.rejectReasonDescription || '').trim();
    this.policyRequestApprovalService.reject(id, { reasonId, reasonDescription }).subscribe({
      next: () => {
        this.approvalActionLoading = false;
        this.messageService.add({ severity: 'success', summary: this.localizationService.localize('AbpUi::Success'), detail: this.localizationService.localize('Policy::PolicyRequestApproval:RejectedSuccess') });
        this.backToList();
      },
      error: () => {
        this.approvalActionLoading = false;
        this.messageService.add({ severity: 'error', summary: this.localizationService.localize('AbpUi::Error'), detail: this.localizationService.localize('Policy::PolicyRequestApproval:RejectFailed') });
      }
    });
  }

  override populateFormFromDetail(detail: any): void {
    super.populateFormFromDetail(detail);
    this.policyDetailLoaded.emit(detail);
  }

  protected override onDetailAppliedToProducts(): void {
    if (this.isTerminationView) {
      this.calculateAndPopulateRefundAmounts();
    }
  }

  private calculateAndPopulateRefundAmounts(): void {
    if (!this.currentPolicyDetail) {
      return;
    }

    const selectedProductIds = Object.keys(this.selectedProducts).filter(id => this.selectedProducts[id]);
    if (selectedProductIds.length === 0) {
      return;
    }

    const detail = this.currentPolicyDetail;
    const savedRefundAmount = detail?.versionDetail?.refundAmount;
    if (savedRefundAmount != null && typeof savedRefundAmount === 'number') {
      this.applyDistributeRefund(savedRefundAmount);
      return;
    }

    const riskMotor = detail?.riskObject?.riskObjectMotor || {};
    const attributes: Record<string, any> = {
      carPlate: riskMotor.carPlate || '',
      carAge: riskMotor.carOld || 0,
      carGroup: riskMotor.carGroupCode || '',
      carUsage: riskMotor.carUsage || '',
      carValue: riskMotor.riskObjectValue || 0,
      seat: String(riskMotor.carSeatNumber || ''),
      payload: String(riskMotor.carPayloadCapacity || '')
    };

    const productInputs = selectedProductIds
      .map(productId => {
        const product = this.products.find(p => p.id === productId);
        if (!product?.code) return null;

        const coverages = this.productCoverages[productId] || [];
        const productCoverages = coverages
          .filter((c: any) => !c.isSectionHeader && c.coverageId)
          .map((c: any) => ({
            productCoverageId: c.productCoverageId || '',
            coverageId: c.coverageId || '',
            amountLiability: c.insuranceAmount || 0,
            quantity: c.quantity || 1,
            premium: c.premium || 0,
            premiumVAT: c.vat || 0
          }));

        const totals = this.getProductTotals(productId);
        return {
          productId: product.code,
          productCoverages,
          totalPremium: totals.premium,
          totalPremiumVAT: totals.premiumWithVAT
        };
      })
      .filter((p): p is NonNullable<typeof p> => p !== null);

    if (productInputs.length === 0) {
      return;
    }

    const batchInput: CalculateRefundAmountBatchInput = {
      policyId: detail.id || '',
      terminationDate: this.localIsoDate(new Date()),
      attributes,
      products: productInputs
    };

    this.policyService.calculateRefundAmountBatch(batchInput).subscribe({
      next: (result: any) => {
        this.totalRefundAmount = result.totalRefund || 0;

        const refundByCoverageId = new Map<string, number>(
          (result.coverageRefunds || []).map((r: any) => [r.coverageId || '', r.refundAmount || 0])
        );

        Object.keys(this.productCoverages).forEach(productId => {
          const items = this.productCoverages[productId] || [];
          items.forEach(item => {
            if (item.coverageId && refundByCoverageId.has(item.coverageId)) {
              item.refundAmount = refundByCoverageId.get(item.coverageId) || 0;
            }
          });
        });
      },
      error: () => {
        // Silently fail — leave refund as 0
      }
    });
  }

  /** Use saved total refund and distribute to coverages via API (approval screen when version has refundAmount). */
  private applyDistributeRefund(totalRefund: number): void {
    this.totalRefundAmount = totalRefund;
    const coverages: DistributeRefundInput['coverages'] = [];
    Object.keys(this.productCoverages).forEach(productId => {
      const items = this.productCoverages[productId] || [];
      items.forEach((item: any) => {
        if (!item.isSectionHeader && item.coverageId) {
          coverages.push({
            productCoverageId: item.productCoverageId || undefined,
            coverageId: item.coverageId,
            amountLiability: item.insuranceAmount ?? 0,
            quantity: item.quantity ?? 1,
            premium: item.premium ?? 0,
            premiumVAT: item.vat ?? 0
          });
        }
      });
    });
    if (coverages.length === 0) {
      return;
    }
    const input: DistributeRefundInput = { coverages, totalRefund };
    this.policyService.distributeRefundToCoverages(input).subscribe({
      next: (result: any) => {
        const refundByCoverageId = new Map<string, number>(
          (result.coverageRefunds || []).map((r: any) => [r.coverageId ?? '', r.refundAmount ?? 0])
        );
        Object.keys(this.productCoverages).forEach(productId => {
          const items = this.productCoverages[productId] || [];
          items.forEach((item: any) => {
            if (item.coverageId && refundByCoverageId.has(item.coverageId)) {
              item.refundAmount = refundByCoverageId.get(item.coverageId) ?? 0;
            }
          });
        });
      },
      error: () => {
        // Silently fail
      }
    });
  }

  private localIsoDate(date: Date): string {
    const pad = (n: number) => String(n).padStart(2, '0');
    return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`;
  }

  protected override afterSaveSuccess(): void {
    // After create/update on the route-based form page, go back to listing.
    this.backToList();
  }
}

