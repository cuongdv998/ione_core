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
import { ResDocumentTypeService } from '@/proxy/master/controllers/res-document-type.service';
import { ResDocumentTypeDto, CreateResDocumentTypeDto, UpdateResDocumentTypeDto, GetResDocumentTypesInput } from '@/proxy/master/res-document-types/models';
import { ResDocumentTypeStatus } from '@/proxy/res-document-types/res-document-type-status.enum';
import { DocumentTypeSearchForm, DocumentTypeFormData } from './document-types.models';
import { PermissionPipe } from '@/core/pipes/permission.pipe';
import { PermissionService } from '@/core/services/permission.service';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';

@Component({
  selector: 'app-document-types',
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
  templateUrl: './document-types.component.html',
  styleUrl: './document-types.component.scss',
  providers: [MessageService, ConfirmationService]
})
export class DocumentTypesComponent implements OnInit {
  // Permissions
  readonly PERMISSIONS = {
    CREATE: 'MasterResDocumentType.Create',
    UPDATE: 'MasterResDocumentType.Edit',
    DELETE: 'MasterResDocumentType.Delete',
    VIEW: 'MasterResDocumentType.View'
  };

  // Data
  documentTypes: ResDocumentTypeDto[] = [];
  totalCount = 0;
  loading = true;
  pageSize = 10;

  // Dialog
  dialogVisible = false;
  dialogMode: 'create' | 'edit' = 'create';
  formData: DocumentTypeFormData = this.getEmptyForm();
  selectedDocumentType?: ResDocumentTypeDto;

  // Search form
  searchForm: DocumentTypeSearchForm = {
    code: null,
    name: null,
    status: null
  };

  // Status options
  statusOptions: Array<{ label: string; value: ResDocumentTypeStatus }> = [];

  // Table
  columns: TableColumn[] = [];
  actions: TableAction<ResDocumentTypeDto>[] = [];
  currentLazyLoadEvent?: TableLazyLoadEvent;

  constructor(
    private documentTypeService: ResDocumentTypeService,
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
      { label: this.localizationService.localize('Master::ResDocumentType:Active'), value: ResDocumentTypeStatus.Active },
      { label: this.localizationService.localize('Master::ResDocumentType:Deactive'), value: ResDocumentTypeStatus.Deactive }
    ];
  }

  /**
   * Initialize table columns with localized headers
   */
  private initializeColumns(): void {
    this.columns = [
      {
        field: 'code',
        header: this.localizationService.localize('Master::ResDocumentType:Code'),
        sortable: true,
        width: '150px'
      },
      {
        field: 'name',
        header: this.localizationService.localize('Master::ResDocumentType:Name'),
        sortable: true,
        width: '250px'
      },
      {
        field: 'documentGroupCode',
        header: this.localizationService.localize('Master::ResDocumentType:DocumentGroupCode'),
        sortable: false,
        width: '160px'
      },
      {
        field: 'description',
        header: this.localizationService.localize('Master::ResDocumentType:Description'),
        sortable: false,
        width: '300px'
      },
      {
        field: 'bucket',
        header: this.localizationService.localize('Master::ResDocumentType:Bucket'),
        sortable: false,
        width: '150px'
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
        header: this.localizationService.localize('Master::ResDocumentType:Status'),
        sortable: true,
        type: 'text',
        width: '120px',
        freeze: 'right',
        align: 'center',
        formatter: (value: any) => this.formatStatus(value),
        cellClass: (value: any) => {
          const statusValue = typeof value === 'number' ? value : (value === ResDocumentTypeStatus.Active ? 0 : 1);
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
   * Load document types with lazy loading
   */
  loadData(event: TableLazyLoadEvent): void {
    this.currentLazyLoadEvent = event;
    this.loading = true;

    const input: GetResDocumentTypesInput = {
      skipCount: event.first || 0,
      maxResultCount: event.rows || 10,
      sorting: this.getSortingString(event),
      code: this.searchForm.code || undefined,
      name: this.searchForm.name || undefined,
      status: this.searchForm.status ?? undefined
    };

    this.documentTypeService.getList(input).subscribe({
      next: (result) => {
        this.documentTypes = result.items || [];
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
    this.selectedDocumentType = undefined;
    this.dialogVisible = true;
  }

  /**
   * Open edit dialog
   * ⚠️ QUAN TRỌNG: Code không được phép sửa khi edit
   */
  openEditDialog(documentType: ResDocumentTypeDto): void {
    this.dialogMode = 'edit';
    this.selectedDocumentType = documentType;
    this.formData = {
      code: documentType.code || '', // Readonly trong edit mode
      name: documentType.name || '',
      description: documentType.description || null,
      status: documentType.status ?? ResDocumentTypeStatus.Active,
      bucket: documentType.bucket || null,
      documentGroupCode: documentType.documentGroupCode || null
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
          detail: this.localizationService.localize('Master::ResDocumentType:CodeRequired')
        });
        return false;
      }

      if (this.formData.code.length > 25) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: this.localizationService.localize('Master::ResDocumentType:CodeMaxLength')
        });
        return false;
      }

      if (!this.validateCode(this.formData.code)) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: this.localizationService.localize('Master::ResDocumentType:CodeInvalid')
        });
        return false;
      }
    }

    if (!this.formData.name || this.formData.name.trim() === '') {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResDocumentType:NameRequired')
      });
      return false;
    }

    if (this.formData.name.length > 250) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResDocumentType:NameMaxLength')
      });
      return false;
    }

    if (this.formData.description && this.formData.description.length > 500) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResDocumentType:DescriptionMaxLength')
      });
      return false;
    }

    if (this.formData.documentGroupCode && this.formData.documentGroupCode.length > 50) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResDocumentType:DocumentGroupCodeMaxLength')
      });
      return false;
    }

    // Validate bucket (required when creating)
    if (this.dialogMode === 'create') {
      if (!this.formData.bucket || this.formData.bucket.trim() === '') {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: this.localizationService.localize('Master::ResDocumentType:BucketRequired') ||
            'Bucket is required.'
        });
        return false;
      }

      if (this.formData.bucket.length > 50) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: this.localizationService.localize('Master::ResDocumentType:BucketMaxLength')
        });
        return false;
      }

      // Validate bucket format
      if (!this.validateBucket(this.formData.bucket.trim())) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: this.localizationService.localize('Master::ResDocumentType:BucketInvalid') ||
            'Bucket name is invalid. Bucket names must be 3-63 characters, lowercase, and can only contain lowercase letters, numbers, dots (.), and hyphens (-). Bucket names cannot start or end with a dot or hyphen, and cannot contain consecutive dots.'
        });
        return false;
      }
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
   * Validate MinIO bucket name format
   * MinIO bucket name rules:
   * - 3-63 characters
   * - Lowercase letters, numbers, dots (.), and hyphens (-) only
   * - Cannot start or end with dot or hyphen
   * - Cannot contain consecutive dots
   * - Cannot be an IP address format
   */
  validateBucket(bucket: string): boolean {
    if (!bucket || bucket.trim() === '') {
      return false;
    }

    const bucketName = bucket.trim().toLowerCase();

    // Length check: 3-63 characters
    if (bucketName.length < 3 || bucketName.length > 63) {
      return false;
    }

    // Must be lowercase
    if (bucketName !== bucketName.toLowerCase()) {
      return false;
    }

    // Cannot start or end with dot or hyphen
    if (bucketName[0] === '.' || bucketName[0] === '-' ||
        bucketName[bucketName.length - 1] === '.' || bucketName[bucketName.length - 1] === '-') {
      return false;
    }

    // Cannot contain consecutive dots
    if (bucketName.includes('..')) {
      return false;
    }

    // Can only contain lowercase letters, numbers, dots, and hyphens
    if (!/^[a-z0-9][a-z0-9\-\.]{1,61}[a-z0-9]$/.test(bucketName)) {
      return false;
    }

    // Cannot be an IP address format (e.g., 192.168.1.1)
    if (/^\d+\.\d+\.\d+\.\d+$/.test(bucketName)) {
      return false;
    }

    return true;
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
   * Convert bucket to lowercase on input
   */
  onBucketInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    const value = input.value.toLowerCase();
    input.value = value;
    this.formData.bucket = value;
  }

  /**
   * Create new document type
   */
  private create(): void {
    this.loading = true;

    const createDto: CreateResDocumentTypeDto = {
      code: this.formData.code.trim(),
      name: this.formData.name.trim(),
      description: this.formData.description?.trim() || undefined,
      status: this.formData.status,
      bucket: this.formData.bucket!.trim().toLowerCase(), // Bucket is required and validated in validateForm()
      documentGroupCode: this.formData.documentGroupCode?.trim() || undefined
    };

    this.documentTypeService.create(createDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Master::Success'),
          detail: this.localizationService.localize('Master::ResDocumentType:CreatedSuccessfully')
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
   * Update existing document type
   * ⚠️ QUAN TRỌNG: Chỉ update Name, Description, Status, Bucket - không update Code
   */
  private update(): void {
    if (!this.selectedDocumentType || !this.selectedDocumentType.id) {
      return;
    }

    this.loading = true;

    const updateDto: UpdateResDocumentTypeDto = {
      name: this.formData.name.trim(),
      description: this.formData.description?.trim() || undefined,
      status: this.formData.status,
      bucket: this.formData.bucket?.trim() || undefined,
      documentGroupCode: this.formData.documentGroupCode?.trim() || undefined
    };

    this.documentTypeService.update(this.selectedDocumentType.id, updateDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Master::Success'),
          detail: this.localizationService.localize('Master::ResDocumentType:UpdatedSuccessfully')
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
   * Delete document type with confirmation
   * ⚠️ QUAN TRỌNG: Soft delete - set Status to Deactive
   */
  delete(documentType: ResDocumentTypeDto): void {
    if (!documentType.id) {
      return;
    }

    this.confirmationService.confirm({
      message: this.localizationService.localize('Master::ResDocumentType:DeleteConfirm'),
      header: this.localizationService.localize('Master::ResDocumentType:Delete'),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.loading = true;
        this.documentTypeService.delete(documentType.id!).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: this.localizationService.localize('Master::Success'),
              detail: this.localizationService.localize('Master::ResDocumentType:DeletedSuccessfully')
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
  private getEmptyForm(): DocumentTypeFormData {
    return {
      code: '',
      name: '',
      description: null,
      status: ResDocumentTypeStatus.Active,
      bucket: null,
      documentGroupCode: null
    };
  }

  /**
   * Format status for display
   */
  formatStatus(status: ResDocumentTypeStatus | number | string | undefined | null): string {
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
      statusValue = status === ResDocumentTypeStatus.Active ? 0 : 1;
    }

    return statusValue === 0
      ? this.localizationService.localize('Master::ResDocumentType:Active')
      : this.localizationService.localize('Master::ResDocumentType:Deactive');
  }
}

