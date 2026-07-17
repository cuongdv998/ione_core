import { Component, OnInit, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
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
import { ResCarCategoryService } from '@/proxy/master/controllers/res-car-category.service';
import { ResCarCategoryDto, CreateResCarCategoryDto, UpdateResCarCategoryDto, GetResCarCategoriesInput } from '@/proxy/master/res-car-categories/models';
import { ResCarCategoryStatus } from '@/proxy/res-car-categories/res-car-category-status.enum';
import { ResMotorClassService } from '@/proxy/master/controllers/res-motor-class.service';
import { ResMotorClassDto, GetResMotorClassesInput } from '@/proxy/master/res-motor-classes/models';
import { ResMotorClassStatus } from '@/proxy/res-motor-classes/res-motor-class-status.enum';
import { ResCarBrandService } from '@/proxy/master/controllers/res-car-brand.service';
import { ResCarBrandDto, GetResCarBrandsInput } from '@/proxy/master/res-car-brands/models';
import { ResCarBrandStatus } from '@/proxy/res-car-brands/res-car-brand-status.enum';
import { ResCarModelService } from '@/proxy/master/controllers/res-car-model.service';
import { ResCarModelDto, GetResCarModelsInput } from '@/proxy/master/res-car-models/models';
import { ResCarLineService } from '@/proxy/master/controllers/res-car-line.service';
import { ResCarLineDto, GetResCarLinesInput } from '@/proxy/master/res-car-lines/models';
import { ResCarLineStatus } from '@/proxy/res-car-lines/res-car-line-status.enum';
import { CarCategorySearchForm, CarCategoryFormData } from './car-categories.models';
import { PermissionPipe } from '@/core/pipes/permission.pipe';
import { PermissionService } from '@/core/services/permission.service';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';
import { environment } from '@environments/environment';

@Component({
  selector: 'app-car-categories',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    PanelModule,
    ButtonModule,
    InputTextModule,
    InputNumberModule,
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
  templateUrl: './car-categories.component.html',
  styleUrl: './car-categories.component.scss',
  providers: [MessageService, ConfirmationService]
})
export class CarCategoriesComponent implements OnInit {
  // Permissions
  readonly PERMISSIONS = {
    CREATE: 'MasterResCarCategory.Create',
    UPDATE: 'MasterResCarCategory.Edit',
    DELETE: 'MasterResCarCategory.Delete',
    VIEW: 'MasterResCarCategory.View'
  };

  // Data
  carCategories: ResCarCategoryDto[] = [];
  totalCount = 0;
  loading = true;
  pageSize = 10;

  // Dialog
  dialogVisible = false;
  dialogMode: 'create' | 'edit' = 'create';
  formData: CarCategoryFormData = this.getEmptyForm();
  selectedCarCategory?: ResCarCategoryDto;

  // Import Dialog
  importDialogVisible = false;
  importResult: any = null;
  importing = false;

  // Search form
  searchForm: CarCategorySearchForm = {
    code: null,
    name: null,
    status: null
  };

  // Status options
  statusOptions: Array<{ label: string; value: ResCarCategoryStatus }> = [];

  // Motor Class options for dropdown
  motorClassOptions: Array<{ label: string; value: string }> = [];
  loadingMotorClasses = false;

  // Car Brand options for dropdown
  carBrandOptions: Array<{ label: string; value: string }> = [];
  loadingCarBrands = false;
  allCarBrandOptions: Array<{ label: string; value: string }> = [];

  // Car Model options for dropdown (filtered by selected brand)
  carModelOptions: Array<{ label: string; value: string }> = [];
  allCarModels: ResCarModelDto[] = []; // Store all models for filtering
  loadingCarModels = false;

  // Car Line options for dropdown
  fullCarLineOptions: Array<{ label: string; value: string }> = [];
  carLineOptions: Array<{ label: string; value: string }> = [];
  loadingCarLines = false;

  // Table
  columns: TableColumn[] = [];
  actions: TableAction<ResCarCategoryDto>[] = [];
  currentLazyLoadEvent?: TableLazyLoadEvent;

  constructor(
    private carCategoryService: ResCarCategoryService,
    private motorClassService: ResMotorClassService,
    private carBrandService: ResCarBrandService,
    private carModelService: ResCarModelService,
    private carLineService: ResCarLineService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private permissionService: PermissionService,
    private localizationService: LocalizationService,
    private http: HttpClient
  ) {
    this.initializeColumns();
    this.initializeActions();
    this.initializeStatusOptions();
  }

  ngOnInit(): void {
    this.loadMotorClasses();
    this.loadCarBrands();
    this.loadCarModels();
    this.loadCarLines();
  }

  /**
   * Load motor classes for dropdown
   */
  private loadMotorClasses(): void {
    this.loadingMotorClasses = true;
    const input: GetResMotorClassesInput = {
      skipCount: 0,
      maxResultCount: 1000, // Get all active motor classes
      status: ResMotorClassStatus.Active,
      sorting: 'name asc'
    };

    this.motorClassService.getList(input).subscribe({
      next: (result) => {
        this.motorClassOptions = (result.items || []).map(mc => ({
          label: `${mc.code} - ${mc.name}`,
          value: mc.id!
        }));
        this.loadingMotorClasses = false;
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: this.localizationService.localize('Master::InternalServerErrorMessage')
        });
        this.loadingMotorClasses = false;
      }
    });
  }

  /**
   * Load car brands for dropdown
   */
  private loadCarBrands(): void {
    this.loadingCarBrands = true;
    const input: GetResCarBrandsInput = {
      skipCount: 0,
      maxResultCount: 1000,
      sorting: 'name asc'
    };

    this.carBrandService.getList(input).subscribe({
      next: (result) => {
        this.allCarBrandOptions = (result.items || []).map(brand => ({
          label: `${brand.code} - ${brand.name}`,
          value: brand.id || ''
        }));

        this.carBrandOptions = (result.items || []).filter(x => x.status === ResCarBrandStatus.Active).map(brand => ({
          label: `${brand.code} - ${brand.name}`,
          value: brand.id || ''
        }));
        this.loadingCarBrands = false;
      },
      error: () => {
        // Silently fail - car brands will just be empty
        this.loadingCarBrands = false;
      }
    });
  }

  /**
   * Load all car models for dropdown (will be filtered by selected brand)
   */
  private loadCarModels(): void {
    this.loadingCarModels = true;
    const input: GetResCarModelsInput = {
      skipCount: 0,
      maxResultCount: 1000,
      sorting: 'name asc'
    };

    this.carModelService.getList(input).subscribe({
      next: (result) => {
        this.allCarModels = result.items || [];
        this.filterCarModels();
        this.loadingCarModels = false;
      },
      error: () => {
        // Silently fail - car models will just be empty
        this.loadingCarModels = false;
      }
    });
  }

  /**
   * Filter car models based on selected car brand
   */
  private filterCarModels(): void {
    if (!this.formData.carBrandId) {
      this.carModelOptions = [];
      return;
    }

    let filtered = this.allCarModels.filter(model => model.carBrandId === this.formData.carBrandId);

    // In create mode, filter only active models. In edit mode, show all (to preserve existing selection even if inactive)
    if (this.dialogMode === 'create') {
      filtered = filtered.filter(model => model.status === ResCarBrandStatus.Active);
    }

    this.carModelOptions = filtered.map(model => ({
      label: `${model.code} - ${model.name}`,
      value: model.id || ''
    }));

    // Clear car model selection if current selection is not in filtered list
    if (this.formData.carModelId && !filtered.find(m => m.id === this.formData.carModelId)) {
      this.formData.carModelId = null;
    }
  }

  /**
   * Handle car brand selection change
   */
  onCarBrandChange(): void {
    this.filterCarModels();
    // Clear car model when brand changes
    this.formData.carModelId = null;
  }

  /**
   * Load car lines for dropdown
   */
  private loadCarLines(): void {
    this.loadingCarLines = true;
    const input: GetResCarLinesInput = {
      skipCount: 0,
      maxResultCount: 1000,
      sorting: 'name asc'
    };

    this.carLineService.getList(input).subscribe({
      next: (result) => {
        this.carLineOptions = (result.items || []).filter(line => line.status === ResCarLineStatus.Active).map(line => ({
          label: `${line.code} - ${line.name}`,
          value: line.id || ''
        }));
        this.fullCarLineOptions = (result.items || []).map(line => ({
          label: `${line.code} - ${line.name}`,
          value: line.id || ''
        }));
        this.loadingCarLines = false;
      },
      error: () => {
        // Silently fail - car lines will just be empty
        this.loadingCarLines = false;
      }
    });
  }

  /**
   * Initialize status options with localized labels
   */
  private initializeStatusOptions(): void {
    this.statusOptions = [
      { label: this.localizationService.localize('Master::ResCarCategory:Active'), value: ResCarCategoryStatus.Active },
      { label: this.localizationService.localize('Master::ResCarCategory:Deactive'), value: ResCarCategoryStatus.Deactive }
    ];
  }

  /**
   * Initialize table columns with localized headers
   */
  private initializeColumns(): void {
    this.columns = [
      {
        field: 'code',
        header: this.localizationService.localize('Master::ResCarCategory:Code'),
        sortable: true,
        width: '150px'
      },
      {
        field: 'name',
        header: this.localizationService.localize('Master::ResCarCategory:Name'),
        sortable: true,
        width: '250px'
      },
      {
        field: 'seatNumber',
        header: this.localizationService.localize('Master::ResCarCategory:SeatNumber'),
        sortable: true,
        type: 'number',
        width: '120px',
        align: 'center'
      },
      {
        field: 'description',
        header: this.localizationService.localize('Master::ResCarCategory:Description'),
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
        header: this.localizationService.localize('Master::ResCarCategory:Status'),
        sortable: true,
        type: 'text',
        width: '120px',
        freeze: 'right',
        align: 'center',
        formatter: (value: any) => this.formatStatus(value),
        cellClass: (value: any) => {
          const statusValue = typeof value === 'number' ? value :
            (value === ResCarCategoryStatus.Active ? 0 : 1);
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
   * Load car categories with lazy loading
   */
  loadData(event: TableLazyLoadEvent): void {
    this.currentLazyLoadEvent = event;
    this.loading = true;

    const input: GetResCarCategoriesInput = {
      skipCount: event.first || 0,
      maxResultCount: event.rows || 10,
      sorting: this.getSortingString(event),
      code: this.searchForm.code || undefined,
      name: this.searchForm.name || undefined,
      status: this.searchForm.status ?? undefined
    };

    this.carCategoryService.getList(input).subscribe({
      next: (result) => {
        this.carCategories = result.items || [];
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
    this.selectedCarCategory = undefined;
    this.dialogVisible = true;
  }

  /**
   * Open edit dialog
   * ⚠️ QUAN TRỌNG: Code, CarBrandId, CarModelId, MotorClassId, CarLineId không được phép sửa
   */
  openEditDialog(carCategory: ResCarCategoryDto): void {
    this.dialogMode = 'edit';
    this.selectedCarCategory = carCategory;
    this.formData = {
      carBrandId: carCategory.carBrandId || '',
      carModelId: carCategory.carModelId || null,
      motorClassId: carCategory.motorClassId || '',
      carLineId: carCategory.carLineId || null,
      code: carCategory.code || '', // Readonly trong edit mode
      name: carCategory.name || '',
      seatNumber: carCategory.seatNumber || 0,
      description: carCategory.description || '',
      status: carCategory.status ?? ResCarCategoryStatus.Active
    };
    // Filter car models based on the selected car brand when editing
    this.filterCarModels();
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
      // Validate code only when creating
      if (!this.formData.code || this.formData.code.trim() === '') {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: this.localizationService.localize('Master::ResCarCategory:CodeRequired')
        });
        return false;
      }

      if (this.formData.code.length > 50) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: this.localizationService.localize('Master::ResCarCategory:CodeMaxLength')
        });
        return false;
      }

      if (!this.validateCode(this.formData.code)) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: this.localizationService.localize('Master::ResCarCategory:CodeInvalid')
        });
        return false;
      }

      // Validate required fields for create
      if (!this.formData.carBrandId || this.formData.carBrandId.trim() === '') {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: this.localizationService.localize('Master::ResCarCategory:CarBrandIdRequired')
        });
        return false;
      }

      if (!this.formData.motorClassId || this.formData.motorClassId.trim() === '') {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: this.localizationService.localize('Master::ResCarCategory:MotorClassIdRequired')
        });
        return false;
      }
    }

    if (!this.formData.name || this.formData.name.trim() === '') {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResCarCategory:NameRequired')
      });
      return false;
    }

    if (this.formData.name.length > 250) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResCarCategory:NameMaxLength')
      });
      return false;
    }

    if (this.formData.seatNumber < 0 || this.formData.seatNumber > 99) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResCarCategory:SeatNumberRange')
      });
      return false;
    }

    if (this.formData.description && this.formData.description.length > 500) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResCarCategory:DescriptionMaxLength')
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
    // e.g., "Master:ResCarCategory:CodeExists" -> "Master::ResCarCategory:CodeExists"
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
   * Create new car category
   */
  private create(): void {
    this.loading = true;

    const createDto: CreateResCarCategoryDto = {
      carBrandId: this.formData.carBrandId.trim(),
      carModelId: this.formData.carModelId?.trim() || undefined,
      motorClassId: this.formData.motorClassId.trim(),
      carLineId: this.formData.carLineId?.trim() || undefined,
      code: this.formData.code.trim(),
      name: this.formData.name.trim(),
      seatNumber: this.formData.seatNumber,
      description: this.formData.description?.trim() || undefined,
      status: this.formData.status
    };

    this.carCategoryService.create(createDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Master::Success'),
          detail: this.localizationService.localize('Master::ResCarCategory:CreatedSuccessfully')
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
   * Update existing car category
   * ⚠️ QUAN TRỌNG: Chỉ update Name, SeatNumber, Description và Status, không update Code, CarBrandId, CarModelId, MotorClassId, CarLineId
   */
  private update(): void {
    if (!this.selectedCarCategory || !this.selectedCarCategory.id) {
      return;
    }

    this.loading = true;

    const updateDto: UpdateResCarCategoryDto = {
      name: this.formData.name.trim(),
      seatNumber: this.formData.seatNumber,
      description: this.formData.description?.trim() || undefined,
      status: this.formData.status
    };

    this.carCategoryService.update(this.selectedCarCategory.id, updateDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Master::Success'),
          detail: this.localizationService.localize('Master::ResCarCategory:UpdatedSuccessfully')
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
   * Delete car category with confirmation
   */
  delete(carCategory: ResCarCategoryDto): void {
    if (!carCategory.id) {
      return;
    }

    this.confirmationService.confirm({
      message: this.localizationService.localize('Master::ResCarCategory:DeleteConfirm'),
      header: this.localizationService.localize('Master::ResCarCategory:Delete'),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.loading = true;
        this.carCategoryService.delete(carCategory.id!).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: this.localizationService.localize('Master::Success'),
              detail: this.localizationService.localize('Master::ResCarCategory:DeletedSuccessfully')
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
  private getEmptyForm(): CarCategoryFormData {
    return {
      carBrandId: '',
      carModelId: null,
      motorClassId: '',
      carLineId: null,
      code: '',
      name: '',
      seatNumber: 0,
      description: '',
      status: ResCarCategoryStatus.Active
    };
  }

  /**
   * Format status for display
   */
  formatStatus(status: ResCarCategoryStatus | number | string | undefined | null): string {
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
      statusValue = status === ResCarCategoryStatus.Active ? 0 : 1;
    }

    return statusValue === 0
      ? this.localizationService.localize('Master::ResCarCategory:Active')
      : this.localizationService.localize('Master::ResCarCategory:Deactive');
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
        detail: this.localizationService.localize('Master::ResCarCategory:InvalidFileType')
      });
      return;
    }

    this.importing = true;

    this.carCategoryService.importExcel(file).subscribe({
      next: (result: any) => {
        this.importResult = result;
        this.importing = false;

        if (result.errorCount === 0) {
          this.messageService.add({
            severity: 'success',
            summary: this.localizationService.localize('Master::Success'),
            detail: `${this.localizationService.localize('Master::ResCarCategory:ImportSuccess')} (${result.successCount} ${this.localizationService.localize('Master::ResCarCategory:Records')})`
          });
          this.search(); // Refresh the list
        } else {
          this.messageService.add({
            severity: 'warn',
            summary: this.localizationService.localize('Master::Warning'),
            detail: `${this.localizationService.localize('Master::ResCarCategory:ImportPartialSuccess')} - ${this.localizationService.localize('Master::ResCarCategory:SuccessCount')}: ${result.successCount}, ${this.localizationService.localize('Master::ResCarCategory:ErrorCount')}: ${result.errorCount}`
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
    const apiEndpoint = `${apiUrl}/api/master/car-categories/export-template`;

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
        link.download = `ResCarCategory_Template_${timestamp}.xlsx`;

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
          detail: this.localizationService.localize('Master::ResCarCategory:TemplateDownloaded')
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

