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
import { TooltipModule } from 'primeng/tooltip';

import { VTable } from '@/shared/components/v-table/v-table';
import { TableColumn } from '@/shared/models/table-column.model';
import { TableAction } from '@/shared/models/table-action.model';
import { ResUserDeviceService } from '@/proxy/master/controllers/res-user-device.service';
import {
  ResUserDeviceDto,
  CreateResUserDeviceDto,
  UpdateResUserDeviceDto,
  GetResUserDevicesInput,
} from '@/proxy/master/res-user-devices/models';
import { ResUserDeviceStatus } from '@/proxy/res-user-devices/res-user-device-status.enum';
import { UserDeviceSearchForm, UserDeviceFormData } from './user-devices.models';
import { PermissionPipe } from '@/core/pipes/permission.pipe';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { PermissionService } from '@/core/services/permission.service';
import { LocalizationService } from '@/core/services/localization.service';

@Component({
  selector: 'app-user-devices',
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
    TooltipModule,
    VTable,
    PermissionPipe,
    TranslatePipe,
  ],
  templateUrl: './user-devices.component.html',
  styleUrl: './user-devices.component.scss',
  providers: [MessageService, ConfirmationService],
})
export class UserDevicesComponent implements OnInit {
  readonly PERMISSIONS = {
    CREATE: 'MasterResUserDevice.Create',
    UPDATE: 'MasterResUserDevice.Edit',
    DELETE: 'MasterResUserDevice.Delete',
    VIEW: 'MasterResUserDevice.View',
  };

  devices: ResUserDeviceDto[] = [];
  totalCount = 0;
  loading = true;
  pageSize = 10;

  dialogVisible = false;
  dialogMode: 'create' | 'edit' = 'create';
  formData: UserDeviceFormData = this.getEmptyForm();
  selectedDevice?: ResUserDeviceDto;

  /** Date object cho datepicker - tránh vòng lặp change detection khi dùng [ngModel]="toDate(...)" */
  effectDatePicker: Date | null = null;
  expirDatePicker: Date | null = null;

  searchForm: UserDeviceSearchForm = {
    userName: null,
    deviceUid: null,
    appChannelCode: null,
    status: null,
  };

  statusOptions: Array<{ label: string; value: ResUserDeviceStatus }> = [];
  columns: TableColumn[] = [];
  actions: TableAction<ResUserDeviceDto>[] = [];
  currentLazyLoadEvent?: TableLazyLoadEvent;

  constructor(
    private userDeviceService: ResUserDeviceService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private permissionService: PermissionService,
    private localizationService: LocalizationService
  ) {
    this.initializeColumns();
    this.initializeActions();
    this.initializeStatusOptions();
  }

  ngOnInit(): void {}

  private initializeStatusOptions(): void {
    this.statusOptions = [
      {
        label: this.localizationService.localize('Master::ResUserDevice:Active'),
        value: ResUserDeviceStatus.Active,
      },
      {
        label: this.localizationService.localize('Master::ResUserDevice:Deactive'),
        value: ResUserDeviceStatus.Deactive,
      },
    ];
  }

  private initializeColumns(): void {
    this.columns = [
      {
        field: 'userName',
        header: this.localizationService.localize('Master::ResUserDevice:UserName'),
        sortable: true,
        width: '140px',
      },
      {
        field: 'deviceUid',
        header: this.localizationService.localize('Master::ResUserDevice:DeviceUid'),
        sortable: true,
        width: '200px',
      },
      {
        field: 'deviceToken',
        header: this.localizationService.localize('Master::ResUserDevice:DeviceToken'),
        sortable: false,
        width: '200px',
      },
      {
        field: 'appChannelCode',
        header: this.localizationService.localize('Master::ResUserDevice:AppChannelCode'),
        sortable: true,
        width: '140px',
      },
      {
        field: 'effectDate',
        header: this.localizationService.localize('Master::ResUserDevice:EffectDate'),
        sortable: true,
        type: 'date',
        width: '120px',
      },
      {
        field: 'expirDate',
        header: this.localizationService.localize('Master::ResUserDevice:ExpirDate'),
        sortable: true,
        type: 'date',
        width: '120px',
      },
      {
        field: 'status',
        header: this.localizationService.localize('Master::ResUserDevice:Status'),
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
              : value === ResUserDeviceStatus.Active
                ? 0
                : 1;
          return statusValue === 0
            ? 'px-2 py-1 rounded text-xs font-semibold bg-green-100 text-green-800'
            : 'px-2 py-1 rounded text-xs font-semibold bg-red-100 text-red-800';
        },
      },
      {
        field: 'os',
        header: this.localizationService.localize('Master::ResUserDevice:Os'),
        sortable: false,
        width: '100px',
      },
      {
        field: 'deviceName',
        header: this.localizationService.localize('Master::ResUserDevice:DeviceName'),
        sortable: false,
        width: '150px',
      },
      {
        field: 'creationTime',
        header: this.localizationService.localize('AbpIdentity::CreationTime'),
        sortable: true,
        type: 'date',
        width: '180px',
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

    const input: GetResUserDevicesInput = {
      skipCount: event.first || 0,
      maxResultCount: event.rows || this.pageSize,
      sorting: this.getSortingString(event),
      userName: this.searchForm.userName || undefined,
      deviceUid: this.searchForm.deviceUid || undefined,
      appChannelCode: this.searchForm.appChannelCode || undefined,
      status: this.searchForm.status ?? undefined,
    };

    this.userDeviceService.getList(input).subscribe({
      next: (result) => {
        this.devices = result.items || [];
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

  search(): void {
    if (this.currentLazyLoadEvent) {
      this.currentLazyLoadEvent.first = 0;
      this.loadData(this.currentLazyLoadEvent);
    }
  }

  resetSearch(): void {
    this.searchForm = {
      userName: null,
      deviceUid: null,
      appChannelCode: null,
      status: null,
    };
    this.search();
  }

  openCreateDialog(): void {
    this.dialogMode = 'create';
    this.formData = this.getEmptyForm();
    this.selectedDevice = undefined;
    this.effectDatePicker = null;
    this.expirDatePicker = null;
    this.dialogVisible = true;
  }

  /**
   * Khi sửa: UserName, DeviceUid, AppChannelCode, EffectDate không được phép sửa (disable trên form)
   */
  openEditDialog(device: ResUserDeviceDto): void {
    this.dialogMode = 'edit';
    this.selectedDevice = device;
    this.formData = {
      userName: device.userName || '',
      deviceUid: device.deviceUid || '',
      deviceToken: device.deviceToken || '',
      appChannelCode: device.appChannelCode || '',
      effectDate: device.effectDate || '',
      expirDate: device.expirDate || null,
      status: device.status ?? ResUserDeviceStatus.Active,
      os: device.os || null,
      deviceName: device.deviceName || null,
    };
    this.effectDatePicker = this.toDate(device.effectDate ?? '');
    this.expirDatePicker = this.toDate(device.expirDate ?? '');
    this.dialogVisible = true;
  }

  save(): void {
    this.syncDatePickersToForm();
    if (!this.validateForm()) {
      return;
    }
    if (this.dialogMode === 'create') {
      this.create();
    } else {
      this.update();
    }
  }

  /** Đồng bộ giá trị từ datepicker (Date) sang formData (string) trước khi validate/save */
  private syncDatePickersToForm(): void {
    this.formData.effectDate = this.fromDate(this.effectDatePicker);
    this.formData.expirDate = this.expirDatePicker ? this.fromDate(this.expirDatePicker) : null;
  }

  private validateForm(): boolean {
    if (this.dialogMode === 'create') {
      if (!this.formData.userName?.trim()) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: this.localizationService.localize('Master::ResUserDevice:UserNameRequired'),
        });
        return false;
      }
      if (this.formData.userName.length > 50) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: this.localizationService.localize('Master::ResUserDevice:UserNameMaxLength'),
        });
        return false;
      }
      if (!this.formData.deviceUid?.trim()) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: this.localizationService.localize('Master::ResUserDevice:DeviceUidRequired'),
        });
        return false;
      }
      if (this.formData.deviceUid.length > 250) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: this.localizationService.localize('Master::ResUserDevice:DeviceUidMaxLength'),
        });
        return false;
      }
      if (!this.formData.appChannelCode?.trim()) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: this.localizationService.localize('Master::ResUserDevice:AppChannelCodeRequired'),
        });
        return false;
      }
      if (this.formData.appChannelCode.length > 50) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: this.localizationService.localize('Master::ResUserDevice:AppChannelCodeMaxLength'),
        });
        return false;
      }
      if (!this.effectDatePicker) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: this.localizationService.localize('Master::ResUserDevice:EffectDateRequired'),
        });
        return false;
      }
    }

    if (!this.formData.deviceToken?.trim()) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Master::ResUserDevice:DeviceTokenRequired'),
      });
      return false;
    }
    if (this.formData.deviceToken.length > 500) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Master::ResUserDevice:DeviceTokenMaxLength'),
      });
      return false;
    }
    if (this.formData.os && this.formData.os.length > 50) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Master::ResUserDevice:OsMaxLength'),
      });
      return false;
    }
    if (this.formData.deviceName && this.formData.deviceName.length > 250) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Master::ResUserDevice:DeviceNameMaxLength'),
      });
      return false;
    }
    return true;
  }

  private create(): void {
    this.loading = true;

    const createDto: CreateResUserDeviceDto = {
      userName: this.formData.userName.trim(),
      deviceUid: this.formData.deviceUid.trim(),
      deviceToken: this.formData.deviceToken.trim(),
      appChannelCode: this.formData.appChannelCode.trim(),
      effectDate: this.formData.effectDate,
      expirDate: this.formData.expirDate || undefined,
      status: this.formData.status,
      os: this.formData.os?.trim() || undefined,
      deviceName: this.formData.deviceName?.trim() || undefined,
    };

    this.userDeviceService.create(createDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Master::Success'),
          detail: this.localizationService.localize('Master::ResUserDevice:CreatedSuccessfully'),
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
        const translated = this.translateErrorMessage(errorMessage, error);
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: translated,
        });
        this.loading = false;
      },
    });
  }

  private update(): void {
    if (!this.selectedDevice?.id) {
      return;
    }
    this.loading = true;

    const updateDto: UpdateResUserDeviceDto = {
      deviceToken: this.formData.deviceToken.trim(),
      expirDate: this.formData.expirDate || undefined,
      status: this.formData.status,
      os: this.formData.os?.trim() || undefined,
      deviceName: this.formData.deviceName?.trim() || undefined,
    };

    this.userDeviceService.update(this.selectedDevice.id, updateDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Master::Success'),
          detail: this.localizationService.localize('Master::ResUserDevice:UpdatedSuccessfully'),
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
        const translated = this.translateErrorMessage(errorMessage, error);
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: translated,
        });
        this.loading = false;
      },
    });
  }

  delete(device: ResUserDeviceDto): void {
    if (!device.id) {
      return;
    }
    this.confirmationService.confirm({
      message: this.localizationService.localize('Master::ResUserDevice:DeleteConfirm'),
      header: this.localizationService.localize('Master::ResUserDevice:Delete'),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.loading = true;
        this.userDeviceService.delete(device.id!).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: this.localizationService.localize('Master::Success'),
              detail: this.localizationService.localize('Master::ResUserDevice:DeletedSuccessfully'),
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

  private getEmptyForm(): UserDeviceFormData {
    return {
      userName: '',
      deviceUid: '',
      deviceToken: '',
      appChannelCode: '',
      effectDate: '',
      expirDate: null,
      status: ResUserDeviceStatus.Active,
      os: null,
      deviceName: null,
    };
  }

  formatStatus(status: unknown): string {
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
      statusValue = status === ResUserDeviceStatus.Active ? 0 : 1;
    }
    return statusValue === 0
      ? this.localizationService.localize('Master::ResUserDevice:Active')
      : this.localizationService.localize('Master::ResUserDevice:Deactive');
  }

  translateErrorMessage(message: string, error?: { error?: { error?: { data?: Record<string, unknown> } } }): string {
    if (!message) return message;
    if (message.includes(':') && !message.includes('::')) {
      if (message.startsWith('Master:')) {
        message = message.replace('Master:', 'Master::');
      }
    }
    let translated = this.localizationService.localize(message);
    if (translated === message) {
      translated = message;
    }
    const data = error?.error?.error?.data as Record<string, string> | undefined;
    if (data) {
      if (translated.includes('{UserName}')) translated = translated.replace(/{UserName}/g, data['UserName'] ?? '');
      if (translated.includes('{DeviceUid}')) translated = translated.replace(/{DeviceUid}/g, data['DeviceUid'] ?? '');
      if (translated.includes('{AppChannelCode}'))
        translated = translated.replace(/{AppChannelCode}/g, data['AppChannelCode'] ?? '');
    }
    return translated;
  }

  /** Format date string for p-datepicker (yyyy-MM-dd) */
  toDate(value: string | undefined | null): Date | null {
    if (!value) return null;
    const d = new Date(value);
    return isNaN(d.getTime()) ? null : d;
  }

  fromDate(date: Date | null): string {
    if (!date) return '';
    return date.toISOString().split('T')[0];
  }
}
