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

import { VTable } from '@/shared/components/v-table/v-table';
import { TableColumn } from '@/shared/models/table-column.model';
import { TableAction } from '@/shared/models/table-action.model';
import { ResBusinessAuthorityService } from '@/proxy/master/controllers/res-business-authority.service';
import { AdminConfigService } from '@/proxy/master/controllers/admin-config.service';
import {
  ResBusinessAuthorityDto,
  CreateResBusinessAuthorityDto,
  UpdateResBusinessAuthorityDto,
  GetResBusinessAuthoritiesInput,
} from '@/proxy/master/res-business-authorities/models';
import { ResBusinessAuthorityStatus } from '@/proxy/res-business-authorities/res-business-authority-status.enum';
import { ResBusinessAuthoritySearchForm, ResBusinessAuthorityFormData } from './res-business-authorities.models';
import { PermissionPipe } from '@/core/pipes/permission.pipe';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { PermissionService } from '@/core/services/permission.service';
import { LocalizationService } from '@/core/services/localization.service';

@Component({
  selector: 'app-res-business-authorities',
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
    VTable,
    PermissionPipe,
    TranslatePipe,
  ],
  templateUrl: './res-business-authorities.component.html',
  styleUrl: './res-business-authorities.component.scss',
  providers: [MessageService, ConfirmationService],
})
export class ResBusinessAuthoritiesComponent implements OnInit {
  readonly PERMISSIONS = {
    CREATE: 'MasterResBusinessAuthority.Create',
    UPDATE: 'MasterResBusinessAuthority.Edit',
    DELETE: 'MasterResBusinessAuthority.Delete',
    VIEW: 'MasterResBusinessAuthority.View',
  };

  items: ResBusinessAuthorityDto[] = [];
  totalCount = 0;
  loading = true;
  pageSize = 10;

  dialogVisible = false;
  dialogMode: 'create' | 'edit' = 'create';
  formData: ResBusinessAuthorityFormData = this.getEmptyForm();
  selectedItem?: ResBusinessAuthorityDto;

  searchForm: ResBusinessAuthoritySearchForm = {
    code: null,
    businessCode: null,
    name: null,
    status: null,
  };

  statusOptions: Array<{ label: string; value: ResBusinessAuthorityStatus }> = [];
  businessCodeOptions: Array<{ label: string; value: string }> = [];

  columns: TableColumn[] = [];
  actions: TableAction<ResBusinessAuthorityDto>[] = [];
  currentLazyLoadEvent?: TableLazyLoadEvent;

  constructor(
    private service: ResBusinessAuthorityService,
    private adminConfigService: AdminConfigService,
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
    this.loadBusinessCodeOptions();
  }

  private initializeStatusOptions(): void {
    this.statusOptions = [
      {
        label: this.localizationService.localize('Master::ResBusinessAuthority:Active'),
        value: ResBusinessAuthorityStatus.Active,
      },
      {
        label: this.localizationService.localize('Master::ResBusinessAuthority:Deactive'),
        value: ResBusinessAuthorityStatus.Deactive,
      },
    ];
  }

  private initializeColumns(): void {
    this.columns = [
      {
        field: 'code',
        header: this.localizationService.localize('Master::ResBusinessAuthority:Code'),
        sortable: true,
        width: '120px',
      },
      {
        field: 'businessCode',
        header: this.localizationService.localize('Master::ResBusinessAuthority:BusinessCode'),
        sortable: true,
        width: '150px',
      },
      {
        field: 'name',
        header: this.localizationService.localize('Master::ResBusinessAuthority:Name'),
        sortable: true,
        width: '250px',
      },
      {
        field: 'creationTime',
        header: this.localizationService.localize('AbpIdentity::CreationTime'),
        sortable: true,
        type: 'date',
        width: '180px',
      },
      {
        field: 'status',
        header: this.localizationService.localize('Master::ResBusinessAuthority:Status'),
        sortable: true,
        type: 'text',
        width: '120px',
        freeze: 'right',
        align: 'center',
        formatter: (value: unknown) => this.formatStatus(value),
        cellClass: (value: unknown) => {
          const statusValue =
            typeof value === 'number'
              ? value
              : value === ResBusinessAuthorityStatus.Active
                ? 0
                : 1;
          return statusValue === 0
            ? 'px-2 py-1 rounded text-xs font-semibold bg-green-100 text-green-800'
            : 'px-2 py-1 rounded text-xs font-semibold bg-red-100 text-red-800';
        },
      },
    ];
  }

  private initializeActions(): void {
    if (this.permissionService.isGranted(this.PERMISSIONS.UPDATE)) {
      this.actions.push({
        label: this.localizationService.localize('Master::Edit'),
        icon: 'pi pi-pencil',
        command: (row) => this.openEditDialog(row),
      });
    }
    if (this.permissionService.isGranted(this.PERMISSIONS.DELETE)) {
      this.actions.push({
        label: this.localizationService.localize('Master::Delete'),
        icon: 'pi pi-trash',
        command: (row) => this.delete(row),
      });
    }
  }

  loadBusinessCodeOptions(): void {
    this.adminConfigService
      .getList({
        code: 'BUSINESS_CODE',
        skipCount: 0,
        maxResultCount: 1000,
      })
      .subscribe({
        next: (result) => {
          const items = result.items || [];
          this.businessCodeOptions = items.map((item) => ({
            label: item.value || item.subCode || '',
            value: item.subCode || '',
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
            detail: errorMessage,
          });
        },
      });
  }

  loadData(event: TableLazyLoadEvent): void {
    this.currentLazyLoadEvent = event;
    this.loading = true;

    const input: GetResBusinessAuthoritiesInput = {
      skipCount: event.first || 0,
      maxResultCount: event.rows || this.pageSize,
      sorting: this.getSortingString(event),
      code: this.searchForm.code || undefined,
      businessCode: this.searchForm.businessCode || undefined,
      name: this.searchForm.name || undefined,
      status: this.searchForm.status ?? undefined,
    };

    this.service.getList(input).subscribe({
      next: (result) => {
        this.items = result.items || [];
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
          detail: errorMessage,
        });
        this.loading = false;
      },
    });
  }

  private getSortingString(event: TableLazyLoadEvent): string | undefined {
    if (!event.sortField) return undefined;
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

  search(): void {
    if (this.currentLazyLoadEvent) {
      this.currentLazyLoadEvent.first = 0;
      this.loadData(this.currentLazyLoadEvent);
    }
  }

  resetSearch(): void {
    this.searchForm = {
      code: null,
      businessCode: null,
      name: null,
      status: null,
    };
    this.search();
  }

  openCreateDialog(): void {
    this.dialogMode = 'create';
    this.formData = this.getEmptyForm();
    this.selectedItem = undefined;
    this.dialogVisible = true;
  }

  openEditDialog(item: ResBusinessAuthorityDto): void {
    this.dialogMode = 'edit';
    this.selectedItem = item;
    this.formData = {
      code: item.code || '',
      businessCode: item.businessCode || '',
      name: item.name || '',
      status: item.status ?? ResBusinessAuthorityStatus.Active,
    };
    this.dialogVisible = true;
  }

  save(): void {
    if (!this.validateForm()) return;

    if (this.dialogMode === 'create') {
      this.create();
    } else {
      this.update();
    }
  }

  private validateForm(): boolean {
    if (this.dialogMode === 'create') {
      if (!this.formData.code || this.formData.code.trim() === '') {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: this.localizationService.localize('Master::ResBusinessAuthority:CodeRequired'),
        });
        return false;
      }
      if (this.formData.code.length > 25) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: this.localizationService.localize('Master::ResBusinessAuthority:CodeMaxLength'),
        });
        return false;
      }
      if (!/^[A-Z0-9_]+$/.test(this.formData.code)) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: this.localizationService.localize('Master::ResBusinessAuthority:CodeInvalid'),
        });
        return false;
      }
      if (!this.formData.businessCode || this.formData.businessCode.trim() === '') {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: this.localizationService.localize('Master::ResBusinessAuthority:BusinessCodeRequired'),
        });
        return false;
      }
    }

    if (!this.formData.name || this.formData.name.trim() === '') {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Master::ResBusinessAuthority:NameRequired'),
      });
      return false;
    }
    if (this.formData.name.length > 250) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Master::ResBusinessAuthority:NameMaxLength'),
      });
      return false;
    }
    return true;
  }

  onCodeInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    const value = input.value.toUpperCase();
    input.value = value;
    this.formData.code = value;
  }

  private create(): void {
    this.loading = true;
    const createDto: CreateResBusinessAuthorityDto = {
      code: this.formData.code.trim(),
      businessCode: this.formData.businessCode.trim(),
      name: this.formData.name.trim(),
      status: this.formData.status,
    };

    this.service.create(createDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Master::Success'),
          detail: this.localizationService.localize('Master::ResBusinessAuthority:CreatedSuccessfully'),
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
        if (error.error?.error?.data?.Code) {
          errorMessage = errorMessage.replace('{Code}', error.error.error.data.Code);
        }
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: errorMessage,
        });
        this.loading = false;
      },
    });
  }

  private update(): void {
    if (!this.selectedItem?.id) return;

    this.loading = true;
    const updateDto: UpdateResBusinessAuthorityDto = {
      name: this.formData.name.trim(),
      status: this.formData.status,
    };

    this.service.update(this.selectedItem.id, updateDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Master::Success'),
          detail: this.localizationService.localize('Master::ResBusinessAuthority:UpdatedSuccessfully'),
        });
        this.dialogVisible = false;
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
          detail: errorMessage,
        });
        this.loading = false;
      },
    });
  }

  delete(item: ResBusinessAuthorityDto): void {
    if (!item.id) return;

    this.confirmationService.confirm({
      message: this.localizationService.localize('Master::ResBusinessAuthority:DeleteConfirm'),
      header: this.localizationService.localize('Master::ResBusinessAuthority:Delete'),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.loading = true;
        this.service.delete(item.id!).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: this.localizationService.localize('Master::Success'),
              detail: this.localizationService.localize('Master::ResBusinessAuthority:DeletedSuccessfully'),
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
              detail: errorMessage,
            });
            this.loading = false;
          },
        });
      },
    });
  }

  private getEmptyForm(): ResBusinessAuthorityFormData {
    return {
      code: '',
      businessCode: '',
      name: '',
      status: ResBusinessAuthorityStatus.Active,
    };
  }

  formatStatus(status: unknown): string {
    if (status === undefined || status === null) return '';

    let statusValue: number;
    if (typeof status === 'number') {
      statusValue = status;
    } else if (typeof status === 'string') {
      statusValue =
        status === 'Active' || status === 'active' || status === '0' ? 0 : 1;
    } else {
      statusValue = status === ResBusinessAuthorityStatus.Active ? 0 : 1;
    }
    return statusValue === 0
      ? this.localizationService.localize('Master::ResBusinessAuthority:Active')
      : this.localizationService.localize('Master::ResBusinessAuthority:Deactive');
  }
}
