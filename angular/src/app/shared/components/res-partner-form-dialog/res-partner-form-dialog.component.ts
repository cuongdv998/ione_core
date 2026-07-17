import {
  Component,
  EventEmitter,
  Input,
  OnChanges,
  Output,
  SimpleChanges,
  ViewChild,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { TextareaModule } from 'primeng/textarea';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { DialogModule } from 'primeng/dialog';
import { TabsModule } from 'primeng/tabs';
import { DatePickerModule } from 'primeng/datepicker';
import { TableModule } from 'primeng/table';
import { InputNumberModule } from 'primeng/inputnumber';

import { TableColumn } from '@/shared/models/table-column.model';
import { ResPartnerService } from '@/proxy/partner/controllers/res-partner.service';
import { ResPartnerDto, CreateResPartnerDto, UpdateResPartnerDto } from '@/proxy/partner/res-partners/models';
import { ResPartnerStatus } from '@/proxy/res-partners/res-partner-status.enum';
import { ResProvinceStatus } from '@/proxy/res-provinces/res-province-status.enum';
import { ResWardStatus } from '@/proxy/res-wards/res-ward-status.enum';
import { OrganizationTypeType } from '@/proxy/res-organization-types/organization-type-type.enum';
import { ResPartnerFormData, ResPartnerAgreementFormData } from '@/pages/partner/res-partners/res-partners.models';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';
import { ResPartnerTypeService } from '@/proxy/partner/controllers/res-partner-type.service';
import { ResPartnerTypeDto } from '@/proxy/partner/res-partner-types/models';
import { ResOrganizationTypeService } from '@/proxy/partner/controllers/res-organization-type.service';
import { ResOrganizationTypeDto } from '@/proxy/partner/res-organization-types/models';
import { ResProvinceService } from '@/proxy/master/controllers/res-province.service';
import { ResWardService } from '@/proxy/master/controllers/res-ward.service';
import { ResChannelService } from '@/proxy/partner/controllers/res-channel.service';
import { ResAgreementTermService } from '@/proxy/partner/controllers/res-agreement-term.service';
import { AdminConfigService } from '@/proxy/master/controllers/admin-config.service';

@Component({
  selector: 'app-res-partner-form-dialog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ButtonModule,
    InputTextModule,
    SelectModule,
    TextareaModule,
    ToastModule,
    DialogModule,
    TabsModule,
    DatePickerModule,
    TableModule,
    InputNumberModule,
    TranslatePipe,
  ],
  templateUrl: './res-partner-form-dialog.component.html',
  styleUrl: './res-partner-form-dialog.component.scss',
  providers: [MessageService],
})
export class ResPartnerFormDialogComponent implements OnChanges {
  @Input() visible = false;
  @Output() visibleChange = new EventEmitter<boolean>();
  @Input() mode: 'create' | 'edit' = 'create';
  @Input() partnerId: string | null = null;
  /** When set in create mode: pre-fill and lock partner type (e.g. GARAGE). */
  @Input() defaultPartnerTypeCode: string | null = null;
  @Output() saved = new EventEmitter<ResPartnerDto>();

  saving = false;
  formData: ResPartnerFormData = this.getEmptyForm();
  selectedResPartner?: ResPartnerDto;
  activeTabValue = '0';
  @ViewChild('partnerForm') partnerForm?: NgForm;

  statusOptions: Array<{ label: string; value: ResPartnerStatus }> = [];
  partnerTypeOptions: Array<{ label: string; value: string }> = [];
  private partnerTypeItems: ResPartnerTypeDto[] = [];
  organizationTypeOptions: Array<{ label: string; value: string }> = [];
  organizationTypeDetails: Map<string, ResOrganizationTypeDto> = new Map();
  provinceOptions: Array<{ label: string; value: string }> = [];
  wardOptions: Array<{ label: string; value: string }> = [];
  invoiceWardOptions: Array<{ label: string; value: string }> = [];
  channelOptions: Array<{ label: string; value: string }> = [];
  partnerRoleOptions: Array<{ label: string; value: string }> = [];
  agreementTermOptions: Array<{ label: string; value: string }> = [];

  agreementColumns: TableColumn[] = [];

  private isRemovingAgreement = false;

  constructor(
    private resPartnerService: ResPartnerService,
    private partnerTypeService: ResPartnerTypeService,
    private organizationTypeService: ResOrganizationTypeService,
    private provinceService: ResProvinceService,
    private wardService: ResWardService,
    private channelService: ResChannelService,
    private agreementTermService: ResAgreementTermService,
    private adminConfigService: AdminConfigService,
    private messageService: MessageService,
    private localizationService: LocalizationService,
  ) {
    this.initializeStatusOptions();
    this.initializeAgreementColumns();
    this.loadDropdownOptions();
  }

  get isPartnerTypeLocked(): boolean {
    return this.mode === 'create' && !!this.defaultPartnerTypeCode?.trim();
  }

  /**
   * Format date to YYYY-MM-DD using local timezone (avoids UTC shift)
   */
  private formatLocalDate(date: Date): string {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  ngOnChanges(changes: SimpleChanges): void {
    const vis = changes['visible'];
    if (vis && vis.currentValue === true && vis.previousValue !== true) {
      this.onDialogOpened();
    }
  }

  onVisibleChange(open: boolean): void {
    this.visibleChange.emit(open);
  }

  closeDialog(): void {
    this.visibleChange.emit(false);
  }

  private onDialogOpened(): void {
    if (this.mode === 'create') {
      this.formData = this.getEmptyForm();
      this.wardOptions = [];
      this.invoiceWardOptions = [];
      this.selectedResPartner = undefined;
      this.activeTabValue = '0';
      this.tryApplyDefaultPartnerType();
      setTimeout(() => {
        this.partnerForm?.resetForm(this.formData);
      }, 0);
    } else if (this.mode === 'edit' && this.partnerId) {
      this.loadPartnerForEdit(this.partnerId);
    }
  }

  private tryApplyDefaultPartnerType(): void {
    if (this.mode !== 'create' || !this.defaultPartnerTypeCode?.trim()) {
      return;
    }
    const code = this.defaultPartnerTypeCode.toUpperCase();
    const match = this.partnerTypeItems.find(t => (t.code || '').toUpperCase() === code);
    if (match?.id) {
      this.formData.partnerTypeId = match.id;
    }
  }

  private loadPartnerForEdit(id: string): void {
    this.resPartnerService.get(id).subscribe({
      next: fullPartner => {
        this.selectedResPartner = fullPartner;
        this.formData = {
          code: fullPartner.code || '',
          name: fullPartner.name || '',
          partnerTypeId: fullPartner.partnerTypeId || '',
          partnerRole: fullPartner.partnerRole || null,
          organizationTypeId: fullPartner.organizationTypeId || null,
          channelId: fullPartner.channelId || null,
          provinceId: fullPartner.provinceId || '',
          wardId: fullPartner.wardId || '',
          address: fullPartner.address || '',
          fullAddress: fullPartner.fullAddress || '',
          email: fullPartner.email || null,
          phone: fullPartner.phone || '',
          note: fullPartner.note || null,
          status: fullPartner.status ?? ResPartnerStatus.Active,
          invoiceProvinceId: fullPartner.invoiceProvinceId || null,
          invoiceWardId: fullPartner.invoiceWardId || null,
          invoiceAddress: fullPartner.invoiceAddress || null,
          invoiceFullAddress: fullPartner.invoiceFullAddress || null,
          idNo: fullPartner.idNo || null,
          tin: fullPartner.tin || null,
          repName: fullPartner.repName || null,
          repEmail: fullPartner.repEmail || null,
          repPhone: fullPartner.repPhone || null,
          repIdNo: fullPartner.repIdNo || null,
          repTitle: fullPartner.repTitle || null,
          authorizer: fullPartner.authorizer || null,
          authorizerPhone: fullPartner.authorizerPhone || null,
          authorizerEmail: fullPartner.authorizerEmail || null,
          authorizerNo: fullPartner.authorizerNo || null,
          authorizerDate: fullPartner.authorizerDate ? new Date(fullPartner.authorizerDate) : null,
          authorizerTitle: fullPartner.authorizerTitle || null,
          businessNo: fullPartner.businessNo || null,
          agreements: (fullPartner.agreements || []).map(a => ({
            id: a.id,
            agreementTermId: a.agreementTermId || '',
            value: a.value || '',
            effectDate: a.effectDate ? new Date(a.effectDate) : new Date(),
            expireDate: a.expireDate ? new Date(a.expireDate) : null,
          })),
        };

        if (this.formData.provinceId) {
          this.loadWards(this.formData.provinceId);
        }
        if (this.formData.invoiceProvinceId) {
          this.loadInvoiceWards(this.formData.invoiceProvinceId);
        }

        setTimeout(() => {
          if (this.partnerForm) {
            this.partnerForm.form.markAsPristine();
            this.partnerForm.form.markAsUntouched();
          }
        });
      },
      error: () => {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Partner::Error'),
          detail: this.localizationService.localize('Partner::InternalServerErrorMessage'),
        });
        this.visibleChange.emit(false);
      },
    });
  }

  /**
   * Load all dropdown options
   */
  private loadDropdownOptions(): void {
    this.loadPartnerTypes();
    this.loadOrganizationTypes();
    this.loadProvinces();
    this.loadChannels();
    this.loadAgreementTerms();
    this.loadPartnerRoles();
  }

  /**
   * Load partner types
   */
  private loadPartnerTypes(): void {
    this.partnerTypeService
      .getList({
        skipCount: 0,
        maxResultCount: 1000,
      })
      .subscribe({
        next: result => {
          this.partnerTypeItems = result.items || [];
          this.partnerTypeOptions = this.partnerTypeItems.map(item => ({
            label: item.name || '',
            value: item.id || '',
          }));
          this.tryApplyDefaultPartnerType();
        },
        error: () => {
          // Silently fail
        },
      });
  }

  /**
   * Load organization types
   */
  private loadOrganizationTypes(): void {
    this.organizationTypeService.getList({
      skipCount: 0,
      maxResultCount: 1000
    }).subscribe({
      next: (result) => {
        this.organizationTypeOptions = (result.items || []).map(item => ({
          label: item.name || '',
          value: item.id || ''
        }));
        // Cache organization type details
        (result.items || []).forEach(item => {
          if (item.id) {
            this.organizationTypeDetails.set(item.id, item);
          }
        });
      },
      error: () => {
        // Silently fail
      }
    });
  }

  /**
   * Load provinces
   */
  private loadProvinces(): void {
    this.provinceService.getList({
      skipCount: 0,
      maxResultCount: 1000,
      status: ResProvinceStatus.Active,
    }).subscribe({
      next: (result) => {
        this.provinceOptions = (result.items || []).map(item => ({
          label: item.name || '',
          value: item.id || ''
        }));
      },
      error: () => {
        // Silently fail
      }
    });
  }

  /**
   * Load wards (will be filtered by selected province)
   */
  loadWards(provinceId?: string | null): void {
    if (!provinceId) {
      this.wardOptions = [];
      return;
    }

    this.wardService.getList({
      skipCount: 0,
      maxResultCount: 1000,
      provinceId: provinceId,
      status: ResWardStatus.Active,
    }).subscribe({
      next: (result) => {
        this.wardOptions = (result.items || []).map(item => ({
          label: item.name || '',
          value: item.id || ''
        }));
      },
      error: () => {
        this.wardOptions = [];
      }
    });
  }

  /**
   * Load invoice wards (will be filtered by selected invoice province)
   */
  loadInvoiceWards(provinceId?: string | null): void {
    if (!provinceId) {
      // Clear invoice ward if province is cleared
      if (!this.formData.invoiceProvinceId) {
        this.formData.invoiceWardId = null;
      }
      return;
    }

    this.wardService.getList({
      skipCount: 0,
      maxResultCount: 1000,
      provinceId: provinceId,
      status: ResWardStatus.Active,
    }).subscribe({
      next: (result) => {
        this.invoiceWardOptions = (result.items || []).map(item => ({
          label: item.name || '',
          value: item.id || ''
        }));
      },
      error: () => {
        // Silently fail
      }
    });
  }

  /**
   * Load channels
   */
  private loadChannels(): void {
    this.channelService.getList({
      skipCount: 0,
      maxResultCount: 1000
    }).subscribe({
      next: (result) => {
        this.channelOptions = (result.items || []).map(item => ({
          label: item.name || '',
          value: item.id || ''
        }));
      },
      error: () => {
        // Silently fail
      }
    });
  }

  /**
   * Load agreement terms
   */
  private loadAgreementTerms(): void {
    this.agreementTermService.getList({
      skipCount: 0,
      maxResultCount: 1000
    }).subscribe({
      next: (result) => {
        this.agreementTermOptions = (result.items || []).map(item => ({
          label: item.name || '',
          value: item.id || ''
        }));
      },
      error: () => {
        // Silently fail
      }
    });
  }

  /**
   * Load partner roles from AdminConfig (code = 'PARTNER_ROLE')
   */
  private loadPartnerRoles(): void {
    this.adminConfigService.getList({
      skipCount: 0,
      maxResultCount: 1000,
      code: 'PARTNER_ROLE'
    }).subscribe({
      next: (result) => {
        // Map AdminConfig items: display value, store subCode
        this.partnerRoleOptions = (result.items || []).map(item => ({
          label: item.value || item.subCode || '',
          value: item.subCode || '' // Store subCode as value
        }));
      },
      error: () => {
        // Silently fail
      }
    });
  }

  /**
   * Initialize status options with localized labels
   */
  private initializeStatusOptions(): void {
    this.statusOptions = [
      { label: this.localizationService.localize('Partner::ResPartner:Status:Active'), value: ResPartnerStatus.Active },
      { label: this.localizationService.localize('Partner::ResPartner:Status:Deactive'), value: ResPartnerStatus.Deactive }
    ];
  }

  /**
   * Initialize agreement table columns
   */
  private initializeAgreementColumns(): void {
    this.agreementColumns = [
      {
        field: 'agreementTermName',
        header: this.localizationService.localize('Partner::ResPartnerAgreement:AgreementTermName'),
        sortable: false,
        width: '200px'
      },
      {
        field: 'value',
        header: this.localizationService.localize('Partner::ResPartnerAgreement:Value'),
        sortable: false,
        width: '150px',
        align: 'right',
        formatter: (value: any) => this.formatCurrency(value)
      },
      {
        field: 'effectDate',
        header: this.localizationService.localize('Partner::ResPartnerAgreement:EffectDate'),
        sortable: false,
        type: 'date',
        width: '150px'
      },
      {
        field: 'expireDate',
        header: this.localizationService.localize('Partner::ResPartnerAgreement:ExpireDate'),
        sortable: false,
        type: 'date',
        width: '150px'
      }
    ];
  }

  /**
   * Save (create or update)
   */
  save(): void {
    if (!this.validateForm()) {
      return;
    }

    if (this.mode === 'create') {
      this.create();
    } else {
      this.update();
    }
  }

  /**
   * Validate form
   */
  private validateForm(): boolean {
    if (this.mode === 'create') {
      if (!this.formData.code || this.formData.code.trim() === '') {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: this.localizationService.localize('Partner::ResPartner:CodeRequired')
        });
        return false;
      }
      if (!this.validateCode(this.formData.code)) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: this.localizationService.localize('Partner::ResPartner:CodeInvalid')
        });
        return false;
      }
    }

    if (!this.formData.name || this.formData.name.trim() === '') {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Partner::ResPartner:NameRequired')
      });
      return false;
    }

    if (!this.formData.partnerTypeId) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Partner::ResPartner:PartnerTypeRequired')
      });
      return false;
    }

    if (!this.formData.provinceId) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Partner::ResPartner:ProvinceRequired')
      });
      return false;
    }

    if (!this.formData.wardId) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Partner::ResPartner:WardRequired')
      });
      return false;
    }

    if (!this.formData.address || this.formData.address.trim() === '') {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Partner::ResPartner:AddressRequired')
      });
      return false;
    }

    if (!this.formData.phone || this.formData.phone.trim() === '') {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Partner::ResPartner:PhoneRequired')
      });
      return false;
    }

    // Validate agreements date ranges
    if (!this.validateAgreements()) {
      return false;
    }

    return true;
  }

  /**
   * Validate code format (A-Z, 0-9, _)
   */
  validateCode(code: string): boolean {
    return /^[A-Z0-9_]+$/.test(code);
  }

  /**
   * Convert code to uppercase on input
   */
  onCodeInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    const value = input.value.toUpperCase();
    input.value = value;
    this.formData.code = value;
  }

  /**
   * Validate agreements date ranges
   */
  private validateAgreements(): boolean {
    // Basic validation for each agreement
    for (const agreement of this.formData.agreements) {
      if (!agreement.agreementTermId) continue;

      if (!agreement.expireDate) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: this.localizationService.localize('Partner::ResPartner:ExpireDateRequired')
        });
        return false;
      }

      if (agreement.effectDate && agreement.expireDate < agreement.effectDate) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: this.localizationService.localize('Partner::ResPartner:ExpireDateInvalid')
        });
        return false;
      }
    }

    // Group by agreementTermId and check for overlaps
    const agreementsByTerm = new Map<string, ResPartnerAgreementFormData[]>();

    for (const agreement of this.formData.agreements) {
      const termId = agreement.agreementTermId;
      if (!termId) continue; // Skip if no term selected

      if (!agreementsByTerm.has(termId)) {
        agreementsByTerm.set(termId, []);
      }
      agreementsByTerm.get(termId)!.push(agreement);
    }

    // Check overlaps for each term
    for (const [termId, agreements] of agreementsByTerm) {
      for (let i = 0; i < agreements.length; i++) {
        for (let j = i + 1; j < agreements.length; j++) {
          const a1 = agreements[i];
          const a2 = agreements[j];

          if (this.isDateRangeOverlap(
            a1.effectDate,
            a1.expireDate,
            a2.effectDate,
            a2.expireDate
          )) {
            this.messageService.add({
              severity: 'error',
              summary: this.localizationService.localize('AbpUi::Error'),
              detail: this.localizationService.localize('Partner::ResPartner:AgreementDateOverlap')
            });
            return false;
          }
        }
      }
    }

    return true;
  }

  /**
   * Check if two date ranges overlap
   */
  private isDateRangeOverlap(
    start1: Date | null,
    end1: Date | null,
    start2: Date | null,
    end2: Date | null
  ): boolean {
    const actualStart1 = start1 ? start1.getTime() : 0;
    const actualStart2 = start2 ? start2.getTime() : 0;
    const end1Time = end1 ? end1.getTime() : Number.MAX_SAFE_INTEGER;
    const end2Time = end2 ? end2.getTime() : Number.MAX_SAFE_INTEGER;

    return actualStart1 <= end2Time && actualStart2 <= end1Time;
  }

  /**
   * Create new res partner
   */
  private create(): void {
    this.saving = true;

    const createDto: CreateResPartnerDto = {
      code: this.formData.code?.trim() || undefined,
      name: this.formData.name.trim(),
      partnerTypeId: this.formData.partnerTypeId,
      partnerRole: this.formData.partnerRole || undefined,
      organizationTypeId: this.formData.organizationTypeId || undefined,
      channelId: this.formData.channelId || undefined,
      provinceId: this.formData.provinceId,
      wardId: this.formData.wardId,
      address: this.formData.address.trim(),
      email: this.formData.email?.trim() || undefined,
      phone: this.formData.phone.trim(),
      note: this.formData.note?.trim() || undefined,
      status: this.formData.status,
      invoiceProvinceId: this.formData.invoiceProvinceId || undefined,
      invoiceWardId: this.formData.invoiceWardId || undefined,
      invoiceAddress: this.formData.invoiceAddress?.trim() || undefined,
      idNo: this.formData.idNo?.trim() || undefined,
      tin: this.formData.tin?.trim() || undefined,
      repName: this.formData.repName?.trim() || undefined,
      repEmail: this.formData.repEmail?.trim() || undefined,
      repPhone: this.formData.repPhone?.trim() || undefined,
      repIdNo: this.formData.repIdNo?.trim() || undefined,
      repTitle: this.formData.repTitle?.trim() || undefined,
      authorizer: this.formData.authorizer?.trim() || undefined,
      authorizerPhone: this.formData.authorizerPhone?.trim() || undefined,
      authorizerEmail: this.formData.authorizerEmail?.trim() || undefined,
      authorizerNo: this.formData.authorizerNo?.trim() || undefined,
      authorizerDate: this.formData.authorizerDate ? this.formatLocalDate(this.formData.authorizerDate) : undefined,
      authorizerTitle: this.formData.authorizerTitle?.trim() || undefined,
      businessNo: this.formData.businessNo?.trim() || undefined,
      agreements:
        this.formData.agreements.length > 0
          ? this.getUniqueAgreements().map(a => {
              const mapped = {
                agreementTermId: a.agreementTermId,
                value: (a.value ?? 0).toString(),
                effectDate: a.effectDate ? this.formatLocalDate(a.effectDate) : undefined,
                expireDate: a.expireDate ? this.formatLocalDate(a.expireDate) : '',
              };
              return mapped;
            })
          : [],
    };

    this.resPartnerService.create(createDto).subscribe({
      next: created => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Partner::Success'),
          detail: this.localizationService.localize('Partner::ResPartner:CreatedSuccessfully'),
        });
        this.saving = false;
        this.visibleChange.emit(false);
        this.saved.emit(created);
      },
      error: error => {
        const errorMessage =
          error.error?.error?.message ||
          error.error?.error?.details ||
          this.localizationService.localize('AbpUi::InternalServerErrorMessage');
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Partner::Error'),
          detail: errorMessage,
        });
        this.saving = false;
      },
    });
  }

  /**
   * Update existing res partner
   */
  private update(): void {
    if (!this.selectedResPartner || !this.selectedResPartner.id) {
      return;
    }

    this.saving = true;

    const updateDto: UpdateResPartnerDto = {
      name: this.formData.name.trim(),
      partnerTypeId: this.formData.partnerTypeId,
      partnerRole: this.formData.partnerRole || undefined,
      organizationTypeId: this.formData.organizationTypeId || undefined,
      channelId: this.formData.channelId || undefined,
      provinceId: this.formData.provinceId,
      wardId: this.formData.wardId,
      address: this.formData.address.trim(),
      email: this.formData.email?.trim() || undefined,
      phone: this.formData.phone.trim(),
      note: this.formData.note?.trim() || undefined,
      status: this.formData.status,
      invoiceProvinceId: this.formData.invoiceProvinceId || undefined,
      invoiceWardId: this.formData.invoiceWardId || undefined,
      invoiceAddress: this.formData.invoiceAddress?.trim() || undefined,
      idNo: this.formData.idNo?.trim() || undefined,
      tin: this.formData.tin?.trim() || undefined,
      repName: this.formData.repName?.trim() || undefined,
      repEmail: this.formData.repEmail?.trim() || undefined,
      repPhone: this.formData.repPhone?.trim() || undefined,
      repIdNo: this.formData.repIdNo?.trim() || undefined,
      repTitle: this.formData.repTitle?.trim() || undefined,
      authorizer: this.formData.authorizer?.trim() || undefined,
      authorizerPhone: this.formData.authorizerPhone?.trim() || undefined,
      authorizerEmail: this.formData.authorizerEmail?.trim() || undefined,
      authorizerNo: this.formData.authorizerNo?.trim() || undefined,
      authorizerDate: this.formData.authorizerDate ? this.formatLocalDate(this.formData.authorizerDate) : undefined,
      authorizerTitle: this.formData.authorizerTitle?.trim() || undefined,
      businessNo: this.formData.businessNo?.trim() || undefined,
      agreements: this.getUniqueAgreements().map(a => ({
        id: a.id,
        agreementTermId: a.agreementTermId,
        value: a.value.toString(),
        effectDate: a.effectDate ? this.formatLocalDate(a.effectDate) : undefined,
        expireDate: a.expireDate ? this.formatLocalDate(a.expireDate) : '',
      })),
    };

    this.resPartnerService.update(this.selectedResPartner.id, updateDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Partner::Success'),
          detail: this.localizationService.localize('Partner::ResPartner:UpdatedSuccessfully'),
        });
        this.saving = false;
        this.visibleChange.emit(false);
        this.saved.emit(this.selectedResPartner!);
      },
      error: error => {
        const errorMessage =
          error.error?.error?.message ||
          error.error?.error?.details ||
          this.localizationService.localize('AbpUi::InternalServerErrorMessage');
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Partner::Error'),
          detail: errorMessage,
        });
        this.saving = false;
      },
    });
  }

  /**
   * Get empty form data
   */
  private getEmptyForm(): ResPartnerFormData {
    return {
      code: '',
      name: '',
      partnerTypeId: '',
      partnerRole: null,
      organizationTypeId: null,
      channelId: null,
      provinceId: '',
      wardId: '',
      address: '',
      fullAddress: '', // Will be computed
      email: null,
      phone: '',
      note: null,
      status: ResPartnerStatus.Active,
      invoiceProvinceId: null,
      invoiceWardId: null,
      invoiceAddress: null,
      invoiceFullAddress: null, // Will be computed
      idNo: null,
      tin: null,
      repName: null,
      repEmail: null,
      repPhone: null,
      repIdNo: null,
      repTitle: null,
      authorizer: null,
      authorizerPhone: null,
      authorizerEmail: null,
      authorizerNo: null,
      authorizerDate: null,
      authorizerTitle: null,
      businessNo: null,
      agreements: []
    };
  }

  /**
   * Format currency for display
   */
  formatCurrency(value: any): string {
    if (value === null || value === undefined || value === '') {
      return '';
    }

    const num = typeof value === 'string' ? parseFloat(value) : value;
    if (isNaN(num)) {
      return value;
    }

    return new Intl.NumberFormat('vi-VN').format(num);
  }

  /**
   * Check if organization type is CN (Cá nhân)
   */
  isOrganizationTypeCN(): boolean {
    if (!this.formData.organizationTypeId) {
      return false;
    }

    const orgType = this.organizationTypeDetails.get(this.formData.organizationTypeId);
    if (!orgType) {
      return false;
    }

    return orgType.type === OrganizationTypeType.CN;
  }

  /**
   * Check if organization type is TC (Tổ chức)
   */
  isOrganizationTypeTC(): boolean {
    if (!this.formData.organizationTypeId) {
      return false;
    }

    const orgType = this.organizationTypeDetails.get(this.formData.organizationTypeId);
    if (!orgType) {
      return false;
    }

    return orgType.type === OrganizationTypeType.TC;
  }


  /**
   * Handle province change - reload wards
   */
  onProvinceChange(): void {
    this.formData.wardId = ''; // Clear ward selection
    this.loadWards(this.formData.provinceId);
    this.updateFullAddress();
  }

  /**
   * Handle ward change - update full address
   */
  onWardChange(): void {
    this.updateFullAddress();
  }

  /**
   * Handle address change - update full address
   */
  onAddressChange(): void {
    this.updateFullAddress();
  }

  /**
   * Update full address (computed from address + ward + province)
   */
  private updateFullAddress(): void {
    if (!this.formData.address || !this.formData.wardId || !this.formData.provinceId) {
      this.formData.fullAddress = '';
      return;
    }

    const ward = this.wardOptions.find(w => w.value === this.formData.wardId);
    const province = this.provinceOptions.find(p => p.value === this.formData.provinceId);

    if (ward && province) {
      this.formData.fullAddress = `${this.formData.address}, ${ward.label}, ${province.label}`;
    } else {
      this.formData.fullAddress = this.formData.address;
    }
  }

  /**
   * Handle invoice province change
   */
  onInvoiceProvinceChange(): void {
    this.formData.invoiceWardId = null;
    this.loadInvoiceWards(this.formData.invoiceProvinceId);
    this.updateInvoiceFullAddress();
  }

  /**
   * Handle invoice ward change
   */
  onInvoiceWardChange(): void {
    this.updateInvoiceFullAddress();
  }

  /**
   * Handle invoice address change
   */
  onInvoiceAddressChange(): void {
    this.updateInvoiceFullAddress();
  }

  /**
   * Update invoice full address
   */
  private updateInvoiceFullAddress(): void {
    if (!this.formData.invoiceAddress || !this.formData.invoiceProvinceId) {
      this.formData.invoiceFullAddress = null;
      return;
    }

    const invoiceProvince = this.provinceOptions.find(p => p.value === this.formData.invoiceProvinceId);
    if (!invoiceProvince) {
      this.formData.invoiceFullAddress = this.formData.invoiceAddress;
      return;
    }

    if (this.formData.invoiceWardId) {
      const invoiceWard = this.invoiceWardOptions.find(w => w.value === this.formData.invoiceWardId);
      if (invoiceWard) {
        this.formData.invoiceFullAddress = `${this.formData.invoiceAddress}, ${invoiceWard.label}, ${invoiceProvince.label}`;
      } else {
        this.formData.invoiceFullAddress = `${this.formData.invoiceAddress}, ${invoiceProvince.label}`;
      }
    } else {
      this.formData.invoiceFullAddress = `${this.formData.invoiceAddress}, ${invoiceProvince.label}`;
    }
  }

  /**
   * Add agreement to form
   */
  addAgreement(): void {
    // Prevent duplicate calls
    if (this.saving) {
      return;
    }

    // Generate a temporary unique ID for new agreements
    //const tempId = `temp_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;

    this.formData.agreements.push({
      //id: tempId,
      agreementTermId: '',
      value: 0,
      effectDate: new Date(),
      expireDate: null
    });
  }

  /**
   * Remove agreement from form by index
   */
  removeAgreementByIndex(index: number, event?: Event): void {
    // Prevent event bubbling
    if (event) {
      event.stopPropagation();
      event.preventDefault();
    }

    // Prevent duplicate calls
    if (this.saving || this.isRemovingAgreement) {
      return;
    }

    // Validate index
    if (index < 0 || index >= this.formData.agreements.length) {
      return;
    }

    // Set flag to prevent duplicate calls
    this.isRemovingAgreement = true;

    try {
      // Remove the agreement at the specified index using splice
      // This is more reliable than filter for PrimeNG table
      const agreements = [...this.formData.agreements];
      agreements.splice(index, 1);
      this.formData.agreements = agreements;
    } finally {
      // Reset flag after a short delay to allow for any pending operations
      setTimeout(() => {
        this.isRemovingAgreement = false;
      }, 100);
    }
  }

  /**
   * Track by function for agreements table
   */
  trackByAgreementIndex(index: number, agreement: ResPartnerAgreementFormData): string | number {
    // Use id if available, otherwise use index
    return agreement.id || index;
  }

  /**
   * Get unique agreements (remove duplicates based on agreementTermId, effectDate, expireDate)
   */
  private getUniqueAgreements(): ResPartnerAgreementFormData[] {
    const seen = new Set<string>();
    const unique: ResPartnerAgreementFormData[] = [];

    for (const agreement of this.formData.agreements) {
      // Skip empty agreements
      if (!agreement.agreementTermId || agreement.agreementTermId.trim() === '') {
        continue;
      }

      // Create a unique key based on agreementTermId, effectDate, and expireDate
      const effectDateStr = agreement.effectDate ? agreement.effectDate.toISOString().split('T')[0] : 'null';
      const expireDateStr = agreement.expireDate ? agreement.expireDate.toISOString().split('T')[0] : 'null';
      const key = `${agreement.agreementTermId}_${effectDateStr}_${expireDateStr}`;

      if (!seen.has(key)) {
        seen.add(key);
        unique.push(agreement);
      }
    }

    return unique;
  }

  /**
   * Load organization type details when organization type changes
   */
  onOrganizationTypeChange(): void {
    if (this.formData.organizationTypeId) {
      // Load organization type details if not cached
      if (!this.organizationTypeDetails.has(this.formData.organizationTypeId)) {
        this.organizationTypeService.get(this.formData.organizationTypeId).subscribe({
          next: (orgType) => {
            if (orgType.id) {
              this.organizationTypeDetails.set(orgType.id, orgType);
            }
            // Clear conditional fields based on type
            this.clearConditionalFields();
          },
          error: () => {
            // Silently fail
            this.clearConditionalFields();
          }
        });
      } else {
        this.clearConditionalFields();
      }
    } else {
      this.clearConditionalFields();
    }
  }

  /**
   * Clear conditional fields based on organization type
   */
  private clearConditionalFields(): void {
    if (this.isOrganizationTypeCN()) {
      // Clear TC fields
      this.formData.tin = null;
      this.formData.repName = null;
      this.formData.repEmail = null;
      this.formData.repPhone = null;
      this.formData.repIdNo = null;
      this.formData.repTitle = null;
      this.formData.authorizer = null;
      this.formData.authorizerPhone = null;
      this.formData.authorizerEmail = null;
      this.formData.authorizerNo = null;
      this.formData.authorizerDate = null;
      this.formData.authorizerTitle = null;
      this.formData.businessNo = null;
    } else if (this.isOrganizationTypeTC()) {
      // Clear CN fields
      this.formData.idNo = null;
    }
  }
}

