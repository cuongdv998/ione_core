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
import { ResWardService } from '@/proxy/master/controllers/res-ward.service';
import { ResWardDto, CreateResWardDto, UpdateResWardDto, GetResWardsInput } from '@/proxy/master/res-wards/models';
import { ResWardStatus } from '@/proxy/res-wards/res-ward-status.enum';
import { ResProvinceService } from '@/proxy/master/controllers/res-province.service';
import { ResProvinceDto } from '@/proxy/master/res-provinces/models';
import { WardSearchForm, WardFormData } from './wards.models';
import { PermissionPipe } from '@/core/pipes/permission.pipe';
import { PermissionService } from '@/core/services/permission.service';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';
import { ResProvinceStatus } from '@/proxy/res-provinces';

@Component({
  selector: 'app-wards',
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
  templateUrl: './wards.component.html',
  styleUrl: './wards.component.scss',
  providers: [MessageService, ConfirmationService]
})
export class WardsComponent implements OnInit {
  // Permissions
  readonly PERMISSIONS = {
    CREATE: 'MasterResWard.Create',
    UPDATE: 'MasterResWard.Edit',
    DELETE: 'MasterResWard.Delete',
    VIEW: 'MasterResWard.View'
  };

  // Data
  wards: ResWardDto[] = [];
  totalCount = 0;
  loading = true;
  pageSize = 10;

  // Dialog
  dialogVisible = false;
  dialogMode: 'create' | 'edit' = 'create';
  formData: WardFormData = this.getEmptyForm();
  selectedWard?: ResWardDto;

  // Search form
  searchForm: WardSearchForm = {
    provinceId: null,
    code: null,
    name: null,
    status: null
  };

  // Options
  statusOptions: Array<{ label: string; value: ResWardStatus }> = [];
  provinceOptions: Array<{ label: string; value: string }> = [];
  fullProvinceOptions: Array<{ label: string; value: string }> = [];
  // Table
  columns: TableColumn[] = [];
  actions: TableAction<ResWardDto>[] = [];
  currentLazyLoadEvent?: TableLazyLoadEvent;

  constructor(
    private wardService: ResWardService,
    private provinceService: ResProvinceService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private permissionService: PermissionService,
    private localizationService: LocalizationService
  ) {
    this.initializeColumns();
    this.initializeActions();
    this.initializeStatusOptions();
    this.loadProvinces();
  }

  ngOnInit(): void {
    // Initial load will be triggered by the table's onLazyLoad event
  }

  /**
   * Load provinces for dropdown
   */
  private loadProvinces(): void {
    this.provinceService.getList({
      skipCount: 0,
      maxResultCount: 1000,
      status: undefined // Load all provinces
    }).subscribe({
      next: (result) => {
        this.provinceOptions = (result.items || []).filter(province => province.status === ResProvinceStatus.Active).map(province => ({
          label: province.name || '',
          value: province.id || ''
        }));
        this.fullProvinceOptions = (result.items || []).map(province => ({
          label: province.name || '',
          value: province.id || ''
        }));
      },
      error: () => {
        // Silently fail - provinces dropdown will be empty
      }
    });
  }

  /**
   * Initialize status options with localized labels
   */
  private initializeStatusOptions(): void {
    this.statusOptions = [
      { label: this.localizationService.localize('Master::ResWard:Active'), value: ResWardStatus.Active },
      { label: this.localizationService.localize('Master::ResWard:Deactive'), value: ResWardStatus.Deactive }
    ];
  }

  /**
   * Initialize table columns with localized headers
   */
  private initializeColumns(): void {
    this.columns = [
      {
        field: 'provinceName',
        header: this.localizationService.localize('Master::ResWard:ProvinceName'),
        sortable: false,
        width: '200px'
      },
      {
        field: 'code',
        header: this.localizationService.localize('Master::ResWard:Code'),
        sortable: true,
        width: '150px'
      },
      {
        field: 'name',
        header: this.localizationService.localize('Master::ResWard:Name'),
        sortable: true,
        width: '250px'
      },
      {
        field: 'description',
        header: this.localizationService.localize('Master::ResWard:Description'),
        sortable: false,
        width: '300px',
        wrap: true
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
        header: this.localizationService.localize('Master::ResWard:Status'),
        sortable: true,
        type: 'text',
        width: '120px',
        freeze: 'right',
        align: 'center',
        formatter: (value: any) => this.formatStatus(value),
        cellClass: (value: any) => {
          const statusValue = typeof value === 'number' ? value :
            (value === ResWardStatus.Active ? 0 : 1);
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
   * Load wards with lazy loading
   */
  loadData(event: TableLazyLoadEvent): void {
    this.currentLazyLoadEvent = event;
    this.loading = true;

    const input: GetResWardsInput = {
      skipCount: event.first || 0,
      maxResultCount: event.rows || 10,
      sorting: this.getSortingString(event),
      provinceId: this.searchForm.provinceId ? this.searchForm.provinceId : undefined,
      code: this.searchForm.code || undefined,
      name: this.searchForm.name || undefined,
      status: this.searchForm.status ?? undefined
    };

    this.wardService.getList(input).subscribe({
      next: (result) => {
        this.wards = result.items || [];
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
   * Execute search with current filters
   */
  search(): void {
    if (this.currentLazyLoadEvent) {
      this.currentLazyLoadEvent.first = 0;
      this.loadData(this.currentLazyLoadEvent);
    }
  }

  /**
   * Listen for Enter key globally to trigger search when no dialog is open
   */
  @HostListener('window:keydown.enter', ['$event'])
  onWindowKeyDown(event: KeyboardEvent): void {
    if (!this.dialogVisible) {
      this.search();
    }
  }

  /**
   * Reset search form and reload data
   */
  resetSearch(): void {
    this.searchForm = {
      provinceId: null,
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
    this.selectedWard = undefined;
    this.dialogVisible = true;
  }

  /**
   * Open edit dialog
   * ⚠️ QUAN TRỌNG: Code không được phép sửa khi edit
   */
  openEditDialog(ward: ResWardDto): void {
    this.dialogMode = 'edit';
    this.selectedWard = ward;
    this.formData = {
      provinceId: ward.provinceId || '',
      code: ward.code || '', // Readonly trong edit mode
      name: ward.name || '',
      status: ward.status ?? ResWardStatus.Active,
      description: ward.description || null
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
    if (!this.formData.provinceId || this.formData.provinceId.trim() === '') {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResWard:ProvinceRequired')
      });
      return false;
    }

    if (this.dialogMode === 'create') {
      // Validate code only when creating
      if (!this.formData.code || this.formData.code.trim() === '') {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: this.localizationService.localize('Master::ResWard:CodeRequired')
        });
        return false;
      }

      if (this.formData.code.length > 25) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: this.localizationService.localize('Master::ResWard:CodeMaxLength')
        });
        return false;
      }

      if (!this.validateCode(this.formData.code)) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: this.localizationService.localize('Master::ResWard:CodeInvalid')
        });
        return false;
      }
    }

    if (!this.formData.name || this.formData.name.trim() === '') {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResWard:NameRequired')
      });
      return false;
    }

    if (this.formData.name.length > 250) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResWard:NameMaxLength')
      });
      return false;
    }

    if (this.formData.description && this.formData.description.length > 500) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResWard:DescriptionMaxLength')
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
   * Create new ward
   */
  private create(): void {
    this.loading = true;

    const createDto: CreateResWardDto = {
      provinceId: this.formData.provinceId,
      code: this.formData.code.trim(),
      name: this.formData.name.trim(),
      status: this.formData.status,
      description: this.formData.description?.trim() || undefined
    };

    this.wardService.create(createDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Master::Success'),
          detail: this.localizationService.localize('Master::ResWard:CreatedSuccessfully')
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
   * Update existing ward
   * ⚠️ QUAN TRỌNG: Chỉ update ProvinceId, Name, Status, Description - không update Code
   */
  private update(): void {
    if (!this.selectedWard || !this.selectedWard.id) {
      return;
    }

    this.loading = true;

    const updateDto: UpdateResWardDto = {
      provinceId: this.formData.provinceId,
      name: this.formData.name.trim(),
      status: this.formData.status,
      description: this.formData.description?.trim() || undefined
    };

    this.wardService.update(this.selectedWard.id, updateDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Master::Success'),
          detail: this.localizationService.localize('Master::ResWard:UpdatedSuccessfully')
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
   * Delete ward with confirmation
   * ⚠️ QUAN TRỌNG: Soft delete - set Status to Deactive
   */
  delete(ward: ResWardDto): void {
    if (!ward.id) {
      return;
    }

    this.confirmationService.confirm({
      message: this.localizationService.localize('Master::ResWard:DeleteConfirm'),
      header: this.localizationService.localize('Master::ResWard:Delete'),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.loading = true;
        this.wardService.delete(ward.id!).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: this.localizationService.localize('Master::Success'),
              detail: this.localizationService.localize('Master::ResWard:DeletedSuccessfully')
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
  private getEmptyForm(): WardFormData {
    return {
      provinceId: '',
      code: '',
      name: '',
      status: ResWardStatus.Active,
      description: null
    };
  }

  /**
   * Format status for display
   */
  formatStatus(status: ResWardStatus | number | string | undefined | null): string {
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
      statusValue = status === ResWardStatus.Active ? 0 : 1;
    }

    return statusValue === 0
      ? this.localizationService.localize('Master::ResWard:Active')
      : this.localizationService.localize('Master::ResWard:Deactive');
  }
}

