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
import { ProCoverageGroupService } from '@/proxy/product/controllers/pro-coverage-group.service';
import { ProCoverageGroupDto, CreateProCoverageGroupDto, UpdateProCoverageGroupDto, GetProCoverageGroupsInput } from '@/proxy/product/pro-coverage-groups/models';
import { ProCoverageGroupStatus } from '@/proxy/pro-coverage-groups';
import { ProCoverageGroupSearchForm } from './pro-coverage-groups.models';
import { PermissionPipe } from '@/core/pipes/permission.pipe';
import { PermissionService } from '@/core/services/permission.service';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';

@Component({
    selector: 'app-pro-coverage-groups',
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
    templateUrl: './pro-coverage-groups.component.html',
    styleUrl: './pro-coverage-groups.component.scss',
    providers: [MessageService, ConfirmationService]
})
export class ProCoverageGroupsComponent implements OnInit {
    readonly PERMISSIONS = {
        CREATE: 'Product.ProCoverageGroup.Create',
        UPDATE: 'Product.ProCoverageGroup.Update',
        DELETE: 'Product.ProCoverageGroup.Delete',
        VIEW: 'Product.ProCoverageGroup'
    };

    // Data
    items: ProCoverageGroupDto[] = [];
    totalCount = 0;
    loading = true;
    pageSize = 10;

    // Dialog
    dialogVisible = false;
    dialogMode: 'create' | 'edit' = 'create';
    selectedItem?: ProCoverageGroupDto;
    submitted = false;

    // Form Data
    formData: Partial<CreateProCoverageGroupDto> = {};

    // Search
    searchForm: ProCoverageGroupSearchForm = {
        code: null,
        name: null,
        status: null
    };

    // Options
    statusOptions: { label: string; value: number }[] = [];

    currentLazyLoadEvent?: TableLazyLoadEvent;
    columns: TableColumn[] = [];
    actions: TableAction<ProCoverageGroupDto>[] = [];

    @ViewChild('form') form?: NgForm;

    constructor(
        private service: ProCoverageGroupService,
        private messageService: MessageService,
        private confirmationService: ConfirmationService,
        private permissionService: PermissionService,
        private localizationService: LocalizationService
    ) {
        this.initializeStatusOptions();
        this.initializeColumns();
        this.initializeActions();
    }

    ngOnInit(): void {
    }

    /**
     * Initialize status options with localized labels
     */
    private initializeStatusOptions(): void {
        this.statusOptions = [
            { label: this.localizationService.localize('Product::ProCoverageGroup:Active'), value: ProCoverageGroupStatus.Active },
            { label: this.localizationService.localize('Product::ProCoverageGroup:Deactive'), value: ProCoverageGroupStatus.Deactive }
        ];
    }

    private initializeColumns(): void {
        this.columns = [
            {
                field: 'code',
                header: this.localizationService.localize('Product::ProCoverageGroup:Code'),
                sortable: true,
                width: '150px'
            },
            {
                field: 'name',
                header: this.localizationService.localize('Product::ProCoverageGroup:Name'),
                sortable: true,
                width: '250px'
            },
            {
                field: 'status',
                header: this.localizationService.localize('Product::ProCoverageGroup:Status'),
                sortable: true,
                width: '100px',
                formatter: (value) => this.formatStatus(value),
                cellClass: (value) => this.getStatusClass(value)
            },
            {
                field: 'description',
                header: this.localizationService.localize('Product::ProCoverageGroup:Description'),
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

    formatStatus(status: number): string {
        return status === ProCoverageGroupStatus.Active
            ? this.localizationService.localize('Product::ProCoverageGroup:Active')
            : this.localizationService.localize('Product::ProCoverageGroup:Deactive');
    }

    getStatusClass(status: number): string {
        return status === ProCoverageGroupStatus.Active
            ? 'px-2 py-1 rounded text-xs font-semibold bg-green-100 text-green-800'
            : 'px-2 py-1 rounded text-xs font-semibold bg-red-100 text-red-800';
    }

    loadData(event: TableLazyLoadEvent): void {
        this.currentLazyLoadEvent = event;
        this.loading = true;

        const input: GetProCoverageGroupsInput = {
            skipCount: event.first || 0,
            maxResultCount: event.rows || this.pageSize,
            sorting: this.getSortingString(event),
            code: this.searchForm.code || undefined,
            name: this.searchForm.name || undefined,
            status: this.searchForm.status !== null ? this.searchForm.status : undefined
        };

        this.service.getList(input).subscribe({
            next: (result: any) => {
                this.items = result.items || [];
                this.totalCount = result.totalCount || 0;
                this.loading = false;
            },
            error: (error: any) => {
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
            status: ProCoverageGroupStatus.Active
        };
        this.submitted = false;
        this.selectedItem = undefined;
        this.dialogVisible = true;
        this.resetValidation();
    }

    openEditDialog(item: ProCoverageGroupDto): void {
        this.dialogMode = 'edit';
        this.selectedItem = item;
        this.formData = {
            ...item
        };
        this.submitted = false;
        this.dialogVisible = true;
        this.resetValidation();
    }

    onDialogHide(): void {
        this.form?.resetForm();
    }

    private resetValidation(): void {
        // Handled by onDialogHide or resetForm
    }

    save(): void {
        this.submitted = true;
        
        // Mark all fields as touched to show validation errors
        if (this.form) {
            Object.keys(this.form.controls).forEach(key => {
                this.form?.controls[key]?.markAsTouched();
            });
        }

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
        const input = this.formData as CreateProCoverageGroupDto;
        this.service.create(input).subscribe({
            next: () => {
                this.messageService.add({
                    severity: 'success',
                    summary: this.localizationService.localize('AbpUi::Success'),
                    detail: this.localizationService.localize('Product::ProCoverageGroup:Created')
                });
                this.dialogVisible = false;
                this.search();
            },
            error: (err: any) => {
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
        const input = this.formData as UpdateProCoverageGroupDto;
        this.service.update(this.selectedItem.id, input).subscribe({
            next: () => {
                this.messageService.add({
                    severity: 'success',
                    summary: this.localizationService.localize('AbpUi::Success'),
                    detail: this.localizationService.localize('Product::ProCoverageGroup:Updated')
                });
                this.dialogVisible = false;
                this.search();
            },
            error: (err: any) => {
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

    deleteItem(item: ProCoverageGroupDto): void {
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
                            detail: this.localizationService.localize('Product::ProCoverageGroup:Deleted')
                        });
                        this.search();
                    },
                    error: (err: any) => {
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
