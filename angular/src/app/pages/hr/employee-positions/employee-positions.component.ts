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
import { HrEmployeePositionService } from '@/proxy/hr/controllers/hr-employee-position.service';
import { HrEmployeePositionDto, CreateHrEmployeePositionDto, UpdateHrEmployeePositionDto, GetHrEmployeePositionsInput } from '@/proxy/hr/hr-employee-positions/models';
import { HrEmployeePositionType } from '@/proxy/hr-employee-positions/hr-employee-position-type.enum';
import { HrEmployeePositionStatus } from '@/proxy/hr-employee-positions/hr-employee-position-status.enum';
import { EmployeePositionSearchForm, EmployeePositionFormData } from './employee-positions.models';
import { PermissionPipe } from '@/core/pipes/permission.pipe';
import { PermissionService } from '@/core/services/permission.service';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';

@Component({
  selector: 'app-employee-positions',
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
  templateUrl: './employee-positions.component.html',
  styleUrl: './employee-positions.component.scss',
  providers: [MessageService, ConfirmationService]
})
export class EmployeePositionsComponent implements OnInit {
  // Permissions
  readonly PERMISSIONS = {
    CREATE: 'HrEmployeePosition.Create',
    UPDATE: 'HrEmployeePosition.Edit',
    DELETE: 'HrEmployeePosition.Delete',
    VIEW: 'HrEmployeePosition.View'
  };

  // Data
  employeePositions: HrEmployeePositionDto[] = [];
  totalCount = 0;
  loading = true;
  pageSize = 10;

  // Dialog
  dialogVisible = false;
  dialogMode: 'create' | 'edit' = 'create';
  formData: EmployeePositionFormData = this.getEmptyForm();
  selectedEmployeePosition?: HrEmployeePositionDto;
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
  searchForm: EmployeePositionSearchForm = {
    code: null,
    name: null,
    type: null,
    status: null
  };

  // Options
  typeOptions: Array<{ label: string; value: HrEmployeePositionType }> = [];
  statusOptions: Array<{ label: string; value: HrEmployeePositionStatus }> = [];

  // Table
  columns: TableColumn[] = [];
  actions: TableAction<HrEmployeePositionDto>[] = [];
  currentLazyLoadEvent?: TableLazyLoadEvent;

  constructor(
    private employeePositionService: HrEmployeePositionService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private permissionService: PermissionService,
    private localizationService: LocalizationService
  ) {
    this.initializeColumns();
    this.initializeActions();
    this.initializeTypeOptions();
    this.initializeStatusOptions();
  }

  ngOnInit(): void {
    // Initial load will be triggered by the table's onLazyLoad event
  }

  /**
   * Initialize type options with localized labels
   */
  private initializeTypeOptions(): void {
    this.typeOptions = [
      { label: this.localizationService.localize('Hr::HrEmployeePosition:Type:Profess'), value: HrEmployeePositionType.Profess },
      { label: this.localizationService.localize('Hr::HrEmployeePosition:Type:Manage'), value: HrEmployeePositionType.Manage }
    ];
  }

  /**
   * Initialize status options with localized labels
   */
  private initializeStatusOptions(): void {
    this.statusOptions = [
      { label: this.localizationService.localize('Hr::HrEmployeePosition:Status:Active'), value: HrEmployeePositionStatus.Active },
      { label: this.localizationService.localize('Hr::HrEmployeePosition:Status:Deactive'), value: HrEmployeePositionStatus.Deactive }
    ];
  }

  /**
   * Initialize table columns with localized headers
   */
  private initializeColumns(): void {
    this.columns = [
      {
        field: 'code',
        header: this.localizationService.localize('Hr::HrEmployeePosition:Code'),
        sortable: true,
        width: '150px'
      },
      {
        field: 'name',
        header: this.localizationService.localize('Hr::HrEmployeePosition:Name'),
        sortable: true,
        width: '250px'
      },
      {
        field: 'type',
        header: this.localizationService.localize('Hr::HrEmployeePosition:Type'),
        sortable: true,
        type: 'text',
        width: '150px',
        formatter: (value: any) => this.formatType(value),
        cellClass: (value: any) => {
          const typeValue = typeof value === 'number' ? value : (value === HrEmployeePositionType.Profess ? 0 : 1);
          return typeValue === 0
            ? 'px-2 py-1 rounded text-xs font-semibold bg-blue-100 text-blue-800'
            : 'px-2 py-1 rounded text-xs font-semibold bg-purple-100 text-purple-800';
        }
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
        header: this.localizationService.localize('Hr::HrEmployeePosition:Status'),
        sortable: true,
        type: 'text',
        width: '120px',
        freeze: 'right',
        align: 'center',
        formatter: (value: any) => this.formatStatus(value),
        cellClass: (value: any) => {
          const statusValue = typeof value === 'number' ? value : (value === HrEmployeePositionStatus.Active ? 0 : 1);
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
   * Load employee positions with lazy loading
   */
  loadData(event: TableLazyLoadEvent): void {
    this.currentLazyLoadEvent = event;
    this.loading = true;

    const input: GetHrEmployeePositionsInput = {
      skipCount: event.first || 0,
      maxResultCount: event.rows || 10,
      sorting: this.getSortingString(event),
      code: this.searchForm.code || undefined,
      name: this.searchForm.name || undefined,
      type: this.searchForm.type ?? undefined,
      status: this.searchForm.status ?? undefined
    };

    this.employeePositionService.getList(input).subscribe({
      next: (result) => {
        this.employeePositions = result.items || [];
        this.totalCount = result.totalCount || 0;
        this.loading = false;
      },
      error: (error) => {
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
      type: null,
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
    this.selectedEmployeePosition = undefined;
    this.dialogVisible = true;
  }

  /**
   * Open edit dialog
   */
  openEditDialog(employeePosition: HrEmployeePositionDto): void {
    this.dialogMode = 'edit';
    this.selectedEmployeePosition = employeePosition;
    this.formData = {
      code: employeePosition.code || '',
      name: employeePosition.name || '',
      type: employeePosition.type ?? HrEmployeePositionType.Profess,
      status: employeePosition.status ?? HrEmployeePositionStatus.Active
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
      if (!this.formData.code || this.formData.code.trim() === '') {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: this.localizationService.localize('Hr::HrEmployeePosition:CodeRequired')
        });
        return false;
      }

      if (this.formData.code.length > 50) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: this.localizationService.localize('Hr::HrEmployeePosition:CodeMaxLength')
        });
        return false;
      }

      if (!this.validateCode(this.formData.code)) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: this.localizationService.localize('Hr::HrEmployeePosition:CodeInvalidFormat')
        });
        return false;
      }
    }

    if (!this.formData.name || this.formData.name.trim() === '') {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Hr::HrEmployeePosition:NameRequired')
      });
      return false;
    }

    if (this.formData.name.length > 250) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Hr::HrEmployeePosition:NameMaxLength')
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
   * Create new employee position
   */
  private create(): void {
    this.loading = true;

    const createDto: CreateHrEmployeePositionDto = {
      code: this.formData.code.trim(),
      name: this.formData.name.trim(),
      type: this.formData.type,
      status: this.formData.status
    };

    this.employeePositionService.create(createDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Hr::Success'),
          detail: this.localizationService.localize('Hr::HrEmployeePosition:CreatedSuccessfully')
        });
        this.dialogVisible = false;
        this.search();
      },
      error: (error) => {
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
   * Update existing employee position
   */
  private update(): void {
    if (!this.selectedEmployeePosition || !this.selectedEmployeePosition.id) {
      return;
    }

    this.loading = true;

    const updateDto: UpdateHrEmployeePositionDto = {
      name: this.formData.name.trim(),
      type: this.formData.type,
      status: this.formData.status
    };

    this.employeePositionService.update(this.selectedEmployeePosition.id, updateDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Hr::Success'),
          detail: this.localizationService.localize('Hr::HrEmployeePosition:UpdatedSuccessfully')
        });
        this.dialogVisible = false;
        this.search();
      },
      error: (error) => {
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
   * Delete employee position with confirmation
   */
  delete(employeePosition: HrEmployeePositionDto): void {
    if (!employeePosition.id) {
      return;
    }

    this.confirmationService.confirm({
      message: this.localizationService.localize('Hr::HrEmployeePosition:DeleteConfirm'),
      header: this.localizationService.localize('Hr::HrEmployeePosition:Delete'),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.loading = true;
        this.employeePositionService.delete(employeePosition.id!).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: this.localizationService.localize('Hr::Success'),
              detail: this.localizationService.localize('Hr::HrEmployeePosition:DeletedSuccessfully')
            });
            this.search();
          },
          error: (error) => {
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
  private getEmptyForm(): EmployeePositionFormData {
    return {
      code: '',
      name: '',
      type: HrEmployeePositionType.Profess,
      status: HrEmployeePositionStatus.Active
    };
  }

  /**
   * Format type for display
   */
  formatType(type: HrEmployeePositionType | number | string | undefined | null): string {
    if (type === undefined || type === null) {
      return '';
    }

    let typeValue: number;

    if (typeof type === 'number') {
      typeValue = type;
    } else if (typeof type === 'string') {
      if (type === 'Profess' || type === 'profess' || type === '0') {
        typeValue = 0;
      } else if (type === 'Manage' || type === 'manage' || type === '1') {
        typeValue = 1;
      } else {
        typeValue = 0;
      }
    } else {
      typeValue = type === HrEmployeePositionType.Profess ? 0 : 1;
    }

    return typeValue === 0
      ? this.localizationService.localize('Hr::HrEmployeePosition:Type:Profess')
      : this.localizationService.localize('Hr::HrEmployeePosition:Type:Manage');
  }

  /**
   * Format status for display
   */
  formatStatus(status: HrEmployeePositionStatus | number | string | undefined | null): string {
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
      statusValue = status === HrEmployeePositionStatus.Active ? 0 : 1;
    }

    return statusValue === 0
      ? this.localizationService.localize('Hr::HrEmployeePosition:Status:Active')
      : this.localizationService.localize('Hr::HrEmployeePosition:Status:Deactive');
  }
}

