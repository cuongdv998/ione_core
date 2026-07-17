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
import { ResOrganizationTypeService } from '@/proxy/partner/controllers/res-organization-type.service';
import { ResOrganizationTypeDto, CreateResOrganizationTypeDto, UpdateResOrganizationTypeDto, GetResOrganizationTypesInput } from '@/proxy/partner/res-organization-types/models';
import { ResOrganizationTypeStatus } from '@/proxy/res-organization-types/res-organization-type-status.enum';
import { OrganizationTypeType } from '@/proxy/res-organization-types/organization-type-type.enum';
import { ResOrganizationTypeSearchForm, ResOrganizationTypeFormData } from './res-organization-types.models';
import { PermissionPipe } from '@/core/pipes/permission.pipe';
import { PermissionService } from '@/core/services/permission.service';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';

@Component({
  selector: 'app-res-organization-types',
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
  templateUrl: './res-organization-types.component.html',
  styleUrl: './res-organization-types.component.scss',
  providers: [MessageService, ConfirmationService]
})
export class ResOrganizationTypesComponent implements OnInit {
  // Permissions
  readonly PERMISSIONS = {
    CREATE: 'PartnerResOrganizationType.Create',
    UPDATE: 'PartnerResOrganizationType.Edit',
    DELETE: 'PartnerResOrganizationType.Delete',
    VIEW: 'PartnerResOrganizationType.View'
  };

  // Data
  resOrganizationTypes: ResOrganizationTypeDto[] = [];
  totalCount = 0;
  loading = true;
  pageSize = 10;

  // Dialog
  dialogVisible = false;
  dialogMode: 'create' | 'edit' = 'create';
  formData: ResOrganizationTypeFormData = this.getEmptyForm();
  selectedResOrganizationType?: ResOrganizationTypeDto;

  // Search form
  searchForm: ResOrganizationTypeSearchForm = {
    code: null,
    name: null,
    status: null
  };

  // Status options
  statusOptions: Array<{ label: string; value: ResOrganizationTypeStatus }> = [];

  // Type options
  typeOptions: Array<{ label: string; value: OrganizationTypeType }> = [];

  // Table
  columns: TableColumn[] = [];
  actions: TableAction<ResOrganizationTypeDto>[] = [];
  currentLazyLoadEvent?: TableLazyLoadEvent;

  constructor(
    private resOrganizationTypeService: ResOrganizationTypeService,
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
    // Initial load will be triggered by the table's onLazyLoad event
  }

  /**
   * Initialize status options with localized labels
   */
  private initializeStatusOptions(): void {
    this.statusOptions = [
      { label: this.localizationService.localize('Partner::ResOrganizationType:Status:Active'), value: ResOrganizationTypeStatus.Active },
      { label: this.localizationService.localize('Partner::ResOrganizationType:Status:Deactive'), value: ResOrganizationTypeStatus.Deactive }
    ];
  }

  /**
   * Initialize type options with localized labels
   */
  private initializeTypeOptions(): void {
    this.typeOptions = [
      { label: this.localizationService.localize('Partner::ResOrganizationType:Type:TC'), value: OrganizationTypeType.TC },
      { label: this.localizationService.localize('Partner::ResOrganizationType:Type:CN'), value: OrganizationTypeType.CN }
    ];
  }

  /**
   * Initialize table columns with localized headers
   */
  private initializeColumns(): void {
    this.columns = [
      {
        field: 'code',
        header: this.localizationService.localize('Partner::ResOrganizationType:Code'),
        sortable: true,
        width: '150px'
      },
      {
        field: 'name',
        header: this.localizationService.localize('Partner::ResOrganizationType:Name'),
        sortable: true,
        width: '250px'
      },
      {
        field: 'type',
        header: this.localizationService.localize('Partner::ResOrganizationType:Type'),
        sortable: true,
        type: 'text',
        width: '120px',
        align: 'center',
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
        header: this.localizationService.localize('Partner::ResOrganizationType:Status'),
        sortable: true,
        type: 'text',
        width: '120px',
        freeze: 'right',
        align: 'center',
        formatter: (value: any) => this.formatStatus(value),
        cellClass: (value: any) => {
          const statusValue = typeof value === 'number' ? value : (value === ResOrganizationTypeStatus.Active ? 0 : 1);
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
   * Load res organization types with lazy loading
   */
  loadData(event: TableLazyLoadEvent): void {
    this.currentLazyLoadEvent = event;
    this.loading = true;

    const input: GetResOrganizationTypesInput = {
      skipCount: event.first || 0,
      maxResultCount: event.rows || 10,
      sorting: this.getSortingString(event),
      code: this.searchForm.code || undefined,
      name: this.searchForm.name || undefined,
      status: this.searchForm.status ?? undefined
    };

    this.resOrganizationTypeService.getList(input).subscribe({
      next: (result) => {
        this.resOrganizationTypes = result.items || [];
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
    this.selectedResOrganizationType = undefined;
    this.dialogVisible = true;
  }

  /**
   * Open edit dialog
   * ⚠️ QUAN TRỌNG: Type không được phép sửa khi edit
   */
  openEditDialog(resOrganizationType: ResOrganizationTypeDto): void {
    this.dialogMode = 'edit';
    this.selectedResOrganizationType = resOrganizationType;
    this.formData = {
      code: resOrganizationType.code || '',
      name: resOrganizationType.name || '',
      status: resOrganizationType.status ?? ResOrganizationTypeStatus.Active,
      type: resOrganizationType.type ?? OrganizationTypeType.TC // Readonly trong edit mode
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
          detail: this.localizationService.localize('Partner::ResOrganizationType:CodeRequired')
        });
        return false;
      }

      if (this.formData.code.length > 25) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: this.localizationService.localize('Partner::ResOrganizationType:CodeMaxLength')
        });
        return false;
      }

      if (!this.validateCode(this.formData.code)) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: this.localizationService.localize('Partner::ResOrganizationType:CodeInvalidFormat')
        });
        return false;
      }
    }

    if (!this.formData.name || this.formData.name.trim() === '') {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Partner::ResOrganizationType:NameRequired')
      });
      return false;
    }

    if (this.formData.name.length > 250) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Partner::ResOrganizationType:NameMaxLength')
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
   * Create new res organization type
   */
  private create(): void {
    this.loading = true;

    const createDto: CreateResOrganizationTypeDto = {
      code: this.formData.code.trim(),
      name: this.formData.name.trim(),
      status: this.formData.status,
      type: this.formData.type
    };

    this.resOrganizationTypeService.create(createDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Partner::Success'),
          detail: this.localizationService.localize('Partner::ResOrganizationType:CreatedSuccessfully')
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
   * Update existing res organization type
   */
  private update(): void {
    if (!this.selectedResOrganizationType || !this.selectedResOrganizationType.id) {
      return;
    }

    this.loading = true;

    const updateDto: UpdateResOrganizationTypeDto = {
      name: this.formData.name.trim(),
      status: this.formData.status,
      type: this.formData.type // Type có thể được update (nhưng UI sẽ disable để user không sửa)
    };

    this.resOrganizationTypeService.update(this.selectedResOrganizationType.id, updateDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Partner::Success'),
          detail: this.localizationService.localize('Partner::ResOrganizationType:UpdatedSuccessfully')
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
   * Delete res organization type with confirmation
   */
  delete(resOrganizationType: ResOrganizationTypeDto): void {
    if (!resOrganizationType.id) {
      return;
    }

    this.confirmationService.confirm({
      message: this.localizationService.localize('Partner::ResOrganizationType:DeleteConfirm'),
      header: this.localizationService.localize('Partner::ResOrganizationType:Delete'),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.loading = true;
        this.resOrganizationTypeService.delete(resOrganizationType.id!).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: this.localizationService.localize('Partner::Success'),
              detail: this.localizationService.localize('Partner::ResOrganizationType:DeletedSuccessfully')
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
  private getEmptyForm(): ResOrganizationTypeFormData {
    return {
      code: '',
      name: '',
      status: ResOrganizationTypeStatus.Active,
      type: OrganizationTypeType.TC // Mặc định là TC (Tổ chức)
    };
  }

  /**
   * Format status for display
   */
  formatStatus(status: ResOrganizationTypeStatus | number | string | undefined | null): string {
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
      statusValue = status === ResOrganizationTypeStatus.Active ? 0 : 1;
    }
    
    return statusValue === 0
      ? this.localizationService.localize('Partner::ResOrganizationType:Status:Active')
      : this.localizationService.localize('Partner::ResOrganizationType:Status:Deactive');
  }

  /**
   * Format type for display
   */
  formatType(type: OrganizationTypeType | number | string | undefined | null): string {
    if (type === undefined || type === null) {
      return '';
    }

    let typeValue: number;

    if (typeof type === 'number') {
      typeValue = type;
    } else if (typeof type === 'string') {
      if (type === 'TC' || type === 'tc' || type === '0') {
        typeValue = 0;
      } else if (type === 'CN' || type === 'cn' || type === '1') {
        typeValue = 1;
      } else {
        typeValue = 0; // Default to TC
      }
    } else {
      typeValue = type === OrganizationTypeType.TC ? 0 : 1;
    }

    return typeValue === 0
      ? this.localizationService.localize('Partner::ResOrganizationType:Type:TC')
      : this.localizationService.localize('Partner::ResOrganizationType:Type:CN');
  }
}

