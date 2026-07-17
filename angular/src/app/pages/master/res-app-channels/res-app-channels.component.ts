import { Component, OnInit, HostListener } from '@angular/core';
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
import { ResAppChannelService } from '@/proxy/master/controllers/res-app-channel.service';
import { ResAppChannelDto, CreateResAppChannelDto, UpdateResAppChannelDto, GetResAppChannelsInput } from '@/proxy/master/res-app-channels/models';
import { ResAppChannelStatus } from '@/proxy/res-app-channels/res-app-channel-status.enum';
import { ResAppChannelType } from '@/proxy/res-app-channels/res-app-channel-type.enum';
import { AppChannelSearchForm, AppChannelFormData } from './res-app-channels.models';
import { PermissionPipe } from '@/core/pipes/permission.pipe';
import { PermissionService } from '@/core/services/permission.service';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';

@Component({
  selector: 'app-res-app-channels',
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
  templateUrl: './res-app-channels.component.html',
  styleUrl: './res-app-channels.component.scss',
  providers: [MessageService, ConfirmationService]
})
export class ResAppChannelsComponent implements OnInit {
  // Permissions
  readonly PERMISSIONS = {
    CREATE: 'MasterResAppChannel.Create',
    UPDATE: 'MasterResAppChannel.Edit',
    DELETE: 'MasterResAppChannel.Delete',
    VIEW: 'MasterResAppChannel.View'
  };

  // Data
  appChannels: ResAppChannelDto[] = [];
  totalCount = 0;
  loading = true;
  pageSize = 10;

  // Dialog
  dialogVisible = false;
  dialogMode: 'create' | 'edit' = 'create';
  formData: AppChannelFormData = this.getEmptyForm();
  selectedAppChannel?: ResAppChannelDto;

  // Search form
  searchForm: AppChannelSearchForm = {
    code: null,
    name: null,
    status: null,
    type: null
  };

  // Status and Type options
  statusOptions: Array<{ label: string; value: ResAppChannelStatus }> = [];
  typeOptions: Array<{ label: string; value: ResAppChannelType }> = [];

  // Table
  columns: TableColumn[] = [];
  actions: TableAction<ResAppChannelDto>[] = [];
  currentLazyLoadEvent?: TableLazyLoadEvent;

  constructor(
    private appChannelService: ResAppChannelService,
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
      { label: this.localizationService.localize('Master::ResAppChannel:Active'), value: ResAppChannelStatus.Active },
      { label: this.localizationService.localize('Master::ResAppChannel:Deactive'), value: ResAppChannelStatus.Deactive }
    ];
  }

  /**
   * Initialize type options with localized labels
   */
  private initializeTypeOptions(): void {
    this.typeOptions = [
      { label: this.localizationService.localize('Master::ResAppChannel:Email'), value: ResAppChannelType.Email },
      { label: this.localizationService.localize('Master::ResAppChannel:Mobile'), value: ResAppChannelType.Mobile },
      { label: this.localizationService.localize('Master::ResAppChannel:Web'), value: ResAppChannelType.Web },
      { label: this.localizationService.localize('Master::ResAppChannel:Phone'), value: ResAppChannelType.Phone },
      { label: this.localizationService.localize('Master::ResAppChannel:Other'), value: ResAppChannelType.Other }
    ];
  }

  /**
   * Initialize table columns with localized headers
   */
  private initializeColumns(): void {
    this.columns = [
      {
        field: 'code',
        header: this.localizationService.localize('Master::ResAppChannel:Code'),
        sortable: true,
        width: '150px'
      },
      {
        field: 'name',
        header: this.localizationService.localize('Master::ResAppChannel:Name'),
        sortable: true,
        width: '250px'
      },
      {
        field: 'description',
        header: this.localizationService.localize('Master::ResAppChannel:Description'),
        sortable: false,
        width: '250px'
      },
      {
        field: 'type',
        header: this.localizationService.localize('Master::ResAppChannel:Type'),
        sortable: true,
        type: 'text',
        width: '120px',
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
        header: this.localizationService.localize('Master::ResAppChannel:Status'),
        sortable: true,
        type: 'text',
        width: '120px',
        freeze: 'right',
        align: 'center',
        formatter: (value: any) => this.formatStatus(value),
        cellClass: (value: any) => {
          const statusValue = typeof value === 'number' ? value :
            (value === ResAppChannelStatus.Active ? 0 : 1);
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
   * Load app channels with lazy loading
   */
  loadData(event: TableLazyLoadEvent): void {
    this.currentLazyLoadEvent = event;
    this.loading = true;

    const input: GetResAppChannelsInput = {
      skipCount: event.first || 0,
      maxResultCount: event.rows || 10,
      sorting: this.getSortingString(event),
      code: this.searchForm.code || undefined,
      name: this.searchForm.name || undefined,
      status: this.searchForm.status ?? undefined,
      type: this.searchForm.type ?? undefined
    };

    this.appChannelService.getList(input).subscribe({
      next: (result) => {
        this.appChannels = result.items || [];
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
      status: null,
      type: null
    };
    this.search();
  }

  /**
   * Open create dialog
   */
  openCreateDialog(): void {
    this.dialogMode = 'create';
    this.formData = this.getEmptyForm();
    this.selectedAppChannel = undefined;
    this.dialogVisible = true;
  }

  /**
   * Open edit dialog
   * ⚠️ QUAN TRỌNG: Code không được phép sửa khi edit
   */
  openEditDialog(appChannel: ResAppChannelDto): void {
    this.dialogMode = 'edit';
    this.selectedAppChannel = appChannel;
    this.formData = {
      code: appChannel.code || '', // Readonly trong edit mode
      name: appChannel.name || '',
      description: appChannel.description || '',
      status: appChannel.status ?? ResAppChannelStatus.Active,
      type: appChannel.type ?? null
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
          detail: this.localizationService.localize('Master::ResAppChannel:CodeRequired')
        });
        return false;
      }

      if (this.formData.code.length > 50) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: this.localizationService.localize('Master::ResAppChannel:CodeMaxLength')
        });
        return false;
      }

      if (!this.validateCode(this.formData.code)) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: this.localizationService.localize('Master::ResAppChannel:CodeInvalid')
        });
        return false;
      }
    }

    if (!this.formData.name || this.formData.name.trim() === '') {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResAppChannel:NameRequired')
      });
      return false;
    }

    if (this.formData.type === null) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResAppChannel:TypeRequired')
      });
      return false;
    }

    if (this.formData.name.length > 50) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResAppChannel:NameMaxLength')
      });
      return false;
    }

    if (this.formData.description && this.formData.description.length > 500) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResAppChannel:DescriptionMaxLength')
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
    // e.g., "Master:ResAppChannel:CodeExists" -> "Master::ResAppChannel:CodeExists"
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
   * Create new app channel
   */
  private create(): void {
    this.loading = true;

    const createDto: CreateResAppChannelDto = {
      code: this.formData.code.trim(),
      name: this.formData.name.trim(),
      description: this.formData.description?.trim() || undefined,
      status: this.formData.status,
      type: this.formData.type || undefined
    };

    this.appChannelService.create(createDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Master::Success'),
          detail: this.localizationService.localize('Master::ResAppChannel:CreatedSuccessfully')
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
   * Update existing app channel
   * ⚠️ QUAN TRỌNG: Chỉ update Name, Description và Status, không update Code
   */
  private update(): void {
    if (!this.selectedAppChannel || !this.selectedAppChannel.id) {
      return;
    }

    this.loading = true;

    const updateDto: UpdateResAppChannelDto = {
      name: this.formData.name.trim(),
      description: this.formData.description?.trim() || undefined,
      status: this.formData.status,
      type: this.formData.type ?? undefined
    };

    this.appChannelService.update(this.selectedAppChannel.id, updateDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Master::Success'),
          detail: this.localizationService.localize('Master::ResAppChannel:UpdatedSuccessfully')
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
   * Delete app channel with confirmation
   */
  delete(appChannel: ResAppChannelDto): void {
    if (!appChannel.id) {
      return;
    }

    this.confirmationService.confirm({
      message: this.localizationService.localize('Master::ResAppChannel:DeleteConfirm'),
      header: this.localizationService.localize('Master::ResAppChannel:Delete'),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.loading = true;
        this.appChannelService.delete(appChannel.id!).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: this.localizationService.localize('Master::Success'),
              detail: this.localizationService.localize('Master::ResAppChannel:DeletedSuccessfully')
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
  private getEmptyForm(): AppChannelFormData {
    return {
      code: '',
      name: '',
      description: '',
      status: ResAppChannelStatus.Active,
      type: null
    };
  }

  /**
   * Format status for display
   */
  formatStatus(status: ResAppChannelStatus | number | string | undefined | null): string {
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
      statusValue = status === ResAppChannelStatus.Active ? 0 : 1;
    }

    return statusValue === 0
      ? this.localizationService.localize('Master::ResAppChannel:Active')
      : this.localizationService.localize('Master::ResAppChannel:Deactive');
  }

  /**
   * Format type for display
   */
  formatType(type: ResAppChannelType | number | string | undefined | null): string {
    if (type === undefined || type === null) {
      return '';
    }

    let typeValue: number;

    if (typeof type === 'number') {
      typeValue = type;
    } else {
      // In case it's a string, we might need a map
      typeValue = Number(type);
    }

    switch (typeValue) {
      case ResAppChannelType.Email: return this.localizationService.localize('Master::ResAppChannel:Email');
      case ResAppChannelType.Mobile: return this.localizationService.localize('Master::ResAppChannel:Mobile');
      case ResAppChannelType.Web: return this.localizationService.localize('Master::ResAppChannel:Web');
      case ResAppChannelType.Phone: return this.localizationService.localize('Master::ResAppChannel:Phone');
      case ResAppChannelType.Other: return this.localizationService.localize('Master::ResAppChannel:Other');
      default: return '';
    }
  }
}
