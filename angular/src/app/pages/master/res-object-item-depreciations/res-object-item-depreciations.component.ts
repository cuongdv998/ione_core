import { Component, HostListener, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { SelectModule } from 'primeng/select';
import { DatePickerModule } from 'primeng/datepicker';
import { TableLazyLoadEvent } from 'primeng/table';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { DialogModule } from 'primeng/dialog';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { FileUploadModule } from 'primeng/fileupload';

import { VTable } from '@/shared/components/v-table/v-table';
import { TableColumn } from '@/shared/models/table-column.model';
import { TableAction } from '@/shared/models/table-action.model';
import { ResObjectItemDepreciationService } from '@/proxy/master/controllers/res-object-item-depreciation.service';
import { HttpClient } from '@angular/common/http';
import { ResObjectItemDepreciationDto, CreateResObjectItemDepreciationDto, UpdateResObjectItemDepreciationDto, GetResObjectItemDepreciationsInput } from '@/proxy/master/res-object-item-depreciations/models';
import { ResObjectItemDepreciationStatus } from '@/proxy/res-object-item-depreciations/res-object-item-depreciation-status.enum';
import { ResObjectItemDepreciationSearchForm, ResObjectItemDepreciationFormData, StatusOption } from './res-object-item-depreciations.models';
import { ResObjectTypeItemService } from '@/proxy/master/controllers/res-object-type-item.service';
import { GetResObjectTypeItemsInput } from '@/proxy/master/res-object-type-items/models';
import { ResObjectTypeItemStatus } from '@/proxy/res-object-type-items/res-object-type-item-status.enum';
import { ResCarGroupService } from '@/proxy/master/controllers/res-car-group.service';
import { GetResCarGroupsInput } from '@/proxy/master/res-car-groups/models';
import { ResCarGroupStatus } from '@/proxy/res-car-groups/res-car-group-status.enum';
import { PermissionPipe } from '@/core/pipes/permission.pipe';
import { PermissionService } from '@/core/services/permission.service';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';
import { environment } from '@environments/environment';

@Component({
  selector: 'app-res-object-item-depreciations',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    PanelModule,
    ButtonModule,
    InputTextModule,
    InputNumberModule,
    SelectModule,
    DatePickerModule,
    ToastModule,
    DialogModule,
    ConfirmDialogModule,
    FileUploadModule,
    VTable,
    PermissionPipe,
    TranslatePipe
  ],
  templateUrl: './res-object-item-depreciations.component.html',
  styleUrl: './res-object-item-depreciations.component.scss',
  providers: [MessageService, ConfirmationService]
})
export class ResObjectItemDepreciationsComponent implements OnInit {
  // Permissions
  readonly PERMISSIONS = {
    CREATE: 'MasterResObjectItemDepreciation.Create',
    UPDATE: 'MasterResObjectItemDepreciation.Edit',
    DELETE: 'MasterResObjectItemDepreciation.Delete',
    VIEW: 'MasterResObjectItemDepreciation.View'
  };

  // Data
  depreciations: ResObjectItemDepreciationDto[] = [];
  totalCount = 0;
  loading = true;
  pageSize = 10;

  // Dialog
  dialogVisible = false;
  dialogMode: 'create' | 'edit' = 'create';
  formData: ResObjectItemDepreciationFormData = this.getEmptyForm();
  selectedDepreciation?: ResObjectItemDepreciationDto;

  // Detail dialog
  detailDialogVisible = false;
  selectedDepreciationDetail?: ResObjectItemDepreciationDto;

  // Import dialog
  importDialogVisible = false;
  importResult: any = null;
  importing = false;

  // Search form
  searchForm: ResObjectItemDepreciationSearchForm = {
    objectTypeItemId: null,
    carGroupId: null,
    status: null,
    effectDateFrom: null,
    effectDateTo: null
  };

  // Status options
  statusOptions: StatusOption[] = [];

  // ObjectTypeItem options for dropdown
  objectTypeItemOptions: Array<{ label: string; value: string }> = [];
  fullObjectTypeItemOptions: Array<{ label: string; value: string }> = [];
  loadingObjectTypeItems = false;

  // CarGroup options for dropdown
  carGroupOptions: Array<{ label: string; value: string }> = [];
  fullCarGroupOptions: Array<{ label: string; value: string }> = [];
  loadingCarGroups = false;

  // Table
  columns: TableColumn[] = [];
  actions: TableAction<ResObjectItemDepreciationDto>[] = [];
  currentLazyLoadEvent?: TableLazyLoadEvent;

  constructor(
    private depreciationService: ResObjectItemDepreciationService,
    private objectTypeItemService: ResObjectTypeItemService,
    private carGroupService: ResCarGroupService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private permissionService: PermissionService,
    private localizationService: LocalizationService,
    private http: HttpClient
  ) {
    this.initializeColumns();
    this.initializeActions();
    this.initializeStatusOptions();
  }

  ngOnInit(): void {
    this.loadObjectTypeItems();
    this.loadCarGroups();
  }

  /**
   * Load ObjectTypeItems for dropdown
   */
  private loadObjectTypeItems(): void {
    this.loadingObjectTypeItems = true;
    const input: GetResObjectTypeItemsInput = {
      skipCount: 0,
      maxResultCount: 1000,
      sorting: 'name asc'
    };

    this.objectTypeItemService.getList(input).subscribe({
      next: (result) => {
        this.objectTypeItemOptions = (result.items || []).filter(item => item.status === ResObjectTypeItemStatus.Active).map(item => ({
          label: `${item.code} - ${item.name}`,
          value: item.id!
        }));
        this.fullObjectTypeItemOptions = (result.items || []).map(item => ({
          label: `${item.code} - ${item.name}`,
          value: item.id!
        }));
        this.loadingObjectTypeItems = false;
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Master::Error'),
          detail: this.localizationService.localize('Master::InternalServerErrorMessage')
        });
        this.loadingObjectTypeItems = false;
      }
    });
  }

  /**
   * Load CarGroups for dropdown using lookup method
   * This method uses GetLookupListAsync which has lower permission requirement (only needs Default permission)
   */
  private loadCarGroups(): void {
    this.loadingCarGroups = true;

    this.carGroupService.getLookupList().subscribe({
      next: (result) => {
        this.carGroupOptions = (result.items || []).map(item => ({
          label: `${item.code} - ${item.name}`,
          value: item.id!
        }));
        this.loadingCarGroups = false;
      },
      error: (error) => {
        // Silently handle errors for lookup dropdown
        console.warn('Failed to load CarGroups for dropdown:', error);
        this.carGroupOptions = [];
        this.loadingCarGroups = false;
      }
    });
  }

  /**
   * Initialize status options with localized labels
   */
  private initializeStatusOptions(): void {
    this.statusOptions = [
      { label: this.localizationService.localize('Master::ResObjectItemDepreciation:Active'), value: ResObjectItemDepreciationStatus.Active },
      { label: this.localizationService.localize('Master::ResObjectItemDepreciation:Deactive'), value: ResObjectItemDepreciationStatus.Deactive }
    ];
  }

  /**
   * Initialize table columns with localized headers
   */
  private initializeColumns(): void {
    this.columns = [
      {
        field: 'objectTypeItemId',
        header: this.localizationService.localize('Master::ResObjectItemDepreciation:ObjectTypeItemId'),
        sortable: true,
        width: '250px',
        formatter: (value: any) => this.formatObjectTypeItemId(value)
      },
      {
        field: 'carGroupCode',
        header: this.localizationService.localize('Master::ResObjectItemDepreciation:CarGroupCode'),
        sortable: true,
        width: '150px',
        formatter: (value: any, row: any) => {
          if (row.carGroupCode && row.carGroupName) {
            return `${row.carGroupCode} - ${row.carGroupName}`;
          }
          return row.carGroupId || '';
        }
      },
      {
        field: 'usedTimeFrom',
        header: this.localizationService.localize('Master::ResObjectItemDepreciation:UsedTimeFrom'),
        sortable: true,
        type: 'text',
        width: '150px',
        align: 'right'
      },
      {
        field: 'usedTimeTo',
        header: this.localizationService.localize('Master::ResObjectItemDepreciation:UsedTimeTo'),
        sortable: true,
        type: 'text',
        width: '150px',
        align: 'right'
      },
      {
        field: 'depreciationPercent',
        header: this.localizationService.localize('Master::ResObjectItemDepreciation:DepreciationPercent'),
        sortable: true,
        type: 'text',
        width: '150px',
        align: 'right',
        formatter: (value: any) => `${value}%`
      },
      {
        field: 'effectDate',
        header: this.localizationService.localize('Master::ResObjectItemDepreciation:EffectDate'),
        sortable: true,
        type: 'date',
        width: '150px'
      },
      {
        field: 'expireDate',
        header: this.localizationService.localize('Master::ResObjectItemDepreciation:ExpireDate'),
        sortable: true,
        type: 'date',
        width: '150px'
      },
      {
        field: 'status',
        header: this.localizationService.localize('Master::ResObjectItemDepreciation:Status'),
        sortable: true,
        type: 'text',
        width: '120px',
        freeze: 'right',
        align: 'center',
        formatter: (value: any) => this.formatStatus(value),
        cellClass: (value: any) => {
          const statusValue = typeof value === 'number' ? value :
            (value === ResObjectItemDepreciationStatus.Active ? 0 : 1);
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
    this.actions.push({
      label: this.localizationService.localize('Master::View'),
      icon: 'pi pi-eye',
      command: (row) => this.openDetailDialog(row)
    });

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
   * Load depreciations with lazy loading
   */
  loadData(event: TableLazyLoadEvent): void {
    this.currentLazyLoadEvent = event;
    this.loading = true;

    const input: GetResObjectItemDepreciationsInput = {
      skipCount: event.first || 0,
      maxResultCount: event.rows || 10,
      sorting: this.getSortingString(event),
      objectTypeItemId: this.searchForm.objectTypeItemId || undefined,
      carGroupId: this.searchForm.carGroupId || undefined,
      status: this.searchForm.status ?? undefined,
      effectDateFrom: this.formatDate(this.searchForm.effectDateFrom) || undefined,
      effectDateTo: this.formatDate(this.searchForm.effectDateTo) || undefined
    };

    this.depreciationService.getList(input).subscribe({
      next: (result) => {
        this.depreciations = result.items || [];
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
    if (!this.dialogVisible && !this.detailDialogVisible && !this.importDialogVisible) {
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
      objectTypeItemId: null,
      carGroupId: null,
      status: null,
      effectDateFrom: null,
      effectDateTo: null
    };
    this.search();
  }

  /**
   * Open create dialog
   */
  openCreateDialog(): void {
    this.dialogMode = 'create';
    this.formData = this.getEmptyForm();
    this.selectedDepreciation = undefined;
    this.dialogVisible = true;
  }

  /**
   * Open edit dialog
   */
  openEditDialog(depreciation: ResObjectItemDepreciationDto): void {
    this.dialogMode = 'edit';
    this.selectedDepreciation = depreciation;
    this.formData = {
      objectTypeItemId: depreciation.objectTypeItemId || '',
      carGroupId: depreciation.carGroupId || '',
      usedTimeFrom: depreciation.usedTimeFrom,
      usedTimeTo: depreciation.usedTimeTo,
      depreciationPercent: depreciation.depreciationPercent,
      effectDate: depreciation.effectDate ? new Date(depreciation.effectDate as string) : null,
      expireDate: depreciation.expireDate ? new Date(depreciation.expireDate as string) : null,
      status: depreciation.status ?? ResObjectItemDepreciationStatus.Active
    };
    this.dialogVisible = true;
  }

  /**
   * Open detail dialog
   */
  openDetailDialog(depreciation: ResObjectItemDepreciationDto): void {
    this.selectedDepreciationDetail = depreciation;
    this.detailDialogVisible = true;
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
    if (!this.formData.objectTypeItemId || this.formData.objectTypeItemId.trim() === '') {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResObjectItemDepreciation:ObjectTypeItemIdRequired')
      });
      return false;
    }

    if (!this.formData.carGroupId || this.formData.carGroupId.trim() === '') {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResObjectItemDepreciation:CarGroupIdRequired')
      });
      return false;
    }

    if (this.formData.usedTimeFrom < 0) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResObjectItemDepreciation:UsedTimeFromRange')
      });
      return false;
    }

    if (this.formData.usedTimeTo < 0) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResObjectItemDepreciation:UsedTimeToRange')
      });
      return false;
    }

    if (this.formData.usedTimeFrom > this.formData.usedTimeTo) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResObjectItemDepreciation:UsedTimeRangeInvalid')
      });
      return false;
    }

    if (this.formData.depreciationPercent < 0 || this.formData.depreciationPercent > 100) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResObjectItemDepreciation:DepreciationPercentRange')
      });
      return false;
    }

    if (!this.formData.effectDate) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Master::Error'),
        detail: this.localizationService.localize('Master::ResObjectItemDepreciation:EffectDateRequired')
      });
      return false;
    }

    return true;
  }

  /**
   * Helper function to convert Date objects or Date strings to YYYY-MM-DD
   * Prevents UTC offset reduction (1 day backward) when transforming to JSON
   */
  private formatDate(date: any): string | undefined {
    if (!date) return undefined;
    if (typeof date === 'string') return date.split('T')[0];

    const d = new Date(date);
    if (isNaN(d.getTime())) return undefined;

    const year = d.getFullYear();
    const month = (d.getMonth() + 1).toString().padStart(2, '0');
    const day = d.getDate().toString().padStart(2, '0');

    return `${year}-${month}-${day}`;
  }

  /**
   * Create new depreciation
   */
  private create(): void {
    this.loading = true;

    const createDto: CreateResObjectItemDepreciationDto = {
      objectTypeItemId: this.formData.objectTypeItemId.trim(),
      carGroupId: this.formData.carGroupId.trim(),
      usedTimeFrom: this.formData.usedTimeFrom,
      usedTimeTo: this.formData.usedTimeTo,
      depreciationPercent: this.formData.depreciationPercent,
      effectDate: this.formatDate(this.formData.effectDate)!,
      expireDate: this.formatDate(this.formData.expireDate),
      status: this.formData.status
    };

    this.depreciationService.create(createDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Master::Success'),
          detail: this.localizationService.localize('Master::ResObjectItemDepreciation:CreatedSuccessfully')
        });
        this.dialogVisible = false;
        this.search();
      },
      error: (error) => {
        let errorMessage = error.error?.error?.message ||
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
   * Update existing depreciation
   */
  private update(): void {
    if (!this.selectedDepreciation || !this.selectedDepreciation.id) {
      return;
    }

    this.loading = true;

    const updateDto: UpdateResObjectItemDepreciationDto = {
      objectTypeItemId: this.formData.objectTypeItemId.trim(),
      carGroupId: this.formData.carGroupId.trim(),
      usedTimeFrom: this.formData.usedTimeFrom,
      usedTimeTo: this.formData.usedTimeTo,
      depreciationPercent: this.formData.depreciationPercent,
      effectDate: this.formatDate(this.formData.effectDate)!,
      expireDate: this.formatDate(this.formData.expireDate),
      status: this.formData.status
    };

    this.depreciationService.update(this.selectedDepreciation.id, updateDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Master::Success'),
          detail: this.localizationService.localize('Master::ResObjectItemDepreciation:UpdatedSuccessfully')
        });
        this.dialogVisible = false;
        this.search();
      },
      error: (error) => {
        let errorMessage = error.error?.error?.message ||
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
   * Delete depreciation with confirmation
   */
  delete(depreciation: ResObjectItemDepreciationDto): void {
    if (!depreciation.id) {
      return;
    }

    this.confirmationService.confirm({
      message: this.localizationService.localize('Master::ResObjectItemDepreciation:DeleteConfirm'),
      header: this.localizationService.localize('Master::ResObjectItemDepreciation:Delete'),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.loading = true;
        this.depreciationService.delete(depreciation.id!).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: this.localizationService.localize('Master::Success'),
              detail: this.localizationService.localize('Master::ResObjectItemDepreciation:DeletedSuccessfully')
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
  private getEmptyForm(): ResObjectItemDepreciationFormData {
    return {
      objectTypeItemId: '',
      carGroupId: '',
      usedTimeFrom: 0,
      usedTimeTo: 0,
      depreciationPercent: 0,
      effectDate: null,
      expireDate: null,
      status: ResObjectItemDepreciationStatus.Active
    };
  }

  /**
   * Open import dialog
   */
  openImportDialog(): void {
    this.importResult = null;
    this.importDialogVisible = true;
  }

  /**
   * Handle file selection and import
   */
  onFileSelect(event: any): void {
    const file = event.files[0];

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
        detail: this.localizationService.localize('Master::ResObjectItemDepreciation:InvalidFileType')
      });
      return;
    }

    this.importing = true;

    this.depreciationService.importExcel(file).subscribe({
      next: (result: any) => {
        this.importResult = result;
        this.importing = false;

        if (result.errorCount === 0) {
          this.messageService.add({
            severity: 'success',
            summary: this.localizationService.localize('Master::Success'),
            detail: `${this.localizationService.localize('Master::ResObjectItemDepreciation:ImportSuccess')} (${result.successCount} ${this.localizationService.localize('Master::ResObjectItemDepreciation:Records')})`
          });
          this.search(); // Refresh the list
        } else {
          this.messageService.add({
            severity: 'warn',
            summary: this.localizationService.localize('Master::Warning'),
            detail: `${this.localizationService.localize('Master::ResObjectItemDepreciation:ImportPartialSuccess')} - ${this.localizationService.localize('Master::ResObjectItemDepreciation:SuccessCount')}: ${result.successCount}, ${this.localizationService.localize('Master::ResObjectItemDepreciation:ErrorCount')}: ${result.errorCount}`
          });
        }
      },
      error: (error) => {
        this.importing = false;
        const errorMessage = error.error?.error?.message ||
          error.error?.error?.details ||
          this.localizationService.localize('Master::InternalServerErrorMessage');
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
    const apiEndpoint = `${apiUrl}/api/master/object-item-depreciations/export-template`;

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
        link.download = `ResObjectItemDepreciation_Template_${timestamp}.xlsx`;

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
          detail: this.localizationService.localize('Master::ResObjectItemDepreciation:TemplateDownloaded')
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
                detail: errorMessage
              });
            } catch (e) {
              this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('Master::Error'),
                detail: this.localizationService.localize('Master::InternalServerErrorMessage')
              });
            }
          };
          reader.readAsText(error.error);
        } else {
          const errorMessage = error.error?.error?.message ||
            error.error?.error?.details ||
            this.localizationService.localize('Master::InternalServerErrorMessage');
          this.messageService.add({
            severity: 'error',
            summary: this.localizationService.localize('Master::Error'),
            detail: errorMessage
          });
        }
      }
    });
  }

  /**
   * Format ObjectTypeItemId for display
   */
  formatObjectTypeItemId(id: string | undefined | null): string {
    if (!id) {
      return '';
    }
    const item = this.fullObjectTypeItemOptions.find(opt => opt.value === id);
    return item ? item.label : id;
  }

  /**
   * Format status for display
   */
  formatStatus(status: ResObjectItemDepreciationStatus | number | string | undefined | null): string {
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
      statusValue = status === ResObjectItemDepreciationStatus.Active ? 0 : 1;
    }

    return statusValue === 0
      ? this.localizationService.localize('Master::ResObjectItemDepreciation:Active')
      : this.localizationService.localize('Master::ResObjectItemDepreciation:Deactive');
  }
}
