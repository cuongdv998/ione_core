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
import { ResCarGroupService } from '@/proxy/master/controllers/res-car-group.service';
import { ResCarGroupDto, CreateResCarGroupDto, UpdateResCarGroupDto, GetResCarGroupsInput } from '@/proxy/master/res-car-groups/models';
import { ResCarGroupStatus } from '@/proxy/res-car-groups/res-car-group-status.enum';
import { ResCarLineService } from '@/proxy/master/controllers/res-car-line.service';
import { ResCarLineDto, GetResCarLinesInput } from '@/proxy/master/res-car-lines/models';
import { ResCarLineStatus } from '@/proxy/res-car-lines/res-car-line-status.enum';
import { CarGroupSearchForm, CarGroupFormData } from './car-groups.models';
import { PermissionPipe } from '@/core/pipes/permission.pipe';
import { PermissionService } from '@/core/services/permission.service';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';

@Component({
  selector: 'app-car-groups',
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
  templateUrl: './car-groups.component.html',
  styleUrl: './car-groups.component.scss',
  providers: [MessageService, ConfirmationService]
})
export class CarGroupsComponent implements OnInit {
  // Permissions
  readonly PERMISSIONS = {
    CREATE: 'MasterResCarGroup.Create',
    UPDATE: 'MasterResCarGroup.Edit',
    DELETE: 'MasterResCarGroup.Delete',
    VIEW: 'MasterResCarGroup.View'
  };

  // Data
  carGroups: ResCarGroupDto[] = [];
  totalCount = 0;
  loading = true;
  pageSize = 10;

  // Dialog
  dialogVisible = false;
  dialogMode: 'create' | 'edit' = 'create';
  formData: CarGroupFormData = this.getEmptyForm();
  selectedCarGroup?: ResCarGroupDto;

  // Search form
  searchForm: CarGroupSearchForm = {
    carLineId: null,
    code: null,
    name: null,
    status: null
  };

  // Status options
  statusOptions: Array<{ label: string; value: ResCarGroupStatus }> = [];

  // Car Line options
  carLineOptions: Array<{ label: string; value: string }> = [];
  fullCarLineOptions: Array<{ label: string; value: string }> = [];
  loadingCarLines = false;

  // Table
  columns: TableColumn[] = [];
  actions: TableAction<ResCarGroupDto>[] = [];
  currentLazyLoadEvent?: TableLazyLoadEvent;

  constructor(
    private carGroupService: ResCarGroupService,
    private carLineService: ResCarLineService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private permissionService: PermissionService,
    private localizationService: LocalizationService
  ) {
    this.initializeColumns();
    this.initializeActions();
    this.initializeStatusOptions();
    this.loadCarLines();
  }

  ngOnInit(): void {
    // Initial load will be triggered by the table's onLazyLoad event
  }

  /**
   * Initialize status options with localized labels
   */
  private initializeStatusOptions(): void {
    this.statusOptions = [
      { label: this.localizationService.localize('Master::ResCarGroup:Active'), value: ResCarGroupStatus.Active },
      { label: this.localizationService.localize('Master::ResCarGroup:Deactive'), value: ResCarGroupStatus.Deactive }
    ];
  }

  /**
   * Load car lines for dropdown
   */
  private loadCarLines(): void {
    this.loadingCarLines = true;
    const input: GetResCarLinesInput = {
      skipCount: 0,
      maxResultCount: 1000,
      sorting: 'name asc'
    };

    this.carLineService.getList(input).subscribe({
      next: (result) => {
        this.carLineOptions = (result.items || []).filter(carLine => carLine.status === ResCarLineStatus.Active).map(carLine => ({
          label: `${carLine.code} - ${carLine.name}`,
          value: carLine.id || ''
        }));
        this.fullCarLineOptions = (result.items || []).map(carLine => ({
          label: `${carLine.code} - ${carLine.name}`,
          value: carLine.id || ''
        }));
        this.loadingCarLines = false;
      },
      error: () => {
        // Silently fail - car lines will just be empty
        this.loadingCarLines = false;
      }
    });
  }

  /**
   * Initialize table columns with localized headers
   */
  private initializeColumns(): void {
    this.columns = [
      {
        field: 'carLineId',
        header: this.localizationService.localize('Master::ResCarGroup:CarLine'),
        sortable: false,
        width: '200px',
        formatter: (value: any, row: ResCarGroupDto) => {
          const carLine = this.fullCarLineOptions.find(opt => opt.value === row.carLineId);
          return carLine ? carLine.label : (row.carLineId || '');
        }
      },
      {
        field: 'code',
        header: this.localizationService.localize('Master::ResCarGroup:Code'),
        sortable: true,
        width: '150px'
      },
      {
        field: 'name',
        header: this.localizationService.localize('Master::ResCarGroup:Name'),
        sortable: true,
        width: '250px'
      },
      {
        field: 'description',
        header: this.localizationService.localize('Master::ResCarGroup:Description'),
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
        header: this.localizationService.localize('Master::ResCarGroup:Status'),
        sortable: true,
        type: 'text',
        width: '120px',
        freeze: 'right',
        align: 'center',
        formatter: (value: any) => this.formatStatus(value),
        cellClass: (value: any) => {
          const statusValue = typeof value === 'number' ? value : 
            (value === ResCarGroupStatus.Active ? 0 : 1);
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
   * Load car groups with lazy loading
   */
  loadData(event: TableLazyLoadEvent): void {
    this.currentLazyLoadEvent = event;
    this.loading = true;

    const input: GetResCarGroupsInput = {
      skipCount: event.first || 0,
      maxResultCount: event.rows || 10,
      sorting: this.getSortingString(event),
      carLineId: this.searchForm.carLineId || undefined,
      code: this.searchForm.code || undefined,
      name: this.searchForm.name || undefined,
      status: this.searchForm.status ?? undefined
    };

    this.carGroupService.getList(input).subscribe({
      next: (result) => {
        this.carGroups = result.items || [];
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
      carLineId: null,
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
    this.selectedCarGroup = undefined;
    this.dialogVisible = true;
  }

  /**
   * Open edit dialog
   * ⚠️ QUAN TRỌNG: Code không được phép sửa khi edit
   */
  openEditDialog(carGroup: ResCarGroupDto): void {
    this.dialogMode = 'edit';
    this.selectedCarGroup = carGroup;
    this.formData = {
      carLineId: carGroup.carLineId || null,
      code: carGroup.code || '', // Readonly trong edit mode
      name: carGroup.name || '',
      description: carGroup.description || '',
      status: carGroup.status ?? ResCarGroupStatus.Active
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
          detail: this.localizationService.localize('Master::ResCarGroup:CodeRequired')
        });
        return false;
      }

      if (this.formData.code.length > 50) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: this.localizationService.localize('Master::ResCarGroup:CodeMaxLength')
        });
        return false;
      }

      if (!this.validateCode(this.formData.code)) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: this.localizationService.localize('Master::ResCarGroup:CodeInvalid')
        });
        return false;
      }
    }

    if (!this.formData.name || this.formData.name.trim() === '') {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResCarGroup:NameRequired')
      });
      return false;
    }

    if (this.formData.name.length > 250) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResCarGroup:NameMaxLength')
      });
      return false;
    }

    if (this.formData.description && this.formData.description.length > 500) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResCarGroup:DescriptionMaxLength')
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
    // e.g., "Master:ResCarGroup:CodeExists" -> "Master::ResCarGroup:CodeExists"
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
   * Create new car group
   */
  private create(): void {
    this.loading = true;

    const createDto: CreateResCarGroupDto = {
      carLineId: this.formData.carLineId || undefined,
      code: this.formData.code.trim(),
      name: this.formData.name.trim(),
      description: this.formData.description?.trim() || undefined,
      status: this.formData.status
    };

    this.carGroupService.create(createDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Master::Success'),
          detail: this.localizationService.localize('Master::ResCarGroup:CreatedSuccessfully')
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
   * Update existing car group
   * ⚠️ QUAN TRỌNG: Chỉ update CarLineId, Name, Description và Status, không update Code
   */
  private update(): void {
    if (!this.selectedCarGroup || !this.selectedCarGroup.id) {
      return;
    }

    this.loading = true;

    const updateDto: UpdateResCarGroupDto = {
      carLineId: this.formData.carLineId || undefined,
      name: this.formData.name.trim(),
      description: this.formData.description?.trim() || undefined,
      status: this.formData.status
    };

    this.carGroupService.update(this.selectedCarGroup.id, updateDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Master::Success'),
          detail: this.localizationService.localize('Master::ResCarGroup:UpdatedSuccessfully')
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
   * Delete car group with confirmation
   */
  delete(carGroup: ResCarGroupDto): void {
    if (!carGroup.id) {
      return;
    }

    this.confirmationService.confirm({
      message: this.localizationService.localize('Master::ResCarGroup:DeleteConfirm'),
      header: this.localizationService.localize('Master::ResCarGroup:Delete'),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.loading = true;
        this.carGroupService.delete(carGroup.id!).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: this.localizationService.localize('Master::Success'),
              detail: this.localizationService.localize('Master::ResCarGroup:DeletedSuccessfully')
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
  private getEmptyForm(): CarGroupFormData {
    return {
      carLineId: null,
      code: '',
      name: '',
      description: '',
      status: ResCarGroupStatus.Active
    };
  }

  /**
   * Format status for display
   */
  formatStatus(status: ResCarGroupStatus | number | string | undefined | null): string {
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
      statusValue = status === ResCarGroupStatus.Active ? 0 : 1;
    }
    
    return statusValue === 0
      ? this.localizationService.localize('Master::ResCarGroup:Active')
      : this.localizationService.localize('Master::ResCarGroup:Deactive');
  }
}
