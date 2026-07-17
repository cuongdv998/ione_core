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
import { DatePickerModule } from 'primeng/datepicker';

import { VTable } from '@/shared/components/v-table/v-table';
import { TableColumn } from '@/shared/models/table-column.model';
import { TableAction } from '@/shared/models/table-action.model';
import { BusinessFlowService } from '@/proxy/master/controllers/business-flow.service';
import { AdminConfigService } from '@/proxy/master/controllers/admin-config.service';
import { HrDepartmentService } from '@/proxy/hr/controllers/hr-department.service';
import { ResPartnerService } from '@/proxy/partner/controllers/res-partner.service';
import {
  BusinessFlowDto,
  CreateBusinessFlowDto,
  UpdateBusinessFlowDto,
  GetBusinessFlowsInput,
} from '@/proxy/master/business-flows/models';
import { BusinessFlowStatus } from '@/proxy/business-flows/business-flow-status.enum';
import { HrDepartmentLevel } from '@/proxy/hr-departments/hr-department-level.enum';
import { BusinessFlowSearchForm, BusinessFlowFormData } from './business-flows.models';
import { PermissionPipe } from '@/core/pipes/permission.pipe';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { PermissionService } from '@/core/services/permission.service';
import { LocalizationService } from '@/core/services/localization.service';

@Component({
  selector: 'app-business-flows',
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
    DatePickerModule,
    VTable,
    PermissionPipe,
    TranslatePipe,
  ],
  templateUrl: './business-flows.component.html',
  styleUrl: './business-flows.component.scss',
  providers: [MessageService, ConfirmationService],
})
export class BusinessFlowsComponent implements OnInit {
  readonly PERMISSIONS = {
    CREATE: 'MasterBusinessFlow.Create',
    UPDATE: 'MasterBusinessFlow.Edit',
    DELETE: 'MasterBusinessFlow.Delete',
    VIEW: 'MasterBusinessFlow.View',
  };

  items: BusinessFlowDto[] = [];
  totalCount = 0;
  loading = true;
  pageSize = 10;

  dialogVisible = false;
  dialogMode: 'create' | 'edit' = 'create';
  formData: BusinessFlowFormData = this.getEmptyForm();
  selectedItem?: BusinessFlowDto;

  effectDatePicker: Date | null = null;
  expireDatePicker: Date | null = null;
  effectDateFromPicker: Date | null = null;
  effectDateToPicker: Date | null = null;
  expireDateFromPicker: Date | null = null;
  expireDateToPicker: Date | null = null;

  searchForm: BusinessFlowSearchForm = {
    organizationId: null,
    insurerId: null,
    businessCode: null,
    workflowName: null,
    workflowVersion: null,
    status: null,
    effectDateFrom: null,
    effectDateTo: null,
    expireDateFrom: null,
    expireDateTo: null,
  };

  businessCodeOptions: Array<{ label: string; value: string }> = [];
  organizationOptions: Array<{ label: string; value: string }> = [];
  insurerOptions: Array<{ label: string; value: string }> = [];
  statusOptions: Array<{ label: string; value: BusinessFlowStatus }> = [];

  columns: TableColumn[] = [];
  actions: TableAction<BusinessFlowDto>[] = [];
  currentLazyLoadEvent?: TableLazyLoadEvent;

  constructor(
    private service: BusinessFlowService,
    private adminConfigService: AdminConfigService,
    private hrDepartmentService: HrDepartmentService,
    private resPartnerService: ResPartnerService,
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
    this.loadOrganizationOptions();
    this.loadInsurerPartnerOptions();
  }

  private initializeStatusOptions(): void {
    this.statusOptions = [
      {
        label: this.localizationService.localize('Master::BusinessFlow:Active'),
        value: BusinessFlowStatus.Active,
      },
      {
        label: this.localizationService.localize('Master::BusinessFlow:Deactive'),
        value: BusinessFlowStatus.Deactive,
      },
    ];
  }

  private initializeColumns(): void {
    this.columns = [
      {
        field: 'organizationId',
        header: this.localizationService.localize('Master::BusinessFlow:OrganizationId'),
        sortable: false,
        width: '140px',
        formatter: (value: unknown, row: BusinessFlowDto) => {
          const id = (value ?? row.organizationId) as string | undefined;
          if (!id) return '';
          const opt = this.organizationOptions.find((o) => o.value === id);
          return opt ? opt.label : id;
        },
      },
      {
        field: 'businessCode',
        header: this.localizationService.localize('Master::BusinessFlow:BusinessCode'),
        sortable: true,
        width: '140px',
        formatter: (value: unknown, row: BusinessFlowDto) => {
          const code = (value ?? row.businessCode) as string | undefined;
          if (!code) return '';
          const opt = this.businessCodeOptions.find((o) => o.value === code);
          return opt ? opt.label : code;
        },
      },
      {
        field: 'workflowName',
        header: this.localizationService.localize('Master::BusinessFlow:WorkflowName'),
        sortable: true,
        width: '180px',
      },
      {
        field: 'workflowVersion',
        header: this.localizationService.localize('Master::BusinessFlow:WorkflowVersion'),
        sortable: true,
        width: '120px',
      },
      {
        field: 'effectDate',
        header: this.localizationService.localize('Master::BusinessFlow:EffectDate'),
        sortable: true,
        type: 'date',
        width: '120px',
      },
      {
        field: 'expireDate',
        header: this.localizationService.localize('Master::BusinessFlow:ExpireDate'),
        sortable: true,
        type: 'date',
        width: '120px',
      },
      {
        field: 'status',
        header: this.localizationService.localize('Master::BusinessFlow:Status'),
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
              : value === BusinessFlowStatus.Active || value === 'Active' || value === 'active'
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
      .getList({ code: 'BUSINESS_CODE', skipCount: 0, maxResultCount: 1000 })
      .subscribe({
        next: (result) => {
          const items = result.items || [];
          this.businessCodeOptions = items.map((item) => ({
            label: item.value || item.subCode || '',
            value: item.subCode || '',
          }));
        },
        error: (err) => this.showError(err),
      });
  }

  loadOrganizationOptions(): void {
    this.hrDepartmentService
      .getList({ deptLevel: HrDepartmentLevel.Unit, skipCount: 0, maxResultCount: 1000 })
      .subscribe({
        next: (result) => {
          const items = result.items || [];
          this.organizationOptions = items.map((item) => ({
            label: item.name || item.code || '',
            value: item.id || '',
          }));
        },
        error: (err) => this.showError(err),
      });
  }

  /** Công ty bảo hiểm: res_partner với partner_type_id tương ứng res_partner_type có mã INSURER */
  loadInsurerPartnerOptions(): void {
    this.resPartnerService.getSelectList('INSURER').subscribe({
      next: (items) => {
        this.insurerOptions = (items || []).map((item) => ({
          label: item.name || item.code || '',
          value: item.id || '',
        }));
      },
      error: (err) => this.showError(err),
    });
  }

  loadData(event: TableLazyLoadEvent): void {
    this.currentLazyLoadEvent = event;
    this.loading = true;

    const input: GetBusinessFlowsInput = {
      skipCount: event.first || 0,
      maxResultCount: event.rows || this.pageSize,
      sorting: this.getSortingString(event),
      organizationId: this.searchForm.organizationId || undefined,
      insurerId: this.searchForm.insurerId || undefined,
      businessCode: this.searchForm.businessCode || undefined,
      workflowName: this.searchForm.workflowName || undefined,
      workflowVersion: this.searchForm.workflowVersion || undefined,
      status: this.searchForm.status ?? undefined,
      effectDateFrom: this.searchForm.effectDateFrom ? this.toISOString(this.searchForm.effectDateFrom) : undefined,
      effectDateTo: this.searchForm.effectDateTo ? this.toISOString(this.searchForm.effectDateTo) : undefined,
      expireDateFrom: this.searchForm.expireDateFrom ? this.toISOString(this.searchForm.expireDateFrom) : undefined,
      expireDateTo: this.searchForm.expireDateTo ? this.toISOString(this.searchForm.expireDateTo) : undefined,
    };

    this.service.getList(input).subscribe({
      next: (result) => {
        this.items = result.items || [];
        this.totalCount = result.totalCount || 0;
        this.effectDateFromPicker = this.searchForm.effectDateFrom;
        this.effectDateToPicker = this.searchForm.effectDateTo;
        this.expireDateFromPicker = this.searchForm.expireDateFrom;
        this.expireDateToPicker = this.searchForm.expireDateTo;
        this.loading = false;
      },
      error: (err) => {
        this.showError(err);
        this.loading = false;
      },
    });
  }

  private toISOString(d: Date): string {
    return d.toISOString().split('T')[0];
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
      organizationId: null,
      insurerId: null,
      businessCode: null,
      workflowName: null,
      workflowVersion: null,
      status: null,
      effectDateFrom: null,
      effectDateTo: null,
      expireDateFrom: null,
      expireDateTo: null,
    };
    this.effectDateFromPicker = null;
    this.effectDateToPicker = null;
    this.expireDateFromPicker = null;
    this.expireDateToPicker = null;
    this.search();
  }

  openCreateDialog(): void {
    this.dialogMode = 'create';
    this.formData = this.getEmptyForm();
    this.effectDatePicker = null;
    this.expireDatePicker = null;
    this.selectedItem = undefined;
    this.dialogVisible = true;
  }

  openEditDialog(item: BusinessFlowDto): void {
    this.dialogMode = 'edit';
    this.selectedItem = item;
    this.formData = {
      organizationId: item.organizationId ?? null,
      insurerId: item.insurerId ?? null,
      businessCode: item.businessCode || '',
      workflowName: item.workflowName || '',
      workflowVersion: item.workflowVersion || '',
      effectDate: item.effectDate ? new Date(item.effectDate) : null,
      expireDate: item.expireDate ? new Date(item.expireDate) : null,
    };
    this.effectDatePicker = this.formData.effectDate;
    this.expireDatePicker = this.formData.expireDate;
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
      if (!this.formData.businessCode?.trim()) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: this.localizationService.localize('Master::BusinessFlow:BusinessCodeRequired'),
        });
        return false;
      }
      if (!this.effectDatePicker) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: this.localizationService.localize('Master::BusinessFlow:EffectDateRequired'),
        });
        return false;
      }
    }

    if (!this.formData.workflowName?.trim()) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Master::BusinessFlow:WorkflowNameRequired'),
      });
      return false;
    }
    if (this.formData.workflowName.length > 50) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Master::BusinessFlow:WorkflowNameMaxLength'),
      });
      return false;
    }
    if (!this.formData.workflowVersion?.trim()) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Master::BusinessFlow:WorkflowVersionRequired'),
      });
      return false;
    }
    if (this.formData.workflowVersion.length > 50) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Master::BusinessFlow:WorkflowVersionMaxLength'),
      });
      return false;
    }
    return true;
  }

  private create(): void {
    this.loading = true;
    const createDto: CreateBusinessFlowDto = {
      organizationId: this.formData.organizationId || undefined,
      insurerId: this.formData.insurerId || undefined,
      businessCode: this.formData.businessCode.trim(),
      workflowName: this.formData.workflowName.trim(),
      workflowVersion: this.formData.workflowVersion.trim(),
      effectDate: this.effectDatePicker ? this.toISOString(this.effectDatePicker) : '',
      expireDate: this.expireDatePicker ? this.toISOString(this.expireDatePicker) : undefined,
    };

    this.service.create(createDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Master::Success'),
          detail: this.localizationService.localize('Master::BusinessFlow:CreatedSuccessfully'),
        });
        this.dialogVisible = false;
        this.loading = false;
        this.search();
      },
      error: (err) => {
        this.showError(err);
        this.loading = false;
      },
    });
  }

  private update(): void {
    if (!this.selectedItem?.id) return;

    this.loading = true;
    const updateDto: UpdateBusinessFlowDto = {
      workflowName: this.formData.workflowName.trim(),
      workflowVersion: this.formData.workflowVersion.trim(),
      expireDate: this.expireDatePicker ? this.toISOString(this.expireDatePicker) : undefined,
    };

    this.service.update(this.selectedItem.id, updateDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Master::Success'),
          detail: this.localizationService.localize('Master::BusinessFlow:UpdatedSuccessfully'),
        });
        this.dialogVisible = false;
        this.loading = false;
        this.search();
      },
      error: (err) => {
        this.showError(err);
        this.loading = false;
      },
    });
  }

  delete(item: BusinessFlowDto): void {
    if (!item.id) return;

    this.confirmationService.confirm({
      message: this.localizationService.localize('Master::BusinessFlow:DeleteConfirm'),
      header: this.localizationService.localize('Master::BusinessFlow:Delete'),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.loading = true;
        this.service.delete(item.id!).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: this.localizationService.localize('Master::Success'),
              detail: this.localizationService.localize('Master::BusinessFlow:DeletedSuccessfully'),
            });
            this.loading = false;
            this.search();
          },
          error: (err) => {
            this.showError(err);
            this.loading = false;
          },
        });
      },
    });
  }

  private showError(err: { error?: { error?: { message?: string; details?: string } } }): void {
    const errorMessage =
      err.error?.error?.message ||
      err.error?.error?.details ||
      this.localizationService.localize('AbpUi::InternalServerErrorMessage');
    this.messageService.add({
      severity: 'error',
      summary: this.localizationService.localize('AbpUi::Error'),
      detail: errorMessage,
    });
  }

  private getEmptyForm(): BusinessFlowFormData {
    return {
      organizationId: null,
      insurerId: null,
      businessCode: '',
      workflowName: '',
      workflowVersion: '',
      effectDate: null,
      expireDate: null,
    };
  }

  formatStatus(status: unknown): string {
    if (status === undefined || status === null) return '';

    let statusValue: number;
    if (typeof status === 'number') {
      statusValue = status;
    } else if (typeof status === 'string') {
      statusValue = status === 'Active' || status === 'active' || status === '0' ? 0 : 1;
    } else {
      statusValue = status === BusinessFlowStatus.Active ? 0 : 1;
    }
    return statusValue === 0
      ? this.localizationService.localize('Master::BusinessFlow:Active')
      : this.localizationService.localize('Master::BusinessFlow:Deactive');
  }

  onEffectDateSelect(): void {
    this.formData.effectDate = this.effectDatePicker;
  }

  onExpireDateSelect(): void {
    this.formData.expireDate = this.expireDatePicker;
  }
}
