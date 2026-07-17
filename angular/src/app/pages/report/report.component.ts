import { Component, OnInit, inject, ViewChild, HostListener } from '@angular/core';
import { ReportDetailModalComponent } from './detail-modal/report-detail-modal.component';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PanelModule } from 'primeng/panel';
import { SelectModule } from 'primeng/select';
import { ToolbarModule } from 'primeng/toolbar';
import { ToastModule } from 'primeng/toast';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ReportTemplateService } from '../../proxy/report/controllers/report-template.service';
import { ReportTemplateDto } from '../../proxy/report/report-templates/models';
import { VTable } from '@/shared/components/v-table/v-table';
import { TableColumn } from '@/shared/models/table-column.model';
import { TableAction } from '@/shared/models/table-action.model';
import { LocalizationService } from '@/core/services/localization.service';
import { TranslatePipe } from '@/core/pipes/translate.pipe';


import { ReportExportDynamicModalComponent } from './export-dynamic-modal/report-export-dynamic-modal.component';

@Component({
    selector: 'app-report',
    standalone: true,
    imports: [CommonModule, FormsModule, TableModule, ButtonModule, InputTextModule, PanelModule, SelectModule, ToolbarModule, VTable, ReportDetailModalComponent, ReportExportDynamicModalComponent, ToastModule, ConfirmDialogModule, TranslatePipe],
    templateUrl: './report.component.html',
    styleUrl: './report.component.scss',
    providers: [MessageService, ConfirmationService]
})
export class ReportComponent implements OnInit {
    private readonly reportTemplateService = inject(ReportTemplateService);
    private readonly messageService = inject(MessageService);
    private readonly confirmationService = inject(ConfirmationService);
    private readonly localizationService = inject(LocalizationService);

    // Data
    items: ReportTemplateDto[] = [];
    totalCount = 0;
    loading = false;

    // Filter
    filtered = {
        code: '',
        name: '',
        status: null
    };

    statusOptions: any[] = [];

    // Pagination
    skipCount = 0;
    maxResultCount = 10;
    pageSize = 10;

    // Table Config
    columns: TableColumn[] = [];
    actions: TableAction[] = [];

    // Modal
    @ViewChild('modal') modal!: ReportDetailModalComponent;
    @ViewChild('exportDynamicModal') exportDynamicModal!: ReportExportDynamicModalComponent;

    ngOnInit() {
        this.initOptions();
        this.initTable();
        this.refresh();
    }

    initOptions() {
        this.statusOptions = [
            { label: this.localizationService.localize('Report::Status:Active'), value: 'active' },
            { label: this.localizationService.localize('Report::Status:Inactive'), value: 'deactive' }
        ];
    }

    initTable() {
        this.columns = [
            { field: 'name', header: this.localizationService.localize('Report::ReportName'), sortable: true, width: '250px' },
            { field: 'code', header: this.localizationService.localize('Report::Code'), sortable: true, width: '150px' },
            { field: 'documentId', header: this.localizationService.localize('Report::DocumentId'), width: '200px' },
            {
                field: 'status', header: this.localizationService.localize('Report::Status'), width: '120px',
                align: 'center',
                formatter: (value: any) => this.formatStatus(value),
                cellClass: (value: any) => {
                    const isActive = value === 'active' || value === 'Active' || value === '0' || value === 0;
                    return isActive
                        ? 'px-2 py-1 rounded text-xs font-semibold text-green-800'
                        : 'px-2 py-1 rounded text-xs font-semibold text-red-800';
                }
            },
            { field: 'effectDate', header: this.localizationService.localize('Report::EffectDate'), width: '150px', align: 'center', formatter: (value: any) => this.formatDate(value) },
            { field: 'expireDate', header: this.localizationService.localize('Report::ExpireDate'), width: '150px', align: 'center', formatter: (value: any) => this.formatDate(value) }
        ];

        this.actions = [
            {
                label: this.localizationService.localize('Report::ExportDynamic'),
                icon: 'pi pi-download',
                command: (row) => this.onExportDynamic(row),
            },
            {
                label: this.localizationService.localize('AbpUi::Edit'),
                icon: 'pi pi-pencil',
                command: (row) => this.onEdit(row),
            },
            {
                label: this.localizationService.localize('AbpUi::Delete'),
                icon: 'pi pi-trash',
                command: (row) => this.onDelete(row),
            }
        ];
    }

    refresh() {
        this.loading = true;
        const input: any = {
            code: this.filtered.code || undefined,
            name: this.filtered.name || undefined,
            status: this.filtered.status || undefined,
            maxResultCount: this.maxResultCount,
            skipCount: this.skipCount
        };

        this.reportTemplateService.getList(input).subscribe({
            next: (res) => {
                this.items = res.items || [];
                console.log('ReportComponent ngOnInit', this.items);
                this.totalCount = res.totalCount || 0;
                this.loading = false;
            },
            error: () => {
                this.loading = false;
            }
        });
    }

    onPage(event: any) {
        this.skipCount = event.first || 0;
        this.maxResultCount = event.rows || 10;
        this.pageSize = this.maxResultCount;
        this.refresh();
    }

    onReset() {
        this.filtered = {
            code: '',
            name: '',
            status: null
        };
        this.refresh();
    }

    @HostListener('window:keydown.enter', ['$event'])
    handleEnter(event: KeyboardEvent) {
        // Only trigger search if dialogs are not visible
        if (!this.modal.visible && !this.exportDynamicModal.visible) {
            this.onSearch();
        }
    }

    onSearch() {
        this.skipCount = 0;
        this.refresh();
    }

    onCreate() {
        this.modal.open();
    }

    onExport() {
        this.loading = true;
        const input: any = {
            code: this.filtered.code || undefined,
            name: this.filtered.name || undefined,
            status: this.filtered.status || undefined,
            maxResultCount: 1000,
            skipCount: 0
        };

        this.reportTemplateService.export(input).subscribe({
            next: (data: Blob) => {
                const a = document.createElement('a');
                const objectUrl = URL.createObjectURL(data);
                a.href = objectUrl;
                a.download = 'report-templates.xlsx';
                a.click();
                URL.revokeObjectURL(objectUrl);
                this.loading = false;
                this.messageService.add({ severity: 'success', summary: this.localizationService.localize('Report::Notify:Success'), detail: this.localizationService.localize('Report::Message:ExportSuccess') });
            },
            error: () => {
                this.loading = false;
                this.messageService.add({ severity: 'error', summary: this.localizationService.localize('Report::Notify:Error'), detail: this.localizationService.localize('Report::Error:ExportFailed') });
            }
        });
    }

    onExportDynamic(row?: ReportTemplateDto) {
        this.exportDynamicModal.open(row?.id);
    }

    onEdit(row: ReportTemplateDto) {
        this.modal.open(row.id);
    }

    onDelete(row: ReportTemplateDto) {
        if (!row.id) {
            return;
        }

        this.confirmationService.confirm({
            message: this.localizationService.localize('Report::Message:DeleteConfirmation'),
            header: this.localizationService.localize('AbpUi::ConfirmDelete'),
            icon: 'pi pi-exclamation-triangle',
            accept: () => {
                this.reportTemplateService.delete(row.id!).subscribe({
                    next: () => {
                        this.messageService.add({ severity: 'success', summary: this.localizationService.localize('Report::Notify:Success'), detail: this.localizationService.localize('Report::Message:DeleteSuccess') });
                        this.refresh();
                    },
                    error: () => {
                        this.messageService.add({ severity: 'error', summary: this.localizationService.localize('Report::Notify:Error'), detail: this.localizationService.localize('Report::Error:DeleteFailed') });
                    }
                });
            }
        });
    }

    formatStatus(status: number | string | undefined | null): string {
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
            statusValue = 1;
        }

        return statusValue === 0
            ? this.localizationService.localize('Report::Status:Active')
            : this.localizationService.localize('Report::Status:Inactive');
    }
    formatDate(date: string | Date | undefined | null): string {
        if (!date) return '';
        const d = new Date(date);
        if (isNaN(d.getTime())) return '';
        const day = d.getDate().toString().padStart(2, '0');
        const month = (d.getMonth() + 1).toString().padStart(2, '0');
        const year = d.getFullYear();
        return `${day}/${month}/${year}`;
    }
}
