import { CommonModule } from '@angular/common';
import { Component, HostListener, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { TableLazyLoadEvent } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { DatePickerModule } from 'primeng/datepicker';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { PanelModule } from 'primeng/panel';
import { SelectModule } from 'primeng/select';
import { ToastModule } from 'primeng/toast';
import { ConfirmationService, MenuItem, MessageService } from 'primeng/api';
import { TooltipModule } from 'primeng/tooltip';
import { MenuModule } from 'primeng/menu';
import { AutoCompleteCompleteEvent, AutoCompleteModule } from 'primeng/autocomplete';

import { LocalizationService } from '@/core/services/localization.service';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { normalizeVehiclePlate } from '@/core/utils/vehicle-plate.util';
import { VTable } from '@/shared/components/v-table/v-table';
import { TableAction } from '@/shared/models/table-action.model';
import { TableColumn } from '@/shared/models/table-column.model';
import { ProcessClaimType } from '@/proxy/claims/process-claim-type.enum';
import { WorkTaskStatus } from '@/proxy/claims/work-task-status.enum';
import { ProLineOfBusinessService } from '@/proxy/product/controllers/pro-line-of-business.service';
import { ResPartnerService } from '@/proxy/partner/controllers/res-partner.service';
import { HrDepartmentService } from '@/proxy/hr/controllers/hr-department.service';
import { HrEmployeeService } from '@/proxy/hr/controllers/hr-employee.service';
import { ClaimTaskDto, GetClaimTasksInput } from '@/proxy/claim/claims/claim-task-models';
import { DetailAssessmentTaskService } from './detail-assessment-task.service';
import { DetailedAssessmentTabService } from '../detailed-assessment-tab/detailed-assessment-tab.service';
import { ReassignDetailedAssessmentModalComponent } from './reassign-detailed-assessment-modal.component';

import { DetailAssessmentSearchForm, DetailAssessmentTaskRow } from './detail-assessment-list.models';

type DetailDialogMode = 'view' | 'accept' | 'process';
type DetailAssessmentScreenMode = 'view' | 'action';

@Component({
  selector: 'app-detail-assessment-list',
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
    ReassignDetailedAssessmentModalComponent,
  ],
  templateUrl: './detail-assessment-list.component.html',
  styleUrl: './detail-assessment-list.component.scss',
  providers: [MessageService, ConfirmationService],
})
export class DetailAssessmentListComponent implements OnInit {
  readonly pageSize = 10;

  loading = true;
  totalCount = 0;
  rows: DetailAssessmentTaskRow[] = [];
  currentLazyLoadEvent?: TableLazyLoadEvent;

  searchForm: DetailAssessmentSearchForm = {
    lobId: null,
    insurerId: null,
    processDeptId: null,
    notifierPhone: null,
    openEmployeeId: null,
    openDateFrom: null,
    openDateTo: null,
    processClaimType: null,
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
  processClaimTypeOptions: Array<{ label: string; value: ProcessClaimType }> = [];
  workTaskStatusOptions: Array<{ label: string; value: WorkTaskStatus }> = [];

  loadingLobs = false;
  loadingInsurers = false;
  loadingDepartments = false;
  loadingEmployees = false;
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
  actions: TableAction<DetailAssessmentTaskRow>[] = [];

  detailDialogVisible = false;
  detailDialogMode: DetailDialogMode = 'view';
  selectedRow: DetailAssessmentTaskRow | null = null;
  reassignModalVisible = false;
  reassignWorkTaskId: string | null = null;

  constructor(
    private readonly detailAssessmentTaskService: DetailAssessmentTaskService,
    private readonly lobService: ProLineOfBusinessService,
    private readonly partnerService: ResPartnerService,
    private readonly departmentService: HrDepartmentService,
    private readonly employeeService: HrEmployeeService,
    private readonly detailedAssessmentTabService: DetailedAssessmentTabService,
    private readonly localizationService: LocalizationService,
    private readonly messageService: MessageService,
    private readonly confirmationService: ConfirmationService,
    private readonly router: Router
  ) {
    this.initializeOptions();
    this.initializeColumns();
    this.initializeActions();
  }

  ngOnInit(): void {
    this.loadLobs();
    this.loadInsurers();
    this.loadDepartments();
    this.loadEmployees();
    this.buildSearchActionMenuItems();
    this.loadData({ first: 0, rows: this.pageSize });
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
        label: 'Xuất file',
        icon: 'pi pi-download',
        command: () => this.exportFile(),
      },
    ];
  }

  loadData(event?: TableLazyLoadEvent): void {
    this.currentLazyLoadEvent = event;
    this.loading = true;

    const input: GetClaimTasksInput = {
      skipCount: event?.first ?? 0,
      maxResultCount: event?.rows ?? this.pageSize,
      sorting: this.getSortingString(event),
      lobId: this.searchForm.lobId || undefined,
      insurerId: this.searchForm.insurerId || undefined,
      processDeptId: this.searchForm.processDeptId || undefined,
      notifierPhone: this.searchForm.notifierPhone?.trim() || undefined,
      openEmployeeId: this.searchForm.openEmployeeId || undefined,
      openDateFrom: this.searchForm.openDateFrom ? this.formatApiDate(this.searchForm.openDateFrom) : undefined,
      openDateTo: this.searchForm.openDateTo ? this.formatApiDate(this.searchForm.openDateTo) : undefined,
      processClaimType: this.searchForm.processClaimType ?? undefined,
      workTaskStatus: this.searchForm.workTaskStatus ?? undefined,
      carPlate: this.searchForm.carPlate?.trim() || undefined,
      vin: this.searchForm.vin?.trim() || undefined,
      engineNumber: this.searchForm.engineNumber?.trim() || undefined,
      code: this.searchForm.code?.trim() || undefined,
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

    console.group('[DetailAssessmentList] Search');
    console.log('Request input:', input);

    this.detailAssessmentTaskService.getList(input).subscribe({
      next: result => {
        this.rows = (result.items || []).map(item => this.mapClaimTaskRow(item));
        this.totalCount = result.totalCount || 0;
        console.log('Total count:', this.totalCount);
        console.table(
          (result.items || []).map(item => ({
            id: item.id,
            claimId: item.claimId,
            claimCode: item.code,
            folderNo: item.folderNo,
            insurerCode: item.insurerCode,
            workTaskStatus: item.workTaskStatus,
            taskCreationTime: item.taskCreationTime,
          }))
        );
        console.groupEnd();
        this.loading = false;
      },
      error: () => {
        this.rows = [];
        this.totalCount = 0;
        console.groupEnd();
        this.loading = false;
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Claim::Error') || 'Lỗi',
          detail: this.localizationService.localize('Claim::InternalServerErrorMessage') || 'Không tải được dữ liệu.',
        });
      },
    });
  }

  @HostListener('window:keydown.enter', ['$event'])
  handleEnter(event: KeyboardEvent) {
    // Only trigger search if dialog is not visible
    if (!this.detailDialogVisible) {
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

    this.loadData({
      ...(this.currentLazyLoadEvent || {}),
      first: 0,
      rows: this.currentLazyLoadEvent?.rows ?? this.pageSize,
    });
  }

  resetSearch(): void {
    this.quickSearchKeyword = null;
    this.quickSearchSelectedType = null;
    this.quickSearchSuggestions = [];
    this.searchForm = {
      lobId: null,
      insurerId: null,
      processDeptId: null,
      notifierPhone: null,
      openEmployeeId: null,
      openDateFrom: null,
      openDateTo: null,
      processClaimType: null,
      workTaskStatus: null,
      carPlate: null,
      vin: null,
      engineNumber: null,
      code: null,
    };
    this.search();
  }

  protected readonly normalizeVehiclePlate = normalizeVehiclePlate;

  exportFile(): void {
    const headers = [
      'Mã yêu cầu',
      'Mã HSBT',
      this.localizationService.localize('Claim::Claim:InsurerCode'),
      'Nghiệp vụ BH',
      'Sản phẩm BH',
      'Biển số xe',
      'Ngày tiếp nhận',
      'Người tiếp nhận',
      'Ngày tổn thất',
      'Hiện trường',
      'Bên xử lý',
      'Ước bồi thường',
      'Trạng thái',
    ];

    const lines = this.rows.map(row =>
      [
        row.claimCode,
        row.folderNo,
        row.insurerCode,
        row.lobName,
        row.productName,
        row.carPlate,
        this.formatDateTime(row.openDate),
        row.openEmployeeName,
        this.formatDateTime(row.incidentDate),
        row.onLocation ? 'Y' : 'N',
        this.formatProcessClaimType(row.processClaimType),
        this.formatCurrency(row.compensationAmount),
        this.formatWorkTaskStatus(row.workTaskStatus),
      ]
        .map(value => `"${`${value ?? ''}`.replace(/"/g, '""')}"`)
        .join(',')
    );

    const content = [headers.join(','), ...lines].join('\n');
    const blob = new Blob(['\uFEFF' + content], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = `detail-assessment-tasks_${new Date().toISOString().slice(0, 10)}.csv`;
    anchor.click();
    URL.revokeObjectURL(url);

    this.messageService.add({
      severity: 'success',
      summary: this.localizationService.localize('Claim::Success') || 'Thành công',
      detail: this.localizationService.localize('Claim::ExportSuccess') || 'Xuất file thành công.',
    });
  }

  openDialog(row: DetailAssessmentTaskRow, mode: DetailDialogMode): void {
    this.selectedRow = row;
    this.detailDialogMode = mode;
    this.detailDialogVisible = true;
  }

  openDetailedAssessmentScreen(row: DetailAssessmentTaskRow, mode: DetailAssessmentScreenMode = 'view'): void {
    if (!row.claimId || !row.workTaskId) {
      return;
    }

    this.router.navigate(
      ['/pages/claim/onsite-assessment-detail', row.claimId, row.workTaskId],
      {
        queryParams: {
          tab: 3,
          source: 'detail-assessment-list',
          detailMode: mode,
          allowCompletedDetailedEdit:
            mode === 'action' && row.canUpdateCompletedDetailedAssessment === true ? true : undefined,
          folderNo: row.folderNo || undefined,
          estimateAmount: row.compensationAmount ?? 0,
        },
      }
    );
  }

  closeDialog(): void {
    this.detailDialogVisible = false;
  }

  openReassignModal(row: DetailAssessmentTaskRow): void {
    if (!row.workTaskId) {
      return;
    }

    this.reassignWorkTaskId = row.workTaskId;
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
    this.loadData(this.currentLazyLoadEvent);
  }

  submitDialog(): void {
    if (!this.selectedRow) {
      return;
    }

    if (this.detailDialogMode === 'accept') {
      this.selectedRow.workTaskStatus = WorkTaskStatus.InProgress;
      this.messageService.add({
        severity: 'success',
        summary: 'Thành công',
        detail: `Đã tiếp nhận hồ sơ ${this.selectedRow.claimCode}.`,
      });
    } else if (this.detailDialogMode === 'process') {
      this.selectedRow.workTaskStatus = WorkTaskStatus.Completed;
      this.selectedRow.canCancel = false;
      this.messageService.add({
        severity: 'success',
        summary: 'Thành công',
        detail: `Đã cập nhật xử lý giám định chi tiết cho hồ sơ ${this.selectedRow.claimCode}.`,
      });
    }

    this.closeDialog();
  }

  confirmCancel(row: DetailAssessmentTaskRow): void {
    this.confirmationService.confirm({
      message: 'Bạn có chắc chắn muốn hủy HSBT hiện tại không?',
      header: 'Xác nhận hủy giám định',
      acceptLabel: this.localizationService.localize('Claim::Yes') || 'Có',
      rejectLabel: this.localizationService.localize('Claim::No') || 'Không',
      acceptButtonStyleClass: 'detailed-cancel-accept-button',
      accept: () => {
        this.detailedAssessmentTabService.cancel(row.workTaskId).subscribe({
          next: () => {
            row.workTaskStatus = WorkTaskStatus.Cancelled;
            row.canCancel = false;
            this.messageService.add({
              severity: 'success',
              summary: 'Thành công',
              detail: `Đã hủy giám định chi tiết cho hồ sơ ${row.claimCode}.`,
            });
            this.loadData(this.currentLazyLoadEvent);
          },
          error: () => {
            this.messageService.add({
              severity: 'error',
              summary: this.localizationService.localize('Claim::Error') || 'Lỗi',
              detail: 'Không hủy được giám định chi tiết.',
            });
          },
        });
      },
    });
    this.focusDetailedCancelAcceptButton();
  }

  private focusDetailedCancelAcceptButton(): void {
    [0, 50, 100, 150, 250, 400, 650, 900].forEach(delay => {
      setTimeout(() => {
        const target = document.querySelector<HTMLElement>('.detailed-cancel-accept-button');
        const button = target instanceof HTMLButtonElement
          ? target
          : target?.querySelector<HTMLButtonElement>('button') ?? null;

        if (button && !button.disabled && document.activeElement !== button) {
          button.focus();
        }
      }, delay);
    });
  }

  get dialogTitle(): string {
    switch (this.detailDialogMode) {
      case 'accept':
        return 'Tiếp nhận HSBT';
      case 'process':
        return 'Xử lý giám định chi tiết';
      default:
        return 'Xem chi tiết HSBT';
    }
  }

  get dialogActionLabel(): string {
    switch (this.detailDialogMode) {
      case 'accept':
        return 'Tiếp nhận';
      case 'process':
        return 'Hoàn tất xử lý';
      default:
        return 'Đóng';
    }
  }

  isViewMode(): boolean {
    return this.detailDialogMode === 'view';
  }

  formatDateTime(value?: string | null): string {
    if (!value) {
      return '';
    }

    const date = new Date(value);
    if (Number.isNaN(date.getTime())) {
      return value;
    }

    return new Intl.DateTimeFormat('vi-VN', {
      hour: '2-digit',
      minute: '2-digit',
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour12: false,
    }).format(date).replace(',', '');
  }

  formatDate(value?: string | null): string {
    if (!value) {
      return '';
    }

    const date = new Date(value);
    if (Number.isNaN(date.getTime())) {
      return value;
    }

    return new Intl.DateTimeFormat('vi-VN', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
    }).format(date);
  }

  formatCurrency(value?: number | null): string {
    return new Intl.NumberFormat('vi-VN').format(value ?? 0);
  }

  formatProcessClaimType(value?: ProcessClaimType | null): string {
    switch (value) {
      case ProcessClaimType.Insurer:
        return 'Bảo hiểm gốc xử lý';
      case ProcessClaimType.Own:
      default:
        return 'Tự xử lý';
    }
  }

  formatWorkTaskStatus(value?: WorkTaskStatus | null): string {
    const labels: Partial<Record<WorkTaskStatus, string>> = {
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
      [WorkTaskStatus.Transfer]: 'Điều chuyển',
    };

    return labels[value as WorkTaskStatus] || (value ? String(value) : '');
  }

  statusCellClass(value: WorkTaskStatus): string {
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
      [WorkTaskStatus.Transfer]: 'px-2 py-1 rounded text-xs font-semibold bg-sky-100 text-sky-800',
    };

    return classMap[value] || '';
  }

  private initializeOptions(): void {
    this.processClaimTypeOptions = [
      { label: 'Tự xử lý', value: ProcessClaimType.Own },
      { label: 'Bảo hiểm gốc xử lý', value: ProcessClaimType.Insurer },
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

  private initializeColumns(): void {
    this.columns = [
      { field: 'claimCode', header: 'Mã yêu cầu', width: '160px', sortable: true, freeze: 'left' },
      { field: 'folderNo', header: 'Mã HSBT', width: '120px', sortable: true, freeze: 'left' },
      {
        field: 'insurerCode',
        header: this.localizationService.localize('Claim::Claim:InsurerCode'),
        width: '140px',
        wrap: true,
      },
      { field: 'lobName', header: 'Nghiệp vụ BH', width: '140px', wrap: true },
      { field: 'productName', header: 'Sản phẩm BH', width: '180px', wrap: true },
      { field: 'carPlate', header: 'Biển số xe', width: '130px' },
      {
        field: 'openDate',
        header: 'Ngày tiếp nhận',
        width: '150px',
        formatter: value => this.formatDateTime(value),
      },
      { field: 'openEmployeeName', header: 'Người tiếp nhận', width: '160px', wrap: true },
      {
        field: 'incidentDate',
        header: 'Ngày tổn thất',
        width: '120px',
        formatter: value => this.formatDateTime(value),
      },
      { field: 'onLocation', header: 'Hiện trường', width: '100px', type: 'boolean', align: 'center' },
      {
        field: 'processClaimType',
        header: 'Bên xử lý',
        width: '150px',
        formatter: value => this.formatProcessClaimType(value),
        wrap: true,
      },
      {
        field: 'compensationAmount',
        header: 'Ước bồi thường',
        width: '160px',
        align: 'right',
        formatter: value => this.formatCurrency(value),
      },
      {
        field: 'workTaskStatus',
        header: 'Trạng thái',
        width: '140px',
        freeze: 'right',
        formatter: value => this.formatWorkTaskStatus(value),
        cellClass: value => this.statusCellClass(value),
      },
    ];
  }

  private initializeActions(): void {
    this.actions = [
      {
        label: 'Xem chi tiết',
        icon: 'pi pi-eye',
        command: row => this.openDetailedAssessmentScreen(row, 'view'),
      },
      {
        label: 'Tiếp nhận',
        icon: 'pi pi-check',
        visible: row => row.workTaskStatus === WorkTaskStatus.New && row.isAssignee === true,
        command: row => this.openDetailedAssessmentScreen(row, 'action'),
      },
      {
        label: 'Xử lý GĐ',
        icon: 'pi pi-file-edit',
        visible: row => row.workTaskStatus === WorkTaskStatus.InProgress && row.isAssignee === true,
        command: row => this.openDetailedAssessmentScreen(row, 'action'),
      },
      {
        label: 'Giám định lại',
        icon: 'pi pi-refresh',
        visible: row =>
          row.workTaskStatus === WorkTaskStatus.Completed &&
          row.isAssignee === true &&
          row.canUpdateCompletedDetailedAssessment === true,
        command: row => this.openDetailedAssessmentScreen(row, 'action'),
      },
      {
        label: 'Hủy GĐ',
        icon: 'pi pi-times-circle',
        visible: row => row.canCancel && row.isReporter === true && row.workTaskStatus !== WorkTaskStatus.Completed,
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

  private loadLobs(): void {
    this.loadingLobs = true;
    this.lobService.getSelectList().subscribe({
      next: items => {
        this.lobOptions = (items || []).map(item => ({ label: item.name || '', value: item.id || '' }));
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
        this.insurerOptions = (items || []).map(item => ({ label: item.name || '', value: item.id || '' }));
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
        this.departmentOptions = (items || []).map(item => ({ label: item.name || '', value: item.id || '' }));
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
        this.employeeOptions = (items || []).map(item => ({ label: item.fullName || '', value: item.id || '' }));
        this.loadingEmployees = false;
      },
      error: () => {
        this.loadingEmployees = false;
      },
    });
  }

  private getSortingString(event?: TableLazyLoadEvent): string | undefined {
    if (!event?.sortField) {
      return undefined;
    }

    const sortOrder = event.sortOrder === 1 ? 'asc' : 'desc';
    const sortField = Array.isArray(event.sortField) ? event.sortField[0] : event.sortField;
    const fieldMap: Record<string, string> = {
      claimcode: 'code',
      opendate: 'openDate',
      worktaskstatus: 'workTaskStatus',
      taskcreationtime: 'creationTime',
    };
    const mapped = fieldMap[String(sortField).toLowerCase()] || String(sortField);

    return `${mapped} ${sortOrder}`;
  }

  private formatApiDate(value: Date): string {
    const date = new Date(value);
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  private mapClaimTaskRow(item: ClaimTaskDto): DetailAssessmentTaskRow {
    const workTaskStatus = item.workTaskStatus ?? WorkTaskStatus.New;
    const canCancel = (item as ClaimTaskDto & { canCancel?: boolean }).canCancel;
    const canUpdateCompletedDetailedAssessment = (
      item as ClaimTaskDto & { canUpdateCompletedDetailedAssessment?: boolean }
    ).canUpdateCompletedDetailedAssessment;
    const isReporter = item.isReporter ?? false;
    const canReassign = item.canReassign ?? (isReporter && this.isReassignableStatus(workTaskStatus));

    return {
      id: item.claimId || item.id || '',
      claimId: item.claimId || '',
      workTaskId: item.id || '',
      claimCode: item.code || '',
      folderNo: item.folderNo || '',
      insurerCode: item.insurerCode || '',
      lobName: item.lobName || '',
      productName: item.productName || '',
      carPlate: item.carPlate || '',
      openDate: item.openDate || '',
      openEmployeeName: item.openEmployeeName || '',
      incidentDate: item.incidentDate || '',
      onLocation: item.onLocation === 'Y',
      processClaimType: item.processClaimType ?? ProcessClaimType.Own,
      compensationAmount: item.estimateAmount ?? 0,
      claimStatus: null,
      workTaskStatus,
      taskCreationTime: item.taskCreationTime || '',
      canCancel: (canCancel ?? workTaskStatus === WorkTaskStatus.New) && isReporter,
      isReporter,
      isAssignee: item.isAssignee ?? false,
      canReassign,
      canUpdateCompletedDetailedAssessment: canUpdateCompletedDetailedAssessment ?? false,
    };
  }

  private canShowReassign(row: DetailAssessmentTaskRow): boolean {
    return row.canReassign || (row.isReporter && this.isReassignableStatus(row.workTaskStatus));
  }

  private isReassignableStatus(status: WorkTaskStatus): boolean {
    return status === WorkTaskStatus.Rejected
      || status === WorkTaskStatus.Pending
      || status === WorkTaskStatus.Return;
  }
}
