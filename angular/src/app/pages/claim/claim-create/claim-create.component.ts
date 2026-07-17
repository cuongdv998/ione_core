import { Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { DatePickerModule } from 'primeng/datepicker';
import { MessageService, type MenuItem } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { DialogModule } from 'primeng/dialog';
import { TextareaModule } from 'primeng/textarea';
import { InputNumberModule } from 'primeng/inputnumber';
import { TableModule } from 'primeng/table';
import { TabsModule } from 'primeng/tabs';
import { MenuModule } from 'primeng/menu';
import { RadioButtonModule } from 'primeng/radiobutton';
import { RatingModule } from 'primeng/rating';
import { TooltipModule } from 'primeng/tooltip';

import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';
import { ClaimCreateFormData } from './claim-create.models';
import { ResProvinceService } from '@/proxy/master/controllers/res-province.service';
import { ResWardService } from '@/proxy/master/controllers/res-ward.service';
import { ResPartnerService } from '@/proxy/partner/controllers/res-partner.service';
import { HrDepartmentService } from '@/proxy/hr/controllers/hr-department.service';
import { HrEmployeeService } from '@/proxy/hr/controllers/hr-employee.service';
import { AdminConfigService } from '@/proxy/master/controllers/admin-config.service';
import { ResReasonService } from '@/proxy/master/controllers/res-reason.service';
import { ProcessClaimType } from '@/proxy/claims/process-claim-type.enum';
import type { ClaimPolicyDto } from '@/proxy/claim/claims/models';
import { ClaimService } from '@/proxy/claim/controllers/claim.service';
import type { CreateClaimDto, CreateClaimResponseDto, UpdateClaimDto } from '@/proxy/claim/claims/models';
import { ClaimStatus } from '@/proxy/claims/claim-status.enum';
import { PolicyStatus } from '@/proxy/policies/policy-status.enum';
import { PolicyService } from '@/proxy/policy/controllers/policy.service';
import type { PolicyClaimLookupDto } from '@/proxy/policy/policies/models';

@Component({
  selector: 'app-claim-create',
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
    TextareaModule,
    InputNumberModule,
    TableModule,
    TabsModule,
    RadioButtonModule,
    RatingModule,
    TooltipModule,
    MenuModule,
    TranslatePipe
  ],
  templateUrl: './claim-create.component.html',
  styleUrl: './claim-create.component.scss',
  providers: [MessageService]
})
export class ClaimCreateComponent implements OnInit, OnDestroy {
  @ViewChild('insuranceInfoSection') insuranceInfoSection?: ElementRef<HTMLElement>;

  private readonly destroy$ = new Subject<void>();

  // Dialog
  dialogVisible = false;
  loading = false;
  // p-tabs dùng value dạng string, nên dùng '0' | '1' | '2'
  activeTab: string = '0';
  notifyDate: Date = new Date();

  // Check if component is used as a page (via route) or as a dialog
  get isPageMode(): boolean {
    // Khi dùng qua router (path 'claim/create' hoặc 'claim/detail/:id'),
    // snapshot.url sẽ có ít nhất 1 segment. Khi mở dưới dạng dialog (không qua router),
    // snapshot.url sẽ rỗng.
    return this.route.snapshot.url.length > 0;
  }

  // View-only mode when navigating to /claim/detail/:id
  get isViewMode(): boolean {
    return this.route.snapshot.url.some(segment => segment.path === 'detail');
  }

  // Edit mode when navigating to /claim/edit/:id (chỉ nháp mới được sửa)
  get isEditMode(): boolean {
    return this.route.snapshot.url.some(segment => segment.path === 'edit');
  }

  // Claim id when in edit mode (for update API)
  claimId: string | null = null;

  // Form data
  formData: ClaimCreateFormData = this.getEmptyForm();

  // Chỉ cho phép gửi giám định khi đã tìm được ít nhất 1 đơn bảo hiểm
  get canSaveAndSendAssessment(): boolean {
    return this.policies.length > 0;
  }

  // Policies table
  policies: ClaimPolicyDto[] = [];
  policiesLoading = false;

  // Dropdown options
  processClaimTypeOptions: Array<{ label: string; value: ProcessClaimType }> = [];
  departmentOptions: Array<{ label: string; value: string }> = [];
  loadingDepartments = false;
  employeeOptions: Array<{ label: string; value: string; departmentId?: string }> = [];
  loadingEmployees = false;
  relationshipOptions: Array<{ label: string; value: string }> = [];
  loadingRelationships = false;
  insurerOptions: Array<{ label: string; value: string }> = [];
  loadingInsurers = false;
  provinceOptions: Array<{ label: string; value: string }> = [];
  loadingProvinces = false;
  wardOptions: Array<{ label: string; value: string }> = [];
  loadingWards = false;
  incidentCauseOptions: Array<{ label: string; value: string }> = [];
  loadingIncidentCauses = false;
  incidentResultOptions: Array<{ label: string; value: string }> = [];
  loadingIncidentResults = false;
  garageOptions: Array<{ label: string; value: string }> = [];
  loadingGarages = false;
  driverSexOptions: Array<{ label: string; value: string }> = [];
  driverLicenseLevelOptions: Array<{ label: string; value: string }> = [];
  loadingDriverLicenseLevels = false;
  onLocationOptions: Array<{ label: string; value: 'Y' | 'N' }> = [];

  // Snapshot link
  snapshotLink: string | null = null;

  constructor(
    private claimService: ClaimService,
    private policyService: PolicyService,
    private localizationService: LocalizationService,
    private messageService: MessageService,
    private provinceService: ResProvinceService,
    private wardService: ResWardService,
    private partnerService: ResPartnerService,
    private departmentService: HrDepartmentService,
    private employeeService: HrEmployeeService,
    private adminConfigService: AdminConfigService,
    private resReasonService: ResReasonService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.initializeOptions();
  }

  /**
   * Normalize input: remove non-alphanumeric chars and convert to uppercase.
   */
  private normalizeAlphaNumericUpper(value: string | null | undefined): string | null {
    if (value == null) {
      return null;
    }
    const trimmed = String(value).trim();
    if (!trimmed) {
      return null;
    }
    return trimmed.toUpperCase().replace(/[^A-Z0-9]/g, '');
  }

  private focusField(fieldId: string, tab: string = '0'): void {
    this.activeTab = tab;
    setTimeout(() => {
      const el = document.getElementById(fieldId) as HTMLElement | null;
      if (el) {
        el.focus();
        if ('scrollIntoView' in el) {
          el.scrollIntoView({ behavior: 'smooth', block: 'center' });
        }
      }
    });
  }

  goBack(): void {
    if (this.isPageMode) {
      this.router.navigate(['/pages/claim/list']);
    } else {
      this.closeDialog();
    }
  }

  ngOnInit(): void {
    // Edit/view: không load toàn bộ nhân viên ở đây — loadClaimDetail sẽ gọi loadEmployees(processDeptId).
    this.loadDialogOptions(this.isEditMode || this.isViewMode);

    // Page mode: show as a normal page (no dialog auto-open).
    if (this.isPageMode) {
      this.formData = this.getEmptyForm();
      this.policies = [];
      this.snapshotLink = null;
      this.notifyDate = new Date();
      this.activeTab = '0';
    }

    // View mode: load claim details by id
    if (this.isViewMode) {
      const id = this.route.snapshot.paramMap.get('id');
      if (id) {
        this.loadClaimDetail(id);
      }
    }

    // Edit mode: load claim by id, chỉ cho phép sửa khi trạng thái Nháp
    if (this.isEditMode) {
      const id = this.route.snapshot.paramMap.get('id');
      if (id) {
        this.loadClaimForEdit(id);
      }
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * Initialize static options
   */
  private initializeOptions(): void {
    // Process Claim Type options
    this.processClaimTypeOptions = [
      { label: this.localizationService.localize('Claim::ProcessClaimType:Own'), value: ProcessClaimType.Own },
      { label: this.localizationService.localize('Claim::ProcessClaimType:Insurer'), value: ProcessClaimType.Insurer }
    ];

    // On Location options
    this.onLocationOptions = [
      { label: this.localizationService.localize('Claim::Yes'), value: 'Y' },
      { label: this.localizationService.localize('Claim::No'), value: 'N' }
    ];

    // Driver Sex options
    this.driverSexOptions = [
      { label: this.localizationService.localize('Claim::Male'), value: 'M' },
      { label: this.localizationService.localize('Claim::Female'), value: 'F' }
    ];

    // Set default priority (1 = Low, 2 = Medium, 3 = High)
    this.formData.priority = 2;
  }

  /**
   * Get empty form data (chỉ dùng khi thêm mới; sửa/xem chi tiết lấy giá trị từ API).
   */
  private getEmptyForm(): ClaimCreateFormData {
    return {
      processClaimType: ProcessClaimType.Own,
      processDeptId: null,
      processEmpId: null,
      notifierName: null,
      notifierPhone: null,
      notifierEmail: null,
      notifierInRelationship: null,
      contactName: null,
      contactPhone: null,
      contactEmail: null,
      contactInRelationship: null,
      incidentDate: null,
      certificateNo: null,
      insurerId: null,
      carPlate: null,
      vin: null,
      engineNumber: null,
      incidentProvinceId: null,
      incidentWardId: null,
      incidentAddress: null,
      onLocation: null,
      incidentCauseId: null,
      incidentDescription: null,
      incidentResult: null,
      // Default priority = 2 (Medium)
      priority: 2,
      assessmentDate: null,
      assessmentPartnerId: null,
      personOnCar: null,
      driverName: null,
      driverSex: null,
      driverIdNo: null,
      driverPhone: null,
      driverLicenseNo: null,
      driverLicenseEffectDate: null,
      driverLicenseExpireDate: null,
      driverLicenseLevel: null,
      driverRegistryNo: null,
      driverRegistryEffectDate: null,
      driverRegistryExpireDate: null,
      note: null
    };
  }

  private loadClaimDetail(id: string): void {
    this.loading = true;
    this.claimService.get(id).pipe(takeUntil(this.destroy$)).subscribe({
      next: (detail) => {
        // Map basic info
        this.notifyDate = detail.notifyDate ? new Date(detail.notifyDate) : new Date();

        this.formData.processClaimType = detail.processClaimType ?? null;
        this.formData.processDeptId = detail.processDeptId ?? null;
        this.formData.processEmpId = detail.processEmpId ?? null;

        this.formData.notifierName = detail.notifierName ?? null;
        this.formData.notifierPhone = detail.notifierPhone ?? null;
        this.formData.notifierEmail = detail.notifierEmail ?? null;
        this.formData.notifierInRelationship = detail.notifierInRelationship ?? null;

        this.formData.contactName = detail.contactName ?? null;
        this.formData.contactPhone = detail.contactPhone ?? null;
        this.formData.contactEmail = detail.contactEmail ?? null;
        this.formData.contactInRelationship = detail.contactInRelationship ?? null;

        this.formData.incidentDate = detail.incidentDate ? new Date(detail.incidentDate) : null;
        this.formData.certificateNo = detail.certificateNo ?? null;
        this.formData.insurerId = detail.insurerId ?? null;

        this.formData.carPlate = detail.carPlate ?? null;
        this.formData.vin = detail.vin ?? null;
        this.formData.engineNumber = detail.engineNumber ?? null;

        const emptyGuid = '00000000-0000-0000-0000-000000000000';
        this.formData.incidentProvinceId = (detail.incidentProvinceId && detail.incidentProvinceId !== emptyGuid) ? detail.incidentProvinceId : null;
        this.formData.incidentWardId = (detail.incidentWardId && detail.incidentWardId !== emptyGuid) ? detail.incidentWardId : null;
        this.formData.incidentAddress = detail.incidentAddress ?? null;
        this.formData.onLocation = (detail.onLocation as 'Y' | 'N') ?? null;

        this.formData.incidentCauseId = detail.incidentCauseId ?? null;
        this.formData.incidentDescription = detail.incidentDescription ?? null;
        this.formData.incidentResult = detail.incidentResult ?? null;

        // Priority is stored as 1-3 (1 = Low, 2 = Medium, 3 = High)
        if (detail.priority == null || detail.priority === undefined) {
          this.formData.priority = 2;
        } else {
          this.formData.priority = detail.priority;
        }

        this.formData.assessmentDate = detail.assessmentDate ? new Date(detail.assessmentDate) : null;
        this.formData.assessmentPartnerId = detail.assessmentPartnerId ?? null;

        this.formData.personOnCar = detail.personOnCar ?? null;
        this.formData.driverName = detail.driverName ?? null;
        const driverSex = detail.driverSex === 'M' || detail.driverSex === 'F' ? detail.driverSex : null;
        this.formData.driverSex = driverSex;
        this.formData.driverIdNo = detail.driverIdNo ?? null;
        this.formData.driverPhone = detail.driverPhone ?? null;
        this.formData.driverLicenseNo = detail.driverLicenseNo ?? null;
        this.formData.driverLicenseEffectDate = detail.driverLicenseEffectDate ? new Date(detail.driverLicenseEffectDate) : null;
        this.formData.driverLicenseExpireDate = detail.driverLicenseExpireDate ? new Date(detail.driverLicenseExpireDate) : null;
        this.formData.driverLicenseLevel = detail.driverLicenseLevel ?? null;
        this.formData.driverRegistryNo = detail.driverRegistryNo ?? null;
        this.formData.driverRegistryEffectDate = detail.driverRegistryEffectDate ? new Date(detail.driverRegistryEffectDate) : null;
        this.formData.driverRegistryExpireDate = detail.driverRegistryExpireDate ? new Date(detail.driverRegistryExpireDate) : null;

        this.formData.note = detail.note ?? null;

        this.snapshotLink = detail.snapshotLink ?? null;

        if (this.formData.incidentProvinceId) {
          this.loadWardsForProvince(this.formData.incidentProvinceId);
        }

        // Load đơn bảo hiểm theo GCN / BSX / SK / SM đã có khi xem chi tiết hoặc chỉnh sửa
        this.loadPoliciesFromCurrentForm();

        // Dropdown NV xử lý theo đơn vị xử lý (tránh hiển thị toàn bộ user khi vào sửa/xem)
        this.loadEmployees(this.formData.processDeptId || undefined);

        this.loading = false;
      },
      error: () => {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Claim::Error'),
          detail: this.localizationService.localize('Claim::InternalServerErrorMessage')
        });
        this.loading = false;
      }
    });
  }

  /**
   * Load claim for edit mode. Chỉ cho phép sửa khi trạng thái Nháp (Draft).
   */
  private loadClaimForEdit(id: string): void {
    this.loading = true;
    this.claimService.get(id).pipe(takeUntil(this.destroy$)).subscribe({
      next: (detail) => {
        if (detail.status !== ClaimStatus.Draft) {
          this.messageService.add({
            severity: 'warn',
            summary: this.localizationService.localize('Claim::Error'),
            detail: this.localizationService.localize('Claim::OnlyDraftCanBeEdited')
          });
          this.router.navigate(['/pages/claim/list']);
          this.loading = false;
          return;
        }
        this.claimId = id;
        this.loadClaimDetail(id);
      },
      error: () => {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Claim::Error'),
          detail: this.localizationService.localize('Claim::InternalServerErrorMessage')
        });
        this.loading = false;
      }
    });
  }

  /**
   * Open dialog
   */
  openDialog(): void {
    this.dialogVisible = true;
    this.formData = this.getEmptyForm();
    this.policies = [];
    this.snapshotLink = null;
    this.notifyDate = new Date();
    this.activeTab = '0';
    this.loadDialogOptions();
  }

  /**
   * Close dialog
   */
  closeDialog(): void {
    this.dialogVisible = false;
    this.formData = this.getEmptyForm();
    this.policies = [];
    this.snapshotLink = null;
    
    // Navigate back to claim list if accessed via route
    if (this.isPageMode) {
      this.router.navigate(['/pages/claim/list']);
    }
  }

  /**
   * Load all dropdown options
   * @param skipEmployees Khi true (sửa/xem chi tiết), bỏ qua — danh sách NV xử lý được load theo processDeptId trong loadClaimDetail.
   */
  private loadDialogOptions(skipEmployees = false): void {
    // Load departments
    this.loadDepartments();

    if (!skipEmployees) {
      this.loadEmployees();
    }

    // Load relationships (PARTY_IN_RELATIONSHIP)
    this.loadRelationships();

    // Load insurers
    this.loadInsurers();

    // Load provinces
    this.loadProvinces();

    // Load incident causes (INCIDENT_REASON)
    this.loadIncidentCauses();

    // Load incident results
    this.loadIncidentResults();

    // Load garages (GARAGE)
    this.loadGarages();

    // Load driver license levels (DRIVER_LICENSE_LEVEL)
    this.loadDriverLicenseLevels();
  }

  /**
   * Load full departments for dropdown.
   * Backend: pass `scope='ALL'` to avoid user-scoped results.
   */
  private loadDepartments(): void {
    this.loadingDepartments = true;
    this.departmentService.getSelectList('ALL').subscribe({
      next: (items) => {
        this.departmentOptions = (items || []).map(d => ({
          label: `${d.code || ''} - ${d.name || ''}`,
          value: d.id || ''
        }));
        this.loadingDepartments = false;
      },
      error: () => {
        this.loadingDepartments = false;
      }
    });
  }

  /**
   * Load employees theo đơn vị (department). Nếu không truyền deptId sẽ lấy tất cả.
   */
  private loadEmployees(deptId?: string): void {
    this.loadingEmployees = true;
    this.employeeService
      .getList({
        departmentId: deptId,
        maxResultCount: 500,
        roleCode: 'GDV'
      })
      .subscribe({
        next: (res) => {
          const items = (res.items || []) as Array<{ id?: string; fullName?: string; departmentId?: string }>;
          this.employeeOptions = items.map(e => ({
            label: e.fullName || e.id || '',
            value: e.id || '',
            departmentId: e.departmentId
          }));
          this.loadingEmployees = false;
        },
        error: () => {
          this.loadingEmployees = false;
        }
      });
  }

  /**
   * Khi thay đổi đơn vị xử lý: reset nhân viên (khi người dùng đổi) và load lại theo đơn vị đó.
   */
  onProcessDeptChange(deptId: string | null, isUserChange: boolean = false): void {
    const normalizedDeptId = deptId || undefined;
    this.formData.processDeptId = deptId;

    if (isUserChange) {
      this.formData.processEmpId = null;
    }
    this.loadEmployees(normalizedDeptId);
  }

  /**
   * Khi chọn nhân viên xử lý: tự động cập nhật đơn vị xử lý theo nhân viên.
   */
  onProcessEmpChange(empId: string | null): void {
    this.formData.processEmpId = empId;
    if (!empId) {
      return;
    }

    const selected = this.employeeOptions.find(e => e.value === empId);
    const deptId = selected?.departmentId || null;
    if (!deptId) {
      return;
    }

    // Cập nhật đơn vị xử lý theo nhân viên nhưng không reset lại nhân viên
    this.onProcessDeptChange(deptId, false);
  }

  /**
   * Load relationships (API select: AdminConfigAppService.GetSelectListAsync code = PARTY_IN_RELATIONSHIP)
   */
  private loadRelationships(): void {
    this.loadingRelationships = true;
    this.adminConfigService.getSelectList('PARTY_IN_RELATIONSHIP').subscribe({
      next: (items) => {
        this.relationshipOptions = (items || []).map(item => ({
          label: item.name || '',
          value: item.subCode || ''
        }));
        this.loadingRelationships = false;
      },
      error: () => {
        this.loadingRelationships = false;
      }
    });
  }

  /**
   * Load insurers
   */
  /**
   * Load insurers (API select: ResPartnerAppService.GetSelectListAsync partnerTypeCode = INSURER)
   */
  private loadInsurers(): void {
    this.loadingInsurers = true;
    this.partnerService.getSelectList('INSURER').subscribe({
      next: (items) => {
        this.insurerOptions = (items || []).map(p => ({
          label: p.name || '',
          value: p.id || ''
        }));
        this.loadingInsurers = false;
      },
      error: () => {
        this.loadingInsurers = false;
      }
    });
  }

  /**
   * Load provinces (API select: ResProvinceAppService.GetSelectListAsync)
   */
  private loadProvinces(): void {
    this.loadingProvinces = true;
    this.provinceService.getSelectList().subscribe({
      next: (items) => {
        this.provinceOptions = (items || []).map(p => ({
          label: p.name || '',
          value: p.id || ''
        }));
        this.loadingProvinces = false;
      },
      error: () => {
        this.loadingProvinces = false;
      }
    });
  }

  /**
   * Load wards when province changes
   */
  onProvinceChange(): void {
    if (!this.formData.incidentProvinceId) {
      this.wardOptions = [];
      this.formData.incidentWardId = null;
      return;
    }

    this.loadingWards = true;
    this.wardService.getSelectList(this.formData.incidentProvinceId ?? undefined).subscribe({
      next: (items) => {
        this.wardOptions = (items || []).map(w => ({
          label: w.name || '',
          value: w.id || ''
        }));
        this.loadingWards = false;
      },
      error: () => {
        this.loadingWards = false;
      }
    });
  }

  /**
   * Load ward options for a given province (e.g. when loading claim detail so Phường/Xã displays).
   */
  private loadWardsForProvince(provinceId: string): void {
    this.loadingWards = true;
    this.wardService.getSelectList(provinceId).subscribe({
      next: (items) => {
        this.wardOptions = (items || []).map(w => ({
          label: w.name || '',
          value: w.id || ''
        }));
        this.loadingWards = false;
      },
      error: () => {
        this.loadingWards = false;
      }
    });
  }

  /**
   * Load incident causes (API select: ResReasonAppService.GetSelectListByGroupCodeAsync code = INCIDENT_REASON)
   */
  private loadIncidentCauses(): void {
    this.loadingIncidentCauses = true;
    this.resReasonService.getSelectListByGroupCode('INCIDENT_REASON').subscribe({
      next: (items) => {
        this.incidentCauseOptions = (items || []).map(r => ({
          label: r.name || '',
          value: r.id || ''
        }));
        this.loadingIncidentCauses = false;
      },
      error: () => {
        this.loadingIncidentCauses = false;
      }
    });
  }

  /**
   * Load incident results
   */
  private loadIncidentResults(): void {
    // TODO: Load from appropriate source (might be admin_config or res_reason)
    // For now, using a simple input field
    this.incidentResultOptions = [];
  }

  /**
   * Load garages (API select: ResPartnerAppService.GetSelectListAsync partnerTypeCode = GARAGE)
   */
  private loadGarages(): void {
    this.loadingGarages = true;
    this.partnerService.getSelectList('GARAGE').subscribe({
      next: (items) => {
        this.garageOptions = (items || []).map(p => ({
          label: p.name || '',
          value: p.id || ''
        }));
        this.loadingGarages = false;
      },
      error: () => {
        this.loadingGarages = false;
      }
    });
  }

  /**
   * Load driver license levels (API select: AdminConfigAppService.GetSelectListAsync code = DRIVER_LICENSE_LEVEL)
   */
  private loadDriverLicenseLevels(): void {
    this.loadingDriverLicenseLevels = true;
    this.adminConfigService.getSelectList('DRIVER_LICENSE_LEVEL').subscribe({
      next: (items) => {
        this.driverLicenseLevelOptions = (items || []).map(item => {
          const selectItem = item as unknown as { value?: string; subCode?: string; code?: string };
          return {
            label: item.name || '',
            value: selectItem.value || selectItem.subCode || selectItem.code || ''
          };
        });
        this.loadingDriverLicenseLevels = false;
      },
      error: () => {
        this.loadingDriverLicenseLevels = false;
      }
    });
  }

  /**
   * Copy notifier info to contact info
   */
  copyNotifierToContact(): void {
    this.formData.contactName = this.formData.notifierName;
    this.formData.contactPhone = this.formData.notifierPhone;
    this.formData.contactEmail = this.formData.notifierEmail;
    this.formData.contactInRelationship = this.formData.notifierInRelationship;
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

  /**
   * Normalize car plate: uppercase and remove special characters.
   */
  onCarPlateChange(value: string | null): void {
    this.formData.carPlate = this.normalizeAlphaNumericUpper(value);
  }

  /**
   * Normalize VIN: uppercase and remove special characters.
   */
  onVinChange(value: string | null): void {
    this.formData.vin = this.normalizeAlphaNumericUpper(value);
  }

  /**
   * Normalize engine number: uppercase and remove special characters.
   */
  onEngineNumberChange(value: string | null): void {
    this.formData.engineNumber = this.normalizeAlphaNumericUpper(value);
  }

  /**
   * Normalize vehicle input fields on each change and reflect back to UI immediately.
   * This ensures special characters are removed as soon as user finishes typing
   * (and also when they move focus to another field).
   */
  onVehicleInput(event: Event, field: 'carPlate' | 'vin' | 'engineNumber'): void {
    const inputEl = event.target as HTMLInputElement | null;
    if (!inputEl) {
      return;
    }

    const rawValue = inputEl.value;
    const normalized = this.normalizeAlphaNumericUpper(rawValue);

    // Update form model
    this.formData[field] = normalized;

    // Reflect normalized value back to the input so UI never shows invalid chars
    inputEl.value = normalized ?? '';
  }

  /**
   * Refresh policies table
   */
  refreshPolicies(): void {
    this.searchPolicies(false);
  }

  /** Chạy tìm đơn bảo hiểm khi người dùng rời khỏi ô nhập (blur). */
  onPolicySearchBlur(): void {
    this.searchPolicies(false);
  }

  onIncidentDateChange(): void {
    const hasLookupCriteria =
      !!this.formData.certificateNo?.trim() ||
      !!this.normalizeAlphaNumericUpper(this.formData.carPlate) ||
      !!this.normalizeAlphaNumericUpper(this.formData.vin) ||
      !!this.normalizeAlphaNumericUpper(this.formData.engineNumber);

    if (!hasLookupCriteria) {
      return;
    }

    this.policies = [];
    this.applyInsurerOptionsFromPolicies();
  }

  onPolicySearchClick(): void {
    // Manual trigger (eye icon): search immediately + focus to insurance table
    this.searchPolicies(true);
  }

  private searchPolicies(focusInsuranceTable: boolean): void {
    const certificateNoRaw = this.formData.certificateNo?.trim() || undefined;
    const carPlateNormalized = this.normalizeAlphaNumericUpper(this.formData.carPlate);
    const vinNormalized = this.normalizeAlphaNumericUpper(this.formData.vin);
    const engineNumberNormalized = this.normalizeAlphaNumericUpper(this.formData.engineNumber);

    // Update form values so UI always shows normalized data when searching
    this.formData.carPlate = carPlateNormalized;
    this.formData.vin = vinNormalized;
    this.formData.engineNumber = engineNumberNormalized;

    const certificateNo = certificateNoRaw;
    const carPlate = carPlateNormalized || undefined;
    const vin = vinNormalized || undefined;
    const engineNumber = engineNumberNormalized || undefined;

    const incidentDate = this.formData.incidentDate ? this.formatDateTimeLocal(this.formData.incidentDate) ?? undefined : undefined;

    const hasAnyCriteria = !!certificateNo || !!carPlate || !!vin || !!engineNumber;
    if (!hasAnyCriteria) {
      this.policies = [];
      return;
    }

    // Avoid noisy queries for very short inputs (auto-search use case)
    const isManual = focusInsuranceTable;
    const minLen = isManual ? 1 : 3;
    const meetsMinLength =
      (certificateNo?.length ?? 0) >= minLen ||
      (carPlate?.length ?? 0) >= minLen ||
      (vin?.length ?? 0) >= minLen ||
      (engineNumber?.length ?? 0) >= minLen;

    if (!meetsMinLength) {
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
      .subscribe({
        next: (items) => {
          this.policies = (items || []).map((x) => this.mapPolicyLookupToClaimPolicy(x));
          this.applyInsurerOptionsFromPolicies();
          this.policiesLoading = false;

          if (focusInsuranceTable) {
            this.focusInsuranceInfoTable();
          }
        },
        error: (error) => {
          this.policiesLoading = false;
          this.messageService.add({
            severity: 'error',
            summary: this.localizationService.localize('AbpUi::Error'),
            detail: error?.error?.error?.message || this.localizationService.localize('AbpUi::UnexpectedError'),
          });
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

  /**
   * Load đơn bảo hiểm theo GCN / BSX / SK / SM đã có trên form (khi xem chi tiết hoặc chỉnh sửa).
   * Gọi sau khi load claim detail để hiển thị danh sách đơn và cập nhật insurerOptions.
   */
  private loadPoliciesFromCurrentForm(): void {
    const certificateNo = this.formData.certificateNo?.trim() || undefined;
    const carPlate = this.normalizeAlphaNumericUpper(this.formData.carPlate) || undefined;
    const vin = this.normalizeAlphaNumericUpper(this.formData.vin) || undefined;
    const engineNumber = this.normalizeAlphaNumericUpper(this.formData.engineNumber) || undefined;

    // Keep normalized values on form as well
    this.formData.carPlate = carPlate ?? null;
    this.formData.vin = vin ?? null;
    this.formData.engineNumber = engineNumber ?? null;
    const incidentDate = this.formData.incidentDate ? this.formatDateTimeLocal(this.formData.incidentDate) ?? undefined : undefined;

    const hasAnyCriteria = !!certificateNo || !!carPlate || !!vin || !!engineNumber;
    if (!hasAnyCriteria) {
      this.policies = [];
      this.loadInsurers();
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
      .subscribe({
        next: (items) => {
          this.policies = (items || []).map((x) => this.mapPolicyLookupToClaimPolicy(x));
          this.applyInsurerOptionsFromPolicies();
          this.policiesLoading = false;
        },
        error: () => {
          this.policiesLoading = false;
          this.loadInsurers();
        },
      });
  }

  /**
   * Sau khi tìm đơn theo CertificateNo (hoặc BSX/SK/SM): lấy danh sách InsurerName từ kết quả,
   * cập nhật insurerOptions. Nếu không có đơn thì gọi lại loadInsurers() (danh sách đầy đủ).
   */
  private applyInsurerOptionsFromPolicies(): void {
    if (this.policies.length === 0) {
      this.loadInsurers();
      return;
    }
    const seen = new Map<string, string>();
    for (const p of this.policies) {
      if (p.insurerId && p.insurerName && !seen.has(p.insurerId)) {
        seen.set(p.insurerId, p.insurerName);
      }
    }
    this.insurerOptions = Array.from(seen.entries()).map(([value, label]) => ({ label, value }));
    const currentInList = this.insurerOptions.some(o => o.value === this.formData.insurerId);
    if (!currentInList && this.insurerOptions.length > 0) {
      this.formData.insurerId = this.insurerOptions[0].value;
    } else if (!currentInList) {
      this.formData.insurerId = null;
    }
  }

  /**
   * Đơn bảo hiểm hiển thị trong bảng: lọc theo InsurerName đã chọn (nếu có).
   */
  get filteredPolicies(): ClaimPolicyDto[] {
    if (!this.formData.insurerId) {
      return this.policies;
    }
    return this.policies.filter(p => p.insurerId === this.formData.insurerId);
  }

  private focusInsuranceInfoTable(): void {
    const el = this.insuranceInfoSection?.nativeElement;
    if (!el) return;

    el.scrollIntoView({ behavior: 'smooth', block: 'start' });
    // Focus after scroll so keyboard users land on table section
    setTimeout(() => el.focus(), 250);
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

  /**
   * Format Date to full local datetime string (YYYY-MM-DDTHH:mm:ss)
   * to preserve both date and time when sending to backend.
   */
  private formatDateTimeLocal(date: Date | null): string | null {
    if (!date) return null;
    const d = new Date(date);
    const year = d.getFullYear();
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    const hour = String(d.getHours()).padStart(2, '0');
    const minute = String(d.getMinutes()).padStart(2, '0');
    const second = String(d.getSeconds()).padStart(2, '0');
    return `${year}-${month}-${day}T${hour}:${minute}:${second}`;
  }

  private formatDateOnly(date: Date): string {
    const d = new Date(date);
    const year = d.getFullYear();
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  /**
   * Email format validation - same rule as backend [EmailAddress]:
   * when value present must be valid email, max 50 chars.
   */
  private isValidEmailFormat(value: string | null | undefined): boolean {
    const s = value == null ? '' : String(value).trim();
    if (s === '') return true;
    if (s.length > 50) return false;
    return /^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(s);
  }

  /**
   * Validate Phone
   * Rule: Only numbers, 10 digits, start with 0
   */
  private validatePhone(phone: string | null | undefined): boolean {
    if (!phone) return true; // Checked separately if required
    const s = String(phone).trim();
    const regex = /^0[0-9]{9}$/;
    return regex.test(s);
  }

  /**
   * Copy snapshot link
   */
  copySnapshotLink(): void {
    if (this.snapshotLink) {
      navigator.clipboard.writeText(this.snapshotLink).then(() => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('AbpUi::Success'),
          detail: this.localizationService.localize('Claim::SnapshotLinkCopied')
        });
      });
    }
  }

  /**
   * Validate form
   */
  private validateForm(): boolean {
    // Required fields validation (ProcessClaimType = 0 = Own is valid, chỉ báo lỗi khi null/undefined)
    if (this.formData.processClaimType == null || this.formData.processClaimType === undefined) {
      this.focusField('processClaimType');
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Claim::ProcessClaimTypeRequired')
      });
      return false;
    }

    if (!this.formData.notifierName?.trim()) {
      this.focusField('notifierName');
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Claim::NotifierNameRequired')
      });
      return false;
    }

    if (!this.formData.notifierPhone?.trim()) {
      this.focusField('notifierPhone');
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Claim::NotifierPhoneRequired')
      });
      return false;
    }
    if (!this.validatePhone(this.formData.notifierPhone)) {
      this.focusField('notifierPhone');
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: 'Số điện thoại người thông báo không hợp lệ (phải gồm 10 chữ số và bắt đầu bằng 0).'
      });
      return false;
    }

    if (!this.formData.notifierInRelationship) {
      this.focusField('notifierInRelationship');
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Claim::NotifierInRelationshipRequired')
      });
      return false;
    }

    if (!this.formData.contactName?.trim()) {
      this.focusField('contactName');
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Claim::ContactNameRequired')
      });
      return false;
    }

    if (!this.formData.contactPhone?.trim()) {
      this.focusField('contactPhone');
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Claim::ContactPhoneRequired')
      });
      return false;
    }
    if (!this.validatePhone(this.formData.contactPhone)) {
      this.focusField('contactPhone');
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: 'Số điện thoại người liên hệ không hợp lệ (phải gồm 10 chữ số và bắt đầu bằng 0).'
      });
      return false;
    }

    if (!this.formData.contactInRelationship) {
      this.focusField('contactInRelationship');
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Claim::ContactInRelationshipRequired')
      });
      return false;
    }

    if (!this.formData.incidentDate) {
      this.focusField('incidentDate');
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Claim::IncidentDateRequired')
      });
      return false;
    }

    // Validate at least one of car_plate / vin / engine_number is provided
    const hasVehicleInfo = !!this.formData.carPlate?.trim() ||
                           !!this.formData.vin?.trim() ||
                           !!this.formData.engineNumber?.trim();

    if (!hasVehicleInfo) {
      this.focusField('carPlate');
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: 'Bạn phải nhập ít nhất một trong các thông tin: Biển số xe / Số khung / Số máy.'
      });
      return false;
    }

    // Validate email format when provided
    if (!this.isValidEmailFormat(this.formData.notifierEmail)) {
      this.focusField('notifierEmail');
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: 'Email người thông báo không hợp lệ.'
      });
      return false;
    }

    if (!this.isValidEmailFormat(this.formData.contactEmail)) {
      this.focusField('contactEmail');
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: 'Email người liên hệ không hợp lệ.'
      });
      return false;
    }

    if (!this.formData.insurerId) {
      this.focusField('insurerId');
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Claim::InsurerIdRequired')
      });
      return false;
    }

    if (!this.formData.incidentProvinceId) {
      this.focusField('incidentProvinceId');
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Claim::IncidentProvinceIdRequired')
      });
      return false;
    }

    if (!this.formData.incidentWardId) {
      this.focusField('incidentWardId');
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Claim::IncidentWardIdRequired')
      });
      return false;
    }

    if (!this.formData.onLocation) {
      this.focusField('onLocationY');
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Claim::OnLocationRequired')
      });
      return false;
    }

    if (!this.formData.incidentCauseId) {
      this.focusField('incidentCauseId');
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Claim::IncidentCauseIdRequired')
      });
      return false;
    }

    if (!this.formData.incidentResult?.trim()) {
      this.focusField('incidentResult');
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Claim::IncidentResultRequired')
      });
      return false;
    }

    // Validate DriverLicenseExpireDate not earlier than DriverLicenseEffectDate / DriverRegistryEffectDate
    if (this.formData.driverLicenseExpireDate) {
      const expire = new Date(this.formData.driverLicenseExpireDate);
      const effect = this.formData.driverLicenseEffectDate ? new Date(this.formData.driverLicenseEffectDate) : null;
      const registryEffect = this.formData.driverRegistryEffectDate ? new Date(this.formData.driverRegistryEffectDate) : null;
      const registryExpireEffect = this.formData.driverRegistryExpireDate ? new Date(this.formData.driverRegistryExpireDate) : null;

      if (effect && expire < effect) {
        this.focusField('driverLicenseExpireDate');
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: 'Ngày hết hạn GPLX không được nhỏ hơn ngày hiệu lực GPLX.'
        });
        return false;
      }

      if (registryExpireEffect && registryEffect && registryExpireEffect < registryEffect) {
        this.focusField('driverLicenseExpireDate');
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: 'Ngày hết hạn đăng kiểm không được nhỏ hơn ngày hiệu lực đăng kiểm.'
        });
        return false;
      }
    }

    return true;
  }

  /**
   * Save as draft
   */
  saveDraft(): void {
    if (!this.validateForm()) {
      return;
    }

    this.loading = true;
    const input = this.buildCreateClaimDto('draft');

    this.claimService.create(input).subscribe({
      next: (response) => {
        this.loading = false;
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('AbpUi::Success'),
          detail: this.localizationService.localize('Claim::CreateSuccess')
        });
        this.policies = response.policies || [];
        this.snapshotLink = response.snapshotLink || null;
        
        // Navigate back to claim list if accessed via route
        if (this.isPageMode) {
          this.router.navigate(['/pages/claim/list']);
        } else {
          this.closeDialog();
        }
      },
      error: (error) => {
        this.loading = false;
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: error.error?.error?.message || this.localizationService.localize('Claim::CreateError')
        });
      }
    });
  }

  /**
   * Save and assign
   */
  saveAndAssign(): void {
    if (!this.validateForm()) {
      return;
    }

    // Validate process_dept_id and process_emp_id for assign action
    if (!this.formData.processDeptId || !this.formData.processEmpId) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Claim::ProcessDeptAndEmpRequiredForAssign')
      });
      return;
    }

    this.loading = true;
    const input = this.buildCreateClaimDto('assign');

    this.claimService.create(input).subscribe({
      next: (response) => {
        this.loading = false;
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('AbpUi::Success'),
          detail: this.localizationService.localize('Claim::CreateAndAssignSuccess')
        });
        this.policies = response.policies || [];
        this.snapshotLink = response.snapshotLink || null;
        
        // Navigate back to claim list if accessed via route
        if (this.isPageMode) {
          this.router.navigate(['/pages/claim/list']);
        } else {
          this.closeDialog();
        }
      },
      error: (error) => {
        this.loading = false;
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: error.error?.error?.message || this.localizationService.localize('Claim::CreateError')
        });
      }
    });
  }

  /**
   * Lưu và gửi giám định: tạo claim rồi gọi quy trình Elsa (CLAIM_ASSIGN).
   */
  saveAndSendAssessment(): void {
    if (!this.validateForm()) {
      return;
    }

    if (!this.formData.processDeptId || !this.formData.processEmpId) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: 'Bạn bắt buộc phải chọn đơn vị và người giám định!'
      });
      return;
    }

    this.loading = true;
    const input = this.buildCreateClaimDto('assign');

    this.claimService.create(input).subscribe({
      next: (response) => {
        this.claimService.submitForAssessment(response.claimId!).subscribe({
          next: () => {
            this.loading = false;
            this.messageService.add({
              severity: 'success',
              summary: this.localizationService.localize('AbpUi::Success'),
              detail: this.localizationService.localize('Claim::CreateAndAssignSuccess')
            });
            if (this.isPageMode) {
              this.router.navigate(['/pages/claim/list']);
            } else {
              this.closeDialog();
            }
          },
          error: (error) => {
            this.loading = false;
            this.messageService.add({
              severity: 'error',
              summary: this.localizationService.localize('AbpUi::Error'),
              detail: error.error?.error?.message || this.localizationService.localize('Claim::CreateError')
            });
          }
        });
      },
      error: (error) => {
        this.loading = false;
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: error.error?.error?.message || this.localizationService.localize('Claim::CreateError')
        });
      }
    });
  }

  /**
   * Cập nhật yêu cầu bồi thường (chỉ khi trạng thái Nháp)
   */
  saveUpdate(): void {
    if (!this.claimId) return;
    if (!this.validateForm()) return;

    this.loading = true;
    const input = this.buildUpdateClaimDto();

    this.claimService.update(this.claimId, input).subscribe({
      next: () => {
        this.loading = false;
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('AbpUi::Success'),
          detail: this.localizationService.localize('Claim::UpdateSuccess')
        });
        this.router.navigate(['/pages/claim/list']);
      },
      error: (error) => {
        this.loading = false;
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: error.error?.error?.message || this.localizationService.localize('Claim::UpdateError')
        });
      }
    });
  }

  /**
   * Cập nhật và gửi giám định (màn sửa yêu cầu, chỉ khi trạng thái Nháp).
   */
  saveUpdateAndSendAssessment(): void {
    if (!this.claimId) {
      return;
    }
    if (!this.validateForm()) {
      return;
    }

    if (!this.formData.processDeptId || !this.formData.processEmpId) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Claim::ProcessDeptAndEmpRequiredForAssign')
      });
      return;
    }

    this.loading = true;
    const input = this.buildUpdateClaimDto();

    this.claimService.update(this.claimId, input).subscribe({
      next: () => {
        this.claimService.submitForAssessment(this.claimId!).subscribe({
          next: () => {
            this.loading = false;
            this.messageService.add({
              severity: 'success',
              summary: this.localizationService.localize('AbpUi::Success'),
              detail: this.localizationService.localize('Claim::CreateAndAssignSuccess')
            });
            this.router.navigate(['/pages/claim/list']);
          },
          error: (error) => {
            this.loading = false;
            this.messageService.add({
              severity: 'error',
              summary: this.localizationService.localize('AbpUi::Error'),
              detail: error.error?.error?.message || this.localizationService.localize('Claim::UpdateError')
            });
          }
        });
      },
      error: (error) => {
        this.loading = false;
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: error.error?.error?.message || this.localizationService.localize('Claim::UpdateError')
        });
      }
    });
  }

  /**
   * Build CreateClaimDto from form data
   */
  private buildCreateClaimDto(action: 'draft' | 'assign'): CreateClaimDto {
    // Format dates that only need day precision to YYYY-MM-DD
    const formatDate = (date: Date | null): string | null => {
      if (!date) return null;
      const d = new Date(date);
      const year = d.getFullYear();
      const month = String(d.getMonth() + 1).padStart(2, '0');
      const day = String(d.getDate()).padStart(2, '0');
      return `${year}-${month}-${day}`;
    };

    // Convert null to undefined helper
    const nullToUndefined = <T>(value: T | null): T | undefined => {
      return value === null ? undefined : value;
    };

    // Validate required fields (ProcessClaimType = 0 = Own is valid)
    if (this.formData.processClaimType == null || this.formData.processClaimType === undefined) {
      throw new Error('ProcessClaimType is required');
    }
    if (!this.formData.incidentDate) {
      throw new Error('IncidentDate is required');
    }

    // Preserve both date and time for incidentDate & assessmentDate
    const incidentDateStr = this.formatDateTimeLocal(this.formData.incidentDate);
    const assessmentDateStr = this.formData.assessmentDate ? this.formatDateTimeLocal(this.formData.assessmentDate) : null;
    
    if (!incidentDateStr) {
      throw new Error('Failed to format required dates');
    }

    const normalizedCarPlate = this.normalizeAlphaNumericUpper(this.formData.carPlate);
    const normalizedVin = this.normalizeAlphaNumericUpper(this.formData.vin);
    const normalizedEngineNumber = this.normalizeAlphaNumericUpper(this.formData.engineNumber);

    const input: CreateClaimDto = {
      processClaimType: this.formData.processClaimType!, // Already ProcessClaimType enum, validated above
      processDeptId: nullToUndefined(this.formData.processDeptId),
      processEmpId: nullToUndefined(this.formData.processEmpId),
      notifierName: this.formData.notifierName?.trim() || '',
      notifierPhone: this.formData.notifierPhone?.trim() || '',
      notifierEmail: nullToUndefined(this.formData.notifierEmail?.trim()),
      notifierInRelationship: this.formData.notifierInRelationship || '',
      contactName: this.formData.contactName?.trim() || '',
      contactPhone: this.formData.contactPhone?.trim() || '',
      contactEmail: nullToUndefined(this.formData.contactEmail?.trim()),
      contactInRelationship: this.formData.contactInRelationship || '',
      incidentDate: incidentDateStr,
      certificateNo: nullToUndefined(this.formData.certificateNo?.trim()),
      insurerId: this.formData.insurerId || '',
      carPlate: nullToUndefined(normalizedCarPlate),
      vin: nullToUndefined(normalizedVin),
      engineNumber: nullToUndefined(normalizedEngineNumber),
      incidentProvinceId: this.formData.incidentProvinceId || '',
      incidentWardId: this.formData.incidentWardId || '',
      incidentAddress: nullToUndefined(this.formData.incidentAddress?.trim()),
      onLocation: this.formData.onLocation || '',
      incidentCauseId: this.formData.incidentCauseId || '',
      incidentDescription: nullToUndefined(this.formData.incidentDescription?.trim()),
      incidentResult: this.formData.incidentResult?.trim() || '',
      // Priority 1-3 (1 = Low, 2 = Medium, 3 = High)
      priority: (() => {
        const value = this.formData.priority;
        if (value == null || value === undefined) {
          return 2;
        }
        const v = Number(value);
        if (Number.isNaN(v)) {
          return 2;
        }
        return Math.min(3, Math.max(1, v));
      })(),
      assessmentDate: assessmentDateStr ?? '',
      assessmentPartnerId: this.formData.assessmentPartnerId || '',
      personOnCar: nullToUndefined(this.formData.personOnCar),
      driverName: nullToUndefined(this.formData.driverName?.trim()),
      driverSex: nullToUndefined(this.formData.driverSex),
      driverIdNo: nullToUndefined(this.formData.driverIdNo?.trim()),
      driverLicenseNo: nullToUndefined(this.formData.driverLicenseNo?.trim()),
      driverLicenseEffectDate: nullToUndefined(formatDate(this.formData.driverLicenseEffectDate)),
      driverLicenseLevel: nullToUndefined(this.formData.driverLicenseLevel),
      driverRegistryNo: nullToUndefined(this.formData.driverRegistryNo?.trim()),
      driverRegistryEffectDate: nullToUndefined(formatDate(this.formData.driverRegistryEffectDate)),
      note: nullToUndefined(this.formData.note?.trim()),
      action: action
    };

    // Extra fields may not yet exist in generated proxy typings.
    // They are safe to send and will be picked up once proxy is regenerated.
    (input as any).driverPhone = nullToUndefined(this.formData.driverPhone?.trim());
    (input as any).driverLicenseExpireDate = nullToUndefined(formatDate(this.formData.driverLicenseExpireDate));
    (input as any).driverRegistryExpireDate = nullToUndefined(formatDate(this.formData.driverRegistryExpireDate));

    return input;
  }

  /**
   * Build UpdateClaimDto from form data (same structure as CreateClaimDto, no action)
   */
  private buildUpdateClaimDto(): UpdateClaimDto {
    const formatDate = (date: Date | null): string | null => {
      if (!date) return null;
      const d = new Date(date);
      const year = d.getFullYear();
      const month = String(d.getMonth() + 1).padStart(2, '0');
      const day = String(d.getDate()).padStart(2, '0');
      return `${year}-${month}-${day}`;
    };
    const nullToUndefined = <T>(value: T | null): T | undefined =>
      value === null ? undefined : value;

    if (this.formData.processClaimType == null || this.formData.processClaimType === undefined ||
        !this.formData.incidentDate) {
      throw new Error('Required fields missing');
    }
    // Preserve both date and time for incidentDate & assessmentDate
    const incidentDateStr = this.formatDateTimeLocal(this.formData.incidentDate)!;
    const assessmentDateStr = this.formData.assessmentDate ? this.formatDateTimeLocal(this.formData.assessmentDate) : null;

    const normalizedCarPlate = this.normalizeAlphaNumericUpper(this.formData.carPlate);
    const normalizedVin = this.normalizeAlphaNumericUpper(this.formData.vin);
    const normalizedEngineNumber = this.normalizeAlphaNumericUpper(this.formData.engineNumber);

    const input: UpdateClaimDto = {
      processClaimType: this.formData.processClaimType,
      processDeptId: nullToUndefined(this.formData.processDeptId),
      processEmpId: nullToUndefined(this.formData.processEmpId),
      notifierName: this.formData.notifierName?.trim() || '',
      notifierPhone: this.formData.notifierPhone?.trim() || '',
      notifierEmail: nullToUndefined(this.formData.notifierEmail?.trim()),
      notifierInRelationship: this.formData.notifierInRelationship || '',
      contactName: this.formData.contactName?.trim() || '',
      contactPhone: this.formData.contactPhone?.trim() || '',
      contactEmail: nullToUndefined(this.formData.contactEmail?.trim()),
      contactInRelationship: this.formData.contactInRelationship || '',
      incidentDate: incidentDateStr,
      certificateNo: nullToUndefined(this.formData.certificateNo?.trim()),
      insurerId: this.formData.insurerId || '',
      carPlate: nullToUndefined(normalizedCarPlate),
      vin: nullToUndefined(normalizedVin),
      engineNumber: nullToUndefined(normalizedEngineNumber),
      incidentProvinceId: this.formData.incidentProvinceId || '',
      incidentWardId: this.formData.incidentWardId || '',
      incidentAddress: nullToUndefined(this.formData.incidentAddress?.trim()),
      onLocation: this.formData.onLocation || '',
      incidentCauseId: this.formData.incidentCauseId || '',
      incidentDescription: nullToUndefined(this.formData.incidentDescription?.trim()),
      incidentResult: this.formData.incidentResult?.trim() || '',
      // Priority 1-3 (1 = Low, 2 = Medium, 3 = High)
      priority: (() => {
        const value = this.formData.priority;
        if (value == null || value === undefined) {
          return 2;
        }
        const v = Number(value);
        if (Number.isNaN(v)) {
          return 2;
        }
        return Math.min(3, Math.max(1, v));
      })(),
      assessmentDate: assessmentDateStr ?? '',
      assessmentPartnerId: this.formData.assessmentPartnerId || '',
      personOnCar: nullToUndefined(this.formData.personOnCar),
      driverName: nullToUndefined(this.formData.driverName?.trim()),
      driverSex: nullToUndefined(this.formData.driverSex),
      driverIdNo: nullToUndefined(this.formData.driverIdNo?.trim()),
      driverLicenseNo: nullToUndefined(this.formData.driverLicenseNo?.trim()),
      driverLicenseEffectDate: nullToUndefined(formatDate(this.formData.driverLicenseEffectDate)),
      driverLicenseLevel: nullToUndefined(this.formData.driverLicenseLevel),
      driverRegistryNo: nullToUndefined(this.formData.driverRegistryNo?.trim()),
      driverRegistryEffectDate: nullToUndefined(formatDate(this.formData.driverRegistryEffectDate)),
      note: nullToUndefined(this.formData.note?.trim())
    };
    (input as any).driverPhone = nullToUndefined(this.formData.driverPhone?.trim());
    (input as any).driverLicenseExpireDate = nullToUndefined(formatDate(this.formData.driverLicenseExpireDate));
    (input as any).driverRegistryExpireDate = nullToUndefined(formatDate(this.formData.driverRegistryExpireDate));
    return input;
  }
}
