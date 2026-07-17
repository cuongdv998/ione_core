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
import { ResRiskService } from '@/proxy/master/controllers/res-risk.service';
import { ResRiskDto, CreateResRiskDto, UpdateResRiskDto, GetResRisksInput } from '@/proxy/master/res-risks/models';
import { ResRiskStatus } from '@/proxy/res-risks/res-risk-status.enum';
import { ResObjectTypeService } from '@/proxy/master/controllers/res-object-type.service';
import { ResObjectTypeDto, GetResObjectTypesInput } from '@/proxy/master/res-object-types/models';
import { ResObjectTypeStatus } from '@/proxy/res-object-types/res-object-type-status.enum';
import { RiskSearchForm, RiskFormData } from './risks.models';
import { PermissionPipe } from '@/core/pipes/permission.pipe';
import { PermissionService } from '@/core/services/permission.service';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';

@Component({
  selector: 'app-risks',
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
  templateUrl: './risks.component.html',
  styleUrl: './risks.component.scss',
  providers: [MessageService, ConfirmationService]
})
export class RisksComponent implements OnInit {
  // Permissions
  readonly PERMISSIONS = {
    CREATE: 'MasterResRisk.Create',
    UPDATE: 'MasterResRisk.Edit',
    DELETE: 'MasterResRisk.Delete',
    VIEW: 'MasterResRisk.View'
  };

  // Data
  risks: ResRiskDto[] = [];
  totalCount = 0;
  loading = true;
  pageSize = 10;

  // Dialog
  dialogVisible = false;
  dialogMode: 'create' | 'edit' = 'create';
  formData: RiskFormData = this.getEmptyForm();
  selectedRisk?: ResRiskDto;

  // Search form
  searchForm: RiskSearchForm = {
    objectTypeId: null,
    code: null,
    name: null,
    status: null
  };

  // Status options
  statusOptions: Array<{ label: string; value: ResRiskStatus }> = [];

  // Object Type options
  objectTypeOptions: Array<{ label: string; value: string }> = [];
  fullObjectTypeOptions: Array<{ label: string; value: string }> = [];
  loadingObjectTypes = false;

  // Table
  columns: TableColumn[] = [];
  actions: TableAction<ResRiskDto>[] = [];
  currentLazyLoadEvent?: TableLazyLoadEvent;

  constructor(
    private riskService: ResRiskService,
    private objectTypeService: ResObjectTypeService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private permissionService: PermissionService,
    private localizationService: LocalizationService
  ) {
    this.initializeColumns();
    this.initializeActions();
    this.initializeStatusOptions();
    this.loadObjectTypes();
  }

  ngOnInit(): void {
    // Initial load will be triggered by the table's onLazyLoad event
  }

  /**
   * Initialize status options with localized labels
   */
  private initializeStatusOptions(): void {
    this.statusOptions = [
      { label: this.localizationService.localize('Master::ResRisk:Active'), value: ResRiskStatus.Active },
      { label: this.localizationService.localize('Master::ResRisk:Deactive'), value: ResRiskStatus.Deactive }
    ];
  }

  /**
   * Load object types for dropdown
   */
  private loadObjectTypes(): void {
    this.loadingObjectTypes = true;
    const input: GetResObjectTypesInput = {
      skipCount: 0,
      maxResultCount: 1000,
      sorting: 'name asc'
    };

    this.objectTypeService.getList(input).subscribe({
      next: (result) => {
        this.objectTypeOptions = (result.items || []).filter(x => x.status == ResObjectTypeStatus.Active).map(objectType => ({
          label: `${objectType.code} - ${objectType.name}`,
          value: objectType.id || ''
        }));
        this.fullObjectTypeOptions = (result.items || []).map(objectType => ({
          label: `${objectType.code} - ${objectType.name}`,
          value: objectType.id || ''
        }));
        this.loadingObjectTypes = false;
      },
      error: () => {
        // Silently fail - object types will just be empty
        this.loadingObjectTypes = false;
      }
    });
  }

  /**
   * Initialize table columns with localized headers
   */
  private initializeColumns(): void {
    this.columns = [
      {
        field: 'objectTypeId',
        header: this.localizationService.localize('Master::ResRisk:ObjectType'),
        sortable: false,
        width: '200px',
        formatter: (value: any, row: ResRiskDto) => {
          const objectType = this.fullObjectTypeOptions.find(opt => opt.value === row.objectTypeId);
          return objectType ? objectType.label : (row.objectTypeId || '');
        }
      },
      {
        field: 'code',
        header: this.localizationService.localize('Master::ResRisk:Code'),
        sortable: true,
        width: '150px'
      },
      {
        field: 'name',
        header: this.localizationService.localize('Master::ResRisk:Name'),
        sortable: true,
        width: '250px'
      },
      {
        field: 'description',
        header: this.localizationService.localize('Master::ResRisk:Description'),
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
        header: this.localizationService.localize('Master::ResRisk:Status'),
        sortable: true,
        type: 'text',
        width: '120px',
        freeze: 'right',
        align: 'center',
        formatter: (value: any) => this.formatStatus(value),
        cellClass: (value: any) => {
          const statusValue = typeof value === 'number' ? value :
            (value === ResRiskStatus.Active ? 0 : 1);
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
   * Load risks with lazy loading
   */
  loadData(event: TableLazyLoadEvent): void {
    this.currentLazyLoadEvent = event;
    this.loading = true;

    const input: GetResRisksInput = {
      skipCount: event.first || 0,
      maxResultCount: event.rows || 10,
      sorting: this.getSortingString(event),
      objectTypeId: this.searchForm.objectTypeId || undefined,
      code: this.searchForm.code || undefined,
      name: this.searchForm.name || undefined,
      status: this.searchForm.status ?? undefined
    };

    this.riskService.getList(input).subscribe({
      next: (result) => {
        this.risks = result.items || [];
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
      objectTypeId: null,
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
    this.selectedRisk = undefined;
    this.dialogVisible = true;
  }

  /**
   * Open edit dialog
   * ⚠️ QUAN TRỌNG: Code không được phép sửa khi edit
   */
  openEditDialog(risk: ResRiskDto): void {
    this.dialogMode = 'edit';
    this.selectedRisk = risk;
    this.formData = {
      objectTypeId: risk.objectTypeId || '',
      code: risk.code || '', // Readonly trong edit mode
      name: risk.name || '',
      description: risk.description || '',
      status: risk.status ?? ResRiskStatus.Active
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
          summary: this.localizationService.localize('Master::Error'),
          detail: this.localizationService.localize('Master::ResRisk:CodeRequired')
        });
        return false;
      }

      if (this.formData.code.length > 50) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: this.localizationService.localize('Master::ResRisk:CodeMaxLength')
        });
        return false;
      }

      if (!this.validateCode(this.formData.code)) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: this.localizationService.localize('Master::ResRisk:CodeInvalid')
        });
        return false;
      }
    }

    if (!this.formData.objectTypeId || this.formData.objectTypeId.trim() === '') {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResRisk:ObjectTypeIdRequired')
      });
      return false;
    }

    if (!this.formData.name || this.formData.name.trim() === '') {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResRisk:NameRequired')
      });
      return false;
    }

    if (this.formData.name.length > 250) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResRisk:NameMaxLength')
      });
      return false;
    }

    if (this.formData.description && this.formData.description.length > 500) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResRisk:DescriptionMaxLength')
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
    // e.g., "Master:ResRisk:CodeExists" -> "Master::ResRisk:CodeExists"
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
   * Create new risk
   */
  private create(): void {
    this.loading = true;

    const createDto: CreateResRiskDto = {
      objectTypeId: this.formData.objectTypeId.trim(),
      code: this.formData.code.trim(),
      name: this.formData.name.trim(),
      description: this.formData.description?.trim() || undefined,
      status: this.formData.status
    };

    this.riskService.create(createDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Master::Success'),
          detail: this.localizationService.localize('Master::ResRisk:CreatedSuccessfully')
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
   * Update existing risk
   * ⚠️ QUAN TRỌNG: Chỉ update ObjectTypeId, Name, Description và Status, không update Code
   */
  private update(): void {
    if (!this.selectedRisk || !this.selectedRisk.id) {
      return;
    }

    this.loading = true;

    const updateDto: UpdateResRiskDto = {
      objectTypeId: this.formData.objectTypeId.trim(),
      name: this.formData.name.trim(),
      description: this.formData.description?.trim() || undefined,
      status: this.formData.status
    };

    this.riskService.update(this.selectedRisk.id, updateDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Master::Success'),
          detail: this.localizationService.localize('Master::ResRisk:UpdatedSuccessfully')
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
   * Delete risk with confirmation
   */
  delete(risk: ResRiskDto): void {
    if (!risk.id) {
      return;
    }

    this.confirmationService.confirm({
      message: this.localizationService.localize('Master::ResRisk:DeleteConfirm'),
      header: this.localizationService.localize('Master::ResRisk:Delete'),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.loading = true;
        this.riskService.delete(risk.id!).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: this.localizationService.localize('Master::Success'),
              detail: this.localizationService.localize('Master::ResRisk:DeletedSuccessfully')
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
  private getEmptyForm(): RiskFormData {
    return {
      objectTypeId: '',
      code: '',
      name: '',
      description: '',
      status: ResRiskStatus.Active
    };
  }

  /**
   * Format status for display
   */
  formatStatus(status: ResRiskStatus | number | string | undefined | null): string {
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
      statusValue = status === ResRiskStatus.Active ? 0 : 1;
    }

    return statusValue === 0
      ? this.localizationService.localize('Master::ResRisk:Active')
      : this.localizationService.localize('Master::ResRisk:Deactive');
  }
}

