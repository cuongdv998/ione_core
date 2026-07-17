import { Component, DestroyRef, OnInit, ViewChild } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { SelectModule } from 'primeng/select';
import { TextareaModule } from 'primeng/textarea';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { MessageService, ConfirmationService } from 'primeng/api';

import { PolicyFormPageComponent } from '@/pages/policy/policies/policy-form-page/policy-form-page.component';
import { PolicyRequestApprovalService } from '@/proxy/policy/controllers/policy-request-approval.service';
import { WorkTaskStatus } from '@/proxy/work-tasks/work-task-status.enum';
import { ResReasonService } from '@/proxy/master/controllers/res-reason.service';
import { AdminConfigService } from '@/proxy/master/controllers/admin-config.service';
import { AdminConfigStatus } from '@/proxy/admin-configs/admin-config-status.enum';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { PermissionService } from '@/core/services/permission.service';
import { LocalizationService } from '@/core/services/localization.service';
import { PolicyRequestApprovalItemDto } from '@/proxy/policy/policy-request-approval/models';
import { forkJoin, of, switchMap, timer } from 'rxjs';

const BUSINESS_CODE_CREATE = 'CREATE_POLICY_APPROVAL';
const BUSINESS_CODE_TERMINATE = 'TERMINATE_POLICY_APPROVAL';
const BUSINESS_CODE_ENDORSEMENT = 'ENDORSEMENT_POLICY_APPROVAL';
const REASON_GROUP_CREATE = 'APPROVAL_POLICY_REASON';
const REASON_GROUP_TERMINATE = 'APPROVAL_POLICY_REASON';
const REASON_GROUP_ENDORSEMENT = 'APPROVAL_POLICY_REASON';

function getReasonGroupCodeForBusinessCode(businessCode: string | null): string | null {
  if (businessCode === BUSINESS_CODE_CREATE) return REASON_GROUP_CREATE;
  if (businessCode === BUSINESS_CODE_TERMINATE) return REASON_GROUP_TERMINATE;
  if (businessCode === BUSINESS_CODE_ENDORSEMENT) return REASON_GROUP_ENDORSEMENT;
  return null;
}

@Component({
  selector: 'app-policy-request-approval-detail',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    PanelModule,
    ButtonModule,
    DialogModule,
    SelectModule,
    TextareaModule,
    ToastModule,
    ConfirmDialogModule,
    PolicyFormPageComponent,
    TranslatePipe
  ],
  templateUrl: './policy-request-approval-detail.component.html',
  styleUrl: './policy-request-approval-detail.component.scss',
  providers: [MessageService, ConfirmationService]
})
export class PolicyRequestApprovalDetailComponent implements OnInit {
  @ViewChild(PolicyFormPageComponent) policyFormPage?: PolicyFormPageComponent;

  readonly PERMISSIONS = {
    APPROVE: 'PolicyPolicy.RequestApproval.Approve',
    TERMINATE_APPROVE: 'PolicyPolicy.TerminateApproval.Approve',
    ENDORSEMENT_APPROVE: 'PolicyPolicy.EndorsementApproval.Approve'
  };

  /** Route param: work task id */
  id: string = '';
  /** Policy id to load in the embedded form (set after getByWorkTaskId). */
  policyIdForEmbed: string | null = null;
  /** Version id để load chi tiết theo version (từ query param; nếu có thì form gọi API get với versionId). */
  versionIdForEmbed: string | null = null;
  policyNo: string = '';
  businessCode: string | null = null;
  /** Work task status from API; used to disable Approve/Reject when already approved or rejected. */
  workTaskStatus: WorkTaskStatus | undefined;
  isTerminateRequest = false;
  isEndorsementRequest = false;
  loading = true;
  actionLoading = false;
  terminationReasonName: string = '-';
  terminationReasonDescription: string = '-';
  endorsementReasonName: string = '-';
  endorsementTypeName: string = '-';
  endorsementDescription: string = '-';

  rejectReasonModalVisible = false;
  rejectReasonId: string | null = null;
  rejectReasonDescription: string | null = null;
  rejectReasonOptions: Array<{ label: string; value: string }> = [];
  rejectReasonOptionsLoading = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private policyRequestApprovalService: PolicyRequestApprovalService,
    private reasonService: ResReasonService,
    private adminConfigService: AdminConfigService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private permissionService: PermissionService,
    private localizationService: LocalizationService,
    private destroyRef: DestroyRef
  ) {}

  ngOnInit(): void {
    this.id = (this.route.snapshot.paramMap.get('id') || '').trim();
    this.versionIdForEmbed = (this.route.snapshot.queryParamMap.get('versionId') || '').trim() || null;

    if (!this.id) {
      this.backToList();
      return;
    }

    this.policyRequestApprovalService.getByWorkTaskId(this.id).subscribe({
      next: (item) => {
        this.applyWorkTaskItem(item, { finalizeInitialLoad: true });
      },
      error: () => {
        this.loading = false;
        this.terminationReasonName = '-';
        this.terminationReasonDescription = '-';
        this.backToList();
      }
    });
  }

  backToList(): void {
    this.router.navigate(['/pages/policy/request-approval']);
  }

  /**
   * Maps work-task API item into component state. When finalizeInitialLoad is true (initial route load), sets loading = false at the end.
   */
  private applyWorkTaskItem(item: PolicyRequestApprovalItemDto, options?: { finalizeInitialLoad?: boolean }): void {
    const finalizeInitialLoad = options?.finalizeInitialLoad === true;

    this.businessCode = item.businessCode ?? null;
    this.isTerminateRequest = this.businessCode === BUSINESS_CODE_TERMINATE;
    this.isEndorsementRequest = this.businessCode === BUSINESS_CODE_ENDORSEMENT;
    this.policyIdForEmbed = item.policy?.id ?? null;
    this.policyNo = item.policy?.policyNo ?? '';
    this.workTaskStatus = item.workTaskStatus;

    if (this.isTerminateRequest && item.policy) {
      this.terminationReasonDescription = (item.policy.terminationReasonDescription || '').trim() || '-';
      const reasonId = item.policy.terminationReasonId;
      if (reasonId) {
        this.reasonService.getSelectListByGroupCode(REASON_GROUP_TERMINATE).subscribe({
          next: (result: any[]) => {
            const selectOptions = (result || []).map((r: any) => ({ label: r.name || '', value: r.id || '' }));
            const found = selectOptions.find(o => o.value === reasonId);
            this.terminationReasonName = found?.label || '-';
          },
          error: () => {
            this.terminationReasonName = '-';
          }
        });
      } else {
        this.terminationReasonName = '-';
      }
    }

    if (this.isEndorsementRequest && item.policy?.versionDetail) {
      this.endorsementDescription = (item.policy.versionDetail.endorsementDescription || '').trim() || '-';
      this.loadEndorsementReasonName(item.policy.versionDetail.endorsementReasonId);
      this.loadEndorsementTypeName(item.policy.versionDetail.endorsementType);
    }

    if (finalizeInitialLoad) {
      this.loading = false;
    }
  }

  onPolicyDetailLoaded(detail: any): void {
    if (!this.isTerminateRequest) return;

    const reasonId = detail?.terminationReasonId;
    const description = (detail?.terminationReasonDescription || '').trim();

    if (description && this.terminationReasonDescription === '-') {
      this.terminationReasonDescription = description;
    }

    if (reasonId && this.terminationReasonName === '-') {
      this.reasonService.getSelectListByGroupCode(REASON_GROUP_TERMINATE).subscribe({
        next: (result: any[]) => {
          const options = (result || []).map((r: any) => ({ label: r.name || '', value: r.id || '' }));
          const found = options.find((o: any) => o.value === reasonId);
          if (found) {
            this.terminationReasonName = found.label;
          }
        },
        error: () => {}
      });
    }
  }

  get canApprove(): boolean {
    if (!this.id) return false;
    if (this.businessCode === BUSINESS_CODE_TERMINATE) {
      return this.permissionService.isGranted(this.PERMISSIONS.TERMINATE_APPROVE);
    }
    if (this.businessCode === BUSINESS_CODE_ENDORSEMENT) {
      return this.permissionService.isGranted(this.PERMISSIONS.ENDORSEMENT_APPROVE);
    }
    return this.permissionService.isGranted(this.PERMISSIONS.APPROVE);
  }

  /** True when work task is already approved or rejected (Approve/Reject buttons should be disabled). */
  get isApprovalActionDisabled(): boolean {
    return this.workTaskStatus === WorkTaskStatus.Approved || this.workTaskStatus === WorkTaskStatus.Rejected;
  }

  approve(): void {
    this.confirmationService.confirm({
      message: this.localizationService.localize('Policy::PolicyRequestApproval:ConfirmApprove'),
      header: this.localizationService.localize('Policy::Policy:Approve'),
      icon: 'pi pi-check',
      acceptLabel: this.localizationService.localize('AbpUi::Yes'),
      rejectLabel: this.localizationService.localize('AbpUi::No'),
      accept: () => this.doApprove()
    });
  }

  private doApprove(): void {
    this.actionLoading = true;
    this.policyRequestApprovalService.approve(this.id).subscribe({
      next: () => {
        timer(1000)
          .pipe(
            takeUntilDestroyed(this.destroyRef),
            switchMap(() =>
              forkJoin({
                task: this.policyRequestApprovalService.getByWorkTaskId(this.id),
                policy: this.policyFormPage?.reloadEmbeddedPolicyDetail() ?? of(void 0)
              })
            )
          )
          .subscribe({
            next: ({ task }) => {
              this.applyWorkTaskItem(task, { finalizeInitialLoad: false });
              this.actionLoading = false;
              this.messageService.add({
                severity: 'success',
                summary: this.localizationService.localize('AbpUi::Success'),
                detail: this.localizationService.localize('Policy::PolicyRequestApproval:ApprovedSuccess')
              });
            },
            error: () => {
              this.actionLoading = false;
              this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Policy::PolicyRequestApproval:ApproveFailed')
              });
            }
          });
      },
      error: () => {
        this.actionLoading = false;
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: this.localizationService.localize('Policy::PolicyRequestApproval:ApproveFailed')
        });
      }
    });
  }

  openRejectReason(): void {
    this.rejectReasonId = null;
    this.rejectReasonDescription = null;
    this.loadRejectReasonOptions();
  }

  private loadRejectReasonOptions(): void {
    const groupCode = getReasonGroupCodeForBusinessCode(this.businessCode);
    this.rejectReasonOptions = [];
    this.rejectReasonOptionsLoading = true;
    if (!groupCode) {
      this.rejectReasonOptionsLoading = false;
      this.rejectReasonModalVisible = true;
      return;
    }
    const toOptions = (result: unknown[]) =>
      (result || []).map((r: unknown) => ({ label: (r as { name?: string }).name || '', value: (r as { id?: string }).id || '' }));
    this.reasonService.getSelectListByGroupCode(groupCode).subscribe({
      next: (result) => {
        this.rejectReasonOptions = toOptions(Array.isArray(result) ? result : []);
        this.rejectReasonOptionsLoading = false;
        this.rejectReasonModalVisible = true;
      },
      error: () => {
        this.rejectReasonOptions = [];
        this.rejectReasonOptionsLoading = false;
        this.messageService.add({
          severity: 'warn',
          summary: this.localizationService.localize('AbpUi::Warning'),
          detail: this.localizationService.localize('Policy::PolicyRequestApproval:RejectReasonLoadFailed')
        });
        this.rejectReasonModalVisible = true;
      }
    });
  }

  confirmRejectReason(): void {
    if (!this.rejectReasonId) {
      this.messageService.add({
        severity: 'warn',
        summary: this.localizationService.localize('AbpUi::Warning'),
        detail: this.localizationService.localize('Policy::PolicyRequestApproval:SelectReason')
      });
      return;
    }
    const trimmedDescription = (this.rejectReasonDescription || '').trim();
    if (!trimmedDescription) {
      this.messageService.add({
        severity: 'warn',
        summary: this.localizationService.localize('AbpUi::Warning'),
        detail: this.localizationService.localize('Policy::PolicyRequestApproval:ReasonDescriptionRequired')
      });
      return;
    }
    const reasonId = this.rejectReasonId;
    const reasonDescription = trimmedDescription;
    this.rejectReasonModalVisible = false;
    this.confirmationService.confirm({
      message: this.localizationService.localize('Policy::PolicyRequestApproval:ConfirmReject'),
      header: this.localizationService.localize('Policy::Policy:Reject'),
      icon: 'pi pi-times',
      acceptLabel: this.localizationService.localize('AbpUi::Yes'),
      rejectLabel: this.localizationService.localize('AbpUi::No'),
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => this.doReject(reasonId, reasonDescription)
    });
  }

  private doReject(reasonId?: string | null, reasonDescription?: string): void {
    const id = reasonId ?? this.rejectReasonId!;
    const description = reasonDescription ?? (this.rejectReasonDescription || '').trim();
    this.actionLoading = true;
    this.policyRequestApprovalService.reject(this.id, { reasonId: id, reasonDescription: description }).subscribe({
      next: () => {
        this.actionLoading = false;
        this.rejectReasonId = null;
        this.rejectReasonDescription = null;
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('AbpUi::Success'),
          detail: this.localizationService.localize('Policy::PolicyRequestApproval:RejectedSuccess')
        });
        this.backToList();
      },
      error: () => {
        this.actionLoading = false;
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: this.localizationService.localize('Policy::PolicyRequestApproval:RejectFailed')
        });
      }
    });
  }

  cancelRejectReason(): void {
    this.rejectReasonModalVisible = false;
    this.rejectReasonId = null;
    this.rejectReasonDescription = null;
  }

  private loadEndorsementReasonName(reasonId: string | undefined | null): void {
    if (!reasonId) {
      this.endorsementReasonName = '-';
      return;
    }
    this.reasonService.getSelectListByGroupCode(REASON_GROUP_ENDORSEMENT).subscribe({
      next: (result: any[]) => {
        const options = (result || []).map((r: any) => ({ label: r.name || '', value: r.id || '' }));
        const found = options.find(o => o.value === reasonId);
        this.endorsementReasonName = found?.label || '-';
      },
      error: () => {
        this.endorsementReasonName = '-';
      }
    });
  }

  private loadEndorsementTypeName(endorsementTypeId: string | undefined | null): void {
    if (!endorsementTypeId) {
      this.endorsementTypeName = '-';
      return;
    }
    this.adminConfigService.getList({
      code: 'ENDORSEMENT_POLICY_TYPE',
      status: AdminConfigStatus.Active,
      maxResultCount: 1000,
      skipCount: 0
    }).subscribe({
      next: (result) => {
        const found = (result.items || []).find((c: any) => c.id === endorsementTypeId);
        this.endorsementTypeName = found?.name || '-';
      },
      error: () => {
        this.endorsementTypeName = '-';
      }
    });
  }
}
