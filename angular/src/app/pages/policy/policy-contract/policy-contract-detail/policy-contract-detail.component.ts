import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { DatePickerModule } from 'primeng/datepicker';
import { MessageService, ConfirmationService } from 'primeng/api';
import { Router, ActivatedRoute } from '@angular/router';
import { ToastModule } from 'primeng/toast';
import { CheckboxModule } from 'primeng/checkbox';
import { TextareaModule } from 'primeng/textarea';
import { TableModule, TableLazyLoadEvent } from 'primeng/table';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { of, Observable } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';

import { PolicyContractFormData } from '../policy-contract-create/policy-contract-create.models';
import { PolicyContractType } from '@/proxy/policy-contracts/policy-contract-type.enum';
import { PolicyContractStatus } from '@/proxy/policy-contracts/policy-contract-status.enum';
import { PolicyContractService } from '@/proxy/policy/controllers/policy-contract.service';
import { PolicyService } from '@/proxy/policy/controllers/policy.service';
import type { PolicyContractDto, UpdatePolicyContractDto } from '@/proxy/policy/policy-contracts/models';
import type { PolicyDto } from '@/proxy/policy/policies/models';
import { ResDocumentService } from '@/proxy/master/controllers/res-document.service';
import { ResDocumentTypeService } from '@/proxy/master/controllers/res-document-type.service';
import type { CreateResDocumentTypeDto } from '@/proxy/master/res-document-types/models';
import { ResDocumentTypeStatus } from '@/proxy/res-document-types/res-document-type-status.enum';
import { ProLineOfBusinessService } from '@/proxy/product/controllers/pro-line-of-business.service';
import { ResPartnerService } from '@/proxy/partner/controllers/res-partner.service';
import { ResCustomerService } from '@/proxy/customer/controllers/res-customer.service';
import { HrEmployeeService } from '@/proxy/hr/controllers/hr-employee.service';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';
import { ContractTerminationModalComponent } from '../contract-termination-modal';
import { ImportPolicyModalComponent } from '../import-policy-modal';

@Component({
  selector: 'app-policy-contract-detail',
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
    CheckboxModule,
    TextareaModule,
    TableModule,
    TooltipModule,
    ConfirmDialogModule,
    TranslatePipe,
    ContractTerminationModalComponent,
    ImportPolicyModalComponent
  ],
  templateUrl: './policy-contract-detail.component.html',
  styleUrl: './policy-contract-detail.component.scss',
  providers: [MessageService, ConfirmationService]
})
export class PolicyContractDetailComponent implements OnInit {
  readonly PolicyContractStatus = PolicyContractStatus;

  formData: PolicyContractFormData = this.getEmptyForm();
  contractFileName = '';
  contractDocumentUrl: string | null = null;
  uploadedContractFile: File | null = null;
  contractId: string | null = null;
  loadingContract = false;
  savingFile = false;

  lobOptions: Array<{ label: string; value: string }> = [];
  insurerOptions: Array<{ label: string; value: string }> = [];
  customerOptions: Array<{ label: string; value: string }> = [];
  employeeOptions: Array<{ label: string; value: string }> = [];
  typeOptions: Array<{ label: string; value: PolicyContractType }> = [];
  statusOptions: Array<{ label: string; value: PolicyContractStatus }> = [];

  policies: PolicyDto[] = [];
  totalPolicyCount = 0;
  loadingPolicies = false;
  policyPageSize = 10;
  policyFirst = 0;
  terminateModalVisible = false;

  @ViewChild('importPolicyModal') importPolicyModal?: ImportPolicyModalComponent;

  constructor(
    private policyContractService: PolicyContractService,
    private policyService: PolicyService,
    private lobService: ProLineOfBusinessService,
    private partnerService: ResPartnerService,
    private customerService: ResCustomerService,
    private employeeService: HrEmployeeService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private localizationService: LocalizationService,
    private router: Router,
    private resDocumentService: ResDocumentService,
    private resDocumentTypeService: ResDocumentTypeService,
    private route: ActivatedRoute
  ) {
    this.initializeOptions();
  }

  ngOnInit(): void {
    this.loadLobs();
    this.loadInsurers();
    this.loadCustomers();
    this.loadEmployees();
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.contractId = id;
      this.loadContract(id);
    }
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

  private getEmptyForm(): PolicyContractFormData {
    return {
      insurerId: null,
      insurerContractCode: '',
      name: '',
      effectDate: null,
      status: PolicyContractStatus.Draft,
      documentId: null,
      code: '',
      type: PolicyContractType.Group,
      expireDate: null,
      employeeId: null,
      isReciveInvoice: false,
      customerId: null,
      lobId: null,
      payerName: '',
      payerEmail: '',
      payerPhone: '',
      payerProvinceId: null,
      payerWardId: null,
      payerAddress: '',
      payerFullAddress: '',
      payerTin: '',
      quantity: 1,
      currentQuantity: 0,
      description: ''
    };
  }

  private loadLobs(): void {
    this.lobService.getSelectList().subscribe({
      next: (items) => {
        this.lobOptions = (items || []).map(lob => ({
          label: lob.name || '',
          value: lob.id || ''
        }));
      }
    });
  }

  private loadInsurers(): void {
    this.partnerService.getSelectList('INSURER').subscribe({
      next: (items) => {
        this.insurerOptions = (items || []).map(p => ({
          label: p.name || '',
          value: p.id || ''
        }));
      }
    });
  }

  private loadCustomers(): void {
    this.customerService.getList({ maxResultCount: 1000, sorting: 'name' }).subscribe({
      next: (result) => {
        this.customerOptions = (result.items || []).map(c => ({
          label: c.name || c.code || '',
          value: c.id || ''
        }));
      }
    });
  }

  private loadEmployees(): void {
    this.employeeService.getList({ maxResultCount: 1000, sorting: 'fullName' }).subscribe({
      next: (result) => {
        this.employeeOptions = (result.items || []).map(emp => ({
          label: emp.fullName || '',
          value: emp.id || ''
        }));
      }
    });
  }

  private loadContract(id: string): void {
    this.loadingContract = true;
    this.policyContractService.get(id).subscribe({
      next: (dto: PolicyContractDto) => {
        this.formData = {
          insurerId: dto.insurerId || null,
          insurerContractCode: dto.insurerContractCode || '',
          name: dto.name || '',
          effectDate: dto.effectDate ? this.parseDate(dto.effectDate) : null,
          status: (dto.status as PolicyContractStatus) ?? PolicyContractStatus.Draft,
          documentId: dto.documentId || null,
          code: dto.code || '',
          type: (dto.type as PolicyContractType) ?? PolicyContractType.Group,
          expireDate: dto.expireDate ? this.parseDate(dto.expireDate) : null,
          employeeId: dto.employeeId || null,
          isReciveInvoice: (dto.isReciveInvoice || '').toUpperCase() === 'Y',
          customerId: dto.customerId || null,
          lobId: dto.lobId || null,
          payerName: dto.payerName || '',
          payerEmail: dto.payerEmail || '',
          payerPhone: dto.payerPhone || '',
          payerProvinceId: dto.payerProvinceId || null,
          payerWardId: dto.payerWardId || null,
          payerAddress: dto.payerAddress || '',
          payerFullAddress: dto.payerFullAddress || '',
          payerTin: dto.payerTaxCode ?? '',
          quantity: dto.quantity ?? 1,
          currentQuantity: dto.currentQuantity ?? 0,
          description: dto.description || ''
        };
        this.loadingContract = false;
        if (dto.documentId) {
          this.loadDocumentInfo(dto.documentId);
        }
      },
      error: () => {
        this.loadingContract = false;
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Policy::Error'),
          detail: this.localizationService.localize('Policy::Policy:InternalServerErrorMessage')
        });
        this.close();
      }
    });
  }

  private loadDocumentInfo(documentId: string): void {
    this.resDocumentService.getSingleFile(documentId).subscribe({
      next: (file) => {
        this.contractFileName = file.fileName || this.localizationService.localize('Policy::Policy:UploadedFile');
        this.contractDocumentUrl = file.url || null;
      },
      error: () => {
        this.contractFileName = this.localizationService.localize('Policy::Policy:UploadedFile');
      }
    });
  }

  onContractFileSelect(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input?.files?.[0];
    this.contractFileName = file ? file.name : this.contractFileName;
    this.contractDocumentUrl = null;
    this.uploadedContractFile = file || null;
    this.formData.documentId = null;
    if (input) {
      input.value = '';
    }
  }

  removeContractFile(): void {
    this.contractFileName = '';
    this.contractDocumentUrl = null;
    this.uploadedContractFile = null;
    this.formData.documentId = null;
  }

  saveContractFile(): void {
    if (!this.contractId || !this.uploadedContractFile) {
      return;
    }

    this.savingFile = true;

    this.uploadFileAndGetId().subscribe({
      next: (documentId) => {
        this.formData.documentId = documentId;
        const dto = this.buildUpdateDto();

        this.policyContractService.update(this.contractId!, dto).subscribe({
          next: () => {
            this.savingFile = false;
            this.messageService.add({
              severity: 'success',
              summary: this.localizationService.localize('Policy::Success'),
              detail: this.localizationService.localize('Policy::Policy:Policy:UpdatedSuccessfully')
            });
            this.loadDocumentInfo(documentId);
            this.uploadedContractFile = null;
          },
          error: (error) => {
            this.savingFile = false;
            const rawMsg =
              error?.error?.error?.message ||
              error?.error?.error?.details ||
              this.localizationService.localize('Policy::Policy:InternalServerErrorMessage');
            this.messageService.add({
              severity: 'error',
              summary: this.localizationService.localize('Policy::Error'),
              detail: rawMsg
            });
          }
        });
      },
      error: (err) => {
        this.savingFile = false;
        console.error(err);
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Policy::Error'),
          detail: this.localizationService.localize('Policy::PolicyContract:UploadFailed') || 'Tải file thất bại.'
        });
      }
    });
  }

  private uploadFileAndGetId(): Observable<string> {
    const code = 'POLICY_CONTRACT';
    return this.resDocumentTypeService.getList({ code, maxResultCount: 1 }).pipe(
      switchMap((res) => {
        if (res.items && res.items.length > 0) {
          return of(res.items[0]);
        }
        const newType: CreateResDocumentTypeDto = {
          code,
          name: 'Policy Contract',
          status: ResDocumentTypeStatus.Active,
          bucket: 'policy-contract'
        };
        return this.resDocumentTypeService.create(newType);
      }),
      switchMap((docType) => {
        if (!docType.id) throw new Error('Document Type ID missing');
        return this.resDocumentService.uploadSingleFile(
          docType.id,
          code,
          this.uploadedContractFile!
        );
      }),
      map((res) => res.id!)
    );
  }

  private buildUpdateDto(): UpdatePolicyContractDto {
    return {
      insurerId: this.formData.insurerId?.trim() || undefined,
      insurerContractCode: this.formData.insurerContractCode?.trim() || undefined,
      lobId: this.formData.lobId?.trim() || undefined,
      name: this.formData.name.trim(),
      type: this.formData.type,
      customerId: this.formData.customerId!,
      effectDate: this.formData.effectDate ? this.formatDate(this.formData.effectDate) : '',
      expireDate: this.formData.expireDate ? this.formatDate(this.formData.expireDate) : undefined,
      quantity: this.formData.quantity,
      currentQuantity: this.formData.currentQuantity,
      status: this.formData.status,
      employeeId: this.formData.employeeId?.trim() || undefined,
      description: this.formData.description?.trim() || undefined,
      isReciveInvoice: this.formData.isReciveInvoice ? 'Y' : 'N',
      documentId: this.formData.documentId?.trim() || undefined,
      payerTaxCode: this.formData.payerTin?.trim() || undefined
    };
  }

  loadPolicies(event: TableLazyLoadEvent): void {
    if (!this.contractId) return;
    this.loadingPolicies = true;
    this.policyFirst = event.first ?? 0;
    const rows = event.rows ?? this.policyPageSize;

    this.policyService.getList({
      contractId: this.contractId,
      skipCount: this.policyFirst,
      maxResultCount: rows,
      sorting: event.sortField ? `${event.sortField} ${event.sortOrder === 1 ? 'asc' : 'desc'}` : undefined
    }).subscribe({
      next: (result) => {
        this.policies = result.items || [];
        this.totalPolicyCount = result.totalCount || 0;
        this.loadingPolicies = false;
      },
      error: () => {
        this.loadingPolicies = false;
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Policy::Error'),
          detail: this.localizationService.localize('Policy::Policy:InternalServerErrorMessage')
        });
      }
    });
  }

  private parseDate(value: string): Date {
    if (!value) return null as unknown as Date;
    const parts = value.split(/[-/]/);
    if (parts.length >= 3) {
      const y = parseInt(parts[0], 10);
      const m = parseInt(parts[1], 10) - 1;
      const d = parseInt(parts[2], 10);
      if (!isNaN(y) && !isNaN(m) && !isNaN(d)) {
        return new Date(y, m, d);
      }
    }
    const parsed = new Date(value);
    return isNaN(parsed.getTime()) ? (null as unknown as Date) : parsed;
  }

  formatDate(date: Date): string {
    if (!date) return '';
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  formatDateDisplay(date: Date | string | null | undefined): string {
    if (!date) return '';
    const d = typeof date === 'string' ? this.parseDate(date) : date;
    if (!d) return '';
    const day = String(d.getDate()).padStart(2, '0');
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const year = d.getFullYear();
    return `${day}/${month}/${year}`;
  }

  formatNumber(value: number | null | undefined): string {
    if (value == null || isNaN(value)) return '';
    return new Intl.NumberFormat('vi-VN').format(value);
  }

  getChassisEngine(policy: PolicyDto): string {
    const parts: string[] = [];
    if (policy.chassisNumber) parts.push(policy.chassisNumber);
    if (policy.engineNumber) parts.push(policy.engineNumber);
    return parts.join(' / ') || '-';
  }

  get totalPremiumSum(): number {
    return this.policies.reduce((sum, p) => sum + (p.premiumTotal ?? 0), 0);
  }

  close(): void {
    this.router.navigate(['/pages/policy/contract-list']);
  }

  editContract(): void {
    if (this.contractId) {
      this.router.navigate(['/pages/policy/contract/edit', this.contractId]);
    }
  }

  cancelContract(): void {
    if (!this.contractId) return;
    this.confirmationService.confirm({
      message: this.localizationService.localize('Policy::PolicyContract:CancelContractConfirmation'),
      header: this.localizationService.localize('Policy::PolicyContract:CancelContract'),
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.policyContractService.cancel(this.contractId!).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: this.localizationService.localize('Policy::Success'),
              detail: this.localizationService.localize('Policy::PolicyContract:CancelSuccess')
            });
            this.loadContract(this.contractId!);
          },
          error: (err) => {
            this.messageService.add({
              severity: 'error',
              summary: this.localizationService.localize('Policy::Error'),
              detail: err?.error?.error?.message || this.localizationService.localize('Policy::Policy:InternalServerErrorMessage')
            });
          }
        });
      }
    });
  }

  terminateContract(): void {
    this.terminateModalVisible = true;
  }

  onTerminateSubmitted(): void {
    if (this.contractId) {
      this.loadContract(this.contractId);
      this.loadPolicies({ first: 0, rows: this.policyPageSize });
    }
  }

  issuePolicy(): void {
    if (this.contractId) {
      this.router.navigate(['/pages/policy/policies/new'], {
        queryParams: { contractId: this.contractId }
      });
    }
  }

  importPolicy(): void {
    if (this.contractId) {
      this.importPolicyModal?.open(this.contractId);
    }
  }

  onImportPolicyDone(): void {
    // Refresh policy list after import
    this.loadPolicies({ first: 0, rows: this.policyPageSize });
  }

  viewPolicy(policy: PolicyDto): void {
    if (policy.id) {
      this.router.navigate(['/pages/policy/policies', policy.id, 'view']);
    }
  }

  getInsurerLabel(id?: string | null): string {
    if (id == null) return '';
    return this.insurerOptions.find(o => o.value === id)?.label ?? '';
  }

  getLobLabel(id?: string | null): string {
    if (id == null) return '';
    return this.lobOptions.find(o => o.value === id)?.label ?? '';
  }

  getCustomerLabel(id?: string | null): string {
    if (id == null) return '';
    return this.customerOptions.find(o => o.value === id)?.label ?? '';
  }

  getEmployeeLabel(id?: string | null): string {
    if (id == null) return '';
    return this.employeeOptions.find(o => o.value === id)?.label ?? '';
  }

  formatType(type?: string | PolicyContractType): string {
    if (type == null) return '';
    const str = typeof type === 'number' ? PolicyContractType[type] : String(type);
    const lower = (str || '').toLowerCase();
    if (lower === 'individual') return this.localizationService.localize('Policy::Policy:ContractTypeIndividual');
    if (lower === 'group') return this.localizationService.localize('Policy::Policy:ContractTypeGroup');
    return str;
  }

  formatStatus(status?: string | number): string {
    if (status == null) return '';
    const str = typeof status === 'number' ? PolicyContractStatus[status] : String(status);
    const lower = str.toLowerCase();
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
}
