import { Component, HostListener, OnInit, ViewChild } from '@angular/core';
import { CommonModule, formatDate } from '@angular/common';
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
import { PermissionPipe } from '@/core/pipes/permission.pipe';
import { PermissionService } from '@/core/services/permission.service';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';

import { ProCoverageService } from '@/proxy/product/controllers/pro-coverage.service';
import { ProLineOfBusinessService } from '@/proxy/product/controllers/pro-line-of-business.service';
import { ProCoverageGroupService } from '@/proxy/product/controllers/pro-coverage-group.service';
import { ProCoverageTypeService } from '@/proxy/product/controllers/pro-coverage-type.service';
import { ResObjectTypeService } from '@/proxy/master/controllers/res-object-type.service';

import { GetProCoveragesInput, ProCoverageDto, CreateProCoverageDto, UpdateProCoverageDto } from '@/proxy/product/pro-coverages/models';
import { ProCoverageTermType, proCoverageTermTypeOptions } from '@/proxy/pro-coverages/pro-coverage-term-type.enum';
import { ProCoverageStatus } from '@/proxy/pro-coverages/pro-coverage-status.enum';
import { ProCoverageSearchForm } from './pro-coverages.models';
import { forkJoin, catchError, of } from 'rxjs';
import { ProLineOfBusinessStatus } from '@/proxy/pro-line-of-businesses';
import { ProCoverageGroupStatus } from '@/proxy/pro-coverage-groups';
import { ProCoverageTypeStatus } from '@/proxy/pro-coverage-types';
import { ResObjectTypeStatus } from '@/proxy/res-object-types';

@Component({
    selector: 'app-pro-coverages',
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
    templateUrl: './pro-coverages.component.html',
    styleUrl: './pro-coverages.component.scss',
    providers: [MessageService, ConfirmationService]
})
export class ProCoveragesComponent implements OnInit {
    readonly PERMISSIONS = {
        CREATE: 'Product.ProCoverage.Create',
        UPDATE: 'Product.ProCoverage.Update',
        DELETE: 'Product.ProCoverage.Delete',
        VIEW: 'Product.ProCoverage'
    };

    // Data
    items: ProCoverageDto[] = [];
    totalCount = 0;
    loading = true;
    pageSize = 10;

    // Dropdown Options
    lobOptions: { label: string, value: string }[] = [];
    groupOptions: { label: string, value: string }[] = [];
    coverageTypeOptions: { label: string, value: string }[] = [];
    objectTypeOptions: { label: string, value: string }[] = [];

    // Search Options (All)
    lobSearchOptions: { label: string, value: string }[] = [];
    groupSearchOptions: { label: string, value: string }[] = [];
    coverageTypeSearchOptions: { label: string, value: string }[] = [];
    objectTypeSearchOptions: { label: string, value: string }[] = [];

    // Static Options
    typeOptions = proCoverageTermTypeOptions;

    statusOptions = [
        { label: 'Active', value: ProCoverageStatus.Active },
        { label: 'Inactive', value: ProCoverageStatus.Deactive }
    ];

    // Dialog
    dialogVisible = false;
    dialogMode: 'create' | 'edit' = 'create';
    selectedItem?: ProCoverageDto;

    // Form Data
    @ViewChild('form') form?: NgForm;
    formData: Partial<CreateProCoverageDto> = {};

    // Search
    searchForm: ProCoverageSearchForm = {
        lobId: null,
        coverageGroupId: null,
        coverageTypeId: null,
        objectTypeId: null,
        type: null,
        code: null,
        name: null,
        status: null
    };

    currentLazyLoadEvent?: TableLazyLoadEvent;
    columns: TableColumn[] = [];
    actions: TableAction<ProCoverageDto>[] = [];

    constructor(
        private service: ProCoverageService,
        private lobService: ProLineOfBusinessService,
        private groupService: ProCoverageGroupService,
        private coverageTypeService: ProCoverageTypeService,
        private objectTypeService: ResObjectTypeService,
        private messageService: MessageService,
        private confirmationService: ConfirmationService,
        private permissionService: PermissionService,
        private localizationService: LocalizationService
    ) {
        this.initializeColumns();
        this.initializeActions();
    }

    ngOnInit(): void {
        this.loadReferenceData();
    }

    private loadReferenceData(): void {
        forkJoin({
            lobs: this.lobService.getList({ maxResultCount: 1000 }).pipe(catchError(() => of({ items: [] } as any))),
            groups: this.groupService.getList({ maxResultCount: 1000 }).pipe(catchError(() => of({ items: [] } as any))),
            coverageTypes: this.coverageTypeService.getList({ maxResultCount: 1000 }).pipe(catchError(() => of({ items: [] } as any))),
            objectTypes: this.objectTypeService.getList({ maxResultCount: 1000 }).pipe(catchError(() => of({ items: [] } as any)))
        }).subscribe({
            next: (results) => {
                // Dialog Options (Active Only)
                this.lobOptions = (results.lobs.items || []).filter((x: any) => x.status === ProLineOfBusinessStatus.Active).map((x: any) => ({ label: x.name || '', value: x.id || '' }));
                this.groupOptions = (results.groups.items || []).filter((x: any) => x.status === ProCoverageGroupStatus.Active).map((x: any) => ({ label: x.name || '', value: x.id || '' }));
                this.coverageTypeOptions = (results.coverageTypes.items || []).filter((x: any) => x.status === ProCoverageTypeStatus.Active).map((x: any) => ({ label: x.name || '', value: x.id || '' }));
                this.objectTypeOptions = (results.objectTypes.items || []).filter((x: any) => x.status === ResObjectTypeStatus.Active).map((x: any) => ({ label: x.name || '', value: x.id || '' }));

                // Search Options (All)
                this.lobSearchOptions = (results.lobs.items || []).map((x: any) => ({ label: x.name || '', value: x.id || '' }));
                this.groupSearchOptions = (results.groups.items || []).map((x: any) => ({ label: x.name || '', value: x.id || '' }));
                this.coverageTypeSearchOptions = (results.coverageTypes.items || []).map((x: any) => ({ label: x.name || '', value: x.id || '' }));
                this.objectTypeSearchOptions = (results.objectTypes.items || []).map((x: any) => ({ label: x.name || '', value: x.id || '' }));

                this.typeOptions = proCoverageTermTypeOptions.map(opt => ({
                    key: opt.key,
                    value: opt.value,
                    label: this.localizationService.localize('Product::Enum:ProCoverageTermType.' + opt.key)
                }));

                this.statusOptions = [
                    { label: this.localizationService.localize('Product::ProCoverage:Active'), value: ProCoverageStatus.Active },
                    { label: this.localizationService.localize('Product::ProCoverage:Inactive'), value: ProCoverageStatus.Deactive }
                ];
            },
            error: (err) => console.error('Error loading references', err)
        });
    }

    private initializeColumns(): void {
        this.columns = [
            {
                field: 'lobName', // Derived
                header: this.localizationService.localize('Product::ProCoverage:LineOfBusiness'),
                sortable: false,
                width: '150px',
                formatter: (_v, row) => this.getOptionLabel(this.lobOptions, row.lobId)
            },
            {
                field: 'groupName', // Derived
                header: this.localizationService.localize('Product::ProCoverage:Group'),
                sortable: false,
                width: '150px',
                formatter: (_v, row) => this.getOptionLabel(this.groupOptions, row.coverageGroupId)
            },
            {
                field: 'code',
                header: this.localizationService.localize('Product::ProCoverage:Code'),
                sortable: true,
                width: '120px'
            },
            {
                field: 'name',
                header: this.localizationService.localize('Product::ProCoverage:Name'),
                sortable: true,
                width: '200px'
            },
            {
                field: 'type',
                header: this.localizationService.localize('Product::ProCoverage:Type'),
                sortable: true,
                width: '150px',
                formatter: (value) => {
                    const option = proCoverageTermTypeOptions.find(opt => opt.value === value);
                    return option ? this.localizationService.localize('Product::Enum:ProCoverageTermType.' + option.key) : '';
                }
            },
            {
                field: 'status',
                header: this.localizationService.localize('Product::ProCoverage:Status'),
                sortable: true,
                width: '100px',
                formatter: (value) => this.formatStatus(value),
                cellClass: (value) => this.getStatusClass(value)
            },
            {
                field: 'creationTime',
                header: this.localizationService.localize('Product::CreatedDate'),
                sortable: true,
                width: '150px',
                formatter: (value) => value ? formatDate(value, 'dd/MM/yyyy', 'vi-VN') : ''
            }
        ];
    }

    private getOptionLabel(options: { label: string, value: string }[], value?: string): string {
        const opt = options.find(o => o.value === value);
        return opt ? opt.label : '';
    }

    private formatStatus(status: ProCoverageStatus): string {
        const option = this.statusOptions.find(o => o.value === status);
        return option ? option.label : '';
    }

    private getStatusClass(status: ProCoverageStatus): string {
        return status === ProCoverageStatus.Active
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

        const input: GetProCoveragesInput = {
            skipCount: event.first || 0,
            maxResultCount: event.rows || this.pageSize,
            sorting: this.getSortingString(event),
            lobId: this.searchForm.lobId || undefined,
            coverageGroupId: this.searchForm.coverageGroupId || undefined,
            coverageTypeId: this.searchForm.coverageTypeId || undefined,
            objectTypeId: this.searchForm.objectTypeId || undefined,
            type: this.searchForm.type || undefined,
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
            lobId: null,
            coverageGroupId: null,
            coverageTypeId: null,
            objectTypeId: null,
            type: null,
            code: null,
            name: null,
            status: null
        };
        this.search();
    }

    openCreateDialog(): void {
        this.dialogMode = 'create';
        this.formData = {
            status: ProCoverageStatus.Active
        };
        this.selectedItem = undefined;
        this.dialogVisible = true;
    }

    openEditDialog(item: ProCoverageDto): void {
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

        if (!this.validate()) return;

        if (this.dialogMode === 'create') {
            this.create();
        } else {
            this.update();
        }
    }

    private validate(): boolean {
        if (!this.formData.lobId) {
            this.showError('Product::ProCoverage:LobIdRequired');
            return false;
        }
        if (!this.formData.coverageGroupId) {
            this.showError('Product::ProCoverage:CoverageGroupIdRequired');
            return false;
        }
        if (!this.formData.code?.trim()) {
            this.showError('Product::ProCoverage:CodeRequired');
            return false;
        }
        if (!/^[a-zA-Z0-9_]+$/.test((this.formData.code || '').trim())) {   
            this.showError('Product::ProCoverage:CodeHasSpecialChar');
            return false;
        }
        if (!this.formData.name?.trim()) {
            this.showError('Product::ProCoverage:NameRequired');
            return false;
        }
        if (!this.formData.shortName?.trim()) {
            this.showError('Product::ProCoverage:ShortNameRequired');
            return false;
        }
        if (!this.formData.objectTypeId) {
            this.showError('Product::ProCoverage:ObjectTypeIdRequired');
            return false;
        }
        if (!this.formData.coverageTypeId) {
            this.showError('Product::ProCoverage:CoverageTypeIdRequired');
            return false;
        }
        if (this.formData.type === undefined || this.formData.type === null) {
            this.showError('Product::ProCoverage:TypeRequired');
            return false;
        }
        if (this.formData.status === undefined || this.formData.status === null) {
            this.showError('Product::ProCoverage:StatusRequired');
            return false;
        }

        return true;
    }

    private showError(key: string): void {
        this.messageService.add({
            severity: 'error',
            summary: this.localizationService.localize('AbpUi::Error'),
            detail: this.localizationService.localize(key)
        });
    }

    private create(): void {
        this.loading = true;
        const input = this.formData as CreateProCoverageDto;
        this.service.create(input).subscribe({
            next: () => {
                this.messageService.add({
                    severity: 'success',
                    summary: this.localizationService.localize('AbpUi::Success'),
                    detail: this.localizationService.localize('Product::ProCoverage:Created')
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
        const input = this.formData as UpdateProCoverageDto;
        this.service.update(this.selectedItem.id, input).subscribe({
            next: () => {
                this.messageService.add({
                    severity: 'success',
                    summary: this.localizationService.localize('AbpUi::Success'),
                    detail: this.localizationService.localize('Product::ProCoverage:Updated')
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

    deleteItem(item: ProCoverageDto): void {
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
                            detail: this.localizationService.localize('Product::ProCoverage:Deleted')
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
