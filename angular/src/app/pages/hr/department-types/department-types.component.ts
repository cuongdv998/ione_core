import { Component, HostListener, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { TableLazyLoadEvent } from 'primeng/table';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { DialogModule } from 'primeng/dialog';
import { ConfirmDialogModule } from 'primeng/confirmdialog';

import { VTable } from '@/shared/components/v-table/v-table';
import { TableColumn } from '@/shared/models/table-column.model';
import { TableAction } from '@/shared/models/table-action.model';
import { HrDepartmentTypeService } from '@/proxy/hr/controllers/hr-department-type.service';
import { HrDepartmentTypeDto, CreateHrDepartmentTypeDto, UpdateHrDepartmentTypeDto, GetHrDepartmentTypesInput } from '@/proxy/hr/hr-department-types/models';
import { HrDepartmentTypeStatus } from '@/proxy/hr-department-types/hr-department-type-status.enum';
import { DepartmentTypeSearchForm, DepartmentTypeFormData } from './department-types.models';
import { PermissionPipe } from '@/core/pipes/permission.pipe';
import { PermissionService } from '@/core/services/permission.service';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';

@Component({
  selector: 'app-department-types',
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
    VTable,
    PermissionPipe,
    TranslatePipe
  ],
  templateUrl: './department-types.component.html',
  styleUrl: './department-types.component.scss',
  providers: [MessageService, ConfirmationService]
})
export class DepartmentTypesComponent implements OnInit {
  // Permissions
  readonly PERMISSIONS = {
    CREATE: 'HrDepartmentType.Create',
    UPDATE: 'HrDepartmentType.Edit',
    DELETE: 'HrDepartmentType.Delete',
    VIEW: 'HrDepartmentType.View'
  };

  // Data
  departmentTypes: HrDepartmentTypeDto[] = [];
  totalCount = 0;
  loading = true;
  pageSize = 10;

  // Dialog
  dialogVisible = false;
  dialogMode: 'create' | 'edit' = 'create';
  formData: DepartmentTypeFormData = this.getEmptyForm();
  selectedDepartmentType?: HrDepartmentTypeDto;
  /**
   * Listen for Enter key events globally to trigger search
   */
  @HostListener('window:keydown.enter', ['$event'])
  onWindowKeyDown(event: KeyboardEvent): void {
    if (!this.dialogVisible) {
      this.search();
    }
  }

  // Search form
  searchForm: DepartmentTypeSearchForm = {
    code: null,
    name: null,
    status: null
  };

  // Status options
  statusOptions: Array<{ label: string; value: HrDepartmentTypeStatus }> = [];

  // Table
  columns: TableColumn[] = [];
  actions: TableAction<HrDepartmentTypeDto>[] = [];
  currentLazyLoadEvent?: TableLazyLoadEvent;

  constructor(
    private departmentTypeService: HrDepartmentTypeService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private permissionService: PermissionService,
    private localizationService: LocalizationService
  ) {
    this.initializeColumns();
    this.initializeActions();
    this.initializeStatusOptions();
  }

  ngOnInit(): void {
    // Initial load will be triggered by the table's onLazyLoad event
  }

  /**
   * Initialize status options with localized labels
   */
  private initializeStatusOptions(): void {
    this.statusOptions = [
      { label: this.localizationService.localize('Hr::HrDepartmentType:Active'), value: HrDepartmentTypeStatus.Active },
      { label: this.localizationService.localize('Hr::HrDepartmentType:Deactive'), value: HrDepartmentTypeStatus.Deactive }
    ];
  }

  /**
   * Initialize table columns with localized headers
   */
  private initializeColumns(): void {
    this.columns = [
      {
        field: 'code',
        header: this.localizationService.localize('Hr::HrDepartmentType:Code'),
        sortable: true,
        width: '150px'
      },
      {
        field: 'name',
        header: this.localizationService.localize('Hr::HrDepartmentType:Name'),
        sortable: true,
        width: '250px'
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
        header: this.localizationService.localize('Hr::HrDepartmentType:Status'),
        sortable: true,
        type: 'text',
        width: '120px',
        freeze: 'right',
        align: 'center',
        formatter: (value: any) => this.formatStatus(value),
        cellClass: (value: any) => {
          const statusValue = typeof value === 'number' ? value : (value === HrDepartmentTypeStatus.Active ? 0 : 1);
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
        label: this.localizationService.localize('Hr::Edit'),
        icon: 'pi pi-pencil',
        command: (row) => this.openEditDialog(row)
      });
    }

    if (this.permissionService.isGranted(this.PERMISSIONS.DELETE)) {
      this.actions.push({
        label: this.localizationService.localize('Hr::Delete'),
        icon: 'pi pi-trash',
        command: (row) => this.delete(row)
      });
    }
  }

  /**
   * Load department types with lazy loading
   */
  loadData(event: TableLazyLoadEvent): void {
    this.currentLazyLoadEvent = event;
    this.loading = true;

    const input: GetHrDepartmentTypesInput = {
      skipCount: event.first || 0,
      maxResultCount: event.rows || 10,
      sorting: this.getSortingString(event),
      code: this.searchForm.code || undefined,
      name: this.searchForm.name || undefined,
      status: this.searchForm.status ?? undefined
    };

    this.departmentTypeService.getList(input).subscribe({
      next: (result) => {
        this.departmentTypes = result.items || [];
        this.totalCount = result.totalCount || 0;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading department types:', error);
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Hr::Error'),
          detail: this.localizationService.localize('Hr::InternalServerErrorMessage')
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
    this.selectedDepartmentType = undefined;
    this.dialogVisible = true;
  }

  /**
   * Open edit dialog
   */
  openEditDialog(departmentType: HrDepartmentTypeDto): void {
    this.dialogMode = 'edit';
    this.selectedDepartmentType = departmentType;
    this.formData = {
      code: departmentType.code || '',
      name: departmentType.name || '',
      status: departmentType.status ?? HrDepartmentTypeStatus.Active
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
    if (!this.formData.code || this.formData.code.trim() === '') {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Hr::HrDepartmentType:CodeRequired')
      });
      return false;
    }

    if (this.formData.code.length > 50) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Hr::HrDepartmentType:CodeMaxLength')
      });
      return false;
    }

    if (!this.validateCode(this.formData.code)) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Hr::HrDepartmentType:CodeInvalidFormat')
      });
      return false;
    }

    if (!this.formData.name || this.formData.name.trim() === '') {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Hr::HrDepartmentType:NameRequired')
      });
      return false;
    }

    if (this.formData.name.length > 250) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Hr::HrDepartmentType:NameMaxLength')
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
   * Create new department type
   */
  private create(): void {
    this.loading = true;

    const createDto: CreateHrDepartmentTypeDto = {
      code: this.formData.code.trim(),
      name: this.formData.name.trim(),
      status: this.formData.status
    };

    this.departmentTypeService.create(createDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Hr::Success'),
          detail: this.localizationService.localize('Hr::HrDepartmentType:CreatedSuccessfully')
        });
        this.dialogVisible = false;
        this.search();
      },
      error: (error) => {
        console.error('Error creating department type:', error);
        const errorMessage = error.error?.error?.message || 
                           error.error?.error?.details || 
                           this.localizationService.localize('AbpUi::InternalServerErrorMessage');
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Hr::Error'),
          detail: errorMessage
        });
        this.loading = false;
      }
    });
  }

  /**
   * Update existing department type
   */
  private update(): void {
    if (!this.selectedDepartmentType || !this.selectedDepartmentType.id) {
      return;
    }

    this.loading = true;

    const updateDto: UpdateHrDepartmentTypeDto = {
      name: this.formData.name.trim(),
      status: this.formData.status
    };

    this.departmentTypeService.update(this.selectedDepartmentType.id, updateDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Hr::Success'),
          detail: this.localizationService.localize('Hr::HrDepartmentType:UpdatedSuccessfully')
        });
        this.dialogVisible = false;
        this.search();
      },
      error: (error) => {
        console.error('Error updating department type:', error);
        const errorMessage = error.error?.error?.message || 
                           error.error?.error?.details || 
                           this.localizationService.localize('AbpUi::InternalServerErrorMessage');
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Hr::Error'),
          detail: errorMessage
        });
        this.loading = false;
      }
    });
  }

  /**
   * Delete department type with confirmation
   */
  delete(departmentType: HrDepartmentTypeDto): void {
    if (!departmentType.id) {
      return;
    }

    this.confirmationService.confirm({
      message: this.localizationService.localize('Hr::HrDepartmentType:DeleteConfirm'),
      header: this.localizationService.localize('Hr::HrDepartmentType:Delete'),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.loading = true;
        this.departmentTypeService.delete(departmentType.id!).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: this.localizationService.localize('Hr::Success'),
              detail: this.localizationService.localize('Hr::HrDepartmentType:DeletedSuccessfully')
            });
            this.search();
          },
          error: (error) => {
            console.error('Error deleting department type:', error);
            const errorMessage = error.error?.error?.message || 
                               error.error?.error?.details || 
                               this.localizationService.localize('AbpUi::InternalServerErrorMessage');
            this.messageService.add({
              severity: 'error',
              summary: this.localizationService.localize('Hr::Error'),
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
  private getEmptyForm(): DepartmentTypeFormData {
    return {
      code: '',
      name: '',
      status: HrDepartmentTypeStatus.Active
    };
  }

  /**
   * Format status for display
   */
  formatStatus(status: HrDepartmentTypeStatus | number | string | undefined | null): string {
    if (status === undefined || status === null) {
      return '';
    }
    
    // Backend returns 0 for Active, 1 for Deactive
    // Handle both enum and numeric values
    let statusValue: number;
    
    if (typeof status === 'number') {
      statusValue = status;
    } else if (typeof status === 'string') {
      // Handle string values
      if (status === 'Active' || status === 'active' || status === '0') {
        statusValue = 0;
      } else if (status === 'Deactive' || status === 'deactive' || status === '1') {
        statusValue = 1;
      } else {
        // Default to Deactive if unknown
        statusValue = 1;
      }
    } else {
      // Handle enum values
      statusValue = status === HrDepartmentTypeStatus.Active ? 0 : 1;
    }
    
    return statusValue === 0
      ? this.localizationService.localize('Hr::HrDepartmentType:Active')
      : this.localizationService.localize('Hr::HrDepartmentType:Deactive');
  }
}

