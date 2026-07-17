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
import { TextareaModule } from 'primeng/textarea';

import { VTable } from '@/shared/components/v-table/v-table';
import { TableColumn } from '@/shared/models/table-column.model';
import { TableAction } from '@/shared/models/table-action.model';
import { ResReasonService } from '@/proxy/master/controllers/res-reason.service';
import { ResReasonDto, CreateResReasonDto, UpdateResReasonDto, GetResReasonsInput } from '@/proxy/master/res-reasons/models';
import { ResReasonGroupStatus } from '@/proxy/res-reason-groups/res-reason-group-status.enum';
import { ResReasonGroupService } from '@/proxy/master/controllers/res-reason-group.service';
import { ResReasonGroupDto, GetResReasonGroupsInput } from '@/proxy/master/res-reason-groups/models';
import { ResReasonSearchForm, ResReasonFormData } from './res-reasons.models';
import { PermissionPipe } from '@/core/pipes/permission.pipe';
import { PermissionService } from '@/core/services/permission.service';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';

@Component({
  selector: 'app-res-reasons',
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
    VTable,
    PermissionPipe,
    TranslatePipe
  ],
  templateUrl: './res-reasons.component.html',
  styleUrl: './res-reasons.component.scss',
  providers: [MessageService, ConfirmationService]
})
export class ResReasonsComponent implements OnInit {
  // Permissions
  readonly PERMISSIONS = {
    CREATE: 'MasterResReason.Create',
    UPDATE: 'MasterResReason.Edit',
    DELETE: 'MasterResReason.Delete',
    VIEW: 'MasterResReason.View'
  };

  // Data
  reasons: ResReasonDto[] = [];
  totalCount = 0;
  loading = true;
  pageSize = 10;

  // Dialog
  dialogVisible = false;
  dialogMode: 'create' | 'edit' = 'create';
  formData: ResReasonFormData = this.getEmptyForm();
  selectedReason?: ResReasonDto;

  // Search form
  searchForm: ResReasonSearchForm = {
    groupId: null,
    code: null,
    name: null,
    status: null
  };

  // Status options
  statusOptions: Array<{ label: string; value: ResReasonGroupStatus }> = [];

  // Reason Group options
  reasonGroupOptions: Array<{ label: string; value: string }> = [];
  fullReasonGroupOptions: Array<{ label: string; value: string }> = [];
  loadingReasonGroups = false;

  // Table
  columns: TableColumn[] = [];
  actions: TableAction<ResReasonDto>[] = [];
  currentLazyLoadEvent?: TableLazyLoadEvent;

  constructor(
    private reasonService: ResReasonService,
    private reasonGroupService: ResReasonGroupService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private permissionService: PermissionService,
    private localizationService: LocalizationService
  ) {
    this.initializeColumns();
    this.initializeActions();
    this.initializeStatusOptions();
    this.loadReasonGroups();
  }

  ngOnInit(): void {
    // Initial load will be triggered by the table's onLazyLoad event
  }

  /**
   * Initialize status options with localized labels
   */
  private initializeStatusOptions(): void {
    this.statusOptions = [
      { label: this.localizationService.localize('Master::ResReason:Active'), value: ResReasonGroupStatus.Active },
      { label: this.localizationService.localize('Master::ResReason:Deactive'), value: ResReasonGroupStatus.Deactive }
    ];
  }

  /**
   * Load reason groups for dropdown
   */
  private loadReasonGroups(): void {
    this.loadingReasonGroups = true;
    const input: GetResReasonGroupsInput = {
      skipCount: 0,
      maxResultCount: 1000,
      sorting: 'name asc'
    };

    this.reasonGroupService.getList(input).subscribe({
      next: (result) => {
        this.reasonGroupOptions = (result.items || []).filter(x => x.status == ResReasonGroupStatus.Active).map(reasonGroup => ({
          label: `${reasonGroup.code} - ${reasonGroup.name}`,
          value: reasonGroup.id || ''
        }));
        this.fullReasonGroupOptions = (result.items || []).map(reasonGroup => ({
          label: `${reasonGroup.code} - ${reasonGroup.name}`,
          value: reasonGroup.id || ''
        }));
        this.loadingReasonGroups = false;
      },
      error: () => {
        // Silently fail - reason groups will just be empty
        this.loadingReasonGroups = false;
      }
    });
  }

  /**
   * Initialize table columns with localized headers
   */
  private initializeColumns(): void {
    this.columns = [
      {
        field: 'groupId',
        header: this.localizationService.localize('Master::ResReason:GroupId'),
        sortable: false,
        width: '200px',
        formatter: (value: any, row: ResReasonDto) => {
          const reasonGroup = this.fullReasonGroupOptions.find(opt => opt.value === row.groupId);
          return reasonGroup ? reasonGroup.label : (row.groupId || '');
        }
      },
      {
        field: 'code',
        header: this.localizationService.localize('Master::ResReason:Code'),
        sortable: true,
        width: '150px'
      },
      {
        field: 'name',
        header: this.localizationService.localize('Master::ResReason:Name'),
        sortable: true,
        width: '250px'
      },
      {
        field: 'description',
        header: this.localizationService.localize('Master::ResReason:Description'),
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
        header: this.localizationService.localize('Master::ResReason:Status'),
        sortable: true,
        type: 'text',
        width: '120px',
        freeze: 'right',
        align: 'center',
        formatter: (value: any) => this.formatStatus(value),
        cellClass: (value: any) => {
          const statusValue = typeof value === 'number' ? value : 
            (value === ResReasonGroupStatus.Active ? 0 : 1);
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
   * Load reasons with lazy loading
   */
  loadData(event: TableLazyLoadEvent): void {
    this.currentLazyLoadEvent = event;
    this.loading = true;

    const input: GetResReasonsInput = {
      skipCount: event.first || 0,
      maxResultCount: event.rows || 10,
      sorting: this.getSortingString(event),
      groupId: this.searchForm.groupId || undefined,
      code: this.searchForm.code || undefined,
      name: this.searchForm.name || undefined,
      status: this.searchForm.status ?? undefined
    };

    this.reasonService.getList(input).subscribe({
      next: (result) => {
        this.reasons = result.items || [];
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
      groupId: null,
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
    this.selectedReason = undefined;
    this.dialogVisible = true;
  }

  /**
   * Open edit dialog
   * ⚠️ QUAN TRỌNG: Code và GroupId không được phép sửa khi edit
   */
  openEditDialog(reason: ResReasonDto): void {
    this.dialogMode = 'edit';
    this.selectedReason = reason;
    this.formData = {
      groupId: reason.groupId || '', // Readonly trong edit mode
      code: reason.code || '', // Readonly trong edit mode
      name: reason.name || '',
      description: reason.description || '',
      status: reason.status ?? ResReasonGroupStatus.Active
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
      // Validate groupId only when creating
      if (!this.formData.groupId || this.formData.groupId.trim() === '') {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: this.localizationService.localize('Master::ResReason:GroupIdRequired')
        });
        return false;
      }

      // Validate code only when creating
      if (!this.formData.code || this.formData.code.trim() === '') {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: this.localizationService.localize('Master::ResReason:CodeRequired')
        });
        return false;
      }

      if (this.formData.code.length > 50) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: this.localizationService.localize('Master::ResReason:CodeMaxLength')
        });
        return false;
      }

      if (!this.validateCode(this.formData.code)) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: this.localizationService.localize('Master::ResReason:CodeInvalid')
        });
        return false;
      }
    }

    if (!this.formData.name || this.formData.name.trim() === '') {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResReason:NameRequired')
      });
      return false;
    }

    if (this.formData.name.length > 250) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResReason:NameMaxLength')
      });
      return false;
    }

    if (this.formData.description && this.formData.description.length > 500) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResReason:DescriptionMaxLength')
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
    // e.g., "Master:ResReason:CodeExists" -> "Master::ResReason:CodeExists"
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
   * Create new reason
   */
  private create(): void {
    this.loading = true;

    const createDto: CreateResReasonDto = {
      groupId: this.formData.groupId.trim(),
      code: this.formData.code.trim(),
      name: this.formData.name.trim(),
      description: this.formData.description?.trim() || undefined,
      status: this.formData.status
    };

    this.reasonService.create(createDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Master::Success'),
          detail: this.localizationService.localize('Master::ResReason:CreatedSuccessfully')
        });
        this.dialogVisible = false;
        this.search();
      },
      error: (error) => {
        let errorMessage = error.error?.error?.message || 
                           error.error?.error?.details || 
                           this.localizationService.localize('Master::InternalServerErrorMessage');
        errorMessage = this.translateErrorMessage(errorMessage, error);
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
   * Update existing reason
   * ⚠️ QUAN TRỌNG: Chỉ update Name, Description và Status, không update Code và GroupId
   */
  private update(): void {
    if (!this.selectedReason || !this.selectedReason.id) {
      return;
    }

    this.loading = true;

    const updateDto: UpdateResReasonDto = {
      name: this.formData.name.trim(),
      description: this.formData.description?.trim() || undefined,
      status: this.formData.status
    };

    this.reasonService.update(this.selectedReason.id, updateDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Master::Success'),
          detail: this.localizationService.localize('Master::ResReason:UpdatedSuccessfully')
        });
        this.dialogVisible = false;
        this.search();
      },
      error: (error) => {
        let errorMessage = error.error?.error?.message || 
                           error.error?.error?.details || 
                           this.localizationService.localize('Master::InternalServerErrorMessage');
        errorMessage = this.translateErrorMessage(errorMessage, error);
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
   * Delete reason with confirmation
   */
  delete(reason: ResReasonDto): void {
    if (!reason.id) {
      return;
    }

    this.confirmationService.confirm({
      message: this.localizationService.localize('Master::ResReason:DeleteConfirm'),
      header: this.localizationService.localize('Master::ResReason:Delete'),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.loading = true;
        this.reasonService.delete(reason.id!).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: this.localizationService.localize('Master::Success'),
              detail: this.localizationService.localize('Master::ResReason:DeletedSuccessfully')
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
  private getEmptyForm(): ResReasonFormData {
    return {
      groupId: '',
      code: '',
      name: '',
      description: '',
      status: ResReasonGroupStatus.Active
    };
  }

  /**
   * Format status for display
   */
  formatStatus(status: ResReasonGroupStatus | number | string | undefined | null): string {
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
      statusValue = status === ResReasonGroupStatus.Active ? 0 : 1;
    }
    
    return statusValue === 0
      ? this.localizationService.localize('Master::ResReason:Active')
      : this.localizationService.localize('Master::ResReason:Deactive');
  }
}
