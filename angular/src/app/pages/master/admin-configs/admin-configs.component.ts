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
import { AdminConfigService } from '@/proxy/master/controllers/admin-config.service';
import { AdminConfigDto, CreateAdminConfigDto, UpdateAdminConfigDto, GetAdminConfigsInput } from '@/proxy/master/admin-configs/models';
import { AdminConfigStatus } from '@/proxy/admin-configs/admin-config-status.enum';
import { AdminConfigSearchForm, AdminConfigFormData } from './admin-configs.models';
import { PermissionPipe } from '@/core/pipes/permission.pipe';
import { PermissionService } from '@/core/services/permission.service';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';

@Component({
  selector: 'app-admin-configs',
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
  templateUrl: './admin-configs.component.html',
  styleUrl: './admin-configs.component.scss',
  providers: [MessageService, ConfirmationService]
})
export class AdminConfigsComponent implements OnInit {
  // Permissions
  readonly PERMISSIONS = {
    CREATE: 'MasterAdminConfig.Create',
    UPDATE: 'MasterAdminConfig.Edit',
    DELETE: 'MasterAdminConfig.Delete',
    VIEW: 'MasterAdminConfig.View'
  };

  // Data
  adminConfigs: AdminConfigDto[] = [];
  totalCount = 0;
  loading = true;
  pageSize = 10;

  // Dialog
  dialogVisible = false;
  dialogMode: 'create' | 'edit' | 'copy' = 'create';
  formData: AdminConfigFormData = this.getEmptyForm();
  selectedAdminConfig?: AdminConfigDto;

  // Search form
  searchForm: AdminConfigSearchForm = {
    code: null,
    subCode: null,
    name: null,
    status: null
  };

  // Status options
  statusOptions: Array<{ label: string; value: AdminConfigStatus }> = [];

  // Table
  columns: TableColumn[] = [];
  actions: TableAction<AdminConfigDto>[] = [];
  currentLazyLoadEvent?: TableLazyLoadEvent;

  constructor(
    private adminConfigService: AdminConfigService,
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
      { label: this.localizationService.localize('Master::AdminConfig:Active'), value: AdminConfigStatus.Active },
      { label: this.localizationService.localize('Master::AdminConfig:Deactive'), value: AdminConfigStatus.Deactive }
    ];
  }

  /**
   * Initialize table columns with localized headers
   */
  private initializeColumns(): void {
    this.columns = [
      {
        field: 'code',
        header: this.localizationService.localize('Master::AdminConfig:Code'),
        sortable: true,
        width: '150px'
      },
      {
        field: 'subCode',
        header: this.localizationService.localize('Master::AdminConfig:SubCode'),
        sortable: true,
        width: '150px'
      },
      {
        field: 'name',
        header: this.localizationService.localize('Master::AdminConfig:Name'),
        sortable: true,
        width: '250px'
      },
      {
        field: 'value',
        header: this.localizationService.localize('Master::AdminConfig:Value'),
        sortable: false,
        width: '200px'
      },
      {
        field: 'description',
        header: this.localizationService.localize('Master::AdminConfig:Description'),
        sortable: false,
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
        header: this.localizationService.localize('Master::AdminConfig:Status'),
        sortable: true,
        type: 'text',
        width: '120px',
        freeze: 'right',
        align: 'center',
        formatter: (value: any) => this.formatStatus(value),
        cellClass: (value: any) => {
          const statusValue = typeof value === 'number' ? value : (value === AdminConfigStatus.Active ? 0 : 1);
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
    if (this.permissionService.isGranted(this.PERMISSIONS.CREATE)) {
      this.actions.push({
        label: this.localizationService.localize('Master::AdminConfig:Copy'),
        icon: 'pi pi-copy',
        command: (row) => this.openCopyDialog(row)
      });
    }

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
   * Load admin configs with lazy loading
   */
  loadData(event: TableLazyLoadEvent): void {
    this.currentLazyLoadEvent = event;
    this.loading = true;

    const input: GetAdminConfigsInput = {
      skipCount: event.first || 0,
      maxResultCount: event.rows || 10,
      sorting: this.getSortingString(event),
      code: this.searchForm.code || undefined,
      subCode: this.searchForm.subCode || undefined,
      name: this.searchForm.name || undefined,
      status: this.searchForm.status ?? undefined
    };

    this.adminConfigService.getList(input).subscribe({
      next: (result) => {
        this.adminConfigs = result.items || [];
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
      code: null,
      subCode: null,
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
    this.selectedAdminConfig = undefined;
    this.dialogVisible = true;
  }

  /**
   * Open edit dialog
   * ⚠️ QUAN TRỌNG: Code và SubCode không được phép sửa khi edit
   */
  openEditDialog(adminConfig: AdminConfigDto): void {
    this.dialogMode = 'edit';
    this.selectedAdminConfig = adminConfig;
    this.formData = {
      code: adminConfig.code || '', // Readonly trong edit mode
      name: adminConfig.name || '',
      subCode: adminConfig.subCode || '', // Readonly trong edit mode
      value: adminConfig.value || '',
      description: adminConfig.description || null,
      status: adminConfig.status ?? AdminConfigStatus.Active
    };
    this.dialogVisible = true;
  }

  /**
   * Open copy dialog
   * Sao chép dữ liệu từ record được chọn, Code và SubCode có thể được sửa
   */
  openCopyDialog(adminConfig: AdminConfigDto): void {
    this.dialogMode = 'copy';
    this.selectedAdminConfig = adminConfig;
    this.formData = {
      code: adminConfig.code || '', // Có thể sửa khi copy
      name: adminConfig.name || '',
      subCode: adminConfig.subCode || '', // Có thể sửa khi copy
      value: adminConfig.value || '',
      description: adminConfig.description || null,
      status: adminConfig.status ?? AdminConfigStatus.Active
    };
    this.dialogVisible = true;
  }

  /**
   * Save (create, update, or copy)
   */
  save(): void {
    if (!this.validateForm()) {
      return;
    }

    if (this.dialogMode === 'create' || this.dialogMode === 'copy') {
      this.create();
    } else {
      this.update();
    }
  }

  /**
   * Validate form
   */
  private validateForm(): boolean {
    if (this.dialogMode === 'create' || this.dialogMode === 'copy') {
      // Validate code only when creating
      if (!this.formData.code || this.formData.code.trim() === '') {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: this.localizationService.localize('Master::AdminConfig:CodeRequired')
        });
        return false;
      }

      if (this.formData.code.length > 25) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: this.localizationService.localize('Master::AdminConfig:CodeMaxLength')
        });
        return false;
      }

      if (!this.validateCode(this.formData.code)) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: this.localizationService.localize('Master::AdminConfig:CodeInvalid')
        });
        return false;
      }

      // Validate subCode only when creating
      if (!this.formData.subCode || this.formData.subCode.trim() === '') {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: this.localizationService.localize('Master::AdminConfig:SubCodeRequired')
        });
        return false;
      }

      if (this.formData.subCode.length > 25) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: this.localizationService.localize('Master::AdminConfig:SubCodeMaxLength')
        });
        return false;
      }

      if (!this.validateCode(this.formData.subCode)) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: this.localizationService.localize('Master::AdminConfig:SubCodeInvalid')
        });
        return false;
      }
    }

    if (!this.formData.name || this.formData.name.trim() === '') {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::AdminConfig:NameRequired')
      });
      return false;
    }

    if (this.formData.name.length > 250) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::AdminConfig:NameMaxLength')
      });
      return false;
    }

    if (!this.formData.value || this.formData.value.trim() === '') {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::AdminConfig:ValueRequired')
      });
      return false;
    }

    if (this.formData.value.length > 250) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::AdminConfig:ValueMaxLength')
      });
      return false;
    }

    if (this.formData.description && this.formData.description.length > 250) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::AdminConfig:DescriptionMaxLength')
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
   * Convert subCode to uppercase on input
   */
  onSubCodeInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    const value = input.value.toUpperCase();
    input.value = value;
    this.formData.subCode = value;
  }

  /**
   * Create new admin config
   */
  private create(): void {
    this.loading = true;

    const createDto: CreateAdminConfigDto = {
      code: this.formData.code.trim().toUpperCase(),
      name: this.formData.name.trim(),
      subCode: this.formData.subCode.trim().toUpperCase(),
      value: this.formData.value.trim(),
      description: this.formData.description?.trim() || undefined,
      status: this.formData.status
    };

    this.adminConfigService.create(createDto).subscribe({
      next: () => {
        const successMessage = this.dialogMode === 'copy'
          ? this.localizationService.localize('Master::AdminConfig:CopiedSuccessfully')
          : this.localizationService.localize('Master::AdminConfig:CreatedSuccessfully');
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Master::Success'),
          detail: successMessage
        });
        this.dialogVisible = false;
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

  /**
   * Update existing admin config
   * ⚠️ QUAN TRỌNG: Chỉ update Name, Value, Description, Status - không update Code và SubCode
   */
  private update(): void {
    if (!this.selectedAdminConfig || !this.selectedAdminConfig.id) {
      return;
    }

    this.loading = true;

    const updateDto: UpdateAdminConfigDto = {
      name: this.formData.name.trim(),
      value: this.formData.value.trim(),
      description: this.formData.description?.trim() || undefined,
      status: this.formData.status
    };

    this.adminConfigService.update(this.selectedAdminConfig.id, updateDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Master::Success'),
          detail: this.localizationService.localize('Master::AdminConfig:UpdatedSuccessfully')
        });
        this.dialogVisible = false;
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

  /**
   * Delete admin config with confirmation
   * ⚠️ QUAN TRỌNG: Soft delete - set Status to Deactive
   */
  delete(adminConfig: AdminConfigDto): void {
    if (!adminConfig.id) {
      return;
    }

    this.confirmationService.confirm({
      message: this.localizationService.localize('Master::AdminConfig:DeleteConfirm'),
      header: this.localizationService.localize('Master::AdminConfig:Delete'),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.loading = true;
        this.adminConfigService.delete(adminConfig.id!).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: this.localizationService.localize('Master::Success'),
              detail: this.localizationService.localize('Master::AdminConfig:DeletedSuccessfully')
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
  private getEmptyForm(): AdminConfigFormData {
    return {
      code: '',
      name: '',
      subCode: '',
      value: '',
      description: null,
      status: AdminConfigStatus.Active
    };
  }

  /**
   * Format status for display
   */
  formatStatus(status: AdminConfigStatus | number | string | undefined | null): string {
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
      statusValue = status === AdminConfigStatus.Active ? 0 : 1;
    }

    return statusValue === 0
      ? this.localizationService.localize('Master::AdminConfig:Active')
      : this.localizationService.localize('Master::AdminConfig:Deactive');
  }
}

