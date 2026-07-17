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
import { ResBusinessAssigneeService } from '@/proxy/master/controllers/res-business-assignee.service';
import { ResBusinessAuthorityService } from '@/proxy/master/controllers/res-business-authority.service';
import { AdminConfigService } from '@/proxy/master/controllers/admin-config.service';
import { HrDepartmentService } from '@/proxy/hr/controllers/hr-department.service';
import { HrEmployeeService } from '@/proxy/hr/controllers/hr-employee.service';
import { HrEmployeeRoleService } from '@/proxy/hr/controllers/hr-employee-role.service';
import {
  ResBusinessAssigneeDto,
  CreateResBusinessAssigneeDto,
  UpdateResBusinessAssigneeDto,
  GetResBusinessAssigneesInput,
} from '@/proxy/master/res-business-assignees/models';
import {
  ResBusinessAssigneeType,
  ResBusinessAssigneeStatus,
  ResBusinessAssigneeDepartmentLevel,
} from '@/proxy/res-business-assignees';
import { HrDepartmentLevel } from '@/proxy/hr-departments/hr-department-level.enum';
import { ResBusinessAssigneeSearchForm, ResBusinessAssigneeFormData } from './res-business-assignees.models';
import { PermissionPipe } from '@/core/pipes/permission.pipe';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { PermissionService } from '@/core/services/permission.service';
import { LocalizationService } from '@/core/services/localization.service';

@Component({
  selector: 'app-res-business-assignees',
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
  templateUrl: './res-business-assignees.component.html',
  styleUrl: './res-business-assignees.component.scss',
  providers: [MessageService, ConfirmationService],
})
export class ResBusinessAssigneesComponent implements OnInit {
  readonly PERMISSIONS = {
    CREATE: 'MasterResBusinessAssignee.Create',
    UPDATE: 'MasterResBusinessAssignee.Edit',
    DELETE: 'MasterResBusinessAssignee.Delete',
    VIEW: 'MasterResBusinessAssignee.View',
  };

  items: ResBusinessAssigneeDto[] = [];
  totalCount = 0;
  loading = true;
  pageSize = 10;

  dialogVisible = false;
  dialogMode: 'create' | 'edit' = 'create';
  formData: ResBusinessAssigneeFormData = this.getEmptyForm();
  selectedItem?: ResBusinessAssigneeDto;

  effectDatePicker: Date | null = null;
  expireDatePicker: Date | null = null;

  searchForm: ResBusinessAssigneeSearchForm = {
    organizationId: null,
    businessCode: null,
    authorityCode: null,
    assigneeType: null,
    departmentId: null,
    status: null,
    effectDateFrom: null,
    effectDateTo: null,
  };

  businessCodeOptions: Array<{ label: string; value: string }> = [];
  authorityCodeOptions: Array<{ label: string; value: string }> = [];
  organizationOptions: Array<{ label: string; value: string }> = [];
  departmentOptions: Array<{ label: string; value: string }> = [];
  assigneeRoleOptions: Array<{ label: string; value: string }> = [];
  assigneeOptions: Array<{ label: string; value: string }> = [];

  statusOptions: Array<{ label: string; value: ResBusinessAssigneeStatus }> = [];
  assigneeTypeOptions: Array<{ label: string; value: ResBusinessAssigneeType }> = [];
  departmentLevelOptions: Array<{ label: string; value: number }> = [];

  columns: TableColumn[] = [];
  actions: TableAction<ResBusinessAssigneeDto>[] = [];
  currentLazyLoadEvent?: TableLazyLoadEvent;

  constructor(
    private service: ResBusinessAssigneeService,
    private authorityService: ResBusinessAuthorityService,
    private adminConfigService: AdminConfigService,
    private hrDepartmentService: HrDepartmentService,
    private hrEmployeeService: HrEmployeeService,
    private hrEmployeeRoleService: HrEmployeeRoleService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private permissionService: PermissionService,
    private localizationService: LocalizationService
  ) {
    this.initializeColumns();
    this.initializeActions();
    this.initializeStatusOptions();
    this.initializeAssigneeTypeOptions();
    this.initializeDepartmentLevelOptions();
  }

  ngOnInit(): void {
    this.loadBusinessCodeOptions();
    this.loadAuthorityCodeOptions();
    this.loadOrganizationOptions();
    this.loadDepartmentOptions();
    this.loadAssigneeRoleOptions();
  }

  private initializeStatusOptions(): void {
    this.statusOptions = [
      { label: this.localizationService.localize('Master::ResBusinessAssignee:Active'), value: ResBusinessAssigneeStatus.Active },
      { label: this.localizationService.localize('Master::ResBusinessAssignee:Deactive'), value: ResBusinessAssigneeStatus.Deactive },
    ];
  }

  private initializeAssigneeTypeOptions(): void {
    this.assigneeTypeOptions = [
      { label: this.localizationService.localize('Master::ResBusinessAssignee:AssigneeTypeEmp'), value: ResBusinessAssigneeType.Emp },
      { label: this.localizationService.localize('Master::ResBusinessAssignee:AssigneeTypeRole'), value: ResBusinessAssigneeType.Role },
      { label: this.localizationService.localize('Master::ResBusinessAssignee:AssigneeTypeSystem'), value: ResBusinessAssigneeType.System },
    ];
  }

  private initializeDepartmentLevelOptions(): void {
    this.departmentLevelOptions = [
      { label: this.localizationService.localize('Master::ResBusinessAssignee:DepartmentLevelIn'), value: ResBusinessAssigneeDepartmentLevel.In },
      { label: this.localizationService.localize('Master::ResBusinessAssignee:DepartmentLevelOut'), value: ResBusinessAssigneeDepartmentLevel.Out },
    ];
  }

  private initializeColumns(): void {
    this.columns = [
      { field: 'organizationId', header: this.localizationService.localize('Master::ResBusinessAssignee:OrganizationId'), sortable: true, width: '180px', formatter: (value: unknown, row?: ResBusinessAssigneeDto) => this.getOrganizationName(value as string) },
      { field: 'businessCode', header: this.localizationService.localize('Master::ResBusinessAssignee:BusinessCode'), sortable: true, width: '130px' },
      { field: 'authorityCode', header: this.localizationService.localize('Master::ResBusinessAssignee:AuthorityCode'), sortable: true, width: '130px' },
      {
        field: 'assigneeType',
        header: this.localizationService.localize('Master::ResBusinessAssignee:AssigneeType'),
        sortable: true,
        width: '120px',
        formatter: (value: unknown) => this.formatAssigneeType(value),
      },
      { field: 'effectDate', header: this.localizationService.localize('Master::ResBusinessAssignee:EffectDate'), sortable: true, type: 'date', width: '120px' },
      { field: 'expireDate', header: this.localizationService.localize('Master::ResBusinessAssignee:ExpireDate'), sortable: true, type: 'date', width: '120px' },
      { field: 'assigneeRole', header: this.localizationService.localize('Master::ResBusinessAssignee:AssigneeRole'), sortable: true, width: '120px' },
      { field: 'departmentId', header: this.localizationService.localize('Master::ResBusinessAssignee:DepartmentId'), sortable: true, width: '150px' },
      {
        field: 'departmentLevel',
        header: this.localizationService.localize('Master::ResBusinessAssignee:DepartmentLevel'),
        sortable: true,
        width: '110px',
        formatter: (value: unknown) => this.formatDepartmentLevel(value),
      },
      {
        field: 'status',
        header: this.localizationService.localize('Master::ResBusinessAssignee:Status'),
        sortable: true,
        type: 'text',
        width: '100px',
        freeze: 'right',
        align: 'center',
        formatter: (value: unknown) => this.formatStatus(value),
        cellClass: (value: unknown) => {
          const v = typeof value === 'number' ? value : value === ResBusinessAssigneeStatus.Active ? 0 : 1;
          return v === 0 ? 'px-2 py-1 rounded text-xs font-semibold bg-green-100 text-green-800' : 'px-2 py-1 rounded text-xs font-semibold bg-red-100 text-red-800';
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
    this.adminConfigService.getList({ code: 'BUSINESS_CODE', skipCount: 0, maxResultCount: 1000 }).subscribe({
      next: (result) => {
        const items = result.items || [];
        this.businessCodeOptions = items.map((item) => ({ label: item.value || item.subCode || '', value: item.subCode || '' }));
      },
      error: (err) => this.showError(err),
    });
  }

  loadAuthorityCodeOptions(): void {
    this.authorityService.getList({ skipCount: 0, maxResultCount: 1000 }).subscribe({
      next: (result) => {
        const items = result.items || [];
        this.authorityCodeOptions = items.map((item) => ({ label: item.name || item.code || '', value: item.code || '' }));
      },
      error: (err) => this.showError(err),
    });
  }

  loadOrganizationOptions(): void {
    this.hrDepartmentService.getList({ deptLevel: HrDepartmentLevel.Unit, skipCount: 0, maxResultCount: 1000 }).subscribe({
      next: (result) => {
        const items = result.items || [];
        this.organizationOptions = items.map((item) => ({ label: item.name || item.code || '', value: item.id || '' }));
      },
      error: (err) => this.showError(err),
    });
  }

  loadDepartmentOptions(): void {
    this.hrDepartmentService.getList({ skipCount: 0, maxResultCount: 1000 }).subscribe({
      next: (result) => {
        const items = result.items || [];
        this.departmentOptions = items.map((item) => ({ label: item.name || item.code || '', value: item.id || '' }));
      },
      error: (err) => this.showError(err),
    });
  }

  loadAssigneeRoleOptions(): void {
    this.hrEmployeeRoleService.getList({ skipCount: 0, maxResultCount: 500 }).subscribe({
      next: (result) => {
        const items = result.items || [];
        this.assigneeRoleOptions = items.map((item) => ({ label: item.name || item.code || '', value: item.code || '' }));
      },
      error: (err) => this.showError(err),
    });
  }

  loadAssigneeOptions(): void {
    const departmentId = this.formData.departmentId || undefined;
    this.hrEmployeeService.getList({ departmentId, skipCount: 0, maxResultCount: 500 }).subscribe({
      next: (result) => {
        const items = result.items || [];
        this.assigneeOptions = items.map((item) => ({ label: item.fullName || item.code || '', value: item.id || '' }));
      },
      error: (err) => this.showError(err),
    });
  }

  onDepartmentIdChange(): void {
    this.formData.assigneeId = null;
    this.loadAssigneeOptions();
  }

  loadData(event: TableLazyLoadEvent): void {
    this.currentLazyLoadEvent = event;
    this.loading = true;
    const input: GetResBusinessAssigneesInput = {
      skipCount: event.first || 0,
      maxResultCount: event.rows || this.pageSize,
      sorting: this.getSortingString(event),
      organizationId: this.searchForm.organizationId || undefined,
      businessCode: this.searchForm.businessCode || undefined,
      authorityCode: this.searchForm.authorityCode || undefined,
      assigneeType: this.searchForm.assigneeType ?? undefined,
      departmentId: this.searchForm.departmentId || undefined,
      status: this.searchForm.status ?? undefined,
      effectDateFrom: this.searchForm.effectDateFrom || undefined,
      effectDateTo: this.searchForm.effectDateTo || undefined,
    };
    this.service.getList(input).subscribe({
      next: (result) => {
        this.items = result.items || [];
        this.totalCount = result.totalCount || 0;
        this.loading = false;
      },
      error: (err) => {
        this.showError(err);
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
    return `${event.sortField} ${event.sortOrder === 1 ? 'asc' : 'desc'}`;
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
      businessCode: null,
      authorityCode: null,
      assigneeType: null,
      departmentId: null,
      status: null,
      effectDateFrom: null,
      effectDateTo: null,
    };
    this.search();
  }

  openCreateDialog(): void {
    this.dialogMode = 'create';
    this.formData = this.getEmptyForm();
    this.selectedItem = undefined;
    this.effectDatePicker = null;
    this.expireDatePicker = null;
    this.loadAssigneeOptions();
    this.dialogVisible = true;
  }

  openEditDialog(item: ResBusinessAssigneeDto): void {
    this.dialogMode = 'edit';
    this.selectedItem = item;
    const assigneeType = (item.assigneeType as ResBusinessAssigneeType | number | undefined) ?? ResBusinessAssigneeType.Emp;
    this.formData = {
      organizationId: item.organizationId ?? null,
      businessCode: item.businessCode ?? '',
      authorityCode: item.authorityCode ?? '',
      assigneeType: assigneeType as ResBusinessAssigneeType,
      effectDate: item.effectDate ?? '',
      expireDate: item.expireDate ?? null,
      assigneeRole: item.assigneeRole ?? null,
      assigneeId: item.assigneeId ?? null,
      departmentId: item.departmentId ?? null,
      departmentLevel: item.departmentLevel != null ? (item.departmentLevel as number) : null,
      status: (item.status ?? ResBusinessAssigneeStatus.Active) as ResBusinessAssigneeStatus,
    };
    this.effectDatePicker = this.toDate(item.effectDate ?? '');
    this.expireDatePicker = this.toDate(item.expireDate ?? '');
    this.loadAssigneeOptions();
    this.dialogVisible = true;
  }

  save(): void {
    this.syncDatePickersToForm();
    if (!this.validateForm()) return;
    if (this.dialogMode === 'create') this.create();
    else this.update();
  }

  private syncDatePickersToForm(): void {
    this.formData.effectDate = this.fromDate(this.effectDatePicker);
    this.formData.expireDate = this.expireDatePicker ? this.fromDate(this.expireDatePicker) : null;
  }

  private validateForm(): boolean {
    if (this.dialogMode === 'create') {
      if (!this.formData.businessCode?.trim()) {
        this.showValidationError('Master::ResBusinessAssignee:BusinessCodeRequired');
        return false;
      }
      if (!this.formData.authorityCode?.trim()) {
        this.showValidationError('Master::ResBusinessAssignee:AuthorityCodeRequired');
        return false;
      }
      if (this.formData.assigneeType == null) {
        this.showValidationError('Master::ResBusinessAssignee:AssigneeTypeRequired');
        return false;
      }
      if (!this.effectDatePicker) {
        this.showValidationError('Master::ResBusinessAssignee:EffectDateRequired');
        return false;
      }
    }
    if (this.dialogMode === 'edit') {
      if (!this.selectedItem?.id) return false;
    }
    return true;
  }

  private create(): void {
    this.loading = true;
    const dto: CreateResBusinessAssigneeDto = {
      organizationId: this.formData.organizationId || undefined,
      businessCode: this.formData.businessCode.trim(),
      authorityCode: this.formData.authorityCode.trim(),
      assigneeType: this.formData.assigneeType,
      effectDate: this.formData.effectDate,
      expireDate: this.formData.expireDate || undefined,
      assigneeRole: this.formData.assigneeRole || undefined,
      assigneeId: this.formData.assigneeId || undefined,
      departmentId: this.formData.departmentId || undefined,
      departmentLevel: this.formData.departmentLevel != null ? (this.formData.departmentLevel as 0 | 1) : undefined,
      status: this.formData.status,
    };
    this.service.create(dto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Master::Success'),
          detail: this.localizationService.localize('Master::ResBusinessAssignee:CreatedSuccessfully'),
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
    const dto: UpdateResBusinessAssigneeDto = {
      assigneeId: this.formData.assigneeId || undefined,
      expireDate: this.formData.expireDate || undefined,
    };
    this.service.update(this.selectedItem.id, dto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Master::Success'),
          detail: this.localizationService.localize('Master::ResBusinessAssignee:UpdatedSuccessfully'),
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

  delete(item: ResBusinessAssigneeDto): void {
    if (!item.id) return;
    this.confirmationService.confirm({
      message: this.localizationService.localize('Master::ResBusinessAssignee:DeleteConfirm'),
      header: this.localizationService.localize('Master::ResBusinessAssignee:Delete'),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.loading = true;
        this.service.delete(item.id!).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: this.localizationService.localize('Master::Success'),
              detail: this.localizationService.localize('Master::ResBusinessAssignee:DeletedSuccessfully'),
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

  private getEmptyForm(): ResBusinessAssigneeFormData {
    return {
      organizationId: null,
      businessCode: '',
      authorityCode: '',
      assigneeType: ResBusinessAssigneeType.Emp,
      effectDate: '',
      expireDate: null,
      assigneeRole: null,
      assigneeId: null,
      departmentId: null,
      departmentLevel: null,
      status: ResBusinessAssigneeStatus.Active,
    };
  }

  formatStatus(status: unknown): string {
    if (status === undefined || status === null) return '';
    const v = typeof status === 'number' ? status : status === ResBusinessAssigneeStatus.Active || status === 'Active' || status === 'active' ? 0 : 1;
    return v === 0 ? this.localizationService.localize('Master::ResBusinessAssignee:Active') : this.localizationService.localize('Master::ResBusinessAssignee:Deactive');
  }

  formatAssigneeType(value: unknown): string {
    if (value === undefined || value === null) return '';
    if (value === ResBusinessAssigneeType.Emp || value === 0) return this.localizationService.localize('Master::ResBusinessAssignee:AssigneeTypeEmp');
    if (value === ResBusinessAssigneeType.Role || value === 1) return this.localizationService.localize('Master::ResBusinessAssignee:AssigneeTypeRole');
    if (value === ResBusinessAssigneeType.System || value === 2) return this.localizationService.localize('Master::ResBusinessAssignee:AssigneeTypeSystem');
    return '';
  }

  formatDepartmentLevel(value: unknown): string {
    if (value === undefined || value === null) return '';
    if (value === ResBusinessAssigneeDepartmentLevel.In || value === 0) return this.localizationService.localize('Master::ResBusinessAssignee:DepartmentLevelIn');
    if (value === ResBusinessAssigneeDepartmentLevel.Out || value === 1) return this.localizationService.localize('Master::ResBusinessAssignee:DepartmentLevelOut');
    return '';
  }

  toDate(value: string | undefined | null): Date | null {
    if (!value) return null;
    const d = new Date(value);
    return isNaN(d.getTime()) ? null : d;
  }

  fromDate(date: Date | null): string {
    if (!date) return '';
    return date.toISOString().split('T')[0];
  }

  private showError(err: unknown): void {
    const msg = (err as { error?: { error?: { message?: string; details?: string } } })?.error?.error?.message || (err as { error?: { error?: { details?: string } } })?.error?.error?.details || this.localizationService.localize('AbpUi::InternalServerErrorMessage');
    this.messageService.add({ severity: 'error', summary: this.localizationService.localize('AbpUi::Error'), detail: msg });
  }

  private showValidationError(key: string): void {
    this.messageService.add({
      severity: 'error',
      summary: this.localizationService.localize('AbpUi::Error'),
      detail: this.localizationService.localize(key),
    });
  }

  getOrganizationName(organizationId: string | undefined | null): string {
    if (!organizationId) return '';
    const opt = this.organizationOptions.find((o) => o.value === organizationId);
    return opt?.label ?? organizationId;
  }

  getDepartmentName(departmentId: string | undefined | null): string {
    if (!departmentId) return '';
    const opt = this.departmentOptions.find((o) => o.value === departmentId);
    return opt?.label ?? departmentId;
  }

  get displayBusinessCode(): string {
    return this.selectedItem?.businessCode ?? '';
  }
  get displayOrganizationName(): string {
    return this.getOrganizationName(this.selectedItem?.organizationId) || (this.selectedItem?.organizationId ?? '');
  }
  get displayDepartmentLevel(): string {
    return this.formatDepartmentLevel(this.selectedItem?.departmentLevel);
  }
  get displayDepartmentName(): string {
    return this.getDepartmentName(this.selectedItem?.departmentId) || (this.selectedItem?.departmentId ?? '');
  }
  get displayAssigneeRole(): string {
    return this.selectedItem?.assigneeRole ?? '';
  }
  get displayStatus(): string {
    return this.formatStatus(this.selectedItem?.status);
  }
  get isAssigneeTypeEmp(): boolean {
    const t = this.selectedItem?.assigneeType as ResBusinessAssigneeType | number | undefined;
    return t === ResBusinessAssigneeType.Emp || t === 0;
  }
  get isAssigneeTypeEmpOrRole(): boolean {
    const t = this.selectedItem?.assigneeType as ResBusinessAssigneeType | number | undefined;
    return t === ResBusinessAssigneeType.Emp || t === ResBusinessAssigneeType.Role || t === 0 || t === 1;
  }
  get displayAuthorityCode(): string {
    return this.selectedItem?.authorityCode ?? '';
  }
  get displayAssigneeType(): string {
    return this.formatAssigneeType(this.selectedItem?.assigneeType);
  }
  get displayEffectDate(): string {
    return this.selectedItem?.effectDate ?? '';
  }
  get displayExpireDate(): string {
    return this.selectedItem?.expireDate ?? '';
  }
}
