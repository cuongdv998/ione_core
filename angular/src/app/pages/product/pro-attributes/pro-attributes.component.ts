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
import { ProAttributeService } from '@/proxy/product/controllers/pro-attribute.service';
import { GetProAttributesInput, ProAttributeDto, CreateProAttributeDto, UpdateProAttributeDto } from '@/proxy/product/pro-attributes';
import { ProAttributeStatus, ProAttributeSpec, ProAttributeDataType, proAttributeSpecOptions, proAttributeDataTypeOptions, proAttributeStatusOptions } from '@/proxy/pro-attributes';
import { ProAttributeSearchForm } from './pro-attributes.models';
import { PermissionPipe } from '@/core/pipes/permission.pipe';
import { PermissionService } from '@/core/services/permission.service';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';

@Component({
    selector: 'app-pro-attributes',
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
    templateUrl: './pro-attributes.component.html',
    styleUrl: './pro-attributes.component.scss',
    providers: [MessageService, ConfirmationService]
})
export class ProAttributesComponent implements OnInit {
    readonly PERMISSIONS = {
        CREATE: 'Product.ProAttribute.Create',
        UPDATE: 'Product.ProAttribute.Update',
        DELETE: 'Product.ProAttribute.Delete',
        VIEW: 'Product.ProAttribute'
    };

    // Data
    items: ProAttributeDto[] = [];
    totalCount = 0;
    loading = true;
    pageSize = 10;

    // Dialog
    dialogVisible = false;
    dialogMode: 'create' | 'edit' = 'create';
    selectedItem?: ProAttributeDto;

    // Form Data
    @ViewChild('form') form?: NgForm;
    formData: Partial<CreateProAttributeDto> = {};

    // Search
    searchForm: ProAttributeSearchForm = {
        code: null,
        name: null,
        status: null
    };

    // Options
    statusOptions = [
        { label: 'Active', value: ProAttributeStatus.Active },
        { label: 'Inactive', value: ProAttributeStatus.Deactive }
    ];
    specOptions: any[] = proAttributeSpecOptions;
    dataTypeOptions = [{ label: 'String', value: ProAttributeDataType.String }, { label: 'Int', value: ProAttributeDataType.Int }, { label: 'Date', value: ProAttributeDataType.Date }, { label: 'Boolean', value: ProAttributeDataType.Boolean }];

    currentLazyLoadEvent?: TableLazyLoadEvent;
    columns: TableColumn[] = [];
    actions: TableAction<ProAttributeDto>[] = [];

    constructor(
        private service: ProAttributeService,
        private messageService: MessageService,
        private confirmationService: ConfirmationService,
        private permissionService: PermissionService,
        private localizationService: LocalizationService
    ) {
        this.initializeColumns();
        this.initializeActions();
        this.statusOptions = [
            { label: this.localizationService.localize('Product::ProAttribute:Active'), value: ProAttributeStatus.Active },
            { label: this.localizationService.localize('Product::ProAttribute:Deactive'), value: ProAttributeStatus.Deactive }
        ];
    }

    ngOnInit(): void {
        this.specOptions = [
            { key: 'Đối tượng bảo hiểm (RiskObject)', value: ProAttributeSpec.RiskObject },
            { key: 'Khách hàng (Customer)', value: ProAttributeSpec.Customer },
            { key: 'Đơn bảo hiểm (Policy)', value: ProAttributeSpec.Policy },
            { key: 'Phạm vi bảo hiểm (Coverage)', value: ProAttributeSpec.Coverage }
        ];
    }

    private initializeColumns(): void {
        this.columns = [
            {
                field: 'code',
                header: this.localizationService.localize('Product::ProAttribute:Code'),
                sortable: true,
                width: '150px'
            },
            {
                field: 'name',
                header: this.localizationService.localize('Product::ProAttribute:Name'),
                sortable: true,
                width: '250px'
            },
            {
                field: 'dataType',
                header: this.localizationService.localize('Product::ProAttribute:DataType'),
                sortable: true,
                width: '150px',
                formatter: (value) => this.formatDataType(value)
            },
            {
                field: 'status',
                header: this.localizationService.localize('Product::ProAttribute:Status'),
                sortable: true,
                width: '100px',
                formatter: (value) => this.formatStatus(value),
                cellClass: (value) => this.getStatusClass(value)
            },
            {
                field: 'description',
                header: this.localizationService.localize('Product::ProAttribute:Description'),
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

    private formatDataType(dataType: ProAttributeDataType): string {
        const option = this.dataTypeOptions.find(o => o.value === dataType);
        return option ? option.label : '';
    }

    private formatStatus(status: ProAttributeStatus): string {
        const option = this.statusOptions.find(o => o.value === status);
        return option ? option.label : '';
    }

    private getStatusClass(status: ProAttributeStatus): string {
        return status === ProAttributeStatus.Active
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

        const input: GetProAttributesInput = {
            skipCount: event.first || 0,
            maxResultCount: event.rows || this.pageSize,
            sorting: this.getSortingString(event),
            code: this.searchForm.code || undefined,
            name: this.searchForm.name || undefined,
            status: this.searchForm.status !== null ? this.searchForm.status : undefined
        };

        this.service.getList(input).subscribe({
            next: (result) => {
                const items = result.items || [];
                this.items = items.map(item => ({
                    ...item,
                    dataType: this.dataTypeOptions.find(opt => opt.value === item.dataType)?.value,
                    status: this.statusOptions.find(opt => opt.value === item.status)?.value
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
            status: ProAttributeStatus.Active,
            spec: ProAttributeSpec.RiskObject,
            dataType: ProAttributeDataType.String
        };
        this.selectedItem = undefined;
        this.dialogVisible = true;
    }

    onDialogHide(): void {
        this.form?.resetForm();
    }

    openEditDialog(item: ProAttributeDto): void {
        this.dialogMode = 'edit';
        this.selectedItem = item;
        this.formData = {
            ...item
        };
        this.dialogVisible = true;
    }

    save(): void {
        // Trim values before validation
        if (this.formData.code) {
            this.formData.code = this.formData.code.trim();
        }
        if (this.formData.name) {
            this.formData.name = this.formData.name.trim();
        }
        if (this.formData.dataPath) {
            this.formData.dataPath = this.formData.dataPath.trim();
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

    private validate(): boolean {
        if (!this.formData.code) {
            this.showError('Product::ProAttribute:CodeRequired');
            return false;
        }
        if (!/^[a-zA-Z0-9_]+$/.test(this.formData.code)) {
            this.showError('Product::ProAttribute:CodeHasSpecialChar');
            return false;
        }
        if (!this.formData.name) {
            this.showError('Product::ProAttribute:NameRequired');
            return false;
        }
        if (this.formData.status === null || this.formData.status === undefined) {
            this.showError('Product::ProAttribute:StatusRequired');
            return false;
        }
        if (this.formData.spec === null || this.formData.spec === undefined) {
            this.showError('Product::ProAttribute:SpecRequired');
            return false;
        }
        if (!this.formData.dataPath) {
            this.showError('Product::ProAttribute:DataPathRequired');
            return false;
        }
        if (this.formData.dataType === null || this.formData.dataType === undefined) {
            this.showError('Product::ProAttribute:DataTypeRequired');
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
        const input = this.formData as CreateProAttributeDto;
        this.service.create(input).subscribe({
            next: () => {
                this.messageService.add({
                    severity: 'success',
                    summary: this.localizationService.localize('AbpUi::Success'),
                    detail: this.localizationService.localize('Product::ProAttribute:CreatedSuccessfully')
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
        const input = this.formData as UpdateProAttributeDto;
        this.service.update(this.selectedItem.id, input).subscribe({
            next: () => {
                this.messageService.add({
                    severity: 'success',
                    summary: this.localizationService.localize('AbpUi::Success'),
                    detail: this.localizationService.localize('Product::ProAttribute:UpdatedSuccessfully')
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

    deleteItem(item: ProAttributeDto): void {
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
                            detail: this.localizationService.localize('Product::ProAttribute:DeletedSuccessfully')
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
