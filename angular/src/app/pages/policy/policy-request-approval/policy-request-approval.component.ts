import { Component, DestroyRef, HostListener, OnInit } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { forkJoin } from 'rxjs';
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { DatePickerModule } from 'primeng/datepicker';
import { TableLazyLoadEvent } from 'primeng/table';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { DialogModule } from 'primeng/dialog';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TextareaModule } from 'primeng/textarea';

import { VTable } from '@/shared/components/v-table/v-table';
import { TableColumn } from '@/shared/models/table-column.model';
import { TableAction } from '@/shared/models/table-action.model';
import { PolicyRequestApprovalService } from '@/proxy/policy/controllers/policy-request-approval.service';
import type { PolicyDto } from '@/proxy/policy/policies/models';
import type { GetPolicyRequestApprovalListInput, PolicyRequestApprovalItemDto } from '@/proxy/policy/policy-request-approval/models';
import { WorkTaskStatus } from '@/proxy/work-tasks/work-task-status.enum';
import { PolicyStatus } from '@/proxy/policies/policy-status.enum';
import { PolicyTerminationStatus } from '@/proxy/policies/policy-termination-status.enum';
import { PolicySellType } from '@/proxy/policies/policy-sell-type.enum';
import { PolicyContractType } from '@/proxy/policy-contracts/policy-contract-type.enum';
import { PolicyContractStatus } from '@/proxy/policy-contracts/policy-contract-status.enum';
import { PolicyRequestApprovalSearchForm, type ApprovalTypeFilter } from './policy-request-approval.models';

const BUSINESS_CODE_CREATE = 'CREATE_POLICY_APPROVAL';
const BUSINESS_CODE_TERMINATE = 'TERMINATE_POLICY_APPROVAL';
const BUSINESS_CODE_ENDORSEMENT = 'ENDORSEMENT_POLICY_APPROVAL';

const REASON_GROUP_CREATE = 'APPROVAL_POLICY_REASON';
const REASON_GROUP_TERMINATE = 'TERMINATE_POLICY_REASON';
const REASON_GROUP_ENDORSEMENT = 'ENDORSEMENT_POLICY_REASON';

function getReasonGroupCodeForBusinessCode(businessCode: string | undefined): string | null {
  if (businessCode === BUSINESS_CODE_CREATE) return REASON_GROUP_CREATE;
  if (businessCode === BUSINESS_CODE_TERMINATE) return REASON_GROUP_TERMINATE;
  if (businessCode === BUSINESS_CODE_ENDORSEMENT) return REASON_GROUP_ENDORSEMENT;
  return null;
}

/** Flattened row for table: policy fields + businessCode + workTaskId + workTaskStatus + policyVersionStatus + policyVersionId (để xem chi tiết theo version) */
export type PolicyApprovalRow = PolicyDto & { businessCode?: string; workTaskId?: string; workTaskStatus?: WorkTaskStatus; policyVersionStatus?: string; policyVersionId?: string };
import { PermissionPipe } from '@/core/pipes/permission.pipe';
import { PermissionService } from '@/core/services/permission.service';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';
import { ResReasonService } from '@/proxy/master/controllers/res-reason.service';
import { ResChannelService } from '@/proxy/partner/controllers/res-channel.service';
import { ResCustomerService } from '@/proxy/customer/controllers/res-customer.service';
import { ResPartnerService } from '@/proxy/partner/controllers/res-partner.service';
import { HrEmployeeService } from '@/proxy/hr/controllers/hr-employee.service';
import { PolicyTypeService } from '@/proxy/policy/controllers/policy-type.service';
import { PolicyContractService } from '@/proxy/policy/controllers/policy-contract.service';
import { ResCarBrandService } from '@/proxy/master/controllers/res-car-brand.service';
import { ResCarCategoryService } from '@/proxy/master/controllers/res-car-category.service';
import { exportToExcel, ExportColumn } from '@/shared/utils/export.util';
import { MulticolumnComboboxComponent } from '@/shared/components/multicolumn-combobox/multicolumn-combobox.component';

@Component({
  selector: 'app-policy-request-approval',
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
    ConfirmDialogModule,
    TextareaModule,
    VTable,
    PermissionPipe,
    TranslatePipe,
    MulticolumnComboboxComponent
  ],
  templateUrl: './policy-request-approval.component.html',
  styleUrl: './policy-request-approval.component.scss',
  providers: [MessageService, ConfirmationService]
})
export class PolicyRequestApprovalComponent implements OnInit {
  readonly PERMISSIONS = {
    VIEW: 'PolicyPolicy.RequestApproval.View',
    APPROVE: 'PolicyPolicy.RequestApproval.Approve',
    TERMINATE_APPROVE: 'PolicyPolicy.TerminateApproval.Approve',
    ENDORSEMENT_APPROVE: 'PolicyPolicy.EndorsementApproval.Approve'
  };

  policies: PolicyApprovalRow[] = [];
  totalCount = 0;
  loading = true;
  pageSize = 10;
  selectedPolicies: PolicyApprovalRow[] = [];
  currentLazyLoadEvent?: TableLazyLoadEvent;

  columns: TableColumn[] = [];
  actions: TableAction<PolicyApprovalRow>[] = [];

  approvalTypeOptions: Array<{ label: string; value: ApprovalTypeFilter }> = [];

  searchForm: PolicyRequestApprovalSearchForm = {
    approvalTypeFilter: 'all',
    policyNo: null,
    contractId: null,
    policyTypeId: null,
    partnerId: null,
    status: null,
    approvalStatus: null,
    channelId: null,
    customerId: null,
    contractType: null,
    contractStatus: null,
    implementerId: null,
    certificateNo: null,
    effectiveDateFrom: null,
    effectiveDateTo: null,
    expiryDateFrom: null,
    expiryDateTo: null,
    carPlate: null,
    carVin: null,
    carEngineNumber: null,
    primaryInsurancePartnerId: null,
    importLotNumber: null
  };

  statusOptions: Array<{ label: string; value: PolicyStatus }> = [];
  approvalStatusOptions: Array<{ label: string; value: string }> = [];
  contractTypeOptions: Array<{ label: string; value: PolicyContractType }> = [];
  contractStatusOptions: Array<{ label: string; value: PolicyContractStatus }> = [];
  channelOptions: Array<{ label: string; value: string }> = [];
  customerOptions: Array<{ label: string; value: string; phone?: string; email?: string; idNo?: string; address?: string }> = [];
  readonly customerColumns = [
    { field: 'label', header: 'Tên KH', width: '25' },
    { field: 'phone', header: 'Điện thoại', width: '15' },
    { field: 'email', header: 'Email', width: '20' },
    { field: 'idNo', header: 'CMND/CCCD', width: '15' },
    { field: 'address', header: 'Địa chỉ', width: '25' },
  ];
  contractOptions: Array<{ label: string; value: string }> = [];
  partnerOptions: Array<{ label: string; value: string; code?: string }> = [];
  implementerOptions: Array<{ label: string; value: string; employeeCode?: string }> = [];
  policyTypeOptions: Array<{ label: string; value: string }> = [];
  primaryInsurancePartnerOptions: Array<{ label: string; value: string; code?: string }> = [];

  rejectReasonModalVisible = false;
  /**
   * Listen for Enter key events globally to trigger search
   */
  @HostListener('window:keydown.enter', ['$event'])
  onWindowKeyDown(event: KeyboardEvent): void {
    // Only trigger search if focus is not in a dialog
    if (!this.rejectReasonModalVisible) {
      this.search();
    }
  }

  rejectReasonId: string | null = null;
  rejectReasonDescription: string | null = null;
  rejectReasonOptions: Array<{ label: string; value: string }> = [];
  rejectReasonOptionsLoading = false;
  pendingRejectPolicies: PolicyApprovalRow[] = [];
  actionLoading = false;
  exporting = false;

  constructor(
    private policyRequestApprovalService: PolicyRequestApprovalService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private permissionService: PermissionService,
    private localizationService: LocalizationService,
    private router: Router,
    private route: ActivatedRoute,
    private reasonService: ResReasonService,
    private channelService: ResChannelService,
    private customerService: ResCustomerService,
    private partnerService: ResPartnerService,
    private employeeService: HrEmployeeService,
    private policyTypeService: PolicyTypeService,
    private policyContractService: PolicyContractService,
    private carBrandService: ResCarBrandService,
    private carCategoryService: ResCarCategoryService,
    private destroyRef: DestroyRef
  ) {
    this.initializeColumns();
    this.initializeActions();
    this.initializeStatusOptions();
    this.initializeApprovalStatusOptions();
    this.initializeContractTypeOptions();
    this.initializeContractStatusOptions();
    this.initializeApprovalTypeOptions();
  }

  ngOnInit(): void {
    this.applyRouteParamsToSearchForm();
    this.loadDropdownOptions();
    this.route.queryParamMap.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      this.applyRouteParamsToSearchForm();
      if (this.currentLazyLoadEvent) {
        this.search();
      }
    });
  }

  /** Route `data.approvalType` (terminate / endorsement) hoặc query `approvalType`, `approvalStatus` (pending|approved|rejected). */
  private applyRouteParamsToSearchForm(): void {
    const fromData = this.route.snapshot.data['approvalType'] as string | undefined;
    const q = this.route.snapshot.queryParamMap;
    const approvalType = fromData || q.get('approvalType') || undefined;
    if (approvalType === 'terminate') {
      this.searchForm.approvalTypeFilter = 'terminate';
    } else if (approvalType === 'endorsement') {
      this.searchForm.approvalTypeFilter = 'endorsement';
    } else if (approvalType === 'request') {
      this.searchForm.approvalTypeFilter = 'request';
    } else {
      this.searchForm.approvalTypeFilter = 'all';
    }
    const rawStatus = (q.get('approvalStatus') || '').trim().toLowerCase();
    if (rawStatus === 'pending' || rawStatus === 'approved' || rawStatus === 'rejected') {
      this.searchForm.approvalStatus = rawStatus;
    } else {
      this.searchForm.approvalStatus = null;
    }
  }

  private getEffectiveBusinessCodes(): string[] {
    switch (this.searchForm.approvalTypeFilter) {
      case 'request':
        return [BUSINESS_CODE_CREATE];
      case 'terminate':
        return [BUSINESS_CODE_TERMINATE];
      case 'endorsement':
        return [BUSINESS_CODE_ENDORSEMENT];
      default:
        return [];
    }
  }

  private formatApprovalType(businessCode: string | undefined): string {
    if (!businessCode) return '-';
    if (businessCode === BUSINESS_CODE_TERMINATE) return this.localizationService.localize('Policy::Menu:TerminateApproval');
    if (businessCode === BUSINESS_CODE_ENDORSEMENT) return this.localizationService.localize('Policy::Menu:EndorsementApproval');
    if (businessCode === BUSINESS_CODE_CREATE) return this.localizationService.localize('Policy::Menu:RequestApproval');
    return businessCode;
  }

  canApproveRow(row: PolicyApprovalRow): boolean {
    const status = row.workTaskStatus;
    const isWaitApprove = status === WorkTaskStatus.WaitApprove || Number(status) === WorkTaskStatus.WaitApprove;
    if (!isWaitApprove) return false;
    const code = row.businessCode;
    if (code === BUSINESS_CODE_TERMINATE) return this.permissionService.isGranted(this.PERMISSIONS.TERMINATE_APPROVE);
    if (code === BUSINESS_CODE_ENDORSEMENT) return this.permissionService.isGranted(this.PERMISSIONS.ENDORSEMENT_APPROVE);
    return this.permissionService.isGranted(this.PERMISSIONS.APPROVE);
  }

  /** Bound for v-table isRowSelectable (only wait_approve rows are selectable). */
  get isRowSelectableFn(): (row: PolicyApprovalRow) => boolean {
    return (row) => this.canApproveRow(row);
  }

  private initializeColumns(): void {
    this.columns = [
      { field: 'businessCode', header: this.localizationService.localize('Policy::PolicyRequestApproval:ApprovalType'), sortable: false, width: '140px', freeze: 'left', formatter: (v: string) => this.formatApprovalType(v) },
      { field: 'contractNo', header: this.localizationService.localize('Policy::Policy:ContractNo'), sortable: true, width: '150px' },
      { field: 'policyNo', header: this.localizationService.localize('Policy::Policy:PolicyNo'), sortable: true, width: '150px' },
      { field: 'certificateNo', header: this.localizationService.localize('Policy::Policy:CertificateNo'), sortable: true, width: '150px' },
      { field: 'productName', header: this.localizationService.localize('Policy::Policy:ProductName'), sortable: true, width: '200px' },
      { field: 'customerName', header: this.localizationService.localize('Policy::Policy:CustomerName'), sortable: true, width: '200px' },
      { field: 'orgEffectDate', header: this.localizationService.localize('Policy::Policy:OrgEffectDate'), sortable: true, type: 'date', width: '130px' },
      { field: 'orgExpireDate', header: this.localizationService.localize('Policy::Policy:OrgExpireDate'), sortable: true, type: 'date', width: '130px' },
      { field: 'vehiclePlate', header: this.localizationService.localize('Policy::Policy:VehiclePlate'), sortable: true, width: '120px' },
      { field: 'chassisEngine', header: this.localizationService.localize('Policy::Policy:ChassisEngine'), sortable: false, width: '180px', formatter: (_, row: any) => `${row.chassisNumber || ''}/${row.engineNumber || ''}`.replace(/\/$|^\//, '') || '-' },
      { field: 'premiumTotal', header: this.localizationService.localize('Policy::Policy:PremiumTotal'), sortable: true, width: '150px', type: 'number', formatter: (v: any) => v ? new Intl.NumberFormat('vi-VN').format(v) : '' },
      { field: 'channelName', header: this.localizationService.localize('Policy::Policy:ChannelName'), sortable: true, width: '150px' },
      { field: 'partnerName', header: this.localizationService.localize('Policy::Policy:PartnerName'), sortable: true, width: '200px' },
      { field: 'implementerName', header: this.localizationService.localize('Policy::Policy:ImplementerName'), sortable: true, width: '150px' },
      {
        field: 'policyVersionStatus',
        header: this.localizationService.localize('Policy::Policy:Status'),
        sortable: true,
        type: 'text',
        width: '120px',
        freeze: 'right',
        align: 'center',
        formatter: (v: any, row: any) => this.formatStatusForVersion(v, row),
        cellClass: (value: any, row: any) => {
          const statusValue = this.normalizeVersionStatusToPolicyStatus(value, row?.status);
          if (statusValue === PolicyStatus.Active) {
            return 'px-2 py-1 rounded text-xs font-semibold bg-green-100 text-green-800';
          } else if (statusValue === PolicyStatus.Expired || statusValue === PolicyStatus.Cancelled || statusValue === PolicyStatus.Terminated) {
            return 'px-2 py-1 rounded text-xs font-semibold bg-red-100 text-red-800';
          } else {
            return 'px-2 py-1 rounded text-xs font-semibold bg-gray-100 text-gray-800';
          }
        }
      },
      {
        field: 'workTaskStatus',
        header: this.localizationService.localize('Policy::Policy:ApprovalStatus'),
        sortable: true,
        width: '150px',
        freeze: 'right',
        align: 'center',
        formatter: (value: any, row: any) => this.getApprovalStatusDisplayLabel(row?.businessCode === BUSINESS_CODE_TERMINATE ? row?.terminationStatus : undefined, value),
        cellClass: (value: any, row: any) => this.getApprovalStatusCellClass(row?.businessCode === BUSINESS_CODE_TERMINATE ? row?.terminationStatus : undefined, value)
      },
      { field: 'approvalDate', header: this.localizationService.localize('Policy::Policy:ApprovalDate'), sortable: true, type: 'date', width: '130px' },
      { field: 'creationTime', header: this.localizationService.localize('AbpIdentity::CreationTime'), sortable: true, type: 'date', width: '130px' },
      { field: 'creatorName', header: this.localizationService.localize('Policy::Policy:CreatorName'), sortable: true, width: '150px' }
    ];
  }

  private initializeActions(): void {
    this.actions = [
      { label: this.localizationService.localize('Policy::Policy:View'), icon: 'pi pi-eye', command: (row) => this.openViewPage(row) },
      { label: this.localizationService.localize('Policy::Policy:Approve'), icon: 'pi pi-check', command: (row) => this.approveOne(row), visible: (row) => this.canApproveRow(row) },
      { label: this.localizationService.localize('Policy::Policy:Reject'), icon: 'pi pi-times', command: (row) => this.openRejectReasonForRows([row]), visible: (row) => this.canApproveRow(row) }
    ];
  }

  private initializeStatusOptions(): void {
    this.statusOptions = [
      { label: this.localizationService.localize('Policy::Policy:Quotation'), value: PolicyStatus.Quotation },
      { label: this.localizationService.localize('Policy::Policy:Draft'), value: PolicyStatus.Draft },
      { label: this.localizationService.localize('Policy::Policy:Active'), value: PolicyStatus.Active },
      { label: this.localizationService.localize('Policy::Policy:Expired'), value: PolicyStatus.Expired },
      { label: this.localizationService.localize('Policy::Policy:Cancelled'), value: PolicyStatus.Cancelled },
      { label: this.localizationService.localize('Policy::Policy:Terminated'), value: PolicyStatus.Terminated }
    ];
  }

  private initializeApprovalStatusOptions(): void {
    this.approvalStatusOptions = [
      { label: this.localizationService.localize('Policy::Policy:ApprovalStatusPendingApproval'), value: 'pending' },
      { label: this.localizationService.localize('Policy::Policy:ApprovalStatusApproved'), value: 'approved' },
      { label: this.localizationService.localize('Policy::Policy:ApprovalStatusRejected'), value: 'rejected' }
    ];
  }

  private initializeContractTypeOptions(): void {
    this.contractTypeOptions = [
      { label: this.localizationService.localize('Policy::Policy:ContractTypeIndividual'), value: PolicyContractType.Individual },
      { label: this.localizationService.localize('Policy::Policy:ContractTypeGroup'), value: PolicyContractType.Group }
    ];
  }

  private initializeApprovalTypeOptions(): void {
    this.approvalTypeOptions = [
      { label: this.localizationService.localize('Policy::PolicyRequestApproval:AllTypes'), value: 'all' },
      { label: this.localizationService.localize('Policy::Menu:RequestApproval'), value: 'request' },
      { label: this.localizationService.localize('Policy::Menu:TerminateApproval'), value: 'terminate' },
      { label: this.localizationService.localize('Policy::Menu:EndorsementApproval'), value: 'endorsement' }
    ];
  }

  private initializeContractStatusOptions(): void {
    this.contractStatusOptions = [
      { label: this.localizationService.localize('Policy::Policy:ContractStatusQuotation'), value: PolicyContractStatus.Quotation },
      { label: this.localizationService.localize('Policy::Policy:ContractStatusDraft'), value: PolicyContractStatus.Draft },
      { label: this.localizationService.localize('Policy::Policy:ContractStatusActive'), value: PolicyContractStatus.Active },
      { label: this.localizationService.localize('Policy::Policy:ContractStatusExpired'), value: PolicyContractStatus.Expired },
      { label: this.localizationService.localize('Policy::Policy:ContractStatusTerminated'), value: PolicyContractStatus.Terminated },
      { label: this.localizationService.localize('Policy::Policy:ContractStatusCancelled'), value: PolicyContractStatus.Cancelled }
    ];
  }

  private loadRejectReasonOptionsForRows(rows: PolicyApprovalRow[]): void {
    const groupCodes = new Set<string>();
    for (const row of rows || []) {
      const code = getReasonGroupCodeForBusinessCode(row.businessCode);
      if (code) groupCodes.add(code);
    }
    this.rejectReasonOptions = [];
    this.rejectReasonOptionsLoading = true;
    const codes = Array.from(groupCodes);
    if (codes.length === 0) {
      this.rejectReasonOptionsLoading = false;
      this.rejectReasonModalVisible = true;
      return;
    }
    const toOptions = (result: any[]) => (result || []).map((r: any) => ({ label: r.name || '', value: r.id || '' }));
    const onDone = (options: Array<{ label: string; value: string }>) => {
      this.rejectReasonOptions = options;
      this.rejectReasonOptionsLoading = false;
      this.rejectReasonModalVisible = true;
    };
    const onError = () => {
      this.rejectReasonOptions = [];
      this.rejectReasonOptionsLoading = false;
      this.messageService.add({ severity: 'warn', summary: this.localizationService.localize('AbpUi::Warning'), detail: this.localizationService.localize('Policy::PolicyRequestApproval:RejectReasonLoadFailed') });
      this.rejectReasonModalVisible = true;
    };
    if (codes.length === 1) {
      this.reasonService.getSelectListByGroupCode(codes[0]).subscribe({
        next: (result: any) => onDone(toOptions(result || [])),
        error: onError
      });
      return;
    }
    forkJoin(codes.map((code) => this.reasonService.getSelectListByGroupCode(code))).subscribe({
      next: (results: any[]) => {
        const seen = new Set<string>();
        const merged: Array<{ id: string; name: string }> = [];
        for (const list of results) {
          for (const r of list || []) {
            const id = r.id || '';
            if (id && !seen.has(id)) {
              seen.add(id);
              merged.push({ id, name: r.name || '' });
            }
          }
        }
        onDone(merged.map((r) => ({ label: r.name, value: r.id })));
      },
      error: onError
    });
  }

  private loadDropdownOptions(): void {
    this.channelService.getList({ maxResultCount: 500, skipCount: 0 }).subscribe(res => {
      this.channelOptions = (res.items || []).filter((c: any) => c.status === 0).map((c: any) => ({ label: c.name || c.code || '', value: c.id }));
    });
    this.loadCustomers();
    this.partnerService.getList({ maxResultCount: 500, skipCount: 0 }).subscribe(res => {
      const partners = (res.items || []).filter((p: any) => p.status === 0).map((p: any) => ({
        label: p.name || p.code || '',
        value: p.id,
        code: p.code || ''
      }));
      this.partnerOptions = partners;
      this.primaryInsurancePartnerOptions = partners.map(p => ({ label: p.label, value: p.value, code: p.code }));
    });
    this.employeeService.getList({ maxResultCount: 500, skipCount: 0 }).subscribe(res => {
      this.implementerOptions = (res.items || []).filter((e: any) => e.status === 0).map((e: any) => ({
        label: e.fullName || e.code || '',
        value: e.id,
        employeeCode: e.code || ''
      }));
    });
    this.policyTypeService.getList({ maxResultCount: 500, skipCount: 0 }).subscribe(res => {
      this.policyTypeOptions = (res.items || []).filter((t: any) => t.status === 0).map((t: any) => ({ label: t.name || t.code || '', value: t.id }));
    });
    this.policyContractService.getList({ maxResultCount: 500, skipCount: 0 }).subscribe(res => {
      this.contractOptions = (res.items || []).map((c: any) => ({ label: c.code || c.name || c.id, value: c.id }));
    });
  }

  private loadCustomers(filterText?: string): void {
    const params: { status?: number; maxResultCount: number; skipCount: number; sorting: string; filter?: string } = {
      status: 0,
      maxResultCount: 50,
      skipCount: 0,
      sorting: 'name asc'
    };
    const trimmed = filterText?.trim();
    if (trimmed) {
      params.filter = trimmed;
    }
    this.customerService.getList(params as any).subscribe({
      next: (result: any) => {
        this.customerOptions = (result.items || []).map((customer: any) => ({
          label: customer.name || '',
          value: customer.id || '',
          phone: customer.phone || '',
          email: customer.email || '',
          idNo: customer.idNo || '',
          address: customer.fullAddress || customer.address || ''
        }));
      }
    });
  }

  onCustomerFilter(event: { filter?: string; value?: string }): void {
    const keyword = (event?.filter ?? event?.value ?? '') as string;
    this.loadCustomers(keyword);
  }

  loadData(event: TableLazyLoadEvent): void {
    this.currentLazyLoadEvent = event;
    this.loading = true;
    const businessCodes = this.getEffectiveBusinessCodes();
    const input: GetPolicyRequestApprovalListInput = {
      skipCount: event.first || 0,
      maxResultCount: event.rows || 10,
      sorting: this.getSortingString(event) ?? 'WorkTaskStatus',
      businessCodes,
      policyNo: this.searchForm.policyNo || undefined,
      contractId: this.searchForm.contractId || undefined,
      policyTypeId: this.searchForm.policyTypeId || undefined,
      partnerId: this.searchForm.partnerId || undefined,
      status: this.searchForm.status ?? undefined,
      approvalStatus: this.searchForm.approvalStatus || undefined,
      channelId: this.searchForm.channelId || undefined,
      customerId: this.searchForm.customerId || undefined,
      contractType: this.searchForm.contractType != null ? String(this.searchForm.contractType) : undefined,
      contractStatus: this.searchForm.contractStatus != null ? String(this.searchForm.contractStatus) : undefined,
      implementerId: this.searchForm.implementerId || undefined,
      certificateNo: this.searchForm.certificateNo || undefined,
      effectiveDateFrom: this.searchForm.effectiveDateFrom ? this.toIsoDate(this.searchForm.effectiveDateFrom) : undefined,
      effectiveDateTo: this.searchForm.effectiveDateTo ? this.toIsoDate(this.searchForm.effectiveDateTo) : undefined,
      expiryDateFrom: this.searchForm.expiryDateFrom ? this.toIsoDate(this.searchForm.expiryDateFrom) : undefined,
      expiryDateTo: this.searchForm.expiryDateTo ? this.toIsoDate(this.searchForm.expiryDateTo) : undefined,
      carPlate: this.searchForm.carPlate
        ? this.searchForm.carPlate.replace(/[^a-zA-Z0-9]/g, '')
        : undefined,
      carVin: this.searchForm.carVin || undefined,
      carEngineNumber: this.searchForm.carEngineNumber || undefined,
      primaryInsurancePartnerId: this.searchForm.primaryInsurancePartnerId || undefined,
      importLotNumber: this.searchForm.importLotNumber || undefined
    };
    this.policyRequestApprovalService.getList(input).subscribe({
      next: (result) => {
        const items = result.items || [];
        this.policies = items.map((item: PolicyRequestApprovalItemDto) => ({
          ...item.policy,
          businessCode: item.businessCode,
          workTaskId: item.workTaskId,
          workTaskStatus: item.workTaskStatus,
          policyVersionStatus: item.policyVersionStatus ?? undefined,
          policyVersionId: item.policyVersionId != null ? String(item.policyVersionId) : undefined
        }));
        this.totalCount = result.totalCount || 0;
        this.loading = false;
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: this.localizationService.localize('AbpUi::Error'), detail: this.localizationService.localize('Policy::InternalServerErrorMessage') });
        this.loading = false;
      }
    });
  }

  private getSortingString(event: TableLazyLoadEvent): string | undefined {
    const field = event.sortField == null ? undefined : Array.isArray(event.sortField) ? event.sortField[0] : event.sortField;
    if (field == null || field === '') return undefined;
    const order = event.sortOrder === 1 ? 'asc' : 'desc';
    return order === 'desc' ? `-${field}` : field;
  }

  private toIsoDate(d: Date): string {
    return d instanceof Date ? d.toISOString() : String(d);
  }

  search(): void {
    if (this.currentLazyLoadEvent) {
      this.currentLazyLoadEvent.first = 0;
      this.loadData(this.currentLazyLoadEvent);
    }
  }

  formatStatus(value: any): string {
    const v = typeof value === 'number' ? value : 0;
    const map: Record<number, string> = {
      [PolicyStatus.Quotation]: this.localizationService.localize('Policy::Policy:Quotation'),
      [PolicyStatus.Draft]: this.localizationService.localize('Policy::Policy:Draft'),
      [PolicyStatus.Active]: this.localizationService.localize('Policy::Policy:Active'),
      [PolicyStatus.Expired]: this.localizationService.localize('Policy::Policy:Expired'),
      [PolicyStatus.Cancelled]: this.localizationService.localize('Policy::Policy:Cancelled'),
      [PolicyStatus.Terminated]: this.localizationService.localize('Policy::Policy:Terminated')
    };
    return map[v] ?? '-';
  }

  /** True when value is an approval outcome (pending, approved, rejected), not a policy lifecycle status. */
  private isApprovalOnlyStatus(value: string | number | undefined): boolean {
    if (value === undefined || value === null) return false;
    const s = String(value).trim().toLowerCase();
    return s === 'pending' || s === 'approved' || s === 'rejected';
  }

  /** Normalize policy version status (string or number) to PolicyStatus enum number for styling. When value is approval-only, returns fallbackStatus if valid (0–5) else -1. */
  private normalizeVersionStatusToPolicyStatus(value: string | number | undefined, fallbackStatus?: number): number {
    if (value === undefined || value === null) return 0;
    if (typeof value === 'number') return value;
    const s = String(value).trim().toLowerCase();
    if (this.isApprovalOnlyStatus(value)) {
      if (fallbackStatus != null && fallbackStatus >= 0 && fallbackStatus <= 5) return fallbackStatus;
      return -1;
    }
    const byName: Record<string, number> = {
      quotation: PolicyStatus.Quotation,
      draft: PolicyStatus.Draft,
      active: PolicyStatus.Active,
      expired: PolicyStatus.Expired,
      cancelled: PolicyStatus.Cancelled,
      terminated: PolicyStatus.Terminated
    };
    if (byName[s] !== undefined) return byName[s];
    const n = parseInt(String(value).trim(), 10);
    if (!Number.isNaN(n) && n >= 0 && n <= 5) return n;
    return 0;
  }

  /** Format policy version status (string or number) for display in the status column. When value is approval-only (e.g. rejected), uses row.status so the column shows policy lifecycle status. */
  formatStatusForVersion(value: string | number | undefined, row?: PolicyApprovalRow): string {
    const normalized = this.normalizeVersionStatusToPolicyStatus(value, row?.status);
    return normalized >= 0 ? this.formatStatus(normalized) : '-';
  }

  formatApprovalStatus(status: string | undefined | null): string {
    if (status == null || status === '') return '-';
    const s = (status || '').toString().toLowerCase();
    if (s === 'approved') return this.localizationService.localize('Policy::Policy:ApprovalStatusApproved');
    if (s === 'rejected') return this.localizationService.localize('Policy::Policy:ApprovalStatusRejected');
    if (s === 'pending') return this.localizationService.localize('Policy::Policy:ApprovalStatusPendingApproval');
    return status;
  }

  formatWorkTaskStatusAsApproval(status: WorkTaskStatus | number | undefined | null): string {
    if (status == null || status === undefined) return '-';
    const s = typeof status === 'number' ? status : -1;
    if (s === WorkTaskStatus.Approved) return this.localizationService.localize('Policy::Policy:ApprovalStatusApproved');
    if (s === WorkTaskStatus.Rejected) return this.localizationService.localize('Policy::Policy:ApprovalStatusRejected');
    if (s === WorkTaskStatus.WaitApprove) return this.localizationService.localize('Policy::Policy:ApprovalStatusPendingApproval');
    return '-';
  }

  /** Display label for approval column: when terminationStatus is set, show termination labels; when Pending, derive from workTaskStatus. */
  getApprovalStatusDisplayLabel(terminationStatus: PolicyTerminationStatus | number | string | undefined | null, workTaskStatus: WorkTaskStatus | number | undefined | null): string {
    const term = this.normalizeTerminationStatus(terminationStatus);
    if (term === PolicyTerminationStatus.Pending) {
      const status = typeof workTaskStatus === 'number' ? workTaskStatus : -1;
      if (status === WorkTaskStatus.WaitApprove) return this.localizationService.localize('Policy::Policy:TerminationStatusPending', 'Chờ duyệt chấm dứt');
      if (status === WorkTaskStatus.Approved) return this.localizationService.localize('Policy::Policy:TerminationStatusApproved', 'Đã duyệt chấm dứt');
      if (status === WorkTaskStatus.Rejected) return this.localizationService.localize('Policy::Policy:TerminationStatusRejected', 'Từ chối chấm dứt');
      return this.localizationService.localize('Policy::Policy:TerminationStatusPending', 'Chờ duyệt chấm dứt');
    }
    if (term === PolicyTerminationStatus.Approved) return this.localizationService.localize('Policy::Policy:TerminationStatusApproved', 'Đã duyệt chấm dứt');
    if (term === PolicyTerminationStatus.Rejected) return this.localizationService.localize('Policy::Policy:TerminationStatusRejected', 'Từ chối chấm dứt');
    return this.formatWorkTaskStatusAsApproval(workTaskStatus);
  }

  /** Cell CSS class for approval column (termination-aware). When Pending, style by workTaskStatus. */
  getApprovalStatusCellClass(terminationStatus: PolicyTerminationStatus | number | string | undefined | null, workTaskStatus: WorkTaskStatus | number | undefined | null): string {
    const base = 'px-2 py-1 rounded text-xs font-semibold ';
    const term = this.normalizeTerminationStatus(terminationStatus);
    if (term === PolicyTerminationStatus.Pending) {
      const status = typeof workTaskStatus === 'number' ? workTaskStatus : -1;
      if (status === WorkTaskStatus.Approved) return base + 'bg-green-100 text-green-800';
      if (status === WorkTaskStatus.Rejected) return base + 'bg-red-100 text-red-800';
      return base + 'bg-amber-100 text-amber-800';
    }
    if (term === PolicyTerminationStatus.Approved) return base + 'bg-green-100 text-green-800';
    if (term === PolicyTerminationStatus.Rejected) return base + 'bg-red-100 text-red-800';
    const status = typeof workTaskStatus === 'number' ? workTaskStatus : -1;
    if (status === WorkTaskStatus.Approved) return base + 'bg-green-100 text-green-800';
    if (status === WorkTaskStatus.Rejected) return base + 'bg-red-100 text-red-800';
    if (status === WorkTaskStatus.WaitApprove) return base + 'bg-amber-100 text-amber-800';
    return base + 'bg-gray-100 text-gray-800';
  }

  private normalizeTerminationStatus(v: PolicyTerminationStatus | number | string | undefined | null): PolicyTerminationStatus | null {
    if (v === undefined || v === null || v === '') return null;
    if (typeof v === 'number' && (v === 0 || v === 1 || v === 2)) return v as PolicyTerminationStatus;
    const s = String(v).trim().toLowerCase();
    if (s === 'pending' || s === '0') return PolicyTerminationStatus.Pending;
    if (s === 'approved' || s === '1') return PolicyTerminationStatus.Approved;
    if (s === 'rejected' || s === '2') return PolicyTerminationStatus.Rejected;
    return null;
  }

  openViewPage(row: PolicyApprovalRow): void {
    const workTaskId = (row?.workTaskId || '').trim();
    if (!workTaskId) return;
    const versionId = (row?.policyVersionId || '').trim() || undefined;
    this.router.navigate(['/pages/policy/policy-request-approval/detail', workTaskId], {
      queryParams: versionId ? { versionId } : {}
    });
  }

  approveOne(row: PolicyApprovalRow): void {
    const workTaskId = (row?.workTaskId || '').trim();
    if (!workTaskId) return;
    this.confirmationService.confirm({
      message: this.localizationService.localize('Policy::PolicyRequestApproval:ConfirmApprove'),
      header: this.localizationService.localize('Policy::Policy:Approve'),
      icon: 'pi pi-check',
      acceptLabel: this.localizationService.localize('AbpUi::Yes'),
      rejectLabel: this.localizationService.localize('AbpUi::No'),
      accept: () => this.doApprove([row])
    });
  }

  hasAnyApprovableSelected(): boolean {
    return this.selectedPolicies.some((row) => this.canApproveRow(row));
  }

  bulkApprove(): void {
    const list = this.selectedPolicies?.length ? this.selectedPolicies : [];
    if (list.length === 0) {
      this.messageService.add({ severity: 'warn', summary: this.localizationService.localize('AbpUi::Warning'), detail: this.localizationService.localize('Policy::PolicyRequestApproval:SelectAtLeastOne') });
      return;
    }
    this.confirmationService.confirm({
      message: this.localizationService.localize('Policy::PolicyRequestApproval:ConfirmApprove'),
      header: this.localizationService.localize('Policy::Policy:Approve'),
      icon: 'pi pi-check',
      acceptLabel: this.localizationService.localize('AbpUi::Yes'),
      rejectLabel: this.localizationService.localize('AbpUi::No'),
      accept: () => this.doApprove(list)
    });
  }

  private doApprove(list: PolicyApprovalRow[]): void {
    const workTaskIds = list.map((p) => (p?.workTaskId ?? '').trim()).filter((id) => id);
    if (workTaskIds.length === 0) {
      this.actionLoading = false;
      return;
    }
    this.actionLoading = true;
    if (workTaskIds.length > 1) {
      this.policyRequestApprovalService.approveBatch({ workTaskIds }).subscribe({
        next: () => {
          this.actionLoading = false;
          this.selectedPolicies = [];
          this.messageService.add({ severity: 'success', summary: this.localizationService.localize('AbpUi::Success'), detail: this.localizationService.localize('Policy::PolicyRequestApproval:ApprovedSuccess') });
          if (this.currentLazyLoadEvent) this.loadData(this.currentLazyLoadEvent);
        },
        error: () => {
          this.actionLoading = false;
          this.messageService.add({ severity: 'error', summary: this.localizationService.localize('AbpUi::Error'), detail: this.localizationService.localize('Policy::PolicyRequestApproval:ApproveFailed') });
        }
      });
    } else {
      this.policyRequestApprovalService.approve(workTaskIds[0]).subscribe({
        next: () => {
          this.actionLoading = false;
          this.selectedPolicies = [];
          this.messageService.add({ severity: 'success', summary: this.localizationService.localize('AbpUi::Success'), detail: this.localizationService.localize('Policy::PolicyRequestApproval:ApprovedSuccess') });
          if (this.currentLazyLoadEvent) this.loadData(this.currentLazyLoadEvent);
        },
        error: () => {
          this.actionLoading = false;
          this.messageService.add({ severity: 'error', summary: this.localizationService.localize('AbpUi::Error'), detail: this.localizationService.localize('Policy::PolicyRequestApproval:ApproveFailed') });
        }
      });
    }
  }

  openRejectReasonForRows(rows: PolicyApprovalRow[]): void {
    this.pendingRejectPolicies = rows || [];
    this.rejectReasonId = null;
    this.rejectReasonDescription = null;
    this.loadRejectReasonOptionsForRows(rows);
  }

  bulkReject(): void {
    const list = this.selectedPolicies?.length ? this.selectedPolicies : [];
    if (list.length === 0) {
      this.messageService.add({ severity: 'warn', summary: this.localizationService.localize('AbpUi::Warning'), detail: this.localizationService.localize('Policy::PolicyRequestApproval:SelectAtLeastOne') });
      return;
    }
    this.openRejectReasonForRows(list);
  }

  confirmRejectReason(): void {
    if (!this.rejectReasonId) {
      this.messageService.add({ severity: 'warn', summary: this.localizationService.localize('AbpUi::Warning'), detail: this.localizationService.localize('Policy::PolicyRequestApproval:SelectReason') });
      return;
    }
    const trimmedDescription = (this.rejectReasonDescription || '').trim();
    if (!trimmedDescription) {
      this.messageService.add({ severity: 'warn', summary: this.localizationService.localize('AbpUi::Warning'), detail: this.localizationService.localize('Policy::PolicyRequestApproval:ReasonDescriptionRequired') });
      return;
    }
    this.rejectReasonModalVisible = false;
    const list = this.pendingRejectPolicies;
    this.confirmationService.confirm({
      message: this.localizationService.localize('Policy::PolicyRequestApproval:ConfirmReject'),
      header: this.localizationService.localize('Policy::Policy:Reject'),
      icon: 'pi pi-times',
      acceptLabel: this.localizationService.localize('AbpUi::Yes'),
      rejectLabel: this.localizationService.localize('AbpUi::No'),
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => this.doReject(list)
    });
  }

  private doReject(list: PolicyApprovalRow[]): void {
    const workTaskIds = list.map((p) => (p?.workTaskId ?? '').trim()).filter((id) => id);
    const reasonId = this.rejectReasonId ?? undefined;
    const reasonDescription = (this.rejectReasonDescription ?? '').trim();
    if (workTaskIds.length === 0) {
      this.actionLoading = false;
      return;
    }
    this.actionLoading = true;
    if (workTaskIds.length > 1) {
      this.policyRequestApprovalService.rejectBatch({ workTaskIds, reasonId, reasonDescription }).subscribe({
        next: () => {
          this.actionLoading = false;
          this.selectedPolicies = [];
          this.pendingRejectPolicies = [];
          this.messageService.add({ severity: 'success', summary: this.localizationService.localize('AbpUi::Success'), detail: this.localizationService.localize('Policy::PolicyRequestApproval:RejectedSuccess') });
          if (this.currentLazyLoadEvent) this.loadData(this.currentLazyLoadEvent);
        },
        error: () => {
          this.actionLoading = false;
          this.messageService.add({ severity: 'error', summary: this.localizationService.localize('AbpUi::Error'), detail: this.localizationService.localize('Policy::PolicyRequestApproval:RejectFailed') });
        }
      });
    } else {
      this.policyRequestApprovalService.reject(workTaskIds[0], { reasonId, reasonDescription }).subscribe({
        next: () => {
          this.actionLoading = false;
          this.selectedPolicies = [];
          this.pendingRejectPolicies = [];
          this.messageService.add({ severity: 'success', summary: this.localizationService.localize('AbpUi::Success'), detail: this.localizationService.localize('Policy::PolicyRequestApproval:RejectedSuccess') });
          if (this.currentLazyLoadEvent) this.loadData(this.currentLazyLoadEvent);
        },
        error: () => {
          this.actionLoading = false;
          this.messageService.add({ severity: 'error', summary: this.localizationService.localize('AbpUi::Error'), detail: this.localizationService.localize('Policy::PolicyRequestApproval:RejectFailed') });
        }
      });
    }
  }

  cancelRejectReason(): void {
    this.rejectReasonModalVisible = false;
    this.pendingRejectPolicies = [];
    this.rejectReasonOptionsLoading = false;
  }

  exportFile(): void {
    this.exporting = true;
    const businessCodes = this.getEffectiveBusinessCodes();
    const input: GetPolicyRequestApprovalListInput = {
      skipCount: 0,
      maxResultCount: 1000,
      sorting: this.currentLazyLoadEvent ? this.getSortingString(this.currentLazyLoadEvent) ?? 'WorkTaskStatus' : 'WorkTaskStatus',
      businessCodes,
      policyNo: this.searchForm.policyNo || undefined,
      contractId: this.searchForm.contractId || undefined,
      policyTypeId: this.searchForm.policyTypeId || undefined,
      partnerId: this.searchForm.partnerId || undefined,
      status: this.searchForm.status ?? undefined,
      approvalStatus: this.searchForm.approvalStatus || undefined,
      channelId: this.searchForm.channelId || undefined,
      customerId: this.searchForm.customerId || undefined,
      contractType: this.searchForm.contractType != null ? String(this.searchForm.contractType) : undefined,
      contractStatus: this.searchForm.contractStatus != null ? String(this.searchForm.contractStatus) : undefined,
      implementerId: this.searchForm.implementerId || undefined,
      certificateNo: this.searchForm.certificateNo || undefined,
      effectiveDateFrom: this.searchForm.effectiveDateFrom ? this.toIsoDate(this.searchForm.effectiveDateFrom) : undefined,
      effectiveDateTo: this.searchForm.effectiveDateTo ? this.toIsoDate(this.searchForm.effectiveDateTo) : undefined,
      expiryDateFrom: this.searchForm.expiryDateFrom ? this.toIsoDate(this.searchForm.expiryDateFrom) : undefined,
      expiryDateTo: this.searchForm.expiryDateTo ? this.toIsoDate(this.searchForm.expiryDateTo) : undefined,
      carPlate: this.searchForm.carPlate
        ? this.searchForm.carPlate.replace(/[^a-zA-Z0-9]/g, '')
        : undefined,
      carVin: this.searchForm.carVin || undefined,
      carEngineNumber: this.searchForm.carEngineNumber || undefined,
      primaryInsurancePartnerId: this.searchForm.primaryInsurancePartnerId || undefined,
      importLotNumber: this.searchForm.importLotNumber || undefined
    };
    this.policyRequestApprovalService.getList(input).subscribe({
      next: (result) => {
        try {
          const items = (result.items || []) as PolicyRequestApprovalItemDto[];
          if (items.length === 0) {
            this.messageService.add({ severity: 'warn', summary: this.localizationService.localize('AbpUi::Warning'), detail: this.localizationService.localize('Policy::PolicyRequestApproval:NoDataToExport') });
            this.exporting = false;
            return;
          }
          const flatRows: PolicyApprovalRow[] = items.map((item) => ({
            ...item.policy,
            businessCode: item.businessCode,
            workTaskId: item.workTaskId,
            workTaskStatus: item.workTaskStatus,
            policyVersionStatus: item.policyVersionStatus ?? undefined
          }));
          const exportColumns: ExportColumn[] = [
            { field: 'businessCode', header: this.localizationService.localize('Policy::PolicyRequestApproval:ApprovalType'), formatter: (v: string) => this.formatApprovalType(v) },
            { field: 'contractNo', header: this.localizationService.localize('Policy::Policy:ContractNo') },
            { field: 'policyNo', header: this.localizationService.localize('Policy::Policy:PolicyNo') },
            { field: 'certificateNo', header: this.localizationService.localize('Policy::Policy:CertificateNo') },
            { field: 'productName', header: this.localizationService.localize('Policy::Policy:ProductName') },
            { field: 'customerName', header: this.localizationService.localize('Policy::Policy:CustomerName') },
            { field: 'orgEffectDate', header: this.localizationService.localize('Policy::Policy:OrgEffectDate'), formatter: (v: any) => v ? new Date(v).toLocaleDateString('vi-VN') : '' },
            { field: 'orgExpireDate', header: this.localizationService.localize('Policy::Policy:OrgExpireDate'), formatter: (v: any) => v ? new Date(v).toLocaleDateString('vi-VN') : '' },
            { field: 'vehiclePlate', header: this.localizationService.localize('Policy::Policy:VehiclePlate') },
            { field: 'chassisNumber', header: this.localizationService.localize('Policy::Policy:ChassisEngine'), formatter: (_: any, row: any) => `${row.chassisNumber || ''}/${row.engineNumber || ''}`.replace(/\/$|^\//, '') || '-' },
            { field: 'premiumTotal', header: this.localizationService.localize('Policy::Policy:PremiumTotal'), formatter: (v: any) => v != null ? new Intl.NumberFormat('vi-VN').format(v) : '' },
            { field: 'channelName', header: this.localizationService.localize('Policy::Policy:ChannelName') },
            { field: 'partnerName', header: this.localizationService.localize('Policy::Policy:PartnerName') },
            { field: 'implementerName', header: this.localizationService.localize('Policy::Policy:ImplementerName') },
            { field: 'policyVersionStatus', header: this.localizationService.localize('Policy::Policy:Status'), formatter: (v: any, row: any) => this.formatStatusForVersion(v, row) },
            { field: 'workTaskStatus', header: this.localizationService.localize('Policy::Policy:ApprovalStatus'), formatter: (v: any, row: any) => this.getApprovalStatusDisplayLabel(row?.businessCode === BUSINESS_CODE_TERMINATE ? row?.terminationStatus : undefined, v) },
            { field: 'approvalDate', header: this.localizationService.localize('Policy::Policy:ApprovalDate'), formatter: (v: any) => v ? new Date(v).toLocaleDateString('vi-VN') : '' },
            { field: 'creationTime', header: this.localizationService.localize('AbpIdentity::CreationTime'), formatter: (v: any) => v ? new Date(v).toLocaleString('vi-VN') : '' },
            { field: 'creatorName', header: this.localizationService.localize('Policy::Policy:CreatorName') }
          ];
          exportToExcel(flatRows, exportColumns, 'PolicyRequestApproval');
          this.messageService.add({ severity: 'success', summary: this.localizationService.localize('AbpUi::Success'), detail: this.localizationService.localize('Policy::PolicyRequestApproval:ExportSuccess') });
        } catch (err) {
          this.messageService.add({ severity: 'error', summary: this.localizationService.localize('AbpUi::Error'), detail: this.localizationService.localize('Policy::PolicyRequestApproval:ExportFailed') });
        } finally {
          this.exporting = false;
        }
      },
      error: () => {
        this.exporting = false;
        this.messageService.add({ severity: 'error', summary: this.localizationService.localize('AbpUi::Error'), detail: this.localizationService.localize('Policy::PolicyRequestApproval:ExportFailed') });
      }
    });
  }
}
