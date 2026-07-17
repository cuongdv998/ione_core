import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject, combineLatest } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { ButtonModule } from 'primeng/button';
import { TabsModule } from 'primeng/tabs';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';

import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';
import type { ClaimDetailDto, ClaimDocumentDto, ClaimPolicyDto } from '@/proxy/claim/claims/models';
import { ClaimService } from '@/proxy/claim/controllers/claim.service';
import { ClaimDocumentService } from '@/proxy/claim/controllers/claim-document.service';
import { ClaimStatus } from '@/proxy/claims/claim-status.enum';
import { WorkTaskStatus } from '@/proxy/work-tasks/work-task-status.enum';
import { PolicyService } from '@/proxy/policy/controllers/policy.service';
import type { PolicyClaimLookupDto } from '@/proxy/policy/policies/models';
import { OnsiteAssessmentDetailService } from './onsite-assessment-detail.service';
import { ClaimTaskInfoTabComponent } from '../claim-task-detail/claim-task-info-tab/claim-task-info-tab.component';
import { ClaimTaskIncidentImagesTabComponent } from '../claim-task-detail/claim-task-incident-images-tab/claim-task-incident-images-tab.component';
import { ClaimOnsiteAssessmentTabComponent } from '../claim-task-detail/claim-onsite-assessment-tab/claim-onsite-assessment-tab.component';
import { DetailedAssessmentTabComponent } from '../detailed-assessment-tab/detailed-assessment-tab.component';
import { DetailedAssessmentEvaluationTabComponent } from '../detailed-assessment-tab/detailed-assessment-evaluation-tab.component';
import { DetailedAssessmentTabService } from '../detailed-assessment-tab/detailed-assessment-tab.service';
import { RejectTaskModalComponent } from '../claim-task-detail/reject-task-modal/reject-task-modal.component';
import { TransferTaskModalComponent } from '../claim-task-detail/transfer-task-modal/transfer-task-modal.component';
import { RepairPlanTabComponent } from '../repair-plan-tab/repair-plan-tab.component';
import { RepairPlanTabService } from '../repair-plan-tab/repair-plan-tab.service';
import { ReassignOnsiteAssessmentModalComponent } from '../onsite-assessment-list/reassign-onsite-assessment-modal.component';
import { ReassignDetailedAssessmentModalComponent } from '../detail-assessment-list/reassign-detailed-assessment-modal.component';

@Component({
  selector: 'app-onsite-assessment-detail',
  standalone: true,
  imports: [
    CommonModule,
    ButtonModule,
    TabsModule,
    ConfirmDialogModule,
    ToastModule,
    TranslatePipe,
    ClaimTaskInfoTabComponent,
    ClaimTaskIncidentImagesTabComponent,
    ClaimOnsiteAssessmentTabComponent,
    DetailedAssessmentTabComponent,
    DetailedAssessmentEvaluationTabComponent,
    RejectTaskModalComponent,
    TransferTaskModalComponent,
    RepairPlanTabComponent,
    ReassignOnsiteAssessmentModalComponent,
    ReassignDetailedAssessmentModalComponent,
  ],
  templateUrl: './onsite-assessment-detail.component.html',
  styleUrl: './onsite-assessment-detail.component.scss',
  providers: [MessageService, ConfirmationService],
})
export class OnsiteAssessmentDetailComponent implements OnInit, OnDestroy {
  readonly WorkTaskStatus = WorkTaskStatus;
  private readonly destroy$ = new Subject<void>();
  sourcePage: string | null = null;
  detailFolderNo: string | null = null;
  detailEstimateAmount: number | null = null;
  detailMode: 'view' | 'action' = 'view';
  taskActionMode: 'onsite' | 'detailed' = 'onsite';

  claimId: string | null = null;
  workTaskId: string | null = null;
  claim: ClaimDetailDto | null = null;
  loading = true;
  activeTab = 2;
  onsiteWorkTaskStatus: WorkTaskStatus | null = null;
  detailedWorkTaskStatus: WorkTaskStatus | null = null;
  autoTriggerComplete = false;
  rejectModalVisible = false;
  transferModalVisible = false;
  reassignModalVisible = false;
  detailedReassignModalVisible = false;
  policies: ClaimPolicyDto[] = [];
  policiesLoading = false;
  documents: ClaimDocumentDto[] = [];
  loadingDocuments = false;
  detailedRefreshVersion = 0;
  onsiteRefreshVersion = 0;

  /** null = chưa kiểm tra; true = hồ sơ VCX — hiển thị tab Phương án sửa chữa. */
  repairPlanPascEligible: boolean | null = null;
  repairPlanQuotationApprovalStatus: string | null = null;
  allowCompletedDetailedEdit = false;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly claimService: ClaimService,
    private readonly claimDocumentService: ClaimDocumentService,
    private readonly policyService: PolicyService,
    private readonly onsiteAssessmentDetailService: OnsiteAssessmentDetailService,
    private readonly detailedAssessmentTabService: DetailedAssessmentTabService,
    private readonly localizationService: LocalizationService,
    private readonly messageService: MessageService,
    private readonly repairPlanTabService: RepairPlanTabService,
  ) {}

  private applyActiveTab(tab: number): void {
    this.activeTab = tab;
    setTimeout(() => {
      this.activeTab = tab;
    });
  }

  ngOnInit(): void {
    combineLatest([
      this.route.paramMap,
      this.route.queryParamMap,
      this.route.url,
    ])
      .pipe(takeUntil(this.destroy$))
      .subscribe(([paramMap, queryParamMap, urlSegments]) => {
        this.repairPlanPascEligible = null;
        this.repairPlanQuotationApprovalStatus = null;
        this.allowCompletedDetailedEdit = false;
        const nextClaimId = paramMap.get('id');
        const nextWorkTaskId = paramMap.get('workTaskId');
        const routeMode = (urlSegments.at(-1)?.path || '').toLowerCase();

        this.claimId = nextClaimId;
        this.workTaskId = nextWorkTaskId;
        this.autoTriggerComplete = (queryParamMap.get('action') || '').toLowerCase() === 'complete';
        this.sourcePage = queryParamMap.get('source');
        this.detailFolderNo = queryParamMap.get('folderNo');
        this.detailEstimateAmount = this.parseEstimateAmount(queryParamMap.get('estimateAmount'));
        this.allowCompletedDetailedEdit =
          (queryParamMap.get('allowCompletedDetailedEdit') || '').toLowerCase() === 'true';

        const requestedDetailMode = (queryParamMap.get('detailMode') || '').toLowerCase();
        this.detailMode = requestedDetailMode === 'action' || routeMode === 'process' ? 'action' : 'view';

        const requestedTab = Number(queryParamMap.get('tab'));
        if (!Number.isNaN(requestedTab) && requestedTab >= 0) {
          this.applyActiveTab(requestedTab);
        } else if (routeMode === 'view') {
          this.applyActiveTab(0);
        } else if (routeMode === 'process') {
          this.applyActiveTab(2);
        } else {
          this.applyActiveTab(2);
        }

        if (!this.showDetailedTab && (this.activeTab === 3 || this.activeTab === 4)) {
          this.applyActiveTab(2);
        }

        if (!this.claimId) {
          this.claim = null;
          this.loading = false;
          return;
        }

        this.loading = true;
        this.claimService
          .get(this.claimId)
          .pipe(takeUntil(this.destroy$))
          .subscribe({
            next: (data) => {
              this.claim = data;
              this.normalizeActiveTabForClaim();
              this.loadPoliciesFromClaim();
              this.loading = false;
              this.refreshRepairPlanPascEligibility();
            },
            error: () => {
              this.loading = false;
            },
          });

        if (this.workTaskId) {
          this.reloadTaskStatus();
        } else {
          this.onsiteWorkTaskStatus = null;
          this.detailedWorkTaskStatus = null;
        }
      });
  }

  get showDetailedActions(): boolean {
    if (this.sourcePage !== 'detail-assessment-list') {
      return true;
    }

    return this.detailMode === 'action';
  }

  get showOnsiteActions(): boolean {
    return this.sourcePage !== 'detail-assessment-list' && this.detailMode === 'action';
  }

  get showOnsiteAssessmentTab(): boolean {
    return this.claim?.onLocation === 'Y';
  }

  get showDetailedTab(): boolean {
    return this.sourcePage === 'detail-assessment-list';
  }

  get headerNumberLabel(): string {
    return this.sourcePage === 'detail-assessment-list'
      ? 'Số HSBT'
      : this.localizationService.localize('Claim::ClaimCode');
  }

  get headerNumberValue(): string {
    return this.sourcePage === 'detail-assessment-list'
      ? this.detailFolderNo || '-'
      : this.claim?.code || '-';
  }

  get showHeaderEstimateAmount(): boolean {
    return this.sourcePage === 'detail-assessment-list' && this.detailEstimateAmount !== null;
  }

  get headerEstimateAmountValue(): string {
    return new Intl.NumberFormat('vi-VN').format(this.detailEstimateAmount ?? 0);
  }

  get forceDetailedReadOnly(): boolean {
    return !this.showDetailedActions;
  }

  onDetailedEvaluationSaved(): void {
    this.applyActiveTab(4);
    this.detailedRefreshVersion++;
  }

  onDetailedAssessmentCompleted(): void {
    this.reloadTaskStatus();
    this.refreshRepairPlanPascEligibility();
    this.detailedRefreshVersion++;
    this.applyActiveTab(4);
  }

  onRepairPlanQuotationApprovalStatusChanged(status: string | null): void {
    this.repairPlanQuotationApprovalStatus = this.normalizeQuotationApprovalStatus(status);
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  closeScreen(): void {
    if (this.sourcePage === 'detail-assessment-list') {
      this.router.navigate(['/pages/claim/detail-assessment-list']);
      return;
    }

    this.router.navigate(['/pages/claim/onsite-assessment-list']);
  }

  private refreshRepairPlanPascEligibility(): void {
    if (!this.claimId || !this.workTaskId || !this.showDetailedTab) {
      this.repairPlanPascEligible = null;
      this.repairPlanQuotationApprovalStatus = null;
      return;
    }

    this.repairPlanTabService
      .getInitData(this.claimId, this.workTaskId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: r => {
          this.repairPlanPascEligible = r.isPascProductEligible === true;
          this.repairPlanQuotationApprovalStatus = this.normalizeQuotationApprovalStatus(
            r.quotationApprovalStatus
          );
          if (!this.repairPlanPascEligible && this.activeTab === 5) {
            this.applyActiveTab(4);
          }
        },
        error: () => {
          this.repairPlanPascEligible = false;
          this.repairPlanQuotationApprovalStatus = null;
          if (this.activeTab === 5) {
            this.applyActiveTab(4);
          }
        },
      });
  }

  onTabChange(index: string | number | undefined): void {
    if (index == null) return;
    const tabIndex = typeof index === 'number' ? index : Number(index);
    if (Number.isNaN(tabIndex)) return;
    if (tabIndex === 2 && !this.showOnsiteAssessmentTab) {
      this.normalizeActiveTabForClaim();
      return;
    }
    this.activeTab = tabIndex;
    if (tabIndex === 1) {
      this.loadDocuments();
    }
    if (tabIndex === 3 && this.showDetailedTab) {
      this.refreshRepairPlanPascEligibility();
    }
  }

  private normalizeQuotationApprovalStatus(status: string | null | undefined): string | null {
    const normalized = (status ?? '').trim().toLowerCase();
    return normalized || null;
  }

  private normalizeActiveTabForClaim(): void {
    if (this.activeTab !== 2 || this.showOnsiteAssessmentTab) {
      return;
    }

    this.applyActiveTab(this.showDetailedTab ? 3 : 0);
  }

  private parseEstimateAmount(rawValue: string | null): number | null {
    if (!rawValue) {
      return null;
    }

    const parsed = Number(rawValue);
    return Number.isFinite(parsed) ? parsed : null;
  }

  openRejectModal(): void {
    this.taskActionMode = 'onsite';
    this.rejectModalVisible = true;
  }

  openDetailedRejectModal(): void {
    this.taskActionMode = 'detailed';
    this.rejectModalVisible = true;
  }

  onRejectModalVisibleChange(visible: boolean): void {
    this.rejectModalVisible = visible;
    if (!visible) this.reloadTaskStatus();
  }

  onTaskRejected(): void {
    if (this.taskActionMode === 'detailed') {
      this.reloadTaskStatus();
      this.detailedRefreshVersion++;
      return;
    }

    this.closeScreen();
  }

  openTransferModal(): void {
    this.taskActionMode = 'onsite';
    this.transferModalVisible = true;
  }

  openDetailedTransferModal(): void {
    this.taskActionMode = 'detailed';
    this.transferModalVisible = true;
  }

  onTransferModalVisibleChange(visible: boolean): void {
    this.transferModalVisible = visible;
    if (!visible) this.reloadTaskStatus();
  }

  onTaskTransferred(): void {
    this.closeScreen();
  }

  openReassignModal(): void {
    this.reassignModalVisible = true;
  }

  openDetailedReassignModal(): void {
    this.detailedReassignModalVisible = true;
  }

  onReassignModalVisibleChange(visible: boolean): void {
    this.reassignModalVisible = visible;
    if (!visible) {
      this.reloadTaskStatus();
    }
  }

  onTaskReassigned(): void {
    this.closeScreen();
  }

  onDetailedReassignModalVisibleChange(visible: boolean): void {
    this.detailedReassignModalVisible = visible;
    if (!visible) {
      this.reloadTaskStatus();
    }
  }

  onDetailedTaskReassigned(): void {
    this.closeScreen();
  }

  acceptTask(): void {
    if (!this.workTaskId) return;
    this.onsiteAssessmentDetailService.accept(this.workTaskId).subscribe({
      next: () => {
        this.onsiteWorkTaskStatus = WorkTaskStatus.InProgress;
        this.refreshOnsiteDetailData();
        this.messageService.add({
          severity: 'success',
          summary: 'Thành công',
          detail: 'Đã tiếp nhận yêu cầu giám định hiện trường.',
        });
      },
    });
  }

  acceptDetailedTask(): void {
    if (!this.workTaskId) return;
    this.taskActionMode = 'detailed';
    this.detailedAssessmentTabService.accept(this.workTaskId).subscribe({
      next: () => {
        this.detailedWorkTaskStatus = WorkTaskStatus.InProgress;
        this.reloadTaskStatus();
        this.detailedRefreshVersion++;
        this.messageService.add({
          severity: 'success',
          summary: 'Thành công',
          detail: 'Đã tiếp nhận yêu cầu giám định chi tiết.',
        });
      },
    });
  }

  getProcessClaimTypeDisplay(): string {
    if (this.claim?.processClaimType === 0) return 'Tự xử lý';
    if (this.claim?.processClaimType === 1) return 'Bảo hiểm gốc xử lý';
    return '-';
  }

  getPriorityDisplay(): string {
    const p = this.claim?.priority;
    if (p == null) return '-';
    if (p >= 3) return 'Cao';
    if (p >= 2) return 'Trung bình';
    return 'Thấp';
  }

  formatDateTime(value: string | Date | undefined): string {
    if (!value) return '-';
    const d = new Date(value);
    if (isNaN(d.getTime())) return String(value);
    const date = d.toLocaleDateString('vi-VN');
    const h = String(d.getHours()).padStart(2, '0');
    const mi = String(d.getMinutes()).padStart(2, '0');
    return `${h}h${mi}, ${date}`;
  }

  getPriorityClass(priority: number | undefined | null): string {
    if (priority == null) return '';
    if (priority >= 3) return 'text-red-600 font-medium';
    if (priority >= 2) return 'text-orange-600 font-medium';
    return 'text-green-600 font-medium';
  }

  getVehicleDisplay(): string {
    if (!this.claim) return '-';
    const parts = [this.claim.carPlate || '-', this.claim.vin || '-', this.claim.engineNumber || '-'];
    return parts.join(' / ');
  }

  getOpenEmployeeDisplay(): string {
    const c = this.claim as Record<string, unknown> | null;
    if (!c) return '-';
    const openEmployeeName = (c['openEmployeeName'] as string | undefined) || '';
    const openEmployeeId = (c['openEmployeeId'] as string | undefined) || '';
    if (openEmployeeName) return openEmployeeName;
    if (openEmployeeId) return openEmployeeId;
    return this.claim?.processEmpName || '-';
  }

  getOpenEmployeeSecondaryText(): string {
    const c = this.claim as Record<string, unknown> | null;
    if (!c) return '-';
    const phone = (c['openEmployeePhone'] as string | undefined) || '';
    return phone || '-';
  }

  getHandlerSecondaryText(): string {
    const c = this.claim as Record<string, unknown> | null;
    if (!c) return '-';
    const dept = (c['processDeptName'] as string | undefined) || '';
    const phone = (c['processEmpPhone'] as string | undefined) || '';
    const text = [dept, phone].filter(Boolean).join(' - ');
    return text || '-';
  }

  getClaimStatusDisplay(): string {
    const status = this.claim?.status;
    if (status == null) return '-';

    if (typeof status === 'number') {
      switch (status) {
        case ClaimStatus.Draft: return 'Nháp';
        case ClaimStatus.PendingReceive: return 'Chờ xử lý';
        case ClaimStatus.InProgress: return 'Đang xử lý';
        case ClaimStatus.Closed: return 'Đã đóng';
        case ClaimStatus.Called: return 'Đã hủy';
        default: return String(status);
      }
    }

    return String(status);
  }

  refreshPolicies(): void {
    this.loadPoliciesFromClaim();
  }

  private refreshOnsiteDetailData(): void {
    this.onsiteRefreshVersion++;
    this.reloadTaskStatus();

    if (!this.claimId) {
      return;
    }

    this.claimService
      .get(this.claimId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.claim = data;
          this.normalizeActiveTabForClaim();
          this.loadPoliciesFromClaim();
        },
      });
  }

  private loadDocuments(): void {
    if (!this.claimId) return;
    this.loadingDocuments = true;

    this.claimDocumentService
      .getOnsiteImages({
        claimId: this.claimId,
        workTaskId: this.workTaskId || undefined,
      })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (documents: ClaimDocumentDto[]) => {
          this.documents = documents || [];
          this.loadingDocuments = false;
        },
        error: () => {
          this.documents = [];
          this.loadingDocuments = false;
        }
      });
  }

  private loadPoliciesFromClaim(): void {
    if (!this.claim) return;
    const certificateNo = this.claim.certificateNo?.trim() || undefined;
    const carPlate = this.claim.carPlate?.trim() || undefined;
    const vin = this.claim.vin?.trim() || undefined;
    const engineNumber = this.claim.engineNumber?.trim() || undefined;
    const incidentDate = this.claim.incidentDate || undefined;
    const hasAny = !!certificateNo || !!carPlate || !!vin || !!engineNumber;
    if (!hasAny) {
      this.policies = [];
      return;
    }
    this.policiesLoading = true;
    this.policyService
      .getClaimLookup({
        certificateNo,
        carPlate,
        vin,
        engineNumber,
        incidentDate,
        maxResultCount: 20
      })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (items) => {
          this.policies = (items || []).map((x) => this.mapPolicyLookupToClaimPolicy(x));
          this.policiesLoading = false;
        },
        error: () => {
          this.policiesLoading = false;
        }
      });
  }

  private mapPolicyLookupToClaimPolicy(input: PolicyClaimLookupDto): ClaimPolicyDto {
    return {
      policyId: input.policyId,
      lobName: input.lobName,
      contractId: input.contractId,
      certificateNo: input.certificateNo,
      products: input.products || [],
      productId: input.productId,
      carPlate: input.carPlate,
      ownerName: input.ownerName,
      effectDate: input.effectDate,
      expireDate: input.expireDate,
      status: input.status,
      paymentStatus: input.paymentStatus,
      certificateUrl: input.certificateUrl,
      insurerId: input.insurerId,
      insurerName: input.insurerName
    };
  }

  private formatDateOnly(value: string | Date | undefined): string {
    if (!value) return '';
    const d = new Date(value);
    if (isNaN(d.getTime())) return '';
    const y = d.getFullYear();
    const m = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${y}-${m}-${day}`;
  }

  private reloadTaskStatus(): void {
    if (!this.workTaskId) return;

    if (this.sourcePage === 'detail-assessment-list') {
      // workTaskId is the detailed-assessment task ID → load detailed status from detailed API
      this.detailedAssessmentTabService
        .getDetail(this.workTaskId)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: detail => {
            this.detailedWorkTaskStatus = this.toWorkTaskStatus(detail?.workTaskStatus);
          },
          error: () => {
            this.detailedWorkTaskStatus = null;
          },
        });

      // Load onsite status separately by claim ID
      if (this.claimId) {
        this.onsiteAssessmentDetailService
          .getDetailByClaimId(this.claimId)
          .pipe(takeUntil(this.destroy$))
          .subscribe({
            next: detail => {
              this.onsiteWorkTaskStatus = this.toWorkTaskStatus(detail?.workTaskStatus);
            },
            error: () => {
              this.onsiteWorkTaskStatus = null;
            },
          });
      }
    } else {
      // workTaskId is the onsite-assessment task ID → load onsite status
      this.onsiteAssessmentDetailService
        .getDetail(this.workTaskId)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: detail => {
            this.onsiteWorkTaskStatus = this.toWorkTaskStatus(detail?.workTaskStatus);
          },
          error: () => {
            this.onsiteWorkTaskStatus = null;
          },
        });
      this.detailedWorkTaskStatus = null;
    }
  }

  private toWorkTaskStatus(value?: number | string | null): WorkTaskStatus | null {
    if (value == null) return null;

    if (typeof value === 'number') {
      return value in WorkTaskStatus ? (value as WorkTaskStatus) : null;
    }

    const normalized = value.trim();
    if (normalized.length === 0) return null;

    const parsedNumber = Number(normalized);
    if (!Number.isNaN(parsedNumber)) {
      return parsedNumber in WorkTaskStatus ? (parsedNumber as WorkTaskStatus) : null;
    }

    const keyLookup: Record<string, WorkTaskStatus> = {
      new: WorkTaskStatus.New,
      inprogress: WorkTaskStatus.InProgress,
      completed: WorkTaskStatus.Completed,
      accepted: WorkTaskStatus.Accepted,
      rejected: WorkTaskStatus.Rejected,
      cancelled: WorkTaskStatus.Cancelled,
      waitapprove: WorkTaskStatus.WaitApprove,
      approved: WorkTaskStatus.Approved,
      pending: WorkTaskStatus.Pending,
      return: WorkTaskStatus.Return
    };

    return keyLookup[normalized.toLowerCase()] ?? null;
  }
}
