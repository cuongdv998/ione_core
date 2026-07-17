import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { DatePickerModule } from 'primeng/datepicker';
import { MessageService } from 'primeng/api';
import { Router, ActivatedRoute } from '@angular/router';
import { ToastModule } from 'primeng/toast';
import { CheckboxModule } from 'primeng/checkbox';
import { TextareaModule } from 'primeng/textarea';
import { DialogModule } from 'primeng/dialog';
import { TooltipModule } from 'primeng/tooltip';

import { PolicyContractFormData } from './policy-contract-create.models';
import { ResCustomerStatus } from '@/proxy/res-customers/res-customer-status.enum';
import { ResCustomerSex } from '@/proxy/res-customers/res-customer-sex.enum';
import { CreateResCustomerDto, UpdateResCustomerDto } from '@/proxy/customer/res-customers/models';
import { ResProvinceService } from '@/proxy/master/controllers/res-province.service';
import { ResWardService } from '@/proxy/master/controllers/res-ward.service';
import { ResOrganizationTypeService } from '@/proxy/partner/controllers/res-organization-type.service';
import { ResIndustryService } from '@/proxy/customer/controllers/res-industry.service';
import { ResProvinceStatus } from '@/proxy/res-provinces/res-province-status.enum';
import { ResWardStatus } from '@/proxy/res-wards/res-ward-status.enum';
import { OrganizationTypeType } from '@/proxy/res-organization-types/organization-type-type.enum';
import { ResIndustryStatus } from '@/proxy/res-industries/res-industry-status.enum';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';
import { ProLineOfBusinessService } from '@/proxy/product/controllers/pro-line-of-business.service';
import { ResPartnerService } from '@/proxy/partner/controllers/res-partner.service';
import { ResCustomerService } from '@/proxy/customer/controllers/res-customer.service';
import { HrEmployeeService } from '@/proxy/hr/controllers/hr-employee.service';
import { PolicyContractType } from '@/proxy/policy-contracts/policy-contract-type.enum';
import { PolicyContractStatus } from '@/proxy/policy-contracts/policy-contract-status.enum';
import { PolicyContractService } from '@/proxy/policy/controllers/policy-contract.service';
import type { CreatePolicyContractDto, PolicyContractDto, UpdatePolicyContractDto } from '@/proxy/policy/policy-contracts/models';
import { ResDocumentService } from '@/proxy/master/controllers/res-document.service';
import { ResDocumentTypeService } from '@/proxy/master/controllers/res-document-type.service';
import type { CreateResDocumentTypeDto } from '@/proxy/master/res-document-types/models';
import { ResDocumentTypeStatus } from '@/proxy/res-document-types/res-document-type-status.enum';
import { switchMap, map } from 'rxjs/operators';
import { of, Observable } from 'rxjs';
import { ConfigStateService } from '@abp/ng.core';

@Component({
  selector: 'app-policy-contract-create',
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
    DialogModule,
    TooltipModule,
    TranslatePipe
  ],
  templateUrl: './policy-contract-create.component.html',
  styleUrl: './policy-contract-create.component.scss',
  providers: [MessageService]
})
export class PolicyContractCreateComponent implements OnInit {
  formData: PolicyContractFormData = this.getEmptyForm();
  saving = false;
  contractFileName = '';
  contractDocumentUrl: string | null = null;
  uploadedContractFile: File | null = null;
  contractId: string | null = null;
  loadingContract = false;

  lobOptions: Array<{ label: string; value: string }> = [];
  loadingLobs = false;

  insurerOptions: Array<{ label: string; value: string }> = [];
  loadingInsurers = false;

  customerOptions: Array<{ label: string; value: string }> = [];
  loadingCustomers = false;

  employeeOptions: Array<{ label: string; value: string }> = [];
  loadingEmployees = false;

  typeOptions: Array<{ label: string; value: PolicyContractType }> = [];
  statusOptions: Array<{ label: string; value: PolicyContractStatus }> = [];

  // Customer dialog (đồng bộ với form policy: đủ trường Cá nhân / Tổ chức, ngành nghề, đại diện, ủy quyền)
  customerDialogVisible = false;
  customerDialogMode: 'create' | 'edit' = 'create';
  selectedCustomerId: string | null = null;
  customerDialogLoading = false;
  customerFormData: {
    name: string;
    phone: string;
    email: string | null;
    organizationTypeId: string | null;
    provinceId: string | null;
    wardId: string | null;
    address: string;
    fullAddress: string;
    sex: ResCustomerSex | null;
    dob: Date | null;
    idNo: string | null;
    status: ResCustomerStatus;
    industryId: string | null;
    businessNo: string;
    tin: string;
    repName: string;
    repPhone: string;
    repEmail: string;
    repIdNo: string;
    repTitle: string;
    authorizerName: string;
    authorizerPhone: string;
    authorizerEmail: string;
    authorizerIdNo: string;
    authorizerTitle: string;
    authorizerDate: Date | null;
  } = this.getEmptyCustomerForm();
  provinceOptions: Array<{ label: string; value: string }> = [];
  customerWardOptions: Array<{ label: string; value: string }> = [];
  payerWardOptions: Array<{ label: string; value: string }> = [];
  organizationTypeOptions: Array<{ label: string; value: string }> = [];
  organizationTypeIdToType: Map<string, OrganizationTypeType> = new Map();
  defaultOrganizationTypeIdCn: string | null = null;
  industryOptions: Array<{ label: string; value: string }> = [];
  sexOptions: Array<{ label: string; value: ResCustomerSex }> = [];
  payerDialogVisible = false;

  constructor(
    private policyContractService: PolicyContractService,
    private lobService: ProLineOfBusinessService,
    private partnerService: ResPartnerService,
    private customerService: ResCustomerService,
    private employeeService: HrEmployeeService,
    private messageService: MessageService,
    private localizationService: LocalizationService,
    private router: Router,
    private resDocumentService: ResDocumentService,
    private resDocumentTypeService: ResDocumentTypeService,
    private provinceService: ResProvinceService,
    private wardService: ResWardService,
    private organizationTypeService: ResOrganizationTypeService,
    private industryService: ResIndustryService,
    private configState: ConfigStateService,
    private route: ActivatedRoute
  ) {
    this.initializeOptions();
  }

  ngOnInit(): void {
    this.loadCustomers();
    this.loadLobs();
    this.loadInsurers();
    this.loadEmployees();
    this.loadProvinces();
    this.loadOrganizationTypes();
    this.loadIndustryOptions();
    this.initializeSexOptions();
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.contractId = id;
      this.loadContract(id);
    }
  }

  private initializeSexOptions(): void {
    this.sexOptions = [
      { label: this.localizationService.localize('Customer::ResCustomer:Male'), value: ResCustomerSex.Male },
      { label: this.localizationService.localize('Customer::ResCustomer:Female'), value: ResCustomerSex.Female }
    ];
  }

  private initializeOptions(): void {
    this.typeOptions = [
      { label: this.localizationService.localize('Policy::Policy:ContractTypeIndividual'), value: PolicyContractType.Individual },
      { label: this.localizationService.localize('Policy::Policy:ContractTypeGroup'), value: PolicyContractType.Group }
    ];
    this.statusOptions = [
      { label: this.localizationService.localize('Policy::Policy:ContractStatusDraft'), value: PolicyContractStatus.Draft },
      { label: this.localizationService.localize('Policy::Policy:ContractStatusActive'), value: PolicyContractStatus.Active }
    ];
  }

  private getEmptyForm(): PolicyContractFormData {
    return {
      insurerId: null,
      insurerContractCode: '',
      name: '',
      effectDate: new Date(),
      status: PolicyContractStatus.Draft,
      documentId: null,
      code: '',
      type: PolicyContractType.Group,
      expireDate: this.addMonths(new Date(), 12),
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

  private addMonths(date: Date, months: number): Date {
    const result = new Date(date.getTime());
    result.setMonth(result.getMonth() + months);
    return result;
  }

  /**
   * Load customer options. When keyword is provided, gọi API với keyword để tìm theo tên.
   * Khi mở dropdown lần đầu (không keyword) chỉ load một phần danh sách; user gõ vào ô filter để tìm theo tên.
   */
  private loadCustomers(keyword?: string): void {
    this.loadingCustomers = true;
    const params: { maxResultCount: number; sorting: string; keyword?: string } = {
      maxResultCount: 50,
      sorting: 'name'
    };
    if (keyword != null && keyword.trim() !== '') {
      params.keyword = keyword.trim();
    }
    this.customerService.getList(params as any).subscribe({
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

  /**
   * Khi user gõ vào ô filter của select Khách hàng → gọi API với keyword để lọc options theo tên.
   */
  onCustomerFilter(event: { filter?: string; value?: string }): void {
    const keyword = (event?.filter ?? event?.value ?? '') as string;
    this.loadCustomers(keyword);
  }

  private loadLobs(): void {
    this.loadingLobs = true;
    this.lobService.getSelectList().subscribe({
      next: (items) => {
        this.lobOptions = (items || []).map(lob => ({
          label: lob.name || '',
          value: lob.id || ''
        }));
        if (!this.contractId && !this.formData.lobId && this.lobOptions.length > 0) {
          const defaultLob =
            this.lobOptions.find(o => (o.label || '').toLowerCase().includes('xe ô tô')) ||
            this.lobOptions[0];
          this.formData.lobId = defaultLob.value;
        }
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
    this.employeeService.getList({ maxResultCount: 1000, sorting: 'fullName' }).subscribe({
      next: (result) => {
        const items = result.items || [];
        this.employeeOptions = items.map(emp => ({
          label: emp.fullName || '',
          value: emp.id || ''
        }));
        this.loadingEmployees = false;
        this.setDefaultEmployeeId(items);
      },
      error: () => {
        this.loadingEmployees = false;
      }
    });
  }

  private setDefaultEmployeeId(employees: Array<{ id?: string; userId?: string }>): void {
    if (this.contractId) return;
    const currentUser = this.configState.getOne('currentUser');
    if (!currentUser?.id) return;
    const emp = employees.find(e => e.userId === currentUser.id);
    if (emp?.id && !this.formData.employeeId) {
      this.formData.employeeId = emp.id;
    }
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
        if (this.formData.payerProvinceId) {
          this.loadPayerWards(this.formData.payerProvinceId);
        } else {
          this.payerWardOptions = [];
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

  private loadProvinces(): void {
    this.provinceService.getList({
      status: ResProvinceStatus.Active,
      maxResultCount: 1000,
      skipCount: 0,
      sorting: 'name asc'
    }).subscribe({
      next: (result) => {
        this.provinceOptions = (result.items || []).map(p => ({
          label: p.name || '',
          value: p.id || ''
        }));
      },
      error: () => {}
    });
  }

  private loadOrganizationTypes(): void {
    this.organizationTypeService.getList({
      skipCount: 0,
      maxResultCount: 1000,
      sorting: 'name asc'
    }).subscribe({
      next: (result) => {
        const items = (result.items || []).filter(
          (item: { type?: OrganizationTypeType }) => item.type === OrganizationTypeType.CN || item.type === OrganizationTypeType.TC
        );
        this.organizationTypeIdToType.clear();
        items.forEach((item: { id?: string; type?: OrganizationTypeType }) => {
          if (item.id != null && item.type != null) this.organizationTypeIdToType.set(item.id, item.type);
        });
        this.organizationTypeOptions = items.map((o: { name?: string; id?: string }) => ({
          label: o.name || '',
          value: o.id || ''
        }));
        const cnItem = items.find((item: { type?: OrganizationTypeType }) => item.type === OrganizationTypeType.CN);
        this.defaultOrganizationTypeIdCn = cnItem?.id ?? null;
      },
      error: () => {}
    });
  }

  private loadIndustryOptions(): void {
    this.industryService.getList({
      status: ResIndustryStatus.Active,
      sorting: 'name asc',
      maxResultCount: 500,
      skipCount: 0
    }).subscribe({
      next: (result) => {
        this.industryOptions = (result.items || []).map(item => ({
          label: item.name || '',
          value: item.id || ''
        }));
      },
      error: () => {
        this.industryOptions = [];
      }
    });
  }

  isCustomerOrgTypeIndividual(): boolean {
    if (!this.customerFormData.organizationTypeId) return true;
    const type = this.organizationTypeIdToType.get(this.customerFormData.organizationTypeId);
    return type !== OrganizationTypeType.TC;
  }

  isCustomerOrgTypeOrganization(): boolean {
    if (!this.customerFormData.organizationTypeId) return false;
    return this.organizationTypeIdToType.get(this.customerFormData.organizationTypeId) === OrganizationTypeType.TC;
  }

  preventSpaceKey(event: KeyboardEvent): void {
    if (event.key === ' ') event.preventDefault();
  }

  onCustomerPhoneChange(value: string): void {
    this.customerFormData.phone = (value || '').replace(/\s/g, '').trim();
  }

  onCustomerEmailChange(value: string | null): void {
    this.customerFormData.email = value != null && value !== '' ? (String(value).replace(/\s/g, '').trim() || null) : null;
  }

  formatDate(date: Date): string {
    if (!date) return '';
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  close(): void {
    this.router.navigate(['/pages/policy/contract-list']);
  }

  private handleCreateSuccess(): void {
    this.messageService.add({
      severity: 'success',
      summary: this.localizationService.localize('Policy::Success'),
      detail: this.localizationService.localize('PolicyContract:CreateSuccess')
    });
    this.saving = false;
    this.router.navigate(['/pages/policy/contract-list']);
  }

  private handleUpdateSuccess(): void {
    this.messageService.add({
      severity: 'success',
      summary: this.localizationService.localize('Policy::Success'),
      detail: this.localizationService.localize('Policy::Policy:Policy:UpdatedSuccessfully')
    });
    this.saving = false;
    this.router.navigate(['/pages/policy/contract-list']);
  }

  saveContract(): void {
    if (!this.validateForm()) return;
    this.saving = true;

    if (this.contractId) {
      if (this.uploadedContractFile) {
        this.uploadFileAndGetId().subscribe({
          next: (documentId) => {
            this.formData.documentId = documentId;
            this.performUpdate();
          },
          error: (err) => {
            this.saving = false;
            console.error(err);
            this.messageService.add({
              severity: 'error',
              summary: this.localizationService.localize('Policy::Error'),
              detail: this.localizationService.localize('Policy::PolicyContract:UploadFailed') || 'Tải file thất bại.'
            });
          }
        });
      } else {
        this.performUpdate();
      }
      return;
    }

    const performCreate = (): void => {
      const dto = this.buildCreateDto();
      this.policyContractService.create(dto).subscribe({
        next: () => {
          this.handleCreateSuccess();
        },
        error: (error) => {
          this.saving = false;
          const body = error.error ?? error;
          const resultPayload = body?.result ?? body;
          if (resultPayload?.success === true && resultPayload?.data) {
            this.handleCreateSuccess();
            return;
          }
          const rawMsg =
            error.error?.error?.message ||
            error.error?.error?.details ||
            this.localizationService.localize('Policy::InternalServerErrorMessage');
          const msg = this.translateErrorMessage(rawMsg);
          this.messageService.add({
            severity: 'error',
            summary: this.localizationService.localize('Policy::Error'),
            detail: msg
          });
        }
      });
    };

    if (this.uploadedContractFile) {
      this.uploadFileAndGetId().subscribe({
        next: (documentId) => {
          this.formData.documentId = documentId;
          performCreate();
        },
        error: (err) => {
          this.saving = false;
          console.error(err);
          this.messageService.add({
            severity: 'error',
            summary: this.localizationService.localize('Policy::Error'),
            detail: this.localizationService.localize('Policy::PolicyContract:UploadFailed') || 'Tải file thất bại.'
          });
        }
      });
    } else {
      performCreate();
    }
  }

  private performUpdate(): void {
    const dto = this.buildUpdateDto();
    this.policyContractService.update(this.contractId!, dto).subscribe({
      next: () => {
        this.handleUpdateSuccess();
      },
      error: (error) => {
        this.saving = false;
        const rawMsg =
          error.error?.error?.message ||
          error.error?.error?.details ||
          this.localizationService.localize('Policy::InternalServerErrorMessage');
        const msg = this.translateErrorMessage(rawMsg);
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Policy::Error'),
          detail: msg
        });
      }
    });
  }

  uploadFileAndGetId(): Observable<string> {
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

  private buildCreateDto(): CreatePolicyContractDto {
    return {
      insurerId: this.formData.insurerId!,
      insurerContractCode: this.formData.insurerContractCode?.trim() || undefined,
      lobId: this.formData.lobId || undefined,
      name: this.formData.name.trim(),
      type: this.formData.type,
      customerId: this.formData.customerId!,
      payerName: this.formData.payerName?.trim() || undefined,
      payerEmail: this.formData.payerEmail?.trim() || undefined,
      payerPhone: this.formData.payerPhone?.trim() || undefined,
      payerProvinceId: this.formData.payerProvinceId || undefined,
      payerWardId: this.formData.payerWardId || undefined,
      payerAddress: this.formData.payerAddress?.trim() || undefined,
      payerFullAddress: this.formData.payerFullAddress?.trim() || undefined,
      payerTaxCode: this.formData.payerTin?.trim() || undefined,
      effectDate: this.formData.effectDate ? this.formatDate(this.formData.effectDate) : undefined,
      expireDate: this.formData.expireDate ? this.formatDate(this.formData.expireDate) : undefined,
      quantity: this.formData.quantity,
      status: this.formData.status,
      employeeId: this.formData.employeeId || undefined,
      description: this.formData.description?.trim() || undefined,
      isReciveInvoice: this.formData.isReciveInvoice ? 'Y' : 'N',
      documentId: this.formData.documentId || undefined
    };
  }

  private buildUpdateDto(): UpdatePolicyContractDto {
    return {
      insurerId: this.formData.insurerId?.trim() || undefined,
      insurerContractCode: this.formData.insurerContractCode?.trim() || undefined,
      lobId: this.formData.lobId?.trim() || undefined,
      name: this.formData.name.trim(),
      type: this.formData.type,
      customerId: this.formData.customerId!,
      payerName: this.formData.payerName?.trim() || undefined,
      payerEmail: this.formData.payerEmail?.trim() || undefined,
      payerPhone: this.formData.payerPhone?.trim() || undefined,
      payerProvinceId: this.formData.payerProvinceId || undefined,
      payerWardId: this.formData.payerWardId || undefined,
      payerAddress: this.formData.payerAddress?.trim() || undefined,
      payerFullAddress: this.formData.payerFullAddress?.trim() || undefined,
      payerTaxCode: this.formData.payerTin?.trim() || undefined,
      effectDate: this.formData.effectDate ? this.formatDate(this.formData.effectDate) : '',
      expireDate: this.formData.expireDate ? this.formatDate(this.formData.expireDate) : undefined,
      quantity: this.formData.quantity,
      currentQuantity: this.formData.currentQuantity,
      status: this.formData.status,
      employeeId: this.formData.employeeId?.trim() || undefined,
      description: this.formData.description?.trim() || undefined,
      isReciveInvoice: this.formData.isReciveInvoice ? 'Y' : 'N',
      documentId: this.formData.documentId?.trim() || undefined
    };
  }

  private validateForm(): boolean {
    if (!this.formData.insurerId) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Policy::Error'),
        detail: this.localizationService.localize('Policy::PolicyContract:InsurerIdRequired')
      });
      return false;
    }
    if (!this.formData.name?.trim()) {
      const msg = this.localizationService.localize('Policy::PolicyContract:NameRequired');
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Policy::Error'),
        detail: msg !== 'Policy::PolicyContract:NameRequired' ? msg : this.localizationService.localize('Policy::Policy:CustomerRequired')
      });
      return false;
    }
    if (!this.formData.effectDate) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Policy::Error'),
        detail: this.localizationService.localize('Policy::Policy:OrgEffectDateRequired')
      });
      return false;
    }
    // Ngày hiệu lực không được nhỏ hơn ngày hiện tại (so sánh theo ngày, bỏ qua giờ/phút)
    if (this.formData.effectDate) {
      const effectDate = new Date(this.formData.effectDate);
      effectDate.setHours(0, 0, 0, 0);
      const today = new Date();
      today.setHours(0, 0, 0, 0);
      if (effectDate < today) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Policy::Error'),
          detail: this.localizationService.localize('Policy::PolicyContract:EffectDateMustBeTodayOrFuture')
        });
        return false;
      }
    }
    if (!this.formData.customerId) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Policy::Error'),
        detail: this.localizationService.localize('Policy::Policy:CustomerRequired')
      });
      return false;
    }
    if (!this.formData.lobId) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Policy::Error'),
        detail: this.localizationService.localize('Policy::Policy:LobIdRequired')
      });
      return false;
    }
    if (!this.formData.expireDate) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Policy::Error'),
        detail: this.localizationService.localize('Policy::PolicyContract:ExpireDateRequired')
      });
      return false;
    }
    if (this.formData.quantity == null || this.formData.quantity < 1) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Policy::Error'),
        detail: this.localizationService.localize('Policy::PolicyContract:QuantityMustBePositive')
      });
      return false;
    }
    if (this.formData.currentQuantity != null && this.formData.quantity < this.formData.currentQuantity) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Policy::Error'),
        detail: this.localizationService.localize('Policy::PolicyContract:QuantityMustBeGreaterOrEqualCurrentQuantity')
      });
      return false;
    }
    return true;
  }

  private translateErrorMessage(rawMsg: string): string {
    if (!rawMsg || typeof rawMsg !== 'string') return rawMsg;
    const key = rawMsg.trim();
    if (key.startsWith('PolicyContract:') || (key.startsWith('Policy:') && !key.startsWith('Policy::'))) {
      const translated = this.localizationService.localize('Policy::' + key);
      return translated !== 'Policy::' + key ? translated : key;
    }
    return key;
  }

  onContractFileSelect(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input?.files?.[0];
    this.contractFileName = file ? file.name : '';
    this.contractDocumentUrl = null;
    this.uploadedContractFile = file || null;
    this.formData.documentId = null;
    if (input) input.value = '';
  }

  removeContractFile(): void {
    this.contractFileName = '';
    this.contractDocumentUrl = null;
    this.uploadedContractFile = null;
    this.formData.documentId = null;
  }

  private getEmptyCustomerForm(): typeof this.customerFormData {
    return {
      name: '',
      phone: '',
      email: null,
      organizationTypeId: null,
      provinceId: null,
      wardId: null,
      address: '',
      fullAddress: '',
      sex: null,
      dob: null,
      idNo: null,
      status: ResCustomerStatus.Active,
      industryId: null,
      businessNo: '',
      tin: '',
      repName: '',
      repPhone: '',
      repEmail: '',
      repIdNo: '',
      repTitle: '',
      authorizerName: '',
      authorizerPhone: '',
      authorizerEmail: '',
      authorizerIdNo: '',
      authorizerTitle: '',
      authorizerDate: null
    };
  }

  openCustomerDialog(customerId?: string | null): void {
    this.selectedCustomerId = customerId && customerId.trim() ? customerId.trim() : null;

    if (this.selectedCustomerId) {
      this.customerDialogMode = 'edit';
      this.customerDialogLoading = true;
      this.customerDialogVisible = true;
      this.customerService.get(this.selectedCustomerId).subscribe({
        next: (customer) => {
          this.customerFormData = {
            name: customer.name || '',
            phone: customer.phone || '',
            email: customer.email || null,
            organizationTypeId: customer.organizationTypeId || null,
            provinceId: customer.provinceId || null,
            wardId: customer.wardId || null,
            address: customer.address || '',
            fullAddress: customer.fullAddress || '',
            sex: customer.sex || null,
            dob: customer.dob ? new Date(customer.dob) : null,
            idNo: customer.idNo || null,
            status: customer.status ?? ResCustomerStatus.Active,
            industryId: customer.industryId || null,
            businessNo: customer.businessNo || '',
            tin: customer.tin || '',
            repName: customer.repName || '',
            repPhone: customer.repPhone || '',
            repEmail: customer.repEmail || '',
            repIdNo: customer.repIdNo || '',
            repTitle: customer.repTitle || '',
            authorizerName: customer.authorizer || '',
            authorizerPhone: customer.authorizerPhone || '',
            authorizerEmail: customer.authorizerEmail || '',
            authorizerIdNo: customer.authorizerNo || '',
            authorizerTitle: customer.authorizerTitle || '',
            authorizerDate: customer.authorizerDate ? new Date(customer.authorizerDate) : null
          };
          if (this.customerFormData.provinceId) {
            this.loadCustomerWards(this.customerFormData.provinceId);
          }
          this.customerDialogLoading = false;
        },
        error: () => {
          this.customerDialogLoading = false;
          this.messageService.add({
            severity: 'error',
            summary: this.localizationService.localize('Policy::Error'),
            detail: this.localizationService.localize('Policy::Policy:FailedToLoadCustomer') || 'Không thể tải thông tin khách hàng.'
          });
        }
      });
      return;
    }

    this.customerDialogMode = 'create';
    this.customerFormData = this.getEmptyCustomerForm();
    if (this.defaultOrganizationTypeIdCn) {
      this.customerFormData.organizationTypeId = this.defaultOrganizationTypeIdCn;
    }
    this.customerDialogVisible = true;
  }

  closeCustomerDialog(): void {
    this.customerDialogVisible = false;
  }

  onCustomerProvinceChanged(provinceId: string | null): void {
    this.customerFormData.wardId = null;
    this.customerWardOptions = [];
    this.loadCustomerWards(provinceId);
    this.updateCustomerFullAddress();
  }

  loadCustomerWards(provinceId?: string | null): void {
    if (!provinceId) {
      this.customerWardOptions = [];
      this.customerFormData.wardId = null;
      this.updateCustomerFullAddress();
      return;
    }
    this.wardService.getList({
      provinceId,
      status: ResWardStatus.Active,
      maxResultCount: 1000,
      skipCount: 0,
      sorting: 'name asc'
    }).subscribe({
      next: (result) => {
        this.customerWardOptions = (result.items || []).map(w => ({
          label: w.name || '',
          value: w.id || ''
        }));
        this.updateCustomerFullAddress();
      },
      error: () => {
        this.customerWardOptions = [];
        this.updateCustomerFullAddress();
      }
    });
  }

  updateCustomerFullAddress(): void {
    const address = (this.customerFormData.address || '').trim();
    const wardLabel = this.getOptionLabel(this.customerWardOptions, this.customerFormData.wardId);
    const provinceLabel = this.getOptionLabel(this.provinceOptions, this.customerFormData.provinceId);
    const parts = [address, wardLabel, provinceLabel].filter(x => !!x && x.trim().length > 0);
    this.customerFormData.fullAddress = parts.join(', ');
  }

  private getOptionLabel(options: Array<{ label: string; value: string }>, value?: string | null): string {
    if (!value) return '';
    const found = options.find(o => String(o.value) === String(value));
    return (found?.label || '').trim();
  }

  getEditPayerText(): string {
    const key = 'Policy::Policy:EditPayer';
    const translated = this.localizationService.localize(key);
    if (translated && translated !== key) {
      return translated;
    }
    // Fallback: reuse existing EditCustomer text
    return this.localizationService.localize('Policy::Policy:EditCustomer');
  }

  openPayerDialog(): void {
    this.payerDialogVisible = true;
  }

  closePayerDialog(): void {
    this.payerDialogVisible = false;
  }

  onCustomerChanged(customerId: string | null | undefined): void {
    const id = (customerId || '').trim();
    if (!id) return;
    this.customerService.get(id).subscribe({
      next: (customer) => {
        this.formData.payerName = customer.name || '';
        this.formData.payerEmail = customer.email || '';
        this.formData.payerPhone = customer.phone || '';
        this.formData.payerProvinceId = customer.provinceId || null;
        this.formData.payerWardId = customer.wardId || null;
        this.formData.payerAddress = customer.address || '';
        if (this.formData.payerProvinceId) {
          this.loadPayerWards(this.formData.payerProvinceId);
        } else {
          this.payerWardOptions = [];
        }
        this.updatePayerFullAddress();
      },
      error: () => {
        // Silent fail; payer info can be entered manually
      }
    });
  }

  onPayerProvinceChanged(provinceId: string | null): void {
    this.formData.payerProvinceId = provinceId;
    this.formData.payerWardId = null;
    this.payerWardOptions = [];
    this.loadPayerWards(provinceId);
    this.updatePayerFullAddress();
  }

  loadPayerWards(provinceId?: string | null): void {
    if (!provinceId) {
      this.payerWardOptions = [];
      this.formData.payerWardId = null;
      this.updatePayerFullAddress();
      return;
    }
    this.wardService.getList({
      provinceId,
      status: ResWardStatus.Active,
      maxResultCount: 1000,
      skipCount: 0,
      sorting: 'name asc'
    }).subscribe({
      next: (result) => {
        this.payerWardOptions = (result.items || []).map(w => ({
          label: w.name || '',
          value: w.id || ''
        }));
        this.updatePayerFullAddress();
      },
      error: () => {
        this.payerWardOptions = [];
        this.updatePayerFullAddress();
      }
    });
  }

  updatePayerFullAddress(): void {
    const address = (this.formData.payerAddress || '').trim();
    const wardLabel = this.getOptionLabel(this.payerWardOptions, this.formData.payerWardId);
    const provinceLabel = this.getOptionLabel(this.provinceOptions, this.formData.payerProvinceId);
    const parts = [address, wardLabel, provinceLabel].filter(x => !!x && x.trim().length > 0);
    this.formData.payerFullAddress = parts.join(', ');
  }

  saveCustomer(): void {
    if (!this.customerFormData.name?.trim()) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Policy::Error'),
        detail: this.localizationService.localize('Customer::ResCustomer:NameRequired')
      });
      return;
    }
    if (!this.customerFormData.phone?.trim()) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Policy::Error'),
        detail: this.localizationService.localize('Customer::ResCustomer:PhoneRequired')
      });
      return;
    }
    if (!this.customerFormData.address?.trim()) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Policy::Error'),
        detail: this.localizationService.localize('Customer::ResCustomer:AddressRequired')
      });
      return;
    }
    if (!this.customerFormData.organizationTypeId?.trim()) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Policy::Error'),
        detail: this.localizationService.localize('Customer::ResCustomer:OrganizationTypeRequired')
      });
      return;
    }

    this.customerDialogLoading = true;
    this.customerFormData.phone = (this.customerFormData.phone || '').replace(/\s/g, '').trim();
    this.customerFormData.repPhone = (this.customerFormData.repPhone ?? '').replace(/\s/g, '').trim() || '';
    this.customerFormData.authorizerPhone = (this.customerFormData.authorizerPhone ?? '').replace(/\s/g, '').trim() || '';
    this.customerFormData.email = this.customerFormData.email != null && this.customerFormData.email !== ''
      ? (String(this.customerFormData.email).replace(/\s/g, '').trim() || null)
      : null;
    this.customerFormData.repEmail = (this.customerFormData.repEmail ?? '').replace(/\s/g, '').trim() || '';
    this.customerFormData.authorizerEmail = (this.customerFormData.authorizerEmail ?? '').replace(/\s/g, '').trim() || '';

    if (this.customerDialogMode === 'edit' && this.selectedCustomerId) {
      const updateDto: UpdateResCustomerDto = {
        name: this.customerFormData.name.trim(),
        phone: this.customerFormData.phone.trim(),
        email: this.customerFormData.email?.trim() || undefined,
        organizationTypeId: this.customerFormData.organizationTypeId || undefined,
        provinceId: this.customerFormData.provinceId || undefined,
        wardId: this.customerFormData.wardId || undefined,
        address: this.customerFormData.address.trim(),
        idNo: this.customerFormData.idNo?.trim() || undefined,
        dob: this.customerFormData.dob ? this.formatDate(this.customerFormData.dob) : undefined,
        sex: this.customerFormData.sex ?? undefined,
        status: this.customerFormData.status,
        industryId: this.customerFormData.industryId || undefined,
        businessNo: this.customerFormData.businessNo?.trim() || undefined,
        tin: this.customerFormData.tin?.trim() || undefined,
        repName: this.customerFormData.repName?.trim() || undefined,
        repPhone: this.customerFormData.repPhone?.trim() ? this.customerFormData.repPhone.trim().slice(0, 15) : undefined,
        repEmail: this.customerFormData.repEmail?.trim() || undefined,
        repIdNo: this.customerFormData.repIdNo?.trim() || undefined,
        repTitle: this.customerFormData.repTitle?.trim() || undefined,
        authorizer: this.customerFormData.authorizerName?.trim() || undefined,
        authorizerPhone: this.customerFormData.authorizerPhone?.trim() ? this.customerFormData.authorizerPhone.trim().slice(0, 15) : undefined,
        authorizerEmail: this.customerFormData.authorizerEmail?.trim() || undefined,
        authorizerNo: this.customerFormData.authorizerIdNo?.trim() || undefined,
        authorizerTitle: this.customerFormData.authorizerTitle?.trim() || undefined,
        authorizerDate: this.customerFormData.authorizerDate ? this.formatDate(this.customerFormData.authorizerDate) : undefined
      };

      this.customerService.update(this.selectedCustomerId, updateDto).subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: this.localizationService.localize('Policy::Success'),
            detail: this.localizationService.localize('Customer::ResCustomer:UpdatedSuccessfully') || 'Cập nhật khách hàng thành công.'
          });
          this.refreshCustomerOptionsAndSelect(this.selectedCustomerId!);
        },
        error: (error) => {
          this.customerDialogLoading = false;
          const msg = error?.error?.error?.message || error?.error?.error?.details ||
            this.localizationService.localize('Policy::InternalServerErrorMessage');
          this.messageService.add({
            severity: 'error',
            summary: this.localizationService.localize('Policy::Error'),
            detail: msg
          });
        }
      });
      return;
    }

    const createDto: CreateResCustomerDto = {
      name: this.customerFormData.name.trim(),
      phone: this.customerFormData.phone.trim(),
      email: this.customerFormData.email?.trim() || undefined,
      organizationTypeId: this.customerFormData.organizationTypeId || undefined,
      provinceId: this.customerFormData.provinceId || undefined,
      wardId: this.customerFormData.wardId || undefined,
      address: this.customerFormData.address.trim(),
      idNo: this.customerFormData.idNo?.trim() || undefined,
      dob: this.customerFormData.dob ? this.formatDate(this.customerFormData.dob) : undefined,
      sex: this.customerFormData.sex ?? undefined,
      status: this.customerFormData.status,
      industryId: this.customerFormData.industryId || undefined,
      businessNo: this.customerFormData.businessNo?.trim() || undefined,
      tin: this.customerFormData.tin?.trim() || undefined,
      repName: this.customerFormData.repName?.trim() || undefined,
      repPhone: this.customerFormData.repPhone?.trim() ? this.customerFormData.repPhone.trim().slice(0, 15) : undefined,
      repEmail: this.customerFormData.repEmail?.trim() || undefined,
      repIdNo: this.customerFormData.repIdNo?.trim() || undefined,
      repTitle: this.customerFormData.repTitle?.trim() || undefined,
      authorizer: this.customerFormData.authorizerName?.trim() || undefined,
      authorizerPhone: this.customerFormData.authorizerPhone?.trim() ? this.customerFormData.authorizerPhone.trim().slice(0, 15) : undefined,
      authorizerEmail: this.customerFormData.authorizerEmail?.trim() || undefined,
      authorizerNo: this.customerFormData.authorizerIdNo?.trim() || undefined,
      authorizerTitle: this.customerFormData.authorizerTitle?.trim() || undefined,
      authorizerDate: this.customerFormData.authorizerDate ? this.formatDate(this.customerFormData.authorizerDate) : undefined
    };

    this.customerService.create(createDto).subscribe({
      next: (newCustomer) => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Policy::Success'),
          detail: this.localizationService.localize('Customer::ResCustomer:CreatedSuccessfully')
        });
        this.refreshCustomerOptionsAndSelect(newCustomer.id);
      },
      error: (error) => {
        this.customerDialogLoading = false;
        const msg = error?.error?.error?.message || error?.error?.error?.details ||
          this.localizationService.localize('Policy::InternalServerErrorMessage');
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Policy::Error'),
          detail: msg
        });
      }
    });
  }

  private refreshCustomerOptionsAndSelect(selectedId?: string): void {
    this.customerService.getList({ maxResultCount: 1000, sorting: 'name' }).subscribe({
      next: (result) => {
        this.customerOptions = (result.items || []).map(c => ({
          label: c.name || c.code || '',
          value: c.id || ''
        }));
        if (selectedId) {
          this.formData.customerId = selectedId;
          this.onCustomerChanged(selectedId);
        }
        this.customerDialogLoading = false;
        this.closeCustomerDialog();
      },
      error: () => {
        this.customerDialogLoading = false;
      }
    });
  }
}
