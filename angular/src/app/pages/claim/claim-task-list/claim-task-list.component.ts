import { Component, HostListener, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { DatePickerModule } from 'primeng/datepicker';
import { TableLazyLoadEvent } from 'primeng/table';
import { MenuItem, MessageService } from 'primeng/api';
import { Router } from '@angular/router';
import { ToastModule } from 'primeng/toast';
import { TooltipModule } from 'primeng/tooltip';
import { MenuModule } from 'primeng/menu';
import { AutoCompleteCompleteEvent, AutoCompleteModule } from 'primeng/autocomplete';

import { VTable } from '@/shared/components/v-table/v-table';
import { TableColumn } from '@/shared/models/table-column.model';
import { TableAction } from '@/shared/models/table-action.model';
import { ClaimTaskSearchForm } from './claim-task-list.models';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';
import { ProLineOfBusinessService } from '@/proxy/product/controllers/pro-line-of-business.service';
import { ResPartnerService } from '@/proxy/partner/controllers/res-partner.service';
import { HrDepartmentService } from '@/proxy/hr/controllers/hr-department.service';
import { HrEmployeeService } from '@/proxy/hr/controllers/hr-employee.service';
import { ProcessClaimType } from '@/proxy/claims/process-claim-type.enum';
import { ClaimStatus } from '@/proxy/claims/claim-status.enum';
import { WorkTaskStatus } from '@/proxy/claims/work-task-status.enum';
import { ClaimTaskService } from '@/proxy/claim/controllers/claim-task.service';
import { ClaimTaskDto, GetClaimTasksInput } from '@/proxy/claim/claims/claim-task-models';
import type { ClaimPolicyDto } from '@/proxy/claim/claims/models';
import { PolicyService } from '@/proxy/policy/controllers/policy.service';
import type { PolicyClaimLookupDto } from '@/proxy/policy/policies/models';
import { AssignOnsiteAssessmentModalComponent } from '@/pages/claim/claim-task-detail/assign-onsite-assessment-modal/assign-onsite-assessment-modal.component';
import { OpenClaimFolderModalComponent } from '@/pages/claim/claim-task-detail/open-claim-folder-modal/open-claim-folder-modal.component';
import { CancelClaimModalComponent } from '@/pages/claim/claim-detail/cancel-claim-modal/cancel-claim-modal.component';

@Component({
  selector: 'app-claim-task-list',
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
    TooltipModule,
    MenuModule,
    AutoCompleteModule,
    VTable,
    TranslatePipe,
    AssignOnsiteAssessmentModalComponent,
    OpenClaimFolderModalComponent,
    CancelClaimModalComponent
  ],
  templateUrl: './claim-task-list.component.html',
  styleUrl: './claim-task-list.component.scss',
  providers: [MessageService]
})
export class ClaimTaskListComponent implements OnInit {
  tasks: ClaimTaskDto[] = [];
  totalCount = 0;
  loading = true;
  pageSize = 10;

  searchForm: ClaimTaskSearchForm = {
    lobId: null,
    insurerId: null,
    processClaimType: null,
    processDeptId: null,
    notifierPhone: null,
    openEmployeeId: null,
    openDateFrom: null,
    openDateTo: null,
    status: null,
    carPlate: null,
    vin: null,
    engineNumber: null,
    code: null
  };

  onVehicleSearchChange(field: 'carPlate' | 'vin' | 'engineNumber', value: string | null | undefined): void {
    const raw = (value ?? '').toString();
    const normalized = raw.toUpperCase().replace(/[^A-Z0-9]/g, '');
    this.searchForm[field] = normalized;
  }

  lobOptions: Array<{ label: string; value: string }> = [];
  loadingLobs = false;
  insurerOptions: Array<{ label: string; value: string }> = [];
  loadingInsurers = false;
  processClaimTypeOptions: Array<{ label: string; value: ProcessClaimType }> = [];
  departmentOptions: Array<{ label: string; value: string }> = [];
  loadingDepartments = false;
  employeeOptions: Array<{ label: string; value: string }> = [];
  loadingEmployees = false;
  statusOptions: Array<{ label: string; value: ClaimStatus }> = [];

  columns: TableColumn[] = [];
  actions: TableAction<ClaimTaskDto>[] = [];
  currentLazyLoadEvent?: TableLazyLoadEvent;

  assignModalVisible = false;
  createFolderModalVisible = false;
  cancelModalVisible = false;
  cancelClaimId: string | null = null;
  selectedClaimId: string | null = null;
  selectedClaimOnLocation: string | null = null;
  selectedWorkTaskStatus: WorkTaskStatus | null = null;
  selectedInsurerId: string | null = null;
  selectedClaimIncidentDate: string | null = null;
  policies: ClaimPolicyDto[] = [];
  policiesLoading = false;
  hasActiveOnsiteAssessmentTask = false;
  readonly WorkTaskStatus = WorkTaskStatus;

  showAdvancedSearch = false;
  quickSearchKeyword: string | null = null;
  quickSearchSelectedType: 'carPlate' | 'vin' | 'engineNumber' | 'code' | null = null;
  quickSearchSuggestions: Array<{
    type: 'carPlate' | 'vin' | 'engineNumber' | 'code';
    prefix: string;
    keyword: string;
    display: string;
  }> = [];
  searchActionMenuItems: MenuItem[] = [];
  loadingExport = false;

  constructor(
    private claimTaskService: ClaimTaskService,
    private lobService: ProLineOfBusinessService,
    private partnerService: ResPartnerService,
    private departmentService: HrDepartmentService,
    private employeeService: HrEmployeeService,
    private messageService: MessageService,
    private localizationService: LocalizationService,
    private router: Router,
    private policyService: PolicyService
  ) {
    this.initializeColumns();
    this.initializeActions();
    this.initializeOptions();
  }

  ngOnInit(): void {
    this.loadLobs();
    this.loadInsurers();
    this.loadDepartments();
    this.loadEmployees();
    this.buildSearchActionMenuItems();
  }

  toggleAdvancedSearch(): void {
    const next = !this.showAdvancedSearch;
    this.showAdvancedSearch = next;
    if (next) {
      this.quickSearchKeyword = null;
      this.quickSearchSelectedType = null;
      this.quickSearchSuggestions = [];
    }
  }

  onQuickSearchKeywordChange(value: string | null): void {
    this.quickSearchKeyword = value;
  }

  onQuickSearchComplete(event: AutoCompleteCompleteEvent): void {
    const keyword = (event.query || '').trim();
    this.rebuildQuickSearchSuggestions(keyword);
  }

  onQuickSearchFocus(): void {
    const keyword = (this.quickSearchKeyword || '').trim();
    if (keyword) {
      this.rebuildQuickSearchSuggestions(keyword);
    }
  }

  private rebuildQuickSearchSuggestions(keyword: string): void {
    if (!keyword) {
      this.quickSearchSuggestions = [];
      return;
    }

    this.quickSearchSuggestions = [
      { type: 'carPlate', prefix: 'Biển số xe', keyword, display: `Biển số xe: ${keyword}` },
      { type: 'vin', prefix: 'Số khung', keyword, display: `Số khung: ${keyword}` },
      { type: 'engineNumber', prefix: 'Số máy', keyword, display: `Số máy: ${keyword}` },
      { type: 'code', prefix: 'Mã yêu cầu', keyword, display: `Mã yêu cầu: ${keyword}` },
    ];
  }

  onQuickSearchSuggestionSelect(event: { value: { type: 'carPlate' | 'vin' | 'engineNumber' | 'code'; keyword: string } }): void {
    const selected = event?.value;
    this.quickSearchSelectedType = selected?.type || null;
    this.quickSearchKeyword = selected?.keyword?.trim() || null;
  }

  get quickSearchPrefixLabel(): string {
    if (this.quickSearchSelectedType === 'carPlate') return 'Biển số xe';
    if (this.quickSearchSelectedType === 'vin') return 'Số khung';
    if (this.quickSearchSelectedType === 'engineNumber') return 'Số máy';
    if (this.quickSearchSelectedType === 'code') return 'Mã yêu cầu';
    return '';
  }

  toggleSearchActionMenu(menu: { toggle: (event: Event) => void }, event: Event): void {
    this.buildSearchActionMenuItems();
    menu.toggle(event);
  }

  private buildSearchActionMenuItems(): void {
    this.searchActionMenuItems = [
      {
        label: this.localizationService.localize('Claim::ExportFile'),
        icon: 'pi pi-download',
        disabled: this.loadingExport,
        command: () => this.exportFile(),
      },
    ];
  }

  private initializeColumns(): void {
    this.columns = [
      { field: 'code', header: this.localizationService.localize('Claim::Claim:Code'), sortable: true, width: '120px' },
      { field: 'insurerCode', header: this.localizationService.localize('Claim::Claim:InsurerCode'), sortable: false, width: '150px' },
      { field: 'lobName', header: this.localizationService.localize('Claim::Claim:LobName'), sortable: false, width: '150px' },
      { field: 'notifierName', header: this.localizationService.localize('Claim::Claim:NotifierName'), sortable: false, width: '200px' },
      { field: 'carPlate', header: this.localizationService.localize('Claim::Claim:CarPlate'), sortable: false, width: '120px' },
      {
        field: 'incidentDate',
        header: this.localizationService.localize('Claim::Claim:IncidentDate'),
        sortable: false,
        width: '160px',
        formatter: (value: string) => this.formatIncidentDate(value)
      },
      {
        field: 'onLocation',
        header: this.localizationService.localize('Claim::Claim:OnLocation'),
        sortable: false,
        width: '120px',
        formatter: (value: string) => value === 'Y' ? (this.localizationService.localize('Claim::Yes') || 'Có') : (this.localizationService.localize('Claim::No') || 'Không')
      },
      { field: 'openEmployeeName', header: this.localizationService.localize('Claim::Claim:OpenEmployeeName'), sortable: false, width: '150px' },
      {
        field: 'processClaimType',
        header: this.localizationService.localize('Claim::Claim:ProcessClaimType'),
        sortable: false,
        width: '120px',
        formatter: (v: ProcessClaimType) => this.formatProcessClaimType(v)
      },
      {
        field: 'claimStatus',
        header: this.localizationService.localize('Claim::Claim:Status'),
        sortable: true,
        width: '120px',
        freeze: 'right',
        align: 'center',
        formatter: (v: ClaimStatus) => this.formatClaimStatus(v),
        cellClass: (v: ClaimStatus) => this.getClaimStatusClass(v)
      }
    ];
  }

  private initializeActions(): void {
    this.actions.push({
      label: this.localizationService.localize('Claim::Action:ViewDetail') || 'Xem chi tiết',
      icon: 'pi pi-eye',
      command: (row) => this.viewDetails(row)
    });
    this.actions.push({
      label: this.localizationService.localize('Claim::Action:ProcessClaim') || 'Xử lý yêu cầu',
      icon: 'pi pi-check',
      visible: (row) => row?.workTaskStatus === WorkTaskStatus.New,
      command: (row) => this.processClaim(row)
    });
    this.actions.push({
      label: this.localizationService.localize('Claim::Action:AssignAssessment') || 'Phân GĐ hiện trường',
      icon: 'pi pi-map-marker',
      visible: (row) =>
        row?.workTaskStatus === WorkTaskStatus.InProgress
        && row?.onLocation === 'Y'
        && !row?.hasActiveOnsiteAssessmentTask
        && !row?.hasFinishedOnsiteAssessment,
      command: (row) => this.openAssessmentModal(row)
    });
    this.actions.push({
      label: this.localizationService.localize('Claim::Action:OpenClaimFile') || 'Mở HS bồi thường',
      icon: 'pi pi-folder-open',
      visible: (row) => row?.workTaskStatus === WorkTaskStatus.InProgress,
      command: (row) => this.openClaimFileModal(row)
    });
    this.actions.push({
      label: this.localizationService.localize('Claim::CancelClaim'),
      icon: 'pi pi-times',
      visible: (row) => !!row?.canCancel,
      command: (row) => this.cancelClaim(row)
    });
  }

  private initializeOptions(): void {
    this.processClaimTypeOptions = [
      { label: this.localizationService.localize('Claim::ProcessClaimType:Own'), value: ProcessClaimType.Own },
      { label: this.localizationService.localize('Claim::ProcessClaimType:Insurer'), value: ProcessClaimType.Insurer }
    ];
    this.statusOptions = [
      { label: this.localizationService.localize('Claim::ClaimStatus:Draft') || 'Nháp', value: ClaimStatus.Draft },
      { label: this.localizationService.localize('Claim::ClaimStatus:PendingReceive') || 'Chờ tiếp nhận', value: ClaimStatus.PendingReceive },
      { label: this.localizationService.localize('Claim::ClaimStatus:InProgress') || 'Đang xử lý', value: ClaimStatus.InProgress },
      { label: this.localizationService.localize('Claim::ClaimStatus:Closed') || 'Đã đóng', value: ClaimStatus.Closed },
      { label: this.localizationService.localize('Claim::ClaimStatus:Called') || 'Đã gọi', value: ClaimStatus.Called }
    ];
  }

  private loadLobs(): void {
    this.loadingLobs = true;
    this.lobService.getSelectList().subscribe({
      next: (items) => {
        this.lobOptions = (items || []).map(lob => ({ label: lob.name || '', value: lob.id || '' }));
        this.loadingLobs = false;
      },
      error: () => { this.loadingLobs = false; }
    });
  }

  private loadInsurers(): void {
    this.loadingInsurers = true;
    this.partnerService.getSelectList('INSURER').subscribe({
      next: (items) => {
        this.insurerOptions = (items || []).map(p => ({ label: p.name || '', value: p.id || '' }));
        this.loadingInsurers = false;
      },
      error: () => { this.loadingInsurers = false; }
    });
  }

  private loadDepartments(): void {
    this.loadingDepartments = true;
    this.departmentService.getSelectList().subscribe({
      next: (items) => {
        this.departmentOptions = (items || []).map(d => ({ label: d.name || '', value: d.id || '' }));
        this.loadingDepartments = false;
      },
      error: () => { this.loadingDepartments = false; }
    });
  }

  private loadEmployees(): void {
    this.loadingEmployees = true;
    this.employeeService.getSelectList().subscribe({
      next: (items) => {
        this.employeeOptions = (items || []).map(e => ({ label: e.fullName || '', value: e.id || '' }));
        this.loadingEmployees = false;
      },
      error: () => { this.loadingEmployees = false; }
    });
  }

  loadData(event: TableLazyLoadEvent): void {
    this.currentLazyLoadEvent = event;
    this.loading = true;
    const input: GetClaimTasksInput = {
      skipCount: event.first || 0,
      maxResultCount: event.rows || 10,
      sorting: this.getSortingString(event),
      lobId: this.searchForm.lobId || undefined,
      insurerId: this.searchForm.insurerId || undefined,
      processClaimType: this.searchForm.processClaimType ?? undefined,
      processDeptId: this.searchForm.processDeptId || undefined,
      notifierPhone: this.searchForm.notifierPhone || undefined,
      openEmployeeId: this.searchForm.openEmployeeId || undefined,
      openDateFrom: this.searchForm.openDateFrom ? this.formatDate(this.searchForm.openDateFrom) : undefined,
      openDateTo: this.searchForm.openDateTo ? this.formatDate(this.searchForm.openDateTo) : undefined,
      status: this.searchForm.status ?? undefined,
      carPlate: this.searchForm.carPlate || undefined,
      vin: this.searchForm.vin || undefined,
      engineNumber: this.searchForm.engineNumber || undefined,
      code: this.searchForm.code || undefined
    };

    const keywordRaw = this.quickSearchKeyword?.trim() || '';
    if (keywordRaw && this.quickSearchSelectedType) {
      input.carPlate = undefined;
      input.vin = undefined;
      input.engineNumber = undefined;
      input.code = undefined;

      if (this.quickSearchSelectedType === 'carPlate') {
        input.carPlate = keywordRaw;
      } else if (this.quickSearchSelectedType === 'vin') {
        input.vin = keywordRaw;
      } else if (this.quickSearchSelectedType === 'engineNumber') {
        input.engineNumber = keywordRaw;
      } else if (this.quickSearchSelectedType === 'code') {
        input.code = keywordRaw;
      }
    }

    this.claimTaskService.getList(input).subscribe({
      next: (result) => {
        this.tasks = result.items || [];
        this.totalCount = result.totalCount || 0;
        this.loading = false;
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: this.localizationService.localize('Claim::Error'), detail: this.localizationService.localize('Claim::InternalServerErrorMessage') });
        this.loading = false;
      }
    });
  }

  private getSortingString(event: TableLazyLoadEvent): string | undefined {
    if (!event.sortField) return undefined;
    const sortOrder = event.sortOrder === 1 ? 'asc' : 'desc';
    const sortField = Array.isArray(event.sortField) ? event.sortField[0] : event.sortField;
    const fieldMap: { [key: string]: string } = { code: 'code', opendate: 'openDate', notifydate: 'notifyDate', claimstatus: 'claimStatus' };
    const mapped = fieldMap[String(sortField).toLowerCase()] || sortField;
    return `${mapped} ${sortOrder}`;
  }

  @HostListener('window:keydown.enter', ['$event'])
  handleEnter(event: KeyboardEvent) {
    if (!this.assignModalVisible && !this.createFolderModalVisible && !this.cancelModalVisible) {
      this.search();
    }
  }

  search(): void {
    const keywordRaw = this.quickSearchKeyword?.trim() || '';
    if (keywordRaw && !this.quickSearchSelectedType) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Chưa chọn loại tìm kiếm',
        detail: 'Vui lòng chọn gợi ý tìm kiếm (Biển số / Số khung / Số máy / Mã yêu cầu).',
      });
      return;
    }

    if (this.currentLazyLoadEvent) {
      this.currentLazyLoadEvent.first = 0;
      this.loadData(this.currentLazyLoadEvent);
    }
  }

  resetSearch(): void {
    this.quickSearchKeyword = null;
    this.quickSearchSelectedType = null;
    this.quickSearchSuggestions = [];
    this.searchForm = {
      lobId: null,
      insurerId: null,
      processClaimType: null,
      processDeptId: null,
      notifierPhone: null,
      openEmployeeId: null,
      openDateFrom: null,
      openDateTo: null,
      status: null,
      carPlate: null,
      vin: null,
      engineNumber: null,
      code: null
    };
    this.search();
  }

  formatDate(date: Date): string {
    if (!date) return '';
    const y = date.getFullYear();
    const m = String(date.getMonth() + 1).padStart(2, '0');
    const d = String(date.getDate()).padStart(2, '0');
    return `${y}-${m}-${d}`;
  }

  formatIncidentDate(value: string | undefined): string {
    if (!value) return '';
    const d = new Date(value);
    if (isNaN(d.getTime())) return value;
    const day = String(d.getDate()).padStart(2, '0');
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const year = d.getFullYear();
    const h = String(d.getHours()).padStart(2, '0');
    const min = String(d.getMinutes()).padStart(2, '0');
    const s = String(d.getSeconds()).padStart(2, '0');
    return `${day}/${month}/${year} ${h}:${min}:${s}`;
  }

  formatProcessClaimType(type: ProcessClaimType): string {
    return type === ProcessClaimType.Own
      ? this.localizationService.localize('Claim::ProcessClaimType:Own')
      : this.localizationService.localize('Claim::ProcessClaimType:Insurer');
  }

  formatClaimStatus(status: ClaimStatus): string {
    const map: { [key: number]: string } = {
      [ClaimStatus.Draft]: this.localizationService.localize('Claim::ClaimStatus:Draft') || 'Nháp',
      [ClaimStatus.PendingReceive]: this.localizationService.localize('Claim::ClaimStatus:PendingReceive') || 'Chờ tiếp nhận',
      [ClaimStatus.InProgress]: this.localizationService.localize('Claim::ClaimStatus:InProgress') || 'Đang xử lý',
      [ClaimStatus.Closed]: this.localizationService.localize('Claim::ClaimStatus:Closed') || 'Đã đóng',
      [ClaimStatus.Called]: this.localizationService.localize('Claim::ClaimStatus:Called') || 'Đã gọi'
    };
    return map[status] ?? '';
  }

  getClaimStatusClass(status: ClaimStatus): string {
    const map: { [key: number]: string } = {
      [ClaimStatus.Draft]: 'px-2 py-1 rounded text-xs font-semibold bg-orange-100 text-orange-800',
      [ClaimStatus.PendingReceive]: 'px-2 py-1 rounded text-xs font-semibold bg-yellow-100 text-yellow-800',
      [ClaimStatus.InProgress]: 'px-2 py-1 rounded text-xs font-semibold bg-green-100 text-green-800',
      [ClaimStatus.Closed]: 'px-2 py-1 rounded text-xs font-semibold bg-blue-100 text-blue-800',
      [ClaimStatus.Called]: 'px-2 py-1 rounded text-xs font-semibold bg-gray-100 text-gray-800'
    };
    return map[status] ?? '';
  }

  viewDetails(row: ClaimTaskDto): void {
    if (row?.claimId && row?.id) this.router.navigate(['/pages/claim/task-detail', row.claimId, row.id]);
  }

  processClaim(row: ClaimTaskDto): void {
    if (row?.claimId && row?.id) this.router.navigate(['/pages/claim/task-detail', row.claimId, row.id]);
  }

  openAssessmentModal(row: ClaimTaskDto): void {
    if (!row?.id || !row?.claimId) return;
    if (row.onLocation !== 'Y') return; // chỉ cho phép khi xe đang ở hiện trường
    if (row.workTaskStatus !== WorkTaskStatus.InProgress) return;
    this.hasActiveOnsiteAssessmentTask = !!row.hasActiveOnsiteAssessmentTask;
    const hasFinished = !!row.hasFinishedOnsiteAssessment;
    if (this.hasActiveOnsiteAssessmentTask || hasFinished) {
      const detailKey = hasFinished
        ? 'Claim::ClaimTask:OnsiteAssessmentLatestRoundFinished'
        : 'Claim::ClaimTask:OnsiteAssessmentAlreadyProcessed';
      this.messageService.add({
        severity: 'warn',
        summary: this.localizationService.localize('Claim::Error'),
        detail: this.localizationService.localize(detailKey)
      });
      return;
    }

    this.selectedClaimId = row.claimId ?? null;
    this.selectedClaimOnLocation = row.onLocation ?? null;
    this.selectedWorkTaskStatus = row.workTaskStatus ?? null;
    this.assignModalVisible = true;
  }

  openClaimFileModal(row: ClaimTaskDto): void {
    this.selectedClaimId = row?.claimId ?? null;
    this.selectedClaimIncidentDate = row?.incidentDate ?? null;

    const carPlate = row.carPlate?.trim() || undefined;
    if (!carPlate) {
      this.policies = [];
      this.selectedInsurerId = null;
      this.selectedClaimIncidentDate = null;
      this.createFolderModalVisible = true;
      return;
    }

    this.policiesLoading = true;
    this.policyService
      .getClaimLookup({
        certificateNo: undefined,
        carPlate,
        vin: undefined,
        engineNumber: undefined,
        incidentDate: undefined,
        maxResultCount: 20
      })
      .subscribe({
        next: (items) => {
          const list = (items || []).map((x) => this.mapPolicyLookupToClaimPolicy(x));
          this.policies = list;
          this.selectedInsurerId = list[0]?.insurerId ?? null;
          this.policiesLoading = false;
          this.createFolderModalVisible = true;
        },
        error: () => {
          this.policiesLoading = false;
          this.policies = [];
          this.selectedInsurerId = null;
          this.createFolderModalVisible = true;
          this.messageService.add({
            severity: 'warn',
            summary: this.localizationService.localize('Claim::Warning') || 'Cảnh báo',
            detail: this.localizationService.localize('Claim::LoadPoliciesError') || 'Không tải được danh sách đơn bảo hiểm, vẫn mở màn hình tạo hồ sơ.'
          });
        }
      });
  }

  onAssignModalVisibleChange(visible: boolean): void {
    this.assignModalVisible = visible;
    if (!visible) {
      this.selectedClaimId = null;
      this.selectedClaimOnLocation = null;
      this.selectedWorkTaskStatus = null;
      this.hasActiveOnsiteAssessmentTask = false;
      if (this.currentLazyLoadEvent) {
        this.loadData(this.currentLazyLoadEvent);
      }
    }
  }

  onOnsiteAssessmentAssigned(): void {
    if (this.currentLazyLoadEvent) {
      this.loadData(this.currentLazyLoadEvent);
    }
  }

  onCreateFolderModalVisibleChange(visible: boolean): void {
    this.createFolderModalVisible = visible;
    if (!visible) {
      this.selectedClaimIncidentDate = null;
      if (this.currentLazyLoadEvent) {
        this.loadData(this.currentLazyLoadEvent);
      }
    }
  }

  onFolderCreated(): void {
    if (this.currentLazyLoadEvent) {
      this.loadData(this.currentLazyLoadEvent);
    }
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
      certificateUrl: input.certificateUrl,
      insurerId: input.insurerId,
      insurerName: input.insurerName
    };
  }

  cancelClaim(row: ClaimTaskDto): void {
    if (!row?.claimId) return;
    this.cancelClaimId = row.claimId;
    this.cancelModalVisible = true;
  }

  onCancelModalClose(visible: boolean): void {
    this.cancelModalVisible = visible;
  }

  onCancelSuccess(): void {
    this.cancelModalVisible = false;
    if (this.currentLazyLoadEvent) {
      this.loadData(this.currentLazyLoadEvent);
    }
  }

  exportFile(): void {
    if (this.loadingExport) return;
    this.loadingExport = true;
    this.buildSearchActionMenuItems();

    const input: GetClaimTasksInput = {
      skipCount: 0,
      maxResultCount: 1000,
      lobId: this.searchForm.lobId || undefined,
      insurerId: this.searchForm.insurerId || undefined,
      processClaimType: this.searchForm.processClaimType ?? undefined,
      processDeptId: this.searchForm.processDeptId || undefined,
      notifierPhone: this.searchForm.notifierPhone || undefined,
      openEmployeeId: this.searchForm.openEmployeeId || undefined,
      openDateFrom: this.searchForm.openDateFrom ? this.formatDate(this.searchForm.openDateFrom) : undefined,
      openDateTo: this.searchForm.openDateTo ? this.formatDate(this.searchForm.openDateTo) : undefined,
      status: this.searchForm.status ?? undefined,
      carPlate: this.searchForm.carPlate || undefined,
      vin: this.searchForm.vin || undefined,
      engineNumber: this.searchForm.engineNumber || undefined,
      code: this.searchForm.code || undefined
    };

    const keywordRaw = this.quickSearchKeyword?.trim() || '';
    if (keywordRaw && this.quickSearchSelectedType) {
      input.carPlate = undefined;
      input.vin = undefined;
      input.engineNumber = undefined;
      input.code = undefined;

      if (this.quickSearchSelectedType === 'carPlate') {
        input.carPlate = keywordRaw;
      } else if (this.quickSearchSelectedType === 'vin') {
        input.vin = keywordRaw;
      } else if (this.quickSearchSelectedType === 'engineNumber') {
        input.engineNumber = keywordRaw;
      } else if (this.quickSearchSelectedType === 'code') {
        input.code = keywordRaw;
      }
    }

    this.claimTaskService.export(input).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `claim-tasks_${new Date().toISOString().slice(0, 10)}.xlsx`;
        a.click();
        URL.revokeObjectURL(url);
        this.loadingExport = false;
        this.buildSearchActionMenuItems();
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: this.localizationService.localize('Claim::Error'), detail: this.localizationService.localize('Claim::InternalServerErrorMessage') });
        this.loadingExport = false;
        this.buildSearchActionMenuItems();
      }
    });
  }

}
