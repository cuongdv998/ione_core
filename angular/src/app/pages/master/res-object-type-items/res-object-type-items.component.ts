import { Component, HostListener, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpHeaders } from '@angular/common/http';
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
import { FileUploadModule } from 'primeng/fileupload';

import { VTable } from '@/shared/components/v-table/v-table';
import { TableColumn } from '@/shared/models/table-column.model';
import { TableAction } from '@/shared/models/table-action.model';
import { ResObjectTypeItemService } from '@/proxy/master/controllers/res-object-type-item.service';
import { ResObjectTypeItemDto, CreateResObjectTypeItemDto, UpdateResObjectTypeItemDto, GetResObjectTypeItemsInput } from '@/proxy/master/res-object-type-items/models';
import { ResObjectTypeItemStatus } from '@/proxy/res-object-type-items/res-object-type-item-status.enum';
import { ResObjectTypeItemSearchForm, ResObjectTypeItemFormData } from './res-object-type-items.models';
import { PermissionPipe } from '@/core/pipes/permission.pipe';
import { PermissionService } from '@/core/services/permission.service';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';
import { environment } from '@environments/environment';
import { ResObjectTypeService } from '@/proxy/master/controllers/res-object-type.service';
import { ResObjectItemTypeService } from '@/proxy/master/controllers/res-object-item-type.service';
import { ResUomService } from '@/proxy/master/controllers/res-uom.service';
import { ResObjectTypeDto } from '@/proxy/master/res-object-types/models';
import { ResObjectItemTypeDto } from '@/proxy/master/res-object-item-types/models';
import { ResUomDto } from '@/proxy/master/res-uoms/models';

@Component({
  selector: 'app-res-object-type-items',
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
    FileUploadModule,
    VTable,
    PermissionPipe,
    TranslatePipe
  ],
  templateUrl: './res-object-type-items.component.html',
  styleUrl: './res-object-type-items.component.scss',
  providers: [MessageService, ConfirmationService]
})
export class ResObjectTypeItemsComponent implements OnInit {
  // Permissions
  readonly PERMISSIONS = {
    CREATE: 'MasterResObjectTypeItem.Create',
    UPDATE: 'MasterResObjectTypeItem.Edit',
    DELETE: 'MasterResObjectTypeItem.Delete',
    VIEW: 'MasterResObjectTypeItem.View'
  };

  // Data
  resObjectTypeItems: ResObjectTypeItemDto[] = [];
  totalCount = 0;
  loading = true;
  pageSize = 10;

  // Dialog
  dialogVisible = false;
  dialogMode: 'create' | 'edit' | 'view' = 'create';
  formData: ResObjectTypeItemFormData = this.getEmptyForm();
  selectedResObjectTypeItem?: ResObjectTypeItemDto;

  // Import Dialog
  importDialogVisible = false;
  importResult: any = null;
  importing = false;

  // Search form
  searchForm: ResObjectTypeItemSearchForm = {
    code: null,
    name: null,
    objectTypeId: null,
    objectItemType: null,
    uomId: null,
    status: null
  };

  // Status options
  statusOptions: Array<{ label: string; value: ResObjectTypeItemStatus }> = [];

  // Lookup data
  objectTypes: Array<{ label: string; value: string }> = [];
  objectItemTypes: Array<{ label: string; value: string }> = [];
  uoms: Array<{ label: string; value: string }> = [];

  // Table
  columns: TableColumn[] = [];
  actions: TableAction<ResObjectTypeItemDto>[] = [];
  currentLazyLoadEvent?: TableLazyLoadEvent;

  constructor(
    private resObjectTypeItemService: ResObjectTypeItemService,
    private resObjectTypeService: ResObjectTypeService,
    private resObjectItemTypeService: ResObjectItemTypeService,
    private resUomService: ResUomService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private permissionService: PermissionService,
    private localizationService: LocalizationService,
    private http: HttpClient
  ) {
    this.initializeColumns();
    this.initializeActions();
    this.initializeStatusOptions();
    this.loadLookupData();
  }

  ngOnInit(): void {
    // Initial load will be triggered by the table's onLazyLoad event
  }

  /**
   * Load lookup data for dropdowns
   */
  private loadLookupData(): void {
    // Load Object Types
    this.resObjectTypeService.getList({ 
      skipCount: 0, 
      maxResultCount: 1000 
    }).subscribe({
      next: (result) => {
        this.objectTypes = result.items?.map(item => ({
          label: `${item.code} - ${item.name}`,
          value: item.id!
        })) || [];
      },
      error: () => {
        this.objectTypes = [];
      }
    });

    // Load Object Item Types
    this.resObjectItemTypeService.getList({ 
      skipCount: 0, 
      maxResultCount: 1000 
    }).subscribe({
      next: (result) => {
        this.objectItemTypes = result.items?.map(item => ({
          label: `${item.code} - ${item.name}`,
          value: item.id!
        })) || [];
      },
      error: () => {
        this.objectItemTypes = [];
      }
    });

    // Load UOMs
    this.resUomService.getList({ 
      skipCount: 0, 
      maxResultCount: 1000 
    }).subscribe({
      next: (result) => {
        this.uoms = result.items?.map(item => ({
          label: `${item.code} - ${item.name}`,
          value: item.id!
        })) || [];
      },
      error: () => {
        this.uoms = [];
      }
    });
  }

  /**
   * Initialize status options with localized labels
   */
  private initializeStatusOptions(): void {
    this.statusOptions = [
      { label: this.localizationService.localize('Master::ResObjectTypeItem:Active'), value: ResObjectTypeItemStatus.Active },
      { label: this.localizationService.localize('Master::ResObjectTypeItem:Deactive'), value: ResObjectTypeItemStatus.Deactive }
    ];
  }

  /**
   * Initialize table columns with localized headers
   */
  private initializeColumns(): void {
    this.columns = [
      {
        field: 'code',
        header: this.localizationService.localize('Master::ResObjectTypeItem:Code'),
        sortable: true,
        width: '150px'
      },
      {
        field: 'name',
        header: this.localizationService.localize('Master::ResObjectTypeItem:Name'),
        sortable: true,
        width: '250px'
      },
      {
        field: 'description',
        header: this.localizationService.localize('Master::ResObjectTypeItem:Description'),
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
        header: this.localizationService.localize('Master::ResObjectTypeItem:Status'),
        sortable: true,
        type: 'text',
        width: '120px',
        freeze: 'right',
        align: 'center',
        formatter: (value: any) => this.formatStatus(value),
        cellClass: (value: any) => {
          const statusValue = typeof value === 'number' ? value : 
            (value === ResObjectTypeItemStatus.Active ? 0 : 1);
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
    if (this.permissionService.isGranted(this.PERMISSIONS.VIEW)) {
      this.actions.push({
        label: this.localizationService.localize('Master::View'),
        icon: 'pi pi-eye',
        command: (row) => this.openViewDialog(row)
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
   * Load ResObjectTypeItems with lazy loading
   */
  loadData(event: TableLazyLoadEvent): void {
    this.currentLazyLoadEvent = event;
    this.loading = true;

    const input: GetResObjectTypeItemsInput = {
      skipCount: event.first || 0,
      maxResultCount: event.rows || 10,
      sorting: this.getSortingString(event),
      code: this.searchForm.code || undefined,
      name: this.searchForm.name || undefined,
      objectTypeId: this.searchForm.objectTypeId || undefined,
      objectItemType: this.searchForm.objectItemType || undefined,
      uomId: this.searchForm.uomId || undefined,
      status: this.searchForm.status ?? undefined
    };

    this.resObjectTypeItemService.getList(input).subscribe({
      next: (result) => {
        this.resObjectTypeItems = result.items || [];
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
    // Only trigger search if dialogs are not visible
    if (!this.dialogVisible && !this.importDialogVisible) {
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
      objectTypeId: null,
      objectItemType: null,
      uomId: null,
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
    this.selectedResObjectTypeItem = undefined;
    this.dialogVisible = true;
  }

  /**
   * Open edit dialog
   * ⚠️ QUAN TRỌNG: Code không được phép sửa khi edit
   */
  openEditDialog(item: ResObjectTypeItemDto): void {
    this.dialogMode = 'edit';
    this.selectedResObjectTypeItem = item;
    this.formData = {
      code: item.code || '', // Readonly trong edit mode
      name: item.name || '',
      objectTypeId: item.objectTypeId || '',
      objectItemType: item.objectItemType || null,
      uomId: item.uomId || '',
      description: item.description || '',
      status: item.status ?? ResObjectTypeItemStatus.Active
    };
    this.dialogVisible = true;
  }

  /**
   * Open view dialog
   */
  openViewDialog(item: ResObjectTypeItemDto): void {
    this.dialogMode = 'view';
    this.selectedResObjectTypeItem = item;
    this.formData = {
      code: item.code || '',
      name: item.name || '',
      objectTypeId: item.objectTypeId || '',
      objectItemType: item.objectItemType || null,
      uomId: item.uomId || '',
      description: item.description || '',
      status: item.status ?? ResObjectTypeItemStatus.Active
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
          detail: this.localizationService.localize('Master::ResObjectTypeItem:CodeRequired')
        });
        return false;
      }

      if (this.formData.code.length > 50) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: this.localizationService.localize('Master::ResObjectTypeItem:CodeMaxLength')
        });
        return false;
      }

      if (!this.validateCode(this.formData.code)) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: this.localizationService.localize('Master::ResObjectTypeItem:CodeInvalid')
        });
        return false;
      }
    }

    if (!this.formData.objectTypeId || this.formData.objectTypeId.trim() === '') {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResObjectTypeItem:ObjectTypeIdRequired')
      });
      return false;
    }

    if (!this.formData.name || this.formData.name.trim() === '') {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResObjectTypeItem:NameRequired')
      });
      return false;
    }

    if (this.formData.name.length > 250) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResObjectTypeItem:NameMaxLength')
      });
      return false;
    }

    if (!this.formData.uomId || this.formData.uomId.trim() === '') {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResObjectTypeItem:UomIdRequired')
      });
      return false;
    }

    if (this.formData.description && this.formData.description.length > 500) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResObjectTypeItem:DescriptionMaxLength')
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
    // e.g., "Master:ResObjectTypeItem:CodeExists" -> "Master::ResObjectTypeItem:CodeExists"
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
   * Create new ResObjectTypeItem
   */
  private create(): void {
    this.loading = true;

    const createDto: CreateResObjectTypeItemDto = {
      code: this.formData.code.trim(),
      name: this.formData.name.trim(),
      objectTypeId: this.formData.objectTypeId,
      objectItemType: this.formData.objectItemType || undefined,
      uomId: this.formData.uomId,
      description: this.formData.description?.trim() || undefined,
      status: this.formData.status
    };

    this.resObjectTypeItemService.create(createDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Master::Success'),
          detail: this.localizationService.localize('Master::ResObjectTypeItem:CreatedSuccessfully')
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
   * Update existing ResObjectTypeItem
   * ⚠️ QUAN TRỌNG: Chỉ update Name, ObjectTypeId, ObjectItemType, UomId, Description và Status, không update Code
   */
  private update(): void {
    if (!this.selectedResObjectTypeItem || !this.selectedResObjectTypeItem.id) {
      return;
    }

    this.loading = true;

    const updateDto: UpdateResObjectTypeItemDto = {
      name: this.formData.name.trim(),
      objectTypeId: this.formData.objectTypeId,
      objectItemType: this.formData.objectItemType || undefined,
      uomId: this.formData.uomId,
      description: this.formData.description?.trim() || undefined,
      status: this.formData.status
    };

    this.resObjectTypeItemService.update(this.selectedResObjectTypeItem.id, updateDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Master::Success'),
          detail: this.localizationService.localize('Master::ResObjectTypeItem:UpdatedSuccessfully')
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
   * Delete ResObjectTypeItem with confirmation
   */
  delete(item: ResObjectTypeItemDto): void {
    if (!item.id) {
      return;
    }

    this.confirmationService.confirm({
      message: this.localizationService.localize('Master::ResObjectTypeItem:DeleteConfirm'),
      header: this.localizationService.localize('Master::ResObjectTypeItem:Delete'),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.loading = true;
        this.resObjectTypeItemService.delete(item.id!).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: this.localizationService.localize('Master::Success'),
              detail: this.localizationService.localize('Master::ResObjectTypeItem:DeletedSuccessfully')
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
  private getEmptyForm(): ResObjectTypeItemFormData {
    return {
      code: '',
      name: '',
      objectTypeId: '',
      objectItemType: null,
      uomId: '',
      description: '',
      status: ResObjectTypeItemStatus.Active
    };
  }

  /**
   * Format status for display
   */
  formatStatus(status: ResObjectTypeItemStatus | number | string | undefined | null): string {
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
      statusValue = status === ResObjectTypeItemStatus.Active ? 0 : 1;
    }
    
    return statusValue === 0
      ? this.localizationService.localize('Master::ResObjectTypeItem:Active')
      : this.localizationService.localize('Master::ResObjectTypeItem:Deactive');
  }

  /**
   * Open import dialog
   */
  openImportDialog(): void {
    this.importDialogVisible = true;
    this.importResult = null;
  }

  /**
   * Handle file upload for Excel import
   */
  onFileSelect(event: any): void {
    const file = event.files?.[0];
    if (!file) {
      return;
    }

    // Validate file type
    const validExtensions = ['.xlsx', '.xls'];
    const fileExtension = file.name.substring(file.name.lastIndexOf('.')).toLowerCase();
    if (!validExtensions.includes(fileExtension)) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResObjectTypeItem:InvalidFileType')
      });
      return;
    }

    this.importing = true;

    this.resObjectTypeItemService.importExcel(file).subscribe({
      next: (result: any) => {
        this.importResult = result;
        this.importing = false;
        
        if (result.errorCount === 0) {
          this.messageService.add({
            severity: 'success',
            summary: this.localizationService.localize('Master::Success'),
            detail: `${this.localizationService.localize('Master::ResObjectTypeItem:ImportSuccess')} (${result.successCount} ${this.localizationService.localize('Master::ResObjectTypeItem:Records')})`
          });
          this.search(); // Refresh the list
        } else {
          this.messageService.add({
            severity: 'warn',
            summary: this.localizationService.localize('Master::Warning'),
            detail: `${this.localizationService.localize('Master::ResObjectTypeItem:ImportPartialSuccess')} - ${this.localizationService.localize('Master::ResObjectTypeItem:SuccessCount')}: ${result.successCount}, ${this.localizationService.localize('Master::ResObjectTypeItem:ErrorCount')}: ${result.errorCount}`
          });
        }
      },
      error: (error) => {
        this.importing = false;
        let errorMessage = error.error?.error?.message || 
                           error.error?.error?.details || 
                           this.localizationService.localize('Master::InternalServerErrorMessage');
        errorMessage = this.translateErrorMessage(errorMessage);
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: errorMessage
        });
      }
    });
  }

  /**
   * Download Excel template
   */
  downloadTemplate(): void {
    this.loading = true;

    // Get API base URL from environment configuration
    const apiConfig = environment.apis['Master'] || environment.apis.default;
    const apiUrl = apiConfig?.url || 'https://localhost:44360';
    const apiEndpoint = `${apiUrl}/api/master/object-type-items/export-template`;

    // Use HttpClient to download the file as blob
    // ABP's HTTP interceptor will automatically add authentication headers
    this.http.get(apiEndpoint, {
      responseType: 'blob'
    }).subscribe({
      next: (blob: Blob) => {
        // Create a blob URL and trigger download
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        
        // Generate filename with timestamp
        const timestamp = new Date().toISOString().replace(/[:.]/g, '-').slice(0, -5);
        link.download = `ResObjectTypeItem_Template_${timestamp}.xlsx`;
        
        // Trigger download
        document.body.appendChild(link);
        link.click();
        
        // Cleanup
        setTimeout(() => {
          document.body.removeChild(link);
          window.URL.revokeObjectURL(url);
        }, 100);
        
        this.loading = false;
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Master::Success'),
          detail: this.localizationService.localize('Master::ResObjectTypeItem:TemplateDownloaded')
        });
      },
      error: (error) => {
        this.loading = false;
        
        // Try to parse error message from blob if response is blob
        if (error.error instanceof Blob) {
          const reader = new FileReader();
          reader.onload = () => {
            try {
              const errorObj = JSON.parse(reader.result as string);
              const errorMessage = errorObj?.error?.message || 
                                 errorObj?.error?.details || 
                                 this.localizationService.localize('Master::InternalServerErrorMessage');
              this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('Master::Error'),
                detail: this.translateErrorMessage(errorMessage)
              });
            } catch {
              this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('Master::Error'),
                detail: this.localizationService.localize('Master::InternalServerErrorMessage')
              });
            }
          };
          reader.readAsText(error.error);
        } else {
          let errorMessage = error.error?.error?.message || 
                           error.error?.error?.details || 
                           this.localizationService.localize('Master::InternalServerErrorMessage');
          errorMessage = this.translateErrorMessage(errorMessage);
          this.messageService.add({
            severity: 'error',
            summary: this.localizationService.localize('Master::Error'),
            detail: errorMessage
          });
        }
      }
    });
  }
}
