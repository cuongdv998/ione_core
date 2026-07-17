import { Component, HostListener, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
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
import { ProProductCategoryService } from '@/proxy/product/controllers/pro-product-category.service';
import { ProLineOfBusinessService } from '@/proxy/product/controllers/pro-line-of-business.service';
import { GetProProductCategorysInput, ProProductCategoryDto, CreateProProductCategoryDto, UpdateProProductCategoryDto } from '@/proxy/product/pro-product-categorys/models';
import { ProProductCategoryStatus } from '@/proxy/pro-product-categorys';
import { ProLineOfBusinessDto } from '@/proxy/product/pro-line-of-businesses/models';
import { ProProductCategorySearchForm } from './pro-product-categories.models';
import { PermissionPipe } from '@/core/pipes/permission.pipe';
import { PermissionService } from '@/core/services/permission.service';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';

@Component({
    selector: 'app-pro-product-categories',
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
    templateUrl: './pro-product-categories.component.html',
    styleUrl: './pro-product-categories.component.scss',
    providers: [MessageService, ConfirmationService]
})
export class ProProductCategoriesComponent implements OnInit {
    readonly PERMISSIONS = {
        CREATE: 'Product.ProProductCategory.Create',
        UPDATE: 'Product.ProProductCategory.Update',
        DELETE: 'Product.ProProductCategory.Delete',
        VIEW: 'Product.ProProductCategory'
    };

    // Data
    items: (ProProductCategoryDto & { lobName?: string, parentName?: string })[] = [];
    totalCount = 0;
    loading = true;
    pageSize = 10;

    // Options
    lobOptions: { label: string, value: string }[] = [];
    parentOptions: { label: string, value: string }[] = [];
    dialogParentOptions: { label: string, value: string }[] = [];

    statusOptions: { label: string, value: ProProductCategoryStatus }[] = [];

    // Dialog
    dialogVisible = false;
    dialogMode: 'create' | 'edit' = 'create';
    selectedItem?: ProProductCategoryDto;

    // Form Data
    @ViewChild('form') form?: NgForm;
    formData: Partial<CreateProProductCategoryDto> = {};

    // Search
    searchForm: ProProductCategorySearchForm = {
        code: null,
        name: null,
        lobId: null,
        parentId: null,
        status: null
    };

    currentLazyLoadEvent?: TableLazyLoadEvent;
    columns: TableColumn[] = [];
    actions: TableAction<ProProductCategoryDto>[] = [];

    constructor(
        private service: ProProductCategoryService,
        private lobService: ProLineOfBusinessService,
        private messageService: MessageService,
        private confirmationService: ConfirmationService,
        private permissionService: PermissionService,
        private localizationService: LocalizationService
    ) {
        this.initializeColumns();
        this.initializeActions();
        this.statusOptions = [
            { label: this.localizationService.localize('Product::ProProductCategory:Active'), value: ProProductCategoryStatus.Active },
            { label: this.localizationService.localize('Product::ProProductCategory:Deactive'), value: ProProductCategoryStatus.Deactive }
        ];
    }

    ngOnInit(): void {
        this.loadLookups();
    }

    private initializeColumns(): void {
        this.columns = [
            {
                field: 'code',
                header: this.localizationService.localize('Product::ProProductCategory:Code'),
                sortable: true,
                width: '150px'
            },
            {
                field: 'name',
                header: this.localizationService.localize('Product::ProProductCategory:Name'),
                sortable: true,
                width: '200px'
            },
            {
                field: 'lobName',
                header: this.localizationService.localize('Product::ProProductCategory:LineOfBusiness'),
                sortable: false,
                width: '200px'
            },
            {
                field: 'parentName',
                header: this.localizationService.localize('Product::ProProductCategory:Parent'),
                sortable: false,
                width: '200px'
            },
            {
                field: 'status',
                header: this.localizationService.localize('Product::ProProductCategory:Status'),
                sortable: true,
                width: '100px',
                formatter: (value: ProProductCategoryStatus) => this.formatStatus(value),
                cellClass: (value: ProProductCategoryStatus) => this.getStatusClass(value)
            },
            {
                field: 'description',
                header: this.localizationService.localize('Product::ProProductCategory:Description'),
                sortable: false
            },
            {
                field: 'creationTime',
                header: this.localizationService.localize('AbpIdentity::CreationTime'),
                sortable: true,
                width: '180px',
                type: 'date'
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

    loadLookups(): void {
        // Load LOBs
        this.lobService.getList({ maxResultCount: 1000 }).subscribe({
            next: (result) => {
                this.lobOptions = (result.items || []).map(item => ({
                    label: item.name || '',
                    value: item.id || ''
                }));
                this.enrichItems();
            }
        });

        // Load Parents (Self - for lookup)
        this.service.getList({ maxResultCount: 1000, status: ProProductCategoryStatus.Active }).subscribe({
            next: (result) => {
                this.parentOptions = (result.items || []).map(item => ({
                    label: item.name || '',
                    value: item.id || ''
                }));
                this.dialogParentOptions = [...this.parentOptions];
                this.enrichItems();
            }
        });
    }

    loadData(event: TableLazyLoadEvent): void {
        this.currentLazyLoadEvent = event;
        this.loading = true;

        const input: GetProProductCategorysInput = {
            skipCount: event.first || 0,
            maxResultCount: event.rows || this.pageSize,
            sorting: this.getSortingString(event),
            code: this.searchForm.code || undefined,
            name: this.searchForm.name || undefined,
            lobId: this.searchForm.lobId || undefined,
            status: this.searchForm.status !== null ? this.searchForm.status : undefined,
            parentId: this.searchForm.parentId || undefined
        };

        this.service.getList(input).subscribe({
            next: (result) => {
                this.items = result.items || [];
                this.enrichItems();
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

    enrichItems(): void {
        if (!this.items.length) return;

        this.items = this.items.map(item => ({
            ...item,
            lobName: this.lobOptions.find(opt => opt.value === item.lobId)?.label || '',
            parentName: this.parentOptions.find(opt => opt.value === item.parentId)?.label || ''
        }));
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
            lobId: null,
            parentId: null,
            status: null
        };
        this.search();
    }

    openCreateDialog(): void {
        this.dialogMode = 'create';
        this.formData = {
            status: ProProductCategoryStatus.Active
        };
        this.selectedItem = undefined;
        this.dialogParentOptions = [...this.parentOptions];
        this.dialogVisible = true;
    }

    openEditDialog(item: ProProductCategoryDto): void {
        this.dialogMode = 'edit';
        this.selectedItem = item;
        this.formData = {
            ...item
        };
        // Exclude self from parent options
        this.dialogParentOptions = this.parentOptions.filter(p => p.value !== item.id);
        this.dialogVisible = true;
    }

    onDialogHide(): void {
        this.form?.resetForm();
    }

    save(): void {
        // Trim values before validation
        if (this.formData.code) {
            this.formData.code = this.formData.code.trim();
        }
        if (this.formData.name) {
            this.formData.name = this.formData.name.trim();
        }

        // Mark all fields as touched to show validation errors
        if (this.form) {
            Object.keys(this.form.controls).forEach(key => {
                this.form?.controls[key]?.markAsTouched();
            });
        }

        // Check form validity
        if (this.form?.invalid) {
            return;
        }

        if (this.dialogMode === 'create') {
            this.create();
        } else {
            this.update();
        }
    }

    private create(): void {
        this.loading = true;
        const input = this.formData as CreateProProductCategoryDto;
        this.service.create(input).subscribe({
            next: () => {
                this.messageService.add({
                    severity: 'success',
                    summary: this.localizationService.localize('AbpUi::Success'),
                    detail: this.localizationService.localize('Product::ProProductCategory:Created')
                });
                this.dialogVisible = false;
                this.search(); // Refresh list will also refresh parents potentially? Ideally yes, but search is lazy.
                // We should refresh parent options too as we added a new category
                this.loadLookups();
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
        const input = this.formData as UpdateProProductCategoryDto;
        this.service.update(this.selectedItem.id, input).subscribe({
            next: () => {
                this.messageService.add({
                    severity: 'success',
                    summary: this.localizationService.localize('AbpUi::Success'),
                    detail: this.localizationService.localize('Product::ProProductCategory:Updated')
                });
                this.dialogVisible = false;
                this.search();
                this.loadLookups();
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

    deleteItem(item: ProProductCategoryDto): void {
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
                            detail: this.localizationService.localize('Product::ProProductCategory:Deleted')
                        });
                        this.search();
                        this.loadLookups();
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

    formatStatus(status: ProProductCategoryStatus): string {
        return status === ProProductCategoryStatus.Active ? 'Hoạt động' : 'Không hoạt động';
    }

    getStatusClass(status: ProProductCategoryStatus): string {
        return status === ProProductCategoryStatus.Active
            ? 'px-2 py-1 rounded text-xs font-semibold bg-green-100 text-green-800'
            : 'px-2 py-1 rounded text-xs font-semibold bg-red-100 text-red-800';
    }
}
