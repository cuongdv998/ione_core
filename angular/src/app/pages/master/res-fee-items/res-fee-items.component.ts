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
import { ResFeeItemService } from '@/proxy/master/res-fee-items/res-fee-item.service';
import { ResFeeItemDto, CreateResFeeItemDto, UpdateResFeeItemDto, GetResFeeItemsInput } from '@/proxy/master/res-fee-items/models';
import { ResFeeItemStatus } from '@/proxy/res-fee-items/res-fee-item-status.enum';
import { ResTaxService } from '@/proxy/product/controllers/res-tax.service';
import { ResTaxDto, GetResTaxesInput } from '@/proxy/product/res-taxes/models';
import { ResTaxStatus } from '@/proxy/res-taxes/res-tax-status.enum';
import { FeeItemSearchForm, FeeItemFormData } from './res-fee-items.models';
import { PermissionPipe } from '@/core/pipes/permission.pipe';
import { PermissionService } from '@/core/services/permission.service';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';

@Component({
  selector: 'app-res-fee-items',
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
  templateUrl: './res-fee-items.component.html',
  styleUrl: './res-fee-items.component.scss',
  providers: [MessageService, ConfirmationService]
})
export class ResFeeItemsComponent implements OnInit {
  // Permissions
  readonly PERMISSIONS = {
    CREATE: 'MasterResFeeItem.Create',
    UPDATE: 'MasterResFeeItem.Edit',
    DELETE: 'MasterResFeeItem.Delete',
    VIEW: 'MasterResFeeItem.View'
  };

  // Data
  feeItems: ResFeeItemDto[] = [];
  totalCount = 0;
  loading = true;
  pageSize = 10;

  // Dialog
  dialogVisible = false;
  dialogMode: 'create' | 'edit' = 'create';
  formData: FeeItemFormData = this.getEmptyForm();
  selectedFeeItem?: ResFeeItemDto;

  // Search form
  searchForm: FeeItemSearchForm = {
    code: null,
    name: null,
    status: null
  };

  // Status options
  statusOptions: Array<{ label: string; value: ResFeeItemStatus }> = [];

  // Tax options
  taxOptions: Array<{ label: string; value: string }> = [];
  fullTaxOptions: Array<{ label: string; value: string }> = [];
  loadingTaxes = false;

  // Form validation errors (inline)
  formErrors: {
    code: string | null;
    name: string | null;
    description: string | null;
  } = {
      code: null,
      name: null,
      description: null
    };

  // Table
  columns: TableColumn[] = [];
  actions: TableAction<ResFeeItemDto>[] = [];
  currentLazyLoadEvent?: TableLazyLoadEvent;

  constructor(
    private feeItemService: ResFeeItemService,
    private taxService: ResTaxService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private permissionService: PermissionService,
    private localizationService: LocalizationService
  ) {
    this.initializeColumns();
    this.initializeActions();
    this.initializeStatusOptions();
    this.loadTaxes();
  }

  ngOnInit(): void {
    // Initial load will be triggered by the table's onLazyLoad event
  }

  /**
   * Initialize status options with localized labels
   */
  private initializeStatusOptions(): void {
    this.statusOptions = [
      { label: this.localizationService.localize('Master::ResFeeItem:Active'), value: ResFeeItemStatus.Active },
      { label: this.localizationService.localize('Master::ResFeeItem:Deactive'), value: ResFeeItemStatus.Deactive }
    ];
  }

  /**
   * Load taxes for dropdown
   */
  private loadTaxes(): void {
    this.loadingTaxes = true;
    const input: GetResTaxesInput = {
      skipCount: 0,
      maxResultCount: 1000,
      sorting: 'name asc'
    };

    this.taxService.getList(input).subscribe({
      next: (result) => {
        this.taxOptions = (result.items || []).filter(tax => tax.status === ResTaxStatus.Active).map(tax => ({
          label: `${tax.code} - ${tax.name}`,
          value: tax.id || ''
        }));
        this.fullTaxOptions = (result.items || []).map(tax => ({
          label: `${tax.code} - ${tax.name}`,
          value: tax.id || ''
        }));
        this.loadingTaxes = false;
      },
      error: () => {
        // Silently fail - taxes will just be empty
        this.loadingTaxes = false;
      }
    });
  }

  /**
   * Initialize table columns with localized headers
   */
  private initializeColumns(): void {
    this.columns = [
      {
        field: 'code',
        header: this.localizationService.localize('Master::ResFeeItem:Code'),
        sortable: true,
        width: '150px'
      },
      {
        field: 'name',
        header: this.localizationService.localize('Master::ResFeeItem:Name'),
        sortable: true,
        width: '250px'
      },
      {
        field: 'taxId',
        header: this.localizationService.localize('Master::ResFeeItem:Tax'),
        sortable: false,
        width: '200px',
        formatter: (value: any, row: ResFeeItemDto) => {
          const tax = this.fullTaxOptions.find(opt => opt.value === row.taxId);
          return tax ? tax.label : (row.taxId || '');
        }
      },
      {
        field: 'description',
        header: this.localizationService.localize('Master::ResFeeItem:Description'),
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
        header: this.localizationService.localize('Master::ResFeeItem:Status'),
        sortable: true,
        type: 'text',
        width: '120px',
        freeze: 'right',
        align: 'center',
        formatter: (value: any) => this.formatStatus(value),
        cellClass: (value: any) => {
          const statusValue = typeof value === 'number' ? value :
            (value === ResFeeItemStatus.Active ? 0 : 1);
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
   * Load fee items with lazy loading
   */
  loadData(event: TableLazyLoadEvent): void {
    this.currentLazyLoadEvent = event;
    this.loading = true;

    const input: GetResFeeItemsInput = {
      skipCount: event.first || 0,
      maxResultCount: event.rows || 10,
      sorting: this.getSortingString(event),
      code: this.searchForm.code || undefined,
      name: this.searchForm.name || undefined,
      status: this.searchForm.status ?? undefined
    };

    this.feeItemService.getList(input).subscribe({
      next: (result) => {
        this.feeItems = result.items || [];
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
    this.clearFormErrors();
    this.selectedFeeItem = undefined;
    this.dialogVisible = true;
  }

  /**
   * Open edit dialog
   * ⚠️ QUAN TRỌNG: Code không được phép sửa khi edit
   */
  openEditDialog(feeItem: ResFeeItemDto): void {
    this.dialogMode = 'edit';
    this.selectedFeeItem = feeItem;
    this.formData = {
      code: feeItem.code || '', // Readonly trong edit mode
      name: feeItem.name || '',
      taxId: feeItem.taxId || null,
      description: feeItem.description || '',
      status: feeItem.status ?? ResFeeItemStatus.Active
    };
    this.clearFormErrors();
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
   * Clear form validation errors
   */
  private clearFormErrors(): void {
    this.formErrors = {
      code: null,
      name: null,
      description: null
    };
  }

  /**
   * Validate form with inline error messages
   */
  private validateForm(): boolean {
    this.clearFormErrors();
    let isValid = true;

    if (this.dialogMode === 'create') {
      // Validate code only when creating
      if (!this.formData.code || this.formData.code.trim() === '') {
        this.formErrors.code = this.localizationService.localize('Master::ResFeeItem:CodeRequired');
        isValid = false;
      } else if (this.formData.code.length > 50) {
        this.formErrors.code = this.localizationService.localize('Master::ResFeeItem:CodeMaxLength');
        isValid = false;
      } else if (!this.validateCode(this.formData.code)) {
        this.formErrors.code = this.localizationService.localize('Master::ResFeeItem:CodeInvalid');
        isValid = false;
      }
    }

    if (!this.formData.name || this.formData.name.trim() === '') {
      this.formErrors.name = this.localizationService.localize('Master::ResFeeItem:NameRequired');
      isValid = false;
    } else if (this.formData.name.length > 250) {
      this.formErrors.name = this.localizationService.localize('Master::ResFeeItem:NameMaxLength');
      isValid = false;
    }

    if (this.formData.description && this.formData.description.length > 500) {
      this.formErrors.description = this.localizationService.localize('Master::ResFeeItem:DescriptionMaxLength');
      isValid = false;
    }

    return isValid;
  }

  /**
   * Validate code format (A-Z, 0-9, _)
   */
  validateCode(code: string): boolean {
    return /^[A-Z0-9_]+$/.test(code);
  }

  /**
   * Convert code to uppercase on input and clear error
   */
  onCodeInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    const value = input.value.toUpperCase();
    input.value = value;
    this.formData.code = value;
    // Clear code error when user types
    if (this.formErrors.code) {
      this.formErrors.code = null;
    }
  }

  /**
   * Clear name error on input change
   */
  onNameInput(): void {
    if (this.formErrors.name) {
      this.formErrors.name = null;
    }
  }

  /**
   * Clear description error on input change
   */
  onDescriptionInput(): void {
    if (this.formErrors.description) {
      this.formErrors.description = null;
    }
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
    // e.g., "Master:ResFeeItem:CodeExists" -> "Master::ResFeeItem:CodeExists"
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
   * Create new fee item
   */
  private create(): void {
    this.loading = true;

    const createDto: CreateResFeeItemDto = {
      code: this.formData.code.trim(),
      name: this.formData.name.trim(),
      taxId: this.formData.taxId || undefined,
      description: this.formData.description?.trim() || undefined,
      status: this.formData.status
    };

    this.feeItemService.create(createDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Master::Success'),
          detail: this.localizationService.localize('Master::ResFeeItem:CreatedSuccessfully')
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
   * Update existing fee item
   * ⚠️ QUAN TRỌNG: Chỉ update Name, Description, Status và TaxId, không update Code
   */
  private update(): void {
    if (!this.selectedFeeItem || !this.selectedFeeItem.id) {
      return;
    }

    this.loading = true;

    const updateDto: UpdateResFeeItemDto = {
      name: this.formData.name.trim(),
      taxId: this.formData.taxId || undefined,
      description: this.formData.description?.trim() || undefined,
      status: this.formData.status
    };

    this.feeItemService.update(this.selectedFeeItem.id, updateDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Master::Success'),
          detail: this.localizationService.localize('Master::ResFeeItem:UpdatedSuccessfully')
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
   * Delete fee item with confirmation
   */
  delete(feeItem: ResFeeItemDto): void {
    if (!feeItem.id) {
      return;
    }

    this.confirmationService.confirm({
      message: this.localizationService.localize('Master::ResFeeItem:DeleteConfirm'),
      header: this.localizationService.localize('Master::ResFeeItem:Delete'),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.loading = true;
        this.feeItemService.delete(feeItem.id!).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: this.localizationService.localize('Master::Success'),
              detail: this.localizationService.localize('Master::ResFeeItem:DeletedSuccessfully')
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
  private getEmptyForm(): FeeItemFormData {
    return {
      code: '',
      name: '',
      taxId: null,
      description: '',
      status: ResFeeItemStatus.Active
    };
  }

  /**
   * Format status for display
   */
  formatStatus(status: ResFeeItemStatus | number | string | undefined | null): string {
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
      statusValue = status === ResFeeItemStatus.Active ? 0 : 1;
    }

    return statusValue === 0
      ? this.localizationService.localize('Master::ResFeeItem:Active')
      : this.localizationService.localize('Master::ResFeeItem:Deactive');
  }
}
