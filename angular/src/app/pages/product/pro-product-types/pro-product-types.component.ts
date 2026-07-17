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
import { ProProductTypeService } from '@/proxy/product/controllers/pro-product-type.service';
import { GetProProductTypesInput, ProProductTypeDto, CreateProProductTypeDto, UpdateProProductTypeDto } from '@/proxy/product/pro-product-types/models';
import { ProProductTypeStatus } from '@/proxy/pro-product-types';
import { PermissionPipe } from '@/core/pipes/permission.pipe';
import { PermissionService } from '@/core/services/permission.service';

import { LocalizationService } from '@/core/services/localization.service';
import { ProLineOfBusinessService } from '@/proxy/product/controllers/pro-line-of-business.service';
import { ProLineOfBusinessDto } from '@/proxy/product/pro-line-of-businesses/models';
import { ProLineOfBusinessStatus } from '@/proxy/pro-line-of-businesses';

@Component({
    selector: 'app-pro-product-types',
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
        PermissionPipe
    ],
    templateUrl: './pro-product-types.component.html',
    styleUrl: './pro-product-types.component.scss',
    providers: [MessageService, ConfirmationService]
})
export class ProProductTypesComponent implements OnInit {
    // Permissions
    readonly PERMISSIONS = {
        CREATE: 'Product.ProProductType.Create',
        UPDATE: 'Product.ProProductType.Update',
        DELETE: 'Product.ProProductType.Delete',
        VIEW: 'Product.ProProductType'
    };

    // Data
    items: (ProProductTypeDto & { lobName?: string })[] = [];
    totalCount = 0;
    loading = true;
    pageSize = 10;

    // LOB Options
    lobOptions: { label: string, value: string }[] = [];
    fullLobOptions: { label: string, value: string }[] = [];

    // Dialog
    dialogVisible = false;
    dialogMode: 'create' | 'edit' = 'create';
    selectedItem?: ProProductTypeDto;

    // Form Data
    formData: Partial<CreateProProductTypeDto> = {};

    // Search
    searchForm: {
        code?: string | null;
        name?: string | null;
        status?: ProProductTypeStatus | null;
        lobId?: string | null;
    } = {
            code: null,
            name: null,
            status: null,
            lobId: null
        };

    // Options

    currentLazyLoadEvent?: TableLazyLoadEvent;
    columns: TableColumn[] = [];
    actions: TableAction<ProProductTypeDto>[] = [];

    statusOptions: { label: string, value: ProProductTypeStatus }[] = [];
    constructor(
        private service: ProProductTypeService,
        private lobService: ProLineOfBusinessService,
        private messageService: MessageService,
        private confirmationService: ConfirmationService,
        private permissionService: PermissionService,
        private localizationService: LocalizationService
    ) {
        this.initializeColumns();
        this.initializeActions();
    }

    ngOnInit(): void {
        this.loadLOBs();
        this.statusOptions = [
            { label: this.localizationService.localize('Product::ProductType:Active'), value: ProProductTypeStatus.Active },
            { label: this.localizationService.localize('Product::ProductType:Deactive'), value: ProProductTypeStatus.Deactive }
        ];
    }


    private initializeColumns(): void {
        this.columns = [
            {
                field: 'code',
                header: 'Mã loại sản phẩm',
                sortable: true,
                width: '150px'
            },
            {
                field: 'name',
                header: 'Tên loại sản phẩm',
                sortable: true,
                width: '250px'
            },
            {
                field: 'lobName',
                header: 'Nghiệp vụ',
                sortable: false,
                width: '200px'
            },
            {
                field: 'status',
                header: 'Trạng thái',
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
                header: 'Mô tả',
                sortable: false
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
        this.actions.push({
            label: 'Chỉnh sửa',
            icon: 'pi pi-pencil',
            command: (row) => this.openEditDialog(row)
        });

        this.actions.push({
            label: 'Xóa',
            icon: 'pi pi-trash',
            command: (row) => this.deleteItem(row)
        });
    }

    loadData(event: TableLazyLoadEvent): void {
        this.currentLazyLoadEvent = event;
        this.loading = true;

        const input: GetProProductTypesInput = {
            skipCount: event.first || 0,
            maxResultCount: event.rows || this.pageSize,
            sorting: this.getSortingString(event),
            code: this.searchForm.code || undefined,
            name: this.searchForm.name || undefined,
            status: this.searchForm.status !== null ? this.searchForm.status : undefined,
            lobId: this.searchForm.lobId || undefined
        };

        this.service.getList(input).subscribe({
            next: (result) => {
                const items = result.items || [];
                // Map LOB names
                this.items = items.map(item => ({
                    ...item,
                    lobName: this.getLobName(item.lobId)
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

    loadLOBs(): void {
        const input = { maxResultCount: 1000 };
        this.lobService.getList(input).subscribe({
            next: (result) => {
                const items = result.items || [];
                this.lobOptions = items.filter(p => p.status === ProLineOfBusinessStatus.Active).map(p => ({
                    label: p.name || '',
                    value: p.id || ''
                }));
                this.fullLobOptions = items.map(p => ({
                    label: p.name || '',
                    value: p.id || ''
                }));

                // Refresh data to update Lob Names if data loaded first
                if (this.items.length > 0 && !this.items[0].lobName) {
                    this.items = this.items.map(item => ({
                        ...item,
                        lobName: this.getLobName(item.lobId)
                    }));
                }
            }
        });
    }

    getLobName(lobId?: string): string {
        if (!lobId) return '';
        const lob = this.fullLobOptions.find(p => p.value === lobId);
        return lob ? lob.label : '';
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
            lobId: null
        };
        this.search();
    }

    openCreateDialog(): void {
        this.dialogMode = 'create';
        this.formData = {
            status: ProProductTypeStatus.Active
        };
        this.selectedItem = undefined;
        this.dialogVisible = true;
    }

    openEditDialog(item: ProProductTypeDto): void {
        this.dialogMode = 'edit';
        this.selectedItem = item;
        this.formData = {
            ...item
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
        if (!this.formData) return false;
        // Validate LOB
        if (!this.formData.lobId) {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: 'Vui lòng chọn nghiệp vụ'
            });
            return false;
        }
        // Validate Code
        if (!this.formData.code) {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Product::ProProductType:CodeRequired')
            });
            return false;
        }

        if (this.formData.code.includes(' ')) {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Product::ProProductType:CodeInvalidFormat')
            });
            return false;
        }

        const codeRegex = /^[a-zA-Z0-9_]*$/;
        if (!codeRegex.test(this.formData.code)) {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Product::ProProductType:CodeInvalidFormat')
            });
            return false;
        }

        if (this.formData.code.length > 50) {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Product::ProProductType:CodeMaxLength')
            });
            return false;
        }

        // Validate Name
        if (!this.formData.name) {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Product::ProProductType:NameRequired')
            });
            return false;
        }

        if (this.formData.name.length > 250) {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Product::ProProductType:NameMaxLength')
            });
            return false;
        }

        // Validate Status
        if (this.formData.status === null || this.formData.status === undefined) {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Product::ProProductType:StatusRequired')
            });
            return false;
        }

        return true;
    }

    private create(): void {
        this.loading = true;
        const input = this.formData as CreateProProductTypeDto;
        this.service.create(input).subscribe({
            next: () => {
                this.messageService.add({
                    severity: 'success',
                    summary: this.localizationService.localize('AbpUi::Success'),
                    detail: this.localizationService.localize('Product::ProProductType:Created')
                });
                this.dialogVisible = false;
                this.search();
            },
            error: (err) => {
                this.loading = false;
                console.error(err);
                const message = err.error?.error?.message || err.error?.error?.details || 'Error executing action';
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: message
                });
            }
        });
    }

    private update(): void {
        if (!this.selectedItem?.id) return;
        this.loading = true;
        const input = this.formData as UpdateProProductTypeDto;
        this.service.update(this.selectedItem.id, input).subscribe({
            next: () => {
                this.messageService.add({
                    severity: 'success',
                    summary: this.localizationService.localize('AbpUi::Success'),
                    detail: this.localizationService.localize('Product::ProProductType:Updated')
                });
                this.dialogVisible = false;
                this.search();
            },
            error: (err) => {
                this.loading = false;
                console.error(err);
                const message = err.error?.error?.message || err.error?.error?.details || 'Error executing action';
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: message
                });
            }
        });
    }

    deleteItem(item: ProProductTypeDto): void {
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
                            detail: this.localizationService.localize('Product::ProProductType:Deleted')
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
