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
import { ConfirmDialogModule } from 'primeng/confirmdialog';

import { VTable } from '@/shared/components/v-table/v-table';
import { ResPartnerFormDialogComponent } from '@/shared/components/res-partner-form-dialog/res-partner-form-dialog.component';
import { TableColumn } from '@/shared/models/table-column.model';
import { TableAction } from '@/shared/models/table-action.model';
import { ResPartnerService } from '@/proxy/partner/controllers/res-partner.service';
import { ResPartnerDto, GetResPartnersInput } from '@/proxy/partner/res-partners/models';
import { ResPartnerStatus } from '@/proxy/res-partners/res-partner-status.enum';
import { ResPartnerSearchForm } from './res-partners.models';
import { PermissionPipe } from '@/core/pipes/permission.pipe';
import { PermissionService } from '@/core/services/permission.service';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';
import { ResPartnerTypeService } from '@/proxy/partner/controllers/res-partner-type.service';

@Component({
  selector: 'app-res-partners',
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
    ConfirmDialogModule,
    VTable,
    PermissionPipe,
    TranslatePipe,
    ResPartnerFormDialogComponent,
  ],
  templateUrl: './res-partners.component.html',
  styleUrl: './res-partners.component.scss',
  providers: [MessageService, ConfirmationService]
})
export class ResPartnersComponent implements OnInit {
  // Permissions
  readonly PERMISSIONS = {
    CREATE: 'PartnerResPartner.Create',
    UPDATE: 'PartnerResPartner.Edit',
    DELETE: 'PartnerResPartner.Delete',
    VIEW: 'PartnerResPartner.View'
  };

  // Data
  resPartners: ResPartnerDto[] = [];
  totalCount = 0;
  loading = true;
  pageSize = 10;

  // Partner create/edit dialog (shared)
  partnerDialogVisible = false;
  partnerDialogMode: 'create' | 'edit' = 'create';
  partnerEditId: string | null = null;

  // Search form
  searchForm: ResPartnerSearchForm = {
    code: null,
    name: null,
    partnerTypeId: null,
    organizationTypeId: null,
    provinceId: null,
    wardId: null,
    status: null,
    partnerRole: null
  };

  // Options
  statusOptions: Array<{ label: string; value: ResPartnerStatus }> = [];
  partnerTypeOptions: Array<{ label: string; value: string }> = [];

  // Table
  columns: TableColumn[] = [];
  actions: TableAction<ResPartnerDto>[] = [];
  currentLazyLoadEvent?: TableLazyLoadEvent;

  constructor(
    private resPartnerService: ResPartnerService,
    private partnerTypeService: ResPartnerTypeService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private permissionService: PermissionService,
    private localizationService: LocalizationService,
  ) {
    this.initializeColumns();
    this.initializeActions();
    this.initializeStatusOptions();
    this.loadPartnerTypes();
  }

  ngOnInit(): void {
    // Table lazy load triggers initial data
  }

  /**
   * Load partner types
   */
  private loadPartnerTypes(): void {
    this.partnerTypeService.getList({
      skipCount: 0,
      maxResultCount: 1000
    }).subscribe({
      next: (result) => {
        this.partnerTypeOptions = (result.items || []).map(item => ({
          label: item.name || '',
          value: item.id || ''
        }));
      },
      error: () => {
        // Silently fail
      }
    });
  }

  /**
   * Initialize status options with localized labels
   */
  private initializeStatusOptions(): void {
    this.statusOptions = [
      { label: this.localizationService.localize('Partner::ResPartner:Status:Active'), value: ResPartnerStatus.Active },
      { label: this.localizationService.localize('Partner::ResPartner:Status:Deactive'), value: ResPartnerStatus.Deactive }
    ];
  }

  /**
   * Initialize table columns with localized headers
   */
  private initializeColumns(): void {
    this.columns = [
      {
        field: 'code',
        header: this.localizationService.localize('Partner::ResPartner:Code'),
        sortable: true,
        width: '150px'
      },
      {
        field: 'name',
        header: this.localizationService.localize('Partner::ResPartner:Name'),
        sortable: true,
        width: '250px'
      },
      {
        field: 'partnerTypeName',
        header: this.localizationService.localize('Partner::ResPartner:PartnerType'),
        sortable: false,
        width: '150px'
      },
      {
        field: 'organizationTypeName',
        header: this.localizationService.localize('Partner::ResPartner:OrganizationType'),
        sortable: false,
        width: '150px'
      },
      {
        field: 'fullAddress',
        header: this.localizationService.localize('Partner::ResPartner:FullAddress'),
        sortable: false,
        width: '300px'
      },
      {
        field: 'phone',
        header: this.localizationService.localize('Partner::ResPartner:Phone'),
        sortable: false,
        width: '120px'
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
        header: this.localizationService.localize('Partner::ResPartner:Status'),
        sortable: true,
        type: 'text',
        width: '120px',
        freeze: 'right',
        align: 'center',
        formatter: (value: any) => this.formatStatus(value),
        cellClass: (value: any) => {
          const statusValue = typeof value === 'number' ? value : (value === ResPartnerStatus.Active ? 0 : 1);
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
        label: this.localizationService.localize('Partner::Edit'),
        icon: 'pi pi-pencil',
        command: (row) => this.openEditDialog(row)
      });
    }

    if (this.permissionService.isGranted(this.PERMISSIONS.DELETE)) {
      this.actions.push({
        label: this.localizationService.localize('Partner::Delete'),
        icon: 'pi pi-trash',
        command: (row) => this.delete(row)
      });
    }
  }

  /**
   * Load res partners with lazy loading
   */
  loadData(event: TableLazyLoadEvent): void {
    this.currentLazyLoadEvent = event;
    this.loading = true;

    const input: GetResPartnersInput = {
      skipCount: event.first || 0,
      maxResultCount: event.rows || 10,
      sorting: this.getSortingString(event),
      code: this.searchForm.code || undefined,
      name: this.searchForm.name || undefined,
      partnerTypeId: this.searchForm.partnerTypeId || undefined,
      organizationTypeId: this.searchForm.organizationTypeId || undefined,
      provinceId: this.searchForm.provinceId || undefined,
      wardId: this.searchForm.wardId || undefined,
      status: this.searchForm.status ?? undefined,
      partnerRole: this.searchForm.partnerRole || undefined
    };

    this.resPartnerService.getList(input).subscribe({
      next: (result) => {
        this.resPartners = result.items || [];
        this.totalCount = result.totalCount || 0;
        this.loading = false;
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Partner::Error'),
          detail: this.localizationService.localize('Partner::InternalServerErrorMessage')
        });
        this.loading = false;
      }
    });
  }

  /**
   * Listen for Enter key globally to trigger search when no dialog is open
   */
  @HostListener('window:keydown.enter', ['$event'])
  onWindowKeyDown(event: Event): void {
    if (!this.partnerDialogVisible) {
      this.search();
    }
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
      partnerTypeId: null,
      organizationTypeId: null,
      provinceId: null,
      wardId: null,
      status: null,
      partnerRole: null
    };
    this.search();
  }

  /**
   * Open create dialog
   */
  openCreateDialog(): void {
    this.partnerDialogMode = 'create';
    this.partnerEditId = null;
    this.partnerDialogVisible = true;
  }

  openEditDialog(resPartner: ResPartnerDto): void {
    this.partnerDialogMode = 'edit';
    this.partnerEditId = resPartner.id ?? null;
    this.partnerDialogVisible = true;
  }

  onPartnerSaved(_partner: ResPartnerDto): void {
    this.search();
  }

  /**
   * Delete res partner with confirmation
   */
  delete(resPartner: ResPartnerDto): void {
    if (!resPartner.id) {
      return;
    }

    this.confirmationService.confirm({
      message: this.localizationService.localize('Partner::ResPartner:DeleteConfirm'),
      header: this.localizationService.localize('Partner::ResPartner:Delete'),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.loading = true;
        this.resPartnerService.delete(resPartner.id!).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: this.localizationService.localize('Partner::Success'),
              detail: this.localizationService.localize('Partner::ResPartner:DeletedSuccessfully')
            });
            this.search();
          },
          error: (error) => {
            const errorMessage = error.error?.error?.message ||
              error.error?.error?.details ||
              this.localizationService.localize('AbpUi::InternalServerErrorMessage');
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: errorMessage
            });
            this.loading = false;
          }
        });
      }
    });
  }

  /**
   * Format status for display
   */
  formatStatus(status: ResPartnerStatus | number | string | undefined | null): string {
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
      statusValue = status === ResPartnerStatus.Active ? 0 : 1;
    }

    return statusValue === 0
      ? this.localizationService.localize('Partner::ResPartner:Status:Active')
      : this.localizationService.localize('Partner::ResPartner:Status:Deactive');
  }

}
