import { Component, HostListener, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { SelectModule } from 'primeng/select';
import { TableLazyLoadEvent } from 'primeng/table';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { DialogModule } from 'primeng/dialog';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TooltipModule } from 'primeng/tooltip';

import { VTable } from '@/shared/components/v-table/v-table';
import { TableColumn } from '@/shared/models/table-column.model';
import { TableAction } from '@/shared/models/table-action.model';
import { ResSequenceService } from '@/proxy/master/controllers/res-sequence.service';
import { 
  ResSequenceDto, 
  CreateResSequenceDto, 
  UpdateResSequenceDto, 
  GetResSequencesInput,
  GetNextSequenceInput,
  GetNextSequenceOutput
} from '@/proxy/master/res-sequences/models';
import { ResSequenceType } from '@/proxy/res-sequences/res-sequence-type.enum';
import { ResSequenceUseDateRange } from '@/proxy/res-sequences/res-sequence-use-date-range.enum';
import { ResSequenceDateRangeType } from '@/proxy/res-sequences/res-sequence-date-range-type.enum';
import { ResSequenceStatus } from '@/proxy/res-sequences/res-sequence-status.enum';
import { ResSequenceSearchForm, ResSequenceFormData } from './res-sequences.models';
import { PermissionPipe } from '@/core/pipes/permission.pipe';
import { PermissionService } from '@/core/services/permission.service';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';

@Component({
  selector: 'app-res-sequences',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    PanelModule,
    ButtonModule,
    InputTextModule,
    InputNumberModule,
    SelectModule,
    ToastModule,
    DialogModule,
    ConfirmDialogModule,
    TooltipModule,
    VTable,
    PermissionPipe,
    TranslatePipe
  ],
  templateUrl: './res-sequences.component.html',
  styleUrl: './res-sequences.component.scss',
  providers: [MessageService, ConfirmationService]
})
export class ResSequencesComponent implements OnInit {
  // Permissions
  readonly PERMISSIONS = {
    CREATE: 'MasterResSequence.Create',
    UPDATE: 'MasterResSequence.Edit',
    DELETE: 'MasterResSequence.Delete',
    VIEW: 'MasterResSequence.View'
  };

  // Data
  sequences: ResSequenceDto[] = [];
  totalCount = 0;
  loading = true;
  pageSize = 10;

  // Dialog
  dialogVisible = false;
  dialogMode: 'create' | 'edit' = 'create';
  formData: ResSequenceFormData = this.getEmptyForm();
  selectedSequence?: ResSequenceDto;

  // Test Dialog
  testDialogVisible = false;
  testLoading = false;
  testResult?: GetNextSequenceOutput;
  testSequence?: ResSequenceDto;
  testParameters: Array<{ key: string; value: string }> = [];

  // Search form
  searchForm: ResSequenceSearchForm = {
    code: null,
    name: null,
    type: null,
    status: null
  };

  // Options
  typeOptions: Array<{ label: string; value: ResSequenceType }> = [];
  statusOptions: Array<{ label: string; value: ResSequenceStatus }> = [];
  useDateRangeOptions: Array<{ label: string; value: ResSequenceUseDateRange }> = [];
  dateRangeTypeOptions: Array<{ label: string; value: ResSequenceDateRangeType }> = [];

  // Table
  columns: TableColumn[] = [];
  actions: TableAction<ResSequenceDto>[] = [];
  currentLazyLoadEvent?: TableLazyLoadEvent;

  constructor(
    private sequenceService: ResSequenceService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private permissionService: PermissionService,
    private localizationService: LocalizationService
  ) {
    this.initializeColumns();
    this.initializeActions();
    this.initializeOptions();
  }

  ngOnInit(): void {
    // Initial load will be triggered by the table's onLazyLoad event
  }

  /**
   * Initialize options with localized labels
   */
  private initializeOptions(): void {
    this.typeOptions = [
      { label: this.localizationService.localize('Master::ResSequence:Type:Normal'), value: ResSequenceType.Normal },
      { label: this.localizationService.localize('Master::ResSequence:Type:NoGap'), value: ResSequenceType.NoGap }
    ];

    this.statusOptions = [
      { label: this.localizationService.localize('Master::ResSequence:Active'), value: ResSequenceStatus.Active },
      { label: this.localizationService.localize('Master::ResSequence:Deactive'), value: ResSequenceStatus.Deactive }
    ];

    this.useDateRangeOptions = [
      { label: this.localizationService.localize('Master::ResSequence:UseDateRange:No'), value: ResSequenceUseDateRange.No },
      { label: this.localizationService.localize('Master::ResSequence:UseDateRange:Yes'), value: ResSequenceUseDateRange.Yes }
    ];

    this.dateRangeTypeOptions = [
      { label: this.localizationService.localize('Master::ResSequence:DateRangeType:Week'), value: ResSequenceDateRangeType.Week },
      { label: this.localizationService.localize('Master::ResSequence:DateRangeType:Month'), value: ResSequenceDateRangeType.Month },
      { label: this.localizationService.localize('Master::ResSequence:DateRangeType:Quarter'), value: ResSequenceDateRangeType.Quarter },
      { label: this.localizationService.localize('Master::ResSequence:DateRangeType:Half'), value: ResSequenceDateRangeType.Half },
      { label: this.localizationService.localize('Master::ResSequence:DateRangeType:Year'), value: ResSequenceDateRangeType.Year }
    ];
  }

  /**
   * Initialize table columns with localized headers
   */
  private initializeColumns(): void {
    this.columns = [
      {
        field: 'code',
        header: this.localizationService.localize('Master::ResSequence:Code'),
        sortable: true,
        width: '150px'
      },
      {
        field: 'name',
        header: this.localizationService.localize('Master::ResSequence:Name'),
        sortable: true,
        width: '250px'
      },
      {
        field: 'type',
        header: this.localizationService.localize('Master::ResSequence:Type'),
        sortable: true,
        type: 'text',
        width: '120px',
        formatter: (value: any) => this.formatType(value)
      },
      {
        field: 'numberNext',
        header: this.localizationService.localize('Master::ResSequence:NumberNext'),
        sortable: true,
        type: 'number',
        width: '120px',
        align: 'right'
      },
      {
        field: 'numberIncrement',
        header: this.localizationService.localize('Master::ResSequence:NumberIncrement'),
        sortable: true,
        type: 'number',
        width: '120px',
        align: 'right'
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
        header: this.localizationService.localize('Master::ResSequence:Status'),
        sortable: true,
        type: 'text',
        width: '120px',
        freeze: 'right',
        align: 'center',
        formatter: (value: any) => this.formatStatus(value),
        cellClass: (value: any) => {
          const statusValue = typeof value === 'number' ? value : 
            (value === ResSequenceStatus.Active ? 0 : 1);
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
    // Test action - always available if user can view
    if (this.permissionService.isGranted(this.PERMISSIONS.VIEW)) {
      this.actions.push({
        label: this.localizationService.localize('Master::ResSequence:Test'),
        icon: 'pi pi-play',
        command: (row) => this.openTestDialog(row)
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
   * Load sequences with lazy loading
   */
  loadData(event: TableLazyLoadEvent): void {
    this.currentLazyLoadEvent = event;
    this.loading = true;

    const input: GetResSequencesInput = {
      skipCount: event.first || 0,
      maxResultCount: event.rows || 10,
      sorting: this.getSortingString(event),
      code: this.searchForm.code || undefined,
      name: this.searchForm.name || undefined,
      type: this.searchForm.type ?? undefined,
      status: this.searchForm.status ?? undefined
    };

    this.sequenceService.getList(input).subscribe({
      next: (result) => {
        this.sequences = result.items || [];
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
    if (!this.dialogVisible && !this.testDialogVisible) {
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
      type: null,
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
    this.selectedSequence = undefined;
    this.dialogVisible = true;
  }

  /**
   * Open edit dialog
   * ⚠️ QUAN TRỌNG: Code không được phép sửa khi edit
   */
  openEditDialog(sequence: ResSequenceDto): void {
    this.dialogMode = 'edit';
    this.selectedSequence = sequence;
    this.formData = {
      code: sequence.code || '', // Readonly trong edit mode
      name: sequence.name || '',
      prefix: sequence.prefix || null,
      suffix: sequence.suffix || null,
      type: sequence.type ?? ResSequenceType.Normal,
      padding: sequence.padding ?? null,
      numberNext: sequence.numberNext ?? 0,
      numberIncrement: sequence.numberIncrement ?? 1,
      useDateRange: sequence.useDateRange ?? ResSequenceUseDateRange.No,
      dateRangeType: sequence.dateRangeType ?? null,
      status: sequence.status ?? ResSequenceStatus.Active
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
          detail: this.localizationService.localize('Master::ResSequence:CodeRequired')
        });
        return false;
      }

      if (this.formData.code.length > 25) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: this.localizationService.localize('Master::ResSequence:CodeMaxLength')
        });
        return false;
      }

      if (!this.validateCode(this.formData.code)) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: this.localizationService.localize('Master::ResSequence:CodeInvalid')
        });
        return false;
      }
    }

    if (!this.formData.name || this.formData.name.trim() === '') {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResSequence:NameRequired')
      });
      return false;
    }

    if (this.formData.name.length > 250) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResSequence:NameMaxLength')
      });
      return false;
    }

    if (this.formData.prefix && this.formData.prefix.length > 150) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResSequence:PrefixMaxLength')
      });
      return false;
    }

    if (this.formData.suffix && this.formData.suffix.length > 150) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResSequence:SuffixMaxLength')
      });
      return false;
    }

    if (this.formData.padding !== null && (this.formData.padding < 0 || this.formData.padding > 99)) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResSequence:PaddingRange')
      });
      return false;
    }

    if (this.formData.numberNext < 0 || this.formData.numberNext > 9999999999) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResSequence:NumberNextRange')
      });
      return false;
    }

    if (this.formData.numberIncrement < 1 || this.formData.numberIncrement > 9) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResSequence:NumberIncrementRange')
      });
      return false;
    }

    // Validate DateRangeType when UseDateRange = Yes
    // Note: Use == null to check for both null and undefined, but not 0 (Week = 0)
    if (this.formData.useDateRange === ResSequenceUseDateRange.Yes && this.formData.dateRangeType == null) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResSequence:DateRangeType') + ' is required when ' + 
                this.localizationService.localize('Master::ResSequence:UseDateRange') + ' = ' +
                this.localizationService.localize('Master::ResSequence:UseDateRange:Yes')
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
   * Handle UseDateRange change
   */
  onUseDateRangeChange(): void {
    if (this.formData.useDateRange === ResSequenceUseDateRange.No) {
      this.formData.dateRangeType = null;
    }
  }

  /**
   * Create new sequence
   */
  private create(): void {
    this.loading = true;

    const createDto: CreateResSequenceDto = {
      code: this.formData.code.trim(),
      name: this.formData.name.trim(),
      prefix: this.formData.prefix?.trim() || undefined,
      suffix: this.formData.suffix?.trim() || undefined,
      type: this.formData.type,
      padding: this.formData.padding ?? undefined,
      numberNext: this.formData.numberNext,
      numberIncrement: this.formData.numberIncrement,
      useDateRange: this.formData.useDateRange,
      dateRangeType: this.formData.dateRangeType ?? undefined,
      status: this.formData.status
    };

    this.sequenceService.create(createDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Master::Success'),
          detail: this.localizationService.localize('Master::ResSequence:CreatedSuccessfully')
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
   * Update existing sequence
   * ⚠️ QUAN TRỌNG: Chỉ update các fields khác, không update Code
   */
  private update(): void {
    if (!this.selectedSequence || !this.selectedSequence.id) {
      return;
    }

    this.loading = true;

    const updateDto: UpdateResSequenceDto = {
      name: this.formData.name.trim(),
      prefix: this.formData.prefix?.trim() || undefined,
      suffix: this.formData.suffix?.trim() || undefined,
      type: this.formData.type,
      padding: this.formData.padding ?? undefined,
      numberNext: this.formData.numberNext,
      numberIncrement: this.formData.numberIncrement,
      useDateRange: this.formData.useDateRange,
      dateRangeType: this.formData.dateRangeType ?? undefined,
      status: this.formData.status
    };

    this.sequenceService.update(this.selectedSequence.id, updateDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Master::Success'),
          detail: this.localizationService.localize('Master::ResSequence:UpdatedSuccessfully')
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
   * Delete sequence with confirmation
   * ⚠️ QUAN TRỌNG: Soft delete - set Status to Deactive
   */
  delete(sequence: ResSequenceDto): void {
    if (!sequence.id) {
      return;
    }

    this.confirmationService.confirm({
      message: this.localizationService.localize('Master::ResSequence:DeleteConfirm'),
      header: this.localizationService.localize('Master::ResSequence:Delete'),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.loading = true;
        this.sequenceService.delete(sequence.id!).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: this.localizationService.localize('Master::Success'),
              detail: this.localizationService.localize('Master::ResSequence:DeletedSuccessfully')
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
  private getEmptyForm(): ResSequenceFormData {
    return {
      code: '',
      name: '',
      prefix: null,
      suffix: null,
      type: ResSequenceType.Normal,
      padding: null,
      numberNext: 0,
      numberIncrement: 1,
      useDateRange: ResSequenceUseDateRange.No,
      dateRangeType: null,
      status: ResSequenceStatus.Active
    };
  }

  /**
   * Format type for display
   */
  formatType(type: ResSequenceType | number | string | undefined | null): string {
    if (type === undefined || type === null) {
      return '';
    }
    
    let typeValue: number;
    
    if (typeof type === 'number') {
      typeValue = type;
    } else if (typeof type === 'string') {
      typeValue = type === 'Normal' || type === 'normal' || type === '0' ? 0 : 1;
    } else {
      typeValue = type === ResSequenceType.Normal ? 0 : 1;
    }
    
    return typeValue === 0
      ? this.localizationService.localize('Master::ResSequence:Type:Normal')
      : this.localizationService.localize('Master::ResSequence:Type:NoGap');
  }

  /**
   * Format status for display
   */
  formatStatus(status: ResSequenceStatus | number | string | undefined | null): string {
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
      statusValue = status === ResSequenceStatus.Active ? 0 : 1;
    }
    
    return statusValue === 0
      ? this.localizationService.localize('Master::ResSequence:Active')
      : this.localizationService.localize('Master::ResSequence:Deactive');
  }

  /**
   * Open test dialog
   */
  openTestDialog(sequence: ResSequenceDto): void {
    this.testSequence = sequence;
    this.testResult = undefined;
    this.testParameters = [];
    this.testDialogVisible = true;
  }

  /**
   * Test generate sequence
   */
  testGenerate(): void {
    if (!this.testSequence || !this.testSequence.id) {
      return;
    }

    // Check if sequence is active
    if (this.testSequence.status !== ResSequenceStatus.Active) {
      this.messageService.add({
        severity: 'warn',
        summary: this.localizationService.localize('Master::Warning'),
        detail: this.localizationService.localize('Master::ResSequence:NotActive')
      });
      return;
    }

    this.testLoading = true;
    this.testResult = undefined;

    // Build parameters object from array
    const parameters: Record<string, string> = {};
    this.testParameters.forEach(param => {
      if (param.key && param.key.trim() && param.value && param.value.trim()) {
        parameters[param.key.trim()] = param.value.trim();
      }
    });

    const input: GetNextSequenceInput = {
      code: this.testSequence.code,
      id: this.testSequence.id,
      parameters: parameters
    };

    this.sequenceService.getNextSequence(input).subscribe({
      next: (result) => {
        this.testResult = result;
        this.testLoading = false;
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Master::Success'),
          detail: this.localizationService.localize('Master::ResSequence:TestSuccess')
        });
        // Reload data to show updated NumberNext
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
        this.testLoading = false;
      }
    });
  }

  /**
   * Add new parameter row
   */
  addTestParameter(): void {
    this.testParameters.push({ key: '', value: '' });
  }

  /**
   * Remove parameter row
   */
  removeTestParameter(index: number): void {
    this.testParameters.splice(index, 1);
  }
}

