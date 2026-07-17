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
import { ResPartnerTypeService } from '@/proxy/partner/controllers/res-partner-type.service';
import { ResPartnerTypeDto, CreateResPartnerTypeDto, UpdateResPartnerTypeDto, GetResPartnerTypesInput } from '@/proxy/partner/res-partner-types/models';
import { ResPartnerTypeStatus } from '@/proxy/res-partner-types/res-partner-type-status.enum';
import { PartnerTypeSearchForm, PartnerTypeFormData } from './partner-types.models';
import { PermissionPipe } from '@/core/pipes/permission.pipe';
import { PermissionService } from '@/core/services/permission.service';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';

@Component({
  selector: 'app-partner-types',
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
  templateUrl: './partner-types.component.html',
  styleUrl: './partner-types.component.scss',
  providers: [MessageService, ConfirmationService]
})
export class PartnerTypesComponent implements OnInit {
  // Permissions
  readonly PERMISSIONS = {
    CREATE: 'PartnerResPartnerType.Create',
    UPDATE: 'PartnerResPartnerType.Edit',
    DELETE: 'PartnerResPartnerType.Delete',
    VIEW: 'PartnerResPartnerType.View'
  };

  // Data
  partnerTypes: ResPartnerTypeDto[] = [];
  totalCount = 0;
  loading = true;
  pageSize = 10;

  // Dialog
  dialogVisible = false;
  dialogMode: 'create' | 'edit' = 'create';
  formData: PartnerTypeFormData = this.getEmptyForm();
  selectedPartnerType?: ResPartnerTypeDto;

  // Search form
  searchForm: PartnerTypeSearchForm = {
    code: null,
    name: null,
    status: null
  };

  // Status options
  statusOptions: Array<{ label: string; value: ResPartnerTypeStatus }> = [];

  // Table
  columns: TableColumn[] = [];
  actions: TableAction<ResPartnerTypeDto>[] = [];
  currentLazyLoadEvent?: TableLazyLoadEvent;

  constructor(
    private partnerTypeService: ResPartnerTypeService,
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
      { label: this.localizationService.localize('Partner::ResPartnerType:Active'), value: ResPartnerTypeStatus.Active },
      { label: this.localizationService.localize('Partner::ResPartnerType:Deactive'), value: ResPartnerTypeStatus.Deactive }
    ];
  }

  /**
   * Initialize table columns with localized headers
   */
  private initializeColumns(): void {
    this.columns = [
      {
        field: 'code',
        header: this.localizationService.localize('Partner::ResPartnerType:Code'),
        sortable: true,
        width: '150px'
      },
      {
        field: 'name',
        header: this.localizationService.localize('Partner::ResPartnerType:Name'),
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
        header: this.localizationService.localize('Partner::ResPartnerType:Status'),
        sortable: true,
        type: 'text',
        width: '120px',
        freeze: 'right',
        align: 'center',
        formatter: (value: any) => this.formatStatus(value),
        cellClass: (value: any) => {
          const statusValue = typeof value === 'number' ? value : (value === ResPartnerTypeStatus.Active ? 0 : 1);
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
        label: this.localizationService.localize('Partner::Edit'),
        icon: 'pi pi-pencil',
        command: (row) => this.openEditDialog(row)
      });
    }

    if (this.permissionService.isGranted(this.PERMISSIONS.DELETE)) {
      this.actions.push({
        label: this.localizationService.localize('Partner::Delete'),
        icon: 'pi pi-trash',
        command: (row) => this.delete(row)
      });
    }
  }

  /**
   * Load partner types with lazy loading
   */
  loadData(event: TableLazyLoadEvent): void {
    this.currentLazyLoadEvent = event;
    this.loading = true;

    const input: GetResPartnerTypesInput = {
      skipCount: event.first || 0,
      maxResultCount: event.rows || 10,
      sorting: this.getSortingString(event),
      code: this.searchForm.code || undefined,
      name: this.searchForm.name || undefined,
      status: this.searchForm.status ?? undefined
    };

    this.partnerTypeService.getList(input).subscribe({
      next: (result) => {
        this.partnerTypes = result.items || [];
        this.totalCount = result.totalCount || 0;
        this.loading = false;
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Partner::Error'),
          detail: this.localizationService.localize('Partner::InternalServerErrorMessage')
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
   * Listen for phím Enter toàn cục để kích hoạt tìm kiếm khi không mở dialog
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
    this.selectedPartnerType = undefined;
    this.dialogVisible = true;
  }

  /**
   * Open edit dialog
   * ⚠️ QUAN TRỌNG: Code không được phép sửa khi edit
   */
  openEditDialog(partnerType: ResPartnerTypeDto): void {
    this.dialogMode = 'edit';
    this.selectedPartnerType = partnerType;
    this.formData = {
      code: partnerType.code || '', // Readonly trong edit mode
      name: partnerType.name || '',
      status: partnerType.status ?? ResPartnerTypeStatus.Active
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
      // Validate code only when creating
      if (!this.formData.code || this.formData.code.trim() === '') {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: this.localizationService.localize('Partner::ResPartnerType:CodeRequired')
        });
        return false;
      }

      if (this.formData.code.length > 25) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: this.localizationService.localize('Partner::ResPartnerType:CodeMaxLength')
        });
        return false;
      }

      if (!this.validateCode(this.formData.code)) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: this.localizationService.localize('Partner::ResPartnerType:CodeInvalidFormat')
        });
        return false;
      }
    }

    if (!this.formData.name || this.formData.name.trim() === '') {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Partner::ResPartnerType:NameRequired')
      });
      return false;
    }

    if (this.formData.name.length > 250) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Partner::ResPartnerType:NameMaxLength')
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
   * Create new partner type
   */
  private create(): void {
    this.loading = true;

    const createDto: CreateResPartnerTypeDto = {
      code: this.formData.code.trim(),
      name: this.formData.name.trim(),
      status: this.formData.status
    };

    this.partnerTypeService.create(createDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Partner::Success'),
          detail: this.localizationService.localize('Partner::ResPartnerType:CreatedSuccessfully')
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
          summary: this.localizationService.localize('Partner::Error'),
          detail: errorMessage
        });
        this.loading = false;
      }
    });
  }

  /**
   * Update existing partner type
   * ⚠️ QUAN TRỌNG: Chỉ update Name và Status, không update Code
   */
  private update(): void {
    if (!this.selectedPartnerType || !this.selectedPartnerType.id) {
      return;
    }

    this.loading = true;

    const updateDto: UpdateResPartnerTypeDto = {
      name: this.formData.name.trim(),
      status: this.formData.status
    };

    this.partnerTypeService.update(this.selectedPartnerType.id, updateDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Partner::Success'),
          detail: this.localizationService.localize('Partner::ResPartnerType:UpdatedSuccessfully')
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
          summary: this.localizationService.localize('Partner::Error'),
          detail: errorMessage
        });
        this.loading = false;
      }
    });
  }

  /**
   * Delete partner type with confirmation
   * ⚠️ QUAN TRỌNG: Soft delete - set Status to Deactive
   */
  delete(partnerType: ResPartnerTypeDto): void {
    if (!partnerType.id) {
      return;
    }

    this.confirmationService.confirm({
      message: this.localizationService.localize('Partner::ResPartnerType:DeleteConfirm'),
      header: this.localizationService.localize('Partner::ResPartnerType:Delete'),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.loading = true;
        this.partnerTypeService.delete(partnerType.id!).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: this.localizationService.localize('Partner::Success'),
              detail: this.localizationService.localize('Partner::ResPartnerType:DeletedSuccessfully')
            });
            this.search();
          },
          error: (error) => {
            const errorMessage = error.error?.error?.message || 
                               error.error?.error?.details || 
                               this.localizationService.localize('AbpUi::InternalServerErrorMessage');
            this.messageService.add({
              severity: 'error',
              summary: this.localizationService.localize('Partner::Error'),
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
  private getEmptyForm(): PartnerTypeFormData {
    return {
      code: '',
      name: '',
      status: ResPartnerTypeStatus.Active
    };
  }

  /**
   * Format status for display
   */
  formatStatus(status: ResPartnerTypeStatus | number | string | undefined | null): string {
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
      statusValue = status === ResPartnerTypeStatus.Active ? 0 : 1;
    }
    
    return statusValue === 0
      ? this.localizationService.localize('Partner::ResPartnerType:Active')
      : this.localizationService.localize('Partner::ResPartnerType:Deactive');
  }
}

