import { Component, HostListener, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { TextareaModule } from 'primeng/textarea';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { DialogModule } from 'primeng/dialog';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TabsModule } from 'primeng/tabs';
import { SplitterModule } from 'primeng/splitter';
import { DatePickerModule } from 'primeng/datepicker';
import { InputNumberModule } from 'primeng/inputnumber';
import { CheckboxModule } from 'primeng/checkbox';
import { TableModule } from 'primeng/table';
import { TreeTableModule } from 'primeng/treetable';
import { MenuModule } from 'primeng/menu';
import { FileUploadModule } from 'primeng/fileupload';
import { TreeNode, MenuItem, Confirmation } from 'primeng/api';

import { VProductTreeComponent, ProductTreeNode } from '@/shared/components/v-product-tree/v-product-tree.component';
import { ProProductService } from '@/proxy/product/pro-products/pro-product.service';
import { ProProductCategoryService } from '@/proxy/product/controllers/pro-product-category.service';
import { ProLineOfBusinessService } from '@/proxy/product/controllers/pro-line-of-business.service';
import { ProProductTypeService } from '@/proxy/product/controllers/pro-product-type.service';
import { ProAttributeService } from '@/proxy/product/controllers/pro-attribute.service';
import { ProCoverageService } from '@/proxy/product/controllers/pro-coverage.service';
import { ProAttributeDto } from '@/proxy/product/pro-attributes/models';
import { ProCoverageDto } from '@/proxy/product/pro-coverages/models';
import { ProAttributeStatus } from '@/proxy/pro-attributes';
import { ProCoverageStatus } from '@/proxy/pro-coverages';
import { ProProductDto, ProProductListDto, CreateProProductDto, UpdateProProductDto, GetProProductsInput, ProProductAttributeDto, ProProductCoverageDto, ProProductCoverageInteractionDto, ProProductCoverageLevelDto, ProProductCoverageLevelTermDto, ProProductTableRateDto } from '@/proxy/product/pro-products/models';
import { ProProductCategoryDto, GetProProductCategorysInput } from '@/proxy/product/pro-product-categorys/models';
import { ProLineOfBusinessDto } from '@/proxy/product/pro-line-of-businesses/models';
import { ProProductTypeDto } from '@/proxy/product/pro-product-types/models';
import { ProProductStatus } from '@/proxy/pro-products';
import { ProProductCoverageAvailabilityType } from '@/proxy/pro-products/pro-product-coverage-availability-type.enum';
import { ProProductCoverageInteractionType, proProductCoverageInteractionTypeOptions } from '@/proxy/pro-products/pro-product-coverage-interaction-type.enum';
import { ProProductCategoryStatus } from '@/proxy/pro-product-categorys';
import { ProCoverageTermType, proCoverageTermTypeOptions } from '@/proxy/pro-coverages/pro-coverage-term-type.enum';
import { ProductSearchForm, ProductFormData } from './pro-products.models';
import { PermissionPipe } from '@/core/pipes/permission.pipe';
import { PermissionService } from '@/core/services/permission.service';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';
import { ResPartnerService } from '@/proxy/partner/controllers/res-partner.service';
import { ResPartnerTypeService } from '@/proxy/partner/controllers/res-partner-type.service';
import { GetResPartnersInput } from '@/proxy/partner/res-partners/models';
import { GetResPartnerTypesInput } from '@/proxy/partner/res-partner-types/models';
import { ResPartnerStatus } from '@/proxy/res-partners/res-partner-status.enum';
import { ResUomService } from '@/proxy/master/controllers/res-uom.service';
import { ResTaxService } from '@/proxy/product/controllers/res-tax.service';
import type { ResTaxDto } from '@/proxy/product/res-taxes/models';
import { ProCoverageGroupService } from '@/proxy/product/controllers/pro-coverage-group.service';
import { ProCoverageLevelTypeService } from '@/proxy/product/controllers/pro-coverage-level-type.service';
import { ProCoverageLevelBasisService } from '@/proxy/product/controllers/pro-coverage-level-basis.service';
import { ResUomStatus } from '@/proxy/res-uoms/res-uom-status.enum';
import { ResTaxStatus } from '@/proxy/res-taxes/res-tax-status.enum';
import { ProCoverageGroupStatus } from '@/proxy/pro-coverage-groups/pro-coverage-group-status.enum';
import { ProCoverageLevelTypeStatus } from '@/proxy/pro-coverage-level-types/pro-coverage-level-type-status.enum';
import { ProCoverageLevelBasisStatus } from '@/proxy/pro-coverage-level-bases/pro-coverage-level-basis-status.enum';
import { ProTableRateService } from '@/proxy/product/controllers/pro-table-rate.service';
import { ProTableRateDto, GetProTableRatesInput } from '@/proxy/product/pro-table-rates/models';
import { ProTableRateStatus } from '@/proxy/pro-table-rates/pro-table-rate-status.enum';
import { ProRuleService } from '@/proxy/product/controllers/pro-rule.service';
import { ProRuleTypeService } from '@/proxy/product/controllers/pro-rule-type.service';
import { ProRuleDto, CreateProRuleDto, UpdateProRuleDto, GetProRulesInput } from '@/proxy/product/pro-rules/models';
import { ProRuleStatus } from '@/proxy/pro-rules/pro-rule-status.enum';
import { ProRuleTypeDto, GetProRuleTypesInput } from '@/proxy/product/pro-rule-types/models';
import { ProRuleTypeStatus } from '@/proxy/pro-rule-types/pro-rule-type-status.enum';
import { ProProductPlanDefinitionService } from '@/proxy/product/pro-product-plan-definitions/pro-product-plan-definition.service';
import { ProProductPlanDefinitionDto, CreateProProductPlanDefinitionDto, UpdateProProductPlanDefinitionDto, GetProProductPlanDefinitionsInput } from '@/proxy/product/pro-product-plan-definitions/models';
import { ProProductPlanDefinitionStatus } from '@/proxy/pro-product-plan-definitions/pro-product-plan-definition-status.enum';
import { ResChannelService } from '@/proxy/partner/controllers/res-channel.service';
import { ResAppChannelService } from '@/proxy/master/controllers/res-app-channel.service';
import { ResCurrencyService } from '@/proxy/master/controllers/res-currency.service';
import { ResDocumentService } from '@/proxy/master/controllers/res-document.service';
import { ResDocumentTypeService } from '@/proxy/master/controllers/res-document-type.service';
import { FileResponseDto } from '@/proxy/master/res-documents/models';
import { GetResChannelsInput } from '@/proxy/partner/res-channels/models';
import { GetResAppChannelsInput } from '@/proxy/master/res-app-channels/models';
import { ResChannelStatus } from '@/proxy/res-channels/res-channel-status.enum';
import { ResAppChannelStatus } from '@/proxy/res-app-channels/res-app-channel-status.enum';
import { CreateProProductDistributionDto, ProProductDistributionDto } from '@/proxy/product/pro-product-distributions/models';
import { ProProductDistributionStatus } from '@/proxy/pro-product-distributions/pro-product-distribution-status.enum';
import { HrEmployeeRoleService } from '@/proxy/hr/controllers/hr-employee-role.service';
import { GetHrEmployeeRolesInput } from '@/proxy/hr/hr-employee-roles/models';
import { VDualListboxComponent, DualListboxOption } from '@/shared/components/v-dual-listbox';
import { forkJoin, Observable, of, EMPTY } from 'rxjs';
import { map, catchError, switchMap } from 'rxjs/operators';
import { exportToExcel, ExportColumn } from '@/shared/utils/export.util';

@Component({
    selector: 'app-pro-products',
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
        TabsModule,
        SplitterModule,
        DatePickerModule,
        InputNumberModule,
        CheckboxModule,
        TableModule,
        TreeTableModule,
        MenuModule,
        FileUploadModule,
        VProductTreeComponent,
        VDualListboxComponent,
        PermissionPipe,
        TranslatePipe
    ],
    templateUrl: './pro-products.component.html',
    styleUrl: './pro-products.component.scss',
    providers: [MessageService, ConfirmationService]
})
export class ProProductsComponent implements OnInit {
    readonly PERMISSIONS = {
        CREATE: 'Product.ProProduct.Create',
        UPDATE: 'Product.ProProduct.Update',
        DELETE: 'Product.ProProduct.Delete',
        VIEW: 'Product.ProProduct'
    };

    // Data
    products: ProProductListDto[] = [];
    categories: ProProductCategoryDto[] = [];
    loading = false;
    treeLoading = false;
    detailLoading = false;
    exporting = false;
    selectedProduct: ProProductDto | null = null;
    activeTabIndex = 0;
    productDetailTabValue: string = '0';

    // Options
    lobOptions: { label: string; value: string }[] = [];
    categoryOptions: { label: string; value: string }[] = [];
    partnerOptions: { label: string; value: string }[] = [];
    productTypeOptions: { label: string; value: string }[] = [];
    currencyOptions: { label: string; value: string; code: string }[] = [];
    parentProductOptions: { label: string; value: string }[] = [];
    planDefinitionOptions: { label: string; value: string }[] = [];
    statusOptions: { label: string; value: ProProductStatus }[] = [];
    rateTypeOptions: { label: string; value: string }[] = [];

    // Attribute tab
    attributeOptions: { label: string; value: string; code: string }[] = [];
    productAttributes: ProProductAttributeDto[] = [];
    attributeFormVisible: boolean = false;
    selectedAttributeId: string | null = null;
    selectedAttributeIsRequired: string = 'N';

    // Coverage tab
    coverageOptions: { label: string; value: string; code: string; coverageGroupId?: string; type?: ProCoverageTermType }[] = [];
    productCoverages: ProProductCoverageDto[] = [];
    coverageTreeData: TreeNode<ProProductCoverageDto>[] = [];
    coverageFormVisible: boolean = false;

    get filteredCoverageOptions(): { label: string; value: string; code: string; coverageGroupId?: string; type?: ProCoverageTermType }[] {
        if (!this.selectedCoverageGroupId) {
            return [];
        }
        return this.coverageOptions.filter(cov => 
            cov.coverageGroupId === this.selectedCoverageGroupId
        );
    }
    selectedCoverageId: string | null = null;
    selectedParentCoverageId: string | null = null;
    parentCoverageOptions: { label: string; value: string }[] = [];
    availabilityTypeOptions: { label: string; value: ProProductCoverageAvailabilityType }[] = [];
    uomOptions: { label: string; value: string }[] = [];
    taxOptions: { label: string; value: string }[] = [];
    private taxById = new Map<string, ResTaxDto>();
    coverageGroupOptions: { label: string; value: string }[] = [];
    selectedCoverageGroupId: string | null = null;
    selectedUomId: string | null = null;
    selectedTaxId: string | null = null;
    selectedAvailabilityType: ProProductCoverageAvailabilityType | null = null;
    selectedEffectDate: Date | null = null;
    selectedExpireDate: Date | null = null;
    selectedSeqNumber: number = 1;
    selectedInsurerCoverageCode: string = '';
    selectedEnableQuantity: boolean = false;
    selectedCoverageForEdit: ProProductCoverageDto | null = null;

    // Interaction modal
    interactionModalVisible: boolean = false;
    selectedCoverageForInteraction: ProProductCoverageDto | null = null;
    interactionFormVisible: boolean = false;
    interactionTypeOptions: { label: string; value: ProProductCoverageInteractionType }[] = [];
    selectedInteractionType: ProProductCoverageInteractionType | null = null;
    selectedInteractionCoverageId: string | null = null;
    selectedInteractionEffectDate: Date | null = null;
    selectedInteractionExpireDate: Date | null = null;
    selectedInteractionForEdit: ProProductCoverageInteractionDto | null = null;
    interactionCoverageOptions: { label: string; value: string }[] = [];
    private coverageMenuItemsCache = new Map<string, MenuItem[]>();

    // Level Setup Modal
    levelModalVisible: boolean = false;
    selectedCoverageForLevel: ProProductCoverageDto | null = null;
    coverageLevels: ProProductCoverageLevelDto[] = [];
    levelFormVisible: boolean = false;
    selectedLevel: ProProductCoverageLevelDto | null = null;
    levelFormData: { code?: string; name?: string; effectDate?: Date | string; expireDate?: Date | string | null; conditionalScript?: string | null } = {};
    termModalVisible: boolean = false;
    selectedLevelForTerm: ProProductCoverageLevelDto | null = null;
    selectedTerm: ProProductCoverageLevelTermDto | null = null;
    termFormData: Partial<ProProductCoverageLevelTermDto> = {};
    termIsDefaultChecked: boolean = false;
    coverageLevelTypeOptions: { label: string; value: string }[] = [];
    coverageLevelBasisOptions: { label: string; value: string }[] = [];
    amountTypeOptions: { label: string; value: string }[] = [
        { label: 'Tỷ lệ', value: 'percent' },
        { label: 'Giá trị', value: 'fix' },
        { label: 'Số lượng', value: 'quantity' }
    ];

    // Table Rate tab
    productTableRates: ProProductTableRateDto[] = [];
    tableRateOptions: { label: string; value: string; code: string }[] = [];
    tableRateFormVisible: boolean = false;
    selectedTableRateId: string | null = null; // Guid as string
    selectedTableRateEffectDate: Date | null = null;
    selectedTableRateExpireDate: Date | null = null;
    selectedTableRateForEdit: ProProductTableRateDto | null = null;
    private tableRateMenuItemsCache = new Map<string, MenuItem[]>();

    // Other Rule tab
    productRules: ProRuleDto[] = [];
    ruleTypeOptions: { label: string; value: string; code: string }[] = [];
    ruleFormVisible: boolean = false;
    selectedRuleTypeId: string | null = null;
    selectedRuleCode: string = '';
    selectedRuleName: string = '';
    selectedRulePriority: number = 1;
    selectedRuleStatus: ProRuleStatus = ProRuleStatus.Active;
    selectedRuleEffectDate: Date | null = null;
    selectedRuleExpireDate: Date | null = null;
    selectedRuleScript: string = '';
    selectedRuleDescription: string = '';
    selectedRuleForEdit: ProRuleDto | null = null;
    isRuleViewMode: boolean = false;
    ruleStatusOptions: { label: string; value: ProRuleStatus }[] = [];

    // Package Definition tab
    productPlanDefinitions: ProProductPlanDefinitionDto[] = [];
    planDefinitionFormVisible: boolean = false;
    editingPlanDefinition: ProProductPlanDefinitionDto | null = null;
    editingPlanDefinitionIndex: number = -1;
    planDefinitionStatusOptions: { label: string; value: ProProductPlanDefinitionStatus }[] = [];

    // Product Distribution (Bán hàng) tab
    allChannelOptions: DualListboxOption[] = [];
    selectedChannelIds: string[] = [];
    allAppChannelOptions: DualListboxOption[] = [];
    selectedAppChannelIds: string[] = [];
    allEmployeeRoleOptions: DualListboxOption[] = [];
    selectedEmployeeRoleIds: string[] = [];
    productDistributions: ProProductDistributionDto[] = [];

    // Image upload
    uploadedImageFile: File | null = null;
    productImageUrl: string | null = null;
    imageUploadLoading = false;

    uploadedCertificateTemplateFile: File | null = null;
    certificateTemplateDocumentUrl: string | null = null;
    certificateTemplateFileName: string | null = null;
    certificateTemplateUploadLoading = false;

    // Search form
    searchForm: ProductSearchForm = {
        lobId: null,
        productCategoryId: null,
        partnerId: null,
        code: null,
        name: null,
        status: null
    };

    treeFilterText = '';

    /** True when any search panel field is applied (same rules as getTreeList input in loadTreeData). */
    get isProductTreePanelFilterActive(): boolean {
        const f = this.searchForm;
        const code = f.code?.trim() ?? '';
        const name = f.name?.trim() ?? '';
        return !!(
            f.lobId ||
            f.productCategoryId ||
            f.partnerId ||
            code.length > 0 ||
            name.length > 0 ||
            f.status != null
        );
    }

    // Dialog
    dialogVisible = false;
    dialogMode: 'create' | 'edit' = 'create';
    formData: ProductFormData = this.getEmptyForm();
    isInCreateMode = false; // Track if user explicitly entered create mode
    isInEditMode = false; // Track if we're in edit mode (vs detail mode)

    constructor(
        private productService: ProProductService,
        private categoryService: ProProductCategoryService,
        private lobService: ProLineOfBusinessService,
        private productTypeService: ProProductTypeService,
        private attributeService: ProAttributeService,
        private coverageService: ProCoverageService,
        private messageService: MessageService,
        private confirmationService: ConfirmationService,
        private permissionService: PermissionService,
        private localizationService: LocalizationService,
        private partnerService: ResPartnerService,
        private partnerTypeService: ResPartnerTypeService,
        private uomService: ResUomService,
        private taxService: ResTaxService,
        private coverageGroupService: ProCoverageGroupService,
        private coverageLevelTypeService: ProCoverageLevelTypeService,
        private coverageLevelBasisService: ProCoverageLevelBasisService,
        private tableRateService: ProTableRateService,
        private ruleService: ProRuleService,
        private ruleTypeService: ProRuleTypeService,
        private planDefinitionService: ProProductPlanDefinitionService,
        private channelService: ResChannelService,
        private appChannelService: ResAppChannelService,
        private currencyService: ResCurrencyService,
        private resDocumentService: ResDocumentService,
        private resDocumentTypeService: ResDocumentTypeService,
        private hrEmployeeRoleService: HrEmployeeRoleService
    ) {
        this.initializeStatusOptions();
        this.initializeRateTypeOptions();
        this.initializeAvailabilityTypeOptions();
        this.initializeInteractionTypeOptions();
        this.initializeRuleStatusOptions();
        this.initializePlanDefinitionStatusOptions();
    }

    ngOnInit(): void {
        this.loadLookups();
        this.loadAttributeOptions();
        this.loadCoverageOptions();
        this.loadUomOptions();
        this.loadTaxOptions();
        this.loadCoverageGroupOptions();
        this.loadCoverageLevelTypeOptions();
        this.loadCoverageLevelBasisOptions();
        this.loadRuleTypeOptions();
        this.loadChannels();
        this.loadAppChannels();
        this.loadEmployeeRoles();
        this.loadTreeData();
    }

    /**
     * PrimeNG only copies keys present on each `confirm()` call; omitting accept/reject button classes
     * can leave stale classes from a previous dialog. Always merge defaults for labels and primary/secondary.
     */
    private confirmWithDefaults(partial: Confirmation): void {
        const defaults: Confirmation = {
            acceptLabel: this.localizationService.localize('AbpUi::Yes'),
            rejectLabel: this.localizationService.localize('AbpUi::No'),
            acceptButtonStyleClass: 'p-button-primary',
            rejectButtonStyleClass: 'p-button-secondary'
        };
        this.confirmationService.confirm({ ...defaults, ...partial });
    }

    private initializeStatusOptions(): void {
        this.statusOptions = [
            { label: this.localizationService.localize('Product::ProProduct:Active'), value: ProProductStatus.Active },
            { label: this.localizationService.localize('Product::ProProduct:Deactive'), value: ProProductStatus.Deactive },
            { label: this.localizationService.localize('Product::ProProduct:Draft'), value: ProProductStatus.Draft }
        ];
    }

    private initializeRateTypeOptions(): void {
        this.rateTypeOptions = [
            { label: this.localizationService.localize('Product::ProProduct:RateType:TableRate'), value: 'table_rate' },
            { label: this.localizationService.localize('Product::ProProduct:RateType:Api'), value: 'api' }
        ];
    }

    private initializeAvailabilityTypeOptions(): void {
        this.availabilityTypeOptions = [
            { label: this.localizationService.localize('Product::ProProduct:AvailabilityType:Required'), value: ProProductCoverageAvailabilityType.Required },
            { label: this.localizationService.localize('Product::ProProduct:AvailabilityType:Standard'), value: ProProductCoverageAvailabilityType.Standard },
            { label: this.localizationService.localize('Product::ProProduct:AvailabilityType:Optional'), value: ProProductCoverageAvailabilityType.Optional },
            { label: this.localizationService.localize('Product::ProProduct:AvailabilityType:Selectable'), value: ProProductCoverageAvailabilityType.Selectable }
        ];
    }

    private initializeInteractionTypeOptions(): void {
        this.interactionTypeOptions = [
            { label: this.localizationService.localize('Product::ProProductCoverageInteraction:Type:Dependency'), value: ProProductCoverageInteractionType.Dependency },
            { label: this.localizationService.localize('Product::ProProductCoverageInteraction:Type:Incompatible'), value: ProProductCoverageInteractionType.Incompatible },
            { label: this.localizationService.localize('Product::ProProductCoverageInteraction:Type:Exclusive'), value: ProProductCoverageInteractionType.Exclusive }
        ];
    }

    private initializeRuleStatusOptions(): void {
        this.ruleStatusOptions = [
            { label: this.localizationService.localize('Product::ProRule:Active'), value: ProRuleStatus.Active },
            { label: this.localizationService.localize('Product::ProRule:Deactive'), value: ProRuleStatus.Deactive }
        ];
    }

    private initializePlanDefinitionStatusOptions(): void {
        this.planDefinitionStatusOptions = [
            { label: this.localizationService.localize('Product::ProProduct:Active'), value: ProProductPlanDefinitionStatus.Active },
            { label: this.localizationService.localize('Product::ProProduct:Deactive'), value: ProProductPlanDefinitionStatus.Deactive }
        ];
    }

    /**
     * Serialize a Date to "YYYY-MM-DD" in LOCAL time (no UTC conversion).
     * Use this for date-only fields to avoid timezone shifts (e.g., VN UTC+7).
     */
    private toLocalIsoDate(date: Date | null | undefined): string | null {
        if (!date) return null;
        const pad2 = (n: number) => String(n).padStart(2, '0');
        const y = date.getFullYear();
        const m = pad2(date.getMonth() + 1);
        const d = pad2(date.getDate());
        return `${y}-${m}-${d}`;
    }

    /**
     * Serialize a Date to "YYYY-MM-DDTHH:mm:ss.SSS" in LOCAL time (no UTC conversion).
     * This prevents the common "-7 hours" issue caused by toISOString() in VN (UTC+7).
     */
    private toLocalIsoDateTime(date: Date | null | undefined): string | null {
        if (!date) return null;
        const pad2 = (n: number) => String(n).padStart(2, '0');
        const pad3 = (n: number) => String(n).padStart(3, '0');
        const y = date.getFullYear();
        const m = pad2(date.getMonth() + 1);
        const d = pad2(date.getDate());
        const hh = pad2(date.getHours());
        const mm = pad2(date.getMinutes());
        const ss = pad2(date.getSeconds());
        const ms = pad3(date.getMilliseconds());
        return `${y}-${m}-${d}T${hh}:${mm}:${ss}.${ms}`;
    }

    /**
     * Serialize a Date/string to local ISO string at midnight (00:00:00.000) in LOCAL time.
     * Accepts both Date instances and ISO-like strings.
     */
    private toLocalIsoDateAtMidnight(value: Date | string | null | undefined): string | null | undefined {
        if (value == null) {
            return value;
        }

        let date: Date;
        if (value instanceof Date) {
            date = new Date(value.getTime());
        } else {
            date = new Date(value);
            if (isNaN(date.getTime())) {
                return typeof value === 'string' ? value : null;
            }
        }

        date.setHours(0, 0, 0, 0);
        return this.toLocalIsoDateTime(date) ?? undefined;
    }

    /**
     * Serialize Date to local ISO string, or pass through string/null/undefined unchanged.
     * Use when building DTOs where value may be Date (from picker) or string (from server).
     */
    private toLocalIsoDateOrPass(value: Date | string | null | undefined): string | null | undefined {
        if (value == null) return value;
        if (value instanceof Date) return this.toLocalIsoDateTime(value) ?? undefined;
        return value;
    }

    loadLookups(): void {
        // Load LOBs
        this.lobService.getList({ maxResultCount: 1000 }).subscribe({
            next: (result) => {
                this.lobOptions = (result.items || []).map(item => ({
                    label: item.name || '',
                    value: item.id || ''
                }));
            }
        });

        // Load Product Types
        this.productTypeService.getList({ maxResultCount: 1000 }).subscribe({
            next: (result) => {
                this.productTypeOptions = (result.items || []).map(item => ({
                    label: item.name || '',
                    value: item.id || ''
                }));
            }
        });

        // Load Partners (INSURER type only)
        this.loadPartners();

        // Load Currencies
        this.currencyService.getList({ maxResultCount: 1000 }).subscribe({
            next: (result) => {
                this.currencyOptions = (result.items || []).map(item => ({
                    label: item.code || item.name || '',
                    value: item.id || '',
                    code: item.code || ''
                }));
                // Set default to VNĐ if available and formData doesn't have currencyId
                const vndCurrency = this.currencyOptions.find(c => 
                    (c.code || '').toUpperCase() === 'VND'
                );
                if (vndCurrency && !this.formData.currencyId) {
                    this.formData.currencyId = vndCurrency.value;
                }
            },
            error: (err) => {
                console.error('Error loading currencies:', err);
                // Set default placeholder
                this.currencyOptions = [
                    { label: 'VND', value: '', code: 'VND' } // Placeholder
                ];
            }
        });
    }

    /**
     * Load active partners with type = INSURER
     */
    private loadPartners(): void {
        // First, get the INSURER partner type
        const partnerTypeInput: GetResPartnerTypesInput = {
            code: 'INSURER',
            maxResultCount: 1000
        };

        this.partnerTypeService.getList(partnerTypeInput).subscribe({
            next: (partnerTypeResult) => {
                // Filter for exact match since backend uses "contains" search
                const insurerType = partnerTypeResult.items?.find(item => item.code === 'INSURER');

                if (!insurerType?.id) {
                    console.warn('INSURER partner type not found');
                    this.partnerOptions = [];
                    return;
                }

                // Then, get active partners with this type
                const partnerInput: GetResPartnersInput = {
                    partnerTypeId: insurerType.id,
                    status: ResPartnerStatus.Active,
                    maxResultCount: 1000,
                    sorting: 'name asc'
                };

                this.partnerService.getList(partnerInput).subscribe({
                    next: (partnerResult) => {
                        this.partnerOptions = (partnerResult.items || []).map(partner => ({
                            label: partner.name || '',
                            value: partner.id || ''
                        }));
                    },
                    error: (err) => {
                        console.error('Error loading partners:', err);
                        this.messageService.add({
                            severity: 'error',
                            summary: this.localizationService.localize('AbpUi::Error'),
                            detail: err.error?.error?.message || 'Error loading partners'
                        });
                        this.partnerOptions = [];
                    }
                });
            },
            error: (err) => {
                console.error('Error loading INSURER partner type:', err);
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: err.error?.error?.message || 'Error loading partner type'
                });
                this.partnerOptions = [];
            }
        });
    }

    /**
     * Load plan definitions from the root parent product
     * Traverses up the parent chain to find the root product (IsRootProduct = 'Y')
     * and loads its plan definitions
     */
    loadPlanDefinitionsFromRootProduct(parentProductId: string | null): void {
        // Clear options if no parent product
        if (!parentProductId) {
            this.planDefinitionOptions = [];
            return;
        }

        // Find parent product in the products list
        let currentProduct = this.products.find(p => p.id === parentProductId);
        
        if (!currentProduct) {
            // If not found in list, try to fetch it
            this.productService.get(parentProductId).subscribe({
                next: (product) => {
                    this.traverseToRootAndLoadPlanDefinitions(product);
                },
                error: (err) => {
                    console.error('Error loading parent product:', err);
                    this.planDefinitionOptions = [];
                }
            });
            return;
        }

        this.traverseToRootAndLoadPlanDefinitions(currentProduct);
    }

    /**
     * Traverse up the parent chain to find root product and load its plan definitions
     */
    private traverseToRootAndLoadPlanDefinitions(product: ProProductListDto | ProProductDto): void {
        // Traverse up to find root product
        let rootProduct: ProProductListDto | ProProductDto | null = product;
        
        // If current product is already root, use it
        if (product.isRootProduct === 'Y') {
            rootProduct = product;
        } else {
            // Traverse up the parent chain
            let current: ProProductListDto | ProProductDto | null = product;
            const visited = new Set<string>(); // Prevent infinite loops
            
            while (current && current.rootProductId && !visited.has(current.id || '')) {
                visited.add(current.id || '');
                
                // Check if current is root
                if (current.isRootProduct === 'Y') {
                    rootProduct = current;
                    break;
                }
                
                // Find parent in products list
                const parent = this.products.find(p => p.id === current!.rootProductId);
                if (!parent) {
                    // If parent not in list, we need to fetch it
                    // For now, use current as root if we can't find parent
                    rootProduct = current;
                    break;
                }
                current = parent;
            }
            // Use the last reached node (top ancestor) as root
            rootProduct = current;
        }

        if (!rootProduct || !rootProduct.id) {
            this.planDefinitionOptions = [];
            return;
        }

        // Load plan definitions for the root product
        const input: GetProProductPlanDefinitionsInput = {
            skipCount: 0,
            maxResultCount: 1000,
            productId: rootProduct.id,
            sorting: 'planCode asc'
        };

        this.planDefinitionService.getList(input).subscribe({
            next: (result) => {
                // Filter to only active plan definitions
                this.planDefinitionOptions = (result.items || [])
                    .filter(plan => plan.status === ProProductPlanDefinitionStatus.Active)
                    .map(plan => ({
                        label: `${plan.planCode} - ${plan.planName}`,
                        value: plan.id || ''
                    }));
            },
            error: (err) => {
                console.error('Error loading plan definitions:', err);
                this.planDefinitionOptions = [];
            }
        });
    }

    /**
     * Handle parent product selection change
     * Loads plan definitions from root product and clears current plan definition selection
     */
    onRootProductIdChange(): void {
        this.tryAutofillLobFromParentProduct(this.formData.rootProductId);
        // Clear current plan definition when parent changes
        this.formData.planDefinitionId = null;
        // Load plan definitions from root product
        this.loadPlanDefinitionsFromRootProduct(this.formData.rootProductId);
    }

    loadTreeData(): void {
        this.treeLoading = true;

        // Load categories
        const categoryInput: GetProProductCategorysInput = {
            skipCount: 0,
            maxResultCount: 1000
        };

        // Load products
        const productInput: GetProProductsInput = {
            skipCount: 0,
            maxResultCount: 1000,
            lobId: this.searchForm.lobId || undefined,
            productCategoryId: this.searchForm.productCategoryId || undefined,
            partnerId: this.searchForm.partnerId || undefined,
            code: this.searchForm.code || undefined,
            name: this.searchForm.name || undefined,
            status: this.searchForm.status ?? undefined
        };

        // Load both in parallel (tree list is lightweight, no nested product details)
        Promise.all([
            this.categoryService.getList(categoryInput).toPromise(),
            this.productService.getTreeList(productInput).toPromise()
        ]).then(([categoryResult, productResult]) => {
            this.categories = categoryResult?.items || [];
            this.products = productResult?.items || [];

            // Build category options for filter
            this.categoryOptions = this.categories.map(c => ({
                label: c.name || '',
                value: c.id || ''
            }));

            // Build parent product options for dialog
            this.parentProductOptions = this.products
                .filter(p => p.isPlan !== 'Y')
                .map(p => ({
                    label: `${p.code} - ${p.name}`,
                    value: p.id || ''
                }));

            this.treeLoading = false;
        }).catch(error => {
            console.error('Error loading tree data:', error);
            this.treeLoading = false;
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('AbpUi::InternalServerErrorMessage')
            });
        });
    }

    search(): void {
        this.loadTreeData();
    }

    /**
     * Listen for Enter key globally to trigger search when no dialog is open
     */
    @HostListener('window:keydown.enter', ['$event'])
    onWindowKeyDown(event: KeyboardEvent): void {
        if (!this.dialogVisible) {
            this.search();
        }
    }

    resetSearch(): void {
        this.searchForm = {
            lobId: null,
            productCategoryId: null,
            partnerId: null,
            code: null,
            name: null,
            status: null
        };
        this.loadTreeData();
    }

    /**
     * Perform the actual export with plan definition map
     */
    private performExport(allProducts: ProProductDto[], planDefinitionMap: Map<string, string>): void {
        try {
            // Define export columns with Vietnamese headers
            const exportColumns: ExportColumn[] = [
                {
                    field: 'lobId',
                    header: this.localizationService.localize('Product::ProProduct:LOB'),
                    formatter: (value: any, row: ProProductDto) => {
                        const lob = this.lobOptions.find(opt => opt.value === row.lobId);
                        return lob ? lob.label : (row.lobId || '');
                    }
                },
                {
                    field: 'productCategoryId',
                    header: this.localizationService.localize('Product::ProProduct:ProductCategory'),
                    formatter: (value: any, row: ProProductDto) => {
                        if (!row.productCategoryId) return '';
                        const category = this.categoryOptions.find(opt => opt.value === row.productCategoryId);
                        return category ? category.label : (row.productCategoryId || '');
                    }
                },
                {
                    field: 'partnerId',
                    header: this.localizationService.localize('Product::ProProduct:Partner'),
                    formatter: (value: any, row: ProProductDto) => {
                        if (!row.partnerId) return '';
                        const partner = this.partnerOptions.find(opt => opt.value === row.partnerId);
                        return partner ? partner.label : (row.partnerId || '');
                    }
                },
                {
                    field: 'rootProductId',
                    header: this.localizationService.localize('Product::ProProduct:ParentProduct'),
                    formatter: (value: any, row: ProProductDto) => {
                        if (!row.rootProductId) return '';
                        const parent = this.parentProductOptions.find(opt => opt.value === row.rootProductId);
                        return parent ? parent.label : (row.rootProductId || '');
                    }
                },
                {
                    field: 'code',
                    header: this.localizationService.localize('Product::ProProduct:Code')
                },
                {
                    field: 'name',
                    header: this.localizationService.localize('Product::ProProduct:Name')
                },
                {
                    field: 'insurerProductCode',
                    header: this.localizationService.localize('Product::ProProduct:InsurerProductCode')
                },
                {
                    field: 'status',
                    header: this.localizationService.localize('Product::ProProduct:Status'),
                    formatter: (value: any) => this.formatStatus(value)
                },
                {
                    field: 'effectDate',
                    header: this.localizationService.localize('Product::ProProduct:EffectDate'),
                    formatter: (value: any) => {
                        if (!value) return '';
                        const date = new Date(value);
                        if (isNaN(date.getTime())) return '';
                        const day = String(date.getDate()).padStart(2, '0');
                        const month = String(date.getMonth() + 1).padStart(2, '0');
                        const year = date.getFullYear();
                        return `${day}/${month}/${year}`;
                    }
                },
                {
                    field: 'expireDate',
                    header: this.localizationService.localize('Product::ProProduct:ExpireDate'),
                    formatter: (value: any) => {
                        if (!value) return '';
                        const date = new Date(value);
                        if (isNaN(date.getTime())) return '';
                        const day = String(date.getDate()).padStart(2, '0');
                        const month = String(date.getMonth() + 1).padStart(2, '0');
                        const year = date.getFullYear();
                        return `${day}/${month}/${year}`;
                    }
                },
                {
                    field: 'rateType',
                    header: this.localizationService.localize('Product::ProProduct:RateType')
                },
                {
                    field: 'productTypeId',
                    header: this.localizationService.localize('Product::ProProduct:ProductType'),
                    formatter: (value: any, row: ProProductDto) => {
                        if (!row.productTypeId) return '';
                        const productType = this.productTypeOptions.find(opt => opt.value === row.productTypeId);
                        return productType ? productType.label : (row.productTypeId || '');
                    }
                },
                {
                    field: 'planDefinitionId',
                    header: this.localizationService.localize('Product::ProProduct:PlanDefinition'),
                    formatter: (value: any, row: ProProductDto) => {
                        if (!row.planDefinitionId) return '';
                        const displayText = planDefinitionMap.get(row.planDefinitionId);
                        return displayText || row.planDefinitionId;
                    }
                }
            ];

            // Export to Excel
            exportToExcel(allProducts, exportColumns, 'ProProduct');

            // Show success message
            this.messageService.add({
                severity: 'success',
                summary: this.localizationService.localize('Product::Success'),
                detail: this.localizationService.localize('Product::ProTableRate:ExportSuccess')
            });
        } catch (error) {
            // Show error message
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('Product::Error'),
                detail: this.localizationService.localize('Product::ProTableRate:ExportFailed')
            });
        } finally {
            this.exporting = false;
        }
    }

    /**
     * Export products to Excel
     */
    exportToExcel(): void {
        this.exporting = true;

        // Build search input with current filters and large maxResultCount to get all records
        const productInput: GetProProductsInput = {
            skipCount: 0,
            maxResultCount: 1000,
            lobId: this.searchForm.lobId || undefined,
            productCategoryId: this.searchForm.productCategoryId || undefined,
            partnerId: this.searchForm.partnerId || undefined,
            code: this.searchForm.code || undefined,
            name: this.searchForm.name || undefined,
            status: this.searchForm.status ?? undefined
        };

        this.productService.getList(productInput).subscribe({
            next: (result) => {
                try {
                    const allProducts = result.items || [];

                    // Check if there's data to export
                    if (allProducts.length === 0) {
                        this.messageService.add({
                            severity: 'warn',
                            summary: this.localizationService.localize('Product::Warning'),
                            detail: this.localizationService.localize('Product::ProTableRate:NoDataToExport')
                        });
                        this.exporting = false;
                        return;
                    }

                    // Collect all unique planDefinitionIds from products
                    const planDefinitionIds = new Set<string>();
                    allProducts.forEach(product => {
                        if (product.planDefinitionId) {
                            planDefinitionIds.add(product.planDefinitionId);
                        }
                    });

                    // Fetch all plan definitions in parallel
                    const planDefinitionRequests = Array.from(planDefinitionIds).map(id => 
                        this.planDefinitionService.get(id).pipe(
                            catchError(err => {
                                console.warn(`Failed to fetch plan definition ${id}:`, err);
                                return of(null);
                            })
                        )
                    );

                    // If no plan definitions to fetch, proceed directly to export
                    if (planDefinitionRequests.length === 0) {
                        this.performExport(allProducts, new Map());
                        return;
                    }

                    // Fetch all plan definitions and then export
                    forkJoin(planDefinitionRequests).subscribe({
                        next: (planDefinitions) => {
                            // Create a map of planDefinitionId -> display string (code - name)
                            const planDefinitionMap = new Map<string, string>();
                            planDefinitions.forEach(planDef => {
                                if (planDef && planDef.id) {
                                    const displayText = `${planDef.planCode || ''} - ${planDef.planName || ''}`.trim();
                                    planDefinitionMap.set(planDef.id, displayText || planDef.id);
                                }
                            });

                            // Also add any plan definitions from the existing options
                            this.planDefinitionOptions.forEach(opt => {
                                if (!planDefinitionMap.has(opt.value)) {
                                    planDefinitionMap.set(opt.value, opt.label);
                                }
                            });

                            this.performExport(allProducts, planDefinitionMap);
                        },
                        error: (err) => {
                            console.error('Error fetching plan definitions:', err);
                            // Proceed with export using existing options only
                            const planDefinitionMap = new Map<string, string>();
                            this.planDefinitionOptions.forEach(opt => {
                                planDefinitionMap.set(opt.value, opt.label);
                            });
                            this.performExport(allProducts, planDefinitionMap);
                        }
                    });
                } catch (error) {
                    // Show error message
                    this.messageService.add({
                        severity: 'error',
                        summary: this.localizationService.localize('Product::Error'),
                        detail: this.localizationService.localize('Product::ProTableRate:ExportFailed')
                    });
                    this.exporting = false;
                }
            },
            error: (err) => {
                console.error('Error exporting products:', err);
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('Product::Error'),
                    detail: err.error?.error?.message || this.localizationService.localize('Product::ProTableRate:ExportFailed')
                });
                this.exporting = false;
            }
        });
    }

    onTreeNodeSelect(node: ProductTreeNode): void {
        if (node.nodeType === 'product') {
            const listItem = node.data as ProProductListDto;
            if (!listItem.id) return;
            this.isInCreateMode = false;
            this.isInEditMode = false;
            this.productDetailTabValue = '0';
            this.detailLoading = true;
            this.productService.get(listItem.id).subscribe({
                next: (fullProduct) => {
                    this.selectedProduct = fullProduct;
                    this.populateFormFromProduct(fullProduct);
                    this.coverageMenuItemsCache.clear();
                    this.tableRateMenuItemsCache.clear();
                    this.detailLoading = false;
                },
                error: () => {
                    this.detailLoading = false;
                }
            });
        } else {
            this.isInCreateMode = false;
            this.isInEditMode = false;
            this.selectedProduct = null;
            this.formData = this.getEmptyForm();
        }
    }

    /**
     * Populate form data from selected product for inline editing
     */
    private populateFormFromProduct(product: ProProductDto): void {
        this.formData = {
            id: product.id,
            lobId: product.lobId || '',
            productCategoryId: product.productCategoryId || null,
            partnerId: product.partnerId || null,
            code: product.code || '',
            name: product.name || '',
            shortName: product.shortName || '',
            status: product.status ?? ProProductStatus.Deactive,
            effectDate: product.effectDate ? new Date(product.effectDate) : new Date(),
            expireDate: product.expireDate ? new Date(product.expireDate) : null,
            rateType: product.rateType || 'table_rate',
            productTypeId: product.productTypeId || null,
            seqNumber: product.seqNumber ?? 1,
            isPlan: product.isPlan ?? 'N',
            description: product.description || '',
            internalNote: product.internalNote || '',
            insurerProductCode: product.insurerProductCode || '',
            rootProductId: product.rootProductId || null,
            isRootProduct: product.isRootProduct || 'Y',
            currencyId: product.currencyId || null,
            planDefinitionId: product.planDefinitionId || null,
            imageDocumentId: product.imageDocumentId || null,
            certificateTemplateDocumentId: product.certificateTemplateDocumentId || null
        };

        // Load image URL if imageDocumentId exists
        if (product.imageDocumentId) {
            this.loadProductImageUrl(product.imageDocumentId);
        } else {
            this.productImageUrl = null;
        }

        if (product.certificateTemplateDocumentId) {
            this.loadCertificateTemplateDocumentMeta(product.certificateTemplateDocumentId);
        } else {
            this.certificateTemplateDocumentUrl = null;
            this.certificateTemplateFileName = null;
        }

        // Load plan definitions from root product if parent product is set
        if (product.rootProductId) {
            this.loadPlanDefinitionsFromRootProduct(product.rootProductId);
        } else {
            this.planDefinitionOptions = [];
        }

        // Load product attributes
        this.productAttributes = product.productAttributes || [];
        
        // Load product coverages
        this.productCoverages = product.productCoverages || [];
        this.buildCoverageTree();

        // Load product table rates
        this.productTableRates = product.productTableRates || [];
        
        // Load table rate options when product is selected
        if (product.lobId) {
            this.loadTableRateOptions(product.lobId, product.partnerId || undefined);
        }

        // Load product rules
        if (product.id) {
            this.loadProductRules(product.id);
        }

        // Load product plan definitions from product (only if PartnerId is null)
        if (product.id && !product.partnerId) {
            const list = product.productPlanDefinitions || [];
            this.productPlanDefinitions = [...list].sort((a, b) => {
                const ta = a.creationTime ? new Date(a.creationTime).getTime() : 0;
                const tb = b.creationTime ? new Date(b.creationTime).getTime() : 0;
                return tb - ta;
            });
        } else {
            this.productPlanDefinitions = [];
        }

        // Load product distributions (always load when product has id)
        if (product.id) {
            this.loadProductDistributions(product);
        } else {
            this.resetProductDistributionState();
        }
    }

    /**
     * Reset the detail form to the original selected product values
     */
    resetDetailForm(): void {
        if (this.selectedProduct) {
            this.populateFormFromProduct(this.selectedProduct);
        } else {
            this.formData = this.getEmptyForm();
        }
    }

    /**
     * Cancel product form action with confirmation
     */
    onCancelProductForm(): void {
        this.confirmWithDefaults({
            message: this.localizationService.localize('Product::ProProduct:CancelActionConfirm'),
            header: this.localizationService.localize('Product::Confirm'),
            icon: 'pi pi-exclamation-triangle',
            accept: () => {
                if (this.isCreateMode()) {
                    // In create mode, clear everything and exit create mode
                    this.isInCreateMode = false;
                    this.isInEditMode = false;
                    this.selectedProduct = null;
                    this.formData = this.getEmptyForm();
                    this.uploadedImageFile = null;
                    this.productImageUrl = null;
                    this.imageUploadLoading = false;
                    this.uploadedCertificateTemplateFile = null;
                    this.certificateTemplateDocumentUrl = null;
                    this.certificateTemplateFileName = null;
                    this.certificateTemplateUploadLoading = false;
                    this.productAttributes = [];
                    this.productCoverages = [];
                    // Reset coverage form state
                    this.coverageFormVisible = false;
                    this.selectedCoverageForEdit = null;
                    this.selectedCoverageId = null;
                    this.selectedParentCoverageId = null;
                    this.selectedCoverageGroupId = null;
                    this.selectedUomId = null;
                    this.selectedTaxId = null;
                    this.selectedAvailabilityType = null;
                    this.selectedEffectDate = null;
                    this.selectedExpireDate = null;
                    this.selectedSeqNumber = 1;
                    this.selectedInsurerCoverageCode = '';
                    this.selectedEnableQuantity = false;
                    this.coverageTreeData = [];
                    // Reset interaction modal state
                    this.interactionModalVisible = false;
                    this.selectedCoverageForInteraction = null;
                    this.interactionFormVisible = false;
                    this.resetInteractionForm();
                    // Reset level modal state
                    this.levelModalVisible = false;
                    this.selectedCoverageForLevel = null;
                    this.coverageLevels = [];
                    this.levelFormVisible = false;
                    this.resetLevelForm();
                    this.productTableRates = [];
                    this.productPlanDefinitions = [];
                    this.resetProductDistributionState();
                } else {
                    // In edit/detail mode, reset to original product data first
                    if (this.selectedProduct) {
                        this.populateFormFromProduct(this.selectedProduct);
                    }
                    // Then clear the detail panel (same as create mode)
                    this.isInEditMode = false;
                    this.selectedProduct = null;
                    // Reset coverage form state
                    this.coverageFormVisible = false;
                    this.selectedCoverageForEdit = null;
                    this.selectedCoverageId = null;
                    this.selectedParentCoverageId = null;
                    this.selectedCoverageGroupId = null;
                    this.selectedUomId = null;
                    this.selectedTaxId = null;
                    this.selectedAvailabilityType = null;
                    this.selectedEffectDate = null;
                    this.selectedExpireDate = null;
                    this.selectedSeqNumber = 1;
                    this.selectedInsurerCoverageCode = '';
                    this.selectedEnableQuantity = false;
                    // Reset interaction modal state
                    this.interactionModalVisible = false;
                    this.selectedCoverageForInteraction = null;
                    this.interactionFormVisible = false;
                    this.resetInteractionForm();
                    // Reset level modal state
                    this.levelModalVisible = false;
                    this.selectedCoverageForLevel = null;
                    this.coverageLevels = [];
                    this.levelFormVisible = false;
                    this.resetLevelForm();
                }
            }
        });
    }

    /**
     * Execute product form action (create or update) with confirmation
     */
    onExecuteProductForm(): void {
        // Safety check: should not be called in detail mode
        if (this.isDetailMode()) {
            return;
        }

        if (!this.validateAllTabs()) {
            return;
        }

        const isCreate = this.isCreateMode();
        const message = isCreate
            ? this.localizationService.localize('Product::ProProduct:CreateProductConfirm')
            : this.localizationService.localize('Product::ProProduct:UpdateProductConfirm');

        this.confirmWithDefaults({
            message,
            header: this.localizationService.localize('Product::Confirm'),
            icon: 'pi pi-question-circle',
            accept: () => {
                if (isCreate) {
                    this.create();
                } else {
                    this.update();
                }
            }
        });
    }

    /**
     * Save the inline edits from the detail panel (internal helper)
     */
    private saveDetail(): void {
        if (!this.selectedProduct?.id) return;

        if (!this.validateAllTabs()) {
            return;
        }

        this.update();
    }

    onAddProduct(event: { parentCategoryId?: string; parentProductId?: string }): void {
        this.isInCreateMode = true;
        this.isInEditMode = false;
        this.selectedProduct = null;
        this.formData = this.getEmptyForm();
        this.productDetailTabValue = '0';
        this.uploadedImageFile = null;
        this.productImageUrl = null;
        this.imageUploadLoading = false;
        this.uploadedCertificateTemplateFile = null;
        this.certificateTemplateDocumentUrl = null;
        this.certificateTemplateFileName = null;
        this.certificateTemplateUploadLoading = false;

        if (event.parentCategoryId) {
            this.formData.productCategoryId = event.parentCategoryId;
            const parentCat = this.categories.find(c => c.id === event.parentCategoryId);
            if (parentCat?.lobId) {
                this.formData.lobId = parentCat.lobId;
            }
            this.onLobIdChange();
        }
        if (event.parentProductId) {
            this.formData.rootProductId = event.parentProductId;
            this.tryAutofillLobFromParentProduct(event.parentProductId);
            // Load plan definitions from root product
            this.loadPlanDefinitionsFromRootProduct(event.parentProductId);
        } else {
            this.planDefinitionOptions = [];
        }

        // Reset related data
        this.productAttributes = [];
        this.productCoverages = [];
        this.productTableRates = [];
        this.productPlanDefinitions = [];
        this.resetProductDistributionState();
    }

    onEditProduct(product: ProProductListDto | ProProductDto): void {
        if (!product.id) return;
        this.isInCreateMode = false;
        // If we already have full product (e.g. from header Edit), use it; otherwise fetch
        const fullProduct = 'productAttributes' in product ? product as ProProductDto : null;
        if (fullProduct?.productAttributes) {
            this.selectedProduct = fullProduct;
            this.isInEditMode = true;
            this.populateFormFromProduct(fullProduct);
            this.coverageMenuItemsCache.clear();
            this.tableRateMenuItemsCache.clear();
            return;
        }
        this.detailLoading = true;
        this.productService.get(product.id).subscribe({
            next: (loaded) => {
                this.selectedProduct = loaded;
                this.isInEditMode = true;
                this.populateFormFromProduct(loaded);
                this.coverageMenuItemsCache.clear();
                this.tableRateMenuItemsCache.clear();
                this.detailLoading = false;
            },
            error: () => {
                this.detailLoading = false;
            }
        });
    }

    onDeleteProduct(product: ProProductListDto): void {
        if (!product.id) return;

        // Only active products have restrictions on delete
        if (product.status === ProProductStatus.Active) {
            this.messageService.add({
                severity: 'warn',
                summary: this.localizationService.localize('AbpUi::Warning'),
                detail: this.localizationService.localize('Product::ProProduct:CannotDeleteActive')
            });
            return;
        }

        this.confirmWithDefaults({
            message: this.localizationService.localize('Product::ProProduct:DeleteConfirm'),
            header: this.localizationService.localize('Product::Delete'),
            icon: 'pi pi-exclamation-triangle',
            accept: () => {
                this.loading = true;
                this.productService.delete(product.id!).subscribe({
                    next: () => {
                        this.messageService.add({
                            severity: 'success',
                            summary: this.localizationService.localize('AbpUi::Success'),
                            detail: this.localizationService.localize('Product::ProProduct:Deleted')
                        });
                        this.isInCreateMode = false;
                        this.selectedProduct = null;
                        this.loadTreeData();
                    },
                    error: (err) => {
                        this.loading = false;
                        this.messageService.add({
                            severity: 'error',
                            summary: this.localizationService.localize('AbpUi::Error'),
                            detail: err.error?.error?.message || 'Error deleting product'
                        });
                    }
                });
            }
        });
    }

    onMakeActive(product: ProProductListDto): void {
        if (!product.id) return;

        this.confirmWithDefaults({
            message: this.localizationService.localize('Product::ProProduct:ActivateConfirm'),
            header: this.localizationService.localize('Product::ProProduct:Activate'),
            icon: 'pi pi-check-circle',
            accept: () => {
                this.productService.get(product.id!).subscribe({
                    next: (fullProduct) => this.updateProductStatus(fullProduct, ProProductStatus.Active),
                    error: () => { this.loading = false; }
                });
            }
        });
    }

    onMakeInactive(product: ProProductListDto): void {
        if (!product.id) return;

        this.confirmWithDefaults({
            message: this.localizationService.localize('Product::ProProduct:DeactivateConfirm'),
            header: this.localizationService.localize('Product::ProProduct:Deactivate'),
            icon: 'pi pi-times-circle',
            accept: () => {
                this.productService.get(product.id!).subscribe({
                    next: (fullProduct) => this.updateProductStatus(fullProduct, ProProductStatus.Deactive),
                    error: () => { this.loading = false; }
                });
            }
        });
    }

    private updateProductStatus(product: ProProductDto, status: ProProductStatus): void {
        this.loading = true;
        const updateDto: UpdateProProductDto = {
            lobId: product.lobId!,
            shortName: product.shortName!,
            name: product.name!,
            rateType: product.rateType!,
            status: status,
            seqNumber: product.seqNumber,
            effectDate: product.effectDate!,
            productTypeId: product.productTypeId,
            partnerId: product.partnerId,
            tableRateId: product.tableRateId,
            rootProductId: product.rootProductId,
            isRootProduct: product.isRootProduct,
            productCategoryId: product.productCategoryId,
            currencyId: product.currencyId,
            insurerProductCode: product.insurerProductCode,
            description: product.description,
            internalNote: product.internalNote,
            expireDate: product.expireDate,
            planDefinitionId: product.planDefinitionId,
            isPlan: product.isPlan,
            productAttributes: product.productAttributes || [],
            productCoverages: product.productCoverages || [],
            productTableRates: product.productTableRates || [],
            proRules: [], // Rules are managed separately via ProRule service API
            productPlanDefinitions: [], // Plan definitions are managed separately via ProProductPlanDefinition service API
            productDistributions: product.productDistributions || []
        };

        this.productService.update(product.id!, updateDto).subscribe({
            next: () => {
                this.messageService.add({
                    severity: 'success',
                    summary: this.localizationService.localize('AbpUi::Success'),
                    detail: this.localizationService.localize('Product::ProProduct:StatusUpdated')
                });
                this.loadTreeData();
            },
            error: (err) => {
                this.loading = false;
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: err.error?.error?.message || 'Error updating status'
                });
            }
        });
    }

    openCreateDialog(): void {
        this.isInCreateMode = true;
        this.isInEditMode = false;
        this.selectedProduct = null;
        this.formData = this.getEmptyForm();
        this.productDetailTabValue = '0';
        this.uploadedImageFile = null;
        this.productImageUrl = null;
        this.imageUploadLoading = false;
        this.uploadedCertificateTemplateFile = null;
        this.certificateTemplateDocumentUrl = null;
        this.certificateTemplateFileName = null;
        this.certificateTemplateUploadLoading = false;
        
        // Set default currency to VND if available
        this.setDefaultVndCurrency();
        
        // Reset related data
        this.productAttributes = [];
        this.productCoverages = [];
        this.productTableRates = [];
        this.productPlanDefinitions = [];
        this.resetProductDistributionState();
        
        // Reset all form visibility flags
        this.attributeFormVisible = false;
        this.coverageFormVisible = false;
        this.tableRateFormVisible = false;
        this.ruleFormVisible = false;
        this.planDefinitionFormVisible = false;
        
        // Reset all selected items for editing
        this.selectedCoverageForEdit = null;
        this.selectedTableRateForEdit = null;
        this.selectedRuleForEdit = null;
        this.editingPlanDefinition = null;
        this.editingPlanDefinitionIndex = -1;
        
        // Reset all modal visibility flags
        this.interactionModalVisible = false;
        this.levelModalVisible = false;
        this.termModalVisible = false;
        
        // Reset coverage-related state
        this.coverageTreeData = [];
        this.selectedCoverageId = null;
        this.selectedParentCoverageId = null;
        this.selectedCoverageGroupId = null;
        this.selectedUomId = null;
        this.selectedTaxId = null;
        this.selectedAvailabilityType = null;
        this.selectedEffectDate = null;
        this.selectedExpireDate = null;
        this.selectedSeqNumber = 1;
        this.selectedInsurerCoverageCode = '';
        this.selectedEnableQuantity = false;
        this.selectedCoverageForInteraction = null;
        this.selectedCoverageForLevel = null;
        this.coverageLevels = [];
        this.selectedLevel = null;
        this.selectedLevelForTerm = null;
        this.selectedTerm = null;
        
        // Reset table rate form state
        this.selectedTableRateId = null;
        this.selectedTableRateEffectDate = null;
        this.selectedTableRateExpireDate = null;
        
        // Reset rule form state
        this.selectedRuleTypeId = null;
        this.selectedRuleCode = '';
        this.selectedRuleName = '';
        this.selectedRulePriority = 1;
        this.selectedRuleStatus = ProRuleStatus.Active;
        this.selectedRuleEffectDate = null;
        this.selectedRuleExpireDate = null;
        this.selectedRuleScript = '';
        this.selectedRuleDescription = '';
        this.isRuleViewMode = false;
        
        // Reset attribute form state
        this.selectedAttributeId = null;
        this.selectedAttributeIsRequired = 'N';
        
        // Call reset methods for complex forms
        this.resetInteractionForm();
        this.resetLevelForm();
        this.resetTermForm();
        this.resetRuleForm();
        
        // Clear menu item caches
        this.coverageMenuItemsCache.clear();
        this.tableRateMenuItemsCache.clear();
    }

    /**
     * Check if we're in create mode (user explicitly clicked "Thêm mới")
     */
    isCreateMode(): boolean {
        return this.isInCreateMode && this.selectedProduct === null;
    }

    /**
     * Check if we're in detail mode (read-only view of existing product)
     */
    isDetailMode(): boolean {
        return !this.isCreateMode() && this.selectedProduct !== null && !this.isInEditMode;
    }

    /**
     * Check if we're in edit mode (editing existing product)
     */
    isEditMode(): boolean {
        return !this.isCreateMode() && this.selectedProduct !== null && this.isInEditMode;
    }

    /**
     * Switch from detail mode to edit mode
     */
    onSwitchToEditMode(): void {
        if (this.isDetailMode()) {
            this.isInEditMode = true;
            // Clear menu item caches to refresh disabled states
            this.coverageMenuItemsCache.clear();
            this.tableRateMenuItemsCache.clear();
        }
    }

    save(): void {
        if (!this.validateAllTabs()) {
            return;
        }

        if (this.dialogMode === 'create') {
            this.create();
        } else {
            this.update();
        }
    }

    /**
     * Validate all tabs before submitting
     */
    private validateAllTabs(): boolean {
        if (!this.validateGeneralInformationTab()) {
            return false;
        }
        if (!this.validateAttributesTab()) {
            return false;
        }
        if (!this.validateCoveragesTab()) {
            return false;
        }
        if (!this.validateTableRatesTab()) {
            return false;
        }
        if (!this.validateRulesTab()) {
            return false;
        }
        if (!this.validatePackageDefinitionTab()) {
            return false;
        }
        return true;
    }

    /**
     * Handle image file selection
     */
    onImageUpload(event: any): void {
        const file = event.files?.[0];
        if (!file) {
            return;
        }

        // Validate file type
        const validTypes = ['image/jpeg', 'image/jpg', 'image/png'];
        if (!validTypes.includes(file.type)) {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Product::ProProduct:InvalidImageFormat')
            });
            return;
        }

        // Validate file size (5MB)
        const maxSize = 5 * 1024 * 1024; // 5MB in bytes
        if (file.size > maxSize) {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Product::ProProduct:ImageSizeExceeded')
            });
            return;
        }

        this.uploadedImageFile = file;
        // Create preview URL
        const reader = new FileReader();
        reader.onload = (e: any) => {
            this.productImageUrl = e.target.result;
        };
        reader.readAsDataURL(file);

        // Auto upload
        this.uploadProductImage();
    }

    /**
     * Get PRODUCT_IMAGE document type
     */
    private getProductImageDocumentType(): Observable<string> {
        const code = 'PRODUCT_IMAGE';
        return this.resDocumentTypeService.getList({ code: code, maxResultCount: 1 }).pipe(
            switchMap(res => {
                if (res.items && res.items.length > 0) {
                    return of(res.items[0].id!);
                } else {
                    this.messageService.add({
                        severity: 'error',
                        summary: this.localizationService.localize('AbpUi::Error'),
                        detail: 'PRODUCT_IMAGE document type not found. Please ensure it exists in the database.'
                    });
                    return EMPTY;
                }
            }),
            catchError(error => {
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: error?.error?.error?.message || 'Failed to get document type'
                });
                return EMPTY;
            })
        );
    }

    /**
     * Upload product image
     */
    uploadProductImage(): void {
        if (!this.uploadedImageFile) {
            return;
        }

        this.imageUploadLoading = true;
        this.getProductImageDocumentType().subscribe({
            next: (documentTypeId) => {
                this.resDocumentService.uploadSingleFile(
                    documentTypeId,
                    'PRODUCT_IMAGE',
                    this.uploadedImageFile!
                ).subscribe({
                    next: (document) => {
                        this.formData.imageDocumentId = document.id!;
                        this.productImageUrl = document.url || null;
                        this.imageUploadLoading = false;
                        this.messageService.add({
                            severity: 'success',
                            summary: this.localizationService.localize('AbpUi::Success'),
                            detail: this.localizationService.localize('Product::ProProduct:ImageUploaded')
                        });
                    },
                    error: (error) => {
                        this.imageUploadLoading = false;
                        const errorMessage = error?.error?.error?.message ||
                            error?.error?.error?.details ||
                            this.localizationService.localize('AbpUi::InternalServerErrorMessage');
                        this.messageService.add({
                            severity: 'error',
                            summary: this.localizationService.localize('AbpUi::Error'),
                            detail: errorMessage
                        });
                    }
                });
            },
            error: () => {
                this.imageUploadLoading = false;
            }
        });
    }

    /**
     * Load product image URL from ResDocument
     */
    private loadProductImageUrl(imageDocumentId: string): void {
        this.resDocumentService.getSingleFile(imageDocumentId).subscribe({
            next: (document: FileResponseDto) => {
                this.productImageUrl = document.url || null;
            },
            error: (error: any) => {
                // Silently fail - image might not exist
                this.productImageUrl = null;
            }
        });
    }

    /**
     * Handle product certificate template (.docx) file selection
     */
    onCertificateTemplateUpload(event: { files?: File[] }): void {
        const file = event.files?.[0];
        if (!file) {
            return;
        }

        const docxMime = 'application/vnd.openxmlformats-officedocument.wordprocessingml.document';
        const nameOk = file.name.toLowerCase().endsWith('.docx');
        const typeOk = !file.type || file.type === docxMime;
        if (!nameOk || !typeOk) {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Product::ProProduct:InvalidCertificateTemplateFormat')
            });
            return;
        }

        const maxSize = 5 * 1024 * 1024;
        if (file.size > maxSize) {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Product::ProProduct:CertificateTemplateSizeExceeded')
            });
            return;
        }

        this.uploadedCertificateTemplateFile = file;
        this.uploadCertificateTemplate();
    }

    private getCertificateTemplateDocumentType(): Observable<string> {
        const code = 'PROD_CERT_TEMPLATE';
        return this.resDocumentTypeService.getList({ code: code, maxResultCount: 1 }).pipe(
            switchMap(res => {
                if (res.items && res.items.length > 0) {
                    return of(res.items[0].id!);
                } else {
                    this.messageService.add({
                        severity: 'error',
                        summary: this.localizationService.localize('AbpUi::Error'),
                        detail: 'PROD_CERT_TEMPLATE document type not found. Please ensure it exists in the database.'
                    });
                    return EMPTY;
                }
            }),
            catchError(error => {
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: error?.error?.error?.message || 'Failed to get document type'
                });
                return EMPTY;
            })
        );
    }

    private uploadCertificateTemplate(): void {
        if (!this.uploadedCertificateTemplateFile) {
            return;
        }

        this.certificateTemplateUploadLoading = true;
        this.getCertificateTemplateDocumentType().subscribe({
            next: (documentTypeId) => {
                this.resDocumentService.uploadSingleFile(
                    documentTypeId,
                    'PROD_CERT_TEMPLATE',
                    this.uploadedCertificateTemplateFile!
                ).subscribe({
                    next: (document) => {
                        this.formData.certificateTemplateDocumentId = document.id!;
                        this.certificateTemplateDocumentUrl = document.url || null;
                        this.certificateTemplateFileName = document.fileName || null;
                        this.certificateTemplateUploadLoading = false;
                        this.messageService.add({
                            severity: 'success',
                            summary: this.localizationService.localize('AbpUi::Success'),
                            detail: this.localizationService.localize('Product::ProProduct:CertificateTemplateUploaded')
                        });
                    },
                    error: (error) => {
                        this.certificateTemplateUploadLoading = false;
                        const errorMessage = error?.error?.error?.message ||
                            error?.error?.error?.details ||
                            this.localizationService.localize('AbpUi::InternalServerErrorMessage');
                        this.messageService.add({
                            severity: 'error',
                            summary: this.localizationService.localize('AbpUi::Error'),
                            detail: errorMessage
                        });
                    }
                });
            },
            error: () => {
                this.certificateTemplateUploadLoading = false;
            }
        });
    }

    private loadCertificateTemplateDocumentMeta(certificateTemplateDocumentId: string): void {
        this.resDocumentService.getSingleFile(certificateTemplateDocumentId).subscribe({
            next: (document: FileResponseDto) => {
                this.certificateTemplateDocumentUrl = document.url || null;
                this.certificateTemplateFileName = document.fileName || null;
            },
            error: () => {
                this.certificateTemplateDocumentUrl = null;
                this.certificateTemplateFileName = null;
            }
        });
    }

    /**
     * Validate General Information tab
     */
    private validateGeneralInformationTab(): boolean {
        if (!this.formData.lobId) {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Product::ProProduct:LobRequired')
            });
            return false;
        }

        if (!this.formData.code || this.formData.code.trim() === '') {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Product::ProProduct:CodeRequired')
            });
            return false;
        }

        // Validate code format: A-Z, _, 0-9 only
        const codeRegex = /^[A-Z0-9_]+$/i;
        if (!codeRegex.test(this.formData.code.trim())) {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Product::ProProduct:CodeFormatInvalid')
            });
            return false;
        }

        if (!this.formData.name || this.formData.name.trim() === '') {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Product::ProProduct:NameRequired')
            });
            return false;
        }

        if (!this.formData.shortName || this.formData.shortName.trim() === '') {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Product::ProProduct:ShortNameRequired')
            });
            return false;
        }

        // Validate currency (required)
        if (!this.formData.currencyId) {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Product::ProProduct:CurrencyRequired')
            });
            return false;
        }

        if (!this.formData.effectDate) {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Product::ProProduct:EffectDateRequired')
            });
            return false;
        }

        const today = new Date();
        const effectDate = this.formData.effectDate instanceof Date 
                ? new Date(this.formData.effectDate)
                : new Date(this.formData.effectDate);

        // Validate effect date >= current date (only for create mode, not edit mode)
        if (!this.isEditMode()) {
            today.setHours(0, 0, 0, 0);
            effectDate.setHours(0, 0, 0, 0);

            if (effectDate < today) {
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: this.localizationService.localize('Product::ProProduct:EffectDateMustBeTodayOrLater')
                });
                return false;
            }
        }

        // Validate expire date if provided
        if (this.formData.expireDate) {
            // Normalize today to 0 hours for consistent date comparison
            today.setHours(0, 0, 0, 0);
            effectDate.setHours(0, 0, 0, 0);
            
            const expireDate = this.formData.expireDate instanceof Date 
                ? new Date(this.formData.expireDate)
                : new Date(this.formData.expireDate);
            expireDate.setHours(0, 0, 0, 0);

            if (expireDate < today) {
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: this.localizationService.localize('Product::ProProduct:ExpireDateMustBeTodayOrLater')
                });
                return false;
            }

            if (expireDate < effectDate) {
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: this.localizationService.localize('Product::ProProduct:ExpireDateMustBeAfterEffectDate')
                });
                return false;
            }
        }

        // Validate isPlan requirements
        if (this.formData.isPlan === 'Y') {
            if (!this.formData.planDefinitionId) {
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: this.localizationService.localize('Product::ProProduct:PlanDefinitionRequired')
                });
                return false;
            }

            if (!this.formData.rootProductId) {
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: this.localizationService.localize('Product::ProProduct:RootProductRequired')
                });
                return false;
            }
        }

        return true;
    }

    /**
     * Validate Attributes tab
     */
    private validateAttributesTab(): boolean {
        // Check if any required attributes are missing
        // Note: Per documentation, attributes are optional, so no validation needed
        // But we could check if required attributes (isRequired = 'Y') are present
        return true;
    }

    /**
     * Validate Coverages tab
     */
    private validateCoveragesTab(): boolean {
        const today = new Date();
        today.setHours(0, 0, 0, 0);

        for (let i = 0; i < this.productCoverages.length; i++) {
            const coverage = this.productCoverages[i];
            
            if (!coverage.coverageId) {
                let message = this.localizationService.localize('Product::ProProductCoverage:CoverageRequired');
                message = message.replace('{0}', String(i + 1));
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: message
                });
                return false;
            }

            if (!coverage.uomId) {
                let message = this.localizationService.localize('Product::ProProductCoverage:UomRequired');
                message = message.replace('{0}', String(i + 1));
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: message
                });
                return false;
            }

            if (!coverage.taxId) {
                let message = this.localizationService.localize('Product::ProProductCoverage:TaxRequired');
                message = message.replace('{0}', String(i + 1));
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: message
                });
                return false;
            }

            if (coverage.availabilityType === undefined || coverage.availabilityType === null) {
                let message = this.localizationService.localize('Product::ProProductCoverage:AvailabilityTypeRequired');
                message = message.replace('{0}', String(i + 1));
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: message
                });
                return false;
            }

            if (!coverage.effectDate) {
                let message = this.localizationService.localize('Product::ProProductCoverage:EffectDateRequired');
                message = message.replace('{0}', String(i + 1));
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: message
                });
                return false;
            }

            // Validate effect date >= current date (only for create mode, not edit mode)
            const effectDate = new Date(coverage.effectDate);
            if (!this.isEditMode()) {
                effectDate.setHours(0, 0, 0, 0);

                if (effectDate < today) {
                    let message = this.localizationService.localize('Product::ProProductCoverage:EffectDateMustBeTodayOrLater');
                    message = message.replace('{0}', String(i + 1));
                    this.messageService.add({
                        severity: 'error',
                        summary: this.localizationService.localize('AbpUi::Error'),
                        detail: message
                    });
                    return false;
                }
            }

            // Validate expire date if provided
            if (coverage.expireDate) {
                const expireDate = new Date(coverage.expireDate);
                expireDate.setHours(0, 0, 0, 0);

                if (!this.isEditMode() && expireDate < today) {
                    let message = this.localizationService.localize('Product::ProProductCoverage:ExpireDateMustBeTodayOrLater');
                    message = message.replace('{0}', String(i + 1));
                    this.messageService.add({
                        severity: 'error',
                        summary: this.localizationService.localize('AbpUi::Error'),
                        detail: message
                    });
                    return false;
                }

                if (expireDate < effectDate) {
                    let message = this.localizationService.localize('Product::ProProductCoverage:ExpireDateMustBeAfterEffectDate');
                    message = message.replace('{0}', String(i + 1));
                    this.messageService.add({
                        severity: 'error',
                        summary: this.localizationService.localize('AbpUi::Error'),
                        detail: message
                    });
                    return false;
                }
            }
        }

        if (!this.validateProductCoveragesNoOverlappingPairwise()) {
            return false;
        }

        return true;
    }

    private startOfDayMs(d: Date): number {
        const x = new Date(d);
        x.setHours(0, 0, 0, 0);
        return x.getTime();
    }

    /** Inclusive range overlap; null/undefined expire means open-ended. */
    private productCoverageDateRangesOverlap(
        effectA: Date,
        expireA: Date | null | undefined,
        effectB: Date,
        expireB: Date | null | undefined
    ): boolean {
        const startA = this.startOfDayMs(effectA);
        const endA = expireA ? this.startOfDayMs(expireA) : Number.MAX_SAFE_INTEGER;
        const startB = this.startOfDayMs(effectB);
        const endB = expireB ? this.startOfDayMs(expireB) : Number.MAX_SAFE_INTEGER;
        return startA <= endB && startB <= endA;
    }

    private getProductCoverageLabelForMessage(coverageId: string): string {
        const opt = this.coverageOptions.find(c => c.value === coverageId);
        return opt?.label ?? coverageId;
    }

    private productCoverageCandidateOverlapsExisting(
        coverageId: string,
        effectDate: Date,
        expireDate: Date | null | undefined,
        excludeIndex: number | null
    ): boolean {
        const cid = String(coverageId);
        for (let i = 0; i < this.productCoverages.length; i++) {
            if (excludeIndex !== null && i === excludeIndex) {
                continue;
            }
            const row = this.productCoverages[i];
            if (String(row.coverageId ?? '') !== cid || !row.effectDate) {
                continue;
            }
            const existingEffect = new Date(row.effectDate);
            const existingExpire = row.expireDate ? new Date(row.expireDate) : null;
            if (this.productCoverageDateRangesOverlap(effectDate, expireDate, existingEffect, existingExpire)) {
                return true;
            }
        }
        return false;
    }

    private validateProductCoveragesNoOverlappingPairwise(): boolean {
        for (let i = 0; i < this.productCoverages.length; i++) {
            const a = this.productCoverages[i];
            if (!a.coverageId || !a.effectDate) {
                continue;
            }
            for (let j = i + 1; j < this.productCoverages.length; j++) {
                const b = this.productCoverages[j];
                if (!b.coverageId || !b.effectDate) {
                    continue;
                }
                if (String(a.coverageId) !== String(b.coverageId)) {
                    continue;
                }
                if (a.id && b.id && a.id === b.id) {
                    continue;
                }
                const ae = new Date(a.effectDate);
                const ax = a.expireDate ? new Date(a.expireDate) : null;
                const be = new Date(b.effectDate);
                const bx = b.expireDate ? new Date(b.expireDate) : null;
                if (!this.productCoverageDateRangesOverlap(ae, ax, be, bx)) {
                    continue;
                }
                const label = this.getProductCoverageLabelForMessage(String(a.coverageId));
                let message = this.localizationService.localize('Product::ProProductCoverage:OverlappingDateRangeRows');
                message = message.replace('{0}', String(i + 1)).replace('{1}', String(j + 1)).replace('{2}', label);
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: message
                });
                return false;
            }
        }
        return true;
    }

    /**
     * Validate Table Rates tab
     */
    private validateTableRatesTab(): boolean {
        const today = new Date();
        today.setHours(0, 0, 0, 0);

        for (let i = 0; i < this.productTableRates.length; i++) {
            const tableRate = this.productTableRates[i];
            
            if (!tableRate.tableRateId) {
                let message = this.localizationService.localize('Product::ProProductTableRate:TableRateRequired');
                message = message.replace('{0}', String(i + 1));
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: message
                });
                return false;
            }

            if (!tableRate.effectDate) {
                let message = this.localizationService.localize('Product::ProProductTableRate:EffectDateRequired');
                message = message.replace('{0}', String(i + 1));
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: message
                });
                return false;
            }

            // Validate effect date >= current date (only for create mode, not edit mode)
            const effectDate = new Date(tableRate.effectDate);
            effectDate.setHours(0, 0, 0, 0);

            if (!this.isEditMode()) {
                if (effectDate < today) {
                    let message = this.localizationService.localize('Product::ProProductTableRate:EffectDateMustBeTodayOrLater');
                    message = message.replace('{0}', String(i + 1));
                    this.messageService.add({
                        severity: 'error',
                        summary: this.localizationService.localize('AbpUi::Error'),
                        detail: message
                    });
                    return false;
                }
            }

            // Validate expire date if provided
            if (tableRate.expireDate) {
                const expireDate = new Date(tableRate.expireDate);
                expireDate.setHours(0, 0, 0, 0);

                if (expireDate < today) {
                    let message = this.localizationService.localize('Product::ProProductTableRate:ExpireDateMustBeTodayOrLater');
                    message = message.replace('{0}', String(i + 1));
                    this.messageService.add({
                        severity: 'error',
                        summary: this.localizationService.localize('AbpUi::Error'),
                        detail: message
                    });
                    return false;
                }

                if (expireDate < effectDate) {
                    let message = this.localizationService.localize('Product::ProProductTableRate:ExpireDateMustBeAfterEffectDate');
                    message = message.replace('{0}', String(i + 1));
                    this.messageService.add({
                        severity: 'error',
                        summary: this.localizationService.localize('AbpUi::Error'),
                        detail: message
                    });
                    return false;
                }
            }
        }

        return true;
    }

    /**
     * Validate Rules tab
     */
    private validateRulesTab(): boolean {
        const today = new Date();
        today.setHours(0, 0, 0, 0);

        for (let i = 0; i < this.productRules.length; i++) {
            const rule = this.productRules[i];
            
            if (!rule.ruleTypeId) {
                let message = this.localizationService.localize('Product::ProRule:RuleTypeRequired');
                message = message.replace('{0}', String(i + 1));
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: message
                });
                return false;
            }

            if (!rule.code || rule.code.trim() === '') {
                let message = this.localizationService.localize('Product::ProRule:CodeRequired');
                message = message.replace('{0}', String(i + 1));
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: message
                });
                return false;
            }

            const ruleCodeRegex = /^[A-Z0-9_]+$/i;
            if (!ruleCodeRegex.test(rule.code.trim())) {
                let message = this.localizationService.localize('Product::ProRule:CodeFormatInvalidIndexed');
                message = message.replace('{0}', String(i + 1));
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: message
                });
                return false;
            }

            if (!rule.name || rule.name.trim() === '') {
                let message = this.localizationService.localize('Product::ProRule:NameRequired');
                message = message.replace('{0}', String(i + 1));
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: message
                });
                return false;
            }

            if (!rule.effectDate) {
                let message = this.localizationService.localize('Product::ProRule:EffectDateRequired');
                message = message.replace('{0}', String(i + 1));
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: message
                });
                return false;
            }

            // Validate effect date >= current date (only for create mode, not edit mode)
            const effectDate = new Date(rule.effectDate);
            if (!this.isEditMode()) {
                effectDate.setHours(0, 0, 0, 0);

                if (effectDate < today) {
                    let message = this.localizationService.localize('Product::ProRule:EffectDateMustBeTodayOrLater');
                    message = message.replace('{0}', String(i + 1));
                    this.messageService.add({
                        severity: 'error',
                        summary: this.localizationService.localize('AbpUi::Error'),
                        detail: message
                    });
                    return false;
                }
            }

            // Validate expire date if provided
            if (rule.expireDate) {
                const expireDate = new Date(rule.expireDate);
                expireDate.setHours(0, 0, 0, 0);

                if (expireDate < today) {
                    let message = this.localizationService.localize('Product::ProRule:ExpireDateMustBeTodayOrLater');
                    message = message.replace('{0}', String(i + 1));
                    this.messageService.add({
                        severity: 'error',
                        summary: this.localizationService.localize('AbpUi::Error'),
                        detail: message
                    });
                    return false;
                }

                if (expireDate < effectDate) {
                    let message = this.localizationService.localize('Product::ProRule:ExpireDateMustBeAfterEffectDate');
                    message = message.replace('{0}', String(i + 1));
                    this.messageService.add({
                        severity: 'error',
                        summary: this.localizationService.localize('AbpUi::Error'),
                        detail: message
                    });
                    return false;
                }
            }
        }

        return true;
    }

    /**
     * Validate Package Definition tab
     */
    private validatePackageDefinitionTab(): boolean {
        const codeSet = new Set<string>();

        for (let i = 0; i < this.productPlanDefinitions.length; i++) {
            const planDef = this.productPlanDefinitions[i];
            
            if (!planDef.planCode || planDef.planCode.trim() === '') {
                let message = this.localizationService.localize('Product::ProProductPlanDefinition:CodeRequired');
                message = message.replace('{0}', String(i + 1));
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: message
                });
                return false;
            }

            // Validate code format: A-Z, _, 0-9 only
            const codeRegex = /^[A-Z0-9_]+$/i;
            if (!codeRegex.test(planDef.planCode.trim())) {
                let message = this.localizationService.localize('Product::ProProductPlanDefinition:CodeFormatInvalid');
                message = message.replace('{0}', String(i + 1));
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: message
                });
                return false;
            }

            // Check for duplicate codes
            const codeUpper = planDef.planCode.trim().toUpperCase();
            if (codeSet.has(codeUpper)) {
                let message = this.localizationService.localize('Product::ProProductPlanDefinition:CodeDuplicate');
                message = message.replace('{0}', String(i + 1)).replace('{Code}', planDef.planCode);
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: message
                });
                return false;
            }
            codeSet.add(codeUpper);

            if (!planDef.planName || planDef.planName.trim() === '') {
                let message = this.localizationService.localize('Product::ProProductPlanDefinition:NameRequired');
                message = message.replace('{0}', String(i + 1));
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: message
                });
                return false;
            }

            if (!planDef.status === undefined || !planDef.status === null) {
                let message = this.localizationService.localize('Product::ProProductPlanDefinition:StatusRequired');
                message = message.replace('{0}', String(i + 1));
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: message
                });
                return false;
            }
        }

        return true;
    }

    /**
     * Recursively cleans empty string GUID fields by converting them to undefined.
     * This prevents backend deserialization errors when empty strings are sent for GUID fields.
     */
    private cleanEmptyGuids(obj: any): any {
        if (obj === null || obj === undefined) {
            return obj;
        }

        // Handle arrays
        if (Array.isArray(obj)) {
            return obj.map(item => this.cleanEmptyGuids(item));
        }

        // Handle objects
        if (typeof obj === 'object') {
            const cleaned: any = {};
            for (const key in obj) {
                if (obj.hasOwnProperty(key)) {
                    const value = obj[key];
                    
                    // List of GUID field names that should be cleaned
                    const guidFields = [
                        'id', 
                        'productId', 
                        'productCoverageId', 
                        'coverageId',
                        'parentId',
                        'tableRateId',
                        'channelId',
                        'appChannelId',
                        'employeeRoleId',
                        'attributeId',
                        'uomId',
                        'taxId',
                        'interactionCoverageId',
                        'productCoverageLevelId',
                        'coverageLevelTypeId',
                        'coverageLevelBasisId',
                        'lobId',
                        'productCategoryId',
                        'partnerId',
                        'productTypeId',
                        'currencyId',
                        'rootProductId',
                        'planDefinitionId'
                    ];

                    // If it's a GUID field, handle empty strings, null values, and temp IDs
                    if (guidFields.includes(key)) {
                        if (value === '' || value === null) {
                            cleaned[key] = undefined;
                        } else if (typeof value === 'string' && value.startsWith('temp-')) {
                            // Temp IDs cannot be converted to GUIDs, set to undefined
                            cleaned[key] = undefined;
                        } else {
                            // Recursively clean nested objects/arrays
                            cleaned[key] = this.cleanEmptyGuids(value);
                        }
                    } else {
                        // Recursively clean nested objects/arrays
                        cleaned[key] = this.cleanEmptyGuids(value);
                    }
                }
            }
            return cleaned;
        }

        // Return primitive values as-is
        return obj;
    }

    /**
     * Resolves temp IDs in productCoverageInteractions and coverage parentId by matching coverageId.
     * Temp IDs are in format "temp-{coverageId}" or "temp-coverageId-{coverageId}" and need to be
     * resolved to actual coverage IDs by finding the corresponding coverage in the productCoverages array.
     *
     * For new products, coverages don't have IDs yet, so we generate temporary UUIDs.
     * For updates, we can match by both coverageId and existing id.
     */
    private resolveTempIds(dto: CreateProProductDto | UpdateProProductDto): void {
        if (!dto.productCoverages || !Array.isArray(dto.productCoverages)) {
            return;
        }

        // Build a map of coverageId to actual coverage id for quick lookup
        // This works for updates where coverages already have IDs
        const coverageIdMap = new Map<string, string>();
        dto.productCoverages.forEach((coverage) => {
            if (coverage.coverageId && coverage.id) {
                const key = String(coverage.coverageId);
                coverageIdMap.set(key, coverage.id);
            }
        });

        // First pass: Generate temp UUIDs for all coverages that don't have IDs yet
        // This is needed so interactions can reference them
        dto.productCoverages.forEach(coverage => {
            if (!coverage.id) {
                // Generate a temporary UUID for the coverage so backend can match it
                coverage.id = crypto.randomUUID();
            }
        });

        // Second pass: Resolve temp IDs in productCoverageInteractions
        dto.productCoverages.forEach(coverage => {
            if (coverage.productCoverageInteractions && Array.isArray(coverage.productCoverageInteractions)) {
                coverage.productCoverageInteractions.forEach(interaction => {
                    // Set productCoverageId to the coverage's ID (temp UUID or actual ID)
                    if (coverage.id) {
                        interaction.productCoverageId = coverage.id;
                    }

                    // Resolve interactionCoverageId
                    if (interaction.interactionCoverageId) {
                        const interactionCoverageId = String(interaction.interactionCoverageId);
                        
                        // Check if it's a temp ID
                        if (interactionCoverageId.startsWith('temp-')) {
                            // Extract coverageId from temp ID (remove "temp-" prefix)
                            const coverageId = interactionCoverageId.substring(5);
                            
                            // First, try to find by existing coverage ID (for updates)
                            const actualCoverageId = coverageIdMap.get(coverageId);
                            
                            if (actualCoverageId) {
                                // Resolve to actual coverage ID
                                interaction.interactionCoverageId = actualCoverageId;
                            } else {
                                // For new products, find coverage by matching coverageId in the same request
                                const matchingCoverage = dto.productCoverages.find(c => 
                                    c.coverageId && String(c.coverageId) === coverageId
                                );
                                
                                if (matchingCoverage && matchingCoverage.id) {
                                    // Use the coverage's ID (temp UUID or actual ID)
                                    interaction.interactionCoverageId = matchingCoverage.id;
                                } else {
                                    // Cannot resolve, set to undefined (will be cleaned by cleanEmptyGuids)
                                    interaction.interactionCoverageId = undefined;
                                }
                            }
                        }
                    }
                });
            }
        });

        // Resolve temp parentId on coverages
        // For new products, generate temporary UUIDs so backend's Strategy 2 can match
        dto.productCoverages.forEach(coverage => {
            if (coverage.parentId) {
                const parentId = String(coverage.parentId);

                // Check if it's a temp ID (format: "temp-coverageId-{coverageId}")
                if (parentId.startsWith('temp-coverageId-')) {
                    // Extract coverageId from temp ID
                    const parentCoverageId = parentId.substring('temp-coverageId-'.length);

                    // Find the parent coverage by coverageId
                    const parentCoverage = dto.productCoverages.find(c =>
                        c.coverageId && String(c.coverageId) === parentCoverageId && c !== coverage
                    );

                    if (parentCoverage) {
                        if (parentCoverage.id) {
                            // Parent already has an ID (update scenario)
                            coverage.parentId = parentCoverage.id;
                        } else {
                            // Parent doesn't have an ID yet (create scenario)
                            // Generate a temporary UUID for the parent so backend can match
                            const tempUuid = crypto.randomUUID();
                            parentCoverage.id = tempUuid;
                            coverage.parentId = tempUuid;
                        }
                    } else {
                        // Parent not found, set to undefined
                        coverage.parentId = undefined;
                    }
                } else if (parentId.startsWith('temp-')) {
                    // Handle legacy format "temp-{coverageId}"
                    const parentCoverageId = parentId.substring(5);

                    const parentCoverage = dto.productCoverages.find(c =>
                        c.coverageId && String(c.coverageId) === parentCoverageId && c !== coverage
                    );

                    if (parentCoverage) {
                        if (parentCoverage.id) {
                            coverage.parentId = parentCoverage.id;
                        } else {
                            const tempUuid = crypto.randomUUID();
                            parentCoverage.id = tempUuid;
                            coverage.parentId = tempUuid;
                        }
                    } else {
                        coverage.parentId = undefined;
                    }
                }
            }
        });
    }

    private buildProRules(): CreateProRuleDto[] {
        // Only include rules without IDs (new rules)
        return this.productRules
            .filter(rule => !rule.id)
            .map(rule => {
                const ruleDto: CreateProRuleDto = {
                    applyTo: 'product',
                    ruleTypeId: rule.ruleTypeId!,
                    code: rule.code!,
                    name: rule.name!,
                    description: rule.description || undefined,
                    ruleScript: rule.ruleScript!,
                    priority: rule.priority || 1,
                    status: rule.status ?? ProRuleStatus.Active,
                    effectDate: rule.effectDate!,
                    expireDate: rule.expireDate || undefined
                };
                // applyToId is optional and will be set by backend
                return ruleDto;
            });
    }

    private buildProductPlanDefinitions(): CreateProProductPlanDefinitionDto[] {
        // Only include plan definitions without IDs (new plans)
        return this.productPlanDefinitions
            .filter(plan => !plan.id)
            .map(plan => ({
                productId: '', // Will be set by backend
                planCode: (plan.planCode || '').trim().toUpperCase(),
                planName: (plan.planName || '').trim(),
                status: plan.status || ProProductPlanDefinitionStatus.Active
            }));
    }

    private buildProductCoverages(): ProProductCoverageDto[] {
        // Ensure each coverage has productCoverageLevels and productCoverageInteractions initialized
        return this.productCoverages.map(coverage => ({
            ...coverage,
            productCoverageLevels: coverage.productCoverageLevels || [],
            productCoverageInteractions: coverage.productCoverageInteractions || []
        }));
    }

    private create(): void {
        this.loading = true;

        const createDto: CreateProProductDto = {
            lobId: this.formData.lobId,
            productCategoryId: this.formData.productCategoryId || undefined,
            partnerId: this.formData.partnerId || undefined,
            productTypeId: this.formData.productTypeId || undefined,
            currencyId: this.formData.currencyId || undefined,
            rootProductId: this.formData.rootProductId || undefined,
            isRootProduct: this.formData.rootProductId ? 'N' : 'Y',
            code: this.formData.code.trim().toUpperCase(),
            insurerProductCode: this.formData.insurerProductCode || undefined,
            shortName: this.formData.shortName.trim(),
            name: this.formData.name.trim(),
            description: this.formData.description || undefined,
            internalNote: this.formData.internalNote || undefined,
            rateType: this.formData.rateType,
            status: this.formData.status,
            seqNumber: this.formData.seqNumber,
            effectDate: this.toLocalIsoDateOrPass(this.formData.effectDate) ?? '',
            expireDate: this.formData.expireDate
                ? (this.toLocalIsoDateOrPass(this.formData.expireDate) ?? undefined)
                : undefined,
            isPlan: this.formData.isPlan || undefined,
            planDefinitionId: this.formData.planDefinitionId || undefined,
            imageDocumentId: this.formData.imageDocumentId || undefined,
            certificateTemplateDocumentId: this.formData.certificateTemplateDocumentId || undefined,
            productAttributes: this.productAttributes,
            productCoverages: this.buildProductCoverages(),
            productTableRates: this.productTableRates.map(tr => ({
                ...tr,
                effectDate: this.toLocalIsoDateAtMidnight(tr.effectDate ?? null) ?? tr.effectDate
            })),
            proRules: this.buildProRules(),
            productPlanDefinitions: this.buildProductPlanDefinitions(),
            productDistributions: this.productDistributions.map(d => ({
                id: d.id,
                productId: d.productId ?? '',
                channelId: d.channelId,
                appChannelId: d.appChannelId,
                employeeRoleId: d.employeeRoleId,
                status: (d.status ?? ProProductDistributionStatus.Active) as ProProductDistributionStatus
            }))
        };

        // Resolve temp IDs first, then clean empty GUID strings before sending to API
        this.resolveTempIds(createDto);
        const cleanedDto = this.cleanEmptyGuids(createDto) as CreateProProductDto;

        this.productService.create(cleanedDto).subscribe({
            next: (createdProduct) => {
                this.loading = false; // Fix: Reset loading state
                this.messageService.add({
                    severity: 'success',
                    summary: this.localizationService.localize('AbpUi::Success'),
                    detail: this.localizationService.localize('Product::ProProduct:Created')
                });
                // Refresh data
                this.loadTreeData();
                this.search();
                
                // Fix: Reset to no selection state instead of selecting the created product
                this.isInCreateMode = false;
                this.selectedProduct = null;
                this.formData = this.getEmptyForm();
                this.uploadedImageFile = null;
                this.productImageUrl = null;
                this.imageUploadLoading = false;
                this.uploadedCertificateTemplateFile = null;
                this.certificateTemplateDocumentUrl = null;
                this.certificateTemplateFileName = null;
                this.certificateTemplateUploadLoading = false;
                this.productAttributes = [];
                this.productCoverages = [];
                this.productTableRates = [];
                this.productRules = [];
                this.productPlanDefinitions = [];
                this.resetProductDistributionState();
            },
            error: (err) => {
                this.loading = false;
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: err.error?.error?.message || 'Error creating product'
                });
            }
        });
    }

    private update(): void {
        if (!this.selectedProduct?.id) return;

        this.loading = true;

        const updateDto: UpdateProProductDto = {
            lobId: this.formData.lobId,
            productCategoryId: this.formData.productCategoryId || undefined,
            partnerId: this.formData.partnerId || undefined,
            productTypeId: this.formData.productTypeId || undefined,
            currencyId: this.formData.currencyId || undefined,
            rootProductId: this.formData.rootProductId || undefined,
            isRootProduct: this.formData.rootProductId ? 'N' : 'Y',
            insurerProductCode: this.formData.insurerProductCode || undefined,
            shortName: this.formData.shortName.trim(),
            name: this.formData.name.trim(),
            description: this.formData.description || undefined,
            internalNote: this.formData.internalNote || undefined,
            rateType: this.formData.rateType,
            status: this.formData.status,
            seqNumber: this.formData.seqNumber,
            effectDate: this.toLocalIsoDateOrPass(this.formData.effectDate) ?? '',
            expireDate: this.formData.expireDate
                ? (this.toLocalIsoDateOrPass(this.formData.expireDate) ?? undefined)
                : undefined,
            isPlan: this.formData.isPlan || undefined,
            planDefinitionId: this.formData.planDefinitionId || undefined,
            imageDocumentId: this.formData.imageDocumentId || undefined,
            certificateTemplateDocumentId: this.formData.certificateTemplateDocumentId || undefined,
            productAttributes: this.productAttributes,
            productCoverages: this.buildProductCoverages(),
            productTableRates: this.productTableRates.map(tr => ({
                ...tr,
                effectDate: this.toLocalIsoDateAtMidnight(tr.effectDate ?? null) ?? tr.effectDate
            })),
            proRules: this.productRules,
            productPlanDefinitions: this.productPlanDefinitions,
            productDistributions: this.productDistributions
        };

        // Resolve temp IDs first, then clean empty GUID strings before sending to API
        this.resolveTempIds(updateDto);
        const cleanedDto = this.cleanEmptyGuids(updateDto) as UpdateProProductDto;

        this.productService.update(this.selectedProduct.id, cleanedDto).subscribe({
            next: (updatedProduct) => {
                this.loading = false; // Fix: Reset loading state
                this.messageService.add({
                    severity: 'success',
                    summary: this.localizationService.localize('AbpUi::Success'),
                    detail: this.localizationService.localize('Product::ProProduct:Updated')
                });
                // Refresh data
                this.loadTreeData();
                this.search();
                
                // Fix: Reset to no selection state instead of selecting the updated product
                this.isInCreateMode = false;
                this.selectedProduct = null;
                this.formData = this.getEmptyForm();
                this.uploadedImageFile = null;
                this.productImageUrl = null;
                this.imageUploadLoading = false;
                this.uploadedCertificateTemplateFile = null;
                this.certificateTemplateDocumentUrl = null;
                this.certificateTemplateFileName = null;
                this.certificateTemplateUploadLoading = false;
                this.productAttributes = [];
                this.productCoverages = [];
                this.productTableRates = [];
                this.productRules = [];
                this.productPlanDefinitions = [];
                this.resetProductDistributionState();
            },
            error: (err) => {
                this.loading = false;
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: err.error?.error?.message || 'Error updating product'
                });
            }
        });
    }

    private getEmptyForm(): ProductFormData {
        const tomorrow = new Date();
        tomorrow.setDate(tomorrow.getDate() + 1);

        return {
            lobId: '',
            productCategoryId: null,
            partnerId: null,
            productTypeId: null,
            currencyId: null,
            rootProductId: null,
            code: '',
            insurerProductCode: null,
            shortName: '',
            name: '',
            description: null,
            internalNote: null,
            rateType: 'table_rate',
            status: ProProductStatus.Deactive,
            seqNumber: 1,
            effectDate: tomorrow,
            expireDate: null,
            isPlan: 'N',
            planDefinitionId: null,
            isRootProduct: 'Y',
            imageDocumentId: null,
            certificateTemplateDocumentId: null
        };
    }

    /**
     * Set default currency to VND if available
     */
    private setDefaultVndCurrency(): void {
        if (this.currencyOptions.length === 0) {
            return; // Currencies not loaded yet
        }
        
        const vndCurrency = this.currencyOptions.find(c => 
            (c.code || '').toUpperCase() === 'VND'
        );
        
        if (vndCurrency) {
            this.formData.currencyId = vndCurrency.value;
        }
    }

    formatStatus(status: ProProductStatus): string {
        const option = this.statusOptions.find(o => o.value === status);
        return option?.label || '';
    }

    getLobName(lobId: string | undefined): string {
        if (!lobId) return '-';
        const option = this.lobOptions.find(o => o.value === lobId);
        return option?.label || '-';
    }

    getCategoryName(categoryId: string | undefined): string {
        if (!categoryId) return '-';
        const option = this.categoryOptions.find(o => o.value === categoryId);
        return option?.label || '-';
    }

    getPartnerName(partnerId: string | undefined): string {
        if (!partnerId) return '-';
        const option = this.partnerOptions.find(o => o.value === partnerId);
        return option?.label || '-';
    }

    getRateTypeName(rateType: string | undefined): string {
        if (!rateType) return '-';
        const option = this.rateTypeOptions.find(o => o.value === rateType);
        return option?.label || rateType;
    }

    // Attribute Tab Methods
    loadAttributeOptions(): void {
        this.attributeService.getList({
            maxResultCount: 1000,
            status: ProAttributeStatus.Active
        }).subscribe({
            next: (result) => {
                this.attributeOptions = (result.items || []).map(attr => ({
                    label: attr.name || '',
                    value: attr.id || '',
                    code: attr.code || ''
                }));
            },
            error: (err) => {
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: err.error?.error?.message || 'Error loading attributes'
                });
            }
        });
    }

    onShowAttributeForm(): void {
        this.attributeFormVisible = true;
        this.selectedAttributeId = null;
        this.selectedAttributeIsRequired = 'N';
    }

    onCancelAttributeForm(): void {
        this.confirmWithDefaults({
            message: this.localizationService.localize('Product::ProProduct:CancelAttributeConfirm'),
            header: this.localizationService.localize('Product::Confirm'),
            icon: 'pi pi-exclamation-triangle',
            accept: () => {
                this.attributeFormVisible = false;
                this.selectedAttributeId = null;
                this.selectedAttributeIsRequired = 'N';
            }
        });
    }

    onSaveAttribute(): void {
        // Validation: Check if we're in create mode or have a selected product
        // Allow adding attributes when creating a new product (selectedProduct is null but isInCreateMode is true)
        const isCreatingNewProduct = this.isCreateMode();
        const hasExistingProduct = this.selectedProduct?.id;

        if (!isCreatingNewProduct && !hasExistingProduct) {
            this.messageService.add({
                severity: 'warn',
                summary: this.localizationService.localize('AbpUi::Warning'),
                detail: this.localizationService.localize('Product::ProProduct:SelectProductFirst', 'Please select a product first')
            });
            return;
        }

        // Validation: Check if attribute is selected
        if (!this.selectedAttributeId) {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Product::ProProduct:SelectAttributeRequired', 'Please select an attribute')
            });
            return;
        }

        // Check for duplicate attribute
        const existingAttribute = this.productAttributes.find(
            attr => attr.attributeId === this.selectedAttributeId
        );
        if (existingAttribute) {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Product::ProProduct:AttributeAlreadyExists', 'This attribute has already been added')
            });
            return;
        }

        // Proxies may temporarily require nested `attribute` depending on OpenAPI/proxy version.
        // Use a safe cast here so UI can add attributes without providing joined data.
        const newAttribute = ({
            productId: hasExistingProduct ? this.selectedProduct!.id : undefined,
            attributeId: this.selectedAttributeId,
            isRequired: this.selectedAttributeIsRequired,
            status: 'Active'
        } as any) as ProProductAttributeDto;

        // Add to productAttributes array (will be saved with product)
        // Create new array reference to ensure change detection
        this.productAttributes = [...this.productAttributes, newAttribute];

        this.messageService.add({
            severity: 'success',
            summary: this.localizationService.localize('AbpUi::Success'),
            detail: this.localizationService.localize('Product::ProProduct:AttributeAdded', 'Attribute added successfully')
        });

        this.attributeFormVisible = false;
        this.selectedAttributeId = null;
        this.selectedAttributeIsRequired = 'N';
    }

    onDeleteAttribute(attribute: ProProductAttributeDto, index: number): void {
        // Check if deletion is allowed
        if (this.selectedProduct?.status !== ProProductStatus.Deactive &&
            this.selectedProduct?.effectDate &&
            new Date(this.selectedProduct.effectDate) > new Date()) {
            this.messageService.add({
                severity: 'warn',
                summary: this.localizationService.localize('AbpUi::Warning'),
                detail: this.localizationService.localize('ProProduct:CannotDeleteAttribute')
            });
            return;
        }

        this.confirmWithDefaults({
            message: this.localizationService.localize('ProProduct:DeleteAttributeConfirm'),
            header: this.localizationService.localize('Product::Delete'),
            icon: 'pi pi-exclamation-triangle',
            accept: () => {
                this.productAttributes.splice(index, 1);
                this.messageService.add({
                    severity: 'success',
                    summary: this.localizationService.localize('AbpUi::Success'),
                    detail: this.localizationService.localize('ProProduct:AttributeDeleted')
                });
            }
        });
    }

    getAttributeName(attributeId: string | undefined): string {
        if (!attributeId) return '-';
        const attr = this.attributeOptions.find(a => a.value === attributeId);
        return attr?.label || '-';
    }

    getAttributeCode(attributeId: string | undefined): string {
        if (!attributeId) return '-';
        const attr = this.attributeOptions.find(a => a.value === attributeId);
        return attr?.code || '-';
    }

    /**
     * Getter for isPlan as boolean (for checkbox binding)
     */
    get isPlanChecked(): boolean {
        return this.formData.isPlan === 'Y';
    }

    /**
     * Setter for isPlan checkbox
     * Workaround for PrimeNG checkbox binary=false issue
     */
    set isPlanChecked(value: boolean) {
        this.formData.isPlan = value ? 'Y' : 'N';
    }

    /**
     * Getter for selectedAttributeIsRequired as boolean (for checkbox binding)
     */
    get attributeIsRequiredChecked(): boolean {
        return this.selectedAttributeIsRequired === 'Y';
    }

    /**
     * Setter for selectedAttributeIsRequired checkbox
     * Workaround for PrimeNG checkbox binary=false issue
     */
    set attributeIsRequiredChecked(value: boolean) {
        this.selectedAttributeIsRequired = value ? 'Y' : 'N';
    }

    // Coverage Tab Methods
    loadCoverageOptions(): void {
        this.coverageService.getList({
            maxResultCount: 1000,
            status: ProCoverageStatus.Active
        }).subscribe({
            next: (result) => {
                this.coverageOptions = (result.items || []).map(cov => ({
                    label: cov.name || '',
                    value: cov.id || '',
                    code: cov.code || '',
                    coverageGroupId: cov.coverageGroupId,
                    type: cov.type
                }));
            },
            error: (err) => {
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: err.error?.error?.message || 'Error loading coverages'
                });
            }
        });
    }

    loadUomOptions(): void {
        this.uomService.getList({
            skipCount: 0,
            maxResultCount: 1000,
            status: ResUomStatus.Active,
            sorting: 'name asc'
        }).subscribe({
            next: (result) => {
                this.uomOptions = (result.items || []).map(uom => ({
                    label: `${uom.code || ''} - ${uom.name || ''}`,
                    value: uom.id || ''
                }));
            },
            error: (err) => {
                console.error('Error loading UOMs:', err);
                this.uomOptions = [];
            }
        });
    }

    loadTaxOptions(): void {
        this.taxService.getList({
            skipCount: 0,
            maxResultCount: 1000,
            status: ResTaxStatus.Active,
            sorting: 'name asc'
        }).subscribe({
            next: (result) => {
                const items = (result.items || []) as any as ResTaxDto[];
                this.taxById = new Map(items.filter(t => !!t.id).map(t => [t.id as string, t]));
                this.taxOptions = (result.items || []).map(tax => ({
                    label: `${tax.code || ''} - ${tax.name || ''}`,
                    value: tax.id || ''
                }));
            },
            error: (err) => {
                console.error('Error loading taxes:', err);
                this.taxById = new Map();
                this.taxOptions = [];
            }
        });
    }

    private getTaxDto(taxId: string | null | undefined): ResTaxDto | undefined {
        if (!taxId) return undefined;
        return this.taxById.get(taxId);
    }

    loadCoverageGroupOptions(): void {
        this.coverageGroupService.getList({
            skipCount: 0,
            maxResultCount: 1000,
            status: ProCoverageGroupStatus.Active,
            sorting: 'name asc'
        }).subscribe({
            next: (result) => {
                this.coverageGroupOptions = (result.items || []).map(group => ({
                    label: group.name || '',
                    value: group.id || ''
                }));
            },
            error: (err) => {
                console.error('Error loading coverage groups:', err);
                this.coverageGroupOptions = [];
            }
        });
    }

    onCoverageChange(): void {
        // Refresh parent coverage options when coverage selection changes
        this.updateParentCoverageOptions();
    }

    onCoverageGroupChange(): void {
        // Clear selected coverage if it doesn't belong to the new group
        if (this.selectedCoverageId) {
            const selectedCoverage = this.coverageOptions.find(c => c.value === this.selectedCoverageId);
            if (selectedCoverage?.coverageGroupId !== this.selectedCoverageGroupId) {
                this.selectedCoverageId = null;
            }
        }
        // Refresh parent coverage options
        this.updateParentCoverageOptions();
    }

    buildCoverageTree(): void {
        const map = new Map<string, TreeNode<ProProductCoverageDto>>();
        const coverageIdMap = new Map<string, TreeNode<ProProductCoverageDto>>(); // Map by coverageId for unsaved parents
        const roots: TreeNode<ProProductCoverageDto>[] = [];

        // First pass: create all nodes with temporary IDs for new coverages
        this.productCoverages.forEach((coverage, index) => {
            const node: TreeNode<ProProductCoverageDto> = {
                data: coverage,
                children: [],
                expanded: true // Start expanded to show hierarchy
            };
            // Use actual ID if exists, otherwise generate temporary ID
            const nodeId = coverage.id || `temp-${index}-${coverage.coverageId || 'new'}`;
            map.set(nodeId, node);

            // Also map by coverageId for looking up unsaved parents
            if (coverage.coverageId) {
                coverageIdMap.set(coverage.coverageId, node);
            }
        });

        // Second pass: build hierarchy
        this.productCoverages.forEach((coverage, index) => {
            // Get the node ID (actual or temporary)
            const nodeId = coverage.id || `temp-${index}-${coverage.coverageId || 'new'}`;
            const node = map.get(nodeId);
            if (!node) return;

            if (coverage.parentId) {
                // Try to find parent by ID first (works for saved coverages)
                let parent = map.get(coverage.parentId);

                // If not found, check if parentId is a temp coverageId reference (format: "temp-coverageId-{coverageId}")
                if (!parent && coverage.parentId.startsWith('temp-coverageId-')) {
                    const parentCoverageId = coverage.parentId.replace('temp-coverageId-', '');
                    parent = coverageIdMap.get(parentCoverageId);
                }

                if (parent) {
                    parent.children!.push(node);
                } else {
                    roots.push(node); // Parent not found, treat as root
                }
            } else {
                roots.push(node); // No parent, is root
            }
        });

        this.coverageTreeData = roots;
    }

    onShowCoverageForm(): void {
        this.coverageFormVisible = true;
        this.selectedCoverageForEdit = null;
        // Reset form fields
        this.selectedCoverageId = null;
        this.selectedParentCoverageId = null;
        this.selectedCoverageGroupId = null;
        this.selectedUomId = null;
        this.selectedTaxId = null;
        this.selectedAvailabilityType = null;
        this.selectedExpireDate = null;
        this.selectedSeqNumber = this.productCoverages.length + 1;
        this.selectedInsurerCoverageCode = '';
        this.selectedEnableQuantity = false;
        
        this.selectedEffectDate = new Date();
        
        // Update parent coverage options from active ProCoverages
        this.updateParentCoverageOptions();
    }

    /**
     * Check if coverage form is valid (all required fields filled)
     */
    get isCoverageFormValid(): boolean {
        // Use != null to check for null/undefined (not truthy check, since 0 is a valid enum value)
        return !!(
            this.selectedCoverageId &&
            this.selectedUomId &&
            this.selectedTaxId &&
            this.selectedAvailabilityType != null &&  // != null checks for both null and undefined, allows 0
            this.selectedEffectDate
        );
    }

    /**
     * True when the 8 coverage scope fields (Coverage Group, Coverage, Parent Coverage,
     * Insurer Coverage Code, Uom, Tax, Availability Type, Effect Date) should be disabled.
     * Locked when editing an existing coverage, product is not draft/deactive, and current date >= coverage effectDate.
     */
    get isCoverageScopeFieldsLocked(): boolean {
        if (this.isEditingProductDraftOrDeactive()) return false;
        if (!this.selectedCoverageForEdit) return false; // adding new coverage: allow all
        const effectDate = this.selectedCoverageForEdit.effectDate
            ? new Date(this.selectedCoverageForEdit.effectDate)
            : null;
        if (!effectDate) return false;
        const today = new Date();
        today.setHours(0, 0, 0, 0);
        effectDate.setHours(0, 0, 0, 0);
        return effectDate <= today;
    }

    /**
     * True when the constraint modal fields (Interaction Type, Related Coverage, Effect Date) should be disabled.
     * Locked when product is not draft and current date >= product effectDate.
     */
    get isInteractionConstraintFieldsLocked(): boolean {
        if (this.isDetailMode()) return true;
        // When adding a new constraint (form visible, no existing row selected), allow editing
        if (this.interactionFormVisible && !this.selectedInteractionForEdit) return false;
        const product = this.selectedProduct ?? (this.formData as { status?: ProProductStatus; effectDate?: Date | string } | null);
        const status = product?.status;
        if (this.isProductDraftOrDeactive(status)) return false;
        const effectDate = product?.effectDate;
        if (!effectDate) return true; // product considered active when not draft and no effectDate
        const today = new Date();
        today.setHours(0, 0, 0, 0);
        const productEffectDate = new Date(effectDate);
        productEffectDate.setHours(0, 0, 0, 0);
        return today >= productEffectDate;
    }

    /** Draft and Deactive products allow full edits in coverage interaction / level / term modals like draft. */
    private isProductDraftOrDeactive(status: ProProductStatus | undefined): boolean {
        return status === ProProductStatus.Draft || status === ProProductStatus.Deactive;
    }

    /** True when either persisted status or current form status is Draft/Deactive during edit flows. */
    private isEditingProductDraftOrDeactive(): boolean {
        return this.isProductDraftOrDeactive(this.selectedProduct?.status) ||
            this.isProductDraftOrDeactive(this.formData?.status);
    }

    /**
     * True when product is active for restriction purposes: not draft and (current date >= effectDate or no effectDate).
     * Used to disable specific fields and block delete on Bảng phí and Quy tắc khác when product is already active.
     */
    private isProductActiveForRestriction(): boolean {
        const product = this.selectedProduct;
        if (!product) return false;
        if (this.isEditingProductDraftOrDeactive()) return false;
        const effectDate = product.effectDate;
        if (!effectDate) return true; // product considered active when not draft and no effectDate
        const today = new Date();
        today.setHours(0, 0, 0, 0);
        const productEffectDate = new Date(effectDate);
        productEffectDate.setHours(0, 0, 0, 0);
        return today >= productEffectDate;
    }

    /**
     * Update parent coverage options from product coverages list
     * Uses coverages already added to the current product, as per DDD requirement
     * Excludes the currently selected/edited coverage to prevent circular references
     */
    private updateParentCoverageOptions(): void {
        // Use productCoverages (coverages already added to the product) instead of coverageOptions
        let filteredOptions = this.productCoverages;

        // Exclude the currently selected/edited coverage to prevent circular references
        if (this.selectedCoverageForEdit) {
            // When editing, exclude the coverage being edited
            if (this.selectedCoverageForEdit.id) {
                filteredOptions = filteredOptions.filter(cov => cov.id !== this.selectedCoverageForEdit!.id);
            } else {
                // For new coverages without id, exclude by coverageId
                filteredOptions = filteredOptions.filter(cov => 
                    cov.coverageId !== this.selectedCoverageForEdit!.coverageId || cov.id
                );
            }
        } else if (this.selectedCoverageId) {
            // When creating, exclude if the same coverage is already in the list
            filteredOptions = filteredOptions.filter(cov => cov.coverageId !== this.selectedCoverageId);
        }

        // Map to the format expected by the dropdown
        this.parentCoverageOptions = filteredOptions.map(productCov => {
            const coverageName = this.getCoverageName(productCov.coverageId);
            // Use id if available, otherwise use a temp identifier
            const value = productCov.id || `temp-${productCov.coverageId}`;
            return {
                label: coverageName,
                value: value
            };
        });
    }

    onCancelCoverageForm(): void {
        this.confirmWithDefaults({
            message: this.localizationService.localize('Product::ProProduct:CancelCoverageConfirm'),
            header: this.localizationService.localize('Product::Confirm'),
            icon: 'pi pi-exclamation-triangle',
            accept: () => {
                this.coverageFormVisible = false;
                this.selectedCoverageForEdit = null;
                this.selectedCoverageId = null;
                this.selectedParentCoverageId = null;
                this.selectedCoverageGroupId = null;
                this.selectedUomId = null;
                this.selectedTaxId = null;
                this.selectedAvailabilityType = null;
                this.selectedEffectDate = null;
                this.selectedExpireDate = null;
                this.selectedSeqNumber = 1;
                this.selectedInsurerCoverageCode = '';
                this.selectedEnableQuantity = false;
            }
        });
    }

    onSaveCoverage(): void {
        // Validation
        if (!this.selectedCoverageId) {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Product::ProProduct:SelectCoverage')
            });
            return;
        }

        if (!this.selectedUomId) {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Product::ProProduct:UomRequired')
            });
            return;
        }

        if (!this.selectedTaxId) {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Product::ProProduct:TaxRequired')
            });
            return;
        }

        if (this.selectedAvailabilityType == null) {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Product::ProProduct:AvailabilityTypeRequired')
            });
            return;
        }

        if (!this.selectedEffectDate) {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Product::ProProduct:EffectDateRequired')
            });
            return;
        }

        // Resolve parent ID - handle temp IDs for newly added coverages
        let resolvedParentId: string | undefined = undefined;
        if (this.selectedParentCoverageId) {
            // Check if it's a temp ID (starts with "temp-")
            if (this.selectedParentCoverageId.startsWith('temp-')) {
                // Extract coverageId from temp ID (format: "temp-{coverageId}")
                const parentCoverageId = this.selectedParentCoverageId.replace('temp-', '');
                // Find the parent coverage by coverageId
                const parentCoverage = this.productCoverages.find(c =>
                    c.coverageId === parentCoverageId
                );
                // Use parent's id if available, otherwise use temp ID format for unsaved parent
                // This allows buildCoverageTree to link children to unsaved parents
                resolvedParentId = parentCoverage?.id || `temp-coverageId-${parentCoverageId}`;
            } else {
                // It's a regular ID, use it directly
                resolvedParentId = this.selectedParentCoverageId;
            }
        }

        if (this.selectedCoverageForEdit) {
            // Editing existing coverage
            // Find the coverage by id if it exists, otherwise find by original coverageId
            let coverageIndex = -1;
            if (this.selectedCoverageForEdit.id) {
                coverageIndex = this.productCoverages.findIndex(c => c.id === this.selectedCoverageForEdit!.id);
            } else {
                // For new coverages without id, find by original coverageId
                coverageIndex = this.productCoverages.findIndex(c => 
                    c.coverageId === this.selectedCoverageForEdit!.coverageId &&
                    !c.id
                );
            }
            
            if (coverageIndex >= 0) {
                if (
                    this.productCoverageCandidateOverlapsExisting(
                        this.selectedCoverageId,
                        this.selectedEffectDate,
                        this.selectedExpireDate,
                        coverageIndex
                    )
                ) {
                    const label = this.getProductCoverageLabelForMessage(this.selectedCoverageId);
                    let message = this.localizationService.localize('Product::ProProductCoverage:OverlappingDateRangeSameCoverage');
                    message = message.replace('{0}', label);
                    this.messageService.add({
                        severity: 'error',
                        summary: this.localizationService.localize('AbpUi::Error'),
                        detail: message
                    });
                    return;
                }

                // Update existing coverage, preserve id and nested data
                // Note: coverage is optional in backend (ProCoverageDto?) but required in proxy-generated TypeScript interface
                // Using type assertion to work around proxy generation issue
                this.productCoverages[coverageIndex] = {
                    ...this.productCoverages[coverageIndex],
                    id: this.selectedCoverageForEdit.id,
                    coverageId: this.selectedCoverageId,
                    parentId: resolvedParentId,
                    insurerCoverageCode: this.selectedInsurerCoverageCode || undefined,
                    uomId: this.selectedUomId,
                    taxId: this.selectedTaxId,
                    tax: this.getTaxDto(this.selectedTaxId) || (this.selectedCoverageForEdit as any).tax,
                    availabilityType: this.selectedAvailabilityType,
                    effectDate: this.toLocalIsoDateOrPass(this.selectedEffectDate),
                    expireDate: this.selectedExpireDate
                        ? this.toLocalIsoDateOrPass(this.selectedExpireDate)
                        : undefined,
                    seqNumber: this.selectedSeqNumber,
                    enableQuantity: this.selectedEnableQuantity ? 'Y' : 'N',
                    // Preserve nested data
                    productCoverageInteractions: this.selectedCoverageForEdit.productCoverageInteractions || [],
                    productCoverageLevels: this.selectedCoverageForEdit.productCoverageLevels || [],
                    coverage: (this.selectedCoverageForEdit.coverage || undefined) as any
                } as ProProductCoverageDto;
                
                this.messageService.add({
                    severity: 'success',
                    summary: this.localizationService.localize('AbpUi::Success'),
                    detail: this.localizationService.localize('Product::ProProduct:CoverageUpdated')
                });
            } else {
                // Coverage not found, show error
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: this.localizationService.localize('Product::ProProduct:CoverageNotFound')
                });
                return;
            }
        } else {
            if (
                this.productCoverageCandidateOverlapsExisting(
                    this.selectedCoverageId,
                    this.selectedEffectDate,
                    this.selectedExpireDate,
                    null
                )
            ) {
                const label = this.getProductCoverageLabelForMessage(this.selectedCoverageId);
                let message = this.localizationService.localize('Product::ProProductCoverage:OverlappingDateRangeSameCoverage');
                message = message.replace('{0}', label);
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: message
                });
                return;
            }

            // Creating new coverage
            // Note: coverage is optional in backend (ProCoverageDto?) but required in proxy-generated TypeScript interface
            // Using type assertion to work around proxy generation issue
            const newCoverage = {
                coverageId: this.selectedCoverageId,
                parentId: resolvedParentId,
                insurerCoverageCode: this.selectedInsurerCoverageCode || undefined,
                uomId: this.selectedUomId,
                taxId: this.selectedTaxId,
                tax: this.getTaxDto(this.selectedTaxId) || ({
                    id: this.selectedTaxId || undefined,
                    code: '',
                    name: '',
                    value: 0,
                    status: ResTaxStatus.Active
                } as any),
                availabilityType: this.selectedAvailabilityType,
                effectDate: this.toLocalIsoDateOrPass(this.selectedEffectDate),
                expireDate: this.selectedExpireDate
                    ? this.toLocalIsoDateOrPass(this.selectedExpireDate)
                    : undefined,
                seqNumber: this.selectedSeqNumber,
                enableQuantity: this.selectedEnableQuantity ? 'Y' : 'N',
                productCoverageInteractions: [],
                productCoverageLevels: [],
                coverage: undefined as any
            } as ProProductCoverageDto;

            this.productCoverages.push(newCoverage);
            
            this.messageService.add({
                severity: 'success',
                summary: this.localizationService.localize('AbpUi::Success'),
                detail: this.localizationService.localize('Product::ProProduct:CoverageAdded')
            });
        }

        this.buildCoverageTree();

        // Reset form
        this.coverageFormVisible = false;
        this.selectedCoverageForEdit = null;
        this.selectedCoverageId = null;
        this.selectedParentCoverageId = null;
        this.selectedCoverageGroupId = null;
        this.selectedUomId = null;
        this.selectedTaxId = null;
        this.selectedAvailabilityType = null;
        this.selectedEffectDate = null;
        this.selectedExpireDate = null;
        this.selectedSeqNumber = 1;
        this.selectedInsurerCoverageCode = '';
        this.selectedEnableQuantity = false;
    }

    onEditCoverage(coverage: ProProductCoverageDto): void {
        // Store the coverage being edited (preserve id and nested data)
        this.selectedCoverageForEdit = { ...coverage };
        
        // Populate form fields with coverage data
        this.selectedCoverageId = coverage.coverageId ?? null;
        this.selectedParentCoverageId = coverage.parentId || null;
        
        // Get coverage group from coverageOptions based on coverageId
        const selectedCoverage = this.coverageOptions.find(c => c.value === coverage.coverageId);
        this.selectedCoverageGroupId = selectedCoverage?.coverageGroupId || null;
        
        this.selectedUomId = coverage.uomId || null;
        this.selectedTaxId = coverage.taxId || null;
        this.selectedAvailabilityType = coverage.availabilityType ?? null;
        
        // Parse date strings to Date objects
        this.selectedEffectDate = coverage.effectDate 
            ? new Date(coverage.effectDate) 
            : new Date();
        this.selectedExpireDate = coverage.expireDate 
            ? new Date(coverage.expireDate) 
            : null;
        
        this.selectedSeqNumber = coverage.seqNumber ?? 1;
        this.selectedInsurerCoverageCode = coverage.insurerCoverageCode || '';
        this.selectedEnableQuantity = coverage.enableQuantity === 'Y';
        
        // Show the form
        this.coverageFormVisible = true;
        
        // Update parent coverage options (exclude current coverage being edited)
        this.updateParentCoverageOptions();
        
        // Clear menu cache to refresh menu state
        this.coverageMenuItemsCache.clear();
    }

    onDeleteCoverage(coverage: ProProductCoverageDto): void {
        // Check if has children
        // Only check for children if coverage.id exists and is valid
        // Convert both to strings for proper comparison to handle type mismatches
        if (coverage.id) {
            const coverageIdStr = String(coverage.id);
            const hasChildren = this.productCoverages.some(c => 
                c.parentId && String(c.parentId) === coverageIdStr
            );
            if (hasChildren) {
                this.messageService.add({
                    severity: 'warn',
                    detail: this.localizationService.localize('Product::ProProduct:CannotDeleteCoverageWithChildren')
                });
                return;
            }
        }

        // Allow delete when: product is draft/deactive OR coverage effectDate is in the future (or coverage has no effectDate).
        // Block and show error when: product is not draft/deactive AND coverage has effectDate AND today >= coverage.effectDate.
        if (!this.isEditingProductDraftOrDeactive() && coverage.effectDate) {
            const today = new Date();
            today.setHours(0, 0, 0, 0);
            const effectDate = new Date(coverage.effectDate);
            effectDate.setHours(0, 0, 0, 0);
            if (effectDate <= today) {
                this.messageService.add({
                    severity: 'warn',
                    detail: this.localizationService.localize('Product::ProProduct:CannotDeleteCoverage')
                });
                return;
            }
        }

        this.confirmWithDefaults({
            message: this.localizationService.localize('Product::ProProduct:DeleteCoverageConfirm'),
            header: this.localizationService.localize('Product::Confirm'),
            icon: 'pi pi-exclamation-triangle',
            accept: () => {
                const index = this.productCoverages.findIndex(c => c.id === coverage.id);
                if (index > -1) {
                    this.productCoverages.splice(index, 1);
                    this.buildCoverageTree();
                    this.messageService.add({
                        severity: 'success',
                        summary: this.localizationService.localize('AbpUi::Success'),
                        detail: this.localizationService.localize('Product::ProProduct:CoverageDeleted')
                    });
                }
            }
        });
    }

    getCoverageName(coverageId: string | undefined): string {
        if (!coverageId) return '-';
        const cov = this.coverageOptions.find(c => c.value === coverageId);
        return cov?.label || '-';
    }

    getCoverageCode(coverageId: string | undefined): string {
        if (!coverageId) return '-';
        const cov = this.coverageOptions.find(c => c.value === coverageId);
        return cov?.code || '-';
    }

    /**
     * Check if a tree node has children (is a parent node)
     */
    hasCoverageChildren(rowNode: any): boolean {
        // Check multiple possible paths for children in PrimeNG TreeTable
        const node = rowNode?.node || rowNode;
        return !!(node?.children && Array.isArray(node.children) && node.children.length > 0);
    }

    getCoverageType(coverageId: string | undefined): string {
        if (!coverageId) return '-';
        const cov = this.coverageOptions.find(c => c.value === coverageId);
        if (cov?.type === undefined || cov.type === null) return '-';
        // Find the option in proCoverageTermTypeOptions and localize it
        const option = proCoverageTermTypeOptions.find(opt => opt.value === cov.type);
        if (option) {
            return this.localizationService.localize(`Product::Enum:ProCoverageTermType.${option.key}`) || option.key;
        }
        return String(cov.type);
    }

    getUomName(uomId: string | undefined): string {
        if (!uomId) return '-';
        const uom = this.uomOptions.find(u => u.value === uomId);
        return uom?.label || '-';
    }

    getTaxName(taxId: string | undefined): string {
        if (!taxId) return '-';
        const tax = this.taxOptions.find(t => t.value === taxId);
        return tax?.label || '-';
    }

    getCoverageGroupName(coverageGroupId: string | undefined): string {
        if (!coverageGroupId) return '-';
        const group = this.coverageGroupOptions.find(g => g.value === coverageGroupId);
        return group?.label || '-';
    }

    getAvailabilityTypeLabel(availabilityType: ProProductCoverageAvailabilityType | undefined): string {
        if (availabilityType == null) return '-'; // Use == null to check for both null and undefined, allows 0
        const option = this.availabilityTypeOptions.find(o => o.value === availabilityType);
        return option?.label || '-';
    }

    // Interaction Modal Methods
    onShowInteractionModal(coverage: ProProductCoverageDto): void {
        // Ensure interactions array exists
        if (!coverage.productCoverageInteractions) {
            coverage.productCoverageInteractions = [];
        }
        
        this.selectedCoverageForInteraction = coverage;
        this.interactionModalVisible = true;
        this.interactionFormVisible = false;
        this.loadInteractionCoverageOptions();
        this.resetInteractionForm();
    }

    onCloseInteractionModal(): void {
        this.interactionModalVisible = false;
        this.selectedCoverageForInteraction = null;
        this.interactionFormVisible = false;
        this.resetInteractionForm();
    }

    loadInteractionCoverageOptions(): void {
        if (!this.selectedCoverageForInteraction || !this.productCoverages.length) {
            this.interactionCoverageOptions = [];
            return;
        }

        // Get the current coverage's ID for comparison (handle both saved and unsaved)
        const currentCoverageId = this.selectedCoverageForInteraction.id;
        const currentCoverageCoverageId = this.selectedCoverageForInteraction.coverageId;

        // Get list of product coverages to show (excluding current)
        const availableProductCoverages = this.productCoverages.filter(cov => {
            // Exclude current coverage
            if (currentCoverageId && cov.id) {
                return cov.id !== currentCoverageId;
            }
            if (currentCoverageCoverageId && cov.coverageId) {
                return cov.coverageId !== currentCoverageCoverageId;
            }
            return false;
        });

        if (availableProductCoverages.length === 0) {
            this.interactionCoverageOptions = [];
            return;
        }

        // Get unique coverage IDs from product coverages
        const productCoverageIds = availableProductCoverages
            .map(cov => cov.coverageId)
            .filter((id): id is string => !!id);

        if (productCoverageIds.length === 0) {
            this.interactionCoverageOptions = [];
            return;
        }

        // Always load from service to ensure we have the latest data
        this.coverageService.getList({
            maxResultCount: 1000,
            status: ProCoverageStatus.Active
        }).subscribe({
            next: (result) => {
                // Create a map for quick lookup
                const coverageMap = new Map<string, any>();
                (result.items || []).forEach(cov => {
                    if (cov.id) {
                        coverageMap.set(cov.id, cov);
                    }
                });

                // Build options from product coverages, using service data for labels
                this.interactionCoverageOptions = availableProductCoverages
                    .map(cov => {
                        if (!cov.coverageId) {
                            return null;
                        }

                        // Ensure cov.id exists (ProProductCoverage ID) - generate temp UUID if missing
                        // This ensures interactionCoverageId always references a ProProductCoverage, not a ProCoverage
                        if (!cov.id) {
                            cov.id = crypto.randomUUID();
                        }

                        // Get coverage details from service response
                        const coverage = coverageMap.get(cov.coverageId);
                        
                        if (!coverage) {
                            // Fallback to coverageOptions if service data not found
                            const coverageInfo = this.coverageOptions.find(c => c.value === cov.coverageId);
                            if (!coverageInfo) {
                                return null;
                            }
                            return {
                                label: `${coverageInfo.code || '-'} - ${coverageInfo.label || '-'}`,
                                value: cov.id  // Always use ProProductCoverage ID
                            };
                        }

                        // Use ProProductCoverage id as value (needed for saving)
                        return {
                            label: `${coverage.code || ''} - ${coverage.name || ''}`,
                            value: cov.id  // Always use ProProductCoverage ID
                        };
                    })
                    .filter((opt): opt is { label: string; value: string } => opt !== null && !!opt.value);
            },
            error: (err) => {
                console.error('Error loading interaction coverage options:', err);
                // Fallback to using coverageOptions if service call fails
                this.interactionCoverageOptions = availableProductCoverages
                    .map(cov => {
                        if (!cov.coverageId) {
                            return null;
                        }
                        
                        // Ensure cov.id exists (ProProductCoverage ID) - generate temp UUID if missing
                        // This ensures interactionCoverageId always references a ProProductCoverage, not a ProCoverage
                        if (!cov.id) {
                            cov.id = crypto.randomUUID();
                        }
                        
                        const coverageInfo = this.coverageOptions.find(c => c.value === cov.coverageId);
                        if (!coverageInfo) {
                            return null;
                        }
                        return {
                            label: `${coverageInfo.code || '-'} - ${coverageInfo.label || '-'}`,
                            value: cov.id  // Always use ProProductCoverage ID
                        };
                    })
                    .filter((opt): opt is { label: string; value: string } => opt !== null && !!opt.value);
            }
        });
    }

    onShowInteractionForm(): void {
        this.interactionFormVisible = true;
        this.selectedInteractionForEdit = null;
        this.resetInteractionForm();
    }

    onCancelInteractionForm(): void {
        this.confirmWithDefaults({
            message: this.localizationService.localize('Product::ProProductCoverageInteraction:CancelConfirm'),
            header: this.localizationService.localize('Product::Confirm'),
            icon: 'pi pi-exclamation-triangle',
            accept: () => {
                this.interactionFormVisible = false;
                this.resetInteractionForm();
            }
        });
    }

    resetInteractionForm(): void {
        this.selectedInteractionType = null;
        this.selectedInteractionCoverageId = null;
        this.selectedInteractionEffectDate = null;
        this.selectedInteractionExpireDate = null;
        this.selectedInteractionForEdit = null;
    }

    get isInteractionFormValid(): boolean {
        return !!(
            this.selectedInteractionType != null &&
            this.selectedInteractionCoverageId &&
            this.selectedInteractionEffectDate
        );
    }

    onSaveInteraction(): void {
        if (!this.selectedCoverageForInteraction) {
            return;
        }

        // Validation
        if (this.selectedInteractionType == null) {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Product::ProProductCoverageInteraction:InteractionTypeRequired')
            });
            return;
        }

        if (!this.selectedInteractionCoverageId) {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Product::ProProductCoverageInteraction:RelatedCoverageRequired')
            });
            return;
        }

        if (!this.selectedInteractionEffectDate) {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Product::ProProductCoverageInteraction:EffectDateRequired')
            });
            return;
        }

        // Validate effect date >= current date (only for create mode, not edit mode)
        const today = new Date();
        today.setHours(0, 0, 0, 0);
        const effectDate = new Date(this.selectedInteractionEffectDate);
        effectDate.setHours(0, 0, 0, 0);
        if (!this.selectedInteractionForEdit) {

            if (effectDate < today) {
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: this.localizationService.localize('Product::ProProductCoverageInteraction:EffectDateMustBeTodayOrLater')
                });
                return;
            }
        }

        // Validate expire date if provided
        if (this.selectedInteractionExpireDate) {
            const expireDate = new Date(this.selectedInteractionExpireDate);
            expireDate.setHours(0, 0, 0, 0);

            if (expireDate < today) {
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: this.localizationService.localize('Product::ProProductCoverageInteraction:ExpireDateMustBeTodayOrLater')
                });
                return;
            }

            if (expireDate < effectDate) {
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: this.localizationService.localize('Product::ProProductCoverageInteraction:ExpireDateMustBeAfterEffectDate')
                });
                return;
            }
        }

        // Ensure productCoverageInteractions array exists
        if (!this.selectedCoverageForInteraction.productCoverageInteractions) {
            this.selectedCoverageForInteraction.productCoverageInteractions = [];
        }

        if (this.selectedInteractionForEdit) {
            // Update existing interaction
            const interaction = this.selectedCoverageForInteraction.productCoverageInteractions.find(
                i => i.id === this.selectedInteractionForEdit?.id
            );
            if (interaction) {
                const fieldsLocked = this.isInteractionConstraintFieldsLocked;
                interaction.interactionType = fieldsLocked ? this.selectedInteractionForEdit.interactionType! : this.selectedInteractionType!;
                interaction.interactionCoverageId = fieldsLocked ? this.selectedInteractionForEdit.interactionCoverageId : (this.selectedInteractionCoverageId || undefined);
                interaction.effectDate = fieldsLocked
                    ? this.selectedInteractionForEdit.effectDate
                    : (this.toLocalIsoDateOrPass(this.selectedInteractionEffectDate) ?? undefined);
                interaction.expireDate = this.selectedInteractionExpireDate
                    ? (this.toLocalIsoDateOrPass(this.selectedInteractionExpireDate) ?? undefined)
                    : undefined;

                this.messageService.add({
                    severity: 'success',
                    summary: this.localizationService.localize('AbpUi::Success'),
                    detail: this.localizationService.localize('Product::ProProductCoverageInteraction:Updated')
                });
            }
        } else {
            // Add new interaction
            const newInteraction: ProProductCoverageInteractionDto = {
                id: undefined, // Will be generated on backend
                productCoverageId: this.selectedCoverageForInteraction.id || undefined,
                interactionType: this.selectedInteractionType!,
                interactionCoverageId: this.selectedInteractionCoverageId || undefined,
                effectDate: this.toLocalIsoDateOrPass(this.selectedInteractionEffectDate) ?? undefined,
                expireDate: this.selectedInteractionExpireDate
                    ? (this.toLocalIsoDateOrPass(this.selectedInteractionExpireDate) ?? undefined)
                    : undefined
            };

            this.selectedCoverageForInteraction.productCoverageInteractions.push(newInteraction);

            this.messageService.add({
                severity: 'success',
                summary: this.localizationService.localize('AbpUi::Success'),
                detail: this.localizationService.localize('Product::ProProductCoverageInteraction:Added')
            });
        }

        // Update the productCoverages array
        // For newly created coverages without ID, find by reference or coverageId
        let index = -1;
        if (this.selectedCoverageForInteraction.id) {
            index = this.productCoverages.findIndex(c => c.id === this.selectedCoverageForInteraction?.id);
        } else {
            // For newly created coverages, find by reference or by coverageId
            const selectedCoverage = this.selectedCoverageForInteraction;
            index = this.productCoverages.findIndex(c => 
                c === selectedCoverage || 
                (selectedCoverage && c.coverageId === selectedCoverage.coverageId && !c.id)
            );
        }
        if (index > -1 && this.selectedCoverageForInteraction) {
            this.productCoverages[index] = this.selectedCoverageForInteraction;
        }

        this.interactionFormVisible = false;
        this.resetInteractionForm();
    }

    onEditInteraction(interaction: ProProductCoverageInteractionDto): void {
        if (!this.canEditInteraction(interaction)) {
            this.messageService.add({
                severity: 'warn',
                summary: this.localizationService.localize('AbpUi::Warning'),
                detail: this.localizationService.localize('Product::ProProductCoverageInteraction:CannotEditWhenActive')
            });
            return;
        }

        this.selectedInteractionForEdit = interaction;
        this.selectedInteractionType = interaction.interactionType ?? null;
        this.selectedInteractionCoverageId = interaction.interactionCoverageId || null;
        this.selectedInteractionEffectDate = interaction.effectDate
            ? new Date(interaction.effectDate)
            : null;
        this.selectedInteractionExpireDate = interaction.expireDate
            ? new Date(interaction.expireDate)
            : null;
        this.interactionFormVisible = true;
    }

    onDeleteInteraction(interaction: ProProductCoverageInteractionDto): void {
        if (!this.canDeleteInteraction(interaction)) {
            this.messageService.add({
                severity: 'warn',
                summary: this.localizationService.localize('AbpUi::Warning'),
                detail: this.localizationService.localize('Product::ProProductCoverageInteraction:CannotDeleteWhenActive')
            });
            return;
        }

        this.confirmWithDefaults({
            message: this.localizationService.localize('Product::ProProductCoverageInteraction:DeleteConfirm'),
            header: this.localizationService.localize('Product::Confirm'),
            icon: 'pi pi-exclamation-triangle',
            accept: () => {
                if (!this.selectedCoverageForInteraction?.productCoverageInteractions) {
                    return;
                }

                const index = this.selectedCoverageForInteraction.productCoverageInteractions.findIndex(
                    i => i.id === interaction.id
                );
                if (index > -1) {
                    this.selectedCoverageForInteraction.productCoverageInteractions.splice(index, 1);

                    // Update the productCoverages array
                    const coverageIndex = this.productCoverages.findIndex(
                        c => c.id === this.selectedCoverageForInteraction?.id
                    );
                    if (coverageIndex > -1) {
                        this.productCoverages[coverageIndex] = this.selectedCoverageForInteraction;
                    }

                    this.messageService.add({
                        severity: 'success',
                        summary: this.localizationService.localize('AbpUi::Success'),
                        detail: this.localizationService.localize('Product::ProProductCoverageInteraction:Deleted')
                    });
                }
            }
        });
    }

    getInteractionTypeLabel(type: ProProductCoverageInteractionType | undefined): string {
        if (type == null) return '-';
        const option = this.interactionTypeOptions.find(o => o.value === type);
        return option?.label || '-';
    }

    getInteractionCoverageName(interactionCoverageId: string | undefined): string {
        if (!interactionCoverageId) return '-';
        
        let productCoverage: ProProductCoverageDto | undefined;
        
        // Check if interactionCoverageId is a temp ID (for newly created coverages without backend ID)
        if (interactionCoverageId.startsWith('temp-')) {
            // Extract coverageId from temp ID (format: "temp-{coverageId}")
            const coverageId = interactionCoverageId.substring(5); // Remove "temp-" prefix
            // Find ProProductCoverage by coverageId for temp IDs
            productCoverage = this.productCoverages.find(c => String(c.coverageId) === String(coverageId));
        } else {
            // Find ProProductCoverage by id (interactionCoverageId is a ProProductCoverage.id)
            // Convert both to strings for comparison to handle Guid/string type differences
            productCoverage = this.productCoverages.find(c => String(c.id) === String(interactionCoverageId));
        }
        
        if (!productCoverage) return '-';
        // Get the ProCoverage name using the coverageId from the ProProductCoverage
        return this.getCoverageName(productCoverage.coverageId);
    }

    getInteractionCoverageCode(interactionCoverageId: string | undefined): string {
        if (!interactionCoverageId) return '-';
        
        let productCoverage: ProProductCoverageDto | undefined;
        
        // Check if interactionCoverageId is a temp ID (for newly created coverages without backend ID)
        if (interactionCoverageId.startsWith('temp-')) {
            // Extract coverageId from temp ID (format: "temp-{coverageId}")
            const coverageId = interactionCoverageId.substring(5); // Remove "temp-" prefix
            // Find ProProductCoverage by coverageId for temp IDs
            productCoverage = this.productCoverages.find(c => String(c.coverageId) === String(coverageId));
        } else {
            // Find ProProductCoverage by id (interactionCoverageId is a ProProductCoverage.id)
            // Convert both to strings for comparison to handle Guid/string type differences
            productCoverage = this.productCoverages.find(c => String(c.id) === String(interactionCoverageId));
        }
        
        if (!productCoverage) return '-';
        // Get the ProCoverage code using the coverageId from the ProProductCoverage
        return this.getCoverageCode(productCoverage.coverageId);
    }

    canEditInteraction(interaction: ProProductCoverageInteractionDto): boolean {
        // No editing allowed in detail mode
        if (this.isDetailMode()) return false;
        
        // Newly added interactions (not yet saved to backend) can always be edited
        // Check this FIRST before checking selectedProduct, as newly added interactions should be editable
        // even if selectedProduct is null (e.g., when modal is open)
        const hasNoId = !interaction.id || interaction.id === '' || interaction.id === '00000000-0000-0000-0000-000000000000';
        const hasNoCreationTime = !interaction.creationTime;
        
        if (hasNoId || hasNoCreationTime) {
            return true;
        }
        
        // For saved interactions, we need selectedProduct to check permissions
        if (!this.selectedProduct) {
            return false;
        }

        // Can edit if product is Draft OR product effect date < current date OR coverage effect date < current date
        const today = new Date();
        today.setHours(0, 0, 0, 0);

        if (this.isProductDraftOrDeactive(this.selectedProduct.status)) {
            return true;
        }

        if (this.selectedProduct.effectDate) {
            const productEffectDate = new Date(this.selectedProduct.effectDate);
            productEffectDate.setHours(0, 0, 0, 0);
            if (productEffectDate < today) {
                return true;
            }
        }

        if (this.selectedCoverageForInteraction?.effectDate) {
            const coverageEffectDate = new Date(this.selectedCoverageForInteraction.effectDate);
            coverageEffectDate.setHours(0, 0, 0, 0);
            if (coverageEffectDate < today) {
                return true;
            }
        }

        return false;
    }

    canDeleteInteraction(interaction: ProProductCoverageInteractionDto): boolean {
        // No deleting allowed in detail mode
        if (this.isDetailMode()) return false;
        
        // Newly added interactions (not yet saved to backend) can always be deleted
        const hasNoId = !interaction.id || interaction.id === '' || interaction.id === '00000000-0000-0000-0000-000000000000';
        const hasNoCreationTime = !interaction.creationTime;
        if (hasNoId || hasNoCreationTime) return true;
        
        // For saved interactions: allow delete only when status = draft OR current date < product effectDate
        const product = this.selectedProduct ?? (this.formData as { status?: ProProductStatus; effectDate?: Date | string } | null);
        if (!product) return false;
        if (this.isProductDraftOrDeactive(product.status)) return true;
        const effectDate = product.effectDate;
        if (!effectDate) return false; // product active, no effectDate → cannot delete
        const today = new Date();
        today.setHours(0, 0, 0, 0);
        const productEffectDate = new Date(effectDate);
        productEffectDate.setHours(0, 0, 0, 0);
        return today < productEffectDate;
    }

    getCoverageMenuItems(coverage: ProProductCoverageDto): MenuItem[] {
        // Cache menu items to prevent recreation on every change detection
        const cacheKey = coverage.id || `temp-${coverage.coverageId}`;
        if (this.coverageMenuItemsCache.has(cacheKey)) {
            return this.coverageMenuItemsCache.get(cacheKey)!;
        }
        
        const isDetail = this.isDetailMode();
        const items: MenuItem[] = [
            {
                label: this.localizationService.localize('Product::ProProductCoverageInteraction:SetupConstraints'),
                icon: 'pi pi-link',
                command: () => {
                    this.onShowInteractionModal(coverage);
                }
            },
            {
                label: this.localizationService.localize('Product::ProProductCoverageLevel:SetupLevel'),
                icon: 'pi pi-sliders-h',
                command: () => {
                    this.onShowLevelModal(coverage);
                }
            },
            {
                label: this.localizationService.localize('Product::Edit'),
                icon: 'pi pi-pencil',
                disabled: isDetail,
                command: () => {
                    this.onEditCoverage(coverage);
                }
            },
            {
                label: this.localizationService.localize('Product::Delete'),
                icon: 'pi pi-trash',
                disabled: isDetail,
                command: () => {
                    this.onDeleteCoverage(coverage);
                }
            }
        ];
        this.coverageMenuItemsCache.set(cacheKey, items);
        return items;
    }

    // Level Setup Modal Methods
    onShowLevelModal(coverage: ProProductCoverageDto): void {
        // Ensure levels array exists
        if (!coverage.productCoverageLevels) {
            coverage.productCoverageLevels = [];
        }
        
        this.selectedCoverageForLevel = coverage;
        this.coverageLevels = coverage.productCoverageLevels || [];
        this.levelModalVisible = true;
        this.levelFormVisible = false;
        this.resetLevelForm();
    }

    onCloseLevelModal(): void {
        this.levelModalVisible = false;
        this.selectedCoverageForLevel = null;
        this.coverageLevels = [];
        this.levelFormVisible = false;
        this.resetLevelForm();
    }

    loadCoverageLevels(): void {
        if (!this.selectedCoverageForLevel) {
            this.coverageLevels = [];
            return;
        }
        this.coverageLevels = this.selectedCoverageForLevel.productCoverageLevels || [];
    }

    onShowLevelForm(level?: ProProductCoverageLevelDto): void {
        this.levelFormVisible = true;
        this.selectedLevel = level || null;
        if (level) {
            this.levelFormData = {
                code: level.code,
                name: level.name,
                effectDate: level.effectDate ? new Date(level.effectDate) : undefined,
                expireDate: level.expireDate ? new Date(level.expireDate) : undefined,
                conditionalScript: level.conditionalScript ?? undefined
            };
        } else {
            this.resetLevelForm();
        }
    }

    get canEditLevelCode(): boolean {
        if (!this.selectedLevel) return true; // adding new level: allow edit
        const product = this.selectedProduct ?? (this.formData as { status?: ProProductStatus; effectDate?: Date | string } | null);
        if (this.isProductDraftOrDeactive(product?.status)) return true;
        if (!product?.effectDate) return true;
        const today = new Date();
        today.setHours(0, 0, 0, 0);
        const productEffectDate = new Date(product.effectDate);
        productEffectDate.setHours(0, 0, 0, 0);
        return productEffectDate > today;
    }

    get canEditLevelEffectDate(): boolean {
        if (!this.selectedLevel) return true; // adding new level: allow edit
        const product = this.selectedProduct ?? (this.formData as { status?: ProProductStatus; effectDate?: Date | string } | null);
        if (this.isProductDraftOrDeactive(product?.status)) return true;
        if (!product?.effectDate) return true;
        const today = new Date();
        today.setHours(0, 0, 0, 0);
        const productEffectDate = new Date(product.effectDate);
        productEffectDate.setHours(0, 0, 0, 0);
        return productEffectDate > today;
    }

    onCancelLevelForm(): void {
        this.confirmWithDefaults({
            message: this.localizationService.localize('Product::ProProduct:CancelActionConfirm'),
            header: this.localizationService.localize('Product::Confirm'),
            icon: 'pi pi-exclamation-triangle',
            accept: () => {
                this.levelFormVisible = false;
                this.selectedLevel = null;
                this.resetLevelForm();
            }
        });
    }

    onSaveLevel(): void {
        // Validation
        if (!this.levelFormData.code || this.levelFormData.code.trim() === '') {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Product::ProProductCoverageLevel:CodeRequired')
            });
            return;
        }

        // Validate code format: A-Z, 0-9, _ only
        if (!/^[A-Z0-9_]+$/.test(this.levelFormData.code.toUpperCase())) {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Product::ProProductCoverageLevel:CodeFormatInvalid')
            });
            return;
        }

        if (!this.levelFormData.name || this.levelFormData.name.trim() === '') {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Product::ProProductCoverageLevel:NameRequired')
            });
            return;
        }

        if (!this.levelFormData.effectDate) {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Product::ProProductCoverageLevel:EffectDateRequired')
            });
            return;
        }

        // Validate effect date >= today (only for create mode, not edit mode)
        const today = new Date();
        today.setHours(0, 0, 0, 0);
        if (!this.selectedLevel) {
        
            if (this.levelFormData.effectDate) {
                const effectDate = this.levelFormData.effectDate instanceof Date 
                    ? new Date(this.levelFormData.effectDate.getTime())
                    : new Date(this.levelFormData.effectDate);
                effectDate.setHours(0, 0, 0, 0);
                if (effectDate < today) {
                    this.messageService.add({
                        severity: 'error',
                        summary: this.localizationService.localize('AbpUi::Error'),
                        detail: this.localizationService.localize('Product::ProProductCoverageLevel:EffectDateMustBeTodayOrLater')
                    });
                    return;
                }
            }
        }

        // Validate expire date if provided
        if (this.levelFormData.expireDate) {
            const expireDate = this.levelFormData.expireDate instanceof Date
                ? new Date(this.levelFormData.expireDate.getTime())
                : new Date(this.levelFormData.expireDate);
            expireDate.setHours(0, 0, 0, 0);
            if (expireDate < today) {
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: this.localizationService.localize('Product::ProProductCoverageLevel:ExpireDateMustBeTodayOrLater')
                });
                return;
            }
            if (this.levelFormData.effectDate) {
                const effectDate = this.levelFormData.effectDate instanceof Date
                    ? new Date(this.levelFormData.effectDate.getTime())
                    : new Date(this.levelFormData.effectDate);
                effectDate.setHours(0, 0, 0, 0);
                if (expireDate < effectDate) {
                    this.messageService.add({
                        severity: 'error',
                        summary: this.localizationService.localize('AbpUi::Error'),
                        detail: this.localizationService.localize('Product::ProProductCoverageLevel:ExpireDateMustBeAfterEffectDate')
                    });
                    return;
                }
            }
        }

        this.confirmWithDefaults({
            message: this.localizationService.localize('Product::ProProductCoverageLevel:AddLevelConfirm'),
            header: this.localizationService.localize('Product::Confirm'),
            icon: 'pi pi-question-circle',
            accept: () => {
                if (!this.selectedCoverageForLevel) return;

                // Ensure levels array exists
                if (!this.selectedCoverageForLevel.productCoverageLevels) {
                    this.selectedCoverageForLevel.productCoverageLevels = [];
                }

                const levelData: ProProductCoverageLevelDto = {
                    productCoverageId: this.selectedCoverageForLevel.id,
                    code: this.levelFormData.code!.trim().toUpperCase(),
                    name: this.levelFormData.name!.trim(),
                    effectDate: this.toLocalIsoDateOrPass(this.levelFormData.effectDate) ?? '',
                    expireDate: this.levelFormData.expireDate && this.levelFormData.expireDate !== null
                        ? (this.toLocalIsoDateOrPass(this.levelFormData.expireDate) ?? undefined)
                        : undefined,
                    conditionalScript: this.levelFormData.conditionalScript?.trim() || undefined,
                    terms: []
                };

                if (this.selectedLevel) {
                    // Update existing level
                    const index = this.selectedCoverageForLevel.productCoverageLevels.findIndex(l => l.id === this.selectedLevel!.id);
                    if (index > -1) {
                        levelData.id = this.selectedLevel.id;
                        levelData.terms = this.selectedLevel.terms || [];
                        this.selectedCoverageForLevel.productCoverageLevels[index] = levelData;
                    }
                } else {
                    // Add new level
                    this.selectedCoverageForLevel.productCoverageLevels.push(levelData);
                }

                this.loadCoverageLevels();
                this.levelFormVisible = false;
                this.selectedLevel = null;
                this.resetLevelForm();

                this.messageService.add({
                    severity: 'success',
                    summary: this.localizationService.localize('AbpUi::Success'),
                    detail: this.localizationService.localize('Product::ProProductCoverageLevel:LevelSaved')
                });
            }
        });
    }

    onDeleteLevel(level: ProProductCoverageLevelDto): void {
        if (!this.selectedCoverageForLevel || !this.selectedProduct) return;

        // Check if can delete
        if (!this.canDeleteLevel(level)) {
            this.messageService.add({
                severity: 'warn',
                summary: this.localizationService.localize('AbpUi::Warning'),
                detail: this.localizationService.localize('Product::ProProductCoverageLevel:CannotDeleteLevel')
            });
            return;
        }

        this.confirmWithDefaults({
            message: this.localizationService.localize('Product::ProProductCoverageLevel:DeleteLevelConfirm'),
            header: this.localizationService.localize('Product::Confirm'),
            icon: 'pi pi-exclamation-triangle',
            accept: () => {
                const index = this.selectedCoverageForLevel!.productCoverageLevels!.findIndex(l => l.id === level.id);
                if (index > -1) {
                    this.selectedCoverageForLevel!.productCoverageLevels!.splice(index, 1);
                    this.loadCoverageLevels();
                    this.messageService.add({
                        severity: 'success',
                        summary: this.localizationService.localize('AbpUi::Success'),
                        detail: this.localizationService.localize('Product::ProProductCoverageLevel:LevelDeleted')
                    });
                }
            }
        });
    }

    onShowTermModal(level: ProProductCoverageLevelDto, term?: ProProductCoverageLevelTermDto): void {
        this.selectedLevelForTerm = level;
        this.selectedTerm = term || null;
        this.termModalVisible = true;
        if (term) {
            this.termFormData = {
                coverageLevelTypeId: term.coverageLevelTypeId,
                coverageLevelBasisId: term.coverageLevelBasisId,
                amountType: term.amountType,
                fromAmount: term.fromAmount,
                toAmount: term.toAmount,
                isDefault: term.isDefault || 'N',
                conditionScript: term.conditionScript,
                computeScript: term.computeScript
            };
            this.termIsDefaultChecked = term.isDefault === 'Y';
        } else {
            this.resetTermForm();
        }
    }

    onCloseTermModal(): void {
        this.termModalVisible = false;
        this.selectedLevelForTerm = null;
        this.selectedTerm = null;
        this.resetTermForm();
    }

    onSaveTerm(): void {
        // Validation - only validate fields that can be edited
        if (this.canEditTermAllFields) {
            if (!this.termFormData.coverageLevelTypeId) {
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: this.localizationService.localize('Product::ProProductCoverageLevelTerm:CoverageLevelTypeRequired')
                });
                return;
            }

            if (!this.termFormData.coverageLevelBasisId) {
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: this.localizationService.localize('Product::ProProductCoverageLevelTerm:CoverageLevelBasisRequired')
                });
                return;
            }

            if (!this.termFormData.amountType) {
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: this.localizationService.localize('Product::ProProductCoverageLevelTerm:AmountTypeRequired')
                });
                return;
            }
        } else {
            // When editing restricted fields, use existing values
            if (this.selectedTerm) {
                this.termFormData.coverageLevelTypeId = this.termFormData.coverageLevelTypeId || this.selectedTerm.coverageLevelTypeId;
                this.termFormData.coverageLevelBasisId = this.termFormData.coverageLevelBasisId || this.selectedTerm.coverageLevelBasisId;
                this.termFormData.amountType = this.termFormData.amountType || this.selectedTerm.amountType;
            }
        }

        if (this.termFormData.fromAmount === undefined || this.termFormData.fromAmount === null || this.termFormData.fromAmount < 0) {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Product::ProProductCoverageLevelTerm:FromAmountRequired')
            });
            return;
        }

        if (this.termFormData.toAmount === undefined || this.termFormData.toAmount === null || this.termFormData.toAmount < 0) {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Product::ProProductCoverageLevelTerm:ToAmountRequired')
            });
            return;
        }

        if (this.termFormData.toAmount < this.termFormData.fromAmount) {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Product::ProProductCoverageLevelTerm:ToAmountMustBeGreaterThanFromAmount')
            });
            return;
        }

        this.confirmWithDefaults({
            message: this.localizationService.localize('Product::ProProductCoverageLevelTerm:AddTermConfirm'),
            header: this.localizationService.localize('Product::Confirm'),
            icon: 'pi pi-question-circle',
            accept: () => {
                if (!this.selectedLevelForTerm) return;

                // Ensure terms array exists
                if (!this.selectedLevelForTerm.terms) {
                    this.selectedLevelForTerm.terms = [];
                }

                const termData: ProProductCoverageLevelTermDto = {
                    productCoverageLevelId: this.selectedLevelForTerm.id,
                    coverageLevelTypeId: this.canEditTermAllFields 
                        ? this.termFormData.coverageLevelTypeId!
                        : (this.selectedTerm?.coverageLevelTypeId || this.termFormData.coverageLevelTypeId!),
                    coverageLevelBasisId: this.canEditTermAllFields
                        ? this.termFormData.coverageLevelBasisId!
                        : (this.selectedTerm?.coverageLevelBasisId || this.termFormData.coverageLevelBasisId!),
                    amountType: this.canEditTermAllFields
                        ? this.termFormData.amountType!
                        : (this.selectedTerm?.amountType || this.termFormData.amountType!),
                    fromAmount: this.termFormData.fromAmount!,
                    toAmount: this.termFormData.toAmount!,
                    isDefault: this.termIsDefaultChecked ? 'Y' : 'N',
                    conditionScript: this.termFormData.conditionScript,
                    computeScript: this.termFormData.computeScript
                };

                if (this.selectedTerm) {
                    // Update existing term
                    const index = this.selectedLevelForTerm.terms.findIndex(t => t.id === this.selectedTerm!.id);
                    if (index > -1) {
                        termData.id = this.selectedTerm.id;
                        this.selectedLevelForTerm.terms[index] = termData;
                    }
                } else {
                    // Add new term
                    this.selectedLevelForTerm.terms.push(termData);
                }

                this.termModalVisible = false;
                this.selectedLevelForTerm = null;
                this.selectedTerm = null;
                this.resetTermForm();

                this.messageService.add({
                    severity: 'success',
                    summary: this.localizationService.localize('AbpUi::Success'),
                    detail: this.localizationService.localize('Product::ProProductCoverageLevelTerm:TermSaved')
                });
            }
        });
    }

    onDeleteTerm(term: ProProductCoverageLevelTermDto, level: ProProductCoverageLevelDto): void {
        if (!this.selectedProduct) return;

        // Check if can delete
        if (!this.canDeleteTerm(term, level)) {
            this.messageService.add({
                severity: 'warn',
                summary: this.localizationService.localize('AbpUi::Warning'),
                detail: this.localizationService.localize('Product::ProProductCoverageLevelTerm:CannotDeleteTerm')
            });
            return;
        }

        this.confirmWithDefaults({
            message: this.localizationService.localize('Product::ProProductCoverageLevelTerm:DeleteTermConfirm'),
            header: this.localizationService.localize('Product::Confirm'),
            icon: 'pi pi-exclamation-triangle',
            accept: () => {
                const index = level.terms.findIndex(t => t.id === term.id);
                if (index > -1) {
                    level.terms.splice(index, 1);
                    this.messageService.add({
                        severity: 'success',
                        summary: this.localizationService.localize('AbpUi::Success'),
                        detail: this.localizationService.localize('Product::ProProductCoverageLevelTerm:TermDeleted')
                    });
                }
            }
        });
    }

    loadCoverageLevelTypeOptions(): void {
        this.coverageLevelTypeService.getList({
            maxResultCount: 1000,
            status: ProCoverageLevelTypeStatus.Active
        }).subscribe({
            next: (result) => {
                this.coverageLevelTypeOptions = (result.items || []).map(type => ({
                    label: type.name || '',
                    value: type.id || ''
                }));
            },
            error: (err) => {
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: err.error?.error?.message || 'Error loading coverage level types'
                });
            }
        });
    }

    loadCoverageLevelBasisOptions(): void {
        this.coverageLevelBasisService.getList({
            maxResultCount: 1000,
            status: ProCoverageLevelBasisStatus.Active
        }).subscribe({
            next: (result) => {
                this.coverageLevelBasisOptions = (result.items || []).map(basis => ({
                    label: basis.name || '',
                    value: basis.id || ''
                }));
            },
            error: (err) => {
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: err.error?.error?.message || 'Error loading coverage level basis'
                });
            }
        });
    }

    getLevelMenuItems(level: ProProductCoverageLevelDto): MenuItem[] {
        const items: MenuItem[] = [
            {
                label: this.localizationService.localize('Product::ProProductCoverageLevel:AddDetail'),
                icon: 'pi pi-plus',
                command: () => {
                    this.onShowTermModal(level);
                }
            },
            {
                label: this.localizationService.localize('Product::Edit'),
                icon: 'pi pi-pencil',
                disabled: !this.canEditLevel(level),
                command: () => {
                    this.onShowLevelForm(level);
                }
            },
            {
                label: this.localizationService.localize('Product::Delete'),
                icon: 'pi pi-trash',
                disabled: !this.canDeleteLevel(level),
                command: () => {
                    this.onDeleteLevel(level);
                }
            }
        ];
        return items;
    }

    getTermMenuItems(term: ProProductCoverageLevelTermDto, level: ProProductCoverageLevelDto): MenuItem[] {
        const items: MenuItem[] = [
            {
                label: this.localizationService.localize('Product::Edit'),
                icon: 'pi pi-pencil',
                disabled: !this.canEditTerm(term, level),
                command: () => {
                    this.onShowTermModal(level, term);
                }
            },
            {
                label: this.localizationService.localize('Product::Delete'),
                icon: 'pi pi-trash',
                disabled: !this.canDeleteTerm(term, level),
                command: () => {
                    this.onDeleteTerm(term, level);
                }
            }
        ];
        return items;
    }

    canEditLevel(level: ProProductCoverageLevelDto): boolean {
        // No editing allowed in detail mode
        if (this.isDetailMode()) return false;
        
        if (!level.effectDate) return true;
        const today = new Date();
        today.setHours(0, 0, 0, 0);
        const effectDate = new Date(level.effectDate);
        effectDate.setHours(0, 0, 0, 0);
        // Can always edit, but validation will restrict fields if effectDate <= today
        return true;
    }

    canDeleteLevel(level: ProProductCoverageLevelDto): boolean {
        // No deleting allowed in detail mode
        if (this.isDetailMode()) return false;
        
        if (!this.selectedProduct) return false;

        // Can delete if product is draft OR (product effectDate < today AND coverage effectDate < today AND level effectDate < today)
        const today = new Date();
        today.setHours(0, 0, 0, 0);

        if (this.isProductDraftOrDeactive(this.selectedProduct.status)) {
            return true;
        }

        if (this.selectedProduct.effectDate) {
            const productEffectDate = new Date(this.selectedProduct.effectDate);
            productEffectDate.setHours(0, 0, 0, 0);
            if (productEffectDate >= today) {
                return false;
            }
        }

        if (this.selectedCoverageForLevel?.effectDate) {
            const coverageEffectDate = new Date(this.selectedCoverageForLevel.effectDate);
            coverageEffectDate.setHours(0, 0, 0, 0);
            if (coverageEffectDate >= today) {
                return false;
            }
        }

        if (level.effectDate) {
            const levelEffectDate = new Date(level.effectDate);
            levelEffectDate.setHours(0, 0, 0, 0);
            if (levelEffectDate >= today) {
                return false;
            }
        }

        return true;
    }

    canEditTerm(term: ProProductCoverageLevelTermDto, level: ProProductCoverageLevelDto): boolean {
        // Can always edit, but validation will restrict fields if level effectDate <= today
        return true;
    }

    get canEditTermAllFields(): boolean {
        if (!this.selectedTerm) return true; // adding new term: allow edit
        const product = this.selectedProduct ?? (this.formData as { status?: ProProductStatus } | null);
        if (this.isProductDraftOrDeactive(product?.status)) return true;
        if (!this.selectedLevelForTerm || !this.selectedLevelForTerm.effectDate) return true;
        const today = new Date();
        today.setHours(0, 0, 0, 0);
        const effectDate = new Date(this.selectedLevelForTerm.effectDate);
        effectDate.setHours(0, 0, 0, 0);
        return effectDate > today;
    }

    canDeleteTerm(term: ProProductCoverageLevelTermDto, level: ProProductCoverageLevelDto): boolean {
        // No deleting allowed in detail mode
        if (this.isDetailMode()) return false;
        
        // Newly added terms (not yet saved to backend) can always be deleted
        // Check this FIRST before checking selectedProduct, as newly added terms should be deletable
        // even if selectedProduct is null (e.g., when modal is open)
        const hasNoId = !term.id || term.id === '' || term.id === '00000000-0000-0000-0000-000000000000';
        const hasNoCreationTime = !term.creationTime;
        
        if (hasNoId || hasNoCreationTime) {
            return true;
        }
        
        // For saved terms, we need selectedProduct to check permissions
        if (!this.selectedProduct) {
            return false;
        }
        
        // Can delete if product is draft OR (product effectDate < today AND coverage effectDate < today AND level effectDate < today)
        const today = new Date();
        today.setHours(0, 0, 0, 0);

        if (this.isProductDraftOrDeactive(this.selectedProduct.status)) {
            return true;
        }

        if (this.selectedProduct.effectDate) {
            const productEffectDate = new Date(this.selectedProduct.effectDate);
            productEffectDate.setHours(0, 0, 0, 0);
            if (productEffectDate >= today) {
                return false;
            }
        }

        if (this.selectedCoverageForLevel?.effectDate) {
            const coverageEffectDate = new Date(this.selectedCoverageForLevel.effectDate);
            coverageEffectDate.setHours(0, 0, 0, 0);
            if (coverageEffectDate >= today) {
                return false;
            }
        }

        if (level.effectDate) {
            const levelEffectDate = new Date(level.effectDate);
            levelEffectDate.setHours(0, 0, 0, 0);
            // Can delete if level hasn't taken effect yet (effectDate > today)
            // Cannot delete if level has already taken effect (effectDate <= today)
            if (levelEffectDate <= today) {
                return false;
            }
        }

        return true;
    }

    getLevelTypeName(levelTypeId: string | undefined): string {
        if (!levelTypeId) return '-';
        const type = this.coverageLevelTypeOptions.find(t => t.value === levelTypeId);
        return type?.label || '-';
    }

    getLevelBasisName(basisId: string | undefined): string {
        if (!basisId) return '-';
        const basis = this.coverageLevelBasisOptions.find(b => b.value === basisId);
        return basis?.label || '-';
    }

    getAmountTypeLabel(amountType: string | undefined): string {
        if (!amountType) return '-';
        const option = this.amountTypeOptions.find(o => o.value === amountType);
        return option?.label || amountType;
    }

    private resetLevelForm(): void {
        const tomorrow = new Date();
        tomorrow.setDate(tomorrow.getDate() + 1);
        tomorrow.setHours(0, 0, 0, 0);

        this.levelFormData = {
            code: '',
            name: '',
            effectDate: tomorrow,
            expireDate: undefined,
            conditionalScript: undefined
        };
    }

    private resetTermForm(): void {
        this.termFormData = {
            coverageLevelTypeId: undefined,
            coverageLevelBasisId: undefined,
            amountType: undefined,
            fromAmount: 0,
            toAmount: 0,
            isDefault: 'N',
            conditionScript: undefined,
            computeScript: undefined
        };
        this.termIsDefaultChecked = false;
    }

    getInteractionMenuItems(interaction: ProProductCoverageInteractionDto): MenuItem[] {
        // Always create a new array to ensure PrimeNG detects changes
        const canEdit = this.canEditInteraction(interaction);
        const canDelete = this.canDeleteInteraction(interaction);
        
        const items: MenuItem[] = [
            {
                label: this.localizationService.localize('Product::ProProductCoverageInteraction:Edit'),
                icon: 'pi pi-pencil',
                command: () => {
                    this.onEditInteraction(interaction);
                },
                disabled: !canEdit
            },
            {
                label: this.localizationService.localize('Product::ProProductCoverageInteraction:Delete'),
                icon: 'pi pi-trash',
                command: () => {
                    this.onDeleteInteraction(interaction);
                },
                disabled: !canDelete
            }
        ];
        return items;
    }

    // Table Rate Tab Methods
    loadTableRateOptions(lobId: string | undefined, insurerId?: string | undefined): void {
        if (!lobId) {
            this.tableRateOptions = [];
            return;
        }

        const input: GetProTableRatesInput = {
            skipCount: 0,
            maxResultCount: 1000,
            lobId: lobId,
            insurerId: insurerId,
            status: ProTableRateStatus.Active,
            sorting: 'name asc'
        };

        this.tableRateService.getList(input).subscribe({
            next: (result) => {
                this.tableRateOptions = (result.items || []).map(tr => ({
                    label: `${tr.code || ''} - ${tr.name || ''}`,
                    value: tr.id || '',
                    code: tr.code || ''
                }));
            },
            error: (err) => {
                console.error('Error loading table rates:', err);
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: err.error?.error?.message || 'Error loading table rates'
                });
                this.tableRateOptions = [];
            }
        });
    }

    /** Options for product category on create/edit when filtering by selected LOB. */
    get productCategorySelectOptions(): { label: string; value: string }[] {
        if (!this.formData.lobId) {
            return this.categoryOptions;
        }
        return this.categories
            .filter(c => c.lobId === this.formData.lobId)
            .map(c => ({ label: c.name || '', value: c.id || '' }));
    }

    /** Options for parent product on create/edit when filtering by selected LOB. */
    get parentProductSelectOptions(): { label: string; value: string }[] {
        const baseProducts = this.products.filter(p => p.isPlan !== 'Y');
        const filteredProducts = this.formData.lobId
            ? baseProducts.filter(p => p.lobId === this.formData.lobId)
            : baseProducts;

        return filteredProducts.map(p => ({
            label: `${p.code} - ${p.name}`,
            value: p.id || ''
        }));
    }

    onLobIdChange(): void {
        if (this.formData.lobId && this.formData.productCategoryId) {
            const cat = this.categories.find(c => c.id === this.formData.productCategoryId);
            if (cat && cat.lobId !== this.formData.lobId) {
                this.formData.productCategoryId = null;
            }
        }
        // Reload table rate options when LOB changes
        // Only reload if we have a LOB selected and the table rate form might be visible
        if (this.formData.lobId) {
            this.loadTableRateOptions(this.formData.lobId, this.formData.partnerId || undefined);
        } else {
            this.tableRateOptions = [];
        }
    }

    private tryAutofillLobFromParentProduct(parentProductId: string | null): void {
        if (!parentProductId || this.formData.lobId) {
            return;
        }

        const parentProduct = this.products.find(p => p.id === parentProductId);
        if (parentProduct) {
            this.applyParentLobIfMissing(parentProduct.lobId);
            return;
        }

        this.productService.get(parentProductId).subscribe({
            next: (product) => {
                this.applyParentLobIfMissing(product.lobId);
            },
            error: (err) => {
                console.warn('Unable to resolve parent product for LOB auto-fill:', err);
            }
        });
    }

    private applyParentLobIfMissing(parentLobId?: string): void {
        if (!this.formData.lobId && parentLobId) {
            this.formData.lobId = parentLobId;
            this.onLobIdChange();
        }
    }

    onProductCategoryIdChange(): void {
        if (!this.formData.productCategoryId) {
            return;
        }
        const cat = this.categories.find(c => c.id === this.formData.productCategoryId);
        if (!cat) {
            return;
        }
        if (this.formData.lobId) {
            if (cat.lobId !== this.formData.lobId) {
                this.formData.productCategoryId = null;
            }
            return;
        }
        if (cat.lobId) {
            this.formData.lobId = cat.lobId;
            this.onLobIdChange();
        }
    }

    onShowTableRateForm(): void {
        this.tableRateFormVisible = true;
        this.selectedTableRateId = null;
        this.selectedTableRateEffectDate = new Date();
        this.selectedTableRateExpireDate = null;
        this.selectedTableRateForEdit = null;
        
        // Load table rate options when form is shown
        if (this.formData.lobId) {
            this.loadTableRateOptions(this.formData.lobId, this.formData.partnerId || undefined);
        }
    }

    onCancelTableRateForm(): void {
        this.confirmWithDefaults({
            message: this.localizationService.localize('Product::ProProduct:CancelActionConfirm'),
            header: this.localizationService.localize('Product::ProProduct:CancelAction'),
            icon: 'pi pi-exclamation-triangle',
            accept: () => {
                this.tableRateFormVisible = false;
                this.selectedTableRateId = null;
                this.selectedTableRateEffectDate = null;
                this.selectedTableRateExpireDate = null;
                this.selectedTableRateForEdit = null;
            }
        });
    }

    onSaveTableRate(): void {
        if (!this.selectedTableRateId) {
            this.messageService.add({
                severity: 'warn',
                summary: this.localizationService.localize('AbpUi::Warning'),
                detail: this.localizationService.localize('Product::ProProduct:TableRateRequired')
            });
            return;
        }

        if (!this.selectedTableRateEffectDate) {
            this.messageService.add({
                severity: 'warn',
                summary: this.localizationService.localize('AbpUi::Warning'),
                detail: this.localizationService.localize('Product::ProProduct:EffectDateRequired')
            });
            return;
        }

        // Validate dates (only for create mode, not edit mode)
        const today = new Date();
        today.setHours(0, 0, 0, 0);
        const effectDate = new Date(this.selectedTableRateEffectDate);
        effectDate.setHours(0, 0, 0, 0);
        if (!this.selectedTableRateForEdit) {
            if (effectDate < today) {
                this.messageService.add({
                    severity: 'warn',
                    summary: this.localizationService.localize('AbpUi::Warning'),
                    detail: this.localizationService.localize('Product::ProProduct:EffectDateMustBeTodayOrLater')
                });
                return;
            }
        }

        if (this.selectedTableRateExpireDate) {
            const expireDate = new Date(this.selectedTableRateExpireDate);
            expireDate.setHours(0, 0, 0, 0);
            if (expireDate < effectDate) {
                this.messageService.add({
                    severity: 'warn',
                    summary: this.localizationService.localize('AbpUi::Warning'),
                    detail: this.localizationService.localize('Product::ProProduct:ExpireDateMustBeAfterEffectDate')
                });
                return;
            }
        }

        if (this.selectedTableRateForEdit) {
            // Update existing
            const index = this.productTableRates.findIndex(tr => tr.id === this.selectedTableRateForEdit!.id);
            if (index >= 0) {
                const oldId = this.selectedTableRateForEdit.id;
                const canEditAll = this.canEditTableRateAllFields(this.selectedTableRateForEdit);
                if (canEditAll) {
                    // Can edit all fields
                    this.productTableRates[index] = {
                        ...this.selectedTableRateForEdit,
                        tableRateId: this.selectedTableRateId || this.selectedTableRateForEdit.tableRateId,
                        effectDate: this.toLocalIsoDateAtMidnight(this.selectedTableRateEffectDate)!,
                        expireDate: this.selectedTableRateExpireDate ? (this.toLocalIsoDateTime(this.selectedTableRateExpireDate) ?? undefined) : undefined
                    };
                } else {
                    // Can only edit expire date
                    this.productTableRates[index] = {
                        ...this.selectedTableRateForEdit,
                        expireDate: this.selectedTableRateExpireDate ? (this.toLocalIsoDateTime(this.selectedTableRateExpireDate) ?? undefined) : undefined
                    };
                }
                // Clear cache for edited table rate
                this.tableRateMenuItemsCache.delete(oldId || '');
            }
        } else {
            // Add new
            const newTableRate: ProProductTableRateDto = {
                id: undefined, // Will be generated by backend
                productId: this.selectedProduct?.id || undefined,
                tableRateId: this.selectedTableRateId || '',
                effectDate: this.toLocalIsoDateAtMidnight(this.selectedTableRateEffectDate)!,
                expireDate: this.selectedTableRateExpireDate ? (this.toLocalIsoDateTime(this.selectedTableRateExpireDate) ?? undefined) : undefined
            };
            this.productTableRates.push(newTableRate);
            // Clear cache for newly added table rate (empty id)
            this.tableRateMenuItemsCache.delete('');
        }

        this.messageService.add({
            severity: 'success',
            summary: this.localizationService.localize('AbpUi::Success'),
            detail: this.localizationService.localize('Product::ProProduct:TableRateAdded')
        });

        this.tableRateFormVisible = false;
        this.selectedTableRateId = null;
        this.selectedTableRateEffectDate = null;
        this.selectedTableRateExpireDate = null;
        this.selectedTableRateForEdit = null;
    }

    onEditTableRate(tableRate: ProProductTableRateDto): void {
        // Check if editing is allowed
        if (!this.canEditTableRate(tableRate)) {
            this.messageService.add({
                severity: 'warn',
                summary: this.localizationService.localize('AbpUi::Warning'),
                detail: this.localizationService.localize('Product::ProProduct:CannotEditTableRate')
            });
            return;
        }

        this.selectedTableRateForEdit = { ...tableRate }; // Create a copy
        this.selectedTableRateId = tableRate.tableRateId || null;
        this.selectedTableRateEffectDate = tableRate.effectDate ? new Date(tableRate.effectDate) : new Date();
        this.selectedTableRateExpireDate = tableRate.expireDate ? new Date(tableRate.expireDate) : null;
        this.tableRateFormVisible = true;
    }

    onDeleteTableRate(tableRate: ProProductTableRateDto): void {
        // Check if deletion is allowed
        if (!this.canDeleteTableRate(tableRate)) {
            this.messageService.add({
                severity: 'warn',
                summary: this.localizationService.localize('AbpUi::Warning'),
                detail: this.localizationService.localize('Product::ProProduct:CannotDeleteTableRateWhenActive')
            });
            return;
        }

        this.confirmWithDefaults({
            message: this.localizationService.localize('Product::ProProduct:DeleteTableRateConfirm'),
            header: this.localizationService.localize('Product::Delete'),
            icon: 'pi pi-exclamation-triangle',
            accept: () => {
                const index = this.productTableRates.findIndex(tr => tr.id === tableRate.id);
                if (index >= 0) {
                    const deletedId = tableRate.id;
                    this.productTableRates.splice(index, 1);
                    // Clear cache for deleted table rate
                    this.tableRateMenuItemsCache.delete(deletedId || '');
                    this.messageService.add({
                        severity: 'success',
                        summary: this.localizationService.localize('AbpUi::Success'),
                        detail: this.localizationService.localize('Product::ProProduct:TableRateDeleted')
                    });
                }
            }
        });
    }

    canEditTableRate(tableRate: ProProductTableRateDto): boolean {
        // No editing allowed in detail mode
        if (this.isDetailMode()) return false;
        
        // Newly added table rates (not yet saved to backend) can always be edited
        // Check this FIRST before checking selectedProduct, as newly added table rates should be editable
        // even if selectedProduct is null (e.g., when modal is open)
        const hasNoId = !tableRate.id || tableRate.id === '' || tableRate.id === '00000000-0000-0000-0000-000000000000';
        const hasNoCreationTime = !tableRate.creationTime;
        
        if (hasNoId || hasNoCreationTime) {
            return true;
        }
        
        // For saved table rates, we need selectedProduct to check permissions
        if (!this.selectedProduct) {
            return false;
        }
        
        // Can edit all fields if product is draft/deactive or effect date < today
        if (this.isEditingProductDraftOrDeactive()) {
            return true;
        }

        if (this.selectedProduct.effectDate) {
            const productEffectDate = new Date(this.selectedProduct.effectDate);
            productEffectDate.setHours(0, 0, 0, 0);
            const today = new Date();
            today.setHours(0, 0, 0, 0);
            if (productEffectDate < today) {
                return true;
            }
        }

        // Can only edit expire date if product is active and effect date >= today
        // This is still considered "editable" but with restrictions
        return true;
    }

    canEditTableRateAllFields(tableRate: ProProductTableRateDto): boolean {
        // No editing allowed in detail mode
        if (this.isDetailMode()) return false;
        
        if (!this.selectedProduct) return false;
        
        // Allow all fields when product is draft or current date < effectiveDate (product not yet active)
        return !this.isProductActiveForRestriction();
    }

    canDeleteTableRate(tableRate: ProProductTableRateDto): boolean {
        // No deleting allowed in detail mode
        if (this.isDetailMode()) return false;
        
        // Newly added table rates (not yet saved to backend) can always be deleted
        const hasNoId = !tableRate.id || tableRate.id === '' || tableRate.id === '00000000-0000-0000-0000-000000000000';
        const hasNoCreationTime = !tableRate.creationTime;
        
        if (hasNoId || hasNoCreationTime) {
            return true;
        }
        
        if (!this.selectedProduct) return false;
        
        // Allow delete when product is draft or current date < effectiveDate (product not yet active)
        return !this.isProductActiveForRestriction();
    }

    getTableRateCode(tableRateId: string | undefined | null): string {
        if (!tableRateId) return '-';
        const tr = this.tableRateOptions.find(t => t.value === tableRateId);
        return tr?.code || '-';
    }

    getTableRateName(tableRateId: string | undefined | null): string {
        if (!tableRateId) return '-';
        const tr = this.tableRateOptions.find(t => t.value === tableRateId);
        return tr?.label || '-';
    }

    getTableRateMenuItems(tableRate: ProProductTableRateDto): MenuItem[] {
        const cacheKey = tableRate.id || '';
        if (this.tableRateMenuItemsCache.has(cacheKey)) {
            return this.tableRateMenuItemsCache.get(cacheKey)!;
        }

        const items: MenuItem[] = [
            {
                label: this.localizationService.localize('Product::ProProduct:ViewDetails'),
                icon: 'pi pi-eye',
                command: () => {
                    this.onViewTableRateDetails(tableRate);
                }
            },
            {
                label: this.localizationService.localize('Product::ProProduct:EditTableRate'),
                icon: 'pi pi-pencil',
                command: () => {
                    this.onEditTableRate(tableRate);
                },
                disabled: !this.canEditTableRate(tableRate)
            },
            {
                label: this.localizationService.localize('Product::Delete'),
                icon: 'pi pi-trash',
                command: () => {
                    this.onDeleteTableRate(tableRate);
                },
                disabled: !this.canDeleteTableRate(tableRate)
            }
        ];

        this.tableRateMenuItemsCache.set(cacheKey, items);
        return items;
    }

    onViewTableRateDetails(tableRate: ProProductTableRateDto): void {
        // Open table rate management page in new tab with search filter
        const tableRateCode = this.getTableRateCode(tableRate.tableRateId);
        if (tableRateCode && tableRateCode !== '-') {
            const url = `/pages/product/pro-table-rates?code=${encodeURIComponent(tableRateCode)}`;
            window.open(url, '_blank');
        } else {
            this.messageService.add({
                severity: 'warn',
                summary: this.localizationService.localize('AbpUi::Warning'),
                detail: this.localizationService.localize('Product::ProProduct:TableRateCodeNotFound')
            });
        }
    }

    get isTableRateFormValid(): boolean {
        return !!this.selectedTableRateId && !!this.selectedTableRateEffectDate;
    }

    // Other Rule Tab Methods
    loadRuleTypeOptions(): void {
        const input: GetProRuleTypesInput = {
            skipCount: 0,
            maxResultCount: 1000,
            status: ProRuleTypeStatus.Active,
            sorting: 'name asc'
        };

        this.ruleTypeService.getList(input).subscribe({
            next: (result) => {
                this.ruleTypeOptions = (result.items || []).map(rt => ({
                    label: `${rt.code || ''} - ${rt.name || ''}`,
                    value: rt.id || '',
                    code: rt.code || ''
                }));
            },
            error: (err) => {
                console.error('Error loading rule types:', err);
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: err.error?.error?.message || 'Error loading rule types'
                });
                this.ruleTypeOptions = [];
            }
        });
    }

    loadProductRules(productId: string): void {
        const input: GetProRulesInput = {
            skipCount: 0,
            maxResultCount: 1000,
            applyTo: 'product',
            applyToId: productId,
            sorting: 'priority asc, creationTime desc'
        };

        this.ruleService.getList(input).subscribe({
            next: (result) => {
                this.productRules = result.items || [];
            },
            error: (err) => {
                console.error('Error loading product rules:', err);
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: err.error?.error?.message || 'Error loading product rules'
                });
                this.productRules = [];
            }
        });
    }

    onShowRuleForm(): void {
        this.ruleFormVisible = true;
        this.isRuleViewMode = false;
        this.selectedRuleTypeId = null;
        this.selectedRuleCode = '';
        this.selectedRuleName = '';
        this.selectedRulePriority = 1;
        this.selectedRuleStatus = ProRuleStatus.Active;
        const tomorrow = new Date();
        tomorrow.setDate(tomorrow.getDate() + 1);
        tomorrow.setHours(0, 0, 0, 0);
        this.selectedRuleEffectDate = tomorrow;
        this.selectedRuleExpireDate = null;
        this.selectedRuleScript = '';
        this.selectedRuleDescription = '';
        this.selectedRuleForEdit = null;
    }

    onCancelRuleForm(): void {
        this.confirmWithDefaults({
            message: this.localizationService.localize('Product::ProProduct:CancelActionConfirm'),
            header: this.localizationService.localize('Product::ProProduct:CancelAction'),
            icon: 'pi pi-exclamation-triangle',
            accept: () => {
                this.ruleFormVisible = false;
                this.resetRuleForm();
            }
        });
    }

    private resetRuleForm(): void {
        this.isRuleViewMode = false;
        this.selectedRuleTypeId = null;
        this.selectedRuleCode = '';
        this.selectedRuleName = '';
        this.selectedRulePriority = 1;
        this.selectedRuleStatus = ProRuleStatus.Active;
        this.selectedRuleEffectDate = null;
        this.selectedRuleExpireDate = null;
        this.selectedRuleScript = '';
        this.selectedRuleDescription = '';
        this.selectedRuleForEdit = null;
    }

    onSaveRule(): void {
        if (!this.validateRuleForm()) {
            return;
        }

        // Validation: Check if we're in create mode or have a selected product
        // Allow adding rules when creating a new product (selectedProduct is null but isInCreateMode is true)
        const isCreatingNewProduct = this.isCreateMode();
        const hasExistingProduct = this.selectedProduct?.id;

        if (!isCreatingNewProduct && !hasExistingProduct) {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Product::ProProduct:SelectProductFirst')
            });
            return;
        }

        if (this.selectedRuleForEdit) {
            // Update existing rule
            this.updateRule();
        } else {
            // Create new rule
            this.createRule();
        }
    }

    private validateRuleForm(): boolean {
        if (!this.selectedRuleTypeId) {
            this.messageService.add({
                severity: 'warn',
                summary: this.localizationService.localize('AbpUi::Warning'),
                detail: this.localizationService.localize('Product::ProRule:RuleTypeRequired')
            });
            return false;
        }

        if (!this.selectedRuleCode || this.selectedRuleCode.trim() === '') {
            this.messageService.add({
                severity: 'warn',
                summary: this.localizationService.localize('AbpUi::Warning'),
                detail: this.localizationService.localize('Product::ProRule:CodeRequired')
            });
            return false;
        }

        const ruleCodeRegex = /^[A-Z0-9_]+$/i;
        if (!ruleCodeRegex.test(this.selectedRuleCode.trim())) {
            this.messageService.add({
                severity: 'warn',
                summary: this.localizationService.localize('AbpUi::Warning'),
                detail: this.localizationService.localize('Product::ProRule:CodeFormatInvalid')
            });
            return false;
        }

        if (!this.selectedRuleName || this.selectedRuleName.trim() === '') {
            this.messageService.add({
                severity: 'warn',
                summary: this.localizationService.localize('AbpUi::Warning'),
                detail: this.localizationService.localize('Product::ProRule:NameRequired')
            });
            return false;
        }

        if (!this.selectedRuleEffectDate) {
            this.messageService.add({
                severity: 'warn',
                summary: this.localizationService.localize('AbpUi::Warning'),
                detail: this.localizationService.localize('Product::ProRule:EffectDateRequired')
            });
            return false;
        }

        // Validate dates (only for create mode, not edit mode)
        const today = new Date();
        today.setHours(0, 0, 0, 0);
        const effectDate = new Date(this.selectedRuleEffectDate);
        effectDate.setHours(0, 0, 0, 0);
        if (!this.selectedRuleForEdit) {
            if (effectDate < today) {
                this.messageService.add({
                    severity: 'warn',
                    summary: this.localizationService.localize('AbpUi::Warning'),
                    detail: this.localizationService.localize('Product::ProRule:EffectDateMustBeTodayOrLater')
                });
                return false;
            }
        }

        if (this.selectedRuleExpireDate) {
            const expireDate = new Date(this.selectedRuleExpireDate);
            expireDate.setHours(0, 0, 0, 0);
            if (expireDate < effectDate) {
                this.messageService.add({
                    severity: 'warn',
                    summary: this.localizationService.localize('AbpUi::Warning'),
                    detail: this.localizationService.localize('Product::ProRule:ExpireDateMustBeAfterEffectDate')
                });
                return false;
            }
        }

        if (!this.selectedRuleScript || this.selectedRuleScript.trim() === '') {
            this.messageService.add({
                severity: 'warn',
                summary: this.localizationService.localize('AbpUi::Warning'),
                detail: this.localizationService.localize('Product::ProRule:RuleScriptRequired')
            });
            return false;
        }

        return true;
    }

    private createRule(): void {
        // Check if we're in create mode (creating a new product)
        const isCreatingNewProduct = this.isCreateMode();
        const hasExistingProduct = this.selectedProduct?.id;

        if (isCreatingNewProduct && !hasExistingProduct) {
            // Store rule locally when creating a new product (will be saved after product is created)
            const newRule: ProRuleDto = {
                applyTo: 'product',
                applyToId: undefined, // Will be set when product is created
                ruleTypeId: this.selectedRuleTypeId!,
                code: this.selectedRuleCode.trim().toUpperCase(),
                name: this.selectedRuleName.trim(),
                description: this.selectedRuleDescription.trim() || undefined,
                ruleScript: this.selectedRuleScript.trim(),
                priority: this.selectedRulePriority || 1,
                status: this.selectedRuleStatus,
                effectDate: this.toLocalIsoDateTime(this.selectedRuleEffectDate!)!,
                expireDate: this.selectedRuleExpireDate ? (this.toLocalIsoDateTime(this.selectedRuleExpireDate) ?? undefined) : undefined
            };

            // Add to productRules array (will be saved with product)
            // Create new array reference to ensure change detection
            this.productRules = [...this.productRules, newRule];
            this.productRules.sort((a, b) => {
                if (a.priority !== b.priority) {
                    return (a.priority || 0) - (b.priority || 0);
                }
                return new Date(b.creationTime || 0).getTime() - new Date(a.creationTime || 0).getTime();
            });

            this.messageService.add({
                severity: 'success',
                summary: this.localizationService.localize('AbpUi::Success'),
                detail: this.localizationService.localize('Product::ProRule:RuleAdded')
            });

            this.ruleFormVisible = false;
            this.resetRuleForm();
            return;
        }

        // For existing products, call API immediately
        this.loading = true;

        const createDto: CreateProRuleDto = {
            applyTo: 'product',
            applyToId: this.selectedProduct!.id!,
            ruleTypeId: this.selectedRuleTypeId!,
            code: this.selectedRuleCode.trim().toUpperCase(),
            name: this.selectedRuleName.trim(),
            description: this.selectedRuleDescription.trim() || undefined,
            ruleScript: this.selectedRuleScript.trim(),
            priority: this.selectedRulePriority || 1,
            status: this.selectedRuleStatus,
            effectDate: this.toLocalIsoDateTime(this.selectedRuleEffectDate!)!,
            expireDate: this.selectedRuleExpireDate ? (this.toLocalIsoDateTime(this.selectedRuleExpireDate) ?? undefined) : undefined
        };

        this.ruleService.create(createDto).subscribe({
            next: (createdRule) => {
                this.messageService.add({
                    severity: 'success',
                    summary: this.localizationService.localize('AbpUi::Success'),
                    detail: this.localizationService.localize('Product::ProRule:RuleAdded')
                });
                this.productRules.push(createdRule);
                this.productRules.sort((a, b) => {
                    if (a.priority !== b.priority) {
                        return (a.priority || 0) - (b.priority || 0);
                    }
                    return new Date(b.creationTime || 0).getTime() - new Date(a.creationTime || 0).getTime();
                });
                this.ruleFormVisible = false;
                this.resetRuleForm();
                this.loading = false;
            },
            error: (err) => {
                this.loading = false;
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: err.error?.error?.message || this.localizationService.localize('Product::ProRule:ErrorAddingRule')
                });
            }
        });
    }

    private updateRule(): void {
        if (!this.selectedRuleForEdit) return;
        
        const ruleToEdit = this.selectedRuleForEdit;

        // Handle local rules (without id) when in create mode
        if (!ruleToEdit.id) {
            if (this.isCreateMode()) {
                // Update rule in local array - match by original properties before edit
                const originalCode = ruleToEdit.code;
                const originalRuleTypeId = ruleToEdit.ruleTypeId;
                const originalEffectDate = ruleToEdit.effectDate;
                
                const index = this.productRules.findIndex(r => 
                    !r.id &&
                    r.code === originalCode && 
                    r.ruleTypeId === originalRuleTypeId &&
                    r.effectDate === originalEffectDate
                );
                
                if (index >= 0) {
                    const canEditAll = this.canEditRuleAllFields(ruleToEdit);
                    const updatedRule: ProRuleDto = {
                        ...ruleToEdit,
                        ruleTypeId: canEditAll ? this.selectedRuleTypeId! : ruleToEdit.ruleTypeId!,
                        code: this.selectedRuleCode.trim().toUpperCase(),
                        name: canEditAll ? this.selectedRuleName.trim() : ruleToEdit.name!,
                        description: this.selectedRuleDescription.trim() || undefined,
                        ruleScript: canEditAll ? this.selectedRuleScript.trim() : ruleToEdit.ruleScript!,
                        priority: canEditAll ? this.selectedRulePriority : ruleToEdit.priority,
                        status: canEditAll ? this.selectedRuleStatus : ruleToEdit.status!,
                        effectDate: canEditAll ? this.toLocalIsoDateTime(this.selectedRuleEffectDate!)! : ruleToEdit.effectDate!,
                        expireDate: this.selectedRuleExpireDate ? (this.toLocalIsoDateTime(this.selectedRuleExpireDate) ?? undefined) : undefined
                    };
                    
                    this.productRules[index] = updatedRule;
                    this.productRules.sort((a, b) => {
                        if (a.priority !== b.priority) {
                            return (a.priority || 0) - (b.priority || 0);
                        }
                        return new Date(b.creationTime || 0).getTime() - new Date(a.creationTime || 0).getTime();
                    });
                    
                    this.messageService.add({
                        severity: 'success',
                        summary: this.localizationService.localize('AbpUi::Success'),
                        detail: this.localizationService.localize('Product::ProRule:RuleUpdated')
                    });
                    
                    this.ruleFormVisible = false;
                    this.resetRuleForm();
                }
                return;
            }
            return;
        }

        // At this point, ruleToEdit.id must be defined (we returned early if it wasn't)
        if (!ruleToEdit.id) {
            return;
        }

        this.loading = true;

        const canEditAll = this.canEditRuleAllFields(ruleToEdit);
        const updateDto: UpdateProRuleDto = {
            applyTo: 'product',
            applyToId: this.selectedProduct!.id!,
            ruleTypeId: canEditAll ? this.selectedRuleTypeId! : ruleToEdit.ruleTypeId!,
            name: canEditAll ? this.selectedRuleName.trim() : ruleToEdit.name!,
            description: this.selectedRuleDescription.trim() || undefined,
            ruleScript: canEditAll ? this.selectedRuleScript.trim() : ruleToEdit.ruleScript!,
            priority: canEditAll ? this.selectedRulePriority : ruleToEdit.priority,
            status: canEditAll ? this.selectedRuleStatus : ruleToEdit.status!,
            effectDate: canEditAll ? this.toLocalIsoDateTime(this.selectedRuleEffectDate!)! : ruleToEdit.effectDate!,
            expireDate: this.selectedRuleExpireDate ? (this.toLocalIsoDateTime(this.selectedRuleExpireDate) ?? undefined) : undefined
        };

        this.ruleService.update(ruleToEdit.id, updateDto).subscribe({
            next: (updatedRule) => {
                this.messageService.add({
                    severity: 'success',
                    summary: this.localizationService.localize('AbpUi::Success'),
                    detail: this.localizationService.localize('Product::ProRule:RuleUpdated')
                });
                const index = this.productRules.findIndex(r => r.id === updatedRule.id);
                if (index >= 0) {
                    this.productRules[index] = updatedRule;
                    this.productRules.sort((a, b) => {
                        if (a.priority !== b.priority) {
                            return (a.priority || 0) - (b.priority || 0);
                        }
                        return new Date(b.creationTime || 0).getTime() - new Date(a.creationTime || 0).getTime();
                    });
                }
                this.ruleFormVisible = false;
                this.resetRuleForm();
                this.loading = false;
            },
            error: (err) => {
                this.loading = false;
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: err.error?.error?.message || this.localizationService.localize('Product::ProRule:ErrorUpdatingRule')
                });
            }
        });
    }

    /**
     * Save pending rules that were created during product creation mode
     * Rules without an id are temporary and need to be saved via API
     */
    private savePendingRules(productId: string): Observable<void> {
        // Find all rules without an id (temporary rules created during create mode)
        const pendingRules = this.productRules.filter(rule => !rule.id);

        if (pendingRules.length === 0) {
            // No pending rules to save
            return of(void 0);
        }

        // Create API calls for each pending rule
        const ruleCreateObservables = pendingRules.map((rule) => {
            const createDto: CreateProRuleDto = {
                applyTo: 'product',
                applyToId: productId,
                ruleTypeId: rule.ruleTypeId!,
                code: rule.code!,
                name: rule.name!,
                description: rule.description || undefined,
                ruleScript: rule.ruleScript!,
                priority: rule.priority || 1,
                status: rule.status ?? ProRuleStatus.Active,
                effectDate: rule.effectDate!,
                expireDate: rule.expireDate || undefined
            };

            return this.ruleService.create(createDto).pipe(
                map((createdRule) => ({ originalRule: rule, createdRule })),
                catchError((err) => {
                    console.error(`Error saving pending rule ${rule.code}:`, err);
                    this.messageService.add({
                        severity: 'error',
                        summary: this.localizationService.localize('AbpUi::Error'),
                        detail: err.error?.error?.message || this.localizationService.localize('Product::ProRule:ErrorAddingRule')
                    });
                    // Return null for failed rules
                    return of({ originalRule: rule, createdRule: null });
                })
            );
        });

        // Execute all rule creation calls in parallel
        return forkJoin(ruleCreateObservables).pipe(
            map((results) => {
                // Replace temporary rules with created ones
                results.forEach(({ originalRule, createdRule }) => {
                    if (createdRule) {
                        // Find the original rule index in productRules by matching code and ruleTypeId
                        const originalIndex = this.productRules.findIndex(r => 
                            !r.id && 
                            r.code === originalRule.code &&
                            r.ruleTypeId === originalRule.ruleTypeId &&
                            r.name === originalRule.name
                        );
                        if (originalIndex >= 0) {
                            this.productRules[originalIndex] = createdRule;
                        }
                    }
                });

                // Sort rules after updates
                this.productRules.sort((a, b) => {
                    if (a.priority !== b.priority) {
                        return (a.priority || 0) - (b.priority || 0);
                    }
                    return new Date(b.creationTime || 0).getTime() - new Date(a.creationTime || 0).getTime();
                });

                return void 0;
            })
        );
    }

    private savePendingPlanDefinitions(productId: string): Observable<void> {
        // Find all plan definitions without an id (temporary plans created during create mode)
        const pendingPlans = this.productPlanDefinitions.filter(plan => !plan.id);

        if (pendingPlans.length === 0) {
            // No pending plan definitions to save
            return of(void 0);
        }

        // Create API calls for each pending plan definition
        const planCreateObservables = pendingPlans.map((plan) => {
            const createDto: CreateProProductPlanDefinitionDto = {
                productId: productId,
                planCode: (plan.planCode || '').trim().toUpperCase(),
                planName: (plan.planName || '').trim(),
                status: plan.status || ProProductPlanDefinitionStatus.Active
            };

            return this.planDefinitionService.create(createDto).pipe(
                map((createdPlan) => ({ originalPlan: plan, createdPlan })),
                catchError((err) => {
                    console.error(`Error saving pending plan definition ${plan.planCode}:`, err);
                    this.messageService.add({
                        severity: 'error',
                        summary: this.localizationService.localize('AbpUi::Error'),
                        detail: err.error?.error?.message || this.localizationService.localize('Product::ProProductPlanDefinition:ErrorAddingPlan')
                    });
                    // Return null for failed plans
                    return of({ originalPlan: plan, createdPlan: null });
                })
            );
        });

        // Execute all plan definition creation calls in parallel
        return forkJoin(planCreateObservables).pipe(
            map((results) => {
                // Replace temporary plan definitions with created ones
                results.forEach(({ originalPlan, createdPlan }) => {
                    if (createdPlan) {
                        // Find the original plan index in productPlanDefinitions by matching planCode
                        const originalIndex = this.productPlanDefinitions.findIndex(p => 
                            !p.id && 
                            p.planCode === originalPlan.planCode &&
                            p.planName === originalPlan.planName
                        );
                        if (originalIndex >= 0) {
                            this.productPlanDefinitions[originalIndex] = createdPlan;
                        }
                    }
                });

                return void 0;
            })
        );
    }

    /**
     * True when the clicked row is the same rule as the one currently shown in the rule form
     * (avoids rehydrating from productRules and wiping unsaved selectedRule* edits).
     */
    private isSameRuleAsOpenForm(rule: ProRuleDto): boolean {
        if (!this.ruleFormVisible || !this.selectedRuleForEdit) {
            return false;
        }
        const open = this.selectedRuleForEdit;
        if (rule.id && open.id) {
            return rule.id === open.id;
        }
        if (!rule.id && !open.id) {
            return (
                rule.code === open.code &&
                rule.ruleTypeId === open.ruleTypeId &&
                rule.effectDate === open.effectDate &&
                rule.name === open.name
            );
        }
        return false;
    }

    onEditRule(rule: ProRuleDto): void {
        if (!this.canEditRule(rule)) {
            this.messageService.add({
                severity: 'warn',
                summary: this.localizationService.localize('AbpUi::Warning'),
                detail: this.localizationService.localize('Product::ProRule:CannotEditRule')
            });
            return;
        }

        if (this.isSameRuleAsOpenForm(rule)) {
            this.isRuleViewMode = false;
            return;
        }

        this.selectedRuleForEdit = { ...rule };
        this.isRuleViewMode = false;
        this.selectedRuleTypeId = rule.ruleTypeId || null;
        this.selectedRuleCode = rule.code || '';
        this.selectedRuleName = rule.name || '';
        this.selectedRulePriority = rule.priority || 1;
        this.selectedRuleStatus = rule.status ?? ProRuleStatus.Active;
        this.selectedRuleEffectDate = rule.effectDate ? new Date(rule.effectDate) : new Date();
        this.selectedRuleExpireDate = rule.expireDate ? new Date(rule.expireDate) : null;
        this.selectedRuleScript = rule.ruleScript || '';
        this.selectedRuleDescription = rule.description || '';
        this.ruleFormVisible = true;
    }

    onDeleteRule(rule: ProRuleDto): void {
        if (!this.canDeleteRule(rule)) {
            this.messageService.add({
                severity: 'warn',
                summary: this.localizationService.localize('AbpUi::Warning'),
                detail: this.localizationService.localize('Product::ProRule:CannotDeleteRuleWhenActive')
            });
            return;
        }

        this.confirmWithDefaults({
            message: this.localizationService.localize('Product::ProRule:DeleteRuleConfirm'),
            header: this.localizationService.localize('Product::Delete'),
            icon: 'pi pi-exclamation-triangle',
            accept: () => {
                // Handle local rules (without id) when in create mode
                if (!rule.id) {
                    if (this.isCreateMode()) {
                        // Remove from local array - match by multiple properties to ensure correct rule
                        const index = this.productRules.findIndex(r => 
                            !r.id &&
                            r.code === rule.code && 
                            r.ruleTypeId === rule.ruleTypeId &&
                            r.effectDate === rule.effectDate &&
                            r.name === rule.name
                        );
                        if (index >= 0) {
                            this.productRules = this.productRules.filter((_, i) => i !== index);
                            this.messageService.add({
                                severity: 'success',
                                summary: this.localizationService.localize('AbpUi::Success'),
                                detail: this.localizationService.localize('Product::ProRule:RuleDeleted')
                            });
                        }
                        return;
                    }
                    return;
                }

                this.loading = true;
                this.ruleService.delete(rule.id).subscribe({
                    next: () => {
                        this.messageService.add({
                            severity: 'success',
                            summary: this.localizationService.localize('AbpUi::Success'),
                            detail: this.localizationService.localize('Product::ProRule:RuleDeleted')
                        });
                        const index = this.productRules.findIndex(r => r.id === rule.id);
                        if (index >= 0) {
                            this.productRules.splice(index, 1);
                        }
                        this.loading = false;
                    },
                    error: (err) => {
                        this.loading = false;
                        this.messageService.add({
                            severity: 'error',
                            summary: this.localizationService.localize('AbpUi::Error'),
                            detail: err.error?.error?.message || this.localizationService.localize('Product::ProRule:ErrorDeletingRule')
                        });
                    }
                });
            }
        });
    }

    onViewRuleDetails(rule: ProRuleDto): void {
        if (this.isSameRuleAsOpenForm(rule)) {
            this.isRuleViewMode = true;
            return;
        }

        // Load rule details and show in form (read-only)
        this.selectedRuleForEdit = { ...rule };
        this.isRuleViewMode = true;
        this.selectedRuleTypeId = rule.ruleTypeId || null;
        this.selectedRuleCode = rule.code || '';
        this.selectedRuleName = rule.name || '';
        this.selectedRulePriority = rule.priority || 1;
        this.selectedRuleStatus = rule.status ?? ProRuleStatus.Active;
        this.selectedRuleEffectDate = rule.effectDate ? new Date(rule.effectDate) : new Date();
        this.selectedRuleExpireDate = rule.expireDate ? new Date(rule.expireDate) : null;
        this.selectedRuleScript = rule.ruleScript || '';
        this.selectedRuleDescription = rule.description || '';
        this.ruleFormVisible = true;
    }

    canEditRule(rule: ProRuleDto): boolean {
        // No editing allowed in detail mode
        if (this.isDetailMode()) return false;
        
        // Allow editing when in create mode (rules are stored locally)
        if (this.isCreateMode()) return true;
        
        if (!this.selectedProduct) return false;
        
        // Can always edit, but validation will restrict fields if product is active and effect date >= today
        return true;
    }

    canEditRuleAllFields(rule: ProRuleDto): boolean {
        // No editing allowed in detail mode
        if (this.isDetailMode()) return false;
        
        // Allow editing all fields when in create mode (new products are essentially in draft state)
        if (this.isCreateMode()) return true;
        
        if (!this.selectedProduct) return false;
        
        // Allow all fields when product is draft or current date < effectiveDate (product not yet active)
        return !this.isProductActiveForRestriction();
    }

    canDeleteRule(rule: ProRuleDto): boolean {
        // No deleting allowed in detail mode
        if (this.isDetailMode()) return false;
        
        // Allow deleting when in create mode (rules are stored locally)
        if (this.isCreateMode()) return true;
        
        if (!this.selectedProduct) return false;
        
        // Allow delete when product is draft or current date < effectiveDate (product not yet active)
        return !this.isProductActiveForRestriction();
    }

    getRuleTypeName(ruleTypeId: string | undefined): string {
        if (!ruleTypeId) return '-';
        const type = this.ruleTypeOptions.find(t => t.value === ruleTypeId);
        return type?.label || '-';
    }

    getRuleStatusLabel(status: ProRuleStatus | undefined): string {
        if (status === undefined) return '-';
        const option = this.ruleStatusOptions.find(o => o.value === status);
        return option?.label || '-';
    }

    getRuleMenuItems(rule: ProRuleDto): MenuItem[] {
        return [
            {
                label: this.localizationService.localize('Product::ProRule:ViewDetails'),
                icon: 'pi pi-eye',
                command: () => {
                    this.onViewRuleDetails(rule);
                }
            },
            {
                label: this.localizationService.localize('Product::ProRule:Edit'),
                icon: 'pi pi-pencil',
                command: () => {
                    this.onEditRule(rule);
                },
                disabled: !this.canEditRule(rule)
            },
            {
                label: this.localizationService.localize('Product::ProRule:Delete'),
                icon: 'pi pi-trash',
                command: () => {
                    this.onDeleteRule(rule);
                },
                disabled: !this.canDeleteRule(rule)
            }
        ];
    }

    get isRuleFormValid(): boolean {
        return !!this.selectedRuleTypeId && 
               !!this.selectedRuleCode && 
               this.selectedRuleCode.trim() !== '' &&
               !!this.selectedRuleName && 
               this.selectedRuleName.trim() !== '' &&
               !!this.selectedRuleEffectDate &&
               !!this.selectedRuleScript &&
               this.selectedRuleScript.trim() !== '';
    }

    // Package Definition Tab Methods
    get isPackageDefinitionTabEnabled(): boolean {
        // In create mode, check formData.partnerId
        // In edit mode, check selectedProduct.partnerId
        const partnerId = this.selectedProduct?.partnerId ?? this.formData?.partnerId;
        return !partnerId;
    }

    /**
     * Add/edit/delete package definitions: allowed in create mode, for Draft/Deactive products,
     * or when the product is not yet "active" per isProductActiveForRestriction().
     * Considers unsaved form status so the status dropdown affects control availability.
     */
    get canMutatePlanDefinitions(): boolean {
        if (this.isDetailMode()) {
            return false;
        }
        if (this.isCreateMode() || !this.selectedProduct) {
            return true;
        }
        if (this.isProductDraftOrDeactive(this.selectedProduct.status) ||
            this.isProductDraftOrDeactive(this.formData?.status)) {
            return true;
        }
        return !this.isProductActiveForRestriction();
    }

    private notifyPlanDefinitionMutateBlocked(): void {
        this.messageService.add({
            severity: 'warn',
            summary: this.localizationService.localize('AbpUi::Warning'),
            detail: this.localizationService.localize('Product::ProProductPlanDefinition:MutateNotAllowed')
        });
    }

    onAddPlanDefinition(): void {
        if (!this.canMutatePlanDefinitions) {
            this.notifyPlanDefinitionMutateBlocked();
            return;
        }
        // Create a new temporary plan definition for inline editing
        const newPlan: ProProductPlanDefinitionDto = {
            id: undefined,
            planCode: '',
            planName: '',
            status: ProProductPlanDefinitionStatus.Active,
            productId: this.selectedProduct?.id
        };

        this.productPlanDefinitions.unshift(newPlan);
        this.editingPlanDefinition = { ...newPlan };
        this.editingPlanDefinitionIndex = 0;
        this.planDefinitionFormVisible = true;
    }

    onEditPlanDefinitionInline(plan: ProProductPlanDefinitionDto, index: number): void {
        if (!this.canMutatePlanDefinitions) {
            this.notifyPlanDefinitionMutateBlocked();
            return;
        }
        this.editingPlanDefinition = { ...plan };
        this.editingPlanDefinitionIndex = index;
        this.planDefinitionFormVisible = true;
    }

    onCancelPlanDefinitionEdit(): void {
        if (this.editingPlanDefinitionIndex >= 0 && !this.editingPlanDefinition?.id) {
            // Remove the temporary new row
            this.productPlanDefinitions.splice(this.editingPlanDefinitionIndex, 1);
        }
        this.editingPlanDefinition = null;
        this.editingPlanDefinitionIndex = -1;
        this.planDefinitionFormVisible = false;
    }

    onSavePlanDefinition(): void {
        if (!this.editingPlanDefinition) return;

        if (!this.canMutatePlanDefinitions) {
            this.notifyPlanDefinitionMutateBlocked();
            return;
        }

        // Validation: Check if we're in create mode or have a selected product
        const isCreatingNewProduct = this.isCreateMode();
        const hasExistingProduct = this.selectedProduct?.id;

        if (!isCreatingNewProduct && !hasExistingProduct) {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Product::ProProduct:SelectProductFirst')
            });
            return;
        }

        // Validation
        if (!this.editingPlanDefinition.planCode || this.editingPlanDefinition.planCode.trim() === '') {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Product::ProProductPlanDefinition:CodeRequired')
            });
            return;
        }

        // Validate code format: A-Z, 0-9, _ only
        const codeRegex = /^[A-Z0-9_]+$/;
        if (!codeRegex.test(this.editingPlanDefinition.planCode.toUpperCase())) {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Product::ProProductPlanDefinition:CodeFormatInvalid')
            });
            return;
        }

        if (!this.editingPlanDefinition.planName || this.editingPlanDefinition.planName.trim() === '') {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Product::ProProductPlanDefinition:NameRequired')
            });
            return;
        }

        // Check code uniqueness (excluding current item)
        const existingPlan = this.productPlanDefinitions.find(
            (p, idx) => p.planCode?.toUpperCase() === this.editingPlanDefinition!.planCode?.toUpperCase() 
                     && idx !== this.editingPlanDefinitionIndex
                     && p.id !== this.editingPlanDefinition!.id
        );
        if (existingPlan) {
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Product::ProProductPlanDefinition:CodeExists')
            });
            return;
        }

        if (this.editingPlanDefinition.id) {
            // Update existing
            this.updatePlanDefinition();
        } else {
            // Create new
            this.createPlanDefinition();
        }
    }

    private createPlanDefinition(): void {
        if (!this.editingPlanDefinition) return;

        // Check if we're in create mode (creating a new product)
        const isCreatingNewProduct = this.isCreateMode();
        const hasExistingProduct = this.selectedProduct?.id;

        if (isCreatingNewProduct && !hasExistingProduct) {
            // Store plan definition locally when creating a new product (will be saved after product is created)
            const newPlan: ProProductPlanDefinitionDto = {
                id: undefined,
                productId: undefined, // Will be set when product is created
                planCode: (this.editingPlanDefinition.planCode || '').trim().toUpperCase(),
                planName: (this.editingPlanDefinition.planName || '').trim(),
                status: this.editingPlanDefinition.status || ProProductPlanDefinitionStatus.Active
            };

            // Update the temporary item in the array
            if (this.editingPlanDefinitionIndex >= 0) {
                this.productPlanDefinitions[this.editingPlanDefinitionIndex] = newPlan;
            } else {
                // If index is invalid, add to array
                this.productPlanDefinitions.unshift(newPlan);
            }

            this.messageService.add({
                severity: 'success',
                summary: this.localizationService.localize('AbpUi::Success'),
                detail: this.localizationService.localize('Product::ProProductPlanDefinition:PlanAdded')
            });

            this.editingPlanDefinition = null;
            this.editingPlanDefinitionIndex = -1;
            this.planDefinitionFormVisible = false;
            return;
        }

        // For existing products, call API immediately
        if (!hasExistingProduct || !this.selectedProduct?.id) return;

        const createDto: CreateProProductPlanDefinitionDto = {
            productId: this.selectedProduct.id!,
            planCode: (this.editingPlanDefinition.planCode || '').trim().toUpperCase(),
            planName: (this.editingPlanDefinition.planName || '').trim(),
            status: this.editingPlanDefinition.status || ProProductPlanDefinitionStatus.Active
        };

        this.loading = true;
        this.planDefinitionService.create(createDto).subscribe({
            next: (createdPlan) => {
                this.messageService.add({
                    severity: 'success',
                    summary: this.localizationService.localize('AbpUi::Success'),
                    detail: this.localizationService.localize('Product::ProProductPlanDefinition:PlanAdded')
                });
                
                // Replace the temporary item with the created one
                if (this.editingPlanDefinitionIndex >= 0) {
                    this.productPlanDefinitions[this.editingPlanDefinitionIndex] = createdPlan;
                }
                
                this.editingPlanDefinition = null;
                this.editingPlanDefinitionIndex = -1;
                this.planDefinitionFormVisible = false;
                this.loading = false;
            },
            error: (err) => {
                this.loading = false;
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: err.error?.error?.message || this.localizationService.localize('Product::ProProductPlanDefinition:ErrorAddingPlan')
                });
            }
        });
    }

    private updatePlanDefinition(): void {
        if (!this.editingPlanDefinition?.id || !this.selectedProduct?.id) return;

        const updateDto: UpdateProProductPlanDefinitionDto = {
            productId: this.selectedProduct.id,
            planName: (this.editingPlanDefinition.planName || '').trim(),
            status: this.editingPlanDefinition.status || ProProductPlanDefinitionStatus.Active
        };

        this.planDefinitionService.update(this.editingPlanDefinition.id, updateDto).subscribe({
            next: (updatedPlan) => {
                this.messageService.add({
                    severity: 'success',
                    summary: this.localizationService.localize('AbpUi::Success'),
                    detail: this.localizationService.localize('Product::ProProductPlanDefinition:PlanUpdated')
                });
                
                // Update the item in the array
                if (this.editingPlanDefinitionIndex >= 0) {
                    this.productPlanDefinitions[this.editingPlanDefinitionIndex] = updatedPlan;
                }
                
                this.editingPlanDefinition = null;
                this.editingPlanDefinitionIndex = -1;
                this.planDefinitionFormVisible = false;
                this.loading = false;
            },
            error: (err) => {
                this.loading = false;
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpUi::Error'),
                    detail: err.error?.error?.message || this.localizationService.localize('Product::ProProductPlanDefinition:ErrorUpdatingPlan')
                });
            }
        });
    }

    private findPlanDefinitionIndex(plan: ProProductPlanDefinitionDto): number {
        if (plan.id) {
            const indexById = this.productPlanDefinitions.findIndex(p => p.id === plan.id);
            if (indexById >= 0) {
                return indexById;
            }
        }

        return this.productPlanDefinitions.findIndex(p => p === plan);
    }

    onDeletePlanDefinition(plan: ProProductPlanDefinitionDto): void {
        if (!this.canMutatePlanDefinitions) {
            this.notifyPlanDefinitionMutateBlocked();
            return;
        }

        this.confirmWithDefaults({
            message: this.localizationService.localize('Product::ProProductPlanDefinition:DeletePlanConfirm'),
            header: this.localizationService.localize('Product::Delete'),
            icon: 'pi pi-exclamation-triangle',
            accept: () => {
                const index = this.findPlanDefinitionIndex(plan);

                if (!plan.id) {
                    if (index >= 0) {
                        this.productPlanDefinitions.splice(index, 1);
                    }

                    if (this.editingPlanDefinitionIndex === index) {
                        this.editingPlanDefinition = null;
                        this.editingPlanDefinitionIndex = -1;
                        this.planDefinitionFormVisible = false;
                    }

                    this.messageService.add({
                        severity: 'success',
                        summary: this.localizationService.localize('AbpUi::Success'),
                        detail: this.localizationService.localize('Product::ProProductPlanDefinition:PlanDeleted')
                    });
                    return;
                }

                this.loading = true;
                this.planDefinitionService.delete(plan.id!).subscribe({
                    next: () => {
                        this.messageService.add({
                            severity: 'success',
                            summary: this.localizationService.localize('AbpUi::Success'),
                            detail: this.localizationService.localize('Product::ProProductPlanDefinition:PlanDeleted')
                        });
                        if (index >= 0) {
                            this.productPlanDefinitions.splice(index, 1);
                        }
                        this.loading = false;
                    },
                    error: (err) => {
                        this.loading = false;
                        const errorMessage = err.error?.error?.message || '';
                        this.messageService.add({
                            severity: 'error',
                            summary: this.localizationService.localize('AbpUi::Error'),
                            detail: errorMessage || this.localizationService.localize('Product::ProProductPlanDefinition:ErrorDeletingPlan')
                        });
                    }
                });
            }
        });
    }

    getPlanDefinitionStatusLabel(status: ProProductPlanDefinitionStatus | undefined): string {
        if (status === undefined) return '-';
        const option = this.planDefinitionStatusOptions.find(o => o.value === status);
        return option?.label || '-';
    }

    getPlanDefinitionMenuItems(plan: ProProductPlanDefinitionDto): MenuItem[] {
        const disabled = !this.canMutatePlanDefinitions;
        return [
            {
                label: this.localizationService.localize('Product::Edit'),
                icon: 'pi pi-pencil',
                disabled,
                command: () => {
                    const index = this.findPlanDefinitionIndex(plan);
                    if (index >= 0) {
                        this.onEditPlanDefinitionInline(plan, index);
                    }
                }
            },
            {
                label: this.localizationService.localize('Product::Delete'),
                icon: 'pi pi-trash',
                disabled,
                command: () => {
                    this.onDeletePlanDefinition(plan);
                }
            }
        ];
    }

    // Product Distribution (Bán hàng) Tab Methods
    private resetProductDistributionState(): void {
        this.productDistributions = [];
        this.selectedChannelIds = [];
        this.selectedAppChannelIds = [];
        this.selectedEmployeeRoleIds = [];
    }

    loadEmployeeRoles(): void {
        const input: GetHrEmployeeRolesInput = {
            skipCount: 0,
            maxResultCount: 1000,
            sorting: 'name asc'
        };

        this.hrEmployeeRoleService.getList(input).subscribe({
            next: (result) => {
                const items = result.items || [];
                this.allEmployeeRoleOptions = items.map((r) => ({
                    label: (r.name || r.code || '').trim() || (r.id ?? ''),
                    value: r.id || ''
                }));
                this.updateEmployeeRoleSelections();
            },
            error: (err) => {
                console.error('Error loading employee roles:', err);
                this.allEmployeeRoleOptions = [];
            }
        });
    }

    loadChannels(): void {
        const input: GetResChannelsInput = {
            status: ResChannelStatus.Active,
            maxResultCount: 1000,
            skipCount: 0,
            sorting: 'name asc'
        };

        this.channelService.getList(input).subscribe({
            next: (result) => {
                this.allChannelOptions = (result.items || []).map(channel => ({
                    label: channel.name || channel.code || '',
                    value: channel.id || ''
                }));
                this.updateChannelSelections();
            },
            error: (err) => {
                console.error('Error loading channels:', err);
                this.allChannelOptions = [];
            }
        });
    }

    loadAppChannels(): void {
        const input: GetResAppChannelsInput = {
            status: ResAppChannelStatus.Active,
            maxResultCount: 1000,
            skipCount: 0,
            sorting: 'name asc'
        };

        this.appChannelService.getList(input).subscribe({
            next: (result) => {
                this.allAppChannelOptions = (result.items || []).map(appChannel => ({
                    label: appChannel.name || appChannel.code || '',
                    value: appChannel.id || ''
                }));
                this.updateAppChannelSelections();
            },
            error: (err) => {
                console.error('Error loading app channels:', err);
                this.allAppChannelOptions = [];
            }
        });
    }

    loadProductDistributions(product: ProProductDto): void {
        const raw = product.productDistributions || [];
        this.productDistributions = raw.map((row) => ({ ...(row as ProProductDistributionDto) }));

        this.selectedChannelIds = this.productDistributions
            .filter(dist => dist.channelId)
            .map(dist => dist.channelId!)
            .filter((id, index, self) => self.indexOf(id) === index);

        this.selectedAppChannelIds = this.productDistributions
            .filter(dist => dist.appChannelId)
            .map(dist => dist.appChannelId!)
            .filter((id, index, self) => self.indexOf(id) === index);

        this.selectedEmployeeRoleIds = this.productDistributions
            .filter(dist => dist.employeeRoleId && !dist.channelId && !dist.appChannelId)
            .map(dist => dist.employeeRoleId!)
            .filter((id, index, self) => self.indexOf(id) === index);

        this.updateChannelSelections();
        this.updateAppChannelSelections();
        this.updateEmployeeRoleSelections();
    }

    private updateChannelSelections(): void {
        // This will be called when channels are loaded or product is selected
        // The dual listbox component will handle the rest via writeValue
    }

    private updateAppChannelSelections(): void {
        // This will be called when app channels are loaded or product is selected
        // The dual listbox component will handle the rest via writeValue
    }

    private updateEmployeeRoleSelections(): void {
        // This will be called when employee roles are loaded or product is selected
        // The dual listbox component will handle the rest via writeValue
    }

    onChannelSelectionChange(selectedOptions: DualListboxOption[]): void {
        this.selectedChannelIds = selectedOptions.map(o => o.value);
        this.syncProductDistributionsFromSelection();
    }

    onAppChannelSelectionChange(selectedOptions: DualListboxOption[]): void {
        this.selectedAppChannelIds = selectedOptions.map(o => o.value);
        this.syncProductDistributionsFromSelection();
    }

    onEmployeeRoleSelectionChange(selectedOptions: DualListboxOption[]): void {
        this.selectedEmployeeRoleIds = selectedOptions.map(o => o.value);
        this.syncProductDistributionsFromSelection();
    }

    /**
     * Keeps this.productDistributions in sync with the dual listbox selections
     * for channels, app channels, and employee roles (each as separate rows).
     */
    private syncProductDistributionsFromSelection(): void {
        const productId: string = this.selectedProduct?.id || '';

        // Keep existing channel distributions that are still selected
        const existingChannels = this.productDistributions.filter(
            d => d.channelId && this.selectedChannelIds.includes(d.channelId)
        );
        // Keep existing app channel distributions that are still selected
        const existingAppChannels = this.productDistributions.filter(
            d => d.appChannelId && this.selectedAppChannelIds.includes(d.appChannelId)
        );
        // Keep existing role-only distributions that are still selected
        const existingRoles = this.productDistributions.filter(
            d => d.employeeRoleId && !d.channelId && !d.appChannelId && this.selectedEmployeeRoleIds.includes(d.employeeRoleId)
        );

        const existingChannelIds = new Set(existingChannels.map(d => d.channelId!));
        const newChannelDtos: ProProductDistributionDto[] = this.selectedChannelIds
            .filter(id => !existingChannelIds.has(id))
            .map(channelId => ({
                productId,
                channelId,
                appChannelId: undefined,
                employeeRoleId: undefined,
                status: ProProductDistributionStatus.Active
            } as ProProductDistributionDto));

        const existingAppChannelIds = new Set(existingAppChannels.map(d => d.appChannelId!));
        const newAppChannelDtos: ProProductDistributionDto[] = this.selectedAppChannelIds
            .filter(id => !existingAppChannelIds.has(id))
            .map(appChannelId => ({
                productId,
                channelId: undefined,
                appChannelId,
                employeeRoleId: undefined,
                status: ProProductDistributionStatus.Active
            } as ProProductDistributionDto));

        const existingRoleIds = new Set(existingRoles.map(d => d.employeeRoleId!));
        const newRoleDtos: ProProductDistributionDto[] = this.selectedEmployeeRoleIds
            .filter(id => !existingRoleIds.has(id))
            .map(employeeRoleId => ({
                productId,
                channelId: undefined,
                appChannelId: undefined,
                employeeRoleId,
                status: ProProductDistributionStatus.Active
            } as ProProductDistributionDto));

        this.productDistributions = [
            ...existingChannels, ...newChannelDtos,
            ...existingAppChannels, ...newAppChannelDtos,
            ...existingRoles, ...newRoleDtos
        ];
    }

    getChannelOptions(): DualListboxOption[] {
        return this.allChannelOptions;
    }

    getSelectedChannelOptions(): DualListboxOption[] {
        return this.allChannelOptions.filter(opt => this.selectedChannelIds.includes(opt.value));
    }

    getAvailableChannelOptions(): DualListboxOption[] {
        return this.allChannelOptions.filter(opt => !this.selectedChannelIds.includes(opt.value));
    }

    getAppChannelOptions(): DualListboxOption[] {
        return this.allAppChannelOptions;
    }

    getSelectedAppChannelOptions(): DualListboxOption[] {
        return this.allAppChannelOptions.filter(opt => this.selectedAppChannelIds.includes(opt.value));
    }

    getAvailableAppChannelOptions(): DualListboxOption[] {
        return this.allAppChannelOptions.filter(opt => !this.selectedAppChannelIds.includes(opt.value));
    }

    getSelectedEmployeeRoleOptions(): DualListboxOption[] {
        return this.allEmployeeRoleOptions.filter(opt => this.selectedEmployeeRoleIds.includes(opt.value));
    }

    getAvailableEmployeeRoleOptions(): DualListboxOption[] {
        return this.allEmployeeRoleOptions.filter(opt => !this.selectedEmployeeRoleIds.includes(opt.value));
    }

    buildProductDistributions(): CreateProProductDistributionDto[] {
        const distributions: CreateProProductDistributionDto[] = [];
        const productId: string = this.selectedProduct?.id || '';

        this.selectedChannelIds.forEach(channelId => {
            distributions.push({
                productId,
                channelId,
                appChannelId: undefined,
                employeeRoleId: undefined,
                status: ProProductDistributionStatus.Active
            });
        });

        this.selectedAppChannelIds.forEach(appChannelId => {
            distributions.push({
                productId,
                channelId: undefined,
                appChannelId,
                employeeRoleId: undefined,
                status: ProProductDistributionStatus.Active
            });
        });

        this.selectedEmployeeRoleIds.forEach(employeeRoleId => {
            distributions.push({
                productId,
                channelId: undefined,
                appChannelId: undefined,
                employeeRoleId,
                status: ProProductDistributionStatus.Active
            });
        });

        return distributions;
    }

    /**
     * Builds product distributions for update: existing (with id) + new selections (without id)
     * so the backend can update in place and insert only new rows.
     */
    buildProductDistributionsForUpdate(): ProProductDistributionDto[] {
        const distributions: ProProductDistributionDto[] = [];
        const productId: string = this.selectedProduct?.id || '';

        // Existing channel distributions that are still selected
        const existingChannels = this.productDistributions.filter(
            d => d.channelId && this.selectedChannelIds.includes(d.channelId)
        );
        existingChannels.forEach(d => {
            distributions.push({
                id: d.id,
                productId: productId || d.productId,
                channelId: d.channelId,
                appChannelId: undefined,
                employeeRoleId: undefined,
                status: (d.status ?? ProProductDistributionStatus.Active) as ProProductDistributionStatus
            });
        });

        // Existing app channel distributions that are still selected
        const existingAppChannels = this.productDistributions.filter(
            d => d.appChannelId && this.selectedAppChannelIds.includes(d.appChannelId)
        );
        existingAppChannels.forEach(d => {
            distributions.push({
                id: d.id,
                productId: productId || d.productId,
                channelId: undefined,
                appChannelId: d.appChannelId,
                employeeRoleId: undefined,
                status: (d.status ?? ProProductDistributionStatus.Active) as ProProductDistributionStatus
            });
        });

        // Existing role-only distributions that are still selected
        const existingRoles = this.productDistributions.filter(
            d => d.employeeRoleId && !d.channelId && !d.appChannelId && this.selectedEmployeeRoleIds.includes(d.employeeRoleId)
        );
        existingRoles.forEach(d => {
            distributions.push({
                id: d.id,
                productId: productId || d.productId,
                channelId: undefined,
                appChannelId: undefined,
                employeeRoleId: d.employeeRoleId,
                status: (d.status ?? ProProductDistributionStatus.Active) as ProProductDistributionStatus
            });
        });

        // New channel selections
        const existingChannelIds = new Set(existingChannels.map(d => d.channelId));
        this.selectedChannelIds.filter(id => !existingChannelIds.has(id)).forEach(channelId => {
            distributions.push({
                productId,
                channelId,
                appChannelId: undefined,
                employeeRoleId: undefined,
                status: ProProductDistributionStatus.Active
            });
        });

        // New app channel selections
        const existingAppChannelIds = new Set(existingAppChannels.map(d => d.appChannelId));
        this.selectedAppChannelIds.filter(id => !existingAppChannelIds.has(id)).forEach(appChannelId => {
            distributions.push({
                productId,
                channelId: undefined,
                appChannelId,
                employeeRoleId: undefined,
                status: ProProductDistributionStatus.Active
            });
        });

        // New employee role selections
        const existingRoleIds = new Set(existingRoles.map(d => d.employeeRoleId));
        this.selectedEmployeeRoleIds.filter(id => !existingRoleIds.has(id)).forEach(employeeRoleId => {
            distributions.push({
                productId,
                channelId: undefined,
                appChannelId: undefined,
                employeeRoleId,
                status: ProProductDistributionStatus.Active
            });
        });

        return distributions;
    }

    isEditingPlanDefinition(plan: ProProductPlanDefinitionDto): boolean {
        if (!this.editingPlanDefinition) return false;
        
        // If plan has an ID, match by ID
        if (plan.id) {
            return this.editingPlanDefinition.id === plan.id;
        }
        
        // If plan doesn't have an ID (new item), match by index
        return this.editingPlanDefinitionIndex >= 0 && 
               this.editingPlanDefinitionIndex < this.productPlanDefinitions.length &&
               this.productPlanDefinitions[this.editingPlanDefinitionIndex] === plan;
    }
}
