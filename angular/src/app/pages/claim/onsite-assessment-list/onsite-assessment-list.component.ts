import { Component, HostListener, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ConfigStateService } from '@abp/ng.core';
import { TableLazyLoadEvent } from 'primeng/table';
import { ConfirmationService, MenuItem, MessageService } from 'primeng/api';
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { DatePickerModule } from 'primeng/datepicker';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { DialogModule } from 'primeng/dialog';
import { TooltipModule } from 'primeng/tooltip';
import { MenuModule } from 'primeng/menu';
import { AutoCompleteCompleteEvent, AutoCompleteModule } from 'primeng/autocomplete';

import { VTable } from '@/shared/components/v-table/v-table';
import { TableAction } from '@/shared/models/table-action.model';
import { TableColumn } from '@/shared/models/table-column.model';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';
import { ProLineOfBusinessService } from '@/proxy/product/controllers/pro-line-of-business.service';
import { ResPartnerService } from '@/proxy/partner/controllers/res-partner.service';
import { HrDepartmentService } from '@/proxy/hr/controllers/hr-department.service';
import { HrEmployeeService } from '@/proxy/hr/controllers/hr-employee.service';
import { ProcessClaimType } from '@/proxy/claims/process-claim-type.enum';
import { WorkTaskStatus } from '@/proxy/claims/work-task-status.enum';
import { normalizeVehiclePlate } from '@/core/utils/vehicle-plate.util';
import { OnsiteAssessmentTaskService } from './onsite-assessment-task.service';
import {
  GetOnsiteAssessmentTasksInput,
  OnsiteAssessmentCreateRequestDto,
  OnsiteAssessmentSearchForm,
  OnsiteAssessmentTaskDto
} from './onsite-assessment-list.models';
import { ReassignOnsiteAssessmentModalComponent } from './reassign-onsite-assessment-modal.component';

@Component({
  selector: 'app-onsite-assessment-list',
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
    DialogModule,
    TooltipModule,
    MenuModule,
    AutoCompleteModule,
    VTable,
    TranslatePipe,
    ReassignOnsiteAssessmentModalComponent,
  ],
  templateUrl: './onsite-assessment-list.component.html',
  styleUrl: './onsite-assessment-list.component.scss',
  providers: [MessageService, ConfirmationService],
})
export class OnsiteAssessmentListComponent implements OnInit {
  tasks: OnsiteAssessmentTaskDto[] = [];
  totalCount = 0;
  loading = true;
  pageSize = 10;

  searchForm: OnsiteAssessmentSearchForm = {
    lobId: null,
    insurerId: null,
    processClaimType: null,
    processDeptId: null,
    notifierPhone: null,
    openEmployeeId: null,
    openDateFrom: null,
    openDateTo: null,
    workTaskStatus: null,
    carPlate: null,
    vin: null,
    engineNumber: null,
    code: null,
  };

  lobOptions: Array<{ label: string; value: string }> = [];
  insurerOptions: Array<{ label: string; value: string }> = [];
  departmentOptions: Array<{ label: string; value: string }> = [];
  employeeOptions: Array<{ label: string; value: string }> = [];

  loadingLobs = false;
  loadingInsurers = false;
  loadingDepartments = false;
  loadingEmployees = false;

  processClaimTypeOptions: Array<{ label: string; value: ProcessClaimType }> = [];
  workTaskStatusOptions: Array<{ label: string; value: WorkTaskStatus }> = [];
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

  columns: TableColumn[] = [];
  actions: TableAction<OnsiteAssessmentTaskDto>[] = [];
  currentLazyLoadEvent?: TableLazyLoadEvent;
  createModalVisible = false;
  assignModalVisible = false;
  reassignModalVisible = false;
  reassignWorkTaskId: string | null = null;
  /**
   * Listen for Enter key events globally to trigger search
   */
  @HostListener('window:keydown.enter', ['$event'])
  onWindowKeyDown(event: KeyboardEvent): void {
    // Only trigger search if focus is not in a dialog
    if (!this.createModalVisible && !this.assignModalVisible) {
      this.search();
    }
  }

  creating = false;
  loadingCreateRequests = false;
  loadingAssignUnits = false;
  loadingAssignEmployees = false;
  createClaimOptions: Array<{ label: string; value: string }> = [];
  selectedCreateClaimId: string | null = null;
  assignUnitOptions: Array<{ label: string; value: string }> = [];
  assignEmployeeOptions: Array<{ label: string; value: string }> = [];
  assignForm: {
    assigneeOrganizationId: string | null;
    assigneeId: string | null;
    startDate: Date | null;
    endDate: Date | null;
  } = {
    assigneeOrganizationId: null,
    assigneeId: null,
    startDate: null,
    endDate: null,
  };

  constructor(
    private onsiteService: OnsiteAssessmentTaskService,
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
      { type: 'code', prefix: 'Mã hồ sơ', keyword, display: `Mã hồ sơ: ${keyword}` },
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
    if (this.quickSearchSelectedType === 'code') return 'Mã hồ sơ';
    return '';
  }

  toggleSearchActionMenu(menu: { toggle: (event: Event) => void }, event: Event): void {
    this.buildSearchActionMenuItems();
    menu.toggle(event);
  }

  private buildSearchActionMenuItems(): void {
    this.searchActionMenuItems = [
      {
        label: this.localizationService.localize('Claim::ExportFile') || 'Export',
        icon: 'pi pi-download',
        command: () => this.exportFile(),
      },
    ];
  }

  private initializeColumns(): void {
    this.columns = [
      { field: 'code', header: this.localizationService.localize('Claim::Claim:Code'), width: '140px', sortable: true },
      { field: 'insurerCode', header: this.localizationService.localize('Claim::Claim:InsurerCode'), width: '180px' },
      { field: 'lobName', header: this.localizationService.localize('Claim::Claim:LobName'), width: '160px' },
      { field: 'notifierName', header: this.localizationService.localize('Claim::Claim:NotifierName'), width: '180px' },
      {
        field: 'notifyDate',
        header: this.localizationService.localize('Claim::Claim:NotifyDate'),
        width: '190px',
        formatter: (value: string) => this.formatDateTime(value),
      },
      { field: 'carPlate', header: this.localizationService.localize('Claim::Claim:CarPlate'), width: '130px' },
      { field: 'reporterName', header: 'Người giao GĐHT', width: '180px' },
      { field: 'assigneeName', header: 'Người GĐHT', width: '180px' },
      {
        field: 'processClaimType',
        header: this.localizationService.localize('Claim::Claim:ProcessClaimType'),
        width: '150px',
        formatter: (value: ProcessClaimType) => this.formatProcessClaimType(value),
      },
      {
        field: 'workTaskStatus',
        header: this.localizationService.localize('Claim::Claim:Status'),
        width: '140px',
        freeze: 'right',
        align: 'center',
        formatter: (value: WorkTaskStatus) => this.formatWorkTaskStatus(value),
        cellClass: (value: WorkTaskStatus) => this.getStatusClass(value),
      },
    ];
  }

  private initializeActions(): void {
    this.actions = [
      {
        label: this.localizationService.localize('Claim::Action:ViewDetail') || 'Xem chi tiết',
        icon: 'pi pi-eye',
        visible: row => row.canView === true,
        command: row => this.openTaskDetail(row, 'view'),
      },
      {
        label: this.localizationService.localize('Claim::Action:Accept') || 'Tiếp nhận',
        icon: 'pi pi-check',
        visible: row => row.canAccept === true && row.isAssignee === true,
        command: row => this.openTaskDetail(row, 'process'),
      },
      {
        label: this.localizationService.localize('Claim::Action:ProcessOnsiteAssessment') || 'Xử lý GĐ',
        icon: 'pi pi-cog',
        visible: row => row.canProcess === true && row.isAssignee === true,
        command: row => this.openTaskDetail(row, 'process'),
      },
      {
        label: this.localizationService.localize('Claim::Action:CompleteOnsiteAssessment') || 'Hoàn thành GĐ',
        icon: 'pi pi-check-circle',
        visible: row => row.canComplete === true && row.isAssignee === true,
        command: row => this.openTaskDetail(row, 'process', 'complete'),
      },
      {
        label: this.localizationService.localize('Claim::Action:CancelOnsiteAssessment') || 'Hủy GĐ',
        icon: 'pi pi-times-circle',
        visible: row => row.canCancel === true && row.workTaskStatus !== WorkTaskStatus.Completed,
        command: row => this.confirmCancel(row),
      },
      {
        label: 'Giao lại',
        icon: 'pi pi-send',
        visible: row => this.canShowReassign(row),
        command: row => this.openReassignModal(row),
      },
    ];
  }

  private initializeOptions(): void {
    this.processClaimTypeOptions = [
      { label: this.localizationService.localize('Claim::ProcessClaimType:Own'), value: ProcessClaimType.Own },
      { label: this.localizationService.localize('Claim::ProcessClaimType:Insurer'), value: ProcessClaimType.Insurer },
    ];

    this.workTaskStatusOptions = [
      { label: this.formatWorkTaskStatus(WorkTaskStatus.New), value: WorkTaskStatus.New },
      { label: this.formatWorkTaskStatus(WorkTaskStatus.InProgress), value: WorkTaskStatus.InProgress },
      { label: this.formatWorkTaskStatus(WorkTaskStatus.Completed), value: WorkTaskStatus.Completed },
      { label: this.formatWorkTaskStatus(WorkTaskStatus.Accepted), value: WorkTaskStatus.Accepted },
      { label: this.formatWorkTaskStatus(WorkTaskStatus.Rejected), value: WorkTaskStatus.Rejected },
      { label: this.formatWorkTaskStatus(WorkTaskStatus.Cancelled), value: WorkTaskStatus.Cancelled },
      { label: this.formatWorkTaskStatus(WorkTaskStatus.WaitApprove), value: WorkTaskStatus.WaitApprove },
      { label: this.formatWorkTaskStatus(WorkTaskStatus.Approved), value: WorkTaskStatus.Approved },
      { label: this.formatWorkTaskStatus(WorkTaskStatus.Pending), value: WorkTaskStatus.Pending },
      { label: this.formatWorkTaskStatus(WorkTaskStatus.Return), value: WorkTaskStatus.Return },
      { label: this.formatWorkTaskStatus(WorkTaskStatus.Transfer), value: WorkTaskStatus.Transfer },
    ];
  }

  private loadLobs(): void {
    this.loadingLobs = true;
    this.lobService.getSelectList().subscribe({
      next: items => {
        this.lobOptions = (items || []).map(x => ({ label: x.name || '', value: x.id || '' }));
        this.loadingLobs = false;
      },
      error: () => {
        this.loadingLobs = false;
      },
    });
  }

  private loadInsurers(): void {
    this.loadingInsurers = true;
    this.partnerService.getSelectList('INSURER').subscribe({
      next: items => {
        this.insurerOptions = (items || []).map(x => ({ label: x.name || '', value: x.id || '' }));
        this.loadingInsurers = false;
      },
      error: () => {
        this.loadingInsurers = false;
      },
    });
  }

  private loadDepartments(): void {
    this.loadingDepartments = true;
    this.departmentService.getSelectList().subscribe({
      next: items => {
        this.departmentOptions = (items || []).map(x => ({ label: x.name || '', value: x.id || '' }));
        this.loadingDepartments = false;
      },
      error: () => {
        this.loadingDepartments = false;
      },
    });
  }

  private loadEmployees(): void {
    this.loadingEmployees = true;
    this.employeeService.getSelectList().subscribe({
      next: items => {
        this.employeeOptions = (items || []).map(x => ({ label: x.fullName || '', value: x.id || '' }));
        this.loadingEmployees = false;
      },
      error: () => {
        this.loadingEmployees = false;
      },
    });
  }

  loadData(event: TableLazyLoadEvent): void {
    this.currentLazyLoadEvent = event;
    this.loading = true;

    const input = this.buildSearchInput(event);

    this.onsiteService.getList(input).subscribe({
      next: result => {
        this.tasks = result.items || [];
        this.totalCount = result.totalCount || 0;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Claim::Error'),
          detail: this.localizationService.localize('Claim::InternalServerErrorMessage'),
        });
      },
    });
  }

  search(): void {
    const keywordRaw = this.quickSearchKeyword?.trim() || '';
    if (keywordRaw && !this.quickSearchSelectedType) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Chưa chọn loại tìm kiếm',
        detail: 'Vui lòng chọn gợi ý tìm kiếm (Biển số / Số khung / Số máy / Mã hồ sơ).',
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
      workTaskStatus: null,
      carPlate: null,
      vin: null,
      engineNumber: null,
      code: null,
    };
    this.search();
  }

  createOnsiteAssessment(): void {
    this.selectedCreateClaimId = null;
    this.createModalVisible = true;
    this.loadCreateRequestOptions();
  }

  focusCreateOnsiteExecuteButton(): void {
    [0, 50, 100, 150, 250, 400, 650, 900].forEach(delay => {
      setTimeout(() => {
        const button = this.getCreateOnsiteExecuteButton();
        if (button && !button.disabled && document.activeElement !== button) {
          button.focus();
        }
      }, delay);
    });
  }

  private getCreateOnsiteExecuteButton(): HTMLButtonElement | null {
    const target = document.querySelector<HTMLElement>(
      '.create-onsite-assessment-dialog .create-onsite-execute-button'
    );

    return target instanceof HTMLButtonElement
      ? target
      : target?.querySelector<HTMLButtonElement>('button') ?? null;
  }

  onCreateModalVisibleChange(visible: boolean): void {
    this.createModalVisible = visible;
  }

  cancelCreateOnsiteAssessment(): void {
    this.confirmationService.confirm({
      message: 'Bạn có muốn đóng cửa sổ hiện tại không?',
      header: 'Xác nhận',
      icon: 'pi pi-question-circle',
      acceptLabel: 'Có',
      rejectLabel: 'Không',
      rejectButtonProps: { severity: 'secondary', outlined: true },
      accept: () => {
        this.createModalVisible = false;
        this.assignModalVisible = false;
      }
    });
  }

  submitCreateOnsiteAssessment(): void {
    const claimId = this.selectedCreateClaimId;
    if (!claimId) {
      this.messageService.add({
        severity: 'warn',
        summary: this.localizationService.localize('AbpUi::ValidationError'),
        detail: 'Yêu cầu bồi thường là bắt buộc.'
      });
      return;
    }

    this.confirmationService.confirm({
      message: 'Bạn có muốn tạo yêu cầu GĐHT không?',
      header: 'Xác nhận',
      icon: 'pi pi-question-circle',
      acceptLabel: 'Có',
      rejectLabel: 'Không',
      accept: () => {
        this.createModalVisible = false;
        this.openAssignModal();
      }
    });
  }

  cancelAssignOnsiteAssessment(): void {
    this.confirmationService.confirm({
      message: 'Bạn có muốn đóng cửa sổ hiện tại không?',
      header: 'Xác nhận',
      icon: 'pi pi-question-circle',
      acceptLabel: 'Có',
      rejectLabel: 'Không',
      rejectButtonProps: { severity: 'secondary', outlined: true },
      accept: () => {
        this.assignModalVisible = false;
      }
    });
  }

  onAssignOrganizationChange(): void {
    this.assignForm.assigneeId = null;
    this.loadAssignEmployeesByOrganization(this.assignForm.assigneeOrganizationId, null);
  }

  submitAssignOnsiteAssessment(): void {
    const claimId = this.selectedCreateClaimId;
    if (!claimId) {
      this.messageService.add({
        severity: 'warn',
        summary: this.localizationService.localize('AbpUi::ValidationError'),
        detail: 'Yêu cầu bồi thường là bắt buộc.'
      });
      return;
    }

    if (!this.assignForm.assigneeOrganizationId) {
      this.messageService.add({ severity: 'warn', summary: 'Cảnh báo', detail: 'Đơn vị giám định là bắt buộc.' });
      return;
    }
    if (!this.assignForm.assigneeId) {
      this.messageService.add({ severity: 'warn', summary: 'Cảnh báo', detail: 'Người giám định là bắt buộc.' });
      return;
    }
    if (!this.assignForm.startDate) {
      this.messageService.add({ severity: 'warn', summary: 'Cảnh báo', detail: 'Ngày giám định là bắt buộc.' });
      return;
    }
    if (!this.assignForm.endDate) {
      this.messageService.add({ severity: 'warn', summary: 'Cảnh báo', detail: 'Ngày dự kiến hoàn thành là bắt buộc.' });
      return;
    }

    this.creating = true;
    this.onsiteService.create({
      claimId,
      assigneeOrganizationId: this.assignForm.assigneeOrganizationId,
      assigneeId: this.assignForm.assigneeId,
      startDate: this.assignForm.startDate.toISOString(),
      endDate: this.assignForm.endDate.toISOString()
    }).subscribe({
      next: (res) => {
        this.creating = false;
        this.assignModalVisible = false;
        this.selectedCreateClaimId = null;
        this.messageService.add({
          severity: 'success',
          summary: 'Thành công',
          detail: 'Đã phân giám định hiện trường.',
        });
        this.reloadLatestOnsiteAssessments();
      },
      error: (error) => {
        this.creating = false;
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Claim::Error') || 'Lỗi',
          detail:
            error?.error?.error?.message ||
            error?.error?.message ||
            this.localizationService.localize('Claim::InternalServerErrorMessage'),
        });
      }
    });
  }

  private reloadLatestOnsiteAssessments(): void {
    const event: TableLazyLoadEvent = {
      ...(this.currentLazyLoadEvent ?? {}),
      first: 0,
      rows: this.currentLazyLoadEvent?.rows ?? this.pageSize,
      sortField: 'creationTime',
      sortOrder: -1
    };

    this.loadData(event);
  }

  private loadCreateRequestOptions(): void {
    this.loadingCreateRequests = true;
    this.onsiteService.getCreateRequestList().subscribe({
      next: (items: any) => {
        const raw = Array.isArray(items)
          ? items
          : Array.isArray(items?.items)
            ? items.items
            : Array.isArray(items?.result)
              ? items.result
              : [];

        const values = raw as OnsiteAssessmentCreateRequestDto[];
        this.createClaimOptions = values.map((x: any) => {
          const claimId = x?.claimId ?? x?.ClaimId ?? '';
          const claimCode = x?.claimCode ?? x?.ClaimCode ?? '';
          const notifyDate = x?.notifyDate ?? x?.NotifyDate;
          const carPlate = x?.carPlate ?? x?.CarPlate ?? '';
          const notifierName = x?.notifierName ?? x?.NotifierName ?? '';

          const notify = notifyDate ? this.formatDateTime(notifyDate) : '';
          const plate = carPlate ? ` - ${carPlate}` : '';
          const name = notifierName ? ` - ${notifierName}` : '';
          const label = `${claimCode}${plate}${name}${notify ? ` (${notify})` : ''}`;
          return {
            label,
            value: claimId
          };
        }).filter(x => !!x.value);
        this.loadingCreateRequests = false;
        if (!this.createClaimOptions.length) {
          this.messageService.add({
            severity: 'info',
            summary: 'Thông báo',
            detail: 'Không có yêu cầu bồi thường phù hợp để tạo giám định hiện trường.',
          });
        } else if (this.createModalVisible) {
          this.focusCreateOnsiteExecuteButton();
        }
      },
      error: () => {
        this.loadingCreateRequests = false;
        this.createClaimOptions = [];
      }
    });
  }

  private openAssignModal(): void {
    const start = new Date();
    const end = new Date(start.getTime());
    end.setDate(end.getDate() + 1);

    this.assignForm = {
      assigneeOrganizationId: null,
      assigneeId: null,
      startDate: start,
      endDate: end
    };
    this.assignModalVisible = true;
    this.loadAssignUnits();
    this.detectAndSetCurrentEmployeeDefaults();
  }

  private loadAssignUnits(): void {
    this.loadingAssignUnits = true;
    this.departmentService.getList({
      skipCount: 0,
      maxResultCount: 500,
    }).subscribe({
      next: (res) => {
        this.assignUnitOptions = (res.items || []).map(x => ({ label: x.name || '', value: x.id || '' })).filter(x => !!x.value);
        this.loadingAssignUnits = false;
      },
      error: () => {
        this.loadingAssignUnits = false;
      }
    });
  }

  private detectAndSetCurrentEmployeeDefaults(): void {
    const currentUser = this.configState.getOne('currentUser') as any;
    const currentUserId = currentUser?.id as string | undefined;
    if (!currentUserId) {
      return;
    }

    this.employeeService.getList({
      skipCount: 0,
      maxResultCount: 1000
    }).subscribe({
      next: (res) => {
        const currentEmployee = (res.items || []).find(x => (x.userId || '').toLowerCase() === currentUserId.toLowerCase());
        if (!currentEmployee?.departmentId) {
          return;
        }

        this.assignForm.assigneeOrganizationId = currentEmployee.departmentId;
        this.loadAssignEmployeesByOrganization(currentEmployee.departmentId, currentEmployee.id || null);
      },
      error: () => {
        // no-op
      }
    });
  }

  private loadAssignEmployeesByOrganization(orgId: string | null, defaultEmployeeId: string | null): void {
    if (!orgId) {
      this.assignEmployeeOptions = [];
      return;
    }

    this.loadingAssignEmployees = true;
    this.employeeService.getList({
      departmentId: orgId,
      maxResultCount: 500
    }).subscribe({
      next: (res) => {
        this.assignEmployeeOptions = (res.items || []).map(x => ({ label: x.fullName || '', value: x.id || '' })).filter(x => !!x.value);
        this.loadingAssignEmployees = false;
        if (defaultEmployeeId && this.assignEmployeeOptions.some(x => x.value === defaultEmployeeId)) {
          this.assignForm.assigneeId = defaultEmployeeId;
        }
      },
      error: () => {
        this.loadingAssignEmployees = false;
        this.assignEmployeeOptions = [];
      }
    });
  }

  exportFile(): void {
    const input = this.buildSearchInput({
      ...this.currentLazyLoadEvent,
      first: 0,
      rows: 10000,
    });

    this.onsiteService.export(input).subscribe({
      next: blob => {
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `onsite-assessment-tasks_${new Date().toISOString().slice(0, 10)}.xlsx`;
        a.click();
        URL.revokeObjectURL(url);
      },
      error: () => {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Claim::Error'),
          detail: this.localizationService.localize('Claim::InternalServerErrorMessage'),
        });
      },
    });
  }

  private openTaskDetail(
    row: OnsiteAssessmentTaskDto,
    mode: 'view' | 'process' = 'view',
    action?: 'complete'
  ): void {
    if (!row.claimId || !row.id) {
      return;
    }

    this.router.navigate(
      ['/pages/claim/onsite-assessment-detail', row.claimId, row.id, mode],
      {
        queryParams: {
          tab: 2,
          ...(action ? { action } : {}),
        }
      }
    );
  }

  private confirmCancel(row: OnsiteAssessmentTaskDto): void {
    if (!row.id) {
      return;
    }

    this.confirmationService.confirm({
      message: 'Bạn có chắc chắn muốn hủy GĐHT hiện tại không?',
      header: 'Xác nhận',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Có',
      rejectLabel: 'Không',
      acceptButtonStyleClass: 'onsite-cancel-accept-button',
      accept: () => {
        this.onsiteService.cancel(row.id as string).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Thành công',
              detail: 'Đã hủy GĐ hiện trường.',
            });

            if (this.currentLazyLoadEvent) {
              this.loadData(this.currentLazyLoadEvent);
            }
          },
          error: () => {
            this.messageService.add({
              severity: 'error',
              summary: this.localizationService.localize('Claim::Error'),
              detail: this.localizationService.localize('Claim::InternalServerErrorMessage'),
            });
          },
        });
      },
    });
    this.focusOnsiteCancelAcceptButton();
  }

  private focusOnsiteCancelAcceptButton(): void {
    [0, 50, 100, 150, 250, 400, 650, 900].forEach(delay => {
      setTimeout(() => {
        const target = document.querySelector<HTMLElement>('.onsite-cancel-accept-button');
        const button = target instanceof HTMLButtonElement
          ? target
          : target?.querySelector<HTMLButtonElement>('button') ?? null;

        if (button && !button.disabled && document.activeElement !== button) {
          button.focus();
        }
      }, delay);
    });
  }

  private openReassignModal(row: OnsiteAssessmentTaskDto): void {
    if (!row.id) {
      return;
    }

    this.reassignWorkTaskId = row.id;
    this.reassignModalVisible = true;
  }

  onReassignModalVisibleChange(value: boolean): void {
    this.reassignModalVisible = value;
    if (!value) {
      this.reassignWorkTaskId = null;
    }
  }

  onReassigned(): void {
    this.reassignModalVisible = false;
    this.reassignWorkTaskId = null;
    if (this.currentLazyLoadEvent) {
      this.loadData(this.currentLazyLoadEvent);
    }
  }

  private formatDate(date: Date): string {
    const y = date.getFullYear();
    const m = String(date.getMonth() + 1).padStart(2, '0');
    const d = String(date.getDate()).padStart(2, '0');
    return `${y}-${m}-${d}`;
  }

  private formatDateTime(value: string | undefined): string {
    if (!value) {
      return '';
    }

    const d = new Date(value);
    if (isNaN(d.getTime())) {
      return value;
    }

    const day = String(d.getDate()).padStart(2, '0');
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const year = d.getFullYear();
    const hh = String(d.getHours()).padStart(2, '0');
    const mm = String(d.getMinutes()).padStart(2, '0');
    const ss = String(d.getSeconds()).padStart(2, '0');

    return `${day}/${month}/${year} ${hh}:${mm}:${ss}`;
  }

  private formatProcessClaimType(value: ProcessClaimType): string {
    return value === ProcessClaimType.Own
      ? this.localizationService.localize('Claim::ProcessClaimType:Own')
      : this.localizationService.localize('Claim::ProcessClaimType:Insurer');
  }

  private formatWorkTaskStatus(value: WorkTaskStatus): string {
    const map: Record<number, string> = {
      [WorkTaskStatus.New]: this.localizationService.localize('Claim::WorkTaskStatus:New') || 'Chờ tiếp nhận',
      [WorkTaskStatus.InProgress]: this.localizationService.localize('Claim::WorkTaskStatus:InProgress') || 'Đang xử lý',
      [WorkTaskStatus.Completed]: this.localizationService.localize('Claim::WorkTaskStatus:Completed') || 'Đã hoàn thành',
      [WorkTaskStatus.Cancelled]: this.localizationService.localize('Claim::WorkTaskStatus:Cancelled') || 'Đã hủy',
      [WorkTaskStatus.Accepted]: 'Đã tiếp nhận',
      [WorkTaskStatus.Rejected]: this.localizationService.localize('Claim::WorkTaskStatus:Rejected') || 'Từ chối',
      [WorkTaskStatus.Pending]: 'Tạm dừng',
      [WorkTaskStatus.Return]: 'Trả lại',
      [WorkTaskStatus.WaitApprove]: 'Chờ duyệt',
      [WorkTaskStatus.Approved]: 'Đã duyệt',
    };

    return map[value] || String(value);
  }

  private canShowReassign(row: OnsiteAssessmentTaskDto): boolean {
    if (row.canReassign != null) {
      return row.canReassign === true;
    }

    const status = row.workTaskStatus;
    return row.isReporter === true
      && (
        status === WorkTaskStatus.Rejected ||
        status === WorkTaskStatus.Return ||
        status === WorkTaskStatus.Completed
      );
  }

  private buildSearchInput(event?: Partial<TableLazyLoadEvent>): GetOnsiteAssessmentTasksInput {
    const normalizeText = (value: string | null): string | undefined => {
      const normalized = value?.trim();
      return normalized ? normalized : undefined;
    };

    const input: GetOnsiteAssessmentTasksInput = {
      skipCount: event?.first ?? 0,
      maxResultCount: event?.rows ?? this.pageSize,
      sorting: this.getSortingString(event as TableLazyLoadEvent),
      lobId: this.searchForm.lobId || undefined,
      insurerId: this.searchForm.insurerId || undefined,
      processClaimType: this.searchForm.processClaimType ?? undefined,
      processDeptId: this.searchForm.processDeptId || undefined,
      notifierPhone: normalizeText(this.searchForm.notifierPhone),
      openEmployeeId: this.searchForm.openEmployeeId || undefined,
      openDateFrom: this.searchForm.openDateFrom ? this.formatDate(this.searchForm.openDateFrom) : undefined,
      openDateTo: this.searchForm.openDateTo ? this.formatDate(this.searchForm.openDateTo) : undefined,
      workTaskStatus: this.searchForm.workTaskStatus ?? undefined,
      carPlate: normalizeText(this.searchForm.carPlate),
      vin: normalizeText(this.searchForm.vin),
      engineNumber: normalizeText(this.searchForm.engineNumber),
      code: normalizeText(this.searchForm.code),
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

    return input;
  }

  protected readonly normalizeVehiclePlate = normalizeVehiclePlate;

  private getStatusClass(status: WorkTaskStatus): string {
    const classMap: { [key: number]: string } = {
      [WorkTaskStatus.New]: 'px-2 py-1 rounded text-xs font-semibold bg-orange-100 text-orange-800',
      [WorkTaskStatus.InProgress]: 'px-2 py-1 rounded text-xs font-semibold bg-green-100 text-green-800',
      [WorkTaskStatus.Completed]: 'px-2 py-1 rounded text-xs font-semibold bg-blue-100 text-blue-800',
      [WorkTaskStatus.Cancelled]: 'px-2 py-1 rounded text-xs font-semibold bg-red-100 text-red-800',
      [WorkTaskStatus.Accepted]: 'px-2 py-1 rounded text-xs font-semibold bg-green-100 text-green-800',
      [WorkTaskStatus.Rejected]: 'px-2 py-1 rounded text-xs font-semibold bg-red-100 text-red-800',
      [WorkTaskStatus.Pending]: 'px-2 py-1 rounded text-xs font-semibold bg-gray-100 text-gray-800',
      [WorkTaskStatus.Return]: 'px-2 py-1 rounded text-xs font-semibold bg-gray-100 text-gray-800',
      [WorkTaskStatus.WaitApprove]: 'px-2 py-1 rounded text-xs font-semibold bg-orange-100 text-orange-800',
      [WorkTaskStatus.Approved]: 'px-2 py-1 rounded text-xs font-semibold bg-blue-100 text-blue-800',
    };
    return classMap[status] || '';
  }

  private getSortingString(event: TableLazyLoadEvent): string | undefined {
    if (!event?.sortField) {
      return undefined;
    }

    const sortOrder = event.sortOrder === 1 ? 'asc' : 'desc';
    const sortField = Array.isArray(event.sortField) ? event.sortField[0] : event.sortField;

    const fieldMap: Record<string, string> = {
      code: 'code',
      opendate: 'openDate',
      notifydate: 'notifyDate',
    };

    const mapped = fieldMap[String(sortField).toLowerCase()] || String(sortField);
    return `${mapped} ${sortOrder}`;
  }
}
