import { Component, OnInit, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
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

import { VTable } from '@/shared/components/v-table/v-table';
import { TableColumn } from '@/shared/models/table-column.model';
import { TableAction } from '@/shared/models/table-action.model';
import { ResUomService } from '@/proxy/master/controllers/res-uom.service';
import { ResUomDto, CreateResUomDto, UpdateResUomDto, GetResUomsInput } from '@/proxy/master/res-uoms/models';
import { ResUomStatus } from '@/proxy/res-uoms/res-uom-status.enum';
import { ResUomType } from '@/proxy/res-uoms/res-uom-type.enum';
import { ResUomSearchForm, ResUomFormData } from './res-uoms.models';
import { PermissionPipe } from '@/core/pipes/permission.pipe';
import { PermissionService } from '@/core/services/permission.service';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';
import { ResUomClassService } from '@/proxy/master/res-uom-classes/res-uom-class.service';

@Component({
  selector: 'app-res-uoms',
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
    VTable,
    PermissionPipe,
    TranslatePipe
  ],
  templateUrl: './res-uoms.component.html',
  styleUrl: './res-uoms.component.scss',
  providers: [MessageService, ConfirmationService]
})
export class ResUomsComponent implements OnInit {
  // Permissions
  readonly PERMISSIONS = {
    CREATE: 'MasterResUom.Create',
    UPDATE: 'MasterResUom.Edit',
    DELETE: 'MasterResUom.Delete',
    VIEW: 'MasterResUom.View'
  };

  // Data
  resUoms: ResUomDto[] = [];
  totalCount = 0;
  loading = true;
  pageSize = 10;

  // Dialog
  dialogVisible = false;
  dialogMode: 'create' | 'edit' = 'create';
  formData: ResUomFormData = this.getEmptyForm();
  selectedResUom?: ResUomDto;

  // Search form
  searchForm: ResUomSearchForm = {
    classId: null,
    code: null,
    name: null,
    status: null,
    type: null
  };

  // Status options
  statusOptions: Array<{ label: string; value: ResUomStatus }> = [];

  // Type options
  typeOptions: Array<{ label: string; value: ResUomType }> = [];

  // UOM Class options for dropdown
  uomClassOptions: Array<{ label: string; value: string }> = [];
  loadingUomClasses = false;

  // InputNumber step (tránh tăng/giảm > 1 đơn vị khi bấm nút +/-)
  readonly roundingStep = 0.001;
  readonly factorStep = 0.001;

  // Table
  columns: TableColumn[] = [];
  actions: TableAction<ResUomDto>[] = [];
  currentLazyLoadEvent?: TableLazyLoadEvent;

  constructor(
    private resUomService: ResUomService,
    private uomClassService: ResUomClassService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private permissionService: PermissionService,
    private localizationService: LocalizationService
  ) {
    this.initializeColumns();
    this.initializeActions();
    this.initializeStatusOptions();
    this.initializeTypeOptions();
  }

  ngOnInit(): void {
    this.loadUomClasses();
  }

  /**
   * Load UOM classes for dropdown
   */
  private loadUomClasses(): void {
    this.loadingUomClasses = true;

    this.uomClassService.getSelectList().subscribe({
      next: (result) => {
        const list = result ?? [];
        this.uomClassOptions = list
          .filter((uc): uc is typeof uc & { id: string } => uc.id != null && uc.id !== '')
          .map(uc => ({
            label: `${uc.code ?? ''} - ${uc.name ?? ''}`,
            value: uc.id
          }));
        this.loadingUomClasses = false;
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: this.localizationService.localize('Master::InternalServerErrorMessage')
        });
        this.loadingUomClasses = false;
      }
    });
  }

  /**
   * Initialize status options with localized labels
   */
  private initializeStatusOptions(): void {
    this.statusOptions = [
      { label: this.localizationService.localize('Master::ResUom:Active'), value: ResUomStatus.Active },
      { label: this.localizationService.localize('Master::ResUom:Deactive'), value: ResUomStatus.Deactive }
    ];
  }

  /**
   * Initialize type options with localized labels
   */
  private initializeTypeOptions(): void {
    this.typeOptions = [
      { label: this.localizationService.localize('Master::ResUom:Ref'), value: ResUomType.Ref },
      { label: this.localizationService.localize('Master::ResUom:None'), value: ResUomType.None }
    ];
  }

  /**
   * Initialize table columns with localized headers
   */
  private initializeColumns(): void {
    this.columns = [
      {
        field: 'code',
        header: this.localizationService.localize('Master::ResUom:Code'),
        sortable: true,
        width: '150px'
      },
      {
        field: 'name',
        header: this.localizationService.localize('Master::ResUom:Name'),
        sortable: true,
        width: '250px'
      },
      {
        field: 'rounding',
        header: this.localizationService.localize('Master::ResUom:Rounding'),
        sortable: true,
        type: 'text',
        width: '120px',
        align: 'right',
        formatter: (value: any) => this.formatRounding(value)
      },
      {
        field: 'factor',
        header: this.localizationService.localize('Master::ResUom:Factor'),
        sortable: true,
        type: 'text',
        width: '150px',
        align: 'right',
        formatter: (value: any) => this.formatFactor(value)
      },
      {
        field: 'type',
        header: this.localizationService.localize('Master::ResUom:Type'),
        sortable: true,
        type: 'text',
        width: '120px',
        formatter: (value: any) => this.formatType(value)
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
        header: this.localizationService.localize('Master::ResUom:Status'),
        sortable: true,
        type: 'text',
        width: '120px',
        freeze: 'right',
        align: 'center',
        formatter: (value: any) => this.formatStatus(value),
        cellClass: (value: any) => {
          const statusValue = typeof value === 'number' ? value : 
            (value === ResUomStatus.Active ? 0 : 1);
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
   * Load res uoms with lazy loading
   */
  loadData(event: TableLazyLoadEvent): void {
    this.currentLazyLoadEvent = event;
    this.loading = true;

    const input: GetResUomsInput = {
      skipCount: event.first || 0,
      maxResultCount: event.rows || 10,
      sorting: this.getSortingString(event),
      classId: this.searchForm.classId || undefined,
      code: this.searchForm.code || undefined,
      name: this.searchForm.name || undefined,
      status: this.searchForm.status ?? undefined,
      type: this.searchForm.type ?? undefined
    };

    this.resUomService.getList(input).subscribe({
      next: (result) => {
        this.resUoms = result.items || [];
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

  /**
   * Trigger search on enter key down
   */
  @HostListener('window:keydown.enter', ['$event'])
  onWindowKeyDown(event: KeyboardEvent): void {
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
      classId: null,
      code: null,
      name: null,
      status: null,
      type: null
    };
    this.search();
  }

  /**
   * Open create dialog
   */
  openCreateDialog(): void {
    this.dialogMode = 'create';
    this.formData = this.getEmptyForm();
    this.selectedResUom = undefined;
    this.dialogVisible = true;
  }

  /**
   * Open edit dialog
   * ⚠️ QUAN TRỌNG: Code và ClassId không được phép sửa khi edit
   */
  openEditDialog(resUom: ResUomDto): void {
    this.dialogMode = 'edit';
    this.selectedResUom = resUom;
    this.formData = {
      classId: resUom.classId || '', // Readonly trong edit mode
      code: resUom.code || '', // Readonly trong edit mode
      name: resUom.name || '',
      status: resUom.status ?? ResUomStatus.Active,
      rounding: resUom.rounding ?? 0.001,
      factor: resUom.factor ?? 0,
      type: resUom.type ?? ResUomType.None
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
      // Validate classId only when creating
      if (!this.formData.classId || this.formData.classId.trim() === '') {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: this.localizationService.localize('Master::ResUom:ClassIdRequired')
        });
        return false;
      }

      // Validate code only when creating
      if (!this.formData.code || this.formData.code.trim() === '') {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: this.localizationService.localize('Master::ResUom:CodeRequired')
        });
        return false;
      }

      if (this.formData.code.length > 25) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: this.localizationService.localize('Master::ResUom:CodeMaxLength')
        });
        return false;
      }

      if (!this.validateCode(this.formData.code)) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: this.localizationService.localize('Master::ResUom:CodeInvalid')
        });
        return false;
      }
    }

    if (!this.formData.name || this.formData.name.trim() === '') {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResUom:NameRequired')
      });
      return false;
    }

    if (this.formData.name.length > 100) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResUom:NameMaxLength')
      });
      return false;
    }

    if (this.formData.rounding == null || this.formData.rounding === undefined || Number.isNaN(this.formData.rounding)) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResUom:RoundingRequired')
      });
      return false;
    }

    if (this.formData.rounding < 0) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResUom:RoundingInvalid')
      });
      return false;
    }

    if (this.formData.factor !== null) {
      if (this.formData.factor < 0) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: this.localizationService.localize('Master::ResUom:FactorInvalid')
        });
        return false;
      }

      if (this.formData.factor >= 1) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: this.localizationService.localize('Master::ResUom:FactorRange')
        });
        return false;
      }
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
    // e.g., "Master:ResUom:CodeExists" -> "Master::ResUom:CodeExists"
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
   * Create new res uom
   */
  private create(): void {
    this.loading = true;

    const createDto: CreateResUomDto = {
      classId: this.formData.classId.trim(),
      code: this.formData.code.trim(),
      name: this.formData.name.trim(),
      status: this.formData.status,
      rounding: this.formData.rounding,
      factor: this.formData.factor ?? 0,
      type: this.formData.type
    };

    this.resUomService.create(createDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Master::Success'),
          detail: this.localizationService.localize('Master::ResUom:CreatedSuccessfully')
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
   * Update existing res uom
   * ⚠️ QUAN TRỌNG: Chỉ update Name, Status, Rounding, Factor và Type, không update Code và ClassId
   */
  private update(): void {
    if (!this.selectedResUom || !this.selectedResUom.id) {
      return;
    }

    this.loading = true;

    const updateDto: UpdateResUomDto = {
      name: this.formData.name.trim(),
      status: this.formData.status,
      rounding: this.formData.rounding,
      factor: this.formData.factor ?? 0,
      type: this.formData.type
    };

    this.resUomService.update(this.selectedResUom.id, updateDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Master::Success'),
          detail: this.localizationService.localize('Master::ResUom:UpdatedSuccessfully')
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
   * Delete res uom with confirmation
   */
  delete(resUom: ResUomDto): void {
    if (!resUom.id) {
      return;
    }

    this.confirmationService.confirm({
      message: this.localizationService.localize('Master::ResUom:DeleteConfirm'),
      header: this.localizationService.localize('Master::ResUom:Delete'),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.loading = true;
        this.resUomService.delete(resUom.id!).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: this.localizationService.localize('Master::Success'),
              detail: this.localizationService.localize('Master::ResUom:DeletedSuccessfully')
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
  private getEmptyForm(): ResUomFormData {
    return {
      classId: '',
      code: '',
      name: '',
      status: ResUomStatus.Active,
      rounding: 0.001,
      factor: 0,
      type: ResUomType.None
    };
  }

  /**
   * Format status for display
   */
  formatStatus(status: ResUomStatus | number | string | undefined | null): string {
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
      statusValue = status === ResUomStatus.Active ? 0 : 1;
    }
    
    return statusValue === 0
      ? this.localizationService.localize('Master::ResUom:Active')
      : this.localizationService.localize('Master::ResUom:Deactive');
  }

  /**
   * Format type for display
   */
  formatType(type: ResUomType | number | string | undefined | null): string {
    if (type === undefined || type === null) {
      return '';
    }
    
    let typeValue: number;
    
    if (typeof type === 'number') {
      typeValue = type;
    } else if (typeof type === 'string') {
      if (type === 'Ref' || type === 'ref' || type === '0') {
        typeValue = 0;
      } else if (type === 'None' || type === 'none' || type === '1') {
        typeValue = 1;
      } else {
        typeValue = 1;
      }
    } else {
      typeValue = type === ResUomType.Ref ? 0 : 1;
    }
    
    return typeValue === 0
      ? this.localizationService.localize('Master::ResUom:Ref')
      : this.localizationService.localize('Master::ResUom:None');
  }

  /**
   * Format rounding for display with full decimal precision (4 decimal places)
   */
  formatRounding(rounding: number | string | null | undefined): string {
    if (rounding === null || rounding === undefined || rounding === '') {
      return '-';
    }
    
    // Convert to number if it's a string
    const numValue = typeof rounding === 'string' ? parseFloat(rounding) : rounding;
    
    // Check if conversion resulted in NaN
    if (isNaN(numValue)) {
      return '-';
    }
    
    // Format with exactly 4 decimal places to show full precision
    return numValue.toFixed(4);
  }

  /**
   * Format factor for display with full decimal precision (6 decimal places)
   */
  formatFactor(factor: number | string | null | undefined): string {
    if (factor === null || factor === undefined || factor === '') {
      return '-';
    }
    
    // Convert to number if it's a string
    const numValue = typeof factor === 'string' ? parseFloat(factor) : factor;
    
    // Check if conversion resulted in NaN
    if (isNaN(numValue)) {
      return '-';
    }
    
    // Format with exactly 6 decimal places to show full precision
    return numValue.toFixed(6);
  }
}
