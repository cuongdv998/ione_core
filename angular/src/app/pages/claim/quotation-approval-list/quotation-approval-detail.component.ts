import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { DialogModule } from 'primeng/dialog';
import { SelectModule } from 'primeng/select';
import { TabsModule } from 'primeng/tabs';
import { TextareaModule } from 'primeng/textarea';
import { ToastModule } from 'primeng/toast';
import { Subject, combineLatest } from 'rxjs';
import { take, takeUntil } from 'rxjs/operators';

import { TranslatePipe } from '@/core/pipes/translate.pipe';
import type { ClaimDetailDto, ClaimDocumentDto, ClaimPolicyDto } from '@/proxy/claim/claims/models';
import { ClaimService } from '@/proxy/claim/controllers/claim.service';
import { ClaimDocumentService } from '@/proxy/claim/controllers/claim-document.service';
import { ClaimStatus } from '@/proxy/claims/claim-status.enum';
import { PolicyService } from '@/proxy/policy/controllers/policy.service';
import type { PolicyClaimLookupDto } from '@/proxy/policy/policies/models';
import { ResReasonService } from '@/proxy/master/controllers/res-reason.service';
import { WorkTaskStatus } from '@/proxy/work-tasks/work-task-status.enum';

import { ClaimTaskInfoTabComponent } from '../claim-task-detail/claim-task-info-tab/claim-task-info-tab.component';
import { ClaimTaskIncidentImagesTabComponent } from '../claim-task-detail/claim-task-incident-images-tab/claim-task-incident-images-tab.component';
import { ClaimOnsiteAssessmentTabComponent } from '../claim-task-detail/claim-onsite-assessment-tab/claim-onsite-assessment-tab.component';
import { DetailedAssessmentEvaluationTabComponent } from '../detailed-assessment-tab/detailed-assessment-evaluation-tab.component';
import { DetailedAssessmentTabComponent } from '../detailed-assessment-tab/detailed-assessment-tab.component';
import { OnsiteAssessmentDetailService } from '../onsite-assessment-detail/onsite-assessment-detail.service';
import { RepairPlanTabComponent } from '../repair-plan-tab/repair-plan-tab.component';
import { RepairPlanTabService } from '../repair-plan-tab/repair-plan-tab.service';

import type { QuotationApprovalUiStatus } from './quotation-approval-list.models';
import { QuotationApprovalListService } from './quotation-approval-list.service';

interface ReasonOption {
  label: string;
  value: string;
}

@Component({
  selector: 'app-quotation-approval-detail',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    DialogModule,
    ButtonModule,
    ToastModule,
    ConfirmDialogModule,
    SelectModule,
    TextareaModule,
    TabsModule,
    TranslatePipe,
    ClaimTaskInfoTabComponent,
    ClaimTaskIncidentImagesTabComponent,
    ClaimOnsiteAssessmentTabComponent,
    DetailedAssessmentTabComponent,
    DetailedAssessmentEvaluationTabComponent,
    RepairPlanTabComponent,
  ],
  providers: [MessageService, ConfirmationService],
  templateUrl: './quotation-approval-detail.component.html',
  styleUrl: './quotation-approval-detail.component.scss',
})
export class QuotationApprovalDetailComponent implements OnInit, OnDestroy {
  readonly WorkTaskStatus = WorkTaskStatus;
  private readonly destroy$ = new Subject<void>();
  private readonly approvalReasonGroupCode = 'APPROVAL_CLAIM_FOLDER_REASON';
  private readonly docPageSize = 50;

  /** Route param */
  claimId: string | null = null;
  /** Approval work task id (route param) */
  approvalWorkTaskId: string | null = null;
  /** Query / context */
  folderNo = '';
  claimCode = '';
  claimFolderId = '';
  detailAssessmentWorkTaskId: string | null = null;
  approvalUiStatus: QuotationApprovalUiStatus = 'new';
  totalPascAmount = 0;

  claim: ClaimDetailDto | null = null;
  loadingClaim = false;
  policies: ClaimPolicyDto[] = [];
  policiesLoading = false;
  documents: ClaimDocumentDto[] = [];
  loadingDocuments = false;

  activeTab = 0;
  onsiteWorkTaskId: string | null = null;
  workTaskStatus: WorkTaskStatus | null = null;
  onsiteRefreshVersion = 0;
  detailedRefreshVersion = 0;

  /** null = đang kiểm tra; true = VCX — hiển thị tab Phương án sửa chữa. */
  repairPlanPascEligible: boolean | null = null;

  rejectDialogVisible = false;
  savingReject = false;
  rejectReasonId: string | null = null;
  rejectReasonOptions: ReasonOption[] = [];
  rejectDescription = '';

  reassignDialogVisible = false;
  savingReassign = false;
  reassignReasonId: string | null = null;
  newAssigneeId: string | null = null;
  reassignDescription = '';
  reassignApproverOptions: Array<{ id: string; name: string }> = [];
  loadingReassignApprovers = false;

  loadingReasonOptions = false;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly quotationApprovalListService: QuotationApprovalListService,
    private readonly resReasonService: ResReasonService,
    private readonly repairPlanTabService: RepairPlanTabService,
    private readonly confirmationService: ConfirmationService,
    private readonly messageService: MessageService,
    private readonly claimService: ClaimService,
    private readonly claimDocumentService: ClaimDocumentService,
    private readonly policyService: PolicyService,
    private readonly onsiteAssessmentDetailService: OnsiteAssessmentDetailService,
  ) {}

  ngOnInit(): void {
    combineLatest([this.route.paramMap, this.route.queryParamMap])
      .pipe(takeUntil(this.destroy$))
      .subscribe(([params, query]) => {
        const cid = params.get('claimId');
        const wt = params.get('workTaskId');
        if (!cid?.trim() || !wt?.trim()) {
          this.claimId = null;
          this.approvalWorkTaskId = null;
          return;
        }

        this.claimId = cid;
        this.approvalWorkTaskId = wt;
        this.folderNo = query.get('folderNo')?.trim() ?? '';
        this.claimCode = query.get('claimCode')?.trim() ?? '';
        this.claimFolderId = query.get('claimFolderId')?.trim() ?? '';
        const dwt = query.get('detailWtId')?.trim();
        this.detailAssessmentWorkTaskId = dwt || null;

        const rawStatus = (query.get('approvalStatus') || 'new').toLowerCase();
        this.approvalUiStatus = ['new', 'approved', 'rejected'].includes(rawStatus)
          ? (rawStatus as QuotationApprovalUiStatus)
          : 'new';

        const pasc = Number(query.get('totalPasc'));
        this.totalPascAmount = Number.isFinite(pasc) ? pasc : 0;

        this.bootstrapPage();
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  get detailWtId(): string | null {
    return this.detailAssessmentWorkTaskId?.trim() || null;
  }

  get showRepairPlanTab(): boolean {
    return this.repairPlanPascEligible === true;
  }

  get repairPlanTabIndex(): number {
    if (this.detailWtId) {
      return this.showRepairPlanTab ? 5 : 4;
    }
    return this.showRepairPlanTab ? 3 : 2;
  }

  get canAct(): boolean {
    return !!this.claimId && !!this.approvalWorkTaskId && this.approvalUiStatus === 'new';
  }

  /** Header fields aligned with onsite-assessment-detail */
  get headerNumberLabel(): string {
    return 'Số HSBT';
  }

  get headerNumberValue(): string {
    return this.folderNo || '-';
  }

  get showHeaderEstimateAmount(): boolean {
    return true;
  }

  get headerEstimateAmountValue(): string {
    return new Intl.NumberFormat('vi-VN').format(this.totalPascAmount ?? 0);
  }

  confirmCloseMain(): void {
    this.confirmationService.confirm({
      message: 'Bạn có muốn đóng màn hình hiện tại không?',
      header: 'Xác nhận',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Có',
      rejectLabel: 'Không',
      accept: () => this.goBackToList(),
    });
  }

  goBackToList(): void {
    void this.router.navigate(['/pages/claim/quotation-approval-list']);
  }

  confirmApprove(): void {
    if (!this.approvalWorkTaskId) {
      return;
    }

    this.confirmationService.confirm({
      message: 'Bạn có muốn duyệt PASC hiện tại không?',
      header: 'Xác nhận phê duyệt',
      icon: 'pi pi-check-circle',
      acceptLabel: 'Có',
      rejectLabel: 'Không',
      accept: () =>
        this.quotationApprovalListService.approve({ workTaskId: this.approvalWorkTaskId! }).subscribe({
          next: () => {
            this.messageService.add({ severity: 'success', summary: 'Thành công', detail: 'Đã phê duyệt PASC.' });
            this.emitDone();
          },
          error: () => {
            this.messageService.add({
              severity: 'error',
              summary: 'Lỗi',
              detail: 'Không phê duyệt được. Vui lòng thử lại.',
            });
          },
        }),
    });
  }

  openReject(): void {
    this.rejectReasonId = null;
    this.rejectDescription = '';
    this.loadReasonOptionsIfNeeded();
    this.rejectDialogVisible = true;
  }

  submitRejectExecuteClick(): void {
    if (!this.approvalWorkTaskId) {
      return;
    }
    if (!this.rejectReasonId) {
      this.messageService.add({ severity: 'warn', summary: 'Cảnh báo', detail: 'Vui lòng chọn lý do từ chối.' });
      return;
    }
    if (!this.rejectDescription.trim()) {
      this.messageService.add({ severity: 'warn', summary: 'Cảnh báo', detail: 'Vui lòng nhập mô tả.' });
      return;
    }

    this.confirmationService.confirm({
      message: 'Bạn có muốn từ chối duyệt PASC hiện tại không?',
      header: 'Xác nhận từ chối',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Có',
      rejectLabel: 'Không',
      accept: () => this.callRejectApi(),
    });
  }

  private callRejectApi(): void {
    if (!this.approvalWorkTaskId || !this.rejectReasonId) {
      return;
    }
    this.savingReject = true;
    this.quotationApprovalListService
      .reject({
        workTaskId: this.approvalWorkTaskId,
        reasonId: this.rejectReasonId,
        comment: this.rejectDescription.trim(),
      })
      .subscribe({
        next: () => {
          this.savingReject = false;
          this.rejectDialogVisible = false;
          this.messageService.add({ severity: 'success', summary: 'Thành công', detail: 'Đã từ chối PASC.' });
          this.emitDone();
        },
        error: () => {
          this.savingReject = false;
          this.messageService.add({ severity: 'error', summary: 'Lỗi', detail: 'Không từ chối được.' });
        },
      });
  }

  confirmCloseRejectModal(): void {
    this.confirmationService.confirm({
      message: 'Bạn có muốn đóng màn hình hiện tại không?',
      header: 'Xác nhận',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Có',
      rejectLabel: 'Không',
      accept: () => {
        this.rejectDialogVisible = false;
        this.rejectReasonId = null;
        this.rejectDescription = '';
      },
    });
  }

  openReassign(): void {
    this.reassignReasonId = null;
    this.reassignDescription = '';
    this.newAssigneeId = null;
    this.loadReasonOptionsIfNeeded();
    this.loadReassignApprovers();
    this.reassignDialogVisible = true;
  }

  confirmCloseReassignModal(): void {
    this.confirmationService.confirm({
      message: 'Bạn có muốn đóng màn hình hiện tại không?',
      header: 'Xác nhận',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Có',
      rejectLabel: 'Không',
      accept: () => {
        this.reassignDialogVisible = false;
        this.reassignReasonId = null;
        this.reassignDescription = '';
        this.newAssigneeId = null;
      },
    });
  }

  submitReassign(): void {
    if (!this.approvalWorkTaskId || !this.newAssigneeId) {
      this.messageService.add({ severity: 'warn', summary: 'Cảnh báo', detail: 'Vui lòng chọn nhân viên nhận điều chuyển.' });
      return;
    }
    if (!this.reassignReasonId) {
      this.messageService.add({ severity: 'warn', summary: 'Cảnh báo', detail: 'Vui lòng chọn lý do điều chuyển.' });
      return;
    }
    if (!this.reassignDescription.trim()) {
      this.messageService.add({ severity: 'warn', summary: 'Cảnh báo', detail: 'Vui lòng nhập mô tả.' });
      return;
    }

    this.savingReassign = true;
    this.quotationApprovalListService
      .reassign({
        workTaskId: this.approvalWorkTaskId,
        newAssigneeId: this.newAssigneeId,
        reasonId: this.reassignReasonId,
        reasonDescription: this.reassignDescription.trim(),
      })
      .subscribe({
        next: () => {
          this.savingReassign = false;
          this.reassignDialogVisible = false;
          this.messageService.add({ severity: 'success', summary: 'Thành công', detail: 'Đã điều chuyển người duyệt PASC.' });
          this.emitDone();
        },
        error: () => {
          this.savingReassign = false;
          this.messageService.add({ severity: 'error', summary: 'Lỗi', detail: 'Không điều chuyển được.' });
        },
      });
  }

  onTabChange(index: string | number | undefined): void {
    if (index == null) {
      return;
    }
    const tabIndex = typeof index === 'number' ? index : Number(index);
    if (Number.isNaN(tabIndex)) {
      return;
    }
    this.activeTab = tabIndex;
    if (tabIndex === 1) {
      this.loadDocuments();
    }
  }

  refreshPolicies(): void {
    this.loadPoliciesFromClaim();
  }

  formatDateTime(value: string | Date | undefined): string {
    if (!value) {
      return '-';
    }
    const d = new Date(value);
    if (isNaN(d.getTime())) {
      return String(value);
    }
    const date = d.toLocaleDateString('vi-VN');
    const h = String(d.getHours()).padStart(2, '0');
    const mi = String(d.getMinutes()).padStart(2, '0');
    return `${h}h${mi}, ${date}`;
  }

  getVehicleDisplay(): string {
    if (!this.claim) {
      return '-';
    }
    const parts = [this.claim.carPlate || '-', this.claim.vin || '-', this.claim.engineNumber || '-'];
    return parts.join(' / ');
  }

  getPriorityDisplay(): string {
    const p = this.claim?.priority;
    if (p == null) {
      return '-';
    }
    if (p >= 3) {
      return 'Cao';
    }
    if (p >= 2) {
      return 'Trung bình';
    }
    return 'Thấp';
  }

  getPriorityClass(priority: number | undefined | null): string {
    if (priority == null) {
      return '';
    }
    if (priority >= 3) {
      return 'text-red-600 font-medium';
    }
    if (priority >= 2) {
      return 'text-orange-600 font-medium';
    }
    return 'text-green-600 font-medium';
  }

  getClaimStatusDisplay(): string {
    const status = this.claim?.status;
    if (status == null) {
      return '-';
    }
    if (typeof status === 'number') {
      switch (status) {
        case ClaimStatus.Draft:
          return 'Nháp';
        case ClaimStatus.PendingReceive:
          return 'Chờ xử lý';
        case ClaimStatus.InProgress:
          return 'Đang xử lý';
        case ClaimStatus.Closed:
          return 'Đã đóng';
        case ClaimStatus.Called:
          return 'Đã hủy';
        default:
          return String(status);
      }
    }
    return String(status);
  }

  getProcessClaimTypeDisplay(): string {
    if (this.claim?.processClaimType === 0) {
      return 'Tự xử lý';
    }
    if (this.claim?.processClaimType === 1) {
      return 'Bảo hiểm gốc xử lý';
    }
    return '-';
  }

  getOpenEmployeeDisplay(): string {
    const c = this.claim as Record<string, unknown> | null;
    if (!c) {
      return '-';
    }
    const openEmployeeName = (c['openEmployeeName'] as string | undefined) || '';
    const openEmployeeId = (c['openEmployeeId'] as string | undefined) || '';
    if (openEmployeeName) {
      return openEmployeeName;
    }
    if (openEmployeeId) {
      return openEmployeeId;
    }
    return this.claim?.processEmpName || '-';
  }

  getOpenEmployeeSecondaryText(): string {
    const c = this.claim as Record<string, unknown> | null;
    if (!c) {
      return '-';
    }
    const phone = (c['openEmployeePhone'] as string | undefined) || '';
    return phone || '-';
  }

  getHandlerSecondaryText(): string {
    const c = this.claim as Record<string, unknown> | null;
    if (!c) {
      return '-';
    }
    const dept = (c['processDeptName'] as string | undefined) || '';
    const phone = (c['processEmpPhone'] as string | undefined) || '';
    const text = [dept, phone].filter(Boolean).join(' - ');
    return text || '-';
  }

  private emitDone(): void {
    this.resetPanels();
    void this.router.navigate(['/pages/claim/quotation-approval-list']);
  }

  private resetPanels(): void {
    this.rejectDialogVisible = false;
    this.reassignDialogVisible = false;
    this.rejectReasonId = null;
    this.rejectDescription = '';
    this.reassignReasonId = null;
    this.reassignDescription = '';
    this.newAssigneeId = null;
    this.reassignApproverOptions = [];
    this.savingReject = false;
    this.savingReassign = false;
  }

  private bootstrapPage(): void {
    this.repairPlanPascEligible = null;
    this.activeTab = 0;
    setTimeout(() => {
      this.activeTab = 0;
    });
    this.claim = null;
    this.policies = [];
    this.documents = [];
    this.onsiteWorkTaskId = null;
    this.workTaskStatus = null;
    this.detailedRefreshVersion++;
    this.onsiteRefreshVersion++;
    this.resetPanels();
    this.loadClaimBundle();
  }

  private loadRepairPlanPascEligibility(): void {
    if (!this.claimId || !this.approvalWorkTaskId) {
      this.repairPlanPascEligible = null;
      return;
    }

    this.repairPlanTabService
      .getPascEligibility(this.claimId, this.approvalWorkTaskId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: r => {
          this.repairPlanPascEligible = r.eligible;
          if (r.eligible) {
            this.activeTab = this.repairPlanTabIndex;
            setTimeout(() => {
              this.activeTab = this.repairPlanTabIndex;
            });
          } else if (this.activeTab > this.repairPlanTabIndex) {
            this.activeTab = this.repairPlanTabIndex;
            setTimeout(() => {
              this.activeTab = this.repairPlanTabIndex;
            });
          }
        },
        error: () => {
          this.repairPlanPascEligible = false;
          if (this.activeTab > this.repairPlanTabIndex) {
            this.activeTab = this.repairPlanTabIndex;
            setTimeout(() => {
              this.activeTab = this.repairPlanTabIndex;
            });
          }
        },
      });
  }

  private loadClaimBundle(): void {
    if (!this.claimId) {
      return;
    }
    this.loadingClaim = true;

    this.claimService
      .get(this.claimId)
      .pipe(take(1))
      .subscribe({
        next: data => {
          this.claim = data;
          if (!this.claimCode && data.code) {
            this.claimCode = data.code;
          }
          this.loadingClaim = false;
          this.loadPoliciesFromClaim();
          this.loadRepairPlanPascEligibility();
        },
        error: () => {
          this.loadingClaim = false;
        },
      });

    this.onsiteAssessmentDetailService
      .getDetailByClaimId(this.claimId)
      .pipe(take(1))
      .subscribe({
        next: detail => {
          this.onsiteWorkTaskId = detail?.workTaskId ?? null;
          this.workTaskStatus = this.toWorkTaskStatus(detail?.workTaskStatus);
        },
        error: () => {
          this.onsiteWorkTaskId = null;
          this.workTaskStatus = null;
        },
      });
  }

  private loadDocuments(): void {
    if (!this.claimId) {
      return;
    }
    this.loadingDocuments = true;
    this.claimDocumentService
      .getList({
        claimId: this.claimId,
        skipCount: 0,
        maxResultCount: this.docPageSize,
      })
      .pipe(take(1))
      .subscribe({
        next: res => {
          this.documents = res.items ?? [];
          this.loadingDocuments = false;
        },
        error: () => {
          this.loadingDocuments = false;
        },
      });
  }

  private loadPoliciesFromClaim(): void {
    if (!this.claim) {
      return;
    }
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
        maxResultCount: 20,
      })
      .pipe(take(1))
      .subscribe({
        next: items => {
          this.policies = (items || []).map(x => this.mapPolicyLookupToClaimPolicy(x));
          this.policiesLoading = false;
        },
        error: () => {
          this.policiesLoading = false;
        },
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
      insurerName: input.insurerName,
    };
  }

  private toWorkTaskStatus(value?: number | string | null): WorkTaskStatus | null {
    if (value == null) {
      return null;
    }
    if (typeof value === 'number') {
      return value in WorkTaskStatus ? (value as WorkTaskStatus) : null;
    }
    const normalized = value.trim();
    if (normalized.length === 0) {
      return null;
    }
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
      return: WorkTaskStatus.Return,
    };
    const k = normalized.toLowerCase();
    return keyLookup[k] ?? null;
  }

  private loadReasonOptionsIfNeeded(): void {
    if (this.rejectReasonOptions.length > 0 || this.loadingReasonOptions) {
      return;
    }
    this.loadingReasonOptions = true;
    this.resReasonService.getSelectListByGroupCode(this.approvalReasonGroupCode).subscribe({
      next: items => {
        this.rejectReasonOptions = (items ?? [])
          .map(x => this.mapResReasonSelectItem(x))
          .filter((x): x is ReasonOption => x !== null)
          .sort((a, b) => a.label.localeCompare(b.label, 'vi', { sensitivity: 'base' }));
        this.loadingReasonOptions = false;
      },
      error: () => {
        this.loadingReasonOptions = false;
      },
    });
  }

  private mapResReasonSelectItem(raw: unknown): ReasonOption | null {
    const x = (raw ?? {}) as Record<string, unknown>;
    const idRaw = x['id'] ?? x['Id'];
    if (idRaw == null || idRaw === '') {
      return null;
    }
    const value = String(idRaw);
    const nameRaw = x['name'] ?? x['Name'] ?? x['description'] ?? x['Description'];
    const name = nameRaw != null ? String(nameRaw).trim() : '';
    const label = name || value;
    return { label, value };
  }

  private loadReassignApprovers(): void {
    if (!this.claimId || !this.approvalWorkTaskId) {
      this.reassignApproverOptions = [];
      return;
    }
    this.loadingReassignApprovers = true;
    this.reassignApproverOptions = [];
    this.repairPlanTabService.getSubmitInfo(this.claimId, this.approvalWorkTaskId).subscribe({
      next: raw => {
        this.loadingReassignApprovers = false;
        const approvers = this.normalizeSubmitInfoApprovers(raw);
        this.reassignApproverOptions = approvers;
        if (!approvers.length) {
          this.messageService.add({
            severity: 'warn',
            summary: 'Cảnh báo',
            detail:
              'Chưa cấu hình người duyệt (CLAIM_QUOTAION_APPROVAL / APPROVAL_LEVEL_ONE). Không thể chọn người điều chuyển.',
          });
        }
      },
      error: () => {
        this.loadingReassignApprovers = false;
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: 'Không tải được danh sách người duyệt.',
        });
      },
    });
  }

  private normalizeSubmitInfoApprovers(raw: unknown): Array<{ id: string; name: string }> {
    const r = raw as Record<string, unknown>;
    const approversRaw = (r['approvers'] ?? r['Approvers'] ?? []) as unknown[];
    return approversRaw.map(a => {
      const row = (a ?? {}) as Record<string, unknown>;
      return {
        id: String(row['id'] ?? row['Id'] ?? ''),
        name: String(row['name'] ?? row['Name'] ?? ''),
      };
    }).filter(x => x.id);
  }
}
