import { Component, HostListener, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom, forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { TableLazyLoadEvent } from 'primeng/table';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { DialogModule } from 'primeng/dialog';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TextareaModule } from 'primeng/textarea';
import { TooltipModule } from 'primeng/tooltip';
import { InputNumberModule } from 'primeng/inputnumber';
import { DatePickerModule } from 'primeng/datepicker';
import { TableModule } from 'primeng/table';
import { FileUploadModule } from 'primeng/fileupload';

import { VTable } from '@/shared/components/v-table/v-table';
import { TableColumn } from '@/shared/models/table-column.model';
import { TableAction } from '@/shared/models/table-action.model';
import { ProTableRateService } from '@/proxy/product/controllers/pro-table-rate.service';
import { ProTableRateDto, CreateProTableRateDto, UpdateProTableRateDto, GetProTableRatesInput } from '@/proxy/product/pro-table-rates/models';
import { ProTableRateVariableDto } from '@/proxy/product/pro-table-rate-variables/models';
import { ProTableRateStatus, proTableRateStatusOptions } from '@/proxy/pro-table-rates/pro-table-rate-status.enum';
import { ProLineOfBusinessService } from '@/proxy/product/controllers/pro-line-of-business.service';
import { GetProLineOfBusinessesInput, ProLineOfBusinessDto } from '@/proxy/product/pro-line-of-businesses/models';
import { ProLineOfBusinessStatus } from '@/proxy/pro-line-of-businesses/pro-line-of-business-status.enum';
import { ResPartnerService } from '@/proxy/partner/controllers/res-partner.service';
import { GetResPartnersInput, ResPartnerDto } from '@/proxy/partner/res-partners/models';
import { ResPartnerStatus } from '@/proxy/res-partners/res-partner-status.enum';
import { ProAttributeService } from '@/proxy/product/controllers/pro-attribute.service';
import { GetProAttributesInput, ProAttributeDto } from '@/proxy/product/pro-attributes/models';
import { ProAttributeStatus } from '@/proxy/pro-attributes/pro-attribute-status.enum';
import { ProAttributeDataType } from '@/proxy/pro-attributes/pro-attribute-data-type.enum';
import { ProTableRateLineService } from '@/proxy/product/controllers/pro-table-rate-line.service';
import { AdminConfigService } from '@/proxy/master/controllers/admin-config.service';
import { AdminConfigDto, GetAdminConfigsInput } from '@/proxy/master/admin-configs/models';
import { AdminConfigStatus } from '@/proxy/admin-configs/admin-config-status.enum';
import { ProTableRateLineDto, CreateProTableRateLineDto, UpdateProTableRateLineDto, GetProTableRateLinesInput } from '@/proxy/product/pro-table-rate-lines/models';
import { ProCoverageService } from '@/proxy/product/controllers/pro-coverage.service';
import { GetProCoveragesInput, ProCoverageDto } from '@/proxy/product/pro-coverages/models';
import { ProCoverageStatus } from '@/proxy/pro-coverages/pro-coverage-status.enum';
import { ResChannelService } from '@/proxy/partner/controllers/res-channel.service';
import { GetResChannelsInput, ResChannelDto } from '@/proxy/partner/res-channels/models';
import { ResChannelStatus } from '@/proxy/res-channels/res-channel-status.enum';
import { ProProductService } from '@/proxy/product/pro-products/pro-product.service';
import { GetProProductsInput, ProProductDto } from '@/proxy/product/pro-products/models';
import { ProProductStatus } from '@/proxy/pro-products';
import { ProTableRateSearchForm, ProTableRateFormData, ProTableRateVariableFormItem, ProTableRateLineFormData } from './pro-table-rates.models';
import { PermissionPipe } from '@/core/pipes/permission.pipe';
import { PermissionService } from '@/core/services/permission.service';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';
import { environment } from '@environments/environment';
import { exportToExcel, ExportColumn } from '@/shared/utils/export.util';

@Component({
  selector: 'app-pro-table-rates',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    PanelModule,
    ButtonModule,
    InputTextModule,
    SelectModule,
    ToastModule,
    DialogModule,
    ConfirmDialogModule,
    TextareaModule,
    TooltipModule,
    InputNumberModule,
    DatePickerModule,
    TableModule,
    FileUploadModule,
    VTable,
    PermissionPipe,
    TranslatePipe
  ],
  templateUrl: './pro-table-rates.component.html',
  styleUrl: './pro-table-rates.component.scss',
  providers: [MessageService, ConfirmationService]
})
export class ProTableRatesComponent implements OnInit {
  // Permissions
  readonly PERMISSIONS = {
    CREATE: 'Product.ProTableRate.Create',
    UPDATE: 'Product.ProTableRate.Edit',
    DELETE: 'Product.ProTableRate.Delete',
    VIEW: 'Product.ProTableRate.View'
  };

  // ProTableRateLine uses the same permissions as ProTableRate
  readonly LINE_PERMISSIONS = {
    CREATE: 'Product.ProTableRate.Create',
    UPDATE: 'Product.ProTableRate.Edit',
    DELETE: 'Product.ProTableRate.Delete',
    VIEW: 'Product.ProTableRate.View'
  };

  // Data
  tableRates: ProTableRateDto[] = [];
  totalCount = 0;
  loading = true;
  pageSize = 10;
  exporting = false;
  exportingLines = false;

  // Dialog
  dialogVisible = false;
  dialogMode: 'create' | 'edit' | 'view' = 'create';
  formData: ProTableRateFormData = this.getEmptyForm();
  selectedTableRate: ProTableRateDto | null = null;

  // Search form
  searchForm: ProTableRateSearchForm = {
    lobId: null,
    insurerId: null,
    code: null,
    name: null,
    status: null,
    productId: null
  };

  // Status options
  statusOptions: Array<{ label: string; value: ProTableRateStatus }> = [];

  // Line of Business options
  lobOptions: Array<{ label: string; value: string }> = [];
  fullLobOptions: Array<{ label: string; value: string }> = [];
  loadingLobs = false;

  // Insurer options
  insurerOptions: Array<{ label: string; value: string }> = [];
  fullInsurerOptions: Array<{ label: string; value: string }> = [];
  loadingInsurers = false;

  // Attribute options (for variables)
  attributeOptions: Array<{ label: string; value: string }> = [];
  loadingAttributes = false;

  // Operator options
  operatorOptions: Array<{ label: string; value: string }> = [];
  loadingOperators = false;

  // Variables in form
  formVariables: ProTableRateVariableFormItem[] = [];
  displayVariables: ProTableRateVariableFormItem[] = []; // Cached display variables
  newVariable: ProTableRateVariableFormItem = { attributeId: '', operator: '' };

  // Table
  columns: TableColumn[] = [];
  actions: TableAction<ProTableRateDto>[] = [];
  currentLazyLoadEvent?: TableLazyLoadEvent;

  // ProTableRateLine Data
  selectedTableRateId?: string;
  selectedTableRateLobId?: string;
  tableRateLines: ProTableRateLineDto[] = [];
  lineTotalCount = 0;
  lineLoading = false;
  linePageSize = 10;
  lineCurrentLazyLoadEvent?: TableLazyLoadEvent;

  // ProTableRateLine Dialog
  lineDialogVisible = false;
  lineDialogMode: 'create' | 'edit' | 'view' = 'create';
  lineFormData: ProTableRateLineFormData = this.getEmptyLineForm();
  selectedTableRateLine?: ProTableRateLineDto;

  // Coverage options
  allCoverageOptions: Array<{ label: string; value: string; lobId?: string }> = [];
  loadingCoverages = false;

  // Channel options
  channelOptions: Array<{ label: string; value: string }> = [];
  loadingChannels = false;

  // Partner options (for lines - same as insurer but can be different)
  linePartnerOptions: Array<{ label: string; value: string }> = [];
  loadingLinePartners = false;

  // Product options
  productOptions: Array<{ label: string; value: string }> = [];
  loadingProducts = false;

  // Line Table
  lineColumns: TableColumn[] = [];
  lineActions: TableAction<ProTableRateLineDto>[] = [];

  // Attribute details cache for datatype lookup
  attributeDetailsMap: Map<string, ProAttributeDto> = new Map();

  // Import
  importDialogVisible = false;
  importResultDialogVisible = false;
  importing = false;
  importResult: any = null;
  selectedImportFile: File | null = null;

  constructor(
    private tableRateService: ProTableRateService,
    private lobService: ProLineOfBusinessService,
    private partnerService: ResPartnerService,
    private attributeService: ProAttributeService,
    private tableRateLineService: ProTableRateLineService,
    private coverageService: ProCoverageService,
    private channelService: ResChannelService,
    private adminConfigService: AdminConfigService,
    private productService: ProProductService,
    private messageService: MessageService,
    private http: HttpClient,
    private confirmationService: ConfirmationService,
    private permissionService: PermissionService,
    private localizationService: LocalizationService,
    private activatedRoute: ActivatedRoute
  ) {
    this.initializeColumns();
    this.initializeActions();
    this.initializeStatusOptions();
    this.initializeLineColumns();
    this.initializeLineActions();
    this.loadLobs();
    this.loadInsurers();
    this.loadAttributes();
    this.loadOperators();
    this.loadCoverages();
    this.loadChannels();
    this.loadLinePartners();
    this.loadProducts();
  }

  ngOnInit(): void {
    const code = this.activatedRoute.snapshot.queryParamMap.get('code');
    if (code) {
      this.searchForm = { ...this.searchForm, code };
    }
    // Initial load will be triggered by the table's onLazyLoad event
  }

  /**
   * Initialize status options with localized labels
   */
  private initializeStatusOptions(): void {
    this.statusOptions = [
      { label: this.localizationService.localize('Product::ProTableRate:Active'), value: ProTableRateStatus.Active },
      { label: this.localizationService.localize('Product::ProTableRate:Deactive'), value: ProTableRateStatus.Deactive }
    ];
  }

  /**
   * Load operator options from AdminConfig with code = 'OPERATOR'
   */
  private loadOperators(): void {
    this.loadingOperators = true;
    const input: GetAdminConfigsInput = {
      skipCount: 0,
      maxResultCount: 1000,
      code: 'OPERATOR',
      status: AdminConfigStatus.Active,
      sorting: 'name asc'
    };

    this.adminConfigService.getList(input).subscribe({
      next: (result) => {
        this.operatorOptions = (result.items || [])
          .filter(config => config.code === 'OPERATOR' && config.status === AdminConfigStatus.Active)
          .map(config => ({
            label: config.name || config.subCode || '',
            value: config.subCode || ''
          }));
        this.loadingOperators = false;
      },
      error: () => {
        // Silently fail - operators will just be empty
        this.loadingOperators = false;
      }
    });
  }


  /**
   * Load line of businesses for dropdown
   */
  private loadLobs(): void {
    this.loadingLobs = true;
    const input: GetProLineOfBusinessesInput = {
      skipCount: 0,
      maxResultCount: 1000,
      sorting: 'name asc'
    };

    this.lobService.getList(input).subscribe({
      next: (result) => {
        this.lobOptions = (result.items || []).filter(lob => lob.status === ProLineOfBusinessStatus.Active).map(lob => ({
          label: `${lob.code} - ${lob.name}`,
          value: lob.id || ''
        }));
        this.fullLobOptions = (result.items || []).map(lob => ({
          label: `${lob.code} - ${lob.name}`,
          value: lob.id || ''
        }));
        this.loadingLobs = false;
      },
      error: () => {
        // Silently fail - LOBs will just be empty
        this.loadingLobs = false;
      }
    });
  }

  /**
   * Load insurers (partners) for dropdown
   */
  private loadInsurers(): void {
    this.loadingInsurers = true;
    const input: GetResPartnersInput = {
      skipCount: 0,
      maxResultCount: 1000,
      partnerTypeCode: 'INSURER',
      sorting: 'name asc'
    };

    this.partnerService.getList(input).subscribe({
      next: (result) => {
        this.insurerOptions = (result.items || []).filter(insurer => insurer.status === ResPartnerStatus.Active).map(insurer => ({
          label: `${insurer.code || ''} - ${insurer.name || ''}`,
          value: insurer.id || ''
        }));
        this.fullInsurerOptions = (result.items || []).map(insurer => ({
          label: `${insurer.code || ''} - ${insurer.name || ''}`,
          value: insurer.id || ''
        }));
        this.loadingInsurers = false;
      },
      error: () => {
        // Silently fail - insurers will just be empty
        this.loadingInsurers = false;
      }
    });
  }

  /**
   * Load attributes for dropdown (for variables)
   */
  private loadAttributes(): void {
    this.loadingAttributes = true;
    const input: GetProAttributesInput = {
      skipCount: 0,
      maxResultCount: 1000,
      status: ProAttributeStatus.Active,
      sorting: 'name asc'
    };

    this.attributeService.getList(input).subscribe({
      next: (result) => {
        this.attributeOptions = (result.items || []).map(attr => ({
          label: `${attr.code || ''} - ${attr.name || ''}`,
          value: attr.id || ''
        }));
        this.loadingAttributes = false;
      },
      error: () => {
        // Silently fail - attributes will just be empty
        this.loadingAttributes = false;
      }
    });
  }

  /**
   * Load attribute details for given attribute IDs and cache them
   */
  private async loadAttributeDetails(attributeIds: string[]): Promise<void> {
    if (!attributeIds || attributeIds.length === 0) {
      return;
    }

    // Filter out attributes we already have cached
    const idsToLoad = attributeIds.filter(id => id && !this.attributeDetailsMap.has(id));

    if (idsToLoad.length === 0) {
      return;
    }

    // Load all attributes in parallel using forkJoin
    const loadObservables = idsToLoad.map(id =>
      this.attributeService.get(id).pipe(
        catchError(() => of(null)) // Return null on error for individual attributes
      )
    );

    try {
      const results = await firstValueFrom(forkJoin(loadObservables));
      results.forEach((attr, index) => {
        if (attr) {
          this.attributeDetailsMap.set(idsToLoad[index], attr);
        }
      });
    } catch (error) {
      // Silently fail - some attributes may not load
    }
  }

  /**
   * Load coverages for dropdown
   */
  private loadCoverages(): void {
    this.loadingCoverages = true;
    const input: GetProCoveragesInput = {
      skipCount: 0,
      maxResultCount: 1000,
      status: ProCoverageStatus.Active,
      sorting: 'name asc'
    };

    this.coverageService.getList(input).subscribe({
      next: (result) => {
        this.allCoverageOptions = (result.items || []).map(cov => ({
          label: `${cov.code || ''} - ${cov.name || ''}`,
          value: cov.id || '',
          lobId: cov.lobId
        }));
        this.loadingCoverages = false;
      },
      error: () => {
        this.loadingCoverages = false;
      }
    });
  }

  /**
   * Load channels for dropdown
   */
  private loadChannels(): void {
    this.loadingChannels = true;
    const input: GetResChannelsInput = {
      skipCount: 0,
      maxResultCount: 1000,
      status: ResChannelStatus.Active,
      sorting: 'name asc'
    };

    this.channelService.getList(input).subscribe({
      next: (result) => {
        this.channelOptions = (result.items || []).map(ch => ({
          label: `${ch.code || ''} - ${ch.name || ''}`,
          value: ch.id || ''
        }));
        this.loadingChannels = false;
      },
      error: () => {
        this.loadingChannels = false;
      }
    });
  }

  /**
   * Load partners for line dropdown
   */
  private loadLinePartners(): void {
    this.loadingLinePartners = true;
    const input: GetResPartnersInput = {
      skipCount: 0,
      maxResultCount: 1000,
      sorting: 'name asc'
    };

    this.partnerService.getList(input).subscribe({
      next: (result) => {
        this.linePartnerOptions = (result.items || []).map(partner => ({
          label: `${partner.code || ''} - ${partner.name || ''}`,
          value: partner.id || ''
        }));
        this.loadingLinePartners = false;
      },
      error: () => {
        this.loadingLinePartners = false;
      }
    });
  }

  /**
   * Load products for dropdown
   */
  private loadProducts(): void {
    this.loadingProducts = true;
    const input: GetProProductsInput = {
      skipCount: 0,
      maxResultCount: 1000,
      status: ProProductStatus.Active,
      sorting: 'name asc'
    };

    this.productService.getList(input).subscribe({
      next: (result) => {
        this.productOptions = (result.items || []).map(product => ({
          label: `${product.code || ''} - ${product.name || ''}`,
          value: product.id || ''
        }));
        this.loadingProducts = false;
      },
      error: () => {
        // Silently fail - products will just be empty
        this.loadingProducts = false;
      }
    });
  }

  /**
   * Initialize table columns with localized headers
   */
  private initializeColumns(): void {
    this.columns = [
      {
        field: 'lobId',
        header: this.localizationService.localize('Product::ProTableRate:Lob'),
        sortable: false,
        width: '200px',
        formatter: (value: any, row: ProTableRateDto) => {
          const lob = this.lobOptions.find(opt => opt.value === row.lobId);
          return lob ? lob.label : (row.lobId || '');
        }
      },
      {
        field: 'insurerId',
        header: this.localizationService.localize('Product::ProTableRate:Insurer'),
        sortable: false,
        width: '200px',
        formatter: (value: any, row: ProTableRateDto) => {
          if (!row.insurerId) return '';
          const insurer = this.insurerOptions.find(opt => opt.value === row.insurerId);
          return insurer ? insurer.label : (row.insurerId || '');
        }
      },
      {
        field: 'code',
        header: this.localizationService.localize('Product::ProTableRate:Code'),
        sortable: true,
        width: '150px'
      },
      {
        field: 'name',
        header: this.localizationService.localize('Product::ProTableRate:Name'),
        sortable: true,
        width: '250px'
      },
      {
        field: 'description',
        header: this.localizationService.localize('Product::ProTableRate:Description'),
        sortable: false,
        width: '300px'
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
        header: this.localizationService.localize('Product::ProTableRate:Status'),
        sortable: true,
        type: 'text',
        width: '120px',
        freeze: 'right',
        align: 'center',
        formatter: (value: any) => this.formatStatus(value),
        cellClass: (value: any) => {
          const statusValue = typeof value === 'number' ? value :
            (value === ProTableRateStatus.Active ? 0 : 1);
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
    if (this.permissionService.isGranted(this.PERMISSIONS.VIEW)) {
      this.actions.push({
        label: this.localizationService.localize('Product::ProTableRate:ViewDetail'),
        icon: 'pi pi-eye',
        command: (row) => this.openViewDialog(row)
      });
    }

    if (this.permissionService.isGranted(this.PERMISSIONS.UPDATE)) {
      this.actions.push({
        label: this.localizationService.localize('Product::Edit'),
        icon: 'pi pi-pencil',
        command: (row) => this.openEditDialog(row)
      });
    }

    if (this.permissionService.isGranted(this.PERMISSIONS.CREATE)) {
      this.actions.push({
        label: this.localizationService.localize('Product::ProTableRateLine:AddFee'),
        icon: 'pi pi-plus',
        command: (row) => this.openAddFeeForTableRate(row)
      });
    }

    if (this.permissionService.isGranted(this.PERMISSIONS.DELETE)) {
      this.actions.push({
        label: this.localizationService.localize('Product::Delete'),
        icon: 'pi pi-trash',
        command: (row) => this.delete(row)
      });
    }

    if (this.permissionService.isGranted(this.PERMISSIONS.CREATE)) {
      this.actions.push({
        label: this.localizationService.localize('Product::ProTableRateLine:ImportExcel'),
        icon: 'pi pi-upload',
        command: (row) => this.openImportDialogForTableRate(row)
      });
    }

    if (this.permissionService.isGranted(this.PERMISSIONS.VIEW)) {
      this.actions.push({
        label: this.localizationService.localize('Product::ProTableRateLine:ExportFile'),
        icon: 'pi pi-download',
        command: (row) => this.exportTableRateLinesToExcel(row)
      });
    }
  }

  /**
   * Initialize line table columns with localized headers
   */
  private initializeLineColumns(): void {
    this.lineColumns = [
      {
        field: 'coverageId',
        header: this.localizationService.localize('Product::ProTableRateLine:Coverage'),
        sortable: false,
        width: '150px',
        formatter: (value: any, row: ProTableRateLineDto) => {
          if (!row.coverageId) return '';
          const coverage = this.allCoverageOptions.find(opt => opt.value === row.coverageId);
          return coverage ? coverage.label : (row.coverageId || '');
        }
      },
      {
        field: 'channelId',
        header: this.localizationService.localize('Product::ProTableRateLine:Channel'),
        sortable: false,
        width: '150px',
        formatter: (value: any, row: ProTableRateLineDto) => {
          if (!row.channelId) return '';
          const channel = this.channelOptions.find(opt => opt.value === row.channelId);
          return channel ? channel.label : (row.channelId || '');
        }
      },
      {
        field: 'partnerId',
        header: this.localizationService.localize('Product::ProTableRateLine:Partner'),
        sortable: false,
        width: '120px',
        formatter: (value: any, row: ProTableRateLineDto) => {
          if (!row.partnerId) return '';
          const partner = this.linePartnerOptions.find(opt => opt.value === row.partnerId);
          return partner ? partner.label : (row.partnerId || '');
        }
      },
      {
        field: 'name',
        header: this.localizationService.localize('Product::ProTableRateLine:Name'),
        sortable: true,
        width: '250px'
      },
      {
        field: 'condition',
        header: this.localizationService.localize('Product::ProTableRateLine:Condition'),
        sortable: true,
        width: '150px',
        formatter: (value: any, row: ProTableRateLineDto) => this.getLineConditionLabel(row.condition)
      },
      {
        field: 'minimumRate',
        header: this.localizationService.localize('Product::ProTableRateLine:MinimumRate'),
        sortable: true,
        type: 'number',
        width: '120px',
        formatter: (value: any) => value != null ? value.toLocaleString() : ''
      },
      {
        field: 'maxDiscount',
        header: this.localizationService.localize('Product::ProTableRateLine:MaxDiscount'),
        sortable: true,
        type: 'number',
        width: '120px',
        formatter: (value: any) => value != null ? value.toLocaleString() : ''
      },
      {
        field: 'loading',
        header: this.localizationService.localize('Product::ProTableRateLine:Loading'),
        sortable: true,
        type: 'number',
        width: '120px',
        formatter: (value: any) => value != null ? value.toLocaleString() : ''
      },
      {
        field: 'effectDate',
        header: this.localizationService.localize('Product::ProTableRateLine:EffectDate'),
        sortable: true,
        type: 'date',
        width: '120px'
      },
      {
        field: 'expireDate',
        header: this.localizationService.localize('Product::ProTableRateLine:ExpireDate'),
        sortable: true,
        type: 'date',
        width: '120px'
      },
      {
        field: 'creationTime',
        header: this.localizationService.localize('AbpIdentity::CreationTime'),
        sortable: true,
        type: 'date',
        width: '180px'
      }
    ];
  }

  /**
   * Initialize line actions based on permissions
   */
  private initializeLineActions(): void {
    if (this.permissionService.isGranted(this.LINE_PERMISSIONS.VIEW)) {
      this.lineActions.push({
        label: this.localizationService.localize('Product::ProTableRateLine:ViewDetail'),
        icon: 'pi pi-eye',
        command: (row) => this.openViewLineDialog(row)
      });
    }

    if (this.permissionService.isGranted(this.LINE_PERMISSIONS.UPDATE)) {
      this.lineActions.push({
        label: this.localizationService.localize('Product::Edit'),
        icon: 'pi pi-pencil',
        command: (row) => this.openEditLineDialog(row)
      });
    }

    if (this.permissionService.isGranted(this.LINE_PERMISSIONS.DELETE)) {
      this.lineActions.push({
        label: this.localizationService.localize('Product::Delete'),
        icon: 'pi pi-trash',
        command: (row) => this.deleteLine(row)
      });
    }
  }


  /**
   * Load table rates with lazy loading
   */
  loadData(event: TableLazyLoadEvent): void {
    this.currentLazyLoadEvent = event;
    this.loading = true;

    const input: GetProTableRatesInput = {
      skipCount: event.first || 0,
      maxResultCount: event.rows || 10,
      sorting: this.getSortingString(event),
      lobId: this.searchForm.lobId || undefined,
      insurerId: this.searchForm.insurerId || undefined,
      code: this.searchForm.code || undefined,
      name: this.searchForm.name || undefined,
      status: this.searchForm.status ?? undefined,
      productId: this.searchForm.productId || undefined
    };

    this.tableRateService.getList(input).subscribe({
      next: (result) => {
        this.tableRates = result.items || [];
        this.totalCount = result.totalCount || 0;
        this.loading = false;
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Product::Error'),
          detail: this.localizationService.localize('AbpUi::InternalServerErrorMessage')
        });
        this.loading = false;
      }
    });
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

  @HostListener('window:keydown.enter', ['$event'])
  handleEnter(event: KeyboardEvent) {
    // Only trigger search if no dialog is visible
    if (!this.dialogVisible && !this.lineDialogVisible && !this.importDialogVisible && !this.importResultDialogVisible) {
      this.search();
    }
  }

  /**
   * Execute search with current filters.
   * Hides "Danh sách phí" and clears table rate selection so only "Danh sách bảng phí" is shown.
   */
  search(): void {
    this.selectedTableRate = null;
    this.selectedTableRateId = undefined;
    this.selectedTableRateLobId = undefined;
    this.tableRateLines = [];
    this.lineTotalCount = 0;
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
      lobId: null,
      insurerId: null,
      code: null,
      name: null,
      status: null,
      productId: null
    };
    this.search();
  }

  /**
   * Open create dialog
   */
  openCreateDialog(): void {
    this.dialogMode = 'create';
    this.formData = this.getEmptyForm();
    this.formVariables = [];
    this.updateDisplayVariables();
    this.newVariable = { attributeId: '', operator: '' };
    this.selectedTableRate = null;
    this.dialogVisible = true;
  }

  /**
   * Open edit dialog
   * ⚠️ QUAN TRỌNG: Code không được phép sửa khi edit
   */
  openEditDialog(tableRate: ProTableRateDto): void {
    this.dialogMode = 'edit';
    this.selectedTableRate = tableRate;
    this.formData = {
      lobId: tableRate.lobId || '',
      insurerId: tableRate.insurerId || null,
      code: tableRate.code || '', // Readonly trong edit mode
      name: tableRate.name || '',
      description: tableRate.description || '',
      status: tableRate.status ?? ProTableRateStatus.Active,
      variables: []
    };
    // Load existing variables with audit fields (creatorName/attributeCreatorName may come from API)
    this.formVariables = (tableRate.variables || []).map(v => {
      const vAny = v as ProTableRateVariableDto & { attributeCreatorName?: string; creatorName?: string };
      return {
        attributeId: v.attributeId || '',
        operator: v.operator || '',
        attributeName: this.getAttributeName(v.attributeId),
        attributeCode: this.getAttributeCode(v.attributeId || ''),
        creationTime: v.creationTime,
        creatorId: v.creatorId,
        creatorName: vAny.attributeCreatorName ?? vAny.creatorName,
      };
    });
    this.updateDisplayVariables();
    this.newVariable = { attributeId: '', operator: '' };
    this.dialogVisible = true;
  }

  /**
   * Open view dialog (read-only)
   */
  openViewDialog(tableRate: ProTableRateDto): void {
    this.dialogMode = 'view';
    this.selectedTableRate = tableRate;
    this.formData = {
      lobId: tableRate.lobId || '',
      insurerId: tableRate.insurerId || null,
      code: tableRate.code || '',
      name: tableRate.name || '',
      description: tableRate.description || '',
      status: tableRate.status ?? ProTableRateStatus.Active,
      variables: []
    };
    // Load existing variables with audit fields (creatorName/attributeCreatorName may come from API but not on DTO type)
    this.formVariables = (tableRate.variables || []).map(v => {
      const vAny = v as ProTableRateVariableDto & { attributeCreatorName?: string; creatorName?: string };
      return {
        attributeId: v.attributeId || '',
        operator: v.operator || '',
        attributeName: this.getAttributeName(v.attributeId),
        attributeCode: this.getAttributeCode(v.attributeId || ''),
        creationTime: v.creationTime,
        creatorId: v.creatorId,
        creatorName: vAny.attributeCreatorName ?? vAny.creatorName,
      };
    });
    this.updateDisplayVariables();
    this.newVariable = { attributeId: '', operator: '' };
    this.dialogVisible = true;
  }

  /**
   * Get attribute name by ID
   */
  getAttributeName(attributeId?: string): string {
    if (!attributeId) return '';
    const attr = this.attributeOptions.find(opt => opt.value === attributeId);
    return attr ? attr.label : '';
  }

  /**
   * Get operator label by value
   */
  getOperatorLabel(operator: string): string {
    if (!operator) return '';
    const opt = this.operatorOptions.find(o => o.value === operator);
    return opt ? opt.label : operator;
  }

  /**
   * Get attribute code by ID
   */
  getAttributeCode(attributeId: string): string {
    if (!attributeId) return '';
    const attr = this.attributeOptions.find(opt => opt.value === attributeId);
    if (!attr) return '';
    // Extract code from label format: "code - name"
    const parts = attr.label.split(' - ');
    return parts[0] || '';
  }

  /**
   * Format date for display
   */
  formatDate(date: Date | string | undefined): string {
    if (!date) return '';
    const dateObj = typeof date === 'string' ? new Date(date) : date;
    if (isNaN(dateObj.getTime())) return '';
    return dateObj.toLocaleDateString('vi-VN', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  /**
   * Format creator ID for display
   */
  formatCreator(creatorName: string | undefined): string {
    if (!creatorName) return '';
    return creatorName;
  }

  /**
   * Update display variables cache
   */
  private updateDisplayVariables(): void {
    this.displayVariables = this.formVariables.map(v => ({
      ...v,
      attributeCode: v.attributeCode || this.getAttributeCode(v.attributeId),
      attributeName: v.attributeName || this.getAttributeName(v.attributeId)
    }));
  }

  /**
   * Get display variables with all necessary fields for table
   */
  getDisplayVariables(): ProTableRateVariableFormItem[] {
    return this.displayVariables;
  }

  /**
   * Get condition label for display in line table (from string)
   */
  getLineConditionLabel(condition: string | undefined | null): string {
    if (!condition) return '';
    return condition;
  }

  /**
   * Generate default condition JSON based on table rate variables
   */
  private generateDefaultConditionJson(): string {
    if (!this.selectedTableRateId) {
      return '{}';
    }

    // Find the selected table rate
    const selectedTableRate = this.tableRates.find(tr => tr.id === this.selectedTableRateId);
    if (!selectedTableRate || !selectedTableRate.variables || selectedTableRate.variables.length === 0) {
      return '{}';
    }

    const conditionObject: Record<string, any> = {};

    // Process each variable
    for (const variable of selectedTableRate.variables) {
      if (!variable.attributeId || !variable.operator) {
        continue;
      }

      // Get attribute code
      const attributeCode = this.getAttributeCode(variable.attributeId);
      if (!attributeCode) {
        continue;
      }

      // Get attribute details for datatype
      const attribute = this.attributeDetailsMap.get(variable.attributeId);

      // Determine default value based on operator
      let defaultValue: any;

      // Operators that require array values: "IN", "NOT_IN", "IN_RANGE"
      if (variable.operator === 'IN' || variable.operator === 'NOT_IN' || variable.operator === 'IN_RANGE') {
        defaultValue = [];
      } else {
        // For other operators, check datatype
        if (attribute && attribute.dataType === ProAttributeDataType.String) {
          defaultValue = '';
        } else {
          // For non-string types (Int, Float, Date, Boolean), leave blank (null)
          defaultValue = null;
        }
      }

      conditionObject[attributeCode] = defaultValue;
    }

    // Return formatted JSON string
    return JSON.stringify(conditionObject, null, 2);
  }

  /**
   * Get filtered coverage options based on selected table rate's LOB ID
   */
  get filteredCoverageOptions(): Array<{ label: string; value: string }> {
    if (!this.selectedTableRateLobId) {
      return [];
    }
    return this.allCoverageOptions
      .filter(cov => cov.lobId === this.selectedTableRateLobId)
      .map(cov => ({ label: cov.label, value: cov.value }));
  }

  /**
   * Add variable to form
   */
  addVariable(): void {
    if (!this.newVariable.attributeId || this.newVariable.attributeId.trim() === '') {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Product::ProTableRateVariable:AttributeRequired')
      });
      return;
    }

    if (!this.newVariable.operator || this.newVariable.operator.trim() === '') {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Product::ProTableRateVariable:OperatorRequired')
      });
      return;
    }

    // Check if attribute already exists
    if (this.formVariables.some(v => v.attributeId === this.newVariable.attributeId)) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Product::ProTableRateVariable:AttributeAlreadyExists')
      });
      return;
    }

    this.formVariables.push({
      ...this.newVariable,
      attributeName: this.getAttributeName(this.newVariable.attributeId),
      attributeCode: this.getAttributeCode(this.newVariable.attributeId),
      creationTime: new Date(), // Set current date for new variables
      creatorId: undefined // Will be set by backend on save
    });
    this.updateDisplayVariables();
    this.newVariable = { attributeId: '', operator: '' };
  }

  /**
   * Remove variable from form by index
   */
  removeVariable(index: number): void {
    if (index >= 0 && index < this.formVariables.length) {
      this.formVariables.splice(index, 1);
    }
  }

  /**
   * Handle remove variable click event
   */
  onRemoveVariable(event: Event, attributeId: string | undefined): void {
    event.stopPropagation();
    event.preventDefault();
    this.removeVariableByAttributeId(attributeId);
  }

  /**
   * Remove variable from form by attributeId
   */
  removeVariableByAttributeId(attributeId: string | undefined): void {
    if (!attributeId) {
      return;
    }

    const index = this.formVariables.findIndex(v => v.attributeId === attributeId);
    if (index !== -1) {
      this.formVariables.splice(index, 1);
      this.updateDisplayVariables(); // Update cache after removal
    }
  }

  /**
   * Remove variable from form by index (used when rowIndex is available)
   * Since getDisplayVariables() preserves order, rowIndex matches formVariables index
   */
  removeVariableByIndex(index: number): void {
    if (index >= 0 && index < this.formVariables.length) {
      this.formVariables.splice(index, 1);
    }
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
  private validateForm(): boolean {
    if (this.dialogMode === 'create') {
      // Validate code only when creating
      if (!this.formData.code || this.formData.code.trim() === '') {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: this.localizationService.localize('Product::ProTableRate:CodeRequired')
        });
        return false;
      }

      if (this.formData.code.length > 50) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: this.localizationService.localize('Product::ProTableRate:CodeMaxLength')
        });
        return false;
      }

      if (!this.validateCode(this.formData.code)) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: this.localizationService.localize('Product::ProTableRate:CodeInvalid')
        });
        return false;
      }
    }

    if (!this.formData.lobId || this.formData.lobId.trim() === '') {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Product::ProTableRate:LobIdRequired')
      });
      return false;
    }

    if (!this.formData.name || this.formData.name.trim() === '') {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Product::ProTableRate:NameRequired')
      });
      return false;
    }

    if (this.formData.name.length > 250) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Product::ProTableRate:NameMaxLength')
      });
      return false;
    }

    if (this.formData.description && this.formData.description.length > 500) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Product::ProTableRate:DescriptionMaxLength')
      });
      return false;
    }

    return true;
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
  onCodeInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    const value = input.value.toUpperCase();
    input.value = value;
    this.formData.code = value;
  }

  /**
   * Translate error message (handles both single and double colon formats)
   */
  translateErrorMessage(message: string, error?: any): string {
    if (!message) {
      return message;
    }

    // Normalize single colon to double colon format for localization keys
    if (message.includes(':') && !message.includes('::')) {
      if (message.startsWith('Product:')) {
        message = message.replace('Product:', 'Product::');
      }
    }

    // Try to translate it
    let translated = this.localizationService.localize(message);

    // If translation returns the same key, it means it wasn't found, use original message
    if (translated === message) {
      translated = message;
    }

    // Replace placeholders if they exist
    if (translated.includes('{Code}')) {
      const codeValue = error?.value || error?.error?.data?.Code || '';
      translated = translated.replace(/{Code}/g, codeValue);
    }

    if (translated.includes('{0}')) {
      const value = error?.value || '';
      translated = translated.replace(/\{0\}/g, value);
    }

    return translated;
  }

  /**
   * Create new table rate
   */
  private create(): void {
    this.loading = true;

    const createDto: CreateProTableRateDto = {
      lobId: this.formData.lobId.trim(),
      insurerId: this.formData.insurerId?.trim() || undefined,
      code: this.formData.code.trim(),
      name: this.formData.name.trim(),
      description: this.formData.description?.trim() || undefined,
      status: this.formData.status,
      variables: this.formVariables.map(v => ({
        attributeId: v.attributeId,
        operator: v.operator
      }))
    };

    this.tableRateService.create(createDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Product::Success'),
          detail: this.localizationService.localize('Product::ProTableRate:CreatedSuccessfully')
        });
        this.dialogVisible = false;
        this.search();
      },
      error: (error) => {
        let errorMessage = error.error?.error?.message ||
          error.error?.error?.details ||
          this.localizationService.localize('AbpUi::InternalServerErrorMessage');
        errorMessage = this.translateErrorMessage(errorMessage, error);
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Product::Error'),
          detail: errorMessage
        });
        this.loading = false;
      }
    });
  }

  /**
   * Update existing table rate
   * ⚠️ QUAN TRỌNG: Chỉ update LobId, InsurerId, Name, Description và Status, không update Code
   */
  private update(): void {
    if (!this.selectedTableRate || !this.selectedTableRate.id) {
      return;
    }

    this.loading = true;

    const updateDto: UpdateProTableRateDto = {
      lobId: this.formData.lobId.trim(),
      insurerId: this.formData.insurerId?.trim() || undefined,
      name: this.formData.name.trim(),
      description: this.formData.description?.trim() || undefined,
      status: this.formData.status,
      variables: this.formVariables.map(v => ({
        attributeId: v.attributeId,
        operator: v.operator
      }))
    };

    this.tableRateService.update(this.selectedTableRate.id, updateDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Product::Success'),
          detail: this.localizationService.localize('Product::ProTableRate:UpdatedSuccessfully')
        });
        this.dialogVisible = false;
        this.search();
      },
      error: (error) => {
        let errorMessage = error.error?.error?.message ||
          error.error?.error?.details ||
          this.localizationService.localize('AbpUi::InternalServerErrorMessage');
        errorMessage = this.translateErrorMessage(errorMessage, error);
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Product::Error'),
          detail: errorMessage
        });
        this.loading = false;
      }
    });
  }

  /**
   * Delete table rate with confirmation
   */
  delete(tableRate: ProTableRateDto): void {
    if (!tableRate.id) {
      return;
    }

    this.confirmationService.confirm({
      message: this.localizationService.localize('Product::ProTableRate:DeleteConfirm'),
      header: this.localizationService.localize('Product::ProTableRate:Delete'),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.loading = true;
        this.tableRateService.delete(tableRate.id!).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: this.localizationService.localize('Product::Success'),
              detail: this.localizationService.localize('Product::ProTableRate:DeletedSuccessfully')
            });
            this.search();
          },
          error: (error) => {
            const errorMessage = error.error?.error?.message ||
              error.error?.error?.details ||
              this.localizationService.localize('AbpUi::InternalServerErrorMessage');
            this.messageService.add({
              severity: 'error',
              summary: this.localizationService.localize('Product::Error'),
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
  private getEmptyForm(): ProTableRateFormData {
    return {
      lobId: '',
      insurerId: null,
      code: '',
      name: '',
      description: '',
      status: ProTableRateStatus.Active,
      variables: []
    };
  }

  /**
   * Format status for display
   */
  formatStatus(status: ProTableRateStatus | number | string | undefined | null): string {
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
      statusValue = status === ProTableRateStatus.Active ? 0 : 1;
    }

    return statusValue === 0
      ? this.localizationService.localize('Product::ProTableRate:Active')
      : this.localizationService.localize('Product::ProTableRate:Deactive');
  }

  /**
   * Handle selection change on ProTableRate table (single selection).
   * Syncs selectedTableRateId/selectedTableRateLobId and loads or clears rate lines.
   */
  onTableRateSelectionChange(selected: ProTableRateDto | null): void {
    this.selectedTableRateId = selected?.id;
    this.selectedTableRateLobId = selected?.lobId ?? undefined;
    if (selected) {
      this.loadTableRateLines();
    } else {
      this.tableRateLines = [];
      this.lineTotalCount = 0;
    }
  }

  /**
   * Handle row click on ProTableRate table (delegates to selection change).
   */
  onTableRateRowClick(tableRate: ProTableRateDto): void {
    if (!tableRate?.id) {
      return;
    }
    this.onTableRateSelectionChange(tableRate);
  }

  /**
   * Open "Danh sách phí" for the given table rate and open create line dialog (row action "Thêm phí").
   */
  openAddFeeForTableRate(tableRate: ProTableRateDto): void {
    if (!tableRate?.id) {
      return;
    }
    this.selectedTableRate = tableRate;
    this.selectedTableRateId = tableRate.id;
    this.selectedTableRateLobId = tableRate.lobId ?? undefined;
    this.loadTableRateLines();
    this.openCreateLineDialog();
  }

  /**
   * Load ProTableRateLine records for selected ProTableRate
   */
  loadTableRateLines(): void {
    if (!this.selectedTableRateId) {
      this.tableRateLines = [];
      this.lineTotalCount = 0;
      return;
    }

    this.lineLoading = true;
    const input: GetProTableRateLinesInput = {
      skipCount: 0,
      maxResultCount: 1000,
      tableRateId: this.selectedTableRateId,
      sorting: 'name asc'
    };

    this.tableRateLineService.getList(input).subscribe({
      next: (result) => {
        this.tableRateLines = result.items || [];
        this.lineTotalCount = result.totalCount || 0;
        this.lineLoading = false;
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Product::Error'),
          detail: this.localizationService.localize('AbpUi::InternalServerErrorMessage')
        });
        this.lineLoading = false;
      }
    });
  }

  /**
   * Load lines with lazy loading (if needed for pagination)
   */
  loadLineData(event: TableLazyLoadEvent): void {
    this.lineCurrentLazyLoadEvent = event;
    if (!this.selectedTableRateId) {
      return;
    }

    this.lineLoading = true;
    const input: GetProTableRateLinesInput = {
      skipCount: event.first || 0,
      maxResultCount: event.rows || this.linePageSize,
      sorting: this.getSortingString(event),
      tableRateId: this.selectedTableRateId
    };

    this.tableRateLineService.getList(input).subscribe({
      next: (result) => {
        this.tableRateLines = result.items || [];
        this.lineTotalCount = result.totalCount || 0;
        this.lineLoading = false;
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Product::Error'),
          detail: this.localizationService.localize('AbpUi::InternalServerErrorMessage')
        });
        this.lineLoading = false;
      }
    });
  }

  /**
   * Open create line dialog
   */
  async openCreateLineDialog(): Promise<void> {
    if (!this.selectedTableRateId) {
      this.messageService.add({
        severity: 'warn',
        summary: this.localizationService.localize('AbpUi::Warning'),
        detail: this.localizationService.localize('Product::ProTableRateLine:SelectTableRateFirst')
      });
      return;
    }

    // Ensure selectedTableRateLobId is set from the selected table rate
    if (!this.selectedTableRateLobId) {
      const selectedTableRate = this.tableRates.find(tr => tr.id === this.selectedTableRateId);
      if (selectedTableRate) {
        this.selectedTableRateLobId = selectedTableRate.lobId || undefined;
      }
    }

    // Get the selected table rate to access variables
    const selectedTableRate = this.tableRates.find(tr => tr.id === this.selectedTableRateId);

    // Load attribute details for all variables in the table rate
    if (selectedTableRate && selectedTableRate.variables && selectedTableRate.variables.length > 0) {
      const attributeIds = selectedTableRate.variables
        .map(v => v.attributeId)
        .filter((id): id is string => !!id);

      await this.loadAttributeDetails(attributeIds);
    }

    this.lineDialogMode = 'create';
    this.lineFormData = this.getEmptyLineForm();
    this.lineFormData.tableRateId = this.selectedTableRateId;

    // Generate and set default condition JSON
    this.lineFormData.condition = this.generateDefaultConditionJson();

    this.selectedTableRateLine = undefined;
    this.lineDialogVisible = true;
  }

  /**
   * Open edit line dialog
   */
  openEditLineDialog(line: ProTableRateLineDto): void {
    this.lineDialogMode = 'edit';
    this.selectedTableRateLine = line;

    // Get the table rate's LOB ID from the tableRates array
    if (line.tableRateId) {
      const tableRate = this.tableRates.find(tr => tr.id === line.tableRateId);
      if (tableRate) {
        this.selectedTableRateLobId = tableRate.lobId || undefined;
      }
    }

    this.lineFormData = {
      tableRateId: line.tableRateId || '',
      coverageId: line.coverageId || null,
      channelId: line.channelId || null,
      partnerId: line.partnerId || null,
      name: line.name || '',
      condition: line.condition || '',
      minimumRate: line.minimumRate ?? null,
      baseRate: line.baseRate ?? null,
      flatRate: line.flatRate ?? null,
      maxDiscount: line.maxDiscount ?? null,
      loading: line.loading ?? null,
      loadingRate: line.loadingRate ?? null,
      effectDate: line.effectDate ? new Date(line.effectDate) : null,
      expireDate: line.expireDate ? new Date(line.expireDate) : null
    };
    this.lineDialogVisible = true;
  }

  /**
   * Open view line dialog (read-only)
   */
  openViewLineDialog(line: ProTableRateLineDto): void {
    this.lineDialogMode = 'view';
    this.selectedTableRateLine = line;

    // Get the table rate's LOB ID from the tableRates array
    if (line.tableRateId) {
      const tableRate = this.tableRates.find(tr => tr.id === line.tableRateId);
      if (tableRate) {
        this.selectedTableRateLobId = tableRate.lobId || undefined;
      }
    }

    this.lineFormData = {
      tableRateId: line.tableRateId || '',
      coverageId: line.coverageId || null,
      channelId: line.channelId || null,
      partnerId: line.partnerId || null,
      name: line.name || '',
      condition: line.condition || '',
      minimumRate: line.minimumRate ?? null,
      baseRate: line.baseRate ?? null,
      flatRate: line.flatRate ?? null,
      maxDiscount: line.maxDiscount ?? null,
      loading: line.loading ?? null,
      loadingRate: line.loadingRate ?? null,
      effectDate: line.effectDate ? new Date(line.effectDate) : null,
      expireDate: line.expireDate ? new Date(line.expireDate) : null
    };
    this.lineDialogVisible = true;
  }

  /**
   * Save line (create or update)
   */
  saveLine(): void {
    if (!this.validateLineForm()) {
      return;
    }

    if (this.lineDialogMode === 'create') {
      this.createLine();
    } else {
      this.updateLine();
    }
  }

  /**
   * Validate line form
   */
  private validateLineForm(): boolean {
    if (!this.lineFormData.tableRateId || this.lineFormData.tableRateId.trim() === '') {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Product::ProTableRateLine:TableRateIdRequired')
      });
      return false;
    }

    if (!this.lineFormData.coverageId || this.lineFormData.coverageId.trim() === '') {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Product::ProTableRateLine:CoverageRequired')
      });
      return false;
    }

    if (this.lineFormData.name != null && this.lineFormData.name.length > 250) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Product::ProTableRateLine:NameMaxLength')
      });
      return false;
    }

    if (!this.lineFormData.effectDate) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Product::ProTableRateLine:EffectDateRequired')
      });
      return false;
    }

    if (this.lineFormData.expireDate && this.lineFormData.expireDate < this.lineFormData.effectDate!) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Product::ProTableRateLine:ExpireDateBeforeEffectDate')
      });
      return false;
    }

    // Validate condition field is valid JSON
    if (!this.lineFormData.condition || this.lineFormData.condition.trim() === '') {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Product::ProTableRateLine:ConditionRequired')
      });
      return false;
    }

    if (!this.isValidJson(this.lineFormData.condition)) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Product::ProTableRateLine:ConditionInvalidJson')
      });
      return false;
    }

    return true;
  }

  /**
   * Validate if a string is valid JSON
   */
  private isValidJson(str: string): boolean {
    if (!str || str.trim() === '') {
      return false;
    }

    try {
      JSON.parse(str);
      return true;
    } catch (error) {
      return false;
    }
  }

  /**
   * Create new line
   */
  private createLine(): void {
    this.lineLoading = true;

    const createDto: CreateProTableRateLineDto = {
      tableRateId: this.lineFormData.tableRateId.trim(),
      coverageId: this.lineFormData.coverageId?.trim() || undefined,
      channelId: this.lineFormData.channelId?.trim() || undefined,
      partnerId: this.lineFormData.partnerId?.trim() || undefined,
      name: this.lineFormData.name?.trim() ?? undefined,
      condition: this.lineFormData.condition.trim(),
      minimumRate: this.lineFormData.minimumRate ?? undefined,
      baseRate: this.lineFormData.baseRate ?? undefined,
      flatRate: this.lineFormData.flatRate ?? undefined,
      maxDiscount: this.lineFormData.maxDiscount ?? undefined,
      loading: this.lineFormData.loading ?? undefined,
      loadingRate: this.lineFormData.loadingRate ?? undefined,
      effectDate: this.toLocalIsoDateOrPass(this.lineFormData.effectDate!) ?? '',
      expireDate: this.lineFormData.expireDate ? (this.toLocalIsoDateOrPass(this.lineFormData.expireDate) ?? undefined) : undefined
    };

    this.tableRateLineService.create(createDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Product::Success'),
          detail: this.localizationService.localize('Product::ProTableRateLine:CreatedSuccessfully')
        });
        this.lineDialogVisible = false;
        this.loadTableRateLines();
      },
      error: (error) => {
        let errorMessage = error.error?.error?.message ||
          error.error?.error?.details ||
          this.localizationService.localize('AbpUi::InternalServerErrorMessage');
        errorMessage = this.translateErrorMessage(errorMessage, error);
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Product::Error'),
          detail: errorMessage
        });
        this.lineLoading = false;
      }
    });
  }

  /**
   * Update existing line
   */
  private updateLine(): void {
    if (!this.selectedTableRateLine || !this.selectedTableRateLine.id) {
      return;
    }

    this.lineLoading = true;

    const updateDto: UpdateProTableRateLineDto = {
      tableRateId: this.lineFormData.tableRateId.trim(),
      coverageId: this.lineFormData.coverageId?.trim() || undefined,
      channelId: this.lineFormData.channelId?.trim() || undefined,
      partnerId: this.lineFormData.partnerId?.trim() || undefined,
      name: this.lineFormData.name?.trim() ?? undefined,
      condition: this.lineFormData.condition.trim(),
      minimumRate: this.lineFormData.minimumRate ?? undefined,
      baseRate: this.lineFormData.baseRate ?? undefined,
      flatRate: this.lineFormData.flatRate ?? undefined,
      maxDiscount: this.lineFormData.maxDiscount ?? undefined,
      loading: this.lineFormData.loading ?? undefined,
      loadingRate: this.lineFormData.loadingRate ?? undefined,
      effectDate: this.toLocalIsoDateOrPass(this.lineFormData.effectDate!) ?? '',
      expireDate: this.lineFormData.expireDate ? (this.toLocalIsoDateOrPass(this.lineFormData.expireDate) ?? undefined) : undefined
    };

    this.tableRateLineService.update(this.selectedTableRateLine.id, updateDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Product::Success'),
          detail: this.localizationService.localize('Product::ProTableRateLine:UpdatedSuccessfully')
        });
        this.lineDialogVisible = false;
        this.loadTableRateLines();
      },
      error: (error) => {
        let errorMessage = error.error?.error?.message ||
          error.error?.error?.details ||
          this.localizationService.localize('AbpUi::InternalServerErrorMessage');
        errorMessage = this.translateErrorMessage(errorMessage, error);
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Product::Error'),
          detail: errorMessage
        });
        this.lineLoading = false;
      }
    });
  }

  /**
   * Delete line with confirmation
   */
  deleteLine(line: ProTableRateLineDto): void {
    if (!line.id) {
      return;
    }

    this.confirmationService.confirm({
      message: this.localizationService.localize('Product::ProTableRateLine:DeleteConfirm'),
      header: this.localizationService.localize('Product::ProTableRateLine:Delete'),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.lineLoading = true;
        this.tableRateLineService.delete(line.id!).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: this.localizationService.localize('Product::Success'),
              detail: this.localizationService.localize('Product::ProTableRateLine:DeletedSuccessfully')
            });
            this.loadTableRateLines();
          },
          error: (error) => {
            const errorMessage = error.error?.error?.message ||
              error.error?.error?.details ||
              this.localizationService.localize('AbpUi::InternalServerErrorMessage');
            this.messageService.add({
              severity: 'error',
              summary: this.localizationService.localize('Product::Error'),
              detail: errorMessage
            });
            this.lineLoading = false;
          }
        });
      }
    });
  }

  /**
   * Get empty line form data
   */
  private getEmptyLineForm(): ProTableRateLineFormData {
    return {
      tableRateId: '',
      coverageId: null,
      channelId: null,
      partnerId: null,
      name: '',
      condition: '',
      minimumRate: null,
      baseRate: null,
      flatRate: null,
      maxDiscount: null,
      loading: null,
      loadingRate: null,
      effectDate: null,
      expireDate: null
    };
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
   * Serialize Date to local ISO string, or pass through string/null/undefined unchanged.
   * Use when building DTOs where value may be Date (from picker) or string (from server).
   */
  private toLocalIsoDateOrPass(value: Date | string | null | undefined): string | null | undefined {
    if (value == null) return value;
    if (value instanceof Date) return this.toLocalIsoDateTime(value) ?? undefined;
    return value;
  }

  /**
   * Open import dialog
   */
  openImportDialog(): void {
    if (!this.selectedTableRateId) {
      this.messageService.add({
        severity: 'warn',
        summary: this.localizationService.localize('Product::Warning'),
        detail: this.localizationService.localize('Product::ProTableRateLine:SelectTableRateFirst')
      });
      return;
    }
    this.importDialogVisible = true;
    this.selectedImportFile = null;
    this.importResult = null;
  }

  /**
   * Open import dialog for a specific table rate (from action menu)
   */
  openImportDialogForTableRate(tableRate: ProTableRateDto): void {
    if (!tableRate?.id) {
      return;
    }

    // Set the selected table rate
    this.selectedTableRate = tableRate;
    this.selectedTableRateId = tableRate.id;
    this.selectedTableRateLobId = tableRate.lobId;

    // Open the import dialog
    this.importDialogVisible = true;
    this.selectedImportFile = null;
    this.importResult = null;

    // Load the table rate lines for this table rate
    this.loadLineData({
      first: 0,
      rows: this.linePageSize
    } as TableLazyLoadEvent);
  }

  /**
   * Close import dialog
   */
  closeImportDialog(): void {
    this.importDialogVisible = false;
    this.selectedImportFile = null;
    this.importResult = null;
  }

  /**
   * Download Excel template
   */
  downloadTemplate(): void {
    if (!this.selectedTableRateId) {
      this.messageService.add({
        severity: 'warn',
        summary: this.localizationService.localize('Product::Warning'),
        detail: this.localizationService.localize('Product::ProTableRateLine:SelectTableRateFirst')
      });
      return;
    }

    // Get API base URL from environment configuration
    const apiConfig = environment.apis['Product'] || environment.apis.default;
    const apiUrl = apiConfig?.url || 'https://localhost:44360';
    const apiEndpoint = `${apiUrl}/api/product/table-rate-lines/export-template?tableRateId=${this.selectedTableRateId}`;

    // Use HttpClient to download the file as blob
    this.http.get(apiEndpoint, {
      responseType: 'blob'
    }).subscribe({
      next: (blob: Blob) => {
        // Create a blob URL and trigger download
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;

        // Generate filename with timestamp
        const timestamp = new Date().toISOString().replace(/[:.]/g, '-').slice(0, -5);
        link.download = `ProTableRateLine_Template_${timestamp}.xlsx`;

        // Trigger download
        document.body.appendChild(link);
        link.click();

        // Cleanup
        setTimeout(() => {
          document.body.removeChild(link);
          window.URL.revokeObjectURL(url);
        }, 100);

        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Product::Success'),
          detail: this.localizationService.localize('Product::ProTableRateLine:TemplateDownloaded')
        });
      },
      error: (error) => {
        // Try to parse error message from blob if response is blob
        if (error.error instanceof Blob) {
          const reader = new FileReader();
          reader.onload = () => {
            try {
              const errorObj = JSON.parse(reader.result as string);
              const errorMessage = errorObj?.error?.message ||
                errorObj?.error?.details ||
                this.localizationService.localize('AbpUi::InternalServerErrorMessage');
              this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('Product::Error'),
                detail: this.translateErrorMessage(errorMessage)
              });
            } catch {
              this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('Product::Error'),
                detail: this.localizationService.localize('AbpUi::InternalServerErrorMessage')
              });
            }
          };
          reader.readAsText(error.error);
        } else {
          let errorMessage = error.error?.error?.message ||
            error.error?.error?.details ||
            this.localizationService.localize('AbpUi::InternalServerErrorMessage');
          errorMessage = this.translateErrorMessage(errorMessage);
          this.messageService.add({
            severity: 'error',
            summary: this.localizationService.localize('Product::Error'),
            detail: errorMessage
          });
        }
      }
    });
  }

  /**
   * Handle file selection
   */
  onFileSelect(event: any): void {
    const file = event.files?.[0];
    if (!file) {
      return;
    }

    // Validate file type
    const validExtensions = ['.xlsx', '.xls'];
    const fileExtension = file.name.substring(file.name.lastIndexOf('.')).toLowerCase();
    if (!validExtensions.includes(fileExtension)) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Product::Error'),
        detail: this.localizationService.localize('Product::ProTableRateLine:InvalidFileType')
      });
      return;
    }

    this.selectedImportFile = file;
  }

  /**
   * Execute import
   */
  executeImport(): void {
    if (!this.selectedImportFile || !this.selectedTableRateId) {
      return;
    }

    this.importing = true;

    // Get API base URL from environment configuration
    const apiConfig = environment.apis['Product'] || environment.apis.default;
    const apiUrl = apiConfig?.url || 'https://localhost:44360';
    const apiEndpoint = `${apiUrl}/api/product/table-rate-lines/import-excel?tableRateId=${this.selectedTableRateId}`;

    const formData = new FormData();
    formData.append('file', this.selectedImportFile);

    this.http.post<any>(apiEndpoint, formData).subscribe({
      next: (result: any) => {
        this.importResult = result;
        this.importing = false;
        this.importDialogVisible = false;
        this.importResultDialogVisible = true;

        if (result.errorCount === 0) {
          this.messageService.add({
            severity: 'success',
            summary: this.localizationService.localize('Product::Success'),
            detail: `${this.localizationService.localize('Product::ProTableRateLine:ImportSuccess')} (${result.successCount} ${this.localizationService.localize('Product::ProTableRateLine:Records')})`
          });
          if (this.lineCurrentLazyLoadEvent) {
            this.loadLineData(this.lineCurrentLazyLoadEvent); // Refresh the list
          } else {
            // Create a default lazy load event if none exists
            this.loadLineData({
              first: 0,
              rows: this.linePageSize
            } as TableLazyLoadEvent);
          }
        } else {
          this.messageService.add({
            severity: 'warn',
            summary: this.localizationService.localize('Product::Warning'),
            detail: `${this.localizationService.localize('Product::ProTableRateLine:ImportPartialSuccess')} - ${this.localizationService.localize('Product::ProTableRateLine:SuccessCount')}: ${result.successCount}, ${this.localizationService.localize('Product::ProTableRateLine:ErrorCount')}: ${result.errorCount}`
          });
        }
      },
      error: (error) => {
        this.importing = false;
        let errorMessage = error.error?.error?.message ||
          error.error?.error?.details ||
          this.localizationService.localize('AbpUi::InternalServerErrorMessage');
        errorMessage = this.translateErrorMessage(errorMessage);
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Product::Error'),
          detail: errorMessage
        });
      }
    });
  }

  /**
   * Download result file
   */
  downloadResultFile(): void {
    if (!this.importResult?.resultFileBytes) {
      return;
    }

    // Convert base64 to blob
    const byteCharacters = atob(this.importResult.resultFileBytes);
    const byteNumbers = new Array(byteCharacters.length);
    for (let i = 0; i < byteCharacters.length; i++) {
      byteNumbers[i] = byteCharacters.charCodeAt(i);
    }
    const byteArray = new Uint8Array(byteNumbers);
    const blob = new Blob([byteArray], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });

    // Create download link
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;

    const timestamp = new Date().toISOString().replace(/[:.]/g, '-').slice(0, -5);
    link.download = `ProTableRateLine_ImportResult_${timestamp}.xlsx`;

    document.body.appendChild(link);
    link.click();

    setTimeout(() => {
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);
    }, 100);
  }

  /**
   * Close import result dialog
   */
  closeImportResultDialog(): void {
    this.importResultDialogVisible = false;
    this.importResult = null;
    this.selectedImportFile = null;
  }

  /**
   * Export table rates to Excel
   */
  exportToExcel(): void {
    // Check if there's data to export
    if (!this.tableRates || this.tableRates.length === 0) {
      this.messageService.add({
        severity: 'warn',
        summary: this.localizationService.localize('Product::Warning'),
        detail: this.localizationService.localize('Product::ProTableRate:NoDataToExport')
      });
      return;
    }

    this.exporting = true;

    try {
      // Define export columns with Vietnamese headers
      const exportColumns: ExportColumn[] = [
        {
          field: 'lobId',
          header: this.localizationService.localize('Product::ProTableRate:Lob'),
          formatter: (value: any, row: ProTableRateDto) => {
            const lob = this.lobOptions.find(opt => opt.value === row.lobId);
            return lob ? lob.label : (row.lobId || '');
          }
        },
        {
          field: 'insurerId',
          header: this.localizationService.localize('Product::ProTableRate:Insurer'),
          formatter: (value: any, row: ProTableRateDto) => {
            if (!row.insurerId) return '';
            const insurer = this.insurerOptions.find(opt => opt.value === row.insurerId);
            return insurer ? insurer.label : (row.insurerId || '');
          }
        },
        {
          field: 'code',
          header: this.localizationService.localize('Product::ProTableRate:Code')
        },
        {
          field: 'name',
          header: this.localizationService.localize('Product::ProTableRate:Name')
        },
        {
          field: 'status',
          header: this.localizationService.localize('Product::ProTableRate:Status'),
          formatter: (value: any) => this.formatStatus(value)
        }
      ];

      // Export to Excel
      exportToExcel(this.tableRates, exportColumns, 'ProTableRate_export');

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
   * Export ProTableRateLines to Excel
   */
  exportTableRateLinesToExcel(tableRate: ProTableRateDto): void {
    if (!tableRate || !tableRate.id) {
      return;
    }

    this.exportingLines = true;

    // Fetch table rate with variables and all lines in parallel
    forkJoin({
      tableRateDetails: this.tableRateService.get(tableRate.id),
      lines: this.tableRateLineService.getList({
        skipCount: 0,
        maxResultCount: 1000,
        tableRateId: tableRate.id,
        sorting: 'name asc'
      })
    }).subscribe({
      next: ({ tableRateDetails, lines }) => {
        try {
          const allLines = lines.items || [];

          // Check if there's data to export
          if (allLines.length === 0) {
            this.messageService.add({
              severity: 'warn',
              summary: this.localizationService.localize('Product::Warning'),
              detail: this.localizationService.localize('Product::ProTableRateLine:NoDataToExport')
            });
            this.exportingLines = false;
            return;
          }

          // Add STT field to each row
          const linesWithStt = allLines.map((line, index) => ({
            ...line,
            stt: index + 1
          }));

          // Build export columns matching import template structure
          const exportColumns: ExportColumn[] = [];

          // Fixed columns: STT + 13 data columns (matches backend import template)
          exportColumns.push({
            field: 'stt',
            header: 'STT'
          });

          exportColumns.push({
            field: 'coverageCode',
            header: 'Mã phạm vi',
            formatter: (value: any, row: ProTableRateLineDto) => {
              if (!row.coverageId) return '';
              const coverage = this.allCoverageOptions.find(opt => opt.value === row.coverageId);
              if (!coverage) return '';
              // Extract code from label format: "code - name"
              const parts = coverage.label.split(' - ');
              return parts[0] || '';
            }
          });

          exportColumns.push({
            field: 'coverageName',
            header: 'Tên phạm vi',
            formatter: (value: any, row: ProTableRateLineDto) => {
              if (!row.coverageId) return '';
              const coverage = this.allCoverageOptions.find(opt => opt.value === row.coverageId);
              if (!coverage) return '';
              // Extract name from label format: "code - name"
              const parts = coverage.label.split(' - ');
              return parts.length > 1 ? parts.slice(1).join(' - ') : '';
            }
          });

          exportColumns.push({
            field: 'channelCode',
            header: 'Mã kênh',
            formatter: (value: any, row: ProTableRateLineDto) => {
              if (!row.channelId) return '';
              const channel = this.channelOptions.find(opt => opt.value === row.channelId);
              if (!channel) return '';
              // Extract code from label format: "code - name"
              const parts = channel.label.split(' - ');
              return parts[0] || '';
            }
          });

          exportColumns.push({
            field: 'partnerCode',
            header: 'Mã đối tác',
            formatter: (value: any, row: ProTableRateLineDto) => {
              if (!row.partnerId) return '';
              const partner = this.linePartnerOptions.find(opt => opt.value === row.partnerId);
              if (!partner) return '';
              // Extract code from label format: "code - name"
              const parts = partner.label.split(' - ');
              return parts[0] || '';
            }
          });

          exportColumns.push({
            field: 'name',
            header: 'Tên phí'
          });

          exportColumns.push({
            field: 'effectDate',
            header: 'Ngày hiệu lực',
            formatter: (value: any, row: ProTableRateLineDto) => {
              if (!row.effectDate) return '';
              const date = typeof row.effectDate === 'string' ? new Date(row.effectDate) : row.effectDate;
              if (isNaN(date.getTime())) return '';
              const day = String(date.getDate()).padStart(2, '0');
              const month = String(date.getMonth() + 1).padStart(2, '0');
              const year = date.getFullYear();
              return `${day}/${month}/${year}`;
            }
          });

          exportColumns.push({
            field: 'expireDate',
            header: 'Ngày hết hạn',
            formatter: (value: any, row: ProTableRateLineDto) => {
              if (!row.expireDate) return '';
              const date = typeof row.expireDate === 'string' ? new Date(row.expireDate) : row.expireDate;
              if (isNaN(date.getTime())) return '';
              const day = String(date.getDate()).padStart(2, '0');
              const month = String(date.getMonth() + 1).padStart(2, '0');
              const year = date.getFullYear();
              return `${day}/${month}/${year}`;
            }
          });

          exportColumns.push({
            field: 'minimumRate',
            header: 'Phí tối thiểu',
            formatter: (value: any, row: ProTableRateLineDto) => {
              return row.minimumRate != null ? row.minimumRate.toString() : '';
            }
          });

          exportColumns.push({
            field: 'baseRate',
            header: 'Phí theo %',
            formatter: (value: any, row: ProTableRateLineDto) => {
              return row.baseRate != null ? row.baseRate.toString() : '';
            }
          });

          exportColumns.push({
            field: 'flatRate',
            header: 'Phí cố định',
            formatter: (value: any, row: ProTableRateLineDto) => {
              return row.flatRate != null ? row.flatRate.toString() : '';
            }
          });

          exportColumns.push({
            field: 'loading',
            header: 'Tăng/giảm phí',
            formatter: (value: any, row: ProTableRateLineDto) => {
              return row.loading != null ? row.loading.toString() : '';
            }
          });

          exportColumns.push({
            field: 'maxDiscount',
            header: 'Giảm giá tối đa',
            formatter: (value: any, row: ProTableRateLineDto) => {
              return row.maxDiscount != null ? row.maxDiscount.toString() : '';
            }
          });

          // Dynamic columns from variables
          const variables = tableRateDetails.variables || [];
          if (variables.length > 0) {
            // Sort variables by attribute name (matching backend order)
            const sortedVariables = [...variables].sort((a, b) => {
              const nameA = this.getAttributeName(a.attributeId || '') || '';
              const nameB = this.getAttributeName(b.attributeId || '') || '';
              return nameA.localeCompare(nameB);
            });

            for (const variable of sortedVariables) {
              if (!variable.attributeId) continue;

              const attributeName = this.getAttributeName(variable.attributeId);
              const attributeCode = this.getAttributeCode(variable.attributeId);

              if (!attributeName || !attributeCode) continue;

              exportColumns.push({
                field: `var_${attributeCode}`,
                header: `${attributeName}/${attributeCode}`,
                formatter: (value: any, row: ProTableRateLineDto) => {
                  try {
                    if (!row.condition) return '';
                    const conditionObj = JSON.parse(row.condition);
                    const varValue = conditionObj[attributeCode];

                    if (varValue === null || varValue === undefined) {
                      return '';
                    }

                    // Handle array values (for IN, NOT_IN, IN_RANGE operators)
                    if (Array.isArray(varValue)) {
                      return varValue.join(',');
                    }

                    // Handle date values
                    if (varValue instanceof Date || (typeof varValue === 'string' && varValue.match(/^\d{4}-\d{2}-\d{2}/))) {
                      const date = new Date(varValue);
                      if (!isNaN(date.getTime())) {
                        const day = String(date.getDate()).padStart(2, '0');
                        const month = String(date.getMonth() + 1).padStart(2, '0');
                        const year = date.getFullYear();
                        return `${day}/${month}/${year}`;
                      }
                    }

                    // Return as string for other types
                    return String(varValue);
                  } catch (error) {
                    // If JSON parsing fails, return empty string
                    return '';
                  }
                }
              });
            }
          }

          // Export to Excel
          const timestamp = new Date().toISOString().replace(/[:.]/g, '-').slice(0, -5);
          const filename = `ProTableRateLine_${tableRate.code || tableRate.id}_${timestamp}`;
          exportToExcel(linesWithStt, exportColumns, filename);

          // Show success message
          this.messageService.add({
            severity: 'success',
            summary: this.localizationService.localize('Product::Success'),
            detail: this.localizationService.localize('Product::ProTableRateLine:ExportSuccess')
          });
        } catch (error) {
          // Show error message
          this.messageService.add({
            severity: 'error',
            summary: this.localizationService.localize('Product::Error'),
            detail: this.localizationService.localize('Product::ProTableRateLine:ExportFailed')
          });
        } finally {
          this.exportingLines = false;
        }
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Product::Error'),
          detail: this.localizationService.localize('Product::ProTableRateLine:ExportFailed')
        });
        this.exportingLines = false;
      }
    });
  }
}
