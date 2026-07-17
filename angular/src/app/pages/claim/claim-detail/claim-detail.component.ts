import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { Subject, forkJoin, of } from 'rxjs';
import { catchError, map, takeUntil } from 'rxjs/operators';
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { TabsModule } from 'primeng/tabs';
import { TableModule } from 'primeng/table';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmPopupModule } from 'primeng/confirmpopup';
import { MessageService, type MenuItem } from 'primeng/api';
import { ConfirmationService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { DatePickerModule } from 'primeng/datepicker';
import { TextareaModule } from 'primeng/textarea';
import { InputNumberModule } from 'primeng/inputnumber';
import { RadioButtonModule } from 'primeng/radiobutton';
import { RatingModule } from 'primeng/rating';
import { TooltipModule } from 'primeng/tooltip';
import { MenuModule } from 'primeng/menu';

import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';
import type { ClaimDetailDto } from '@/proxy/claim/claims/models';
import type { ClaimFolderListDto, ClaimStageProgressDto, ClaimSentMessageDto, ClaimDocumentDto, ClaimPolicyDto } from '@/proxy/claim/claims/models';
import { ClaimService } from '@/proxy/claim/controllers/claim.service';
import { ClaimOnsiteAssessmentTaskService } from '@/proxy/claim/controllers/claim-onsite-assessment-task.service';
import { ClaimFolderService } from '@/proxy/claim/controllers/claim-folder.service';
import { ClaimStageProgressService } from '@/proxy/claim/controllers/claim-stage-progress.service';
import { ClaimSentMessageService } from '@/proxy/claim/controllers/claim-sent-message.service';
import { ClaimDocumentService } from '@/proxy/claim/controllers/claim-document.service';
import { PolicyService } from '@/proxy/policy/controllers/policy.service';
import { PolicyStatus } from '@/proxy/policies/policy-status.enum';
import { ProcessClaimType } from '@/proxy/claims/process-claim-type.enum';
import { ResProvinceService } from '@/proxy/master/controllers/res-province.service';
import { ResWardService } from '@/proxy/master/controllers/res-ward.service';
import { AdminConfigService } from '@/proxy/master/controllers/admin-config.service';
import { HrEmployeeService } from '@/proxy/hr/controllers/hr-employee.service';
import { ClaimStatus } from '@/proxy/claims/claim-status.enum';
import { AssignAssessmentModalComponent } from './assign-assessment-modal/assign-assessment-modal.component';
import { CancelClaimModalComponent } from './cancel-claim-modal/cancel-claim-modal.component';
import { CreateClaimFolderModalComponent } from './create-claim-folder-modal/create-claim-folder-modal.component';

@Component({
  selector: 'app-claim-detail',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    PanelModule,
    ButtonModule,
    TabsModule,
    TableModule,
    ConfirmDialogModule,
    ConfirmPopupModule,
    ToastModule,
    InputTextModule,
    SelectModule,
    DatePickerModule,
    TextareaModule,
    InputNumberModule,
    RadioButtonModule,
    RatingModule,
    TooltipModule,
    MenuModule,
    TranslatePipe,
    AssignAssessmentModalComponent,
    CancelClaimModalComponent,
    CreateClaimFolderModalComponent
  ],
  templateUrl: './claim-detail.component.html',
  styleUrl: './claim-detail.component.scss',
  providers: [MessageService, ConfirmationService]
})
export class ClaimDetailComponent implements OnInit, OnDestroy {
  private readonly destroy$ = new Subject<void>();
  private readonly confirmationService = inject(ConfirmationService);

  claimId: string | null = null;
  claim: ClaimDetailDto | null = null;
  loadingClaim = true;
  currentEmployeeId: string | null = null;
  activeTab = 0;

  // Tab 0: Insurance policies (thông tin bảo hiểm giống màn create, chỉ xem)
  policies: ClaimPolicyDto[] = [];
  policiesLoading = false;

  // Tab 0: Location display & onLocation radio
  incidentProvinceName: string | null = null;
  incidentWardName: string | null = null;
  onLocationValue: 'Y' | 'N' | null = null;

  // Relationship options (for displaying relationship labels)
  relationshipOptions: Array<{ label: string; value: string }> = [];

  // Tab 2: documents (images/videos)
  documents: ClaimDocumentDto[] = [];
  documentsTotal = 0;
  loadingDocuments = false;
  docSkip = 0;
  docPageSize = 50;

  // Tab 3: folders, stage progress, sent messages
  folders: ClaimFolderListDto[] = [];
  foldersTotal = 0;
  loadingFolders = false;
  stageProgress: ClaimStageProgressDto[] = [];
  stageProgressTotal = 0;
  loadingStageProgress = false;
  sentMessages: ClaimSentMessageDto[] = [];
  sentMessagesTotal = 0;
  loadingSentMessages = false;

  assignModalVisible = false;
  createFolderModalVisible = false;
  cancelModalVisible = false;
  hasOnsiteAssessment = false;
  loadingCancelEligibility = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private claimService: ClaimService,
    private claimOnsiteAssessmentTaskService: ClaimOnsiteAssessmentTaskService,
    private claimFolderService: ClaimFolderService,
    private claimStageProgressService: ClaimStageProgressService,
    private claimSentMessageService: ClaimSentMessageService,
    private claimDocumentService: ClaimDocumentService,
    private messageService: MessageService,
    private localizationService: LocalizationService,
    private policyService: PolicyService,
    private provinceService: ResProvinceService,
    private wardService: ResWardService,
    private adminConfigService: AdminConfigService,
    private hrEmployeeService: HrEmployeeService
  ) {}

  get canEditClaim(): boolean {
    if (!this.claim) {
      return false;
    }

    const status = this.claim.status;
    return status === ClaimStatus.Draft;
  }

  get canCancelClaim(): boolean {
    if (!this.claim) return false;
    if (!this.currentEmployeeId) return false;

    // processEmpId là người mở tiếp nhận yêu cầu trong detail response
    if (this.claim.processEmpId !== this.currentEmployeeId) return false;

    const status = this.claim.status;
    return status === ClaimStatus.Draft || status === ClaimStatus.PendingReceive;
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.claimId = id;
      this.loadClaim();
    }

    this.loadRelationships();
    this.hrEmployeeService.getCurrent().subscribe({
      next: (emp) => { this.currentEmployeeId = emp?.id ?? null; },
      error: (err) => {
        console.warn('[ClaimDetail] getCurrent() failed — using creatorId fallback:', err?.status ?? err);
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
          this.hasOnsiteAssessment = false;
          this.loadingCancelEligibility = false;

          if (data.status === ClaimStatus.InProgress && this.claimId) {
            this.loadingCancelEligibility = true;
            forkJoin({
              onsiteExists: this.claimOnsiteAssessmentTaskService.getDetailByClaimId(this.claimId).pipe(
                map(() => true),
                catchError(() => of(false))
              ),
              foldersPage: this.claimFolderService.getList({
                claimId: this.claimId,
                skipCount: 0,
                maxResultCount: 100
              })
            })
              .pipe(takeUntil(this.destroy$))
              .subscribe({
                next: ({ onsiteExists, foldersPage }) => {
                  this.hasOnsiteAssessment = onsiteExists;
                  this.folders = foldersPage.items ?? [];
                  this.foldersTotal = foldersPage.totalCount ?? 0;
                  this.loadingCancelEligibility = false;
                },
                error: () => {
                  this.loadingCancelEligibility = false;
                }
              });
          }

          this.resolveLocationNamesAndOnLocation();
          this.loadPoliciesFromClaim();
          this.loadingClaim = false;
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

  /**
   * Hiển thị trạng thái đơn bảo hiểm giống màn Policy List.
   */
  formatPolicyStatus(status: string | null | undefined): string {
    if (!status) {
      return '';
    }

    const s = status.toString().toLowerCase();

    switch (s) {
      case 'quotation':
        return this.localizationService.localize('Policy::Policy:Quotation');
      case 'draft':
        return this.localizationService.localize('Policy::Policy:Draft');
      case 'active':
        return this.localizationService.localize('Policy::Policy:Active');
      case 'expired':
        return this.localizationService.localize('Policy::Policy:Expired');
      case 'terminated':
        return this.localizationService.localize('Policy::Policy:Terminated');
      case 'cancelled':
        return this.localizationService.localize('Policy::Policy:Cancelled');
      default:
        return status;
    }
  }

  private resolveLocationNamesAndOnLocation(): void {
    this.incidentProvinceName = null;
    this.incidentWardName = null;
    this.onLocationValue = null;

    if (!this.claim) {
      return;
    }

    const onLocation = this.claim.onLocation === 'Y' || this.claim.onLocation === 'N'
      ? (this.claim.onLocation as 'Y' | 'N')
      : null;
    this.onLocationValue = onLocation;

    const provinceId = this.claim.incidentProvinceId;
    const wardId = this.claim.incidentWardId;

    if (!provinceId) {
      return;
    }

    this.provinceService
      .getSelectList()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (items) => {
          const found = (items || []).find((p) => p.id === provinceId);
          this.incidentProvinceName = found?.name || null;
        },
        error: () => {
          this.incidentProvinceName = null;
        }
      });

    if (!wardId) {
      return;
    }

    this.wardService
      .getSelectList(provinceId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (items) => {
          const found = (items || []).find((w) => w.id === wardId);
          this.incidentWardName = found?.name || null;
        },
        error: () => {
          this.incidentWardName = null;
        }
      });
  }

  /**
   * Load relationships (PARTY_IN_RELATIONSHIP) for displaying human-readable labels.
   */
  private loadRelationships(): void {
    this.adminConfigService
      .getSelectList('PARTY_IN_RELATIONSHIP')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (items) => {
          this.relationshipOptions = (items || []).map((item) => ({
            label: item.name || '',
            value: item.subCode || ''
          }));
        },
        error: () => {
          this.relationshipOptions = [];
        }
      });
  }

  getRelationshipLabel(code: string | null | undefined): string {
    if (!code) {
      return '';
    }
    const found = this.relationshipOptions.find((x) => x.value === code);
    return found?.label || code;
  }

  getDriverSexLabel(code: string | null | undefined): string {
    if (!code) {
      return '';
    }
    switch (code) {
      case 'M':
        return this.localizationService.localize('Claim::Male');
      case 'F':
        return this.localizationService.localize('Claim::Female');
      default:
        return code;
    }
  }

  getProcessClaimTypeDisplay(): string {
    const type = this.claim?.processClaimType;
    if (type == null) {
      return '-';
    }

    let key: string;
    switch (type) {
      case ProcessClaimType.Own:
        key = 'Claim::ProcessClaimTypeOwn';
        break;
      case ProcessClaimType.Insurer:
        key = 'Claim::ProcessClaimTypeInsurer';
        break;
      default:
        return String(type);
    }

    return this.localizationService.localize(key) || String(type);
  }

  /**
   * Bảng Thông tin bảo hiểm (InsuranceInfo) – load theo thông tin GCN/BSX/SK/SM trên claim hiện tại.
   */
  private loadPoliciesFromClaim(): void {
    if (!this.claim) {
      this.policies = [];
      return;
    }

    const certificateNo = this.claim.certificateNo?.trim() || undefined;
    const carPlate = this.claim.carPlate?.trim() || undefined;
    const vin = this.claim.vin?.trim() || undefined;
    const engineNumber = this.claim.engineNumber?.trim() || undefined;
    const incidentDate = this.claim.incidentDate || undefined;

    const hasAnyCriteria = !!certificateNo || !!carPlate || !!vin || !!engineNumber;
    if (!hasAnyCriteria) {
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
          this.policies = (items || []).map((x) => ({
            policyId: x.policyId,
            lobName: x.lobName,
            contractId: x.contractId,
            certificateNo: x.certificateNo,
            products: x.products || [],
            carPlate: x.carPlate,
            ownerName: x.ownerName,
            effectDate: x.effectDate,
            expireDate: x.expireDate,
            status: x.status,
            certificateUrl: x.certificateUrl,
            insurerId: x.insurerId,
            insurerName: x.insurerName
          }));
          this.policiesLoading = false;
        },
        error: () => {
          this.policiesLoading = false;
        }
      });
  }

  get filteredPolicies(): ClaimPolicyDto[] {
    if (!this.claim?.insurerId) {
      return this.policies;
    }
    return this.policies.filter((p) => p.insurerId === this.claim!.insurerId);
  }

  refreshPolicies(): void {
    this.loadPoliciesFromClaim();
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

  goBack(): void {
    this.router.navigate(['/pages/claim/list']);
  }

  openEditInNewTab(): void {
    if (!this.claimId) {
      return;
    }

    const tree = this.router.createUrlTree(['/pages/claim/edit', this.claimId]);
    const url = this.router.serializeUrl(tree);
    window.open(url, '_blank');
  }

  closeScreen(): void {
    this.confirmationService.confirm({
      message: this.localizationService.localize('Claim::ConfirmCloseScreen'),
      header: this.localizationService.localize('Claim::Close'),
      icon: 'pi pi-question-circle',
      accept: () => {
        this.router.navigate(['/pages/claim/list']);
      }
    });
  }

  openAssignModal(): void {
    this.assignModalVisible = true;
  }

  onAssignModalVisibleChange(visible: boolean): void {
    this.assignModalVisible = visible;
    if (!visible) this.loadClaim();
  }

  openCreateFolderModal(): void {
    this.createFolderModalVisible = true;
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
  }

  onCreateFolderModalVisibleChange(visible: boolean): void {
    this.createFolderModalVisible = visible;
    if (!visible) {
      if (this.activeTab === 2) this.loadFolders();
    }
  }

  formatDate(value: string | undefined): string {
    if (!value) return '-';
    const d = new Date(value);
    return isNaN(d.getTime()) ? value : d.toLocaleDateString('vi-VN');
  }

  formatDateTime(value: string | undefined): string {
    if (!value) return '-';
    const d = new Date(value);
    if (isNaN(d.getTime())) return value;

    const day = String(d.getDate()).padStart(2, '0');
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const year = d.getFullYear();
    const hour = String(d.getHours()).padStart(2, '0');
    const minute = String(d.getMinutes()).padStart(2, '0');

    return `${day}/${month}/${year} ${hour}:${minute}`;
  }

  formatTotalEstimatedLossAmount(value: number | undefined | null): string {
    if (value == null) {
      return '-';
    }
    return new Intl.NumberFormat('vi-VN').format(value);
  }

  isImage(mime: string | undefined): boolean {
    if (!mime) return false;
    return (mime as string).startsWith('image/');
  }

  openDocUrl(url: string | undefined): void {
    if (url) window.open(url, '_blank');
  }

  /**
   * Xem giấy chứng nhận bảo hiểm của một policy (mở url mới).
   */
  viewPolicyCertificate(policy: ClaimPolicyDto): void {
    const url = policy.certificateUrl;
    if (!url) {
      this.messageService.add({
        severity: 'warn',
        summary: this.localizationService.localize('AbpUi::Warning'),
        detail: this.localizationService.localize('Claim::CertificateUrlNotFound') || 'Không tìm thấy đường dẫn giấy chứng nhận.'
      });
      return;
    }
    window.open(url, '_blank');
  }

  /**
   * Menu hành động cho từng đơn bảo hiểm.
   */
  getPolicyMenuItems(policy: ClaimPolicyDto): MenuItem[] {
    return [
      {
        label: 'Xem giấy chứng nhận',
        icon: 'pi pi-file-pdf',
        command: () => this.viewPolicyCertificate(policy),
        disabled: !policy.certificateUrl
      }
    ];
  }

  copySnapshotLink(): void {
    const link = this.claim?.snapshotLink;
    if (!link) {
      return;
    }

    navigator.clipboard.writeText(link).then(() => {
      this.messageService.add({
        severity: 'success',
        summary: this.localizationService.localize('AbpUi::Success'),
        detail: this.localizationService.localize('Claim::SnapshotLinkCopied')
      });
    });
  }
}
