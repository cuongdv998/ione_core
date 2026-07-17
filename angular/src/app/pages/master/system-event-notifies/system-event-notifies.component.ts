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
import { SystemEventNotifyService } from '@/proxy/master/controllers/system-event-notify.service';
import { ResAppChannelService } from '@/proxy/master/controllers/res-app-channel.service';
import type {
  SystemEventNotifyDto,
  CreateSystemEventNotifyDto,
  UpdateSystemEventNotifyDto,
  GetSystemEventNotifiesInput,
} from '@/proxy/master/system-event-notifies/models';
import type { ResAppChannelDto } from '@/proxy/master/res-app-channels/models';
import { SystemEventNotifyStatus } from '@/proxy/system-event-notifies/system-event-notify-status.enum';
import type { SystemEventNotifySearchForm, SystemEventNotifyFormData } from './system-event-notifies.models';
import { PermissionPipe } from '@/core/pipes/permission.pipe';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { PermissionService } from '@/core/services/permission.service';
import { LocalizationService } from '@/core/services/localization.service';

@Component({
  selector: 'app-system-event-notifies',
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
  templateUrl: './system-event-notifies.component.html',
  styleUrl: './system-event-notifies.component.scss',
  providers: [MessageService, ConfirmationService],
})
export class SystemEventNotifiesComponent implements OnInit {
  readonly PERMISSIONS = {
    CREATE: 'MasterSystemEventNotify.Create',
    UPDATE: 'MasterSystemEventNotify.Edit',
    DELETE: 'MasterSystemEventNotify.Delete',
    VIEW: 'MasterSystemEventNotify.View',
  };

  items: SystemEventNotifyDto[] = [];
  totalCount = 0;
  loading = true;
  pageSize = 10;

  dialogVisible = false;
  dialogMode: 'create' | 'edit' = 'create';
  formData: SystemEventNotifyFormData = this.getEmptyForm();
  selectedItem?: SystemEventNotifyDto;

  scheduleAtPicker: Date | null = null;
  sentAtPicker: Date | null = null;
  readAtPicker: Date | null = null;

  searchForm: SystemEventNotifySearchForm = {
    eventCode: null,
    appChannelId: null,
    recipientType: null,
    recipientId: null,
    recipient: null,
    status: null,
    scheduleAtFrom: null,
    scheduleAtTo: null,
    sentAtFrom: null,
    sentAtTo: null,
  };

  scheduleAtFromPicker: Date | null = null;
  scheduleAtToPicker: Date | null = null;
  sentAtFromPicker: Date | null = null;
  sentAtToPicker: Date | null = null;

  statusOptions: Array<{ label: string; value: SystemEventNotifyStatus }> = [];
  /** Kênh ứng dụng từ bảng res_app_channel */
  appChannelOptions: Array<{ label: string; value: string }> = [];
  /** Loại người nhận: cus (khách hàng), emp (nhân viên) */
  recipientTypeOptions: Array<{ label: string; value: string }> = [];
  columns: TableColumn[] = [];
  actions: TableAction<SystemEventNotifyDto>[] = [];
  currentLazyLoadEvent?: TableLazyLoadEvent;

  constructor(
    private systemEventNotifyService: SystemEventNotifyService,
    private appChannelService: ResAppChannelService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private permissionService: PermissionService,
    private localizationService: LocalizationService
  ) {
    this.initializeColumns();
    this.initializeActions();
    this.initializeStatusOptions();
    this.initializeRecipientTypeOptions();
  }

  ngOnInit(): void {
    this.loadAppChannels();
  }

  private loadAppChannels(): void {
    this.appChannelService.getList({ skipCount: 0, maxResultCount: 1000 }).subscribe({
      next: (result) => {
        const items = (result.items || []) as ResAppChannelDto[];
        this.appChannelOptions = items.map((ch) => ({
          label: `${ch.code || ''} - ${ch.name || ''}`.trim() || ch.id || '',
          value: ch.id!,
        }));
      },
      error: () => {
        this.appChannelOptions = [];
      },
    });
  }

  private initializeRecipientTypeOptions(): void {
    this.recipientTypeOptions = [
      { label: this.localizationService.localize('Master::SystemEventNotify:RecipientTypeCus'), value: 'cus' },
      { label: this.localizationService.localize('Master::SystemEventNotify:RecipientTypeEmp'), value: 'emp' },
    ];
  }

  private initializeStatusOptions(): void {
    this.statusOptions = [
      { label: this.localizationService.localize('Master::SystemEventNotify:Pending'), value: SystemEventNotifyStatus.Pending },
      { label: this.localizationService.localize('Master::SystemEventNotify:Sent'), value: SystemEventNotifyStatus.Sent },
      { label: this.localizationService.localize('Master::SystemEventNotify:Fail'), value: SystemEventNotifyStatus.Fail },
      { label: this.localizationService.localize('Master::SystemEventNotify:Read'), value: SystemEventNotifyStatus.Read },
      { label: this.localizationService.localize('Master::SystemEventNotify:Deactive'), value: SystemEventNotifyStatus.Deactive },
    ];
  }

  private initializeColumns(): void {
    this.columns = [
      { field: 'eventCode', header: this.localizationService.localize('Master::SystemEventNotify:EventCode'), sortable: true, width: '120px' },
      { field: 'title', header: this.localizationService.localize('Master::SystemEventNotify:Title'), sortable: true, width: '200px' },
      {
        field: 'recipientType',
        header: this.localizationService.localize('Master::SystemEventNotify:RecipientType'),
        sortable: true,
        width: '120px',
        formatter: (value: unknown) => this.formatRecipientType(value),
      },
      { field: 'recipient', header: this.localizationService.localize('Master::SystemEventNotify:Recipient'), sortable: false, width: '150px' },
      {
        field: 'status',
        header: this.localizationService.localize('Master::SystemEventNotify:Status'),
        sortable: true,
        type: 'text',
        width: '110px',
        freeze: 'right',
        align: 'center',
        formatter: (value: unknown) => this.formatStatus(value),
        cellClass: (value: unknown) => this.getStatusCellClass(value),
      },
      { field: 'scheduleAt', header: this.localizationService.localize('Master::SystemEventNotify:ScheduleAt'), sortable: true, type: 'date', width: '160px' },
      { field: 'sentAt', header: this.localizationService.localize('Master::SystemEventNotify:SentAt'), sortable: true, type: 'date', width: '160px' },
      { field: 'creationTime', header: this.localizationService.localize('AbpIdentity::CreationTime'), sortable: true, type: 'date', width: '180px' },
    ];
  }

  private getStatusCellClass(value: unknown): string {
    const v = this.getStatusValue(value);
    const map: Record<number, string> = {
      0: 'px-2 py-1 rounded text-xs font-semibold bg-amber-100 text-amber-800',
      1: 'px-2 py-1 rounded text-xs font-semibold bg-blue-100 text-blue-800',
      2: 'px-2 py-1 rounded text-xs font-semibold bg-red-100 text-red-800',
      3: 'px-2 py-1 rounded text-xs font-semibold bg-green-100 text-green-800',
      4: 'px-2 py-1 rounded text-xs font-semibold bg-gray-100 text-gray-800',
    };
    return map[v] ?? map[4];
  }

  private getStatusValue(status: unknown): number {
    if (status === undefined || status === null) return 4;
    if (typeof status === 'number') return status;
    if (typeof status === 'string') {
      const s = (status as string).toLowerCase();
      if (s === 'pending' || s === '0') return 0;
      if (s === 'sent' || s === '1') return 1;
      if (s === 'fail' || s === '2') return 2;
      if (s === 'read' || s === '3') return 3;
      if (s === 'deactive' || s === '4') return 4;
    }
    return 4;
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

    const input: GetSystemEventNotifiesInput = {
      skipCount: event.first || 0,
      maxResultCount: event.rows || this.pageSize,
      sorting: this.getSortingString(event),
      eventCode: this.searchForm.eventCode || undefined,
      appChannelId: this.searchForm.appChannelId || undefined,
      recipientType: this.searchForm.recipientType || undefined,
      recipientId: this.searchForm.recipientId || undefined,
      recipient: this.searchForm.recipient || undefined,
      status: this.searchForm.status ?? undefined,
      scheduleAtFrom: this.searchForm.scheduleAtFrom || undefined,
      scheduleAtTo: this.searchForm.scheduleAtTo || undefined,
      sentAtFrom: this.searchForm.sentAtFrom || undefined,
      sentAtTo: this.searchForm.sentAtTo || undefined,
    };

    this.systemEventNotifyService.getList(input).subscribe({
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
    this.syncSearchDatePickersToForm();
    if (this.currentLazyLoadEvent) {
      this.currentLazyLoadEvent.first = 0;
      this.loadData(this.currentLazyLoadEvent);
    }
  }

  private syncSearchDatePickersToForm(): void {
    this.searchForm.scheduleAtFrom = this.scheduleAtFromPicker ? this.fromDate(this.scheduleAtFromPicker) : null;
    this.searchForm.scheduleAtTo = this.scheduleAtToPicker ? this.fromDate(this.scheduleAtToPicker) : null;
    this.searchForm.sentAtFrom = this.sentAtFromPicker ? this.fromDate(this.sentAtFromPicker) : null;
    this.searchForm.sentAtTo = this.sentAtToPicker ? this.fromDate(this.sentAtToPicker) : null;
  }

  resetSearch(): void {
    this.searchForm = {
      eventCode: null,
      appChannelId: null,
      recipientType: null,
      recipientId: null,
      recipient: null,
      status: null,
      scheduleAtFrom: null,
      scheduleAtTo: null,
      sentAtFrom: null,
      sentAtTo: null,
    };
    this.scheduleAtFromPicker = null;
    this.scheduleAtToPicker = null;
    this.sentAtFromPicker = null;
    this.sentAtToPicker = null;
    this.search();
  }

  openCreateDialog(): void {
    if (this.appChannelOptions.length === 0) {
      this.loadAppChannels();
    }
    this.dialogMode = 'create';
    this.formData = this.getEmptyForm();
    this.selectedItem = undefined;
    this.scheduleAtPicker = null;
    this.sentAtPicker = null;
    this.readAtPicker = null;
    this.dialogVisible = true;
  }

  /** Khi sửa: chỉ được phép sửa trạng thái (Status). */
  openEditDialog(item: SystemEventNotifyDto): void {
    this.dialogMode = 'edit';
    this.selectedItem = item;
    this.formData = {
      ...this.getEmptyForm(),
      status: (item.status as SystemEventNotifyStatus) ?? SystemEventNotifyStatus.Pending,
    };
    this.dialogVisible = true;
  }

  save(): void {
    if (this.dialogMode === 'create') {
      this.syncCreateDatePickersToForm();
      if (!this.validateCreateForm()) return;
      this.create();
    } else {
      if (!this.selectedItem?.id) return;
      this.update();
    }
  }

  private syncCreateDatePickersToForm(): void {
    this.formData.scheduleAt = this.fromDate(this.scheduleAtPicker);
    this.formData.sentAt = this.fromDate(this.sentAtPicker) || null;
    this.formData.readAt = this.fromDate(this.readAtPicker) || null;
  }

  private validateCreateForm(): boolean {
    if (!this.formData.eventCode?.trim()) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Master::SystemEventNotify:EventCodeRequired'),
      });
      return false;
    }
    if (!this.formData.appChannelId?.trim()) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Master::SystemEventNotify:AppChannelIdRequired'),
      });
      return false;
    }
    if (!this.formData.title?.trim()) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Master::SystemEventNotify:TitleRequired'),
      });
      return false;
    }
    if (!this.formData.body?.trim()) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Master::SystemEventNotify:BodyRequired'),
      });
      return false;
    }
    if (!this.formData.recipientType?.trim()) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Master::SystemEventNotify:RecipientTypeRequired'),
      });
      return false;
    }
    if (!this.formData.recipientId?.trim()) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Master::SystemEventNotify:RecipientIdRequired'),
      });
      return false;
    }
    if (!this.scheduleAtPicker) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Master::SystemEventNotify:ScheduleAtRequired'),
      });
      return false;
    }
    return true;
  }

  private create(): void {
    this.loading = true;
    this.syncCreateDatePickersToForm();

    const createDto: CreateSystemEventNotifyDto = {
      eventCode: this.formData.eventCode.trim(),
      appChannelId: this.formData.appChannelId.trim(),
      title: this.formData.title.trim(),
      body: this.formData.body.trim(),
      payload: this.formData.payload?.trim() || undefined,
      recipientType: this.formData.recipientType.trim(),
      recipientId: this.formData.recipientId.trim(),
      recipient: this.formData.recipient?.trim() || undefined,
      status: this.formData.status,
      scheduleAt: this.formData.scheduleAt,
      sentAt: this.formData.sentAt || undefined,
      readAt: this.formData.readAt || undefined,
      errorMessage: this.formData.errorMessage?.trim() || undefined,
      retryNumber: 0,
    };

    this.systemEventNotifyService.create(createDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Master::Success'),
          detail: this.localizationService.localize('Master::SystemEventNotify:CreatedSuccessfully'),
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

  private update(): void {
    if (!this.selectedItem?.id) return;
    this.loading = true;

    const updateDto: UpdateSystemEventNotifyDto = {
      status: this.formData.status,
    };

    this.systemEventNotifyService.update(this.selectedItem.id, updateDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Master::Success'),
          detail: this.localizationService.localize('Master::SystemEventNotify:UpdatedSuccessfully'),
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

  delete(item: SystemEventNotifyDto): void {
    if (!item.id) return;
    this.confirmationService.confirm({
      message: this.localizationService.localize('Master::SystemEventNotify:DeleteConfirm'),
      header: this.localizationService.localize('Master::SystemEventNotify:Delete'),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.loading = true;
        this.systemEventNotifyService.delete(item.id!).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: this.localizationService.localize('Master::Success'),
              detail: this.localizationService.localize('Master::SystemEventNotify:DeletedSuccessfully'),
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

  private getEmptyForm(): SystemEventNotifyFormData {
    return {
      eventCode: '',
      appChannelId: '',
      title: '',
      body: '',
      payload: null,
      recipientType: '',
      recipientId: '',
      recipient: null,
      status: SystemEventNotifyStatus.Pending,
      scheduleAt: '',
      sentAt: null,
      readAt: null,
      errorMessage: null,
    };
  }

  formatRecipientType(value: unknown): string {
    if (value === undefined || value === null) return '';
    const s = String(value).toLowerCase();
    if (s === 'cus') return this.localizationService.localize('Master::SystemEventNotify:RecipientTypeCus');
    if (s === 'emp') return this.localizationService.localize('Master::SystemEventNotify:RecipientTypeEmp');
    return String(value);
  }

  formatStatus(status: unknown): string {
    const v = this.getStatusValue(status);
    const keys: Record<number, string> = {
      0: 'Master::SystemEventNotify:Pending',
      1: 'Master::SystemEventNotify:Sent',
      2: 'Master::SystemEventNotify:Fail',
      3: 'Master::SystemEventNotify:Read',
      4: 'Master::SystemEventNotify:Deactive',
    };
    return this.localizationService.localize(keys[v] ?? keys[4]);
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

  /** Hiển thị tên kênh ứng dụng trong modal sửa (read-only) */
  getAppChannelLabel(id: string | undefined): string {
    if (!id) return '';
    const opt = this.appChannelOptions.find((o) => o.value === id);
    return opt?.label ?? id;
  }

  /** Format ngày giờ để hiển thị (read-only). Chấp nhận string hoặc Date (audit fields). */
  formatDateTimeDisplay(value: string | Date | undefined | null): string {
    if (value == null) return '';
    const d = value instanceof Date ? value : new Date(value);
    return isNaN(d.getTime()) ? (typeof value === 'string' ? value : '') : d.toLocaleString();
  }
}
