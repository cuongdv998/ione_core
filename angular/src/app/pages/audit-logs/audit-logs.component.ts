import { Component, HostListener, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { DatePickerModule } from 'primeng/datepicker';
import { SelectModule } from 'primeng/select';
import { CheckboxModule } from 'primeng/checkbox';
import { InputNumberModule } from 'primeng/inputnumber';
import { TableLazyLoadEvent } from 'primeng/table';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { DialogModule } from 'primeng/dialog';
import { TabsModule } from 'primeng/tabs';

import { VTable } from '@/shared/components/v-table/v-table';
import { TableColumn } from '@/shared/models/table-column.model';
import { TableAction } from '@/shared/models/table-action.model';
import { AuditLogService } from '@/proxy/volo/abp/audit-logging/audit-log.service';
import { AuditLogDto, GetAuditLogsInput } from '@/proxy/volo/abp/audit-logging/models';
import { AuditLogSearchForm, HTTP_METHOD_OPTIONS, HttpMethodOption } from './audit-logs.models';
import { PermissionPipe } from '@/core/pipes/permission.pipe';
import { PermissionService } from '@/core/services/permission.service';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';

@Component({
  selector: 'app-audit-logs',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    PanelModule,
    ButtonModule,
    InputTextModule,
    DatePickerModule,
    SelectModule,
    CheckboxModule,
    InputNumberModule,
    ToastModule,
    DialogModule,
    TabsModule,
    VTable,
    PermissionPipe,
    TranslatePipe
  ],
  templateUrl: './audit-logs.component.html',
  styleUrl: './audit-logs.component.scss',
  providers: [MessageService]
})
export class AuditLogsComponent implements OnInit {
  // Permissions
  readonly PERMISSIONS = {
    VIEW: 'AbpAuditLogging.AuditLogs.View',
    VIEW_DETAILS: 'AbpAuditLogging.AuditLogs.ViewDetails',
    VIEW_ENTITY_CHANGES: 'AbpAuditLogging.AuditLogs.ViewEntityChanges'
  };

  // Data properties
  auditLogs: any[] = [];  // Using 'any' because API returns camelCase but DTO defines PascalCase
  totalCount = 0;
  loading = true;
  pageSize = 10;
  
  // Dialog properties
  detailDialogVisible = false;
  selectedAuditLog?: any;  // Using 'any' for same reason

  // Search form
  searchForm: AuditLogSearchForm = {
    startTime: null,
    endTime: null,
    httpMethod: null,
    url: null,
    userName: null,
    applicationName: null,
    clientIpAddress: null,
    correlationId: null,
    maxExecutionDuration: null,
    minExecutionDuration: null,
    hasException: null,
    httpStatusCode: null
  };

  // Lazy load event
  currentLazyLoadEvent?: TableLazyLoadEvent;

  // Options
  httpMethodOptions: HttpMethodOption[] = HTTP_METHOD_OPTIONS;

  // Table configuration
  columns: TableColumn[] = [];

  // Table actions
  actions: TableAction<any>[] = [];

  constructor(
    private auditLogService: AuditLogService,
    private messageService: MessageService,
    private permissionService: PermissionService,
    private localizationService: LocalizationService
  ) {
    // Initialize columns and actions
    this.initializeColumns();
    this.initializeActions();
  }

  ngOnInit(): void {
    // Initial load will be triggered by the table's onLazyLoad event
  }

  /**
   * Initialize table columns with localized headers
   */
  private initializeColumns(): void {
    this.columns = [
      {
        field: 'userName',
        header: this.localizationService.localize('AbpAuditLogging::UserName'),
        sortable: true,
        width: '150px'
      },
      {
        field: 'httpMethod',
        header: this.localizationService.localize('AbpAuditLogging::HttpMethod'),
        sortable: true,
        width: '100px'
      },
      {
        field: 'url',
        header: this.localizationService.localize('AbpAuditLogging::Url'),
        sortable: true,
        width: '300px'
      },
      {
        field: 'httpStatusCode',
        header: this.localizationService.localize('AbpAuditLogging::Status'),
        sortable: true,
        type: 'number',
        width: '100px'
      },
      {
        field: 'executionTime',
        header: this.localizationService.localize('AbpAuditLogging::ExecutionTime'),
        sortable: true,
        type: 'date',
        width: '180px'
      },
      {
        field: 'executionDuration',
        header: this.localizationService.localize('AbpAuditLogging::DurationMs'),
        sortable: true,
        type: 'number',
        width: '140px'
      },
      {
        field: 'clientIpAddress',
        header: this.localizationService.localize('AbpAuditLogging::IpAddress'),
        sortable: true,
        width: '140px'
      },
      {
        field: 'applicationName',
        header: this.localizationService.localize('AbpAuditLogging::ApplicationName'),
        sortable: true,
        width: '150px'
      }
    ];
  }

  /**
   * Initialize actions based on permissions
   */
  private initializeActions(): void {
    if (this.permissionService.isGranted(this.PERMISSIONS.VIEW_DETAILS)) {
      this.actions.push({
        label: this.localizationService.localize('AbpAuditLogging::ViewDetail'),
        icon: 'pi pi-eye',
        command: (row) => this.viewDetail(row)
      });
    }
  }

  /**
   * Load audit logs with lazy loading
   */
  loadAuditLogs(event: TableLazyLoadEvent): void {
    this.currentLazyLoadEvent = event;
    this.loading = true;

    const input: GetAuditLogsInput = {
      skipCount: event.first || 0,
      maxResultCount: event.rows || 10,
      sorting: this.getSortingString(event),
      includeDetails: false,
      ...this.buildSearchParams()
    };

    this.auditLogService.getList(input).subscribe({
      next: (result) => {
        this.auditLogs = result.items || [];
        this.totalCount = result.totalCount || 0;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading audit logs:', error);
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: this.localizationService.localize('AbpUi::InternalServerErrorMessage')
        });
        this.loading = false;
      }
    });
  }

  /**
   * Execute search with current filters
   */
  search(): void {
    if (this.currentLazyLoadEvent) {
      this.currentLazyLoadEvent.first = 0;
      this.loadAuditLogs(this.currentLazyLoadEvent);
    }
  }

  /**
   * Listen for Enter key globally to trigger search when no dialog is open
   */
  @HostListener('window:keydown.enter', ['$event'])
  onWindowKeyDown(event: KeyboardEvent): void {
    if (!this.detailDialogVisible) {
      this.search();
    }
  }

  /**
   * Reset search form and reload data
   */
  resetSearch(): void {
    this.searchForm = {
      startTime: null,
      endTime: null,
      httpMethod: null,
      url: null,
      userName: null,
      applicationName: null,
      clientIpAddress: null,
      correlationId: null,
      maxExecutionDuration: null,
      minExecutionDuration: null,
      hasException: null,
      httpStatusCode: null
    };
    this.search();
  }

  /**
   * View audit log details
   */
  viewDetail(auditLog: any): void {
    this.loading = true;
    
    // Fetch the full audit log with details
    this.auditLogService.get(auditLog.id!).subscribe({
      next: (result) => {
        this.selectedAuditLog = result;
        this.detailDialogVisible = true;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading audit log detail:', error);
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: this.localizationService.localize('AbpUi::InternalServerErrorMessage')
        });
        this.loading = false;
      }
    });
  }

  /**
   * Build search parameters from form
   */
  private buildSearchParams(): Partial<GetAuditLogsInput> {
    const params: any = {};

    if (this.searchForm.startTime) {
      params.startTime = this.searchForm.startTime.toISOString();
    }
    if (this.searchForm.endTime) {
      params.endTime = this.searchForm.endTime.toISOString();
    }
    if (this.searchForm.httpMethod) {
      params.httpMethod = this.searchForm.httpMethod;
    }
    if (this.searchForm.url) {
      params.url = this.searchForm.url;
    }
    if (this.searchForm.userName) {
      params.userName = this.searchForm.userName;
    }
    if (this.searchForm.applicationName) {
      params.applicationName = this.searchForm.applicationName;
    }
    if (this.searchForm.clientIpAddress) {
      params.clientIpAddress = this.searchForm.clientIpAddress;
    }
    if (this.searchForm.correlationId) {
      params.correlationId = this.searchForm.correlationId;
    }
    if (this.searchForm.maxExecutionDuration) {
      params.maxExecutionDuration = this.searchForm.maxExecutionDuration;
    }
    if (this.searchForm.minExecutionDuration) {
      params.minExecutionDuration = this.searchForm.minExecutionDuration;
    }
    if (this.searchForm.hasException !== null && this.searchForm.hasException !== undefined) {
      params.hasException = this.searchForm.hasException;
    }
    if (this.searchForm.httpStatusCode) {
      params.httpStatusCode = this.searchForm.httpStatusCode;
    }

    return params;
  }

  /**
   * Get sorting string from table event
   */
  private getSortingString(event: TableLazyLoadEvent): string {
    if (event.sortField) {
      const direction = event.sortOrder === 1 ? 'ASC' : 'DESC';
      return `${event.sortField} ${direction}`;
    }
    return '';
  }
}

