import { Component, OnInit, HostListener } from '@angular/core';
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
import { FileUploadModule } from 'primeng/fileupload';

import { VTable } from '@/shared/components/v-table/v-table';
import { TableColumn } from '@/shared/models/table-column.model';
import { TableAction } from '@/shared/models/table-action.model';
import { ResCarModelService } from '@/proxy/master/controllers/res-car-model.service';
import { ResCarBrandService } from '@/proxy/master/controllers/res-car-brand.service';
import { ResCarModelDto, CreateResCarModelDto, UpdateResCarModelDto, GetResCarModelsInput } from '@/proxy/master/res-car-models/models';
import { GetResCarBrandsInput } from '@/proxy/master/res-car-brands/models';
import { ResCarBrandStatus } from '@/proxy/res-car-brands/res-car-brand-status.enum';
import { CarModelSearchForm, CarModelFormData } from './car-models.models';
import { PermissionPipe } from '@/core/pipes/permission.pipe';
import { PermissionService } from '@/core/services/permission.service';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';
import { environment } from '@environments/environment';

@Component({
  selector: 'app-car-models',
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
    FileUploadModule,
    VTable,
    PermissionPipe,
    TranslatePipe
  ],
  templateUrl: './car-models.component.html',
  styleUrl: './car-models.component.scss',
  providers: [MessageService, ConfirmationService]
})
export class CarModelsComponent implements OnInit {
  // Permissions
  readonly PERMISSIONS = {
    CREATE: 'MasterResCarModel.Create',
    UPDATE: 'MasterResCarModel.Edit',
    DELETE: 'MasterResCarModel.Delete',
    VIEW: 'MasterResCarModel.View'
  };

  // Data
  carModels: ResCarModelDto[] = [];
  totalCount = 0;
  loading = true;
  pageSize = 10;

  // Dialog
  dialogVisible = false;
  dialogMode: 'create' | 'edit' = 'create';
  formData: CarModelFormData = this.getEmptyForm();
  selectedCarModel?: ResCarModelDto;

  // Import Dialog
  importDialogVisible = false;
  importResult: any = null;
  importing = false;

  // Search form
  searchForm: CarModelSearchForm = {
    carBrandId: null,
    code: null,
    name: null,
    status: null
  };

  // Options
  statusOptions: Array<{ label: string; value: ResCarBrandStatus }> = [];
  carBrandOptions: Array<{ label: string; value: string }> = [];
  fullCarBrandOptions: Array<{ label: string; value: string }> = [];

  // Table
  columns: TableColumn[] = [];
  actions: TableAction<ResCarModelDto>[] = [];
  currentLazyLoadEvent?: TableLazyLoadEvent;

  constructor(
    private carModelService: ResCarModelService,
    private carBrandService: ResCarBrandService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private permissionService: PermissionService,
    private localizationService: LocalizationService,
    private http: HttpClient
  ) {
    this.initializeColumns();
    this.initializeActions();
    this.initializeStatusOptions();
    this.loadCarBrands();
  }

  ngOnInit(): void {
    // Initial load will be triggered by the table's onLazyLoad event
  }

  /**
   * Load car brands for dropdown
   */
  private loadCarBrands(): void {
    const input: GetResCarBrandsInput = {
      skipCount: 0,
      maxResultCount: 1000
    };

    this.carBrandService.getList(input).subscribe({
      next: (result) => {
        this.carBrandOptions = (result.items || []).filter(brand => brand.status === ResCarBrandStatus.Active).map(brand => ({
          label: `${brand.code} - ${brand.name}`,
          value: brand.id || ''
        }));
        this.fullCarBrandOptions = (result.items || []).map(brand => ({
          label: `${brand.code} - ${brand.name}`,
          value: brand.id || ''
        }));
      },
      error: () => {
        // Silently fail - car brands will just be empty
      }
    });
  }

  /**
   * Initialize status options with localized labels
   */
  private initializeStatusOptions(): void {
    this.statusOptions = [
      { label: this.localizationService.localize('Master::ResCarModel:Active'), value: ResCarBrandStatus.Active },
      { label: this.localizationService.localize('Master::ResCarModel:Deactive'), value: ResCarBrandStatus.Deactive }
    ];
  }

  /**
   * Initialize table columns with localized headers
   */
  private initializeColumns(): void {
    this.columns = [
      {
        field: 'carBrandId',
        header: this.localizationService.localize('Master::ResCarModel:CarBrand'),
        sortable: false,
        width: '200px',
        formatter: (value: any, row: ResCarModelDto) => this.getCarBrandName(row.carBrandId || '')
      },
      {
        field: 'code',
        header: this.localizationService.localize('Master::ResCarModel:Code'),
        sortable: true,
        width: '150px'
      },
      {
        field: 'name',
        header: this.localizationService.localize('Master::ResCarModel:Name'),
        sortable: true,
        width: '250px'
      },
      {
        field: 'description',
        header: this.localizationService.localize('Master::ResCarModel:Description'),
        sortable: false,
        width: '300px'
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
        header: this.localizationService.localize('Master::ResCarModel:Status'),
        sortable: true,
        type: 'text',
        width: '120px',
        freeze: 'right',
        align: 'center',
        formatter: (value: any) => this.formatStatus(value),
        cellClass: (value: any) => {
          const statusValue = typeof value === 'number' ? value :
            (value === ResCarBrandStatus.Active ? 0 : 1);
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
   * Load car models with lazy loading
   */
  loadData(event: TableLazyLoadEvent): void {
    this.currentLazyLoadEvent = event;
    this.loading = true;

    const input: GetResCarModelsInput = {
      skipCount: event.first || 0,
      maxResultCount: event.rows || 10,
      sorting: this.getSortingString(event),
      carBrandId: this.searchForm.carBrandId || undefined,
      code: this.searchForm.code || undefined,
      name: this.searchForm.name || undefined,
      status: this.searchForm.status ?? undefined
    };

    this.carModelService.getList(input).subscribe({
      next: (result) => {
        this.carModels = result.items || [];
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
    // Only trigger search if dialog is not visible
    if (!this.dialogVisible) {
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
      carBrandId: null,
      code: null,
      name: null,
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
    this.selectedCarModel = undefined;
    this.dialogVisible = true;
  }

  /**
   * Open edit dialog
   * ⚠️ QUAN TRỌNG: Code và CarBrandId không được phép sửa khi edit
   */
  openEditDialog(carModel: ResCarModelDto): void {
    this.dialogMode = 'edit';
    this.selectedCarModel = carModel;
    this.formData = {
      carBrandId: carModel.carBrandId || '',
      code: carModel.code || '', // Readonly trong edit mode
      name: carModel.name || '',
      description: carModel.description || '',
      status: carModel.status ?? ResCarBrandStatus.Active
    };
    this.dialogVisible = true;
  }

  /**
   * Save (create or update)
   */
  save(): void {
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
   * Validate form
   */
  private validateForm(): boolean {
    if (this.dialogMode === 'create') {
      // Validate carBrandId only when creating
      if (!this.formData.carBrandId || this.formData.carBrandId.trim() === '') {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: this.localizationService.localize('Master::ResCarModel:CarBrandIdRequired')
        });
        return false;
      }

      // Validate code only when creating
      if (!this.formData.code || this.formData.code.trim() === '') {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: this.localizationService.localize('Master::ResCarModel:CodeRequired')
        });
        return false;
      }

      if (this.formData.code.length > 50) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: this.localizationService.localize('Master::ResCarModel:CodeMaxLength')
        });
        return false;
      }

      if (!this.validateCode(this.formData.code)) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: this.localizationService.localize('Master::ResCarModel:CodeInvalid')
        });
        return false;
      }
    }

    if (!this.formData.name || this.formData.name.trim() === '') {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResCarModel:NameRequired')
      });
      return false;
    }

    if (this.formData.name.length > 250) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResCarModel:NameMaxLength')
      });
      return false;
    }

    if (this.formData.description && this.formData.description.length > 500) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResCarModel:DescriptionMaxLength')
      });
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
   * Translate error message (handles both single and double colon formats)
   * Also replaces placeholders like {Code} with values from error context
   */
  translateErrorMessage(message: string, error?: any): string {
    if (!message) {
      return message;
    }

    // Normalize single colon to double colon format for localization keys
    // e.g., "Master:ResCarModel:CodeExists" -> "Master::ResCarModel:CodeExists"
    if (message.includes(':') && !message.includes('::')) {
      // Check if it looks like a localization key (starts with "Master:")
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

    // Replace placeholders if they exist in the translated message
    // Check for {Code} placeholder and replace with value from error context or error.value
    if (translated.includes('{Code}')) {
      const codeValue = error?.value || error?.error?.data?.Code || '';
      translated = translated.replace(/{Code}/g, codeValue);
    }

    // Replace {0} placeholder (used in some error messages)
    if (translated.includes('{0}')) {
      const value = error?.value || '';
      translated = translated.replace(/\{0\}/g, value);
    }

    return translated;
  }

  /**
   * Create new car model
   */
  private create(): void {
    this.loading = true;

    const createDto: CreateResCarModelDto = {
      carBrandId: this.formData.carBrandId.trim(),
      code: this.formData.code.trim(),
      name: this.formData.name.trim(),
      description: this.formData.description?.trim() || undefined,
      status: this.formData.status
    };

    this.carModelService.create(createDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Master::Success'),
          detail: this.localizationService.localize('Master::ResCarModel:CreatedSuccessfully')
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
   * Update existing car model
   * ⚠️ QUAN TRỌNG: Chỉ update Name, Description và Status, không update Code và CarBrandId
   */
  private update(): void {
    if (!this.selectedCarModel || !this.selectedCarModel.id) {
      return;
    }

    this.loading = true;

    const updateDto: UpdateResCarModelDto = {
      name: this.formData.name.trim(),
      description: this.formData.description?.trim() || undefined,
      status: this.formData.status
    };

    this.carModelService.update(this.selectedCarModel.id, updateDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Master::Success'),
          detail: this.localizationService.localize('Master::ResCarModel:UpdatedSuccessfully')
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
   * Delete car model with confirmation
   */
  delete(carModel: ResCarModelDto): void {
    if (!carModel.id) {
      return;
    }

    this.confirmationService.confirm({
      message: this.localizationService.localize('Master::ResCarModel:DeleteConfirm'),
      header: this.localizationService.localize('Master::ResCarModel:Delete'),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.loading = true;
        this.carModelService.delete(carModel.id!).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: this.localizationService.localize('Master::Success'),
              detail: this.localizationService.localize('Master::ResCarModel:DeletedSuccessfully')
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
  private getEmptyForm(): CarModelFormData {
    return {
      carBrandId: '',
      code: '',
      name: '',
      description: '',
      status: ResCarBrandStatus.Active
    };
  }

  /**
   * Format status for display
   */
  formatStatus(status: ResCarBrandStatus | number | string | undefined | null): string {
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
      statusValue = status === ResCarBrandStatus.Active ? 0 : 1;
    }

    return statusValue === 0
      ? this.localizationService.localize('Master::ResCarModel:Active')
      : this.localizationService.localize('Master::ResCarModel:Deactive');
  }

  /**
   * Get car brand name by ID
   */
  getCarBrandName(carBrandId: string): string {
    const brand = this.fullCarBrandOptions.find(b => b.value === carBrandId);
    return brand ? brand.label : carBrandId;
  }

  /**
   * Open import dialog
   */
  openImportDialog(): void {
    this.importDialogVisible = true;
    this.importResult = null;
  }

  /**
   * Handle file upload for Excel import
   */
  onFileSelect(event: any): void {
    const file = event.files?.[0];
    if (!file) {
      return;
    }

    // Validate file type
    const validExtensions = ['.xlsx', '.xls'];
    const fileExtension = file.name.substring(file.name.lastIndexOf('.')).toLowerCase();
    if (!validExtensions.includes(fileExtension)) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResCarModel:InvalidFileType')
      });
      return;
    }

    this.importing = true;

    this.carModelService.importExcel(file).subscribe({
      next: (result: any) => {
        this.importResult = result;
        this.importing = false;

        if (result.errorCount === 0) {
          this.messageService.add({
            severity: 'success',
            summary: this.localizationService.localize('Master::Success'),
            detail: `${this.localizationService.localize('Master::ResCarModel:ImportSuccess')} (${result.successCount} ${this.localizationService.localize('Master::ResCarModel:Records')})`
          });
          this.search(); // Refresh the list
        } else {
          this.messageService.add({
            severity: 'warn',
            summary: this.localizationService.localize('Master::Warning'),
            detail: `${this.localizationService.localize('Master::ResCarModel:ImportPartialSuccess')} - ${this.localizationService.localize('Master::ResCarModel:SuccessCount')}: ${result.successCount}, ${this.localizationService.localize('Master::ResCarModel:ErrorCount')}: ${result.errorCount}`
          });
        }
      },
      error: (error) => {
        this.importing = false;
        let errorMessage = error.error?.error?.message ||
          error.error?.error?.details ||
          this.localizationService.localize('Master::InternalServerErrorMessage');
        errorMessage = this.translateErrorMessage(errorMessage);
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: errorMessage
        });
      }
    });
  }

  /**
   * Download Excel template
   */
  downloadTemplate(): void {
    this.loading = true;

    // Get API base URL from environment configuration
    const apiConfig = environment.apis['Master'] || environment.apis.default;
    const apiUrl = apiConfig?.url || 'https://localhost:44360';
    const apiEndpoint = `${apiUrl}/api/master/car-models/export-template`;

    // Use HttpClient to download the file as blob
    // ABP's HTTP interceptor will automatically add authentication headers
    this.http.get(apiEndpoint, {
      responseType: 'blob'
    }).subscribe({
      next: (blob: Blob) => {
        // Create a blob URL and trigger download
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;

        // Generate filename with timestamp
        const timestamp = new Date().toISOString().replace(/[:.]/g, '-').slice(0, -5);
        link.download = `ResCarModel_Template_${timestamp}.xlsx`;

        // Trigger download
        document.body.appendChild(link);
        link.click();

        // Cleanup
        setTimeout(() => {
          document.body.removeChild(link);
          window.URL.revokeObjectURL(url);
        }, 100);

        this.loading = false;
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Master::Success'),
          detail: this.localizationService.localize('Master::ResCarModel:TemplateDownloaded')
        });
      },
      error: (error) => {
        this.loading = false;

        // Try to parse error message from blob if response is blob
        if (error.error instanceof Blob) {
          const reader = new FileReader();
          reader.onload = () => {
            try {
              const errorObj = JSON.parse(reader.result as string);
              const errorMessage = errorObj?.error?.message ||
                errorObj?.error?.details ||
                this.localizationService.localize('Master::InternalServerErrorMessage');
              this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('Master::Error'),
                detail: this.translateErrorMessage(errorMessage)
              });
            } catch {
              this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('Master::Error'),
                detail: this.localizationService.localize('Master::InternalServerErrorMessage')
              });
            }
          };
          reader.readAsText(error.error);
        } else {
          let errorMessage = error.error?.error?.message ||
            error.error?.error?.details ||
            this.localizationService.localize('Master::InternalServerErrorMessage');
          errorMessage = this.translateErrorMessage(errorMessage);
          this.messageService.add({
            severity: 'error',
            summary: this.localizationService.localize('Master::Error'),
            detail: errorMessage
          });
        }
      }
    });
  }
}

