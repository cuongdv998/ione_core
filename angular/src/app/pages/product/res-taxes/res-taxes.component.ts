import { Component, HostListener, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { SelectModule } from 'primeng/select';
import { TextareaModule } from 'primeng/textarea';
import { TableLazyLoadEvent } from 'primeng/table';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { DialogModule } from 'primeng/dialog';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TooltipModule } from 'primeng/tooltip';

import { VTable } from '@/shared/components/v-table/v-table';
import { TableColumn } from '@/shared/models/table-column.model';
import { TableAction } from '@/shared/models/table-action.model';
import { ResTaxService } from '@/proxy/product/controllers/res-tax.service';
import { GetResTaxesInput, ResTaxDto, CreateResTaxDto, UpdateResTaxDto } from '@/proxy/product/res-taxes/models';
import { ResTaxStatus } from '@/proxy/res-taxes/res-tax-status.enum';
import { PermissionPipe } from '@/core/pipes/permission.pipe';
import { PermissionService } from '@/core/services/permission.service';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';

@Component({
    selector: 'app-res-taxes',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        PanelModule,
        ButtonModule,
        InputTextModule,
        InputNumberModule,
        SelectModule,
        TextareaModule,
        ToastModule,
        DialogModule,
        ConfirmDialogModule,
        VTable,
        TooltipModule,
        PermissionPipe,
        TranslatePipe
    ],
    templateUrl: './res-taxes.component.html',
    styleUrl: './res-taxes.component.scss',
    providers: [MessageService, ConfirmationService]
})
export class ResTaxesComponent implements OnInit {
    // Permissions
    readonly PERMISSIONS = {
        CREATE: 'Product.ResTax.Create',
        UPDATE: 'Product.ResTax.Edit',
        DELETE: 'Product.ResTax.Delete',
        VIEW: 'Product.ResTax'
    };

    // Data
    items: ResTaxDto[] = [];
    totalCount = 0;
    loading = true;
    pageSize = 10;

    // Dialog
    dialogVisible = false;
    dialogMode: 'create' | 'edit' = 'create';
    selectedItem?: ResTaxDto;

    // Form Data
    formData: Partial<CreateResTaxDto> = {};

    // Search
    searchForm: {
        code?: string | null;
        name?: string | null;
        status?: ResTaxStatus | null;
    } = {
            code: null,
            name: null,
            status: null
        };

    // Options
    statusOptions: Array<{ label: string; value: ResTaxStatus }> = [];

    currentLazyLoadEvent?: TableLazyLoadEvent;
    columns: TableColumn[] = [];
    actions: TableAction<ResTaxDto>[] = [];

    constructor(
        private service: ResTaxService,
        private messageService: MessageService,
        private confirmationService: ConfirmationService,
        private permissionService: PermissionService,
        private localizationService: LocalizationService
    ) {
        this.initializeStatusOptions();
        this.initializeColumns();
        this.initializeActions();
    }

    private initializeStatusOptions(): void {
        this.statusOptions = [
            { label: this.localizationService.localize('Product::ResTax:Active'), value: ResTaxStatus.Active },
            { label: this.localizationService.localize('Product::ResTax:Inactive'), value: ResTaxStatus.Deactive }
        ];
    }

    ngOnInit(): void {
        // Component initialized
    }

    private initializeColumns(): void {
        this.columns = [
            {
                field: 'code',
                header: this.localizationService.localize('Product::ResTax:Code'),
                sortable: true,
                width: '150px'
            },
            {
                field: 'name',
                header: this.localizationService.localize('Product::ResTax:Name'),
                sortable: true,
                width: '250px'
            },
            {
                field: 'value',
                header: this.localizationService.localize('Product::ResTax:Value'),
                sortable: true,
                width: '120px'
            },
            {
                field: 'status',
                header: this.localizationService.localize('Product::ResTax:Status'),
                sortable: true,
                width: '100px',
                formatter: (value: any) => this.formatStatus(value)
            },
            {
                field: 'description',
                header: this.localizationService.localize('Product::ResTax:Description'),
                sortable: false
            }
        ];
    }

    private initializeActions(): void {
        this.actions.push({
            label: this.localizationService.localize('Product::Edit'),
            icon: 'pi pi-pencil',
            command: (row) => this.openEditDialog(row)
        });

        this.actions.push({
            label: this.localizationService.localize('Product::Delete'),
            icon: 'pi pi-trash',
            command: (row) => this.deleteItem(row)
        });
    }

    loadData(event: TableLazyLoadEvent): void {
        this.currentLazyLoadEvent = event;
        this.loading = true;

        const input: GetResTaxesInput = {
            skipCount: event.first || 0,
            maxResultCount: event.rows || this.pageSize,
            sorting: this.getSortingString(event),
            code: this.searchForm.code || undefined,
            name: this.searchForm.name || undefined,
            status: this.searchForm.status !== null ? this.searchForm.status : undefined
        };

        this.service.getList(input).subscribe({
            next: (result) => {
                this.items = result.items || [];
                this.totalCount = result.totalCount || 0;
                this.loading = false;
            },
            error: (err) => {
                console.error('Error loading data:', err);
                this.loading = false;
                const detail = this.getApiErrorMessage(err);
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: this.translateErrorMessage(detail, err)
                });
            }
        });
    }

    /**
     * Listen for Enter phím toàn cục để kích hoạt tìm kiếm khi không mở bảng thêm mới/sửa
     */
    @HostListener('window:keydown.enter', ['$event'])
    onWindowKeyDown(event: KeyboardEvent): void {
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
            name: null,
            status: null
        };
        this.search();
    }

    openCreateDialog(): void {
        this.dialogMode = 'create';
        this.formData = {
            status: ResTaxStatus.Active,
            value: 0
        };
        this.selectedItem = undefined;
        this.dialogVisible = true;
    }

    openEditDialog(item: ResTaxDto): void {
        this.dialogMode = 'edit';
        this.selectedItem = item;
        this.formData = {
            name: item.name,
            value: item.value,
            status: item.status,
            description: item.description
        };
        this.dialogVisible = true;
    }

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

    private validateForm(): boolean {
        // Validate required fields
        if (!this.formData.name ||
            this.formData.value === null ||
            this.formData.value === undefined ||
            this.formData.status === null ||
            this.formData.status === undefined ||
            (this.dialogMode === 'create' && !this.formData.code)) {

            this.messageService.add({
                severity: 'warn',
                summary: this.localizationService.localize('AbpUi::Warning'),
                detail: 'Vui lòng nhập đầy đủ thông tin bắt buộc'
            });
            return false;
        }

        // Validate code special characters (only for create mode as code is read-only or hidden in edit)
        if (this.dialogMode === 'create' && this.formData.code) {
            // Check for special characters (allow alphanumeric and underscore)
            const hasSpecialChars = /[^a-zA-Z0-9_]/.test(this.formData.code);
            if (hasSpecialChars) {
                this.messageService.add({
                    severity: 'warn',
                    summary: this.localizationService.localize('AbpUi::Warning'),
                    detail: 'Mã không được chứa ký tự đặc biệt'
                });
                return false;
            }
        }

        return true;
    }

    private getApiErrorMessage(err: unknown): string {
        const info = (err as { error?: { error?: { message?: string; details?: string; validationErrors?: Array<{ message?: string }> } } })?.error?.error;
        const validationErrors = info?.validationErrors;
        if (Array.isArray(validationErrors) && validationErrors.length > 0) {
            const lines = validationErrors
                .map((ve) => String(ve?.message ?? '').trim())
                .filter((s) => s.length > 0);
            if (lines.length > 0) {
                return lines.join('\n');
            }
        }
        return (
            info?.message ||
            info?.details ||
            (err as { message?: string })?.message ||
            this.localizationService.localize('AbpUi::InternalServerErrorMessage')
        );
    }

    /**
     * Translate API error message (e.g. ResTax:CodeInvalidFormat) to localized text.
     * API returns keys like "ResTax:CodeInvalidFormat" but localization expects "Product::ResTax:CodeInvalidFormat".
     * Also replaces placeholders like {Code} from error data (like ResTax:CodeExists).
     */
    private translateErrorMessage(message: string, err?: unknown): string {
        if (!message?.trim()) return message;
        const data = (err as { error?: { error?: { data?: Record<string, unknown> } } })?.error?.error?.data;
        const lines = message.split('\n').map((line) => {
            const key = line.trim();
            if (!key) return line;
            // Convert "ResTax:Key" to "Product::ResTax:Key" so localize finds it in Product resource
            let fullKey = key;
            if (key.startsWith('ResTax:') && !key.includes('::')) {
                fullKey = 'Product::' + key;
            }
            let translated = this.localizationService.localize(fullKey);
            if (translated === fullKey) translated = key;
            if (translated.includes('{Code}') && data?.['Code'] != null) {
                translated = translated.replace(/\{Code\}/g, String(data['Code']));
            }
            if (translated.includes('{0}')) {
                translated = translated.replace(/\{0\}/g, String(data?.['value'] ?? ''));
            }
            return translated;
        });
        return lines.join('\n');
    }

    private create(): void {
        this.loading = true;
        const input = this.formData as CreateResTaxDto;
        this.service.create(input).subscribe({
            next: () => {
                this.messageService.add({
                    severity: 'success',
                    summary: this.localizationService.localize('AbpUi::Success'),
                    detail: this.localizationService.localize('Product::ResTax:Created')
                });
                this.dialogVisible = false;
                this.search();
            },
            error: (err) => {
                this.loading = false;
                console.error(err);
                const detail = this.getApiErrorMessage(err);
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: this.translateErrorMessage(detail, err)
                });
            }
        });
    }

    private update(): void {
        if (!this.selectedItem?.id) return;
        this.loading = true;
        const input = this.formData as UpdateResTaxDto;
        this.service.update(this.selectedItem.id, input).subscribe({
            next: () => {
                this.messageService.add({
                    severity: 'success',
                    summary: this.localizationService.localize('AbpUi::Success'),
                    detail: this.localizationService.localize('Product::ResTax:Updated')
                });
                this.dialogVisible = false;
                this.search();
            },
            error: (err) => {
                this.loading = false;
                console.error(err);
                const detail = this.getApiErrorMessage(err);
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: this.translateErrorMessage(detail, err)
                });
            }
        });
    }

    deleteItem(item: ResTaxDto): void {
        const id = item.id;
        if (!id) return;

        this.confirmationService.confirm({
            message: this.localizationService.localize('Product::ResTax:DeleteConfirm'),
            header: this.localizationService.localize('Product::ResTax:Delete'),
            icon: 'pi pi-exclamation-triangle',
            accept: () => {
                this.loading = true;
                this.service.delete(id).subscribe({
                    next: () => {
                        this.messageService.add({
                            severity: 'success',
                            summary: this.localizationService.localize('AbpUi::Success'),
                            detail: this.localizationService.localize('Product::ResTax:Deleted')
                        });
                        this.search();
                    },
                    error: (err) => {
                        this.loading = false;
                        const detail = this.getApiErrorMessage(err);
                        this.messageService.add({
                            severity: 'error',
                            summary: this.localizationService.localize('AbpUi::Error'),
                            detail: this.translateErrorMessage(detail, err)
                        });
                    }
                });
            }
        });
    }

    private getSortingString(event: TableLazyLoadEvent): string {
        if (event.sortField) {
            const direction = event.sortOrder === 1 ? 'ASC' : 'DESC';
            return `${event.sortField} ${direction}`;
        }
        return '';
    }

    formatStatus(status: ResTaxStatus | number | string | undefined | null): string {
        if (status === undefined || status === null) {
            return '';
        }

        let statusValue: number;

        if (typeof status === 'number') {
            statusValue = status;
        } else if (typeof status === 'string') {
            if (status === 'Active' || status === 'active' || status === '0') {
                statusValue = 0;
            } else if (status === 'Deactive' || status === 'deactive' || status === 'Inactive' || status === 'inactive' || status === '1') {
                statusValue = 1;
            } else {
                statusValue = 1;
            }
        } else {
            statusValue = status === ResTaxStatus.Active ? 0 : 1;
        }

        return statusValue === 0
            ? this.localizationService.localize('Product::ResTax:Active')
            : this.localizationService.localize('Product::ResTax:Inactive');
    }
}

