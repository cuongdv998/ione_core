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
import { ResTaskCategoryService } from '@/proxy/master/controllers/res-task-category.service';
import {
  ResTaskCategoryDto,
  CreateResTaskCategoryDto,
  UpdateResTaskCategoryDto,
  GetResTaskCategoriesInput,
} from '@/proxy/master/res-task-categories/models';
import { ResTaskCategoryStatus } from '@/proxy/res-task-categories/res-task-category-status.enum';
import { ResTaskCategoryBusinessType } from '@/proxy/res-task-categories/res-task-category-business-type.enum';
import { ResTaskCategorySearchForm, ResTaskCategoryFormData } from './res-task-categories.models';
import { PermissionPipe } from '@/core/pipes/permission.pipe';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { PermissionService } from '@/core/services/permission.service';
import { LocalizationService } from '@/core/services/localization.service';

@Component({
  selector: 'app-res-task-categories',
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
  templateUrl: './res-task-categories.component.html',
  styleUrl: './res-task-categories.component.scss',
  providers: [MessageService, ConfirmationService],
})
export class ResTaskCategoriesComponent implements OnInit {
  readonly PERMISSIONS = {
    CREATE: 'MasterResTaskCategory.Create',
    UPDATE: 'MasterResTaskCategory.Edit',
    DELETE: 'MasterResTaskCategory.Delete',
    VIEW: 'MasterResTaskCategory.View',
  };

  items: ResTaskCategoryDto[] = [];
  totalCount = 0;
  loading = true;
  pageSize = 10;

  dialogVisible = false;
  dialogMode: 'create' | 'edit' = 'create';
  formData: ResTaskCategoryFormData = this.getEmptyForm();
  selectedItem?: ResTaskCategoryDto;

  searchForm: ResTaskCategorySearchForm = {
    code: null,
    businessType: null,
    name: null,
    status: null,
  };

  statusOptions: Array<{ label: string; value: ResTaskCategoryStatus }> = [];
  businessTypeOptions: Array<{ label: string; value: ResTaskCategoryBusinessType }> = [];

  columns: TableColumn[] = [];
  actions: TableAction<ResTaskCategoryDto>[] = [];
  currentLazyLoadEvent?: TableLazyLoadEvent;

  constructor(
    private service: ResTaskCategoryService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private permissionService: PermissionService,
    private localizationService: LocalizationService
  ) {
    this.initializeColumns();
    this.initializeActions();
    this.initializeStatusOptions();
    this.initializeBusinessTypeOptions();
  }

  ngOnInit(): void {}

  private initializeStatusOptions(): void {
    this.statusOptions = [
      {
        label: this.localizationService.localize('Master::ResTaskCategory:Active'),
        value: ResTaskCategoryStatus.Active,
      },
      {
        label: this.localizationService.localize('Master::ResTaskCategory:Deactive'),
        value: ResTaskCategoryStatus.Deactive,
      },
    ];
  }

  private initializeBusinessTypeOptions(): void {
    this.businessTypeOptions = [
      {
        label: this.localizationService.localize('Master::ResTaskCategory:Policy'),
        value: ResTaskCategoryBusinessType.Policy,
      },
      {
        label: this.localizationService.localize('Master::ResTaskCategory:Claim'),
        value: ResTaskCategoryBusinessType.Claim,
      },
      {
        label: this.localizationService.localize('Master::ResTaskCategory:Common'),
        value: ResTaskCategoryBusinessType.Common,
      },
    ];
  }

  private initializeColumns(): void {
    this.columns = [
      {
        field: 'code',
        header: this.localizationService.localize('Master::ResTaskCategory:Code'),
        sortable: true,
        width: '120px',
      },
      {
        field: 'businessType',
        header: this.localizationService.localize('Master::ResTaskCategory:BusinessType'),
        sortable: true,
        width: '140px',
        formatter: (value: unknown) => this.formatBusinessType(value),
      },
      {
        field: 'name',
        header: this.localizationService.localize('Master::ResTaskCategory:Name'),
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
        header: this.localizationService.localize('Master::ResTaskCategory:Status'),
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
              : value === ResTaskCategoryStatus.Active
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

  loadData(event: TableLazyLoadEvent): void {
    this.currentLazyLoadEvent = event;
    this.loading = true;

    const input: GetResTaskCategoriesInput = {
      skipCount: event.first || 0,
      maxResultCount: event.rows || this.pageSize,
      sorting: this.getSortingString(event),
      code: this.searchForm.code || undefined,
      businessType: this.searchForm.businessType ?? undefined,
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

  @HostListener('window:keydown.enter', ['$event'])
  handleEnter(event: KeyboardEvent) {
    // Only trigger search if dialog is not visible
    if (!this.dialogVisible) {
      this.search();
    }
  }

  private getSortingString(event: TableLazyLoadEvent): string | undefined {
    if (!event.sortField) return undefined;
    const sortOrder = event.sortOrder === 1 ? 'asc' : 'desc';
    return `${event.sortField} ${sortOrder}`;
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
      businessType: null,
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

  openEditDialog(item: ResTaskCategoryDto): void {
    this.dialogMode = 'edit';
    this.selectedItem = item;
    this.formData = {
      businessType: item.businessType ?? ResTaskCategoryBusinessType.Policy,
      code: item.code || '',
      name: item.name || '',
      status: item.status ?? ResTaskCategoryStatus.Active,
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
          detail: this.localizationService.localize('Master::ResTaskCategory:CodeRequired'),
        });
        return false;
      }
      if (this.formData.code.length > 50) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: this.localizationService.localize('Master::ResTaskCategory:CodeMaxLength'),
        });
        return false;
      }
      if (!/^[A-Z0-9_]+$/.test(this.formData.code)) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: this.localizationService.localize('Master::ResTaskCategory:CodeInvalid'),
        });
        return false;
      }
      if (this.formData.businessType === undefined || this.formData.businessType === null) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: this.localizationService.localize('Master::ResTaskCategory:BusinessTypeRequired'),
        });
        return false;
      }
    }

    if (!this.formData.name || this.formData.name.trim() === '') {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Master::ResTaskCategory:NameRequired'),
      });
      return false;
    }
    if (this.formData.name.length > 250) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Master::ResTaskCategory:NameMaxLength'),
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
    const createDto: CreateResTaskCategoryDto = {
      businessType: this.formData.businessType,
      code: this.formData.code.trim(),
      name: this.formData.name.trim(),
      status: this.formData.status,
    };

    this.service.create(createDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Master::Success'),
          detail: this.localizationService.localize('Master::ResTaskCategory:CreatedSuccessfully'),
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
    const updateDto: UpdateResTaskCategoryDto = {
      name: this.formData.name.trim(),
      status: this.formData.status,
    };

    this.service.update(this.selectedItem.id, updateDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Master::Success'),
          detail: this.localizationService.localize('Master::ResTaskCategory:UpdatedSuccessfully'),
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

  delete(item: ResTaskCategoryDto): void {
    if (!item.id) return;

    this.confirmationService.confirm({
      message: this.localizationService.localize('Master::ResTaskCategory:DeleteConfirm'),
      header: this.localizationService.localize('Master::ResTaskCategory:Delete'),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.loading = true;
        this.service.delete(item.id!).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: this.localizationService.localize('Master::Success'),
              detail: this.localizationService.localize('Master::ResTaskCategory:DeletedSuccessfully'),
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

  private getEmptyForm(): ResTaskCategoryFormData {
    return {
      businessType: ResTaskCategoryBusinessType.Policy,
      code: '',
      name: '',
      status: ResTaskCategoryStatus.Active,
    };
  }

  formatBusinessType(value: unknown): string {
    if (value === undefined || value === null) return '';
    const v = typeof value === 'number' ? value : Number(value);
    if (v === ResTaskCategoryBusinessType.Policy) return this.localizationService.localize('Master::ResTaskCategory:Policy');
    if (v === ResTaskCategoryBusinessType.Claim) return this.localizationService.localize('Master::ResTaskCategory:Claim');
    if (v === ResTaskCategoryBusinessType.Common) return this.localizationService.localize('Master::ResTaskCategory:Common');
    return '';
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
      statusValue = status === ResTaskCategoryStatus.Active ? 0 : 1;
    }
    return statusValue === 0
      ? this.localizationService.localize('Master::ResTaskCategory:Active')
      : this.localizationService.localize('Master::ResTaskCategory:Deactive');
  }
}
