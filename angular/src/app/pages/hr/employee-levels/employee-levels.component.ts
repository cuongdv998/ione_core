import { Component, HostListener, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { TextareaModule } from 'primeng/textarea';
import { TableLazyLoadEvent } from 'primeng/table';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { DialogModule } from 'primeng/dialog';
import { ConfirmDialogModule } from 'primeng/confirmdialog';

import { VTable } from '@/shared/components/v-table/v-table';
import { TableColumn } from '@/shared/models/table-column.model';
import { TableAction } from '@/shared/models/table-action.model';
import { HrEmployeeLevelService } from '@/proxy/hr/controllers/hr-employee-level.service';
import { HrEmployeeLevelDto, CreateHrEmployeeLevelDto, UpdateHrEmployeeLevelDto, GetHrEmployeeLevelsInput } from '@/proxy/hr/hr-employee-levels/models';
import { HrEmployeeLevelStatus } from '@/proxy/hr-employee-levels/hr-employee-level-status.enum';
import { EmployeeLevelSearchForm, EmployeeLevelFormData } from './employee-levels.models';
import { PermissionPipe } from '@/core/pipes/permission.pipe';
import { PermissionService } from '@/core/services/permission.service';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';

@Component({
  selector: 'app-employee-levels',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    PanelModule,
    ButtonModule,
    InputTextModule,
    SelectModule,
    TextareaModule,
    ToastModule,
    DialogModule,
    ConfirmDialogModule,
    VTable,
    PermissionPipe,
    TranslatePipe
  ],
  templateUrl: './employee-levels.component.html',
  styleUrl: './employee-levels.component.scss',
  providers: [MessageService, ConfirmationService]
})
export class EmployeeLevelsComponent implements OnInit {
  // Permissions
  readonly PERMISSIONS = {
    CREATE: 'HrEmployeeLevel.Create',
    UPDATE: 'HrEmployeeLevel.Edit',
    DELETE: 'HrEmployeeLevel.Delete',
    VIEW: 'HrEmployeeLevel.View'
  };

  // Data
  employeeLevels: HrEmployeeLevelDto[] = [];
  totalCount = 0;
  loading = true;
  pageSize = 10;

  // Dialog
  dialogVisible = false;
  dialogMode: 'create' | 'edit' = 'create';
  formData: EmployeeLevelFormData = this.getEmptyForm();
  selectedEmployeeLevel?: HrEmployeeLevelDto;
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
  searchForm: EmployeeLevelSearchForm = {
    code: null,
    name: null,
    status: null
  };

  // Status options
  statusOptions: Array<{ label: string; value: HrEmployeeLevelStatus }> = [];

  // Table
  columns: TableColumn[] = [];
  actions: TableAction<HrEmployeeLevelDto>[] = [];
  currentLazyLoadEvent?: TableLazyLoadEvent;

  constructor(
    private employeeLevelService: HrEmployeeLevelService,
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
      { label: this.localizationService.localize('Hr::HrEmployeeLevel:Active'), value: HrEmployeeLevelStatus.Active },
      { label: this.localizationService.localize('Hr::HrEmployeeLevel:Deactive'), value: HrEmployeeLevelStatus.Deactive }
    ];
  }

  /**
   * Initialize table columns with localized headers
   */
  private initializeColumns(): void {
    this.columns = [
      {
        field: 'code',
        header: this.localizationService.localize('Hr::HrEmployeeLevel:Code'),
        sortable: true,
        width: '150px'
      },
      {
        field: 'name',
        header: this.localizationService.localize('Hr::HrEmployeeLevel:Name'),
        sortable: true,
        width: '250px'
      },
      {
        field: 'description',
        header: this.localizationService.localize('Hr::HrEmployeeLevel:Description'),
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
        header: this.localizationService.localize('Hr::HrEmployeeLevel:Status'),
        sortable: true,
        type: 'text',
        width: '120px',
        freeze: 'right',
        align: 'center',
        formatter: (value: any) => this.formatStatus(value),
        cellClass: (value: any) => {
          const statusValue = typeof value === 'number' ? value : (value === HrEmployeeLevelStatus.Active ? 0 : 1);
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
   * Load employee levels with lazy loading
   */
  loadData(event: TableLazyLoadEvent): void {
    this.currentLazyLoadEvent = event;
    this.loading = true;

    const input: GetHrEmployeeLevelsInput = {
      skipCount: event.first || 0,
      maxResultCount: event.rows || 10,
      sorting: this.getSortingString(event),
      code: this.searchForm.code || undefined,
      name: this.searchForm.name || undefined,
      status: this.searchForm.status ?? undefined
    };

    this.employeeLevelService.getList(input).subscribe({
      next: (result) => {
        this.employeeLevels = result.items || [];
        this.totalCount = result.totalCount || 0;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading employee levels:', error);
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
    this.selectedEmployeeLevel = undefined;
    this.dialogVisible = true;
  }

  /**
   * Open edit dialog
   */
  openEditDialog(employeeLevel: HrEmployeeLevelDto): void {
    this.dialogMode = 'edit';
    this.selectedEmployeeLevel = employeeLevel;
    this.formData = {
      code: employeeLevel.code || '',
      name: employeeLevel.name || '',
      status: employeeLevel.status ?? HrEmployeeLevelStatus.Active,
      description: employeeLevel.description || ''
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
        detail: this.localizationService.localize('Hr::HrEmployeeLevel:CodeRequired')
      });
      return false;
    }

    if (this.formData.code.length > 50) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Hr::HrEmployeeLevel:CodeMaxLength')
      });
      return false;
    }

    if (!this.validateCode(this.formData.code)) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Hr::HrEmployeeLevel:CodeInvalidFormat')
      });
      return false;
    }

    if (!this.formData.name || this.formData.name.trim() === '') {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Hr::HrEmployeeLevel:NameRequired')
      });
      return false;
    }

    if (this.formData.name.length > 250) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Hr::HrEmployeeLevel:NameMaxLength')
      });
      return false;
    }

    if (this.formData.description && this.formData.description.length > 500) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Hr::HrEmployeeLevel:DescriptionMaxLength')
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
   * Create new employee level
   */
  private create(): void {
    this.loading = true;

    const createDto: CreateHrEmployeeLevelDto = {
      code: this.formData.code.trim(),
      name: this.formData.name.trim(),
      status: this.formData.status,
      description: this.formData.description?.trim() || undefined
    };

    this.employeeLevelService.create(createDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Hr::Success'),
          detail: this.localizationService.localize('Hr::HrEmployeeLevel:CreatedSuccessfully')
        });
        this.dialogVisible = false;
        this.search();
      },
      error: (error) => {
        console.error('Error creating employee level:', error);
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
   * Update existing employee level
   */
  private update(): void {
    if (!this.selectedEmployeeLevel || !this.selectedEmployeeLevel.id) {
      return;
    }

    this.loading = true;

    const updateDto: UpdateHrEmployeeLevelDto = {
      name: this.formData.name.trim(),
      status: this.formData.status,
      description: this.formData.description?.trim() || undefined
    };

    this.employeeLevelService.update(this.selectedEmployeeLevel.id, updateDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Hr::Success'),
          detail: this.localizationService.localize('Hr::HrEmployeeLevel:UpdatedSuccessfully')
        });
        this.dialogVisible = false;
        this.search();
      },
      error: (error) => {
        console.error('Error updating employee level:', error);
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
   * Delete employee level with confirmation
   */
  delete(employeeLevel: HrEmployeeLevelDto): void {
    if (!employeeLevel.id) {
      return;
    }

    this.confirmationService.confirm({
      message: this.localizationService.localize('Hr::HrEmployeeLevel:DeleteConfirm'),
      header: this.localizationService.localize('Hr::HrEmployeeLevel:Delete'),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.loading = true;
        this.employeeLevelService.delete(employeeLevel.id!).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: this.localizationService.localize('Hr::Success'),
              detail: this.localizationService.localize('Hr::HrEmployeeLevel:DeletedSuccessfully')
            });
            this.search();
          },
          error: (error) => {
            console.error('Error deleting employee level:', error);
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
  private getEmptyForm(): EmployeeLevelFormData {
    return {
      code: '',
      name: '',
      status: HrEmployeeLevelStatus.Active,
      description: ''
    };
  }

  /**
   * Format status for display
   */
  formatStatus(status: HrEmployeeLevelStatus | number | string | undefined | null): string {
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
      statusValue = status === HrEmployeeLevelStatus.Active ? 0 : 1;
    }
    
    return statusValue === 0
      ? this.localizationService.localize('Hr::HrEmployeeLevel:Active')
      : this.localizationService.localize('Hr::HrEmployeeLevel:Deactive');
  }
}

