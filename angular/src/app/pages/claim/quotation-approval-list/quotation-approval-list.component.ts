import { CommonModule } from '@angular/common';
import { Component, HostListener, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import type { TableLazyLoadEvent } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { AutoCompleteCompleteEvent, AutoCompleteModule } from 'primeng/autocomplete';
import { InputTextModule } from 'primeng/inputtext';
import { PanelModule } from 'primeng/panel';
import { SelectModule } from 'primeng/select';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { TooltipModule } from 'primeng/tooltip';

import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';
import { VTable } from '@/shared/components/v-table/v-table';
import type { TableAction } from '@/shared/models/table-action.model';
import type { TableColumn } from '@/shared/models/table-column.model';
import { ProLineOfBusinessService } from '@/proxy/product/controllers/pro-line-of-business.service';
import { ResPartnerService } from '@/proxy/partner/controllers/res-partner.service';
import { HrDepartmentService } from '@/proxy/hr/controllers/hr-department.service';
import { HrEmployeeService } from '@/proxy/hr/controllers/hr-employee.service';
import { ClaimService } from '@/proxy/claim/controllers/claim.service';

import type { QuotationApprovalListRow, QuotationApprovalSearchForm, QuotationApprovalUiStatus } from './quotation-approval-list.models';
import { QuotationApprovalListService } from './quotation-approval-list.service';

@Component({
  selector: 'app-quotation-approval-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    PanelModule,
    ButtonModule,
    InputTextModule,
    SelectModule,
    ToastModule,
    AutoCompleteModule,
    TooltipModule,
    VTable,
    TranslatePipe,
  ],
  templateUrl: './quotation-approval-list.component.html',
  styleUrl: './quotation-approval-list.component.scss',
  providers: [MessageService],
})
export class QuotationApprovalListComponent implements OnInit {
  readonly pageSize = 10;

  loading = false;
  totalCount = 0;
  rows: QuotationApprovalListRow[] = [];
  currentLazyLoadEvent?: TableLazyLoadEvent;

  searchForm: QuotationApprovalSearchForm = {
    lobId: null,
    insurerId: null,
    processDeptId: null,
    claimId: null,
    workTaskStatus: '',
    carPlate: null,
    vin: null,
    engineNumber: null,
    folderNo: null,
    reporterId: null,
  };

  lobOptions: Array<{ label: string; value: string }> = [];
  insurerOptions: Array<{ label: string; value: string }> = [];
  departmentOptions: Array<{ label: string; value: string }> = [];
  employeeOptions: Array<{ label: string; value: string }> = [];
  claimOptions: Array<{ label: string; value: string }> = [];
  statusOptions: Array<{ label: string; value: '' | QuotationApprovalUiStatus }> = [
    { label: '--Tất cả--', value: '' },
    { label: 'Chưa duyệt', value: 'new' },
    { label: 'Đã duyệt', value: 'approved' },
    { label: 'Đã từ chối', value: 'rejected' },
  ];

  loadingLobs = false;
  loadingInsurers = false;
  loadingDepartments = false;
  loadingEmployees = false;
  loadingClaims = false;

  showAdvancedSearch = false;
  quickSearchKeyword: string | null = null;
  quickSearchSelectedType: 'carPlate' | 'vin' | 'engineNumber' | 'folderNo' | null = null;
  quickSearchSuggestions: Array<{
    type: 'carPlate' | 'vin' | 'engineNumber' | 'folderNo';
    prefix: string;
    keyword: string;
    display: string;
  }> = [];

  columns: TableColumn[] = [];
  actions: TableAction<QuotationApprovalListRow>[] = [];

  constructor(
    private readonly router: Router,
    private readonly quotationApprovalService: QuotationApprovalListService,
    private readonly lobService: ProLineOfBusinessService,
    private readonly partnerService: ResPartnerService,
    private readonly departmentService: HrDepartmentService,
    private readonly employeeService: HrEmployeeService,
    private readonly claimService: ClaimService,
    private readonly localizationService: LocalizationService,
    private readonly messageService: MessageService,
  ) {
    this.initializeColumns();
    this.initializeActions();
  }

  ngOnInit(): void {
    this.loadLobs();
    this.loadInsurers();
    this.loadDepartments();
    this.loadEmployees();
    this.loadClaims();
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

  onQuickSearchSuggestionSelect(event: {
    value: { type: 'carPlate' | 'vin' | 'engineNumber' | 'folderNo'; keyword: string };
  }): void {
    const selected = event?.value;
    this.quickSearchSelectedType = selected?.type || null;
    this.quickSearchKeyword = selected?.keyword?.trim() || null;
  }

  get quickSearchPrefixLabel(): string {
    if (this.quickSearchSelectedType === 'carPlate') return 'Biển số xe';
    if (this.quickSearchSelectedType === 'vin') return 'Số khung';
    if (this.quickSearchSelectedType === 'engineNumber') return 'Số máy';
    if (this.quickSearchSelectedType === 'folderNo') return 'Mã hồ sơ bồi thường';
    return '';
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
      {
        type: 'folderNo',
        prefix: 'Mã hồ sơ bồi thường',
        keyword,
        display: `Mã hồ sơ bồi thường: ${keyword}`,
      },
    ];
  }

  loadData(event?: TableLazyLoadEvent): void {
    this.currentLazyLoadEvent = event;
    this.loading = true;

    const statusVal = this.searchForm.workTaskStatus;
    const approvalStatus =
      statusVal === 'new' || statusVal === 'approved' || statusVal === 'rejected' ? statusVal : undefined;

    let carPlate = this.normalizeVehicleSearch(this.searchForm.carPlate)?.trim();
    let vin = this.normalizeVehicleSearch(this.searchForm.vin);
    let engineNumber = this.normalizeVehicleSearch(this.searchForm.engineNumber);
    let folderNo = this.searchForm.folderNo?.trim() || undefined;

    const keywordRaw = this.quickSearchKeyword?.trim() || '';
    if (keywordRaw && this.quickSearchSelectedType) {
      carPlate = undefined;
      vin = undefined;
      engineNumber = undefined;
      folderNo = undefined;

      if (this.quickSearchSelectedType === 'carPlate') {
        carPlate = this.normalizeVehicleSearch(keywordRaw)?.trim();
      } else if (this.quickSearchSelectedType === 'vin') {
        vin = this.normalizeVehicleSearch(keywordRaw);
      } else if (this.quickSearchSelectedType === 'engineNumber') {
        engineNumber = this.normalizeVehicleSearch(keywordRaw);
      } else if (this.quickSearchSelectedType === 'folderNo') {
        folderNo = keywordRaw.toUpperCase();
      }
    }

    this.quotationApprovalService
      .getList({
        skipCount: event?.first ?? 0,
        maxResultCount: event?.rows ?? this.pageSize,
        sorting: this.getSortingString(event),
        lobId: this.searchForm.lobId || undefined,
        insurerId: this.searchForm.insurerId || undefined,
        processDeptId: this.searchForm.processDeptId || undefined,
        claimId: this.searchForm.claimId || undefined,
        approvalStatus,
        carPlate,
        vin,
        engineNumber,
        folderNo,
        reporterId: this.searchForm.reporterId || undefined,
      })
      .subscribe({
        next: result => {
          const items = (result.items ?? []) as Record<string, unknown>[];
          const first = event?.first ?? 0;
          this.rows = items.map((it, idx) => this.mapApiRow(it, first + idx + 1));
          this.totalCount = result.totalCount ?? 0;
          this.loading = false;
        },
        error: () => {
          this.rows = [];
          this.totalCount = 0;
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
  handleEnter(event: Event): void {
    const target = event.target as HTMLElement;
    if (
      target?.closest?.('.p-datepicker-panel') == null &&
      target?.closest?.('.p-autocomplete-panel') == null
    ) {
      this.search();
    }
  }

  search(): void {
    const keywordRaw = this.quickSearchKeyword?.trim() || '';
    if (keywordRaw && !this.quickSearchSelectedType) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Chưa chọn loại tìm kiếm',
        detail: 'Vui lòng chọn gợi ý tìm kiếm (Biển số / Số khung / Số máy / Mã hồ sơ bồi thường).',
      });
      return;
    }

    this.loadData({
      ...(this.currentLazyLoadEvent ?? {}),
      first: 0,
      rows: this.currentLazyLoadEvent?.rows ?? this.pageSize,
    });
  }

  resetSearch(): void {
    this.quickSearchKeyword = null;
    this.quickSearchSelectedType = null;
    this.quickSearchSuggestions = [];
    this.searchForm = {
      lobId: this.defaultXeOToLobId,
      insurerId: null,
      processDeptId: null,
      claimId: null,
      workTaskStatus: '',
      carPlate: null,
      vin: null,
      engineNumber: null,
      folderNo: null,
      reporterId: null,
    };
    this.search();
  }

  onVehicleFieldBlur(field: 'carPlate' | 'vin' | 'engineNumber'): void {
    const cur = this.searchForm[field];
    if (!cur?.trim()) {
      this.searchForm[field] = null;
      return;
    }
    this.searchForm[field] = cur.replace(/[^A-Za-z0-9]/g, '').toUpperCase();
  }

  exportCsv(): void {
    const headers = [
      'STT',
      'Mã yêu cầu',
      'Mã HSBT',
      'BH gốc',
      'NV BH',
      'SPBH',
      'Biển số',
      'Người trình duyệt',
      'Ngày trình duyệt',
      'Ngày duyệt',
      'Ngày tổn thất',
      'Hiện trường',
      'PASC',
      'Trạng thái',
    ];
    const lines = this.rows.map(r =>
      [
        String(r.stt ?? ''),
        r.claimCode,
        r.folderNo,
        r.insurerCode ?? '',
        r.lobName ?? '',
        r.productName ?? '',
        r.carPlate ?? '',
        r.reporterName ?? '',
        this.formatDateTime(r.creationTime),
        this.formatDateTime(r.actualEndDate),
        this.formatDateTime(r.incidentDate),
        r.onLocation === 'Y' ? 'Y' : r.onLocation === 'N' ? 'N' : '',
        String(r.totalPascAmount ?? 0),
        this.formatUiStatus(r.approvalUiStatus),
      ]
        .map(v => `"${`${v ?? ''}`.replace(/"/g, '""')}"`)
        .join(','),
    );
    const content = [headers.join(','), ...lines].join('\n');
    const blob = new Blob(['\uFEFF' + content], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `quotation-approval_${new Date().toISOString().slice(0, 10)}.csv`;
    a.click();
    URL.revokeObjectURL(url);
  }

  openDetail(row: QuotationApprovalListRow): void {
    void this.router.navigate(['/pages/claim/quotation-approval-detail', row.claimId, row.workTaskId], {
      queryParams: {
        folderNo: row.folderNo || undefined,
        claimCode: row.claimCode || undefined,
        claimFolderId: row.claimFolderId || undefined,
        detailWtId: row.detailAssessmentWorkTaskId || undefined,
        approvalStatus: row.approvalUiStatus,
        totalPasc: row.totalPascAmount,
      },
    });
  }

  formatDateTime(value?: string | null): string {
    if (!value) return '';
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) return String(value);

    const d = date.toLocaleString('vi-VN', {
      hour: '2-digit',
      minute: '2-digit',
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour12: false,
    });
    return d.replace(',', '').replace(/\s+/g, ' ').trim();
  }

  formatCurrency(value?: number | null): string {
    return new Intl.NumberFormat('vi-VN').format(value ?? 0);
  }

  formatUiStatus(s: QuotationApprovalUiStatus): string {
    if (s === 'new') return 'Chưa duyệt';
    if (s === 'approved') return 'Đã duyệt';
    return 'Đã từ chối';
  }

  uiStatusCellClass(s: QuotationApprovalUiStatus): string {
    if (s === 'new') return 'px-2 py-1 rounded text-xs font-semibold bg-orange-100 text-orange-800';
    if (s === 'approved') return 'px-2 py-1 rounded text-xs font-semibold bg-green-100 text-green-800';
    return 'px-2 py-1 rounded text-xs font-semibold bg-red-100 text-red-800';
  }

  private defaultXeOToLobId: string | null = null;

  private initializeColumns(): void {
    this.columns = [
      { field: 'stt', header: 'STT', width: '72px', align: 'center', freeze: 'left' },
      { field: 'claimCode', header: 'Mã yêu cầu', width: '130px', sortable: true, freeze: 'left' },
      { field: 'folderNo', header: 'Mã HSBT', width: '120px', sortable: true },
      {
        field: 'insurerCode',
        header: this.localizationService.localize('Claim::Claim:InsurerCode') || 'BH gốc',
        width: '120px',
      },
      { field: 'lobName', header: 'Nghiệp vụ BH', width: '140px', wrap: true },
      { field: 'productName', header: 'Sản phẩm BH', width: '160px', wrap: true },
      { field: 'carPlate', header: 'Biển số xe', width: '120px' },
      { field: 'reporterName', header: 'Người trình duyệt', width: '170px', wrap: true },
      {
        field: 'creationTime',
        header: 'Ngày trình duyệt',
        width: '155px',
        formatter: (_, row) =>
          this.formatDateTime((row as QuotationApprovalListRow)?.creationTime),
      },
      {
        field: 'actualEndDate',
        header: 'Ngày duyệt',
        width: '155px',
        formatter: (_, row) =>
          this.formatDateTime((row as QuotationApprovalListRow)?.actualEndDate),
      },
      {
        field: 'incidentDate',
        header: 'Ngày tổn thất',
        width: '155px',
        formatter: (_, row) =>
          this.formatDateTime((row as QuotationApprovalListRow)?.incidentDate),
      },
      {
        field: 'onLocation',
        header: 'Hiện trường',
        width: '100px',
        align: 'center',
        formatter: v => ((v === 'Y' || v === true) ? 'Y' : (v === 'N' ? 'N' : '')),
      },
      {
        field: 'totalPascAmount',
        header: 'Số tiền theo PASC',
        align: 'right',
        width: '150px',
        formatter: v => this.formatCurrency(Number(v)),
      },
      {
        field: 'approvalUiStatus',
        header: 'Trạng thái',
        width: '130px',
        freeze: 'right',
        formatter: v => this.formatUiStatus(v as QuotationApprovalUiStatus),
        cellClass: v => this.uiStatusCellClass(v as QuotationApprovalUiStatus),
      },
    ];
  }

  private initializeActions(): void {
    this.actions = [
      {
        label: 'Xem chi tiết',
        icon: 'pi pi-eye',
        command: row => this.openDetail(row),
      },
    ];
  }

  private mapApiRow(raw: Record<string, unknown>, stt: number): QuotationApprovalListRow {
    const str = (k: string) => String(raw[k] ?? raw[this.pascal(k)] ?? '') || '';
    const num = (k: string) => Number(raw[k] ?? raw[this.pascal(k)] ?? 0);
    const optStr = (k: string): string | null => {
      const v = raw[k] ?? raw[this.pascal(k)];
      if (v === null || v === undefined) return null;
      return String(v);
    };

    let approvalUi = String(raw['approvalUiStatus'] ?? raw['ApprovalUiStatus'] ?? 'new').toLowerCase() as QuotationApprovalUiStatus;
    if (!['new', 'approved', 'rejected'].includes(approvalUi)) approvalUi = 'new';

    return {
      stt,
      workTaskId: str('workTaskId'),
      claimId: str('claimId'),
      claimFolderId: str('claimFolderId'),
      claimCode: str('claimCode'),
      folderNo: str('folderNo'),
      insurerCode: optStr('insurerCode') ?? '',
      lobName: optStr('lobName') ?? '',
      productName: optStr('productName') ?? '',
      carPlate: optStr('carPlate'),
      reporterName: optStr('reporterName'),
      creationTime: str('creationTime'),
      actualEndDate: optStr('actualEndDate'),
      incidentDate: optStr('incidentDate'),
      onLocation: optStr('onLocation'),
      totalPascAmount: num('totalPascAmount'),
      workTaskStatus: String(raw['workTaskStatus'] ?? raw['WorkTaskStatus'] ?? ''),
      approvalUiStatus: approvalUi,
      detailAssessmentWorkTaskId: optStr('detailAssessmentWorkTaskId') ?? optStr('DetailAssessmentWorkTaskId'),
    };
  }

  private pascal(k: string): string {
    return k.charAt(0).toUpperCase() + k.slice(1);
  }

  private getSortingString(event?: TableLazyLoadEvent): string | undefined {
    if (!event?.sortField) return undefined;
    const sortOrder = event.sortOrder === 1 ? 'asc' : 'desc';
    const sortField = Array.isArray(event.sortField) ? event.sortField[0] : event.sortField;
    const fieldMap: Record<string, string> = {
      claimcode: 'claimCode',
      folderNo: 'folderNo',
      creationtime: 'creationTime',
    };
    const key = String(sortField).toLowerCase();
    return `${fieldMap[key] ?? String(sortField)} ${sortOrder}`;
  }

  private loadLobs(): void {
    this.loadingLobs = true;
    this.lobService.getSelectList().subscribe({
      next: items => {
        this.lobOptions = (items ?? []).map(i => ({
          label: i.name ?? '',
          value: i.id ?? '',
        }));
        this.loadingLobs = false;

        const xeOto =
          items?.find(i => i.name?.toLowerCase().includes('xe ô tô') || i.name?.toLowerCase().includes('xe o to')) ??
          items?.find(i => i.code?.toLowerCase() === 'motor');
        const id = xeOto?.id ?? '';
        if (id) this.defaultXeOToLobId = id;

        if (id && !this.searchForm.lobId) this.searchForm.lobId = id;
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
        this.insurerOptions = (items ?? []).map(i => ({ label: `${i.code ?? ''} — ${i.name ?? ''}`, value: i.id ?? '' }));
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
        this.departmentOptions = (items ?? []).map(i => ({ label: i.name ?? '', value: i.id ?? '' }));
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
        this.employeeOptions = (items ?? []).map(e => ({
          label: `${e.fullName ?? ''} (${e.code ?? ''})`,
          value: e.id ?? '',
        }));
        this.loadingEmployees = false;
      },
      error: () => {
        this.loadingEmployees = false;
      },
    });
  }

  private loadClaims(): void {
    this.loadingClaims = true;
    this.claimService
      .getList({
        skipCount: 0,
        maxResultCount: 50,
        sorting: 'creationTime desc',
      })
      .subscribe({
        next: res => {
          this.claimOptions = (res.items ?? []).map(c => ({
            label: `${c.code ?? ''}`.trim(),
            value: c.id ?? '',
          }));
          this.loadingClaims = false;
        },
        error: () => {
          this.loadingClaims = false;
        },
      });
  }

  private normalizeVehicleSearch(v: string | null | undefined): string | undefined {
    if (!v?.trim()) return undefined;
    return v.replace(/[^A-Za-z0-9]/g, '').toUpperCase();
  }
}
