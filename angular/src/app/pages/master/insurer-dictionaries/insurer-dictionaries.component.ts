import { Component, HostListener, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { TableLazyLoadEvent } from 'primeng/table';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { DialogModule } from 'primeng/dialog';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TextareaModule } from 'primeng/textarea';
import { DatePickerModule } from 'primeng/datepicker';

import { VTable } from '@/shared/components/v-table/v-table';
import { TableColumn } from '@/shared/models/table-column.model';
import { TableAction } from '@/shared/models/table-action.model';
import { InsurerDictionaryService } from '@/proxy/master/controllers/insurer-dictionary.service';
import { AdminConfigService } from '@/proxy/master/controllers/admin-config.service';
import { ResPartnerService } from '@/proxy/partner/controllers/res-partner.service';
import { InsurerDictionaryDto, CreateInsurerDictionaryDto, UpdateInsurerDictionaryDto, GetInsurerDictionariesInput, ImportInsurerDictionaryResultDto } from '@/proxy/master/insurer-dictionaries/models';
import { environment } from '@environments/environment';
import { InsurerDictionaryStatus } from '@/proxy/insurer-dictionaries/insurer-dictionary-status.enum';
import { InsurerDictionarySearchForm, InsurerDictionaryFormData } from './insurer-dictionaries.models';
import { PermissionPipe } from '@/core/pipes/permission.pipe';
import { PermissionService } from '@/core/services/permission.service';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';

@Component({
  selector: 'app-insurer-dictionaries',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    PanelModule,
    ButtonModule,
    InputTextModule,
    SelectModule,
    ToastModule,
    DialogModule,
    ConfirmDialogModule,
    TextareaModule,
    DatePickerModule,
    VTable,
    PermissionPipe,
    TranslatePipe
  ],
  templateUrl: './insurer-dictionaries.component.html',
  styleUrl: './insurer-dictionaries.component.scss',
  providers: [MessageService, ConfirmationService]
})
export class InsurerDictionariesComponent implements OnInit {
  // Permissions
  readonly PERMISSIONS = {
    CREATE: 'MasterInsurerDictionary.Create',
    UPDATE: 'MasterInsurerDictionary.Edit',
    DELETE: 'MasterInsurerDictionary.Delete',
    VIEW: 'MasterInsurerDictionary.View'
  };

  // Data
  insurerDictionaries: InsurerDictionaryDto[] = [];
  totalCount = 0;
  loading = true;
  pageSize = 10;

  // Dialog
  dialogVisible = false;
  dialogMode: 'create' | 'edit' = 'create';
  formData: InsurerDictionaryFormData = this.getEmptyForm();
  selectedInsurerDictionary?: InsurerDictionaryDto;

  // Search form
  searchForm: InsurerDictionarySearchForm = {
    businessName: null,
    insurerId: null,
    ownCode: null,
    insurerCode: null,
    status: null
  };

  // Status options
  statusOptions: Array<{ label: string; value: InsurerDictionaryStatus }> = [];

  // Insurer (res_partner type INSURER) options
  insurerOptions: Array<{ label: string; value: string }> = [];

  // Tên bảng dữ liệu: admin_config code = TABLE_MAPPING_INSURER, sub_code = value, value = label
  businessNameOptions: Array<{ label: string; value: string }> = [];

  // Date pickers (Date object for p-datepicker; sync with formData when open/save)
  effectDatePicker: Date | null = null;
  expireDatePicker: Date | null = null;

  // Table
  columns: TableColumn[] = [];
  actions: TableAction<InsurerDictionaryDto>[] = [];
  currentLazyLoadEvent?: TableLazyLoadEvent;

  // Import
  importDialogVisible = false;
  importResult: ImportInsurerDictionaryResultDto | null = null;
  importing = false;

  constructor(
    private insurerDictionaryService: InsurerDictionaryService,
    private adminConfigService: AdminConfigService,
    private resPartnerService: ResPartnerService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private permissionService: PermissionService,
    private localizationService: LocalizationService,
    private http: HttpClient
  ) {
    this.initializeColumns();
    this.initializeActions();
    this.initializeStatusOptions();
    this.loadTableMappingOptions();
    this.loadInsurerOptions();
  }

  /** Load Tên bảng dữ liệu từ admin_config code = TABLE_MAPPING_INSURER (sub_code = value, value = label) */
  private loadTableMappingOptions(): void {
    this.adminConfigService
      .getList({ code: 'TABLE_MAPPING_INSURER', skipCount: 0, maxResultCount: 1000 })
      .subscribe({
        next: (result) => {
          const items = result.items || [];
          this.businessNameOptions = items.map((item) => ({
            label: item.value ?? item.subCode ?? '',
            value: item.subCode ?? ''
          }));
        },
        error: () => {}
      });
  }

  /** Load insurer partners (res_partner with partner_type code INSURER) */
  private loadInsurerOptions(): void {
    this.resPartnerService.getSelectList('INSURER').subscribe({
      next: (items) => {
        this.insurerOptions = (items || []).map((item) => ({
          label: item.name || item.code || '',
          value: item.id || ''
        }));
      },
      error: () => {}
    });
  }

  ngOnInit(): void {
    // Initial load will be triggered by the table's onLazyLoad event
  }

  /**
   * Initialize status options with localized labels
   */
  private initializeStatusOptions(): void {
    this.statusOptions = [
      { label: this.localizationService.localize('Master::InsurerDictionary:Active'), value: InsurerDictionaryStatus.Active },
      { label: this.localizationService.localize('Master::InsurerDictionary:Deactive'), value: InsurerDictionaryStatus.Deactive }
    ];
  }

  /**
   * Initialize table columns with localized headers
   */
  private initializeColumns(): void {
    this.columns = [
      {
        field: 'businessName',
        header: this.localizationService.localize('Master::InsurerDictionary:BusinessName'),
        sortable: true,
        width: '180px',
        formatter: (value: unknown, row: InsurerDictionaryDto) => this.getBusinessNameLabel(row.businessName ?? '')
      },
      {
        field: 'insurerId',
        header: this.localizationService.localize('Master::InsurerDictionary:InsurerId'),
        sortable: false,
        width: '200px',
        formatter: (value: unknown, row: InsurerDictionaryDto) => {
          const id = row.insurerId;
          if (!id) return '';
          const opt = this.insurerOptions.find((o) => o.value === id);
          return opt ? opt.label : id;
        }
      },
      {
        field: 'ownCode',
        header: this.localizationService.localize('Master::InsurerDictionary:OwnCode'),
        sortable: true,
        width: '130px'
      },
      {
        field: 'insurerCode',
        header: this.localizationService.localize('Master::InsurerDictionary:InsurerCode'),
        sortable: true,
        width: '150px'
      },
      {
        field: 'extraData',
        header: this.localizationService.localize('Master::InsurerDictionary:ExtraData'),
        sortable: false,
        width: '250px'
      },
      {
        field: 'effectDate',
        header: this.localizationService.localize('Master::InsurerDictionary:EffectDate'),
        sortable: true,
        type: 'date',
        width: '150px'
      },
      {
        field: 'expireDate',
        header: this.localizationService.localize('Master::InsurerDictionary:ExpireDate'),
        sortable: true,
        type: 'date',
        width: '150px'
      },
      {
        field: 'creationTime',
        header: this.localizationService.localize('AbpIdentity::CreationTime'),
        sortable: true,
        type: 'date',
        width: '180px'
      },
      {
        field: 'status',
        header: this.localizationService.localize('Master::InsurerDictionary:Status'),
        sortable: true,
        type: 'text',
        width: '120px',
        freeze: 'right',
        align: 'center',
        formatter: (value: any) => this.formatStatus(value),
        cellClass: (value: any) => {
          const statusValue = typeof value === 'number' ? value : 
            (value === InsurerDictionaryStatus.Active ? 0 : 1);
          return statusValue === 0 
            ? 'px-2 py-1 rounded text-xs font-semibold bg-green-100 text-green-800' 
            : 'px-2 py-1 rounded text-xs font-semibold bg-red-100 text-red-800';
        }
      }
    ];
  }

  /**
   * Initialize actions based on permissions
   */
  private initializeActions(): void {
    if (this.permissionService.isGranted(this.PERMISSIONS.UPDATE)) {
      this.actions.push({
        label: this.localizationService.localize('Master::Edit'),
        icon: 'pi pi-pencil',
        command: (row) => this.openEditDialog(row)
      });
    }

    if (this.permissionService.isGranted(this.PERMISSIONS.DELETE)) {
      this.actions.push({
        label: this.localizationService.localize('Master::Delete'),
        icon: 'pi pi-trash',
        command: (row) => this.delete(row)
      });
    }
  }

  /**
   * Load insurer dictionaries with lazy loading
   */
  loadData(event: TableLazyLoadEvent): void {
    this.currentLazyLoadEvent = event;
    this.loading = true;

    const input: GetInsurerDictionariesInput = {
      skipCount: event.first || 0,
      maxResultCount: event.rows || 10,
      sorting: this.getSortingString(event),
      businessName: this.searchForm.businessName || undefined,
      insurerId: this.searchForm.insurerId || undefined,
      ownCode: this.searchForm.ownCode || undefined,
      insurerCode: this.searchForm.insurerCode || undefined,
      status: this.searchForm.status ?? undefined
    };

    this.insurerDictionaryService.getList(input).subscribe({
      next: (result) => {
        this.insurerDictionaries = result.items || [];
        this.totalCount = result.totalCount || 0;
        this.loading = false;
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: this.localizationService.localize('Master::InternalServerErrorMessage')
        });
        this.loading = false;
      }
    });
  }

  /**
   * Get sorting string from lazy load event
   */
  private getSortingString(event: TableLazyLoadEvent): string | undefined {
    if (!event.sortField) {
      return undefined;
    }

    const sortOrder = event.sortOrder === 1 ? 'asc' : 'desc';
    return `${event.sortField} ${sortOrder}`;
  }

  @HostListener('window:keydown.enter', ['$event'])
  handleEnter(event: KeyboardEvent) {
    // Only trigger search if dialogs are not visible
    if (!this.dialogVisible && !this.importDialogVisible) {
      this.search();
    }
  }

  /**
   * Execute search with current filters
   */
  search(): void {
    if (this.currentLazyLoadEvent) {
      this.currentLazyLoadEvent.first = 0;
      this.loadData(this.currentLazyLoadEvent);
    }
  }

  /**
   * Reset search form and reload data
   */
  resetSearch(): void {
    this.searchForm = {
      businessName: null,
      insurerId: null,
      ownCode: null,
      insurerCode: null,
      status: null
    };
    this.search();
  }

  /**
   * Open create dialog
   */
  openCreateDialog(): void {
    this.dialogMode = 'create';
    this.formData = this.getEmptyForm();
    this.selectedInsurerDictionary = undefined;
    const today = new Date();
    this.effectDatePicker = today;
    this.expireDatePicker = null;
    this.dialogVisible = true;
  }

  /**
   * Open edit dialog.
   * BusinessName, InsurerId, OwnCode are readonly (not sent in UpdateDto).
   */
  openEditDialog(insurerDictionary: InsurerDictionaryDto): void {
    this.dialogMode = 'edit';
    this.selectedInsurerDictionary = insurerDictionary;
    this.formData = {
      businessName: insurerDictionary.businessName || '',
      insurerId: insurerDictionary.insurerId || '',
      ownCode: insurerDictionary.ownCode || '',
      insurerCode: insurerDictionary.insurerCode || '',
      extraData: insurerDictionary.extraData || '',
      status: insurerDictionary.status ?? InsurerDictionaryStatus.Active,
      effectDate: insurerDictionary.effectDate ? this.toYyyyMmDd(new Date(insurerDictionary.effectDate)) : '',
      expireDate: insurerDictionary.expireDate ? this.toYyyyMmDd(new Date(insurerDictionary.expireDate)) : ''
    };
    this.effectDatePicker = insurerDictionary.effectDate ? new Date(insurerDictionary.effectDate) : null;
    this.expireDatePicker = insurerDictionary.expireDate ? new Date(insurerDictionary.expireDate) : null;
    this.dialogVisible = true;
  }

  private toYyyyMmDd(d: Date): string {
    const y = d.getFullYear();
    const m = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${y}-${m}-${day}`;
  }

  /**
   * Save (create or update). Sync datepickers to formData before validate.
   */
  save(): void {
    this.formData.effectDate = this.effectDatePicker ? this.toYyyyMmDd(this.effectDatePicker) : '';
    this.formData.expireDate = this.expireDatePicker ? this.toYyyyMmDd(this.expireDatePicker) : '';
    if (!this.validateForm()) {
      return;
    }
    if (this.dialogMode === 'create') {
      this.create();
    } else {
      this.update();
    }
  }

  /**
   * Validate form. Create: all fields; Edit: only InsurerCode, ExtraData, Status, EffectDate, ExpireDate.
   */
  private validateForm(): boolean {
    if (this.dialogMode === 'create') {
      if (!this.formData.businessName || this.formData.businessName.trim() === '') {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: this.localizationService.localize('Master::InsurerDictionary:BusinessNameRequired')
        });
        return false;
      }
      if (this.formData.businessName.length > 50) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: this.localizationService.localize('Master::InsurerDictionary:BusinessNameMaxLength')
        });
        return false;
      }
      if (!this.formData.insurerId || this.formData.insurerId.trim() === '') {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: this.localizationService.localize('Master::InsurerDictionary:InsurerIdRequired')
        });
        return false;
      }
      if (!this.formData.ownCode || this.formData.ownCode.trim() === '') {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: this.localizationService.localize('Master::InsurerDictionary:OwnCodeRequired')
        });
        return false;
      }
      if (this.formData.ownCode.length > 50) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: this.localizationService.localize('Master::InsurerDictionary:OwnCodeMaxLength')
        });
        return false;
      }
    }

    if (!this.formData.insurerCode || this.formData.insurerCode.trim() === '') {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::InsurerDictionary:InsurerCodeRequired')
      });
      return false;
    }

    if (this.formData.insurerCode.length > 50) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::InsurerDictionary:InsurerCodeMaxLength')
      });
      return false;
    }

    if (!this.formData.effectDate) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::InsurerDictionary:EffectDateRequired')
      });
      return false;
    }

    // Ngày hết hạn không bắt buộc (cả thêm và sửa).
    if (this.formData.expireDate) {
      const effectDate = new Date(this.formData.effectDate);
      const expireDate = new Date(this.formData.expireDate);
      if (expireDate < effectDate) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: this.localizationService.localize('Master::InsurerDictionary:ExpireDateMustBeAfterEffectDate')
        });
        return false;
      }
    }

    return true;
  }

  /**
   * Translate error message (handles both single and double colon formats)
   */
  translateErrorMessage(message: string, error?: any): string {
    if (!message) {
      return message;
    }
    
    // Normalize single colon to double colon format for localization keys
    if (message.includes(':') && !message.includes('::')) {
      if (message.startsWith('Master:')) {
        message = message.replace('Master:', 'Master::');
      }
    }
    
    // Try to translate it
    let translated = this.localizationService.localize(message);
    
    // If translation returns the same key, it means it wasn't found, use original message
    if (translated === message) {
      translated = message;
    }
    
    return translated;
  }

  /**
   * Create new insurer dictionary
   */
  private create(): void {
    this.loading = true;

    const createDto: CreateInsurerDictionaryDto = {
      businessName: this.formData.businessName.trim(),
      insurerId: this.formData.insurerId.trim(),
      ownCode: this.formData.ownCode.trim(),
      insurerCode: this.formData.insurerCode.trim(),
      extraData: this.formData.extraData?.trim() || undefined,
      status: this.formData.status,
      effectDate: this.formData.effectDate,
      expireDate: this.formData.expireDate || undefined
    };

    this.insurerDictionaryService.create(createDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Master::Success'),
          detail: this.localizationService.localize('Master::InsurerDictionary:CreatedSuccessfully')
        });
        this.dialogVisible = false;
        this.search();
      },
      error: (error) => {
        let errorMessage = error.error?.error?.message || 
                           error.error?.error?.details || 
                           this.localizationService.localize('Master::InternalServerErrorMessage');
        errorMessage = this.translateErrorMessage(errorMessage);
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: errorMessage
        });
        this.loading = false;
      }
    });
  }

  /**
   * Update existing insurer dictionary. Only InsurerCode, ExtraData, Status, EffectDate, ExpireDate (backend does not allow changing BusinessName, InsurerId, OwnCode).
   */
  private update(): void {
    if (!this.selectedInsurerDictionary || !this.selectedInsurerDictionary.id) {
      return;
    }

    this.loading = true;

    const updateDto: UpdateInsurerDictionaryDto = {
      insurerCode: this.formData.insurerCode.trim(),
      extraData: this.formData.extraData?.trim() || undefined,
      status: this.formData.status,
      effectDate: this.formData.effectDate,
      expireDate: this.formData.expireDate || undefined
    };

    this.insurerDictionaryService.update(this.selectedInsurerDictionary.id, updateDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Master::Success'),
          detail: this.localizationService.localize('Master::InsurerDictionary:UpdatedSuccessfully')
        });
        this.dialogVisible = false;
        this.search();
      },
      error: (error) => {
        let errorMessage = error.error?.error?.message || 
                           error.error?.error?.details || 
                           this.localizationService.localize('Master::InternalServerErrorMessage');
        errorMessage = this.translateErrorMessage(errorMessage);
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: errorMessage
        });
        this.loading = false;
      }
    });
  }

  /**
   * Delete insurer dictionary with confirmation
   */
  delete(insurerDictionary: InsurerDictionaryDto): void {
    if (!insurerDictionary.id) {
      return;
    }

    this.confirmationService.confirm({
      message: this.localizationService.localize('Master::InsurerDictionary:DeleteConfirm'),
      header: this.localizationService.localize('Master::InsurerDictionary:Delete'),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.loading = true;
        this.insurerDictionaryService.delete(insurerDictionary.id!).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: this.localizationService.localize('Master::Success'),
              detail: this.localizationService.localize('Master::InsurerDictionary:DeletedSuccessfully')
            });
            this.search();
          },
          error: (error) => {
            const errorMessage = error.error?.error?.message || 
                               error.error?.error?.details || 
                               this.localizationService.localize('Master::InternalServerErrorMessage');
            this.messageService.add({
              severity: 'error',
              summary: this.localizationService.localize('Master::Error'),
              detail: errorMessage
            });
            this.loading = false;
          }
        });
      }
    });
  }

  /**
   * Get empty form data
   */
  private getEmptyForm(): InsurerDictionaryFormData {
    const today = this.toYyyyMmDd(new Date());
    return {
      businessName: '',
      insurerId: '',
      ownCode: '',
      insurerCode: '',
      extraData: '',
      status: InsurerDictionaryStatus.Active,
      effectDate: today,
      expireDate: ''
    };
  }

  /** Get Tên bảng dữ liệu label by sub_code (from admin_config TABLE_MAPPING_INSURER) */
  getBusinessNameLabel(businessName: string): string {
    if (!businessName) return '';
    const opt = this.businessNameOptions.find((o) => o.value === businessName);
    return opt ? opt.label : businessName;
  }

  /** Get insurer display label by id (for readonly in edit dialog) */
  getInsurerLabel(insurerId: string): string {
    if (!insurerId) return '';
    const opt = this.insurerOptions.find((o) => o.value === insurerId);
    return opt ? opt.label : insurerId;
  }

  /**
   * Get max date for effect date picker (expire date)
   */
  getMaxEffectDate(): Date | undefined {
    if (!this.formData.expireDate) {
      return undefined;
    }
    return new Date(this.formData.expireDate);
  }

  /**
   * Get min date for expire date picker (effect date)
   */
  getMinExpireDate(): Date | undefined {
    if (!this.formData.effectDate) {
      return undefined;
    }
    return new Date(this.formData.effectDate);
  }

  /**
   * Format status for display
   */
  formatStatus(status: InsurerDictionaryStatus | number | string | undefined | null): string {
    if (status === undefined || status === null) {
      return '';
    }
    
    let statusValue: number;
    
    if (typeof status === 'number') {
      statusValue = status;
    } else if (typeof status === 'string') {
      if (status === 'Active' || status === 'active' || status === '0') {
        statusValue = 0;
      } else if (status === 'Deactive' || status === 'deactive' || status === '1') {
        statusValue = 1;
      } else {
        statusValue = 1;
      }
    } else {
      statusValue = status === InsurerDictionaryStatus.Active ? 0 : 1;
    }
    
    return statusValue === 0
      ? this.localizationService.localize('Master::InsurerDictionary:Active')
      : this.localizationService.localize('Master::InsurerDictionary:Deactive');
  }

  /** Open import dialog */
  openImportDialog(): void {
    this.importResult = null;
    this.importDialogVisible = true;
  }

  /** Download Excel template */
  downloadTemplate(): void {
    this.loading = true;
    const apiConfig = environment.apis['Master'] || environment.apis.default;
    const apiUrl = apiConfig?.url || '';
    const apiEndpoint = `${apiUrl}/api/master/insurer-dictionaries/export-template`;

    this.http.get(apiEndpoint, { responseType: 'blob' }).subscribe({
      next: (blob: Blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `InsurerDictionary_Template_${new Date().toISOString().replace(/[:.]/g, '-').slice(0, -5)}.xlsx`;
        document.body.appendChild(link);
        link.click();
        setTimeout(() => {
          document.body.removeChild(link);
          window.URL.revokeObjectURL(url);
        }, 100);
        this.loading = false;
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Master::Success'),
          detail: this.localizationService.localize('Master::InsurerDictionary:DownloadTemplate') + ' OK'
        });
      },
      error: (err) => {
        this.loading = false;
        if (err.error instanceof Blob) {
          const reader = new FileReader();
          reader.onload = () => {
            try {
              const errorObj = JSON.parse(reader.result as string);
              const msg = errorObj?.error?.message || errorObj?.error?.details || 'Error';
              this.messageService.add({ severity: 'error', summary: this.localizationService.localize('Master::Error'), detail: msg });
            } catch {
              this.messageService.add({ severity: 'error', summary: this.localizationService.localize('Master::Error'), detail: 'Error' });
            }
          };
          reader.readAsText(err.error);
        } else {
          this.messageService.add({
            severity: 'error',
            summary: this.localizationService.localize('Master::Error'),
            detail: err.error?.error?.message || err.error?.error?.details || 'Error'
          });
        }
      }
    });
  }

  /** Handle file input change (hidden input triggered by Import button) */
  onImportFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    if (!file) return;
    this.doImport(file);
  }

  /** Run import with selected file */
  private doImport(file: File): void {
    const ext = file.name.substring(file.name.lastIndexOf('.')).toLowerCase();
    if (ext !== '.xlsx' && ext !== '.xls') {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::InsurerDictionary:ExcelFileInvalid')
      });
      return;
    }

    this.importing = true;
    this.insurerDictionaryService.importExcel(file).subscribe({
      next: (result) => {
        this.importResult = result;
        this.importing = false;
        const msg = `Kết quả import: ${result.successCount} thành công, ${result.errorCount} lỗi.`;
        this.messageService.add({
          severity: result.errorCount === 0 ? 'success' : 'warn',
          summary: this.localizationService.localize('Master::Success'),
          detail: msg
        });
        if (result.totalRows > 0) {
          this.search();
        }
        if (result.fileBase64 && result.fileName) {
          this.downloadResultFile(result.fileBase64, result.fileName);
        }
      },
      error: (err) => {
        this.importing = false;
        const detail = err.error?.error?.message || err.error?.error?.details || this.localizationService.localize('Master::InternalServerErrorMessage');
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: typeof detail === 'string' ? detail : JSON.stringify(detail)
        });
      }
    });
  }

  /** Trigger download of result file (base64) */
  private downloadResultFile(base64: string, fileName: string): void {
    try {
      const binary = atob(base64);
      const bytes = new Uint8Array(binary.length);
      for (let i = 0; i < binary.length; i++) {
        bytes[i] = binary.charCodeAt(i);
      }
      const blob = new Blob([bytes], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = fileName;
      document.body.appendChild(link);
      link.click();
      setTimeout(() => {
        document.body.removeChild(link);
        window.URL.revokeObjectURL(url);
      }, 100);
    } catch {
      // ignore
    }
  }
}
