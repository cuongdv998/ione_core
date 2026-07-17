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
import { DatePickerModule } from 'primeng/datepicker';

import { VTable } from '@/shared/components/v-table/v-table';
import { TableColumn } from '@/shared/models/table-column.model';
import { TableAction } from '@/shared/models/table-action.model';
import { ResCustomerService } from '@/proxy/customer/controllers/res-customer.service';
import { ResCustomerDto, CreateResCustomerDto, UpdateResCustomerDto, GetResCustomersInput } from '@/proxy/customer/res-customers/models';
import { ResCustomerStatus } from '@/proxy/res-customers/res-customer-status.enum';
import { ResCustomerSex } from '@/proxy/res-customers/res-customer-sex.enum';
import { OrganizationTypeType } from '@/proxy/res-organization-types/organization-type-type.enum';
import { ResCustomerSearchForm, ResCustomerFormData } from './res-customers.models';
import { PermissionPipe } from '@/core/pipes/permission.pipe';
import { PermissionService } from '@/core/services/permission.service';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';
import { ResIndustryService } from '@/proxy/customer/controllers/res-industry.service';
import { ResIndustryDto } from '@/proxy/customer/res-industries/models';
import { ResOrganizationTypeService } from '@/proxy/partner/controllers/res-organization-type.service';
import { ResOrganizationTypeDto } from '@/proxy/partner/res-organization-types/models';
import { ResProvinceService } from '@/proxy/master/controllers/res-province.service';
import { ResProvinceDto } from '@/proxy/master/res-provinces/models';
import { ResWardService } from '@/proxy/master/controllers/res-ward.service';
import { ResWardDto } from '@/proxy/master/res-wards/models';
import { HrEmployeeService } from '@/proxy/hr/controllers/hr-employee.service';
import { HrEmployeeDto } from '@/proxy/hr/hr-employees/models';
import { ResIndustryStatus } from '@/proxy/res-industries/res-industry-status.enum';
import { ResProvinceStatus } from '@/proxy/res-provinces/res-province-status.enum';
import { ResWardStatus } from '@/proxy/res-wards/res-ward-status.enum';
import { HrEmployeeStatus } from '@/proxy/hr-employees/hr-employee-status.enum';
import { ResOrganizationTypeStatus } from '@/proxy/res-organization-types';

@Component({
    selector: 'app-res-customers',
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
        DatePickerModule,
        VTable,
        PermissionPipe,
        TranslatePipe
    ],
    templateUrl: './res-customers.component.html',
    styleUrl: './res-customers.component.scss',
    providers: [MessageService, ConfirmationService]
})
export class ResCustomersComponent implements OnInit {
    // Permissions
    readonly PERMISSIONS = {
        CREATE: 'CustomerResCustomer.Create',
        UPDATE: 'CustomerResCustomer.Edit',
        DELETE: 'CustomerResCustomer.Delete',
        VIEW: 'CustomerResCustomer.View'
    };

    // Data
    resCustomers: ResCustomerDto[] = [];
    totalCount = 0;
    loading = true;
    pageSize = 10;

    // Dialog
    dialogVisible = false;
    dialogMode: 'create' | 'edit' = 'create';
    formData: ResCustomerFormData = this.getEmptyForm();
    selectedResCustomer?: ResCustomerDto;

    // Search form
    searchForm: ResCustomerSearchForm = {
        filter: null,
        code: null,
        name: null,
        industryId: null,
        provinceId: null,
        wardId: null,
        organizationTypeId: null,
        saleId: null,
        status: null
    };

    // Options
    statusOptions: Array<{ label: string; value: ResCustomerStatus }> = [];
    sexOptions: Array<{ label: string; value: ResCustomerSex }> = [];
    industryOptions: Array<{ label: string; value: string }> = [];
    organizationTypeOptions: Array<{ label: string; value: string }> = [];
    organizationTypeDetails: Map<string, ResOrganizationTypeDto> = new Map(); // Cache org type details
    provinceOptions: Array<{ label: string; value: string }> = [];
    wardOptions: Array<{ label: string; value: string }> = [];
    invoiceProvinceOptions: Array<{ label: string; value: string }> = [];
    invoiceWardOptions: Array<{ label: string; value: string }> = [];
    saleOptions: Array<{ label: string; value: string }> = [];

    // Table
    columns: TableColumn[] = [];
    actions: TableAction<ResCustomerDto>[] = [];
    currentLazyLoadEvent?: TableLazyLoadEvent;

    constructor(
        private resCustomerService: ResCustomerService,
        private industryService: ResIndustryService,
        private organizationTypeService: ResOrganizationTypeService,
        private provinceService: ResProvinceService,
        private wardService: ResWardService,
        private employeeService: HrEmployeeService,
        private messageService: MessageService,
        private confirmationService: ConfirmationService,
        private permissionService: PermissionService,
        private localizationService: LocalizationService
    ) {
        this.initializeColumns();
        this.initializeActions();
        this.initializeStatusOptions();
        this.initializeSexOptions();
        this.loadDropdownOptions();
    }

    ngOnInit(): void {
        // Initial load will be triggered by the table's onLazyLoad event
    }

    /**
     * Load all dropdown options
     */
    private loadDropdownOptions(): void {
        this.loadIndustries();
        this.loadOrganizationTypes();
        this.loadProvinces();
        this.loadSales();
    }

    /**
     * Load industries (only active)
     */
    private loadIndustries(): void {
        this.industryService.getList({
            skipCount: 0,
            maxResultCount: 1000,
            status: ResIndustryStatus.Active
        }).subscribe({
            next: (result) => {
                this.industryOptions = (result.items || []).map(item => ({
                    label: item.name || '',
                    value: item.id || ''
                }));
            },
            error: () => {
                // Silently fail
            }
        });
    }

    /**
     * Load organization types
     */
    private loadOrganizationTypes(): void {
        this.organizationTypeService.getList({
            skipCount: 0,
            maxResultCount: 1000
        }).subscribe({
            next: (result) => {
                this.organizationTypeOptions = (result.items || []).filter(x => x.status == ResOrganizationTypeStatus.Active).map(item => ({
                    label: item.name || '',
                    value: item.id || ''
                }));
                // Cache organization type details
                (result.items || []).forEach(item => {
                    if (item.id) {
                        this.organizationTypeDetails.set(item.id, item);
                    }
                });
            },
            error: () => {
                // Silently fail
            }
        });
    }

    /**
     * Load provinces (only active)
     */
    private loadProvinces(): void {
        this.provinceService.getList({
            skipCount: 0,
            maxResultCount: 1000,
            status: ResProvinceStatus.Active
        }).subscribe({
            next: (result) => {
                this.provinceOptions = (result.items || []).map(item => ({
                    label: item.name || '',
                    value: item.id || ''
                }));
                // Also set invoice province options (same data)
                this.invoiceProvinceOptions = [...this.provinceOptions];
            },
            error: () => {
                // Silently fail
            }
        });
    }

    /**
     * Load wards (will be filtered by selected province)
     */
    loadWards(provinceId?: string | null): void {
        if (!provinceId) {
            this.wardOptions = [];
            return;
        }

        this.wardService.getList({
            skipCount: 0,
            maxResultCount: 1000,
            provinceId: provinceId,
            status: ResWardStatus.Active
        }).subscribe({
            next: (result) => {
                this.wardOptions = (result.items || []).map(item => ({
                    label: item.name || '',
                    value: item.id || ''
                }));
            },
            error: () => {
                this.wardOptions = [];
            }
        });
    }

    /**
     * Load invoice wards (will be filtered by selected invoice province)
     */
    loadInvoiceWards(provinceId?: string | null): void {
        if (!provinceId) {
            this.invoiceWardOptions = [];
            return;
        }

        this.wardService.getList({
            skipCount: 0,
            maxResultCount: 1000,
            provinceId: provinceId,
            status: ResWardStatus.Active
        }).subscribe({
            next: (result) => {
                this.invoiceWardOptions = (result.items || []).map(item => ({
                    label: item.name || '',
                    value: item.id || ''
                }));
            },
            error: () => {
                this.invoiceWardOptions = [];
            }
        });
    }

    /**
     * Load sales (only active employees)
     */
    private loadSales(): void {
        this.employeeService.getList({
            skipCount: 0,
            maxResultCount: 1000,
            status: HrEmployeeStatus.Active
        }).subscribe({
            next: (result) => {
                this.saleOptions = (result.items || []).map(item => ({
                    label: `${item.code || ''} - ${item.fullName || ''}`,
                    value: item.id || ''
                }));
            },
            error: () => {
                // Silently fail
            }
        });
    }

    /**
     * Initialize status options with localized labels
     */
    private initializeStatusOptions(): void {
        this.statusOptions = [
            { label: this.localizationService.localize('Customer::ResCustomer:Active'), value: ResCustomerStatus.Active },
            { label: this.localizationService.localize('Customer::ResCustomer:Deactive'), value: ResCustomerStatus.Deactive }
        ];
    }

    /**
     * Initialize sex options with localized labels
     */
    private initializeSexOptions(): void {
        this.sexOptions = [
            { label: this.localizationService.localize('Customer::ResCustomer:Male'), value: ResCustomerSex.Male },
            { label: this.localizationService.localize('Customer::ResCustomer:Female'), value: ResCustomerSex.Female }
        ];
    }

    /**
     * Initialize table columns with localized headers
     */
    private initializeColumns(): void {
        this.columns = [
            {
                field: 'code',
                header: this.localizationService.localize('Customer::ResCustomer:Code'),
                sortable: true,
                width: '150px'
            },
            {
                field: 'name',
                header: this.localizationService.localize('Customer::ResCustomer:Name'),
                sortable: true,
                width: '250px'
            },
            {
                field: 'industryName',
                header: this.localizationService.localize('Customer::ResCustomer:Industry'),
                sortable: false,
                width: '200px'
            },
            {
                field: 'phone',
                header: this.localizationService.localize('Customer::ResCustomer:Phone'),
                sortable: true,
                width: '150px'
            },
            {
                field: 'email',
                header: this.localizationService.localize('Customer::ResCustomer:Email'),
                sortable: true,
                width: '200px'
            },
            {
                field: 'fullAddress',
                header: this.localizationService.localize('Customer::ResCustomer:FullAddress'),
                sortable: false,
                width: '300px'
            },
            {
                field: 'organizationTypeName',
                header: this.localizationService.localize('Customer::ResCustomer:OrganizationType'),
                sortable: false,
                width: '150px'
            },
            {
                field: 'creationTime',
                header: this.localizationService.localize('AbpIdentity::CreationTime'),
                sortable: true,
                type: 'date',
                width: '180px'
            },
            {
                field: 'status',
                header: this.localizationService.localize('Customer::ResCustomer:Status'),
                sortable: true,
                type: 'text',
                width: '120px',
                freeze: 'right',
                align: 'center',
                formatter: (value: any) => this.formatStatus(value),
                cellClass: (value: any) => {
                    const statusValue = typeof value === 'number' ? value :
                        (value === ResCustomerStatus.Active ? 0 : 1);
                    return statusValue === 0
                        ? 'px-2 py-1 rounded text-xs font-semibold bg-green-100 text-green-800'
                        : 'px-2 py-1 rounded text-xs font-semibold bg-red-100 text-red-800';
                }
            }
        ];
    }

    /**
     * Initialize actions based on permissions
     */
    private initializeActions(): void {
        if (this.permissionService.isGranted(this.PERMISSIONS.UPDATE)) {
            this.actions.push({
                label: this.localizationService.localize('Customer::Edit'),
                icon: 'pi pi-pencil',
                command: (row) => this.openEditDialog(row)
            });
        }

        if (this.permissionService.isGranted(this.PERMISSIONS.DELETE)) {
            this.actions.push({
                label: this.localizationService.localize('Customer::Delete'),
                icon: 'pi pi-trash',
                command: (row) => this.delete(row)
            });
        }
    }

    /**
     * Load customers with lazy loading
     */
    loadData(event: TableLazyLoadEvent): void {
        this.currentLazyLoadEvent = event;
        this.loading = true;

        const input: GetResCustomersInput = {
            skipCount: event.first || 0,
            maxResultCount: event.rows || 10,
            sorting: this.getSortingString(event),
            filter: this.searchForm.filter || undefined,
            code: this.searchForm.code || undefined,
            name: this.searchForm.name || undefined,
            industryId: this.searchForm.industryId || undefined,
            provinceId: this.searchForm.provinceId || undefined,
            wardId: this.searchForm.wardId || undefined,
            organizationTypeId: this.searchForm.organizationTypeId || undefined,
            saleId: this.searchForm.saleId || undefined,
            status: this.searchForm.status ?? undefined
        };

        this.resCustomerService.getList(input).subscribe({
            next: (result) => {
                this.resCustomers = result.items || [];
                this.totalCount = result.totalCount || 0;
                this.loading = false;
            },
            error: (error) => {
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('Customer::Error'),
                    detail: this.localizationService.localize('Customer::InternalServerErrorMessage')
                });
                this.loading = false;
            }
        });
    }

    /**
     * Listen for Enter key globally to trigger search when no dialog is open
     */
    @HostListener('window:keydown.enter', ['$event'])
    onWindowKeyDown(event: KeyboardEvent): void {
        const target = event.target as HTMLElement;
        const isInput = target.tagName === 'INPUT' || target.tagName === 'TEXTAREA' || target.isContentEditable;

        // Trigger search ONLY if:
        // 1. No dialog is open
        // 2. We are NOT inside a Create/Edit dialog (if any other part of the app uses it)
        // 3. Optional: if target is within search panel or simple body
        if (!this.dialogVisible) {
            this.search();
        }
    }

    /**
     * Get sorting string from lazy load event
     */
    private getSortingString(event: TableLazyLoadEvent): string | undefined {
        if (!event.sortField) {
            return undefined;
        }

        const sortOrder = event.sortOrder === 1 ? 'asc' : 'desc';
        return `${event.sortField} ${sortOrder}`;
    }

    /**
     * Execute search with current filters
     */
    search(): void {
        if (this.currentLazyLoadEvent) {
            this.currentLazyLoadEvent.first = 0;
            this.loadData(this.currentLazyLoadEvent);
        }
    }

    /**
     * Reset search form and reload data
     */
    resetSearch(): void {
        this.searchForm = {
            filter: null,
            code: null,
            name: null,
            industryId: null,
            provinceId: null,
            wardId: null,
            organizationTypeId: null,
            saleId: null,
            status: null
        };
        this.search();
    }

    /**
     * Open create dialog
     */
    openCreateDialog(): void {
        this.dialogMode = 'create';
        this.formData = this.getEmptyForm();
        this.selectedResCustomer = undefined;
        this.dialogVisible = true;
    }

    /**
     * Open edit dialog
     * ⚠️ QUAN TRỌNG: Code không được phép sửa khi edit
     */
    openEditDialog(customer: ResCustomerDto): void {
        this.dialogMode = 'edit';
        this.selectedResCustomer = customer;

        // Load wards for selected province
        if (customer.provinceId) {
            this.loadWards(customer.provinceId);
        }

        // Load invoice wards for selected invoice province
        if (customer.invoiceProvinceId) {
            this.loadInvoiceWards(customer.invoiceProvinceId);
        }

        // Load organization type details if needed
        if (customer.organizationTypeId && !this.organizationTypeDetails.has(customer.organizationTypeId)) {
            this.organizationTypeService.get(customer.organizationTypeId).subscribe({
                next: (orgType) => {
                    if (orgType.id) {
                        this.organizationTypeDetails.set(orgType.id, orgType);
                    }
                },
                error: () => {
                    // Silently fail
                }
            });
        }

        // Parse date strings to Date objects
        let dob: Date | null = null;
        if (customer.dob) {
            dob = new Date(customer.dob);
        }

        let authorizerDate: Date | null = null;
        if (customer.authorizerDate) {
            authorizerDate = new Date(customer.authorizerDate);
        }

        this.formData = {
            code: customer.code || '', // Readonly trong edit mode
            name: customer.name || '',
            industryId: customer.industryId || null,
            provinceId: customer.provinceId || '',
            wardId: customer.wardId || '',
            address: customer.address || '',
            fullAddress: customer.fullAddress || '', // Computed, readonly
            email: customer.email || null,
            phone: customer.phone || '',
            note: customer.note || null,
            status: customer.status ?? ResCustomerStatus.Active,
            tin: customer.tin || null,
            idNo: customer.idNo || null,
            passportNo: customer.passportNo || null,
            dob: dob,
            sex: customer.sex ?? null,
            repName: customer.repName || null,
            repEmail: customer.repEmail || null,
            repPhone: customer.repPhone || null,
            repIdNo: customer.repIdNo || null,
            repTitle: customer.repTitle || null,
            authorizer: customer.authorizer || null,
            authorizerPhone: customer.authorizerPhone || null,
            authorizerEmail: customer.authorizerEmail || null,
            authorizerNo: customer.authorizerNo || null,
            authorizerDate: authorizerDate,
            authorizerTitle: customer.authorizerTitle || null,
            businessNo: customer.businessNo || null,
            organizationTypeId: customer.organizationTypeId || null,
            invoiceProvinceId: customer.invoiceProvinceId || null,
            invoiceWardId: customer.invoiceWardId || null,
            invoiceAddress: customer.invoiceAddress || null,
            invoiceFullAddress: customer.invoiceFullAddress || null, // Computed, readonly
            saleId: customer.saleId || null
        };
        this.dialogVisible = true;
    }

    /**
     * Save (create or update)
     */
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

    /**
     * Validate form
     */
    validateForm(): boolean {
        // Code is auto-generated by backend, no validation needed here


        if (!this.formData.name || this.formData.name.trim() === '') {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Customer::ResCustomer:NameRequired')
            });
            return false;
        }

        if (this.formData.name.length > 250) {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Customer::ResCustomer:NameMaxLength')
            });
            return false;
        }

        if (!this.formData.address || this.formData.address.trim() === '') {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Customer::ResCustomer:AddressRequired')
            });
            return false;
        }

        if (this.formData.address.length > 250) {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Customer::ResCustomer:AddressMaxLength')
            });
            return false;
        }

        if (!this.formData.phone || this.formData.phone.trim() === '') {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Customer::ResCustomer:PhoneRequired')
            });
            return false;
        }

        if (this.formData.phone && !this.validatePhone(this.formData.phone)) {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Customer::ResCustomer:PhoneInvalid')
            });
            return false;
        }

        // Validate TC specific required fields
        if (this.isOrganizationTypeTC()) {
            if (!this.formData.tin || this.formData.tin.trim() === '') {
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: this.localizationService.localize('Customer::ResCustomer:TINRequired')
                });
                return false;
            }

            if (!this.formData.email || this.formData.email.trim() === '') {
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: this.localizationService.localize('Customer::ResCustomer:EmailRequired')
                });
                return false;
            }

            if (!this.formData.repPhone || this.formData.repPhone.trim() === '') {
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: this.localizationService.localize('Customer::ResCustomer:RepPhoneRequired')
                });
                return false;
            }

            if (!this.formData.repEmail || this.formData.repEmail.trim() === '') {
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: this.localizationService.localize('Customer::ResCustomer:RepEmailRequired')
                });
                return false;
            }
        }

        if (this.formData.email && this.formData.email.length > 250) {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Customer::ResCustomer:EmailMaxLength')
            });
            return false;
        }

        if (this.formData.email && !this.isValidEmailFormat(this.formData.email)) {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Customer::ResCustomer:EmailInvalid')
            });
            return false;
        }

        if (this.formData.repEmail && !this.isValidEmailFormat(this.formData.repEmail)) {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Customer::ResCustomer:RepEmail') + ': ' + this.localizationService.localize('Customer::ResCustomer:EmailInvalid')
            });
            return false;
        }

        if (this.formData.authorizerEmail && !this.isValidEmailFormat(this.formData.authorizerEmail)) {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Customer::ResCustomer:AuthorizerEmail') + ': ' + this.localizationService.localize('Customer::ResCustomer:EmailInvalid')
            });
            return false;
        }

        if (this.formData.note && this.formData.note.length > 500) {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Customer::ResCustomer:NoteMaxLength')
            });
            return false;
        }

        // Validate TIN (MST)
        if (this.formData.tin && !this.validateMST(this.formData.tin)) {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Customer::ResCustomer:TinInvalid')
            });
            return false;
        }

        // Validate ID No (CCCD/CMND)
        if (this.formData.idNo && !this.validateIdNo(this.formData.idNo)) {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Customer::ResCustomer:IdNoInvalid')
            });
            return false;
        }

        // Validate Passport No
        if (this.formData.passportNo && !this.validatePassport(this.formData.passportNo)) {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Customer::ResCustomer:PassportNoInvalid')
            });
            return false;
        }

        // Validate Rep Phone
        if (this.formData.repPhone && !this.validatePhone(this.formData.repPhone)) {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Customer::ResCustomer:RepPhoneInvalid')
            });
            return false;
        }

        // Validate Authorizer Phone
        if (this.formData.authorizerPhone && !this.validatePhone(this.formData.authorizerPhone)) {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Customer::ResCustomer:AuthorizerPhoneInvalid')
            });
            return false;
        }

        return true;
    }

    /** Email format validation - same rule as backend [EmailAddress]: when value present must be valid email, max 50 chars. */
    private isValidEmailFormat(value: string | null | undefined): boolean {
        const s = value == null ? '' : String(value).trim();
        if (s === '') return true;
        if (s.length > 50) return false;
        return /^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(s);
    }

    /**
     * Validate code format (A-Z, 0-9, _)
     */
    validateCode(code: string): boolean {
        return /^[A-Z0-9_]+$/.test(code);
    }

    /**
     * Convert code to uppercase on input
     */


    /**
     * Convert passport to uppercase on input
     */
    onPassportInput(event: Event): void {
        const input = event.target as HTMLInputElement;
        const value = input.value.toUpperCase();
        input.value = value;
        this.formData.passportNo = value;
    }

    /**
     * Validate MST
     * Rule: 10<=len<=14, only numbers or -, after - must be 3 digits
     */
    validateMST(tin: string): boolean {
        if (!tin) return true; // Optional
        if (tin.length < 10 || tin.length > 14) return false;
        const regex = /^[0-9]+(-[0-9]{3})?$/;
        return regex.test(tin);
    }

    /**
     * Validate Phone
     * Rule: Only numbers, 10 digits, start with 0
     */
    validatePhone(phone: string): boolean {
        if (!phone) return true; // Checked separately if required
        // Remove spaces if necessary? Request says "Không khoảng trắng".
        // If user enters spaces, validation fails.
        const regex = /^0[0-9]{9}$/;
        return regex.test(phone);
    }

    /**
     * Validate ID No (CCCD/CMND)
     * Rule: Length 9 or 12
     */
    validateIdNo(idNo: string): boolean {
        if (!idNo) return true;
        // Typically numeric
        if (idNo.length !== 9 && idNo.length !== 12) return false;
        return /^[0-9]+$/.test(idNo);
    }

    /**
     * Validate Passport
     * Rule: Chars+Num, No special, Len 6-9, Uppercase
     */
    validatePassport(passport: string): boolean {
        if (!passport) return true;
        const regex = /^[A-Z0-9]{6,9}$/;
        return regex.test(passport.toUpperCase());
    }

    /**
     * Create new customer
     */
    private create(): void {
        this.loading = true;

        console.log("create", this.formData);

        const createDto: CreateResCustomerDto = {
            code: this.formData.code.trim(),
            name: this.formData.name.trim(),
            industryId: this.formData.industryId || undefined,
            provinceId: this.formData.provinceId || '',
            wardId: this.formData.wardId || '',
            address: this.formData.address.trim(),
            email: this.formData.email?.trim() || undefined,
            phone: this.formData.phone.trim(),
            note: this.formData.note?.trim() || undefined,
            status: this.formData.status,
            tin: this.formData.tin?.trim() || undefined,
            idNo: this.formData.idNo?.trim() || undefined,
            passportNo: this.formData.passportNo?.trim() || undefined,
            dob: this.formData.dob ? this.formatDateForApi(this.formData.dob) : undefined,
            sex: this.formData.sex ?? undefined,
            repName: this.formData.repName?.trim() || undefined,
            repEmail: this.formData.repEmail?.trim() || undefined,
            repPhone: this.formData.repPhone?.trim() || undefined,
            repIdNo: this.formData.repIdNo?.trim() || undefined,
            repTitle: this.formData.repTitle?.trim() || undefined,
            authorizer: this.formData.authorizer?.trim() || undefined,
            authorizerPhone: this.formData.authorizerPhone?.trim() || undefined,
            authorizerEmail: this.formData.authorizerEmail?.trim() || undefined,
            authorizerNo: this.formData.authorizerNo?.trim() || undefined,
            authorizerDate: this.formData.authorizerDate ? this.formatDateForApi(this.formData.authorizerDate) : undefined,
            authorizerTitle: this.formData.authorizerTitle?.trim() || undefined,
            businessNo: this.formData.businessNo?.trim() || undefined,
            organizationTypeId: this.formData.organizationTypeId || undefined,
            invoiceProvinceId: this.formData.invoiceProvinceId || undefined,
            invoiceWardId: this.formData.invoiceWardId || undefined,
            invoiceAddress: this.formData.invoiceAddress?.trim() || undefined,
            saleId: this.formData.saleId || undefined
        };

        this.resCustomerService.create(createDto).subscribe({
            next: () => {
                this.messageService.add({
                    severity: 'success',
                    summary: this.localizationService.localize('Customer::Success'),
                    detail: this.localizationService.localize('Customer::ResCustomer:CreatedSuccessfully')
                });
                this.dialogVisible = false;
                this.search();
            },
            error: (error) => {
                this.handleError(error);
            }
        });
    }

    /**
     * Update existing customer
     * ⚠️ QUAN TRỌNG: Chỉ update các fields có thể sửa, không update Code
     */
    private update(): void {
        if (!this.selectedResCustomer || !this.selectedResCustomer.id) {
            return;
        }

        this.loading = true;

        console.log("update", this.formData);

        console.log(this.formData.invoiceProvinceId)
        console.log(this.formData.invoiceWardId)
        console.log(this.formData.invoiceAddress)

        const updateDto: UpdateResCustomerDto = {
            name: this.formData.name.trim(),
            industryId: this.formData.industryId || undefined,
            provinceId: this.formData.provinceId || '',
            wardId: this.formData.wardId || '',
            address: this.formData.address.trim(),
            email: this.formData.email?.trim() || undefined,
            phone: this.formData.phone.trim(),
            note: this.formData.note?.trim() || undefined,
            status: this.formData.status,
            tin: this.formData.tin?.trim() || undefined,
            idNo: this.formData.idNo?.trim() || undefined,
            passportNo: this.formData.passportNo?.trim() || undefined,
            dob: this.formData.dob ? this.formatDateForApi(this.formData.dob) : undefined,
            sex: this.formData.sex ?? undefined,
            repName: this.formData.repName?.trim() || undefined,
            repEmail: this.formData.repEmail?.trim() || undefined,
            repPhone: this.formData.repPhone?.trim() || undefined,
            repIdNo: this.formData.repIdNo?.trim() || undefined,
            repTitle: this.formData.repTitle?.trim() || undefined,
            authorizer: this.formData.authorizer?.trim() || undefined,
            authorizerPhone: this.formData.authorizerPhone?.trim() || undefined,
            authorizerEmail: this.formData.authorizerEmail?.trim() || undefined,
            authorizerNo: this.formData.authorizerNo?.trim() || undefined,
            authorizerDate: this.formData.authorizerDate ? this.formatDateForApi(this.formData.authorizerDate) : undefined,
            authorizerTitle: this.formData.authorizerTitle?.trim() || undefined,
            businessNo: this.formData.businessNo?.trim() || undefined,
            organizationTypeId: this.formData.organizationTypeId || undefined,
            invoiceProvinceId: this.formData.invoiceProvinceId || undefined,
            invoiceWardId: this.formData.invoiceWardId || undefined,
            invoiceAddress: this.formData.invoiceAddress?.trim() || undefined,
            saleId: this.formData.saleId || undefined
        };

        console.log("update", updateDto);

        this.resCustomerService.update(this.selectedResCustomer.id, updateDto).subscribe({
            next: () => {
                this.messageService.add({
                    severity: 'success',
                    summary: this.localizationService.localize('Customer::Success'),
                    detail: this.localizationService.localize('Customer::ResCustomer:UpdatedSuccessfully')
                });
                this.dialogVisible = false;
                this.search();
            },
            error: (error) => {
                this.handleError(error);
            }
        });
    }

    /**
     * Delete customer with confirmation
     * ⚠️ QUAN TRỌNG: Soft delete - set Status to Deactive
     */
    delete(customer: ResCustomerDto): void {
        if (!customer.id) {
            return;
        }

        this.confirmationService.confirm({
            message: this.localizationService.localize('Customer::ResCustomer:DeleteConfirm'),
            header: this.localizationService.localize('Customer::ResCustomer:Delete'),
            icon: 'pi pi-exclamation-triangle',
            acceptButtonStyleClass: 'p-button-danger',
            accept: () => {
                this.loading = true;
                this.resCustomerService.delete(customer.id!).subscribe({
                    next: () => {
                        this.messageService.add({
                            severity: 'success',
                            summary: this.localizationService.localize('Customer::Success'),
                            detail: this.localizationService.localize('Customer::ResCustomer:DeletedSuccessfully')
                        });
                        this.search();
                    },
                    error: (error) => {
                        const errorMessage = error.error?.error?.message ||
                            error.error?.error?.details ||
                            this.localizationService.localize('AbpUi::InternalServerErrorMessage');
                        this.messageService.add({
                            severity: 'error',
                            summary: this.localizationService.localize('Customer::Error'),
                            detail: errorMessage
                        });
                        this.loading = false;
                    }
                });
            }
        });
    }

    /**
     * Get empty form data
     */
    private getEmptyForm(): ResCustomerFormData {
        return {
            code: '',
            name: '',
            industryId: null,
            provinceId: null,
            wardId: null,
            address: '',
            fullAddress: '', // Computed, readonly
            email: null,
            phone: '',
            note: null,
            status: ResCustomerStatus.Active,
            tin: null,
            idNo: null,
            passportNo: null,
            dob: null,
            sex: null,
            repName: null,
            repEmail: null,
            repPhone: null,
            repIdNo: null,
            repTitle: null,
            authorizer: null,
            authorizerPhone: null,
            authorizerEmail: null,
            authorizerNo: null,
            authorizerDate: null,
            authorizerTitle: null,
            businessNo: null,
            organizationTypeId: null,
            invoiceProvinceId: null,
            invoiceWardId: null,
            invoiceAddress: null,
            invoiceFullAddress: null, // Computed, readonly
            saleId: null
        };
    }

    /**
     * Format status for display
     */
    formatStatus(status: ResCustomerStatus | number | string | undefined | null): string {
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
            statusValue = status === ResCustomerStatus.Active ? 0 : 1;
        }

        return statusValue === 0
            ? this.localizationService.localize('Customer::ResCustomer:Active')
            : this.localizationService.localize('Customer::ResCustomer:Deactive');
    }

    /**
     * Handle province change - reload wards
     */
    onProvinceChange(): void {
        this.formData.wardId = null;
        this.loadWards(this.formData.provinceId);
        this.updateFullAddress();
    }

    /**
     * Handle ward change - update full address
     */
    onWardChange(): void {
        this.updateFullAddress();
    }

    /**
     * Handle address change - update full address
     */
    onAddressChange(): void {
        this.updateFullAddress();
    }

    /**
     * Update full address (computed from address + ward + province)
     */
    private updateFullAddress(): void {
        const addr = (this.formData.address || '').trim();
        if (!addr) {
            this.formData.fullAddress = '';
            return;
        }

        const parts: string[] = [addr];
        if (this.formData.wardId) {
            const ward = this.wardOptions.find(w => w.value === this.formData.wardId);
            if (ward) {
                parts.push(ward.label);
            }
        }
        if (this.formData.provinceId) {
            const province = this.provinceOptions.find(p => p.value === this.formData.provinceId);
            if (province) {
                parts.push(province.label);
            }
        }
        this.formData.fullAddress = parts.join(', ');
    }

    /**
     * Handle invoice province change
     */
    onInvoiceProvinceChange(): void {
        this.formData.invoiceWardId = null;
        this.loadInvoiceWards(this.formData.invoiceProvinceId);
        this.updateInvoiceFullAddress();
    }

    /**
     * Handle invoice ward change
     */
    onInvoiceWardChange(): void {
        this.updateInvoiceFullAddress();
    }

    /**
     * Handle invoice address change
     */
    onInvoiceAddressChange(): void {
        this.updateInvoiceFullAddress();
    }

    /**
     * Update invoice full address
     */
    private updateInvoiceFullAddress(): void {
        if (!this.formData.invoiceAddress || !this.formData.invoiceProvinceId) {
            this.formData.invoiceFullAddress = null;
            return;
        }

        const invoiceProvince = this.invoiceProvinceOptions.find(p => p.value === this.formData.invoiceProvinceId);
        if (!invoiceProvince) {
            this.formData.invoiceFullAddress = this.formData.invoiceAddress;
            return;
        }

        if (this.formData.invoiceWardId) {
            const invoiceWard = this.invoiceWardOptions.find(w => w.value === this.formData.invoiceWardId);
            if (invoiceWard) {
                this.formData.invoiceFullAddress = `${this.formData.invoiceAddress}, ${invoiceWard.label}, ${invoiceProvince.label}`;
            } else {
                this.formData.invoiceFullAddress = `${this.formData.invoiceAddress}, ${invoiceProvince.label}`;
            }
        } else {
            this.formData.invoiceFullAddress = `${this.formData.invoiceAddress}, ${invoiceProvince.label}`;
        }
    }

    /**
     * Load organization type details when organization type changes
     */
    onOrganizationTypeChange(): void {
        if (this.formData.organizationTypeId) {
            // Load organization type details if not cached
            if (!this.organizationTypeDetails.has(this.formData.organizationTypeId)) {
                this.organizationTypeService.get(this.formData.organizationTypeId).subscribe({
                    next: (orgType) => {
                        if (orgType.id) {
                            this.organizationTypeDetails.set(orgType.id, orgType);
                        }
                        // Clear conditional fields based on type
                        this.clearConditionalFields();
                    },
                    error: () => {
                        // Silently fail
                        this.clearConditionalFields();
                    }
                });
            } else {
                this.clearConditionalFields();
            }
        } else {
            this.clearConditionalFields();
        }
    }

    /**
     * Check if organization type is CN (Cá nhân)
     */
    isOrganizationTypeCN(): boolean {
        if (!this.formData.organizationTypeId) {
            return false;
        }

        const orgType = this.organizationTypeDetails.get(this.formData.organizationTypeId);
        if (!orgType) {
            return false;
        }

        return orgType.type === OrganizationTypeType.CN;
    }

    /**
     * Check if organization type is TC (Tổ chức)
     */
    isOrganizationTypeTC(): boolean {
        if (!this.formData.organizationTypeId) {
            return false;
        }

        const orgType = this.organizationTypeDetails.get(this.formData.organizationTypeId);
        if (!orgType) {
            return false;
        }

        return orgType.type === OrganizationTypeType.TC;
    }

    /**
     * Clear conditional fields based on organization type
     */
    private clearConditionalFields(): void {
        if (this.isOrganizationTypeCN()) {
            // Clear TC fields
            this.formData.tin = null;
            this.formData.repName = null;
            this.formData.repEmail = null;
            this.formData.repPhone = null;
            this.formData.repIdNo = null;
            this.formData.repTitle = null;
            this.formData.authorizer = null;
            this.formData.authorizerPhone = null;
            this.formData.authorizerEmail = null;
            this.formData.authorizerNo = null;
            this.formData.authorizerDate = null;
            this.formData.authorizerTitle = null;
            this.formData.businessNo = null;
        } else if (this.isOrganizationTypeTC()) {
            // Clear CN fields
            this.formData.idNo = null;
        }
    }

    /**
     * Format date for API (YYYY-MM-DD)
     */
    private formatDateForApi(date: Date): string {
        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const day = String(date.getDate()).padStart(2, '0');
        return `${year}-${month}-${day}`;
    }
    /**
     * Handle API error
     */
    private handleError(error: any): void {
        const errorResponse = error.error?.error;

        // Check if error is about Email / RepEmail / AuthorizerEmail validation (same [EmailAddress] on BE)
        const emailFieldNames = ['email', 'repemail', 'authorizeremail'];
        const emailValidationError = errorResponse?.validationErrors?.find((e: any) =>
            e.members?.some((m: string) => emailFieldNames.includes(m.toLowerCase()))
        );
        if (emailValidationError) {
            const fieldKey = emailValidationError.members?.find((m: string) => emailFieldNames.includes(m.toLowerCase()));
            const fieldLabel = fieldKey === 'repemail'
                ? this.localizationService.localize('Customer::ResCustomer:RepEmail')
                : fieldKey === 'authorizeremail'
                    ? this.localizationService.localize('Customer::ResCustomer:AuthorizerEmail')
                    : null;
            const detail = fieldLabel
                ? fieldLabel + ': ' + this.localizationService.localize('Customer::ResCustomer:EmailInvalid')
                : this.localizationService.localize('Customer::ResCustomer:EmailInvalid');
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('Customer::Error'),
                detail,
                life: 7000
            });
            this.loading = false;
            return;
        }

        let errorMessage = errorResponse?.message || '';

        if (errorResponse?.details) {
            errorMessage += (errorMessage ? '\n' : '') + errorResponse.details;
        } else if (errorResponse?.validationErrors?.length) {
            errorMessage += (errorMessage ? '\n' : '') + errorResponse.validationErrors.map((e: any) => e.message).join('\n');
        }

        if (!errorMessage) {
            errorMessage = this.localizationService.localize('AbpUi::InternalServerErrorMessage');
        }

        this.messageService.add({
            severity: 'error',
            summary: this.localizationService.localize('Customer::Error'),
            detail: errorMessage,
            life: 7000
        });
        this.loading = false;
    }
}
