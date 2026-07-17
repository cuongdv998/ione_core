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
import { ResObjectTypeService } from '@/proxy/master/controllers/res-object-type.service';
import { ResObjectTypeDto, CreateResObjectTypeDto, UpdateResObjectTypeDto, GetResObjectTypesInput } from '@/proxy/master/res-object-types/models';
import { ResObjectTypeStatus } from '@/proxy/res-object-types/res-object-type-status.enum';
import { ResObjectGroup } from '@/proxy/res-object-types/res-object-group.enum';
import { ObjectTypeSearchForm, ObjectTypeFormData } from './object-types.models';
import { PermissionPipe } from '@/core/pipes/permission.pipe';
import { PermissionService } from '@/core/services/permission.service';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';

@Component({
  selector: 'app-object-types',
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
  templateUrl: './object-types.component.html',
  styleUrl: './object-types.component.scss',
  providers: [MessageService, ConfirmationService]
})
export class ObjectTypesComponent implements OnInit {
  // Permissions
  readonly PERMISSIONS = {
    CREATE: 'MasterResObjectType.Create',
    UPDATE: 'MasterResObjectType.Edit',
    DELETE: 'MasterResObjectType.Delete',
    VIEW: 'MasterResObjectType.View'
  };

  // Data
  objectTypes: ResObjectTypeDto[] = [];
  totalCount = 0;
  loading = true;
  pageSize = 10;

  // Dialog
  dialogVisible = false;
  dialogMode: 'create' | 'edit' = 'create';
  formData: ObjectTypeFormData = this.getEmptyForm();
  selectedObjectType?: ResObjectTypeDto;

  // Search form
  searchForm: ObjectTypeSearchForm = {
    code: null,
    name: null,
    objectGroup: null,
    status: null
  };

  // Status options
  statusOptions: Array<{ label: string; value: ResObjectTypeStatus }> = [];

  // ObjectGroup options
  objectGroupOptions: Array<{ label: string; value: ResObjectGroup | null }> = [];

  // Table
  columns: TableColumn[] = [];
  actions: TableAction<ResObjectTypeDto>[] = [];
  currentLazyLoadEvent?: TableLazyLoadEvent;

  constructor(
    private objectTypeService: ResObjectTypeService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private permissionService: PermissionService,
    private localizationService: LocalizationService
  ) {
    this.initializeColumns();
    this.initializeActions();
    this.initializeStatusOptions();
    this.initializeObjectGroupOptions();
  }

  ngOnInit(): void {
    // Initial load will be triggered by the table's onLazyLoad event
  }

  /**
   * Initialize status options with localized labels
   */
  private initializeStatusOptions(): void {
    this.statusOptions = [
      { label: this.localizationService.localize('Master::ResObjectType:Active'), value: ResObjectTypeStatus.Active },
      { label: this.localizationService.localize('Master::ResObjectType:Deactive'), value: ResObjectTypeStatus.Deactive }
    ];
  }

  /**
   * Initialize object group options
   */
  private initializeObjectGroupOptions(): void {
    this.objectGroupOptions = [
      { label: this.localizationService.localize('Master::ResObjectType:ObjectGroupPerson'), value: ResObjectGroup.Person },
      { label: this.localizationService.localize('Master::ResObjectType:ObjectGroupProperty'), value: ResObjectGroup.Property }
    ];
  }

  /**
   * Convert string (M/P) to ResObjectGroup enum
   */
  private stringToObjectGroup(value: string | undefined | null): ResObjectGroup | null {
    if (!value) {
      return null;
    }
    if (value === 'M') {
      return ResObjectGroup.Person;
    }
    if (value === 'P') {
      return ResObjectGroup.Property;
    }
    return null;
  }

  /**
   * Convert ResObjectGroup enum to string (M/P)
   */
  private objectGroupToString(value: ResObjectGroup | null | undefined): string | undefined {
    if (value === null || value === undefined) {
      return undefined;
    }
    if (value === ResObjectGroup.Person) {
      return 'M';
    }
    if (value === ResObjectGroup.Property) {
      return 'P';
    }
    return undefined;
  }

  /**
   * Initialize table columns with localized headers
   */
  private initializeColumns(): void {
    this.columns = [
      {
        field: 'code',
        header: this.localizationService.localize('Master::ResObjectType:Code'),
        sortable: true,
        width: '150px'
      },
      {
        field: 'name',
        header: this.localizationService.localize('Master::ResObjectType:Name'),
        sortable: true,
        width: '250px'
      },
      {
        field: 'objectGroup',
        header: this.localizationService.localize('Master::ResObjectType:ObjectGroup'),
        sortable: true,
        width: '150px',
        formatter: (value: any) => this.formatObjectGroup(value)
      },
      {
        field: 'description',
        header: this.localizationService.localize('Master::ResObjectType:Description'),
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
        header: this.localizationService.localize('Master::ResObjectType:Status'),
        sortable: true,
        type: 'text',
        width: '120px',
        freeze: 'right',
        align: 'center',
        formatter: (value: any) => this.formatStatus(value),
        cellClass: (value: any) => {
          const statusValue = typeof value === 'number' ? value : 
            (value === ResObjectTypeStatus.Active ? 0 : 1);
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
   * Load object types with lazy loading
   */
  loadData(event: TableLazyLoadEvent): void {
    this.currentLazyLoadEvent = event;
    this.loading = true;

    const input: GetResObjectTypesInput = {
      skipCount: event.first || 0,
      maxResultCount: event.rows || 10,
      sorting: this.getSortingString(event),
      code: this.searchForm.code || undefined,
      name: this.searchForm.name || undefined,
      objectGroup: this.searchForm.objectGroup ?? undefined,
      status: this.searchForm.status ?? undefined
    };

    this.objectTypeService.getList(input).subscribe({
      next: (result) => {
        this.objectTypes = result.items || [];
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
      objectGroup: null,
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
    this.selectedObjectType = undefined;
    this.dialogVisible = true;
  }

  /**
   * Open edit dialog
   * ⚠️ QUAN TRỌNG: Code không được phép sửa khi edit
   */
  openEditDialog(objectType: ResObjectTypeDto): void {
    this.dialogMode = 'edit';
    this.selectedObjectType = objectType;
    this.formData = {
      code: objectType.code || '', // Readonly trong edit mode
      name: objectType.name || '',
      objectGroup: objectType.objectGroup ?? null,
      description: objectType.description || '',
      status: objectType.status ?? ResObjectTypeStatus.Active
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
          detail: this.localizationService.localize('Master::ResObjectType:CodeRequired')
        });
        return false;
      }

      if (this.formData.code.length > 50) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: this.localizationService.localize('Master::ResObjectType:CodeMaxLength')
        });
        return false;
      }

      if (!this.validateCode(this.formData.code)) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: this.localizationService.localize('Master::ResObjectType:CodeInvalid')
        });
        return false;
      }
    }

    if (!this.formData.name || this.formData.name.trim() === '') {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResObjectType:NameRequired')
      });
      return false;
    }

    if (this.formData.name.length > 250) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResObjectType:NameMaxLength')
      });
      return false;
    }


    if (this.formData.description && this.formData.description.length > 500) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResObjectType:DescriptionMaxLength')
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
    // e.g., "Master:ResObjectType:CodeExists" -> "Master::ResObjectType:CodeExists"
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
   * Create new object type
   */
  private create(): void {
    this.loading = true;

    const createDto: CreateResObjectTypeDto = {
      code: this.formData.code.trim(),
      name: this.formData.name.trim(),
      objectGroup: this.formData.objectGroup ?? undefined,
      description: this.formData.description?.trim() || undefined,
      status: this.formData.status
    };

    this.objectTypeService.create(createDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Master::Success'),
          detail: this.localizationService.localize('Master::ResObjectType:CreatedSuccessfully')
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
   * Update existing object type
   * ⚠️ QUAN TRỌNG: Chỉ update Name, ObjectGroup, Description và Status, không update Code
   */
  private update(): void {
    if (!this.selectedObjectType || !this.selectedObjectType.id) {
      return;
    }

    this.loading = true;

    const updateDto: UpdateResObjectTypeDto = {
      name: this.formData.name.trim(),
      objectGroup: this.formData.objectGroup ?? undefined,
      description: this.formData.description?.trim() || undefined,
      status: this.formData.status
    };

    this.objectTypeService.update(this.selectedObjectType.id, updateDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Master::Success'),
          detail: this.localizationService.localize('Master::ResObjectType:UpdatedSuccessfully')
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
   * Delete object type with confirmation
   */
  delete(objectType: ResObjectTypeDto): void {
    if (!objectType.id) {
      return;
    }

    this.confirmationService.confirm({
      message: this.localizationService.localize('Master::ResObjectType:DeleteConfirm'),
      header: this.localizationService.localize('Master::ResObjectType:Delete'),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.loading = true;
        this.objectTypeService.delete(objectType.id!).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: this.localizationService.localize('Master::Success'),
              detail: this.localizationService.localize('Master::ResObjectType:DeletedSuccessfully')
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
  private getEmptyForm(): ObjectTypeFormData {
    return {
      code: '',
      name: '',
      objectGroup: null,
      description: '',
      status: ResObjectTypeStatus.Active
    };
  }

  /**
   * Format status for display
   */
  formatStatus(status: ResObjectTypeStatus | number | string | undefined | null): string {
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
      statusValue = status === ResObjectTypeStatus.Active ? 0 : 1;
    }
    
    return statusValue === 0
      ? this.localizationService.localize('Master::ResObjectType:Active')
      : this.localizationService.localize('Master::ResObjectType:Deactive');
  }

  /**
   * Format object group for display
   */
  formatObjectGroup(objectGroup: ResObjectGroup | undefined | null): string {
    if (objectGroup === null || objectGroup === undefined) {
      return '';
    }
    
    if (objectGroup === ResObjectGroup.Person) {
      return this.localizationService.localize('Master::ResObjectType:ObjectGroupPerson');
    }
    if (objectGroup === ResObjectGroup.Property) {
      return this.localizationService.localize('Master::ResObjectType:ObjectGroupProperty');
    }
    
    return String(objectGroup);
  }

}

