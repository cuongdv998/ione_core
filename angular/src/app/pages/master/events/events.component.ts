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
import { InputNumberModule } from 'primeng/inputnumber';
import { TooltipModule } from 'primeng/tooltip';

import { VTable } from '@/shared/components/v-table/v-table';
import { TableColumn } from '@/shared/models/table-column.model';
import { TableAction } from '@/shared/models/table-action.model';
import { ResEventService } from '@/proxy/master/controllers/res-event.service';
import { ResEventNotifyTemplateService } from '@/proxy/master/controllers/res-event-notify-template.service';
import { ResAppChannelService } from '@/proxy/master/controllers/res-app-channel.service';
import {
  ResEventDto,
  CreateResEventDto,
  UpdateResEventDto,
  GetResEventsInput,
  ResEventNotifyTemplateDto,
  CreateResEventNotifyTemplateDto,
  UpdateResEventNotifyTemplateDto,
} from '@/proxy/master/res-events/models';
import { ResAppChannelDto } from '@/proxy/master/res-app-channels/models';
import { ResEventStatus } from '@/proxy/res-events/res-event-status.enum';
import { EventSearchForm, EventFormData, EventNotifyTemplateFormData } from './events.models';
import { PermissionPipe } from '@/core/pipes/permission.pipe';
import { PermissionService } from '@/core/services/permission.service';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';

@Component({
  selector: 'app-events',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    PanelModule,
    ButtonModule,
    InputTextModule,
    SelectModule,
    InputNumberModule,
    ToastModule,
    DialogModule,
    ConfirmDialogModule,
    TextareaModule,
    TooltipModule,
    VTable,
    PermissionPipe,
    TranslatePipe
  ],
  templateUrl: './events.component.html',
  styleUrl: './events.component.scss',
  providers: [MessageService, ConfirmationService]
})
export class EventsComponent implements OnInit {
  // Permissions
  readonly PERMISSIONS = {
    CREATE: 'MasterResEvent.Create',
    UPDATE: 'MasterResEvent.Edit',
    DELETE: 'MasterResEvent.Delete',
    VIEW: 'MasterResEvent.View'
  };

  readonly TEMPLATE_PERMISSIONS = {
    CREATE: 'MasterResEventNotifyTemplate.Create',
    UPDATE: 'MasterResEventNotifyTemplate.Edit',
    DELETE: 'MasterResEventNotifyTemplate.Delete',
    VIEW: 'MasterResEventNotifyTemplate.View'
  };

  // Data
  events: ResEventDto[] = [];
  totalCount = 0;
  loading = true;
  pageSize = 10;

  // Dialog
  dialogVisible = false;
  dialogMode: 'create' | 'edit' = 'create';
  formData: EventFormData = this.getEmptyForm();
  selectedEvent?: ResEventDto;

  // Search form
  searchForm: EventSearchForm = {
    code: null,
    name: null,
    status: null
  };

  // Status options
  statusOptions: Array<{ label: string; value: ResEventStatus }> = [];

  // Table
  columns: TableColumn[] = [];
  actions: TableAction<ResEventDto>[] = [];
  currentLazyLoadEvent?: TableLazyLoadEvent;

  // Templates panel
  selectedEventForTemplates?: ResEventDto;
  templatesPanelVisible = false;
  templates: ResEventNotifyTemplateDto[] = [];
  templatesLoading = false;

  // Template dialog
  templateDialogVisible = false;
  templateDialogMode: 'create' | 'edit' = 'create';
  templateFormData: EventNotifyTemplateFormData = this.getEmptyTemplateForm();
  selectedTemplate?: ResEventNotifyTemplateDto;
  appChannels: ResAppChannelDto[] = [];
  appChannelOptions: Array<{ label: string; value: string }> = [];

  constructor(
    private eventService: ResEventService,
    private templateService: ResEventNotifyTemplateService,
    private appChannelService: ResAppChannelService,
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
    // Load app channels for template dropdown
    this.loadAppChannels();
  }

  /**
   * Initialize status options with localized labels
   */
  private initializeStatusOptions(): void {
    this.statusOptions = [
      { label: this.localizationService.localize('Master::ResEvent:Active'), value: ResEventStatus.Active },
      { label: this.localizationService.localize('Master::ResEvent:Deactive'), value: ResEventStatus.Deactive }
    ];
  }

  /**
   * Initialize table columns with localized headers
   */
  private initializeColumns(): void {
    this.columns = [
      {
        field: 'code',
        header: this.localizationService.localize('Master::ResEvent:Code'),
        sortable: true,
        width: '150px'
      },
      {
        field: 'name',
        header: this.localizationService.localize('Master::ResEvent:Name'),
        sortable: true,
        width: '250px'
      },
      {
        field: 'description',
        header: this.localizationService.localize('Master::ResEvent:Description'),
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
        header: this.localizationService.localize('Master::ResEvent:Status'),
        sortable: true,
        type: 'text',
        width: '120px',
        freeze: 'right',
        align: 'center',
        formatter: (value: any) => this.formatStatus(value),
        cellClass: (value: any) => {
          const statusValue = typeof value === 'number' ? value : 
            (value === ResEventStatus.Active ? 0 : 1);
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

    if (this.permissionService.isGranted(this.TEMPLATE_PERMISSIONS.VIEW)) {
      this.actions.push({
        label: this.localizationService.localize('Master::ResEvent:ConfigureTemplate'),
        icon: 'pi pi-cog',
        command: (row) => this.openTemplatesPanel(row)
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
   * Load events with lazy loading
   */
  loadData(event: TableLazyLoadEvent): void {
    this.currentLazyLoadEvent = event;
    this.loading = true;

    const input: GetResEventsInput = {
      skipCount: event.first || 0,
      maxResultCount: event.rows || this.pageSize,
      sorting: this.getSortingString(event),
      code: this.searchForm.code || undefined,
      name: this.searchForm.name || undefined,
      status: this.searchForm.status ?? undefined
    };

    this.eventService.getList(input).subscribe({
      next: (result) => {
        this.events = result.items || [];
        this.totalCount = result.totalCount || 0;
        this.loading = false;
      },
      error: (error) => {
        const errorMessage =
          error.error?.error?.message ||
          error.error?.error?.details ||
          this.localizationService.localize('AbpUi::InternalServerErrorMessage');

        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: errorMessage
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
    // Only trigger search if no dialog is visible
    if (!this.dialogVisible && !this.templatesPanelVisible && !this.templateDialogVisible) {
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
    this.selectedEvent = undefined;
    this.dialogVisible = true;
  }

  /**
   * Open edit dialog
   * ⚠️ QUAN TRỌNG: Code không được phép sửa khi edit
   */
  openEditDialog(event: ResEventDto): void {
    this.dialogMode = 'edit';
    this.selectedEvent = event;
    this.formData = {
      code: event.code || '', // Readonly trong edit mode
      name: event.name || '',
      description: event.description || '',
      status: event.status ?? ResEventStatus.Active
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
          detail: this.localizationService.localize('Master::ResEvent:CodeRequired')
        });
        return false;
      }

      if (this.formData.code.length > 50) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: this.localizationService.localize('Master::ResEvent:CodeMaxLength')
        });
        return false;
      }

      if (!this.validateCode(this.formData.code)) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: this.localizationService.localize('Master::ResEvent:CodeInvalid')
        });
        return false;
      }
    }

    if (!this.formData.name || this.formData.name.trim() === '') {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Master::ResEvent:NameRequired')
      });
      return false;
    }

    if (this.formData.name.length > 50) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Master::ResEvent:NameMaxLength')
      });
      return false;
    }

    if (this.formData.description && this.formData.description.length > 500) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Master::ResEvent:DescriptionMaxLength')
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
   * Translate error message
   */
  translateErrorMessage(message: string, error?: any): string {
    if (!message) {
      return message;
    }
    
    if (message.includes(':') && !message.includes('::')) {
      if (message.startsWith('Master:')) {
        message = message.replace('Master:', 'Master::');
      }
    }
    
    let translated = this.localizationService.localize(message);
    
    if (translated === message) {
      translated = message;
    }
    
    if (translated.includes('{Code}')) {
      const codeValue = error?.value || error?.error?.data?.Code || '';
      translated = translated.replace(/{Code}/g, codeValue);
    }
    
    if (translated.includes('{0}')) {
      const value = error?.value || '';
      translated = translated.replace(/\{0\}/g, value);
    }
    
    return translated;
  }

  /**
   * Create new event
   */
  private create(): void {
    this.loading = true;

    const createDto: CreateResEventDto = {
      code: this.formData.code.trim(),
      name: this.formData.name.trim(),
      description: this.formData.description?.trim() || undefined,
      status: this.formData.status
    };

    this.eventService.create(createDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Master::Success'),
          detail: this.localizationService.localize('Master::ResEvent:CreatedSuccessfully')
        });
        this.dialogVisible = false;
        this.loading = false;
        this.search();
      },
      error: (error) => {
        let errorMessage =
          error.error?.error?.message ||
          error.error?.error?.details ||
          this.localizationService.localize('AbpUi::InternalServerErrorMessage');

        errorMessage = this.translateErrorMessage(errorMessage, error);

        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: errorMessage
        });
        this.loading = false;
      }
    });
  }

  /**
   * Update existing event
   * ⚠️ QUAN TRỌNG: Chỉ update Name, Description và Status, không update Code
   */
  private update(): void {
    if (!this.selectedEvent || !this.selectedEvent.id) {
      return;
    }

    this.loading = true;

    const updateDto: UpdateResEventDto = {
      name: this.formData.name.trim(),
      description: this.formData.description?.trim() || undefined,
      status: this.formData.status
    };

    this.eventService.update(this.selectedEvent.id, updateDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Master::Success'),
          detail: this.localizationService.localize('Master::ResEvent:UpdatedSuccessfully')
        });
        this.dialogVisible = false;
        this.loading = false;
        this.search();
      },
      error: (error) => {
        let errorMessage =
          error.error?.error?.message ||
          error.error?.error?.details ||
          this.localizationService.localize('AbpUi::InternalServerErrorMessage');

        errorMessage = this.translateErrorMessage(errorMessage, error);

        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: errorMessage
        });
        this.loading = false;
      }
    });
  }

  /**
   * Delete event with confirmation
   */
  delete(event: ResEventDto): void {
    if (!event.id) {
      return;
    }

    this.confirmationService.confirm({
      message: this.localizationService.localize('Master::ResEvent:DeleteConfirm'),
      header: this.localizationService.localize('Master::ResEvent:Delete'),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.loading = true;
        this.eventService.delete(event.id!).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: this.localizationService.localize('Master::Success'),
              detail: this.localizationService.localize('Master::ResEvent:DeletedSuccessfully')
            });
            this.loading = false;
            this.search();
          },
          error: (error) => {
            const errorMessage =
              error.error?.error?.message ||
              error.error?.error?.details ||
              this.localizationService.localize('AbpUi::InternalServerErrorMessage');

            this.messageService.add({
              severity: 'error',
              summary: this.localizationService.localize('AbpUi::Error'),
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
  private getEmptyForm(): EventFormData {
    return {
      code: '',
      name: '',
      description: '',
      status: ResEventStatus.Active
    };
  }

  /**
   * Format status for display
   */
  formatStatus(status: ResEventStatus | number | string | undefined | null): string {
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
      statusValue = status === ResEventStatus.Active ? 0 : 1;
    }
    
    return statusValue === 0
      ? this.localizationService.localize('Master::ResEvent:Active')
      : this.localizationService.localize('Master::ResEvent:Deactive');
  }

  /**
   * Open templates panel for selected event
   */
  openTemplatesPanel(event: ResEventDto): void {
    if (!event.id) {
      return;
    }
    this.selectedEventForTemplates = event;
    this.templatesPanelVisible = true;
    this.loadTemplates(event.id);
  }

  /**
   * Load templates for event
   */
  loadTemplates(eventId: string): void {
    this.templatesLoading = true;
    this.templateService.getListByEventId(eventId).subscribe({
      next: (result) => {
        this.templates = result.items || [];
        this.templatesLoading = false;
      },
      error: (error) => {
        const errorMessage =
          error.error?.error?.message ||
          error.error?.error?.details ||
          this.localizationService.localize('AbpUi::InternalServerErrorMessage');

        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: errorMessage
        });
        this.templatesLoading = false;
      }
    });
  }

  /**
   * Open create template dialog
   */
  openCreateTemplateDialog(): void {
    if (!this.selectedEventForTemplates?.id) {
      return;
    }
    // Load app channels if not loaded yet
    if (this.appChannelOptions.length === 0) {
      this.loadAppChannels();
    }
    this.templateDialogMode = 'create';
    this.templateFormData = this.getEmptyTemplateForm();
    this.selectedTemplate = undefined;
    this.templateDialogVisible = true;
  }

  /**
   * Open edit template dialog
   */
  openEditTemplateDialog(template: ResEventNotifyTemplateDto): void {
    // Load app channels if not loaded yet
    if (this.appChannelOptions.length === 0) {
      this.loadAppChannels();
    }
    this.templateDialogMode = 'edit';
    this.selectedTemplate = template;
    this.templateFormData = {
      appChannelId: template.appChannelId || '',
      retryNumber: template.retryNumber || 0,
      title: template.title || '',
      body: template.body || '',
      data: template.data || ''
    };
    this.templateDialogVisible = true;
  }

  /**
   * Save template (create or update)
   */
  saveTemplate(): void {
    if (!this.validateTemplateForm()) {
      return;
    }

    if (this.templateDialogMode === 'create') {
      this.createTemplate();
    } else {
      this.updateTemplate();
    }
  }

  /**
   * Validate template form
   */
  private validateTemplateForm(): boolean {
    if (!this.templateFormData.appChannelId) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Master::ResEventNotifyTemplate:AppChannelIdRequired')
      });
      return false;
    }

    if (this.templateFormData.retryNumber < 0 || this.templateFormData.retryNumber > 99) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Master::ResEventNotifyTemplate:RetryNumberRange')
      });
      return false;
    }

    if (!this.templateFormData.title || this.templateFormData.title.trim() === '') {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Master::ResEventNotifyTemplate:TitleRequired')
      });
      return false;
    }

    if (this.templateFormData.title.length > 250) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Master::ResEventNotifyTemplate:TitleMaxLength')
      });
      return false;
    }

    if (!this.templateFormData.body || this.templateFormData.body.trim() === '') {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Master::ResEventNotifyTemplate:BodyRequired')
      });
      return false;
    }

    if (this.templateFormData.body.length > 500) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Master::ResEventNotifyTemplate:BodyMaxLength')
      });
      return false;
    }

    if (this.templateFormData.data && this.templateFormData.data.length > 1000) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Master::ResEventNotifyTemplate:DataMaxLength')
      });
      return false;
    }

    return true;
  }

  /**
   * Create template
   */
  private createTemplate(): void {
    if (!this.selectedEventForTemplates?.id) {
      return;
    }

    this.templatesLoading = true;

    const createDto: CreateResEventNotifyTemplateDto = {
      eventId: this.selectedEventForTemplates.id,
      appChannelId: this.templateFormData.appChannelId,
      retryNumber: this.templateFormData.retryNumber,
      title: this.templateFormData.title.trim(),
      body: this.templateFormData.body.trim(),
      data: this.templateFormData.data?.trim() || undefined
    };

    this.templateService.create(createDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Master::Success'),
          detail: this.localizationService.localize('Master::ResEventNotifyTemplate:CreatedSuccessfully')
        });
        this.templateDialogVisible = false;
        this.templatesLoading = false;
        this.loadTemplates(this.selectedEventForTemplates!.id!);
      },
      error: (error) => {
        let errorMessage =
          error.error?.error?.message ||
          error.error?.error?.details ||
          this.localizationService.localize('AbpUi::InternalServerErrorMessage');

        errorMessage = this.translateErrorMessage(errorMessage, error);

        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: errorMessage
        });
        this.templatesLoading = false;
      }
    });
  }

  /**
   * Update template
   */
  private updateTemplate(): void {
    if (!this.selectedTemplate?.id) {
      return;
    }

    this.templatesLoading = true;

    const updateDto: UpdateResEventNotifyTemplateDto = {
      appChannelId: this.templateFormData.appChannelId,
      retryNumber: this.templateFormData.retryNumber,
      title: this.templateFormData.title.trim(),
      body: this.templateFormData.body.trim(),
      data: this.templateFormData.data?.trim() || undefined
    };

    this.templateService.update(this.selectedTemplate.id, updateDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Master::Success'),
          detail: this.localizationService.localize('Master::ResEventNotifyTemplate:UpdatedSuccessfully')
        });
        this.templateDialogVisible = false;
        this.templatesLoading = false;
        if (this.selectedEventForTemplates?.id) {
          this.loadTemplates(this.selectedEventForTemplates.id);
        }
      },
      error: (error) => {
        let errorMessage =
          error.error?.error?.message ||
          error.error?.error?.details ||
          this.localizationService.localize('AbpUi::InternalServerErrorMessage');

        errorMessage = this.translateErrorMessage(errorMessage, error);

        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: errorMessage
        });
        this.templatesLoading = false;
      }
    });
  }

  /**
   * Delete template
   */
  deleteTemplate(template: ResEventNotifyTemplateDto): void {
    if (!template.id) {
      return;
    }

    this.confirmationService.confirm({
      message: this.localizationService.localize('Master::ResEventNotifyTemplate:DeleteConfirm'),
      header: this.localizationService.localize('Master::ResEventNotifyTemplate:Delete'),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.templatesLoading = true;
        this.templateService.delete(template.id!).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: this.localizationService.localize('Master::Success'),
              detail: this.localizationService.localize('Master::ResEventNotifyTemplate:DeletedSuccessfully')
            });
            this.templatesLoading = false;
            if (this.selectedEventForTemplates?.id) {
              this.loadTemplates(this.selectedEventForTemplates.id);
            }
          },
          error: (error) => {
            const errorMessage =
              error.error?.error?.message ||
              error.error?.error?.details ||
              this.localizationService.localize('AbpUi::InternalServerErrorMessage');

            this.messageService.add({
              severity: 'error',
              summary: this.localizationService.localize('AbpUi::Error'),
              detail: errorMessage
            });
            this.templatesLoading = false;
          }
        });
      }
    });
  }

  /**
   * Get empty template form
   */
  private getEmptyTemplateForm(): EventNotifyTemplateFormData {
    return {
      appChannelId: '',
      retryNumber: 0,
      title: '',
      body: '',
      data: ''
    };
  }

  /**
   * Load app channels for dropdown
   */
  loadAppChannels(): void {
    this.appChannelService.getList({ skipCount: 0, maxResultCount: 1000 }).subscribe({
      next: (result) => {
        this.appChannels = result.items || [];
        this.appChannelOptions = this.appChannels.map(ch => ({
          label: `${ch.code} - ${ch.name}`,
          value: ch.id!
        }));
      },
      error: (error) => {
        const errorMessage =
          error.error?.error?.message ||
          error.error?.error?.details ||
          this.localizationService.localize('AbpUi::InternalServerErrorMessage');

        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: errorMessage
        });
      }
    });
  }
}

