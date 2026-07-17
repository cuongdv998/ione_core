import { Component, EventEmitter, Input, Output, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { TextareaModule } from 'primeng/textarea';
import { DatePickerModule } from 'primeng/datepicker';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { TableModule } from 'primeng/table';
import { CheckboxModule } from 'primeng/checkbox';
import { FileUploadModule } from 'primeng/fileupload';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { RestService } from '@abp/ng.core';

import { PolicyService } from '@/proxy/policy/controllers/policy.service';
import { PolicyDto } from '@/proxy/policy/policies/models';
import { ResReasonService } from '@/proxy/master/controllers/res-reason.service';
import { CalculateRefundAmountBatchInput, TerminatePolicyInput } from '@/proxy/policy/policies/models';
import { LocalizationService } from '@/core/services/localization.service';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { PolicyFormData, CoverageItem } from '../policies.models';
import { ProLineOfBusinessService } from '@/proxy/product/controllers/pro-line-of-business.service';
import { ProLineOfBusinessStatus } from '@/proxy/pro-line-of-businesses';
import { ResCustomerService } from '@/proxy/customer/controllers/res-customer.service';
import { ResCustomerStatus } from '@/proxy/res-customers/res-customer-status.enum';
import { ResCurrencyService } from '@/proxy/master/controllers/res-currency.service';
import { ResPartnerService } from '@/proxy/partner/controllers/res-partner.service';
import { ResPartnerStatus } from '@/proxy/res-partners/res-partner-status.enum';
import { PolicyTypeService } from '@/proxy/policy/controllers/policy-type.service';
import { PolicyTypeStatus } from '@/proxy/policy-types/policy-type-status.enum';
import { HrEmployeeService } from '@/proxy/hr/controllers/hr-employee.service';
import { HrEmployeeStatus } from '@/proxy/hr-employees/hr-employee-status.enum';
import { ResCarBrandService } from '@/proxy/master/controllers/res-car-brand.service';
import { ResCarBrandStatus } from '@/proxy/res-car-brands/res-car-brand-status.enum';
import { ResCarModelService } from '@/proxy/master/controllers/res-car-model.service';
import { ResCarTypeService } from '@/proxy/master/controllers/res-car-type.service';
import { ResCarTypeStatus } from '@/proxy/res-car-types/res-car-type-status.enum';
import { ResCarLineService } from '@/proxy/master/controllers/res-car-line.service';
import { ResCarLineStatus } from '@/proxy/res-car-lines/res-car-line-status.enum';
import { ResCarGroupService } from '@/proxy/master/controllers/res-car-group.service';
import { ResCarGroupStatus } from '@/proxy/res-car-groups/res-car-group-status.enum';
import { AdminConfigService } from '@/proxy/master/controllers/admin-config.service';
import { AdminConfigStatus } from '@/proxy/admin-configs/admin-config-status.enum';
import { ProProductDto } from '@/proxy/product/pro-products/models';
import { PolicySellType } from '@/proxy/policies/policy-sell-type.enum';
import { PolicyContractType } from '@/proxy/policy-contracts/policy-contract-type.enum';

interface TerminationFormData extends PolicyFormData {
  terminationReasonId: string | null;
  terminationReasonDescription: string | null;
}

@Component({
  selector: 'app-policy-termination-modal',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    DialogModule,
    ButtonModule,
    SelectModule,
    TextareaModule,
    DatePickerModule,
    InputTextModule,
    InputNumberModule,
    TableModule,
    CheckboxModule,
    FileUploadModule,
    ToastModule,
    TranslatePipe
  ],
  templateUrl: './policy-termination-modal.component.html',
  styleUrl: './policy-termination-modal.component.scss',
  providers: [MessageService]
})
export class PolicyTerminationModalComponent implements OnInit {
  private policyService = inject(PolicyService);
  private reasonService = inject(ResReasonService);
  private messageService = inject(MessageService);
  private localizationService = inject(LocalizationService);
  private restService = inject(RestService);
  private lobService = inject(ProLineOfBusinessService);
  private customerService = inject(ResCustomerService);
  private currencyService = inject(ResCurrencyService);
  private partnerService = inject(ResPartnerService);
  private policyTypeService = inject(PolicyTypeService);
  private employeeService = inject(HrEmployeeService);
  private carBrandService = inject(ResCarBrandService);
  private carModelService = inject(ResCarModelService);
  private carTypeService = inject(ResCarTypeService);
  private carLineService = inject(ResCarLineService);
  private carGroupService = inject(ResCarGroupService);
  private adminConfigService = inject(AdminConfigService);

  @Input() pageMode = false;
  @Input() policyId: string | null = null;
  @Input() visible = false;
  @Output() visibleChange = new EventEmitter<boolean>();
  @Output() closed = new EventEmitter<void>();
  @Output() submitted = new EventEmitter<{ terminationReasonId: string; terminationReasonDescription?: string }>();

  loading = false;
  formData: TerminationFormData = this.getEmptyForm();
  terminationReasonOptions: Array<{ label: string; value: string }> = [];

  // Refund calculation
  totalRefundAmount = 0;
  private policyDetail: any = null;

  // Dropdown options
  primaryInsurancePartnerOptions: Array<{ label: string; value: string }> = [];
  lobOptions: Array<{ label: string; value: string }> = [];
  customerOptions: Array<{ label: string; value: string }> = [];
  payerOptions: Array<{ label: string; value: string }> = [];
  vehicleOwnerOptions: Array<{ label: string; value: string }> = [];
  beneficiaryOptions: Array<{ label: string; value: string }> = [];
  currencyOptions: Array<{ label: string; value: string }> = [];
  partnerOptions: Array<{ label: string; value: string }> = [];
  policyTypeOptions: Array<{ label: string; value: string }> = [];
  sellerOptions: Array<{ label: string; value: string }> = [];
  implementerOptions: Array<{ label: string; value: string }> = [];
  carBrandOptions: Array<{ label: string; value: string }> = [];
  carModelOptions: Array<{ label: string; value: string }> = [];
  carTypeOptions: Array<{ label: string; value: string }> = [];
  carLineOptions: Array<{ label: string; value: string }> = [];
  carGroupOptions: Array<{ label: string; value: string }> = [];
  carColorOptions: Array<{ label: string; value: string }> = [];
  carPlateTypeOptions: Array<{ label: string; value: string }> = [];
  carOriginOptions: Array<{ label: string; value: string }> = [];
  businessPurposeOptions: Array<{ label: string; value: string }> = [
    { label: 'Có kinh doanh', value: 'KDVT' },
    { label: 'Không kinh doanh', value: 'KKDVT' }
  ];
  contractTypeOptions: Array<{ label: string; value: PolicyContractType }> = [];
  sellTypeOptions: Array<{ label: string; value: PolicySellType }> = [];

  // Products and coverages
  products: ProProductDto[] = [];
  selectedProducts: { [productId: string]: boolean } = {};
  productCoverages: { [productId: string]: CoverageItem[] } = {};
  private pendingDetailForEdit: any = null;

  /**
   * Format date for display
   */
  formatDate(date: Date | string | null): string {
    if (!date) return '-';
    const d = typeof date === 'string' ? new Date(date) : date;
    if (isNaN(d.getTime())) return '-';
    return d.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' });
  }

  /**
   * Format insurance period range
   */
  formatInsurancePeriod(): string {
    const from = this.formatDate(this.formData.insurancePeriodFrom);
    const to = this.formatDate(this.formData.insurancePeriodTo);
    return `${from} - ${to}`;
  }

  /**
   * Get empty form with all fields initialized
   */
  private getEmptyForm(): TerminationFormData {
    return {
      // Termination fields
      terminationReasonId: null,
      terminationReasonDescription: null,
      // Policy fields (all initialized as empty)
      contractId: null,
      primaryInsurancePartnerId: null,
      lobId: '',
      contractType: null,
      customerId: null,
      payerId: null,
      payerName: null,
      payerPhone: null,
      payerEmail: null,
      payerAddress: null,
      payerFullAddress: null,
      payerProvinceId: null,
      payerWardId: null,
      isReceiveInvoice: false,
      invoiceRecipient: null,
      contractNo: null,
      contractName: null,
      rootPolicyNo: null,
      policyNo: '',
      lastVersionId: '0',
      versionNo: null,
      sellType: 0,
      insurerPolicyNo: null,
      policyTypeId: '',
      partnerId: '',
      sellerId: '',
      implementerId: '',
      currencyId: '',
      exchangeRate: 1,
      status: 0 as any,
      issueDate: null,
      approvalStatus: null,
      terminationStatus: null,
      paymentStatus: null,
      orgEffectDate: '',
      orgExpireDate: '',
      isRenewal: false,
      isGift: 'N',
      premiumTotal: 0,
      premium: 0,
      vat: 0,
      discount: null,
      discountRate: null,
      isBankLoan: false,
      channelId: null,
      certificateNo: null,
      beneficiaryId: null,
      vehicleOwnerId: null,
      vehicleOwnerName: null,
      vehicleOwnerPhone: null,
      vehicleOwnerEmail: null,
      vehicleOwnerAddress: null,
      vehicleOwnerFullAddress: null,
      vehicleOwnerProvinceId: null,
      vehicleOwnerWardId: null,
      vehicleOwnerIdNo: null,
      carUsage: '',
      carBrandId: null,
      carModelId: null,
      carCategoryId: null,
      seatingCapacity: null,
      weight: null,
      isNewCar: null,
      carValue: null,
      carColor: null,
      vehiclePlate: null,
      vehiclePlateType: null,
      chassisNumber: null,
      engineNumber: null,
      vehicleTypeId: null,
      productionYear: null,
      origin: null,
      carLineId: null,
      carGroupId: null,
      insuredName: null,
      insuredIdNo: null,
      insuredPhone: null,
      insuredEmail: null,
      insuredProvinceId: null,
      insuredWardId: null,
      insuredAddress: null,
      insuredFullAddress: null,
      beneficiaryName: null,
      beneficiaryIdNo: null,
      beneficiaryPhone: null,
      beneficiaryEmail: null,
      beneficiaryProvinceId: null,
      beneficiaryWardId: null,
      beneficiaryAddress: null,
      beneficiaryFullAddress: null,
      insurancePeriodFrom: new Date(),
      insurancePeriodTo: new Date(),
      isVehicleMaterialInsurance: false,
      isMandatoryCivilLiability: false,
      isVoluntaryCivilLiability: false,
      packageType: null,
      customerNotes: null,
      internalNotes: null
    };
  }

  /**
   * Load all dropdown options for the form
   */
  private loadDialogOptions(): void {
    // Load Line of Business options
    this.lobService.getList({
      status: ProLineOfBusinessStatus.Active,
      maxResultCount: 1000,
      skipCount: 0,
      sorting: 'name asc'
    }).subscribe({
      next: (result) => {
        this.lobOptions = (result.items || []).map(lob => ({
          label: lob.name || '',
          value: lob.id || ''
        }));
      },
      error: () => {
        // Silently fail
      }
    });

    // Load Customer options
    this.customerService.getList({
      status: ResCustomerStatus.Active,
      maxResultCount: 1000,
      skipCount: 0,
      sorting: 'name asc'
    }).subscribe({
      next: (result) => {
        const customerOptions = (result.items || []).map(customer => ({
          label: customer.name || '',
          value: customer.id || ''
        }));
        this.customerOptions = customerOptions;
        this.payerOptions = customerOptions;
        this.vehicleOwnerOptions = customerOptions;
        this.beneficiaryOptions = customerOptions;
      },
      error: () => {
        // Silently fail
      }
    });

    // Load Currency options
    this.currencyService.getList({
      maxResultCount: 1000,
      skipCount: 0,
      sorting: 'name asc'
    }).subscribe({
      next: (result) => {
        this.currencyOptions = (result.items || []).map(currency => ({
          label: currency.name || '',
          value: currency.id || ''
        }));
      },
      error: () => {
        // Silently fail
      }
    });

    // Load Partner options
    this.partnerService.getList({
      status: ResPartnerStatus.Active,
      maxResultCount: 1000,
      skipCount: 0,
      sorting: 'name asc'
    }).subscribe({
      next: (result) => {
        this.primaryInsurancePartnerOptions = (result.items || []).map(partner => ({
          label: partner.name || '',
          value: partner.id || ''
        }));
        this.partnerOptions = this.primaryInsurancePartnerOptions;
      },
      error: () => {
        // Silently fail
      }
    });

    // Load Policy Type options
    this.policyTypeService.getList({
      status: PolicyTypeStatus.Active,
      maxResultCount: 1000,
      skipCount: 0,
      sorting: 'name asc'
    }).subscribe({
      next: (result) => {
        this.policyTypeOptions = (result.items || []).map(type => ({
          label: type.name || '',
          value: type.id || ''
        }));
      },
      error: () => {
        // Silently fail
      }
    });

    // Load Employee options for Seller and Implementer
    this.employeeService.getList({
      maxResultCount: 1000,
      skipCount: 0,
      status: HrEmployeeStatus.Active,
      sorting: 'fullName asc'
    }).subscribe({
      next: (result) => {
        const employees = result.items || [];
        const employeeOptions = employees.map(emp => ({
          label: emp.fullName || '',
          value: emp.id || ''
        }));
        this.sellerOptions = employeeOptions;
        this.implementerOptions = employeeOptions;
      },
      error: () => {
        // Silently fail
      }
    });

    // Load Car Brand options
    this.carBrandService.getList({
      status: ResCarBrandStatus.Active,
      maxResultCount: 1000,
      skipCount: 0,
      sorting: 'name asc'
    }).subscribe({
      next: (result) => {
        this.carBrandOptions = (result.items || []).map(brand => ({
          label: brand.name || '',
          value: brand.id || ''
        }));
      },
      error: () => {
        // Silently fail
      }
    });

    // Load Car Line options
    this.carLineService.getList({
      status: ResCarLineStatus.Active,
      maxResultCount: 1000,
      skipCount: 0,
      sorting: 'name asc'
    }).subscribe({
      next: (result) => {
        this.carLineOptions = (result.items || []).map(line => ({
          label: line.name || '',
          value: line.id || ''
        }));
      },
      error: () => {
        // Silently fail
      }
    });

    // Load Car Group options
    this.carGroupService.getList({
      status: ResCarGroupStatus.Active,
      maxResultCount: 1000,
      skipCount: 0,
      sorting: 'name asc'
    }).subscribe({
      next: (result) => {
        this.carGroupOptions = (result.items || []).map(group => ({
          label: group.name || '',
          value: group.id || ''
        }));
      },
      error: () => {
        // Silently fail
      }
    });

    // Load Car Type options
    this.carTypeService.getList({
      status: ResCarTypeStatus.Active,
      maxResultCount: 1000,
      skipCount: 0,
      sorting: 'name asc'
    }).subscribe({
      next: (result) => {
        this.carTypeOptions = (result.items || []).map(type => ({
          label: type.name || '',
          value: type.id || ''
        }));
      },
      error: () => {
        // Silently fail
      }
    });

    // Load admin config options
    this.loadAdminConfigOptions();

    // Initialize static options
    this.initializeStaticOptions();
  }

  /**
   * Load admin config options for car color, plate type, and origin
   */
  private loadAdminConfigOptions(): void {
    // Load Car Color options (CAR_COLOR)
    this.adminConfigService.getList({
      code: 'CAR_COLOR',
      status: AdminConfigStatus.Active,
      maxResultCount: 1000,
      skipCount: 0,
      sorting: 'name asc'
    }).subscribe({
      next: (result) => {
        this.carColorOptions = (result.items || []).map(config => ({
          label: config.name || '',
          value: config.value || ''
        }));
      },
      error: () => {
        // Silently fail
      }
    });

    // Load Car Plate Type options (CAR_PLATE_TYPE)
    this.adminConfigService.getList({
      code: 'CAR_PLATE_TYPE',
      status: AdminConfigStatus.Active,
      maxResultCount: 1000,
      skipCount: 0,
      sorting: 'name asc'
    }).subscribe({
      next: (result) => {
        this.carPlateTypeOptions = (result.items || []).map(config => ({
          label: config.name || '',
          value: config.value || ''
        }));
      },
      error: () => {
        // Silently fail
      }
    });

    // Load Car Origin options (CAR_ORG)
    this.adminConfigService.getList({
      code: 'CAR_ORG',
      status: AdminConfigStatus.Active,
      maxResultCount: 1000,
      skipCount: 0,
      sorting: 'name asc'
    }).subscribe({
      next: (result) => {
        this.carOriginOptions = (result.items || []).map(config => ({
          label: config.name || '',
          value: config.value || ''
        }));
      },
      error: () => {
        // Silently fail
      }
    });
  }

  /**
   * Initialize static options (sell type, contract type)
   */
  private initializeStaticOptions(): void {
    this.sellTypeOptions = [
      { label: this.localizationService.localize('Policy::Policy:SellTypeAgency'), value: PolicySellType.Agency },
      { label: this.localizationService.localize('Policy::Policy:SellTypeDirect'), value: PolicySellType.Direct },
      { label: this.localizationService.localize('Policy::Policy:SellTypeIndirect'), value: PolicySellType.Indirect }
    ];

    this.contractTypeOptions = [
      { label: this.localizationService.localize('Policy::Policy:ContractTypeIndividual'), value: PolicyContractType.Individual },
      { label: this.localizationService.localize('Policy::Policy:ContractTypeGroup'), value: PolicyContractType.Group }
    ];
  }

  /**
   * Load Car Model options filtered by Car Brand
   */
  loadCarModels(carBrandId?: string | null): void {
    if (!carBrandId) {
      this.carModelOptions = [];
      return;
    }

    this.carModelService.getList({
      carBrandId: carBrandId,
      status: ResCarBrandStatus.Active,
      maxResultCount: 1000,
      skipCount: 0,
      sorting: 'name asc'
    }).subscribe({
      next: (result) => {
        this.carModelOptions = (result.items || []).map(model => ({
          label: model.name || '',
          value: model.id || ''
        }));
      },
      error: () => {
        this.carModelOptions = [];
      }
    });
  }

  ngOnInit(): void {
    if (this.pageMode && this.policyId) {
      this.visible = true;
      this.visibleChange.emit(true);
      this.formData = this.getEmptyForm();
      this.loadDialogOptions();
      this.loadTerminationReasonOptions();
      this.loadPolicyDetail(this.policyId);
    }
  }

  /**
   * Open modal and load policy data
   */
  open(policyId: string): void {
    this.visible = true;
    this.visibleChange.emit(true);
    this.formData = this.getEmptyForm();
    this.loadDialogOptions();
    this.loadTerminationReasonOptions();
    this.loadPolicyDetail(policyId);
  }

  /**
   * Close modal
   */
  close(): void {
    this.visible = false;
    this.visibleChange.emit(false);
    this.closed.emit();
    this.formData = this.getEmptyForm();
  }

  /**
   * Handle visible change from dialog
   */
  onVisibleChange(value: boolean): void {
    if (!value) {
      this.close();
    }
  }

  /**
   * Handle cancel button click
   */
  onCancel(): void {
    this.close();
  }

  /**
   * Handle submit button click
   */
  onSubmit(): void {
    if (!this.formData.terminationReasonId) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Policy::Policy:TerminationReasonRequired')
      });
      return;
    }

    if (!this.policyId && !this.policyDetail?.id) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: 'Policy ID is required'
      });
      return;
    }

    this.loading = true;

    // Build coverage refunds from loaded data
    const coverageRefunds: Array<{ coverageId: string; refundAmount: number }> = [];
    Object.keys(this.productCoverages).forEach(productId => {
      const items = this.productCoverages[productId] || [];
      items.forEach(item => {
        if (!item.isSectionHeader && item.coverageId && item.refundAmount !== undefined && item.refundAmount !== null) {
          coverageRefunds.push({
            coverageId: item.coverageId,
            refundAmount: item.refundAmount
          });
        }
      });
    });

    const input: TerminatePolicyInput = {
      terminationReasonId: this.formData.terminationReasonId,
      terminationReasonDescription: this.formData.terminationReasonDescription || undefined,
      // Date-only: keep LOCAL day (avoid UTC shift)
      terminationDate: this.toLocalIsoDate(new Date()),
      totalRefundAmount: this.totalRefundAmount,
      coverageRefunds: coverageRefunds,
      documents: []
    };

    // Determine the policy ID to use
    const targetPolicyId = this.policyId || this.policyDetail?.id;

    if (!targetPolicyId) {
      this.loading = false;
      return;
    }

    this.policyService.terminatePolicy(targetPolicyId, input).subscribe({
      next: () => {
        this.loading = false;
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('AbpUi::Success'),
          detail: this.localizationService.localize('Policy::Policy:TerminatedSuccessfully')
        });

        this.submitted.emit({
          terminationReasonId: this.formData.terminationReasonId!,
          terminationReasonDescription: this.formData.terminationReasonDescription || undefined
        });

        this.close();
      },
      error: (error: any) => {
        this.loading = false;
        console.error('Failed to terminate policy:', error);
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: error?.error?.error?.message || 'Failed to terminate policy'
        });
      }
    });
  }

  /**
   * Load termination reason options using getSelectListByGroupCode
   */
  private loadTerminationReasonOptions(): void {
    // Using the simplified method which is more reliable than searching groups manually
    this.reasonService.getSelectListByGroupCode('TERMINATE_POLICY_REASON').subscribe({
      next: (result: any) => {
        this.terminationReasonOptions = (result || []).map((reason: any) => ({
          label: reason.name || '',
          value: reason.id || ''
        }));
      },
      error: (error: any) => {
        console.error('Failed to load termination reasons:', error);
        this.messageService.add({
          severity: 'warn',
          summary: this.localizationService.localize('AbpUi::Warning'),
          detail: 'Failed to load termination reasons'
        });
      }
    });
  }

  /**
   * Load policy detail and populate form
   */
  private loadPolicyDetail(policyId: string): void {
    this.loading = true;
    this.policyService.get(policyId).subscribe({
      next: (detail: any) => {
        this.policyDetail = detail;
        this.populateFormFromDetail(detail);
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Failed to load policy detail:', error);
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: 'Failed to load policy details'
        });
        this.loading = false;
        this.close();
      }
    });
  }

  /**
   * Populate form from policy detail
   * Similar to policies.component.ts populateFormFromDetail but simplified
   */
  private populateFormFromDetail(detail: any): void {
    const contract = detail?.contract;
    const versionDetail = detail?.versionDetail;
    const riskObject = detail?.riskObject;
    const riskMotor = riskObject?.riskObjectMotor;

    const customerId = contract?.customerId || detail?.customerId || null;

    this.formData = {
      ...this.formData, // Keep termination fields
      // Contract fields
      contractId: contract?.id || detail?.contractId || null,
      contractNo: contract?.code || detail?.contractNo || null,
      contractName: contract?.name || null,
      lobId: detail?.lobId || '',
      primaryInsurancePartnerId: contract?.insurerId || detail?.partnerId || null,
      contractType: contract?.type ?? null,
      customerId: customerId,
      payerId: detail?.payerId || null,
      payerName: contract?.payerName || null,
      payerPhone: contract?.payerPhone || null,
      payerEmail: contract?.payerEmail || null,
      payerAddress: contract?.payerAddress || null,
      payerFullAddress: contract?.payerFullAddress || null,
      payerProvinceId: contract?.payerProvinceId || null,
      payerWardId: contract?.payerWardId || null,

      vehicleOwnerId: detail?.vehicleOwnerId || null,
      vehicleOwnerName: riskObject?.repName || null,
      vehicleOwnerPhone: riskObject?.repPhone || null,
      vehicleOwnerEmail: riskObject?.repEmail || null,
      vehicleOwnerAddress: riskObject?.repAddress || null,
      vehicleOwnerFullAddress: riskObject?.repFullAddress || null,
      vehicleOwnerProvinceId: riskObject?.repProvinceId || null,
      vehicleOwnerWardId: riskObject?.repWardId || null,
      vehicleOwnerIdNo: riskObject?.repIdNo || null,
      beneficiaryId: detail?.beneficiaryId || null,
      isReceiveInvoice: contract?.isReciveInvoice === 'Y',
      invoiceRecipient: null, // Note: Not in detail currently

      // Policy fields
      rootPolicyNo: detail?.rootPolicyNo || null,
      policyNo: detail?.policyNo || '',
      lastVersionId: detail?.lastVersionId || '0',
      versionNo: versionDetail?.version ? Number(versionDetail.version) : null,
      sellType: detail?.sellType ?? 0,
      insurerPolicyNo: detail?.insurerPolicyNo || null,
      policyTypeId: detail?.policyTypeId || '',
      partnerId: detail?.partnerId || '',
      sellerId: detail?.sellerId || '',
      implementerId: detail?.implementerId || '',
      currencyId: detail?.currencyId || '',
      exchangeRate: detail?.exchangeRate ?? 1,
      status: detail?.status,
      issueDate: detail?.issueDate || null,
      approvalStatus: detail?.approvalStatus || null,
      paymentStatus: detail?.paymentStatus || null,
      orgEffectDate: detail?.orgEffectDate || '',
      orgExpireDate: detail?.orgExpireDate || '',
      isRenewal: detail?.isRenewal === 'Y',
      isGift: detail?.isGift || 'N',
      premiumTotal: detail?.premiumTotal ?? 0,
      premium: detail?.premium ?? 0,
      vat: detail?.vat ?? 0,
      discount: detail?.discount ?? null,
      discountRate: detail?.discountRate ?? null,
      isBankLoan: detail?.isBankLoan === 'Y',
      channelId: detail?.channelId || null,
      certificateNo: detail?.certificateNo || null,

      // Insured person fields
      insuredName: detail?.insuredName || null,
      insuredIdNo: detail?.insuredIdNo || null,
      insuredPhone: detail?.insuredPhone || null,
      insuredEmail: detail?.insuredEmail || null,
      insuredProvinceId: detail?.insuredProvinceId || null,
      insuredWardId: detail?.insuredWardId || null,
      insuredAddress: detail?.insuredAddress || null,
      insuredFullAddress: detail?.insuredFullAddress || null,

      // Beneficiary fields
      beneficiaryName: detail?.beneficiaryName || null,
      beneficiaryIdNo: detail?.beneficiaryIdNo || null,
      beneficiaryPhone: detail?.beneficiaryPhone || null,
      beneficiaryEmail: detail?.beneficiaryEmail || null,
      beneficiaryProvinceId: detail?.beneficiaryProvinceId || null,
      beneficiaryWardId: detail?.beneficiaryWardId || null,
      beneficiaryAddress: detail?.beneficiaryAddress || null,
      beneficiaryFullAddress: detail?.beneficiaryFullAddress || null,

      // Vehicle fields
      vehiclePlate: riskMotor?.carPlate || detail?.vehiclePlate || null,
      engineNumber: riskMotor?.carEngineNumber || detail?.engineNumber || null,
      chassisNumber: riskMotor?.carVin || detail?.chassisNumber || null,
      seatingCapacity: riskMotor?.carSeatNumber ?? null,
      weight: riskMotor?.carPayloadCapacity ?? null,
      carValue: riskMotor?.riskObjectValue ?? null,
      carColor: riskMotor?.carColor ?? null,
      carUsage: riskMotor?.carUsage || null,
      carBrandId: null, // Will match by code
      carModelId: null, // Will match by code
      carCategoryId: null, // Not used in termination flow
      vehicleTypeId: null, // Will match by code
      carLineId: null, // Will match by code
      carGroupId: null, // Will match by code
      isNewCar: (riskMotor?.carNew === 'Y' || riskMotor?.isNewCar === true),
      productionYear: riskMotor?.carProductionYear ? new Date(riskMotor.carProductionYear) : null,
      origin: riskMotor?.carOrigin || null,

      // Thời hạn bảo hiểm (từ)/(đến) lấy theo policy version: effectDate, expireDate
      insurancePeriodFrom: versionDetail?.effectDate
        ? new Date(versionDetail.effectDate)
        : (detail?.orgEffectDate ? new Date(detail.orgEffectDate) : new Date()),
      insurancePeriodTo: versionDetail?.expireDate
        ? new Date(versionDetail.expireDate)
        : (detail?.orgExpireDate ? new Date(detail.orgExpireDate) : new Date()),

      isVehicleMaterialInsurance: false,
      isMandatoryCivilLiability: false,
      isVoluntaryCivilLiability: false,
      packageType: null,

      // Notes
      customerNotes: versionDetail?.customerNote || null,
      internalNotes: versionDetail?.internalNote || null,
      terminationReasonId: null,
      terminationReasonDescription: null
    };

    this.pendingDetailForEdit = detail;
    this.loadProducts();
  }

  /**
   * API by-lob-partner requires channelId; fallback to insurer partner when detail has no channel.
   */
  private ensureChannelIdForProductLoad(done: () => void): void {
    if (this.formData.channelId?.trim()) {
      done();
      return;
    }
    const insurerId = this.formData.primaryInsurancePartnerId?.trim();
    if (insurerId) {
      this.partnerService.get(insurerId).subscribe({
        next: (partner) => {
          const ch = partner?.channelId?.trim();
          if (ch) {
            this.formData.channelId = ch;
          }
          done();
        },
        error: () => done()
      });
      return;
    }
    done();
  }

  /**
   * Load products based on lobId and primaryInsurancePartnerId.
   * Uses by-lob-partner API so products include productCoverages (required for Phạm vi bảo hiểm section).
   */
  private loadProducts(): void {
    if (!this.formData.lobId || !this.formData.primaryInsurancePartnerId) {
      this.products = [];
      this.selectedProducts = {};
      this.productCoverages = {};
      return;
    }

    const execute = (): void => {
      if (!this.formData.channelId?.trim()) {
        this.products = [];
        this.selectedProducts = {};
        this.productCoverages = {};
        return;
      }

    this.restService.request<any, ProProductDto[]>({
      method: 'GET',
      url: '/api/product/pro-products/by-lob-partner',
      params: {
        lobId: this.formData.lobId,
        partnerId: this.formData.primaryInsurancePartnerId,
        channelId: this.formData.channelId || undefined,
        appChannelId: undefined
      }
    }).subscribe({
      next: (result: any) => {
        const productsList = Array.isArray(result) ? result : (result ? [result] : []);
        this.products = productsList;

        // Initialize selected products and coverages
        this.selectedProducts = {};
        this.productCoverages = {};

        // Select products that are in the policy
        if (this.pendingDetailForEdit && this.pendingDetailForEdit.products) {
          this.pendingDetailForEdit.products.forEach((p: any) => {
            if (p.productId) {
              this.selectedProducts[p.productId] = true;
            }
          });
        }

        // Initialize coverages for selected products (by-lob-partner returns productCoverages)
        this.products.forEach(p => {
          if (p.id && this.selectedProducts[p.id]) {
            this.loadProductCoverages(p.id);
          }
        });

        // After products are loaded, apply detail values to them
        if (this.pendingDetailForEdit) {
          this.applyDetailToLoadedProducts(this.pendingDetailForEdit);
        }
      },
      error: (error: any) => {
        console.error('Failed to load products:', error);
        this.products = [];
        this.selectedProducts = {};
        this.productCoverages = {};
      }
    });
    };

    if (!this.formData.channelId?.trim()) {
      this.ensureChannelIdForProductLoad(() => execute());
      return;
    }

    execute();
  }

  /**
   * Load coverages for a product
   */
  private loadProductCoverages(productId: string): void {
    const product = this.products.find(p => p.id === productId);
    if (!product || !product.productCoverages) {
      return;
    }

    const sortedProductCoverages = (product.productCoverages || [])
      .map((pc: any, idx: number) => ({ pc, idx }))
      .sort((a: any, b: any) => {
        const typeA = a.pc.coverage?.type ?? 99;
        const typeB = b.pc.coverage?.type ?? 99;
        if (typeA !== typeB) return typeA - typeB;
        return a.idx - b.idx;
      })
      .map((x: any) => x.pc);

    this.productCoverages[productId] = sortedProductCoverages.map((pc: any) => {
      return {
        productCoverageId: pc.id,
        coverageId: pc.coverageId,
        coverageName: pc.coverage?.name,
        benefit: pc.coverage?.name || pc.coverageId || '',
        // Using optional checks properly
        coverageType: pc.coverage?.type ?? 0,

        // Initialize numeric fields
        insuranceAmount: null,
        liabilityAmountMin: pc.minAmount,
        liabilityAmountMax: pc.maxAmount,
        liabilityAmountFixed: pc.fixedAmount,

        quantity: null,
        taxRate: null,
        premiumRate: null,
        premium: 0,
        vat: 0,
        premiumWithVAT: 0,
        deductible: null,
        description: pc.description,

        selected: false,
        isSectionHeader: false,

        // New fields
        refundAmount: 0 // Initialize refund amount
      };
    });
  }

  /**
   * Apply coverage data from policy detail to loaded products
   */
  private applyDetailToLoadedProducts(detail: any): void {
    if (!detail || !detail.products) return;

    // Map detail products by productId
    const detailProductsByProductId = new Map<string, any>();
    detail.products.forEach((p: any) => {
      if (p.productId) detailProductsByProductId.set(p.productId, p);
    });

    Object.keys(this.productCoverages).forEach(productId => {
      // If this product isn't in the policy detail (shouldn't happen if selectedProducts is correct), skip
      if (!detailProductsByProductId.has(productId)) return;

      const detailProd = detailProductsByProductId.get(productId);
      const detailCoverages = detailProd.coverages || [];

      // Map detail coverages by coverageId
      const byCoverageId = new Map<string, any>();
      detailCoverages.forEach((c: any) => {
        if (c.coverageId) byCoverageId.set(String(c.coverageId), c);
      });

      const items = this.productCoverages[productId] || [];
      items.forEach((item: any) => {
        const covId = item?.coverageId;
        if (!covId) return;
        const dc = byCoverageId.get(String(covId));
        if (!dc) return;

        if (dc.amountLiability !== undefined && dc.amountLiability !== null) {
          item.insuranceAmount = dc.amountLiability;
        }

        if (dc.quantity !== undefined && dc.quantity !== null) {
          item.quantity = dc.quantity;
        }

        if (item.coverageType !== 0) {
          item.selected = true;
        }

        // Apply calculated values if available
        if (dc.premium !== undefined && dc.premium !== null) {
          item.premium = dc.premium;
        }
        if (dc.vat !== undefined && dc.vat !== null) {
          item.vat = dc.vat;
        }
        if (dc.premiumTotal !== undefined && dc.premiumTotal !== null) {
          item.premiumWithVAT = dc.premiumTotal;
        }
        if (dc.taxRate !== undefined && dc.taxRate !== null) {
          item.taxRate = dc.taxRate;
        }
        if (dc.premiumRate !== undefined && dc.premiumRate !== null) {
          item.premiumRate = dc.premiumRate;
        }
        if (dc.deductible !== undefined && dc.deductible !== null) {
          item.deductible = String(dc.deductible);
        }
      });
    });

    // Calculate refund amounts after loading products
    this.calculateRefundAmounts();
  }

  /**
   * Calculate refund amounts by calling the backend API
   */
  private calculateRefundAmounts(): void {
    if (!this.policyDetail) {
      return;
    }

    const selectedProductIds = Object.keys(this.selectedProducts).filter(id => this.selectedProducts[id]);
    if (selectedProductIds.length === 0) {
      return;
    }

    // Build attributes from risk motor data (shared across all products)
    const riskMotor = this.policyDetail?.riskObject?.riskObjectMotor || {};
    const attributes: Record<string, any> = {
      carPlate: riskMotor.carPlate || '',
      carAge: riskMotor.carOld || 0,
      carGroup: riskMotor.carGroupCode || '',
      carUsage: riskMotor.carUsage || '',
      carValue: riskMotor.riskObjectValue || 0,
      seat: String(riskMotor.carSeatNumber || ''),
      payload: String(riskMotor.carPayloadCapacity || '')
    };

    const productInputs = selectedProductIds
      .map(productId => {
        const product = this.products.find(p => p.id === productId);
        if (!product?.code) return null;

        const coverages = this.productCoverages[productId] || [];
        const productCoverages = coverages
          .filter((c: any) => !c.isSectionHeader && c.coverageId)
          .map((c: any) => ({
            productCoverageId: c.productCoverageId || '',
            coverageId: c.coverageId || '',
            amountLiability: c.insuranceAmount || 0,
            quantity: c.quantity || 1,
            premium: c.premium || 0,
            premiumVAT: c.vat || 0
          }));

        const totals = this.getProductTotals(productId);
        return {
          productId: product.code,
          productCoverages,
          totalPremium: totals.premium,
          totalPremiumVAT: totals.premiumWithVAT
        };
      })
      .filter((p): p is NonNullable<typeof p> => p !== null);

    if (productInputs.length === 0) {
      return;
    }

    const batchInput: CalculateRefundAmountBatchInput = {
      policyId: this.policyDetail.id || '',
      // Date-only: keep LOCAL day (avoid UTC shift)
      terminationDate: this.toLocalIsoDate(new Date()),
      attributes,
      products: productInputs
    };

    this.policyService.calculateRefundAmountBatch(batchInput).subscribe({
      next: (result: any) => {
        this.totalRefundAmount = result.totalRefund || 0;

        // Apply refund amounts to coverages
        const refundByCoverageId = new Map<string, number>(
          (result.coverageRefunds || []).map((r: any) => [r.coverageId || '', r.refundAmount || 0])
        );

        Object.keys(this.productCoverages).forEach(productId => {
          const items = this.productCoverages[productId] || [];
          items.forEach(item => {
            if (item.coverageId && refundByCoverageId.has(item.coverageId)) {
              item.refundAmount = refundByCoverageId.get(item.coverageId) || 0;
            }
          });
        });
      },
      error: (error: any) => {
        console.error('Failed to calculate refund amounts:', error);
        // Don't show error to user, just leave refund as 0
      }
    });
  }

  /**
   * Get coverage items for a product
   */
  getProductCoverages(productId: string): CoverageItem[] {
    return this.productCoverages[productId] || [];
  }

  /**
   * Get main coverage items (type === 0) for a product
   */
  getMainCoverages(productId: string): CoverageItem[] {
    const coverages = this.productCoverages[productId] || [];
    return coverages.filter((c: any) => c.coverageType === 0 && !c.isSectionHeader);
  }

  /**
   * Get non-main coverage items (type !== 0) for a product
   */
  getNonMainCoverages(productId: string): CoverageItem[] {
    const coverages = this.productCoverages[productId] || [];
    return coverages.filter((c: any) => c.coverageType !== 0 && !c.isSectionHeader);
  }

  /**
   * Get all coverages with section headers
   */
  getAllCoveragesWithSections(productId: string): CoverageItem[] {
    const mainCoverages = this.getMainCoverages(productId);
    const nonMainCoverages = this.getNonMainCoverages(productId);
    const result: CoverageItem[] = [];

    // Add main coverages section if it has items
    if (mainCoverages.length > 0) {
      result.push({
        benefit: '1. Phạm vi chính',
        insuranceAmount: null,
        quantity: null,
        taxRate: null,
        premiumRate: null,
        premium: 0,
        vat: 0,
        premiumWithVAT: 0,
        deductible: null,
        description: null,
        isSectionHeader: true
      });
      result.push(...mainCoverages);
    }

    // Add non-main coverages section if it has items
    if (nonMainCoverages.length > 0) {
      result.push({
        benefit: '2. Phạm vi bổ sung',
        insuranceAmount: null,
        quantity: null,
        taxRate: null,
        premiumRate: null,
        premium: 0,
        vat: 0,
        premiumWithVAT: 0,
        deductible: null,
        description: null,
        isSectionHeader: true
      });
      result.push(...nonMainCoverages);
    }

    return result;
  }

  /**
   * Check if a product is selected
   */
  isProductSelected(productId: string): boolean {
    return this.selectedProducts[productId] || false;
  }

  /** productTypeCode (pro_product_type.code) = VCX */
  isVcxProductCode(productOrCode: { productTypeCode?: string | null } | string | undefined | null): boolean {
    if (productOrCode == null) return false;
    const code = typeof productOrCode === 'string' ? productOrCode : (productOrCode?.productTypeCode ?? '');
    return (code || '').toString().trim().toUpperCase() === 'VCX';
  }

  /**
   * Get product totals
   */
  getProductTotals(productId: string): { premium: number; vat: number; premiumWithVAT: number } {
    const coverages = this.productCoverages[productId] || [];
    let premium = 0;
    let vat = 0;
    let premiumWithVAT = 0;

    coverages.forEach(item => {
      if (!item.isSectionHeader) {
        premium += item.premium || 0;
        vat += item.vat || 0;
        premiumWithVAT += item.premiumWithVAT || 0;
      }
    });

    return { premium, vat, premiumWithVAT };
  }

  private toLocalIsoDate(date: Date): string {
    const pad2 = (n: number) => String(n).padStart(2, '0');
    const y = date.getFullYear();
    const m = pad2(date.getMonth() + 1);
    const d = pad2(date.getDate());
    return `${y}-${m}-${d}`;
  }
}
