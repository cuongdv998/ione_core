import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { TabsModule } from 'primeng/tabs';
import { TableModule } from 'primeng/table';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { MessageService } from 'primeng/api';
import { ConfirmationService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { TooltipModule } from 'primeng/tooltip';

import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';
import type { ClaimDetailDto, ClaimPolicyDto } from '@/proxy/claim/claims/models';
import type { ClaimTaskDto } from '@/proxy/claim/claims/claim-task-models';
import type {
  ClaimFolderListDto,
  ClaimStageProgressDto,
  ClaimSentMessageDto,
  ClaimDocumentDto
} from '@/proxy/claim/claims/models';
import { ClaimService } from '@/proxy/claim/controllers/claim.service';
import { ClaimFolderService } from '@/proxy/claim/controllers/claim-folder.service';
import { ClaimStageProgressService } from '@/proxy/claim/controllers/claim-stage-progress.service';
import { ClaimSentMessageService } from '@/proxy/claim/controllers/claim-sent-message.service';
import { ClaimDocumentService } from '@/proxy/claim/controllers/claim-document.service';
import { PolicyService } from '@/proxy/policy/controllers/policy.service';
import type { PolicyClaimLookupDto } from '@/proxy/policy/policies/models';
import { ProcessClaimType } from '@/proxy/claims/process-claim-type.enum';
import { WorkTaskStatus } from '@/proxy/work-tasks/work-task-status.enum';
import { ClaimTaskService } from '@/proxy/claim/controllers/claim-task.service';
import { ClaimStatus } from '@/proxy/claims/claim-status.enum';
import { AssignOnsiteAssessmentModalComponent } from './assign-onsite-assessment-modal/assign-onsite-assessment-modal.component';
import { OpenClaimFolderModalComponent } from './open-claim-folder-modal/open-claim-folder-modal.component';
import { RejectTaskModalComponent } from './reject-task-modal/reject-task-modal.component';
import { TransferTaskModalComponent } from './transfer-task-modal/transfer-task-modal.component';
import { ClaimTaskInfoTabComponent } from './claim-task-info-tab/claim-task-info-tab.component';
import { ClaimTaskIncidentImagesTabComponent } from './claim-task-incident-images-tab/claim-task-incident-images-tab.component';
import { ClaimTaskProcessingProgressTabComponent } from './claim-task-processing-progress-tab/claim-task-processing-progress-tab.component';
import { CancelClaimModalComponent } from '../claim-detail/cancel-claim-modal/cancel-claim-modal.component';
import { HrEmployeeService } from '@/proxy/hr/controllers/hr-employee.service';

@Component({
  selector: 'app-claim-task-detail',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    PanelModule,
    ButtonModule,
    TabsModule,
    TableModule,
    ConfirmDialogModule,
    ToastModule,
    InputTextModule,
    TextareaModule,
    TooltipModule,
    TranslatePipe,
    AssignOnsiteAssessmentModalComponent,
    OpenClaimFolderModalComponent,
    RejectTaskModalComponent,
    TransferTaskModalComponent,
    ClaimTaskInfoTabComponent,
    ClaimTaskIncidentImagesTabComponent,
    ClaimTaskProcessingProgressTabComponent,
    CancelClaimModalComponent
  ],
  templateUrl: './claim-task-detail.component.html',
  styleUrl: './claim-task-detail.component.scss',
  providers: [MessageService, ConfirmationService]
})
export class ClaimTaskDetailComponent implements OnInit, OnDestroy {
  private readonly destroy$ = new Subject<void>();
  private readonly confirmationService = inject(ConfirmationService);

  claimId: string | null = null;
  workTaskId: string | null = null;
  claim: ClaimDetailDto | null = null;
  task: ClaimTaskDto | null = null;
  workTaskStatus: WorkTaskStatus | null = null;
  loadingClaim = true;
  activeTab = 0;

  documents: ClaimDocumentDto[] = [];
  documentsTotal = 0;
  loadingDocuments = false;
  docPageSize = 50;

  folders: ClaimFolderListDto[] = [];
  foldersTotal = 0;
  loadingFolders = false;
  stageProgress: ClaimStageProgressDto[] = [];
  stageProgressTotal = 0;
  loadingStageProgress = false;
  sentMessages: ClaimSentMessageDto[] = [];
  sentMessagesTotal = 0;
  loadingSentMessages = false;

  policies: ClaimPolicyDto[] = [];
  policiesLoading = false;

  assignModalVisible = false;
  createFolderModalVisible = false;
  rejectModalVisible = false;
  transferModalVisible = false;
  cancelModalVisible = false;
  currentEmployeeId: string | null = null;

  readonly WorkTaskStatus = WorkTaskStatus;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private claimService: ClaimService,
    private claimTaskService: ClaimTaskService,
    private claimFolderService: ClaimFolderService,
    private claimStageProgressService: ClaimStageProgressService,
    private claimSentMessageService: ClaimSentMessageService,
    private claimDocumentService: ClaimDocumentService,
    private policyService: PolicyService,
    private messageService: MessageService,
    private localizationService: LocalizationService,
    private hrEmployeeService: HrEmployeeService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    const workTaskIdParam = this.route.snapshot.paramMap.get('workTaskId');
    if (id) {
      this.claimId = id;
      this.workTaskId = workTaskIdParam;
      this.loadClaim();
      if (this.workTaskId) this.loadTask();
    }
    this.hrEmployeeService.getCurrent().subscribe({
      next: (emp) => { this.currentEmployeeId = emp?.id ?? null; },
      error: (err) => {
        console.warn('[ClaimTaskDetail] getCurrent() failed — using creatorId fallback:', err?.status ?? err);
      }
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadClaim(): void {
    if (!this.claimId) return;
    this.loadingClaim = true;
    this.claimService
      .get(this.claimId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.claim = data;
          this.loadingClaim = false;
          this.loadPoliciesFromClaim();
        },
        error: () => {
          this.loadingClaim = false;
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Claim::LoadClaimError'
          });
        }
      });
  }

  loadTask(): void {
    if (!this.workTaskId) return;
    this.claimTaskService
      .get(this.workTaskId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (task) => {
          this.task = task;
          this.workTaskStatus = task.workTaskStatus ?? null;
        },
        error: () => {
          this.task = null;
          this.workTaskStatus = null;
        }
      });
  }

  /** True when GĐHT is in progress or the latest on-site round is already completed. */
  get assignOnsiteAssessmentDisabled(): boolean {
    return this.isOnsiteAssignBlocked(this.task);
  }

  private isOnsiteAssignBlocked(task: ClaimTaskDto | null): boolean {
    if (!task) return false;
    return !!(task.hasActiveOnsiteAssessmentTask || task.hasFinishedOnsiteAssessment);
  }

  private loadPoliciesFromClaim(): void {
    if (!this.claim) return;
    const certificateNo = this.claim.certificateNo?.trim() || undefined;
    const carPlate = this.claim.carPlate?.trim() || undefined;
    const vin = this.claim.vin?.trim() || undefined;
    const engineNumber = this.claim.engineNumber?.trim() || undefined;
    const incidentDate = this.claim.incidentDate || undefined;
    const hasAny =
      !!certificateNo || !!carPlate || !!vin || !!engineNumber;
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
          this.policies = (items || []).map((x) =>
            this.mapPolicyLookupToClaimPolicy(x)
          );
          this.policiesLoading = false;
        },
        error: () => {
          this.policiesLoading = false;
        }
      });
  }

  private mapPolicyLookupToClaimPolicy(
    input: PolicyClaimLookupDto
  ): ClaimPolicyDto {
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

  onTabChange(index: string | number): void {
    this.activeTab = typeof index === 'number' ? index : Number(index);
    if (index === 1 && this.claimId) this.loadDocuments();
    if (index === 2 && this.claimId) {
      this.loadFolders();
      this.loadStageProgress();
      this.loadSentMessages();
    }
  }

  loadDocuments(): void {
    if (!this.claimId) return;
    this.loadingDocuments = true;
    this.claimDocumentService
      .getList({
        claimId: this.claimId,
        skipCount: 0,
        maxResultCount: this.docPageSize
      })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          this.documents = res.items ?? [];
          this.documentsTotal = res.totalCount ?? 0;
          this.loadingDocuments = false;
        },
        error: () => {
          this.loadingDocuments = false;
        }
      });
  }

  loadFolders(): void {
    if (!this.claimId) return;
    this.loadingFolders = true;
    this.claimFolderService
      .getList({
        claimId: this.claimId,
        skipCount: 0,
        maxResultCount: 100
      })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          this.folders = res.items ?? [];
          this.foldersTotal = res.totalCount ?? 0;
          this.loadingFolders = false;
        },
        error: () => {
          this.loadingFolders = false;
        }
      });
  }

  loadStageProgress(): void {
    if (!this.claimId) return;
    this.loadingStageProgress = true;
    this.claimStageProgressService
      .getList({
        claimId: this.claimId,
        skipCount: 0,
        maxResultCount: 200
      })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          this.stageProgress = res.items ?? [];
          this.stageProgressTotal = res.totalCount ?? 0;
          this.loadingStageProgress = false;
        },
        error: () => {
          this.loadingStageProgress = false;
        }
      });
  }

  loadSentMessages(): void {
    if (!this.claimId) return;
    this.loadingSentMessages = true;
    this.claimSentMessageService
      .getList({
        claimId: this.claimId,
        skipCount: 0,
        maxResultCount: 100
      })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          this.sentMessages = res.items ?? [];
          this.sentMessagesTotal = res.totalCount ?? 0;
          this.loadingSentMessages = false;
        },
        error: () => {
          this.loadingSentMessages = false;
        }
      });
  }

  closeScreen(): void {
    this.confirmationService.confirm({
      message: this.localizationService.localize('Claim::ConfirmCloseScreen'),
      header: this.localizationService.localize('Claim::Close'),
      icon: 'pi pi-question-circle',
      accept: () => {
        this.router.navigate(['/pages/claim/task-list']);
      }
    });
  }

  openAssignModal(): void {
    // Chỉ cho phép phân giám định khi xe đang ở hiện trường.
    if (this.claim?.onLocation !== 'Y') return;
    if (this.isOnsiteAssignBlocked(this.task)) {
      const detailKey = this.task?.hasFinishedOnsiteAssessment
        ? 'Claim::ClaimTask:OnsiteAssessmentLatestRoundFinished'
        : 'Claim::ClaimTask:OnsiteAssessmentAlreadyProcessed';
      this.messageService.add({
        severity: 'warn',
        summary: this.localizationService.localize('Claim::Error'),
        detail: this.localizationService.localize(detailKey)
      });
      return;
    }
    this.assignModalVisible = true;
  }

  onAssignModalVisibleChange(visible: boolean): void {
    this.assignModalVisible = visible;
    if (!visible) this.loadClaim();
  }

  onOnsiteAssessmentAssigned(): void {
    this.loadClaim();
    this.loadTask();
  }

  openCreateFolderModal(): void {
    this.createFolderModalVisible = true;
  }

  onCreateFolderModalVisibleChange(visible: boolean): void {
    this.createFolderModalVisible = visible;
    if (!visible && this.activeTab === 2) this.loadFolders();
  }

  openRejectModal(): void {
    this.rejectModalVisible = true;
  }

  onRejectModalVisibleChange(visible: boolean): void {
    this.rejectModalVisible = visible;
    if (!visible) this.loadTask();
  }

  onTaskRejected(): void {
    this.workTaskStatus = WorkTaskStatus.Rejected;
    this.router.navigate(['/pages/claim/task-list']);
  }

  openTransferModal(): void {
    this.transferModalVisible = true;
  }

  onTransferModalVisibleChange(visible: boolean): void {
    this.transferModalVisible = visible;
    if (!visible) this.loadTask();
  }

  onTaskTransferred(): void {
    this.workTaskStatus = WorkTaskStatus.Return;
    this.router.navigate(['/pages/claim/task-list']);
  }

  acceptTask(): void {
    if (!this.workTaskId) return;
    this.claimTaskService.accept(this.workTaskId).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('AbpUi::Success'),
          detail: this.localizationService.localize('Claim::AcceptSuccess')
        });
        this.loadTask();
      },
      error: () => {}
    });
  }

  formatDate(value: string | undefined): string {
    if (!value) return '-';
    const d = new Date(value);
    return isNaN(d.getTime()) ? String(value) : d.toLocaleDateString('vi-VN');
  }

  formatDateTime(value: string | Date | undefined): string {
    if (!value) return '-';
    const d = new Date(value);
    if (isNaN(d.getTime())) return String(value);

    const day = String(d.getDate()).padStart(2, '0');
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const year = d.getFullYear();
    const hour = String(d.getHours()).padStart(2, '0');
    const minute = String(d.getMinutes()).padStart(2, '0');

    return `${day}/${month}/${year} ${hour}:${minute}`;
  }

  getPriorityLabel(priority: number | undefined | null): string {
    if (priority == null) return '-';
    if (priority >= 3) return this.localizationService.localize('Claim::PriorityHigh') || 'Cao';
    if (priority >= 2) return this.localizationService.localize('Claim::PriorityMedium') || 'Trung bình';
    return this.localizationService.localize('Claim::PriorityLow') || 'Thấp';
  }

  getPriorityClass(priority: number | undefined | null): string {
    if (priority == null) return '';
    if (priority >= 3) return 'text-red-600 font-medium';
    if (priority >= 2) return 'text-orange-600 font-medium';
    return 'text-green-600 font-medium';
  }

  getVehicleDisplay(): string {
    if (!this.claim) return '-';
    const parts = [
      this.claim.carPlate || '-',
      this.claim.vin || '-',
      this.claim.engineNumber || '-'
    ];
    return parts.join(' / ');
  }

  getProcessClaimTypeDisplay(): string {
    if (this.claim?.processClaimType == null) return '-';
    const key =
      this.claim.processClaimType === ProcessClaimType.Own
        ? 'Claim::ProcessClaimTypeOwn'
        : 'Claim::ProcessClaimTypeInsurer';
    return this.localizationService.localize(key) || String(this.claim.processClaimType);
  }

  refreshPolicies(): void {
    this.loadPoliciesFromClaim();
  }

  getOpenEmployeeDisplay(): string {
    // Ưu tiên lấy từ work task (có OpenEmployeeName)
    if (this.task?.openEmployeeName) {
      return this.task.openEmployeeName;
    }

    // Fallback: dùng người xử lý nếu không có thông tin người mở hồ sơ
    if (this.claim?.processEmpName) {
      return this.claim.processEmpName;
    }

    return '-';
  }

  getProcessEmpDisplay(): string {
    if (!this.claim) return '-';
    const parts = [this.claim.processEmpName, this.claim.processDeptName].filter(Boolean);
    const phone = this.claim.processEmpPhone;
    if (phone) parts.push(phone);
    return parts.length ? parts.join(' - ') : '-';
  }

  getAssessmentPartnerDisplay(): string {
    if (!this.claim) return '-';
    const name = (this.claim as { assessmentPartnerName?: string })['assessmentPartnerName']?.trim();
    const id = (this.claim.assessmentPartnerId ?? '').toString().trim();
    // Backend đôi khi trả chuỗi rỗng ('') => UI phải hiển thị '-' thay vì để trống.
    return name || id || '-';
  }

  get canCancelClaim(): boolean {
    if (!this.claim) return false;
    if (!this.currentEmployeeId) return false;

    // processEmpId là người mở tiếp nhận yêu cầu trong detail response
    if (this.claim.processEmpId !== this.currentEmployeeId) return false;

    const status = this.claim.status;
    return status === ClaimStatus.Draft || status === ClaimStatus.PendingReceive;
  }

  openCancelModal(): void {
    this.cancelModalVisible = true;
  }

  onCancelModalClose(visible: boolean): void {
    this.cancelModalVisible = visible;
  }

  onCancelSuccess(): void {
    this.cancelModalVisible = false;
    this.loadClaim();
    this.loadTask();
  }

  getClaimStatusDisplay(status: ClaimStatus | null | undefined): string {
    if (status == null) return '-';
    const statusMap: { [key: number]: string } = {
      [ClaimStatus.Draft]: this.localizationService.localize('Claim::ClaimStatus:Draft'),
      [ClaimStatus.PendingReceive]: this.localizationService.localize('Claim::ClaimStatus:PendingReceive'),
      [ClaimStatus.InProgress]: this.localizationService.localize('Claim::ClaimStatus:InProgress'),
      [ClaimStatus.Closed]: this.localizationService.localize('Claim::ClaimStatus:Closed'),
      [ClaimStatus.Called]: this.localizationService.localize('Claim::ClaimStatus:Cancelled')
    };
    return statusMap[status] || String(status);
  }

}
