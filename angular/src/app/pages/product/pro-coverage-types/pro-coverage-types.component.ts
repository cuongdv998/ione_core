import { Component, HostListener, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { TextareaModule } from 'primeng/textarea';
import { TableLazyLoadEvent } from 'primeng/table';
import { MessageService, ConfirmationService, SelectItem } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { DialogModule } from 'primeng/dialog';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TooltipModule } from 'primeng/tooltip';

import { VTable } from '@/shared/components/v-table/v-table';
import { TableColumn } from '@/shared/models/table-column.model';
import { TableAction } from '@/shared/models/table-action.model';
import { ProCoverageTypeService } from '@/proxy/product/controllers/pro-coverage-type.service';
import { CreateProCoverageTypeDto, GetProCoverageTypesInput, ProCoverageTypeDto, UpdateProCoverageTypeDto } from '@/proxy/product/pro-coverage-types/models';
import { ProCoverageTypeStatus } from '@/proxy/pro-coverage-types';
import { ProCoverageTypeSearchForm } from './pro-coverage-types.models';
import { PermissionPipe } from '@/core/pipes/permission.pipe';
import { PermissionService } from '@/core/services/permission.service';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';

@Component({
    selector: 'app-pro-coverage-types',
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
    templateUrl: './pro-coverage-types.component.html',
    styleUrl: './pro-coverage-types.component.scss',
    providers: [MessageService, ConfirmationService]
})
export class ProCoverageTypesComponent implements OnInit {
    readonly PERMISSIONS = {
        CREATE: 'Product.ProCoverageType.Create',
        UPDATE: 'Product.ProCoverageType.Update',
        DELETE: 'Product.ProCoverageType.Delete',
        VIEW: 'Product.ProCoverageType'
    };

    // Data
    items: ProCoverageTypeDto[] = [];
    totalCount = 0;
    loading = true;
    pageSize = 10;

    // Dialog
    dialogVisible = false;
    dialogMode: 'create' | 'edit' = 'create';
    selectedItem?: ProCoverageTypeDto;

    // Form Data
    @ViewChild('form') form?: NgForm;
    formData: Partial<CreateProCoverageTypeDto> = {};

    // Search
    searchForm: ProCoverageTypeSearchForm = {
        code: null,
        name: null,
        status: null
    };

    // Options

    currentLazyLoadEvent?: TableLazyLoadEvent;
    columns: TableColumn[] = [];
    actions: TableAction<ProCoverageTypeDto>[] = [];
    statusOptions: { label: string, value: ProCoverageTypeStatus }[] = [];
    constructor(
        private service: ProCoverageTypeService,
        private messageService: MessageService,
        private confirmationService: ConfirmationService,
        private permissionService: PermissionService,
        private localizationService: LocalizationService
    ) {
        this.initializeColumns();
        this.initializeActions();
    }

    ngOnInit(): void {
        this.statusOptions = [
            { label: this.localizationService.localize('Product::Active'), value: ProCoverageTypeStatus.Active },
            { label: this.localizationService.localize('Product::Inactive'), value: ProCoverageTypeStatus.Deactive }
        ];
    }

    private initializeColumns(): void {
        this.columns = [
            {
                field: 'code',
                header: this.localizationService.localize('Product::ProCoverageType:Code'),
                sortable: true,
                width: '150px'
            },
            {
                field: 'name',
                header: this.localizationService.localize('Product::ProCoverageType:Name'),
                sortable: true,
                width: '250px'
            },
            {
                field: 'status',
                header: this.localizationService.localize('Product::ProCoverageType:Status'),
                sortable: true,
                width: '100px',
                formatter: (value) => this.formatStatus(value),
                cellClass: (value) => this.getStatusClass(value)
            },
            {
                field: 'description',
                header: this.localizationService.localize('Product::ProCoverageType:Description'),
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

    private formatStatus(status: ProCoverageTypeStatus): string {
        const option = this.statusOptions.find(o => o.value === status);
        return option ? option.label : '';
    }

    private getStatusClass(status: ProCoverageTypeStatus): string {
        return status === ProCoverageTypeStatus.Active
            ? 'px-2 py-1 rounded text-xs font-semibold bg-green-100 text-green-800'
            : 'px-2 py-1 rounded text-xs font-semibold bg-red-100 text-red-800';
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

        const input: GetProCoverageTypesInput = {
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
            status: null
        };
        this.search();
    }

    openCreateDialog(): void {
        this.dialogMode = 'create';
        this.formData = {
            status: ProCoverageTypeStatus.Active // Default
        };
        this.selectedItem = undefined;
        this.dialogVisible = true;
    }

    openEditDialog(item: ProCoverageTypeDto): void {
        this.dialogMode = 'edit';
        this.selectedItem = item;
        this.formData = {
            ...item
        };
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
        const input = this.formData as CreateProCoverageTypeDto;
        this.service.create(input).subscribe({
            next: () => {
                this.messageService.add({
                    severity: 'success',
                    summary: this.localizationService.localize('AbpUi::Success'),
                    detail: this.localizationService.localize('Product::ProCoverageType:Created')
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

    private update(): void {
        if (!this.selectedItem?.id) return;
        this.loading = true;
        const input = this.formData as UpdateProCoverageTypeDto;
        this.service.update(this.selectedItem.id, input).subscribe({
            next: () => {
                this.messageService.add({
                    severity: 'success',
                    summary: this.localizationService.localize('AbpUi::Success'),
                    detail: this.localizationService.localize('Product::ProCoverageType:Updated')
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

    deleteItem(item: ProCoverageTypeDto): void {
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
                            detail: this.localizationService.localize('Product::ProCoverageType:Deleted')
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
