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
import { DialogModule } from 'primeng/dialog';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TooltipModule } from 'primeng/tooltip';

import { VTable } from '@/shared/components/v-table/v-table';
import { TableColumn } from '@/shared/models/table-column.model';
import { TableAction } from '@/shared/models/table-action.model';
import { ProLineOfBusinessService } from '@/proxy/product/controllers/pro-line-of-business.service';
import { GetProLineOfBusinessesInput, ProLineOfBusinessDto, CreateProLineOfBusinessDto, UpdateProLineOfBusinessDto } from '@/proxy/product/pro-line-of-businesses/models';
import { ProLineOfBusinessStatus } from '@/proxy/pro-line-of-businesses';
import { ProLineOfBusinessSearchForm } from './pro-line-of-businesses.models';
import { PermissionPipe } from '@/core/pipes/permission.pipe';
import { PermissionService } from '@/core/services/permission.service';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';
import { forkJoin } from 'rxjs';

@Component({
    selector: 'app-pro-line-of-businesses',
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
        DialogModule,
        ConfirmDialogModule,
        VTable,
        TooltipModule,
        PermissionPipe,
        TranslatePipe
    ],
    templateUrl: './pro-line-of-businesses.component.html',
    styleUrl: './pro-line-of-businesses.component.scss',
    providers: [MessageService, ConfirmationService]
})
export class ProLineOfBusinessesComponent implements OnInit {
    // Permissions (assuming these keys, fallback to checking later if needed)
    readonly PERMISSIONS = {
        CREATE: 'Product.ProLineOfBusiness.Create',
        UPDATE: 'Product.ProLineOfBusiness.Update',
        DELETE: 'Product.ProLineOfBusiness.Delete',
        VIEW: 'Product.ProLineOfBusiness'
    };

    // Data
    items: (ProLineOfBusinessDto & { parentName?: string })[] = [];
    totalCount = 0;
    loading = true;
    pageSize = 10;

    // Parent Options
    // Parent Options
    parentOptions: { label: string, value: string }[] = [];
    dialogParentOptions: { label: string, value: string }[] = [];

    // Dialog
    dialogVisible = false;
    dialogMode: 'create' | 'edit' | 'view' = 'create';
    selectedItem?: ProLineOfBusinessDto;

    // Form Data
    formData: Partial<CreateProLineOfBusinessDto> | null = {};

    // Validation errors
    formErrors: { code?: boolean; name?: boolean; status?: boolean } = {};

    // Search
    searchForm: ProLineOfBusinessSearchForm = {
        code: null,
        name: null,
        status: null,
        parentId: null
    };

    // Options
    statusOptions = [
        { label: 'Active', value: ProLineOfBusinessStatus.Active },
        { label: 'Inactive', value: ProLineOfBusinessStatus.Deactive }
    ];

    currentLazyLoadEvent?: TableLazyLoadEvent;
    columns: TableColumn[] = [];
    actions: TableAction<ProLineOfBusinessDto>[] = [];

    constructor(
        private service: ProLineOfBusinessService,
        private messageService: MessageService,
        private confirmationService: ConfirmationService,
        private permissionService: PermissionService,
        private localizationService: LocalizationService
    ) {
        this.initializeColumns();
        this.initializeActions();
    }

    ngOnInit(): void {
        this.loadParents();
    }

    private initializeColumns(): void {
        this.columns = [
            {
                field: 'code',
                header: this.localizationService.localize('Product::ProLineOfBusiness:Code'),
                sortable: true,
                width: '150px'
            },
            {
                field: 'name',
                header: this.localizationService.localize('Product::ProLineOfBusiness:Name'),
                sortable: true,
                width: '250px'
            },
            {
                field: 'parentName',
                header: 'Nghiệp vụ cha', // Using raw string/key as per wireframe requirement if key not available
                sortable: false,
                width: '200px'
            },
            {
                field: 'status',
                header: this.localizationService.localize('Product::ProLineOfBusiness:Status'),
                sortable: true,
                width: '100px',
                type: 'text',
                freeze: 'right',
                align: 'center',
                formatter: (value: any) => this.formatStatus(value),
                cellClass: (value: any) => {
                    const statusValue = typeof value === 'number' ? value :
                        (value === ProLineOfBusinessStatus.Active ? 0 : 1);
                    return statusValue === 0
                        ? 'px-2 py-1 rounded text-xs font-semibold bg-green-100 text-green-800'
                        : 'px-2 py-1 rounded text-xs font-semibold bg-red-100 text-red-800';
                }
            },
            {
                field: 'description',
                header: this.localizationService.localize('Product::ProLineOfBusiness:Description'),
                sortable: false,
                width: '200px'
            },
            {
                field: 'creationTime',
                header: this.localizationService.localize('AbpIdentity::CreationTime'),
                sortable: true,
                type: 'date',
                width: '180px'
            },
        ];
    }

    formatStatus(status: ProLineOfBusinessStatus | number | string | undefined | null): string {
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
            statusValue = status === ProLineOfBusinessStatus.Active ? 0 : 1;
        }

        return statusValue === 0
            ? this.localizationService.localize('Master::ResWard:Active')
            : this.localizationService.localize('Master::ResWard:Deactive');
    }

    private initializeActions(): void {
        // Always add edit/delete for now, assuming permissions will be checked by backend or directive
        // But better to use safety check if we had permission service working perfectly with known keys
        // For now, I'll add them.

        this.actions.push({
            label: this.localizationService.localize('Product::View'),
            icon: 'pi pi-eye',
            command: (row) => this.openViewDialog(row)
        });

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

        const input: GetProLineOfBusinessesInput = {
            skipCount: event.first || 0,
            maxResultCount: event.rows || this.pageSize,
            sorting: this.getSortingString(event),
            code: this.searchForm.code || undefined,
            name: this.searchForm.name || undefined,
            status: this.searchForm.status !== null ? this.searchForm.status : undefined,
            parentId: this.searchForm.parentId || undefined
        };

        this.service.getList(input).subscribe({
            next: (result) => {
                const items = result.items || [];
                // Map parent names
                this.items = items.map(item => ({
                    ...item,
                    parentName: this.getParentName(item.parentId)
                }));
                this.totalCount = result.totalCount || 0;
                this.loading = false;
            },
            error: (error) => {
                console.error('Error loading data:', error);
                this.loading = false;
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: this.localizationService.localize('AbpUi::InternalServerErrorMessage')
                });
            }
        });
    }

    loadParents(): void {
        const input: GetProLineOfBusinessesInput = { maxResultCount: 1000 };
        this.service.getList(input).subscribe({
            next: (result) => {
                const items = result.items || [];
                this.parentOptions = items.map(p => ({
                    label: p.name || '',
                    value: p.id || ''
                }));
                // Update dialog options if dialog is not open or if we want to refresh
                // For now, simpler to just start with all options
                this.dialogParentOptions = [...this.parentOptions];

                // If we already have items loaded, re-map names
                if (this.items.length > 0) {
                    this.items = this.items.map(item => ({
                        ...item,
                        parentName: this.getParentName(item.parentId)
                    }));
                }
            }
        });
    }

    getParentName(parentId?: string): string {
        if (!parentId) return '';
        const parent = this.parentOptions.find(p => p.value === parentId);
        return parent ? parent.label : '';
    }



    /**
     * Listen for phím Enter toàn cục để kích hoạt tìm kiếm khi không mở dialog
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
            status: null,
            parentId: null
        };
        this.search();
    }

    openCreateDialog(): void {
        this.dialogMode = 'create';
        this.selectedItem = undefined;
        this.dialogParentOptions = [...this.parentOptions];
        this.formData = null;
        this.formErrors = {};
        this.dialogVisible = true;
        setTimeout(() => {
            this.formData = {
                status: ProLineOfBusinessStatus.Active
            };
        });
    }

    openViewDialog(item: ProLineOfBusinessDto): void {
        this.dialogMode = 'view';
        this.selectedItem = item;
        this.dialogParentOptions = this.parentOptions;
        this.formData = null;
        this.dialogVisible = true;
        setTimeout(() => {
            this.formData = { ...item };
        });
    }

    openEditDialog(item: ProLineOfBusinessDto): void {
        this.dialogMode = 'edit';
        this.selectedItem = item;
        this.dialogParentOptions = this.parentOptions.filter(p => p.value !== item.id);
        this.formData = null;
        this.formErrors = {};
        this.dialogVisible = true;
        setTimeout(() => {
            this.formData = { ...item };
        });
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
        this.formErrors = {};

        // Required: Code
        if (!this.formData?.code?.trim()) {
            this.formErrors.code = true;
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Product::ProLineOfBusiness:CodeIsRequired')
            });
            return false;
        }

        // Required: Name
        if (!this.formData?.name?.trim()) {
            this.formErrors.name = true;
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Product::ProLineOfBusiness:NameIsRequired')
            });
            return false;
        }

        // Required: Status
        if (this.formData?.status === undefined || this.formData?.status === null) {
            this.formErrors.status = true;
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Product::ProLineOfBusiness:RequiredFieldIsEmpty')
            });
            return false;
        }

        // Validation for special characters in Code
        const validCodeRegex = /^[a-zA-Z0-9_]+$/;
        if (!validCodeRegex.test(this.formData!.code!)) {
            this.formErrors.code = true;
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Product::ProLineOfBusiness:CodeInvalidFormat')
            });
            setTimeout(() => {
                const element = document.getElementById('code');
                if (element) element.focus();
            }, 0);
            return false;
        }

        return true;
    }

    private create(): void {
        this.loading = true;
        const input = this.formData as CreateProLineOfBusinessDto;
        this.service.create(input).subscribe({
            next: () => {
                this.messageService.add({
                    severity: 'success',
                    summary: this.localizationService.localize('AbpUi::Success'),
                    detail: this.localizationService.localize('Product::ProLineOfBusiness:CreatedSuccessfully')
                });
                this.dialogVisible = false;
                this.search();
                this.loadParents();
            },
            error: (err) => {
                this.loading = false;
                console.error(err);
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: err.error?.error?.message || 'Error executing action'
                });
            }
        });
    }

    private update(): void {
        if (!this.selectedItem?.id) return;
        this.loading = true;
        const input = this.formData as UpdateProLineOfBusinessDto;
        this.service.update(this.selectedItem.id, input).subscribe({
            next: () => {
                this.messageService.add({
                    severity: 'success',
                    summary: this.localizationService.localize('AbpUi::Success'),
                    detail: this.localizationService.localize('Product::ProLineOfBusiness:UpdatedSuccessfully')
                });
                this.dialogVisible = false;
                this.search();
            },
            error: (err) => {
                this.loading = false;
                console.error(err);
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: err.error?.error?.message || 'Error executing action'
                });
            }
        });
    }

    deleteItem(item: ProLineOfBusinessDto): void {
        const id = item.id;
        if (!id) return;

        this.confirmationService.confirm({
            message: this.localizationService.localize('AbpUi::AreYouSure'),
            header: this.localizationService.localize('Product::Delete'),
            icon: 'pi pi-exclamation-triangle',
            accept: () => {
                this.loading = true;
                this.service.delete(id).subscribe({
                    next: () => {
                        this.messageService.add({
                            severity: 'success',
                            summary: this.localizationService.localize('AbpUi::Success'),
                            detail: this.localizationService.localize('Product::ProLineOfBusiness:DeletedSuccessfully')
                        });
                        this.search();
                    },
                    error: (err) => {
                        this.loading = false;
                        this.messageService.add({
                            severity: 'error',
                            summary: this.localizationService.localize('AbpUi::Error'),
                            detail: err.error?.error?.message || 'Error executing action'
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
}
