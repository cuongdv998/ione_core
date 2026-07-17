import { Component, HostListener, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { DatePickerModule } from 'primeng/datepicker';
import { TableLazyLoadEvent } from 'primeng/table';
import { ConfirmationService, MenuItem, MessageService } from 'primeng/api';
import { Router } from '@angular/router';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TooltipModule } from 'primeng/tooltip';
import { MenuModule } from 'primeng/menu';
import { AutoCompleteCompleteEvent, AutoCompleteModule } from 'primeng/autocomplete';

import { VTable } from '@/shared/components/v-table/v-table';
import { TableColumn } from '@/shared/models/table-column.model';
import { TableAction } from '@/shared/models/table-action.model';
import { ClaimSearchForm } from './claim-list.models';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';
import { ProLineOfBusinessService } from '@/proxy/product/controllers/pro-line-of-business.service';
import { ResPartnerService } from '@/proxy/partner/controllers/res-partner.service';
import { HrDepartmentService } from '@/proxy/hr/controllers/hr-department.service';
import { HrEmployeeService } from '@/proxy/hr/controllers/hr-employee.service';
import { ClaimStatus } from '@/proxy/claims/claim-status.enum';
import { ProcessClaimType } from '@/proxy/claims/process-claim-type.enum';
import { ClaimService } from '@/proxy/claim/controllers/claim.service';
import { ClaimDto, GetClaimsInput } from '@/proxy/claim/claims/models';
import { ReportTemplateService } from '@/proxy/report/controllers/report-template.service';
import { CancelClaimModalComponent } from '../claim-detail/cancel-claim-modal/cancel-claim-modal.component';
import { ConfigStateService } from '@abp/ng.core';

const CLAIM_REQUEST_REPORT_CODE = 'CLAIM_REQUEST_REPORT';

@Component({
  selector: 'app-claim-list',
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
    ConfirmDialogModule,
    TooltipModule,
    MenuModule,
    AutoCompleteModule,
    VTable,
    TranslatePipe,
    CancelClaimModalComponent
  ],
  templateUrl: './claim-list.component.html',
  styleUrl: './claim-list.component.scss',
  providers: [MessageService, ConfirmationService]
})
export class ClaimListComponent implements OnInit {
  // Data
  claims: ClaimDto[] = [];
  totalCount = 0;
  loading = true;
  pageSize = 10;

  // Search form
  searchForm: ClaimSearchForm = {
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

  // Dropdown options
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
  /**
   * Listen for Enter key events globally to trigger search
   */
  @HostListener('window:keydown.enter', ['$event'])
  onWindowKeyDown(event: KeyboardEvent): void {
    // Both confirmation dialog and toast use their own logic or are not blocking for search in this component's simple view
    // There are no local dialogVisible flags in this component
    this.search();
  }


  // Table
  columns: TableColumn[] = [];
  actions: TableAction<any>[] = [];
  currentLazyLoadEvent?: TableLazyLoadEvent;

  loadingExport = false;

  cancelModalVisible = false;
  cancelClaimId: string | null = null;
  currentEmployeeId: string | null = null;
  currentUserId: string | null = null;

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

  constructor(
    private claimService: ClaimService,
    private reportTemplateService: ReportTemplateService,
    private lobService: ProLineOfBusinessService,
    private partnerService: ResPartnerService,
    private departmentService: HrDepartmentService,
    private employeeService: HrEmployeeService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private localizationService: LocalizationService,
    private router: Router,
    private configState: ConfigStateService
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
    this.loadCurrentEmployee();
  }

  private loadCurrentEmployee(): void {
    const user = this.configState.getOne('currentUser');
    this.currentUserId = (user?.id ?? '').toString().toLowerCase() || null;

    this.employeeService.getCurrent().subscribe({
      next: (emp) => { this.currentEmployeeId = emp?.id ?? null; },
      error: (err) => {
        console.warn('[ClaimList] getCurrent() failed — using creatorId fallback:', err?.status ?? err);
      }
    });
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
      {
        field: 'code',
        header: this.localizationService.localize('Claim::Claim:Code'),
        sortable: true,
        width: '120px'
      },
      {
        field: 'insurerCode',
        header: this.localizationService.localize('Claim::Claim:InsurerCode'),
        sortable: false,
        width: '150px'
      },
      {
        field: 'lobName',
        header: this.localizationService.localize('Claim::Claim:LobName'),
        sortable: false,
        width: '150px'
      },
      {
        field: 'notifierName',
        header: this.localizationService.localize('Claim::Claim:NotifierName'),
        sortable: false,
        width: '200px'
      },
      {
        field: 'notifyDate',
        header: this.localizationService.localize('Claim::Claim:NotifyDate'),
        sortable: true,
        type: 'date',
        width: '180px',
        formatter: (value: Date | string | undefined | null) => this.formatDateTime(value)
      },
      {
        field: 'carPlate',
        header: this.localizationService.localize('Claim::Claim:CarPlate'),
        sortable: false,
        width: '120px'
      },
      {
        field: 'openEmployeeName',
        header: this.localizationService.localize('Claim::Claim:OpenEmployeeName'),
        sortable: false,
        width: '150px'
      },
      {
        field: 'processClaimType',
        header: this.localizationService.localize('Claim::Claim:ProcessClaimType'),
        sortable: false,
        width: '120px',
        formatter: (value: ProcessClaimType) => this.formatProcessClaimType(value)
      },
      {
        field: 'status',
        header: this.localizationService.localize('Claim::Claim:Status'),
        sortable: true,
        type: 'text',
        width: '120px',
        freeze: 'right',
        align: 'center',
        formatter: (value: ClaimStatus) => this.formatStatus(value),
        cellClass: (value: ClaimStatus) => this.getStatusClass(value)
      }
    ];
  }

  private initializeActions(): void {
    this.actions.push({
      label: this.localizationService.localize('Claim::Action:ViewDetail'),
      icon: 'pi pi-ellipsis-v',
      command: (row) => this.viewDetails(row)
    });

    this.actions.push({
      label: this.localizationService.localize('Claim::Update'),
      icon: 'pi pi-pencil',
      visible: (row) => row?.status === ClaimStatus.Draft,
      command: (row) => this.editClaim(row)
    });

    this.actions.push({
      label: this.localizationService.localize('Claim::CopyLink'),
      icon: 'pi pi-link',
      command: (row) => this.copySnapshotLink(row)
    });

    this.actions.push({
      label: this.localizationService.localize('Claim::CancelClaim'),
      icon: 'pi pi-times',
      visible: (row) => {
        const canByStatus = row?.status === ClaimStatus.Draft || row?.status === ClaimStatus.PendingReceive;
        if (!canByStatus) return false;
        // Primary: so sánh openEmployeeId với HR employee hiện tại (getCurrent)
        if (this.currentEmployeeId) {
          return row?.openEmployeeId === this.currentEmployeeId;
        }
        // Fallback: so sánh creatorId (FullAuditedEntityDto) với ABP currentUser.id
        if (this.currentUserId) {
          return (row?.creatorId ?? '').toString().toLowerCase() === this.currentUserId;
        }
        return false;
      },
      command: (row) => this.cancelClaim(row)
    });
  }

  private initializeOptions(): void {
    // ProcessClaimType options
    this.processClaimTypeOptions = [
      { label: this.localizationService.localize('Claim::ProcessClaimType:Own'), value: ProcessClaimType.Own },
      { label: this.localizationService.localize('Claim::ProcessClaimType:Insurer'), value: ProcessClaimType.Insurer }
    ];

    // Status options
    this.statusOptions = [
      { label: this.localizationService.localize('Claim::ClaimStatus:Draft'), value: ClaimStatus.Draft },
      { label: this.localizationService.localize('Claim::ClaimStatus:PendingReceive'), value: ClaimStatus.PendingReceive },
      { label: this.localizationService.localize('Claim::ClaimStatus:InProgress'), value: ClaimStatus.InProgress },
      { label: this.localizationService.localize('Claim::ClaimStatus:Closed'), value: ClaimStatus.Closed },
      { label: this.localizationService.localize('Claim::ClaimStatus:Cancelled'), value: ClaimStatus.Called }
    ];
  }

  private loadLobs(): void {
    this.loadingLobs = true;
    this.lobService.getSelectList().subscribe({
      next: (items) => {
        this.lobOptions = (items || []).map(lob => ({
          label: lob.name || '',
          value: lob.id || ''
        }));
        this.loadingLobs = false;
      },
      error: () => {
        this.loadingLobs = false;
      }
    });
  }

  private loadInsurers(): void {
    this.loadingInsurers = true;
    this.partnerService.getSelectList('INSURER').subscribe({
      next: (items) => {
        this.insurerOptions = (items || []).map(insurer => ({
          label: insurer.name || '',
          value: insurer.id || ''
        }));
        this.loadingInsurers = false;
      },
      error: () => {
        this.loadingInsurers = false;
      }
    });
  }

  private loadDepartments(): void {
    this.loadingDepartments = true;
    this.departmentService.getSelectList().subscribe({
      next: (items) => {
        this.departmentOptions = (items || []).map(dept => ({
          label: dept.name || '',
          value: dept.id || ''
        }));
        this.loadingDepartments = false;
      },
      error: () => {
        this.loadingDepartments = false;
      }
    });
  }

  private loadEmployees(): void {
    this.loadingEmployees = true;
    this.employeeService.getSelectList().subscribe({
      next: (items) => {
        this.employeeOptions = (items || []).map(emp => ({
          label: emp.fullName || '',
          value: emp.id || ''
        }));
        this.loadingEmployees = false;
      },
      error: () => {
        this.loadingEmployees = false;
      }
    });
  }

  loadData(event: TableLazyLoadEvent): void {
    this.currentLazyLoadEvent = event;
    this.loading = true;

    const input: GetClaimsInput = {
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

    this.claimService.getList(input).subscribe({
      next: (result) => {
        this.claims = result.items || [];
        this.totalCount = result.totalCount || 0;
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

  private getSortingString(event: TableLazyLoadEvent): string | undefined {
    if (!event.sortField) {
      return undefined;
    }
    const sortOrder = event.sortOrder === 1 ? 'asc' : 'desc';
    const sortField = Array.isArray(event.sortField) ? event.sortField[0] : event.sortField;
    const fieldMap: { [key: string]: string } = {
      'code': 'code',
      'opendate': 'openDate',
      'notifydate': 'notifyDate',
      'status': 'status'
    };
    const mappedField = fieldMap[sortField.toLowerCase()] || sortField;
    return `${mappedField} ${sortOrder}`;
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

  formatStatus(status: ClaimStatus): string {
    const statusMap: { [key: number]: string } = {
      [ClaimStatus.Draft]: this.localizationService.localize('Claim::ClaimStatus:Draft'),
      [ClaimStatus.PendingReceive]: this.localizationService.localize('Claim::ClaimStatus:PendingReceive'),
      [ClaimStatus.InProgress]: this.localizationService.localize('Claim::ClaimStatus:InProgress'),
      [ClaimStatus.Closed]: this.localizationService.localize('Claim::ClaimStatus:Closed'),
      [ClaimStatus.Called]: this.localizationService.localize('Claim::ClaimStatus:Cancelled')
    };
    return statusMap[status] || '';
  }

  getStatusClass(status: ClaimStatus): string {
    const classMap: { [key: number]: string } = {
      [ClaimStatus.Draft]: 'px-2 py-1 rounded text-xs font-semibold bg-gray-100 text-gray-800',
      [ClaimStatus.PendingReceive]: 'px-2 py-1 rounded text-xs font-semibold bg-orange-100 text-orange-800',
      [ClaimStatus.InProgress]: 'px-2 py-1 rounded text-xs font-semibold bg-green-100 text-green-800',
      [ClaimStatus.Closed]: 'px-2 py-1 rounded text-xs font-semibold bg-blue-100 text-blue-800',
      [ClaimStatus.Called]: 'px-2 py-1 rounded text-xs font-semibold bg-red-100 text-red-800'
    };
    return classMap[status] || '';
  }

  formatProcessClaimType(type: ProcessClaimType): string {
    return type === ProcessClaimType.Own
      ? this.localizationService.localize('Claim::ProcessClaimType:Own')
      : this.localizationService.localize('Claim::ProcessClaimType:Insurer');
  }

  formatDate(date: Date): string {
    if (!date) return '';
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  formatDateTime(value: Date | string | undefined | null): string {
    if (!value) return '';
    const d = value instanceof Date ? value : new Date(value);
    if (isNaN(d.getTime())) return String(value);

    const day = String(d.getDate()).padStart(2, '0');
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const year = d.getFullYear();
    const hour = String(d.getHours()).padStart(2, '0');
    const minute = String(d.getMinutes()).padStart(2, '0');

    return `${day}/${month}/${year} ${hour}:${minute}`;
  }

  viewDetails(row: any): void {
    if (!row || !row.id) {
      return;
    }
    this.router.navigate(['/pages/claim/detail', row.id]);
  }

  editClaim(row: any): void {
    if (!row || !row.id) {
      return;
    }
    this.router.navigate(['/pages/claim/edit', row.id]);
  }

  copySnapshotLink(row: any): void {
    if (!row || !row.id) {
      return;
    }

    this.claimService.getSnapshotLink(row.id).subscribe({
      next: (link: string) => {
        console.log('snapshot link from API:', link);
        if (link) {
          const copy = () => {
            if (navigator && navigator.clipboard && navigator.clipboard.writeText) {
              return navigator.clipboard.writeText(link);
            }
            console.log('link', link);
            // Fallback cho trình duyệt không hỗ trợ Clipboard API
            const textarea = document.createElement('textarea');
            textarea.value = link;
            textarea.style.position = 'fixed';
            textarea.style.opacity = '0';
            document.body.appendChild(textarea);
            textarea.focus();
            textarea.select();
            try {
              document.execCommand('copy');
              document.body.removeChild(textarea);
              return Promise.resolve();
            } catch {
              document.body.removeChild(textarea);
              return Promise.reject();
            }
          };

          copy().then(() => {
            this.messageService.add({
              severity: 'success',
              summary: this.localizationService.localize('Claim::Success'),
              detail: this.localizationService.localize('Claim::SnapshotLinkCopied')
            });
          }).catch(() => {
            this.messageService.add({
              severity: 'error',
              summary: this.localizationService.localize('Claim::Error'),
              detail: this.localizationService.localize('Claim::InternalServerErrorMessage')
            });
          });
        }
      },
      error: () => {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Claim::Error'),
          detail: this.localizationService.localize('Claim::InternalServerErrorMessage')
        });
      }
    });
  }

  cancelClaim(row: any): void {
    if (!row || !row.id) {
      return;
    }
    this.cancelClaimId = row.id;
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

  addClaim(): void {
    this.router.navigate(['/pages/claim/create']);
  }

  exportFile(): void {
    if (this.loadingExport) return;
    this.loadingExport = true;
    this.buildSearchActionMenuItems();

    let carPlate = this.searchForm.carPlate ?? '';
    let vin = this.searchForm.vin ?? '';
    let engineNumber = this.searchForm.engineNumber ?? '';
    let code = this.searchForm.code ?? '';
    const keywordRaw = this.quickSearchKeyword?.trim() || '';
    if (keywordRaw && this.quickSearchSelectedType) {
      carPlate = '';
      vin = '';
      engineNumber = '';
      code = '';
      if (this.quickSearchSelectedType === 'carPlate') carPlate = keywordRaw;
      else if (this.quickSearchSelectedType === 'vin') vin = keywordRaw;
      else if (this.quickSearchSelectedType === 'engineNumber') engineNumber = keywordRaw;
      else if (this.quickSearchSelectedType === 'code') code = keywordRaw;
    }

    const params: Record<string, string> = {
      car_plate: carPlate,
      code,
      engine_number: engineNumber,
      insurer_id: this.searchForm.insurerId ?? '',
      lob_id: this.searchForm.lobId ?? '',
      notifier_phone: this.searchForm.notifierPhone ?? '',
      open_date_from: this.searchForm.openDateFrom ? this.formatDate(this.searchForm.openDateFrom) : '',
      open_date_to: this.searchForm.openDateTo ? this.formatDate(this.searchForm.openDateTo) : '',
      open_employee_id: this.searchForm.openEmployeeId ?? '',
      process_claim_type: this.searchForm.processClaimType != null ? String(this.searchForm.processClaimType) : '',
      process_dept_id: this.searchForm.processDeptId ?? '',
      status: this.searchForm.status != null ? String(this.searchForm.status) : '',
      vin
    };

    this.reportTemplateService.genDynamicFileByCode(CLAIM_REQUEST_REPORT_CODE, params as unknown as Record<string, object>).subscribe({
      next: (data: Blob) => {
        const a = document.createElement('a');
        const objectUrl = URL.createObjectURL(data);
        a.href = objectUrl;
        const dateStr = new Date().toISOString().slice(0, 10);
        a.download = `claim_request_report_${dateStr}.xlsx`;
        a.click();
        URL.revokeObjectURL(objectUrl);
        this.loadingExport = false;
        this.buildSearchActionMenuItems();
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Claim::Success'),
          detail: this.localizationService.localize('Claim::ExportSuccess')
        });
      },
      error: () => {
        this.loadingExport = false;
        this.buildSearchActionMenuItems();
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Claim::Error'),
          detail: this.localizationService.localize('Claim::InternalServerErrorMessage')
        });
      }
    });
  }
}
