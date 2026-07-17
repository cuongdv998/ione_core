import { Component, HostListener, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { DatePickerModule } from 'primeng/datepicker';
import { TableLazyLoadEvent } from 'primeng/table';
import { MessageService, ConfirmationService } from 'primeng/api';
import { Router } from '@angular/router';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { VTable } from '@/shared/components/v-table/v-table';
import { TableColumn } from '@/shared/models/table-column.model';
import { TableAction } from '@/shared/models/table-action.model';
import { PolicyContractSearchForm } from './policy-contract-list.models';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';
import { ProLineOfBusinessService } from '@/proxy/product/controllers/pro-line-of-business.service';
import { ResPartnerService } from '@/proxy/partner/controllers/res-partner.service';
import { ResCustomerService } from '@/proxy/customer/controllers/res-customer.service';
import { HrEmployeeService } from '@/proxy/hr/controllers/hr-employee.service';
import { PolicyContractType } from '@/proxy/policy-contracts/policy-contract-type.enum';
import { PolicyContractStatus } from '@/proxy/policy-contracts/policy-contract-status.enum';
import { PolicyContractService } from '@/proxy/policy/controllers/policy-contract.service';
import type { PolicyContractSearchResultDto } from '@/proxy/policy/policy-contracts/models';
import { environment } from '@environments/environment';
import { ContractTerminationModalComponent } from '../contract-termination-modal';

@Component({
  selector: 'app-policy-contract-list',
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
    VTable,
    TranslatePipe,
    ContractTerminationModalComponent
  ],
  templateUrl: './policy-contract-list.component.html',
  styleUrl: './policy-contract-list.component.scss',
  providers: [MessageService, ConfirmationService]
})
export class PolicyContractListComponent implements OnInit {
  contracts: PolicyContractSearchResultDto[] = [];
  totalCount = 0;
  loading = true;
  pageSize = 10;

  searchForm: PolicyContractSearchForm = {
    customerId: null,
    code: null,
    type: null,
    status: null,
    certificateCode: null,
    policyNo: null,
    sellerId: null,
    effectDateFrom: null,
    effectDateTo: null,
    expireDateFrom: null,
    expireDateTo: null,
    insurerId: null
  };

  customerOptions: Array<{ label: string; value: string }> = [];
  loadingCustomers = false;

  lobOptions: Array<{ label: string; value: string }> = [];
  loadingLobs = false;

  insurerOptions: Array<{ label: string; value: string }> = [];
  loadingInsurers = false;

  employeeOptions: Array<{ label: string; value: string }> = [];
  loadingEmployees = false;

  typeOptions: Array<{ label: string; value: PolicyContractType }> = [];
  statusOptions: Array<{ label: string; value: PolicyContractStatus }> = [];

  columns: TableColumn[] = [];
  actions: TableAction<PolicyContractSearchResultDto>[] = [];
  currentLazyLoadEvent?: TableLazyLoadEvent;
  terminateModalVisible = false;
  selectedContractId: string | null = null;
  /**
   * Listen for Enter key events globally to trigger search
   */
  @HostListener('window:keydown.enter', ['$event'])
  onWindowKeyDown(event: KeyboardEvent): void {
    // Only trigger search if focus is not in a dialog
    if (!this.terminateModalVisible) {
      this.search();
    }
  }


  constructor(
    private policyContractService: PolicyContractService,
    private http: HttpClient,
    private lobService: ProLineOfBusinessService,
    private partnerService: ResPartnerService,
    private customerService: ResCustomerService,
    private employeeService: HrEmployeeService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private localizationService: LocalizationService,
    private router: Router
  ) {
    this.initializeColumns();
    this.initializeActions();
    this.initializeOptions();
  }

  ngOnInit(): void {
    this.loadCustomers();
    this.loadLobs();
    this.loadInsurers();
    this.loadEmployees();
  }

  private initializeColumns(): void {
    this.columns = [
      {
        field: 'code',
        header: this.localizationService.localize('Policy::Policy:ContractNo'),
        sortable: true,
        width: '120px'
      },
      {
        field: 'name',
        header: this.localizationService.localize('Policy::Policy:ContractName'),
        sortable: false,
        width: '200px'
      },
      {
        field: 'lobId',
        header: this.localizationService.localize('Policy::Policy:LineOfBusiness'),
        sortable: false,
        width: '150px',
        formatter: (_, row) => this.getLobName(row?.lobId)
      },
      {
        field: 'type',
        header: this.localizationService.localize('Policy::Policy:ContractType'),
        sortable: false,
        width: '120px',
        formatter: (value: string) => this.formatType(value)
      },
      {
        field: 'insurerId',
        header: this.localizationService.localize('Policy::Policy:RootInsuranceName'),
        sortable: false,
        width: '120px',
        formatter: (_, row) => this.getInsurerName(row?.insurerId)
      },
      {
        field: 'customerId',
        header: this.localizationService.localize('Policy::Policy:Customer'),
        sortable: false,
        width: '180px',
        formatter: (_, row) => this.getCustomerName(row?.customerId)
      },
      {
        field: 'quantity',
        header: this.localizationService.localize('Policy::Policy:Quantity'),
        sortable: false,
        width: '90px',
        type: 'number'
      },
      {
        field: 'currentQuantity',
        header: this.localizationService.localize('Policy::Policy:CurrentQuantity'),
        sortable: false,
        width: '110px',
        type: 'number'
      },
      {
        field: 'effectDate',
        header: this.localizationService.localize('Policy::Policy:OrgEffectDate'),
        sortable: false,
        width: '120px'
      },
      {
        field: 'expireDate',
        header: this.localizationService.localize('Policy::Policy:OrgExpireDate'),
        sortable: false,
        width: '120px'
      },
      {
        field: 'status',
        header: this.localizationService.localize('Policy::Policy:ContractStatus'),
        sortable: false,
        width: '120px',
        freeze: 'right',
        align: 'center',
        formatter: (value: string) => this.formatStatus(value),
        cellClass: (value: string) => this.getStatusClass(value)
      },
      {
        field: 'creationTime',
        header: this.localizationService.localize('Policy::Policy:CreationDate'),
        sortable: false,
        width: '140px'
      },
      {
        field: 'creatorName',
        header: this.localizationService.localize('Policy::Policy:CreatorName'),
        sortable: false,
        width: '150px'
      }
    ];
  }

  private initializeActions(): void {
    this.actions.push({
      label: this.localizationService.localize('Policy::Policy:Detail'),
      icon: 'pi pi-eye',
      command: (row) => this.viewContract(row)
    });

    this.actions.push({
      label: this.localizationService.localize('Policy::Policy:TerminateContract'),
      icon: 'pi pi-stop',
      visible: (row) => this.getStatusLower(row?.status) === 'active',
      command: (row) => this.terminateContract(row)
    });

    this.actions.push({
      label: this.localizationService.localize('Policy::PolicyContract:Edit'),
      icon: 'pi pi-pencil',
      visible: (row) => this.getStatusLower(row?.status) !== 'cancelled',
      command: (row) => this.editContract(row)
    });

    this.actions.push({
      label: this.localizationService.localize('Policy::PolicyContract:CancelContract'),
      icon: 'pi pi-times',
      visible: (row) => this.getStatusLower(row?.status) !== 'cancelled',
      command: (row) => this.cancelContract(row)
    });
  }

  private getStatusLower(status?: string | number): string {
    if (status == null) return '';
    return String(status).toLowerCase();
  }

  private initializeOptions(): void {
    this.typeOptions = [
      { label: this.localizationService.localize('Policy::Policy:ContractTypeIndividual'), value: PolicyContractType.Individual },
      { label: this.localizationService.localize('Policy::Policy:ContractTypeGroup'), value: PolicyContractType.Group }
    ];
    this.statusOptions = [
      { label: this.localizationService.localize('Policy::Policy:ContractStatusQuotation'), value: PolicyContractStatus.Quotation },
      { label: this.localizationService.localize('Policy::Policy:ContractStatusDraft'), value: PolicyContractStatus.Draft },
      { label: this.localizationService.localize('Policy::Policy:ContractStatusActive'), value: PolicyContractStatus.Active },
      { label: this.localizationService.localize('Policy::Policy:ContractStatusExpired'), value: PolicyContractStatus.Expired },
      { label: this.localizationService.localize('Policy::Policy:ContractStatusTerminated'), value: PolicyContractStatus.Terminated },
      { label: this.localizationService.localize('Policy::Policy:ContractStatusCancelled'), value: PolicyContractStatus.Cancelled }
    ];
  }

  private loadCustomers(): void {
    this.loadingCustomers = true;
    this.customerService.getList({ maxResultCount: 1000, sorting: 'name' }).subscribe({
      next: (result) => {
        this.customerOptions = (result.items || []).map(c => ({
          label: c.name || c.code || '',
          value: c.id || ''
        }));
        this.loadingCustomers = false;
      },
      error: () => {
        this.loadingCustomers = false;
      }
    });
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

  getLobName(lobId?: string): string {
    if (!lobId) return '';
    return this.lobOptions.find(o => o.value === lobId)?.label ?? '';
  }

  getInsurerName(insurerId?: string): string {
    if (!insurerId) return '';
    return this.insurerOptions.find(o => o.value === insurerId)?.label ?? '';
  }

  getCustomerName(customerId?: string): string {
    if (!customerId) return '';
    return this.customerOptions.find(o => o.value === customerId)?.label ?? '';
  }

  loadData(event: TableLazyLoadEvent): void {
    this.currentLazyLoadEvent = event;
    this.loading = true;

    const input = {
      skipCount: event.first || 0,
      maxResultCount: event.rows || 10,
      sorting: this.getSortingString(event),
      customerId: this.searchForm.customerId || undefined,
      code: this.searchForm.code || undefined,
      type: this.searchForm.type ?? undefined,
      status: this.searchForm.status ?? undefined,
      certificateCode: this.searchForm.certificateCode || undefined,
      policyNo: this.searchForm.policyNo || undefined,
      sellerId: this.searchForm.sellerId || undefined,
      effectDateFrom: this.searchForm.effectDateFrom ? this.formatDate(this.searchForm.effectDateFrom) : undefined,
      effectDateTo: this.searchForm.effectDateTo ? this.formatDate(this.searchForm.effectDateTo) : undefined,
      expireDateFrom: this.searchForm.expireDateFrom ? this.formatDate(this.searchForm.expireDateFrom) : undefined,
      expireDateTo: this.searchForm.expireDateTo ? this.formatDate(this.searchForm.expireDateTo) : undefined,
      insurerId: this.searchForm.insurerId || undefined
    };

    this.policyContractService.search(input).subscribe({
      next: (result) => {
        this.contracts = result.items || [];
        this.totalCount = result.totalCount || 0;
        this.loading = false;
      },
      error: () => {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Claim::Error'),
          detail: this.localizationService.localize('Policy::Policy:InternalServerErrorMessage')
        });
        this.loading = false;
      }
    });
  }

  private getSortingString(event: TableLazyLoadEvent): string | undefined {
    if (!event.sortField) return undefined;
    const sortOrder = event.sortOrder === 1 ? 'asc' : 'desc';
    const sortField = Array.isArray(event.sortField) ? event.sortField[0] : event.sortField;
    return `${sortField} ${sortOrder}`;
  }

  search(): void {
    if (this.currentLazyLoadEvent) {
      this.currentLazyLoadEvent.first = 0;
      this.loadData(this.currentLazyLoadEvent);
    }
  }

  resetSearch(): void {
    this.searchForm = {
      customerId: null,
      code: null,
      type: null,
      status: null,
      certificateCode: null,
      policyNo: null,
      sellerId: null,
      effectDateFrom: null,
      effectDateTo: null,
      expireDateFrom: null,
      expireDateTo: null,
      insurerId: null
    };
    this.search();
  }

  formatType(type?: string): string {
    if (!type) return '';
    const lower = (type || '').toLowerCase();
    if (lower === 'individual') return this.localizationService.localize('Policy::Policy:ContractTypeIndividual');
    if (lower === 'group') return this.localizationService.localize('Policy::Policy:ContractTypeGroup');
    return type;
  }

  formatStatus(status?: string): string {
    if (!status) return '';
    const lower = (status || '').toLowerCase();
    const map: Record<string, string> = {
      quotation: this.localizationService.localize('Policy::Policy:ContractStatusQuotation'),
      draft: this.localizationService.localize('Policy::Policy:ContractStatusDraft'),
      active: this.localizationService.localize('Policy::Policy:ContractStatusActive'),
      expired: this.localizationService.localize('Policy::Policy:ContractStatusExpired'),
      terminated: this.localizationService.localize('Policy::Policy:ContractStatusTerminated'),
      cancelled: this.localizationService.localize('Policy::Policy:ContractStatusCancelled')
    };
    return map[lower] ?? status;
  }

  getStatusClass(status?: string): string {
    if (!status) return '';
    const lower = (status || '').toLowerCase();
    const classMap: Record<string, string> = {
      quotation: 'px-2 py-1 rounded text-xs font-semibold bg-gray-100 text-gray-800',
      draft: 'px-2 py-1 rounded text-xs font-semibold bg-gray-100 text-gray-800',
      active: 'px-2 py-1 rounded text-xs font-semibold bg-green-100 text-green-800',
      expired: 'px-2 py-1 rounded text-xs font-semibold bg-orange-100 text-orange-800',
      terminated: 'px-2 py-1 rounded text-xs font-semibold bg-blue-100 text-blue-800',
      cancelled: 'px-2 py-1 rounded text-xs font-semibold bg-red-100 text-red-800'
    };
    return classMap[lower] ?? '';
  }

  formatDate(date: Date): string {
    if (!date) return '';
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  viewContract(row: PolicyContractSearchResultDto): void {
    if (!row?.id) return;
    this.router.navigate(['/pages/policy/contract', row.id]);
  }

  editContract(row: PolicyContractSearchResultDto): void {
    if (!row?.id) return;
    this.router.navigate(['/pages/policy/contract/edit', row.id]);
  }

  terminateContract(row: PolicyContractSearchResultDto): void {
    if (!row?.id) return;
    this.selectedContractId = row.id;
    this.terminateModalVisible = true;
  }

  onTerminateSubmitted(): void {
    this.terminateModalVisible = false;
    this.selectedContractId = null;
    if (this.currentLazyLoadEvent) {
      this.loadData(this.currentLazyLoadEvent);
    }
  }

  cancelContract(row: PolicyContractSearchResultDto): void {
    if (!row?.id) return;
    this.confirmationService.confirm({
      message: this.localizationService.localize('Policy::PolicyContract:CancelContractConfirmation'),
      header: this.localizationService.localize('Policy::PolicyContract:CancelContract'),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.policyContractService.cancel(row.id!).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: this.localizationService.localize('Claim::Success'),
              detail: this.localizationService.localize('Policy::PolicyContract:CancelSuccess')
            });
            if (this.currentLazyLoadEvent) {
              this.loadData(this.currentLazyLoadEvent);
            }
          },
          error: (err) => {
            this.messageService.add({
              severity: 'error',
              summary: this.localizationService.localize('Claim::Error'),
              detail: err?.error?.error?.message || this.localizationService.localize('Policy::Policy:InternalServerErrorMessage')
            });
          }
        });
      }
    });
  }

  addContract(): void {
    this.router.navigate(['/pages/policy/contract/create']);
  }

  exportFile(): void {
    const params: Record<string, string> = {
      skipCount: '0',
      maxResultCount: '10'
    };
    if (this.searchForm.customerId) params['customerId'] = this.searchForm.customerId;
    if (this.searchForm.code) params['code'] = this.searchForm.code;
    if (this.searchForm.type != null) params['type'] = String(this.searchForm.type);
    if (this.searchForm.status != null) params['status'] = String(this.searchForm.status);
    if (this.searchForm.certificateCode) params['certificateCode'] = this.searchForm.certificateCode;
    if (this.searchForm.policyNo) params['policyNo'] = this.searchForm.policyNo;
    if (this.searchForm.sellerId) params['sellerId'] = this.searchForm.sellerId;
    if (this.searchForm.effectDateFrom) params['effectDateFrom'] = this.formatDate(this.searchForm.effectDateFrom);
    if (this.searchForm.effectDateTo) params['effectDateTo'] = this.formatDate(this.searchForm.effectDateTo);
    if (this.searchForm.expireDateFrom) params['expireDateFrom'] = this.formatDate(this.searchForm.expireDateFrom);
    if (this.searchForm.expireDateTo) params['expireDateTo'] = this.formatDate(this.searchForm.expireDateTo);
    if (this.searchForm.insurerId) params['insurerId'] = this.searchForm.insurerId;
    const query = new URLSearchParams(params).toString();
    const apiConfig = environment.apis['Policy'] ?? environment.apis['default'];
    const apiUrl = (apiConfig as { url?: string })?.url ?? 'https://localhost:44360';
    const url = `${apiUrl}/api/policy/policy-contracts/export-excel?${query}`;
    this.http.get(url, { responseType: 'blob' }).subscribe({
      next: (blob: Blob) => {
        const objectUrl = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = objectUrl;
        a.download = `policy-contracts-${new Date().toISOString().slice(0, 10)}.xlsx`;
        a.click();
        window.URL.revokeObjectURL(objectUrl);
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Policy::Success'),
          detail: this.localizationService.localize('Policy::Policy:Policy:UpdatedSuccessfully')
        });
      },
      error: () => {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Policy::Error'),
          detail: this.localizationService.localize('Policy::Policy:InternalServerErrorMessage')
        });
      }
    });
  }
}
