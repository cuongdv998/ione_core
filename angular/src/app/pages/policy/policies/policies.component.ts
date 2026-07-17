import { ChangeDetectorRef, Component, OnInit, DoCheck, ViewChild, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { DatePickerModule } from 'primeng/datepicker';
import { TableLazyLoadEvent } from 'primeng/table';
import { MessageService, ConfirmationService, MenuItem } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { DialogModule } from 'primeng/dialog';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TextareaModule } from 'primeng/textarea';
import { InputNumberModule } from 'primeng/inputnumber';
import { FileUploadModule } from 'primeng/fileupload';
import { TableModule } from 'primeng/table';
import { TooltipModule } from 'primeng/tooltip';
import { CheckboxModule } from 'primeng/checkbox';
import { MenuModule } from 'primeng/menu';
import { AutoCompleteModule, AutoCompleteCompleteEvent } from 'primeng/autocomplete';
import { VTable } from '@/shared/components/v-table/v-table';
import { TableColumn } from '@/shared/models/table-column.model';
import { TableAction } from '@/shared/models/table-action.model';
import { PolicyService } from '@/proxy/policy/controllers/policy.service';
import { PolicyDto, CreatePolicyDto, UpdatePolicyDetailDto, GetPoliciesInput, ExtractPolicyAttributeParametersInputDto } from '@/proxy/policy/policies/models';
import { PolicyStatus } from '@/proxy/policies/policy-status.enum';
import { PolicyTerminationStatus } from '@/proxy/policies/policy-termination-status.enum';
import { PolicySellType } from '@/proxy/policies/policy-sell-type.enum';
import { PolicyContractType } from '@/proxy/policy-contracts/policy-contract-type.enum';
import { PolicyContractStatus } from '@/proxy/policy-contracts/policy-contract-status.enum';
import { PolicySearchForm, PolicyFormData, CoverageItem, SummaryItem } from './policies.models';
import { PermissionPipe } from '@/core/pipes/permission.pipe';
import { PermissionService } from '@/core/services/permission.service';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';
import { ConfigStateService } from '@abp/ng.core';
import { ResPartnerService } from '@/proxy/partner/controllers/res-partner.service';
import { ResPartnerStatus } from '@/proxy/res-partners/res-partner-status.enum';
import { ProLineOfBusinessService } from '@/proxy/product/controllers/pro-line-of-business.service';
import { ProLineOfBusinessStatus } from '@/proxy/pro-line-of-businesses';
import { ResCustomerService } from '@/proxy/customer/controllers/res-customer.service';
import { ResIndustryService } from '@/proxy/customer/controllers/res-industry.service';
import { ResCustomerStatus } from '@/proxy/res-customers/res-customer-status.enum';
import { ResCustomerSex } from '@/proxy/res-customers/res-customer-sex.enum';
import { CreateResCustomerDto, UpdateResCustomerDto } from '@/proxy/customer/res-customers/models';
import { ResIndustryStatus } from '@/proxy/res-industries/res-industry-status.enum';
import { ResOrganizationTypeService } from '@/proxy/partner/controllers/res-organization-type.service';
import { OrganizationTypeType } from '@/proxy/res-organization-types/organization-type-type.enum';
import { ResCurrencyService } from '@/proxy/master/controllers/res-currency.service';
import { ResChannelService } from '@/proxy/partner/controllers/res-channel.service';
import { ResChannelStatus } from '@/proxy/res-channels/res-channel-status.enum';
import { HrEmployeeService } from '@/proxy/hr/controllers/hr-employee.service';
import { HrEmployeeStatus } from '@/proxy/hr-employees/hr-employee-status.enum';
import { ResProvinceService } from '@/proxy/master/controllers/res-province.service';
import { ResProvinceStatus } from '@/proxy/res-provinces/res-province-status.enum';
import { ResWardService } from '@/proxy/master/controllers/res-ward.service';
import { ResWardStatus } from '@/proxy/res-wards/res-ward-status.enum';
import { PolicyTypeService } from '@/proxy/policy/controllers/policy-type.service';
import { PolicyTypeStatus } from '@/proxy/policy-types/policy-type-status.enum';
import { PolicyContractService } from '@/proxy/policy/controllers/policy-contract.service';
import { PolicyCertificateService } from '@/proxy/policy/controllers/policy-certificate.service';
import type { PolicyContractDto } from '@/proxy/policy/policy-contracts/models';
import { ResCarBrandService } from '@/proxy/master/controllers/res-car-brand.service';
import { ResCarCategoryService } from '@/proxy/master/controllers/res-car-category.service';
import { ResCarLineService } from '@/proxy/master/controllers/res-car-line.service';
import { ResCarGroupService } from '@/proxy/master/controllers/res-car-group.service';
import { ResCarTypeService } from '@/proxy/master/controllers/res-car-type.service';
import { AdminConfigService } from '@/proxy/master/controllers/admin-config.service';
import { ResCarBrandStatus } from '@/proxy/res-car-brands/res-car-brand-status.enum';
import { ResCarCategoryStatus } from '@/proxy/res-car-categories/res-car-category-status.enum';
import { ResCarLineStatus } from '@/proxy/res-car-lines/res-car-line-status.enum';
import { ResCarGroupStatus } from '@/proxy/res-car-groups/res-car-group-status.enum';
import { ResCarTypeStatus } from '@/proxy/res-car-types/res-car-type-status.enum';
import { AdminConfigStatus } from '@/proxy/admin-configs/admin-config-status.enum';
import { ProProductService } from '@/proxy/product/pro-products/pro-product.service';
import { ProProductDto } from '@/proxy/product/pro-products/models';
import { RestService } from '@abp/ng.core';
import { forkJoin, of } from 'rxjs';
import { switchMap, map, catchError, finalize } from 'rxjs/operators';
import { ResDocumentService } from '@/proxy/master/controllers/res-document.service';
import { ResDocumentTypeService } from '@/proxy/master/controllers/res-document-type.service';
import { ResDocumentTypeStatus } from '@/proxy/res-document-types/res-document-type-status.enum';
import type { ResDocumentDto } from '@/proxy/master/res-documents/models';
import type { FileResponseDto } from '@/proxy/master/res-documents/models';
import { ResObjectTypeService } from '@/proxy/master/controllers/res-object-type.service';
import { ResObjectTypeStatus } from '@/proxy/res-object-types/res-object-type-status.enum';
import { PolicyTerminationModalComponent } from './termination-modal/policy-termination-modal.component';
import { PaymentUpdateModalComponent } from './payment-update-modal/payment-update-modal.component';
import { ImportPolicyModalComponent } from '../policy-contract/import-policy-modal';
import { normalizeVehiclePlate } from '@/core/utils/vehicle-plate.util';
import { ReportTemplateService } from '@/proxy/report/controllers/report-template.service';
import { MulticolumnComboboxComponent } from '@/shared/components/multicolumn-combobox/multicolumn-combobox.component';
import { saveAs } from 'file-saver';

const POLICY_SEARCH_REPORT_CODE = 'POLICY_SEARCH';

@Component({
  selector: 'app-policies',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    PanelModule,
    ButtonModule,
    InputTextModule,
    SelectModule,
    DatePickerModule,
    ToastModule,
    DialogModule,
    ConfirmDialogModule,
    TextareaModule,
    InputNumberModule,
    FileUploadModule,
    TableModule,
    TooltipModule,
    CheckboxModule,
    MenuModule,
    AutoCompleteModule,
    VTable,
    PermissionPipe,
    TranslatePipe,
    PolicyTerminationModalComponent,
    PaymentUpdateModalComponent,
    ImportPolicyModalComponent,
    MulticolumnComboboxComponent
  ],
  templateUrl: './policies.component.html',
  styleUrl: './policies.component.scss',
  providers: [MessageService, ConfirmationService]
})
export class PoliciesComponent implements OnInit, DoCheck {
  // Permissions
  readonly PERMISSIONS = {
    CREATE: 'PolicyPolicy.Create',
    UPDATE: 'PolicyPolicy.Edit',
    DELETE: 'PolicyPolicy.Delete',
    VIEW: 'PolicyPolicy.View'
  };

  /**
   * True when the route is under `/policy/motorbike-policies` (menu Đơn bảo hiểm xe máy).
   * Cùng API và form với đơn ô tô; khác mặc định LOB (MOTOR / CAR, admin_config DEFAULT_LOB_MOTORBIKE) và nhãn giao diện.
   */
  protected isMotorbikeIssuanceUi = false;

  protected refreshMotorbikeIssuanceFlag(): void {
    const path = (this.router.url || '').split('?')[0];
    this.isMotorbikeIssuanceUi = path.includes('/policy/motorbike-policies');
  }

  protected getPolicyRoutePrefix(): string[] {
    return this.isMotorbikeIssuanceUi
      ? ['/pages', 'policy', 'motorbike-policies']
      : ['/pages', 'policy', 'policies'];
  }

  protected navigateToPolicyList(): void {
    this.router.navigate(this.getPolicyRoutePrefix());
  }

  get policyDataListPanelTitleKey(): string {
    return this.isMotorbikeIssuanceUi ? 'Policy::Menu:MotorbikePolicy' : 'AbpAuditLogging::DataList';
  }

  get policyCreateButtonLabelKey(): string {
    return this.isMotorbikeIssuanceUi ? 'Policy::Policy:NewMotorbike' : 'Policy::Policy:New';
  }

  /** Tiêu đề dialog danh sách / trang form (create | view | edit). */
  get policyFormPageTitleKey(): string {
    if (this.isMotorbikeIssuanceUi) {
      return this.dialogMode === 'create'
        ? 'Policy::Policy:NewMotorbike'
        : this.dialogMode === 'view'
          ? 'Policy::Policy:ViewMotorbike'
          : 'Policy::Policy:EditMotorbike';
    }
    return this.dialogMode === 'create'
      ? 'Policy::Policy:New'
      : this.dialogMode === 'view'
        ? 'Policy::Policy:View'
        : 'Policy::Policy:Edit';
  }

  /**
   * Trên route đơn xe máy: phần đối tượng BH luôn rút gọn (chủ xe, biển, khung, máy, loại xe) —
   * áp dụng tạo mới / xem / sửa (không chỉ create).
   */
  get isMotorbikeReducedInsuredObjectSection(): boolean {
    return this.isMotorbikeIssuanceUi;
  }

  // Data
  policies: PolicyDto[] = [];
  totalCount = 0;
  loading = true;
  loadingExport = false;
  pageSize = 10;

  // Dialog
  dialogVisible = false;
  dialogMode: 'create' | 'edit' | 'view' = 'create';
  /** When true, customer list on create-mode is restricted to customers whose saleId = current user's employee id. */
  protected restrictCustomerListToCurrentSale = false;
  formData: PolicyFormData = this.getEmptyForm();
  selectedPolicy?: PolicyDto;
  /** When true, after save success we call submit-for-approval then afterSaveSuccess(). */
  private willSubmitForApprovalAfterSave = false;
  /** HrEmployee id to send with submit-for-approval when user chose from approval modal. */
  private pendingApproverId = '';

  // Customer creation dialog
  customerDialogVisible = false;
  customerDialogTargetField: 'customer' | 'payer' | 'beneficiary' | 'vehicleOwner' = 'customer';
  customerDialogMode: 'create' | 'edit' | 'snapshot-edit' = 'create';
  selectedCustomerId?: string;
  customerFormData: {
    code: string;
    name: string;
    phone: string;
    email: string | null;
    organizationTypeId: string | null;
    provinceId: string | null;
    wardId: string | null;
    address: string;
    fullAddress: string;
    sex: ResCustomerSex | null;
    dob: Date | null;
    idNo: string | null;
    status: ResCustomerStatus;
    // Customer-only (organization) fields
    industryId: string | null;
    businessNo: string;
    tin: string;
    repName: string;
    repPhone: string;
    repEmail: string;
    repIdNo: string;
    repTitle: string;
    authorizerName: string;
    authorizerPhone: string;
    authorizerEmail: string;
    authorizerIdNo: string;
    authorizerTitle: string;
    authorizerDate: Date | null;
  } = {
      code: '',
      name: '',
      phone: '',
      email: null,
      organizationTypeId: null,
      provinceId: null,
      wardId: null,
      address: '',
      fullAddress: '',
      sex: null,
      dob: null,
      idNo: null,
      status: ResCustomerStatus.Active,
      industryId: null,
      businessNo: '',
      tin: '',
      repName: '',
      repPhone: '',
      repEmail: '',
      repIdNo: '',
      repTitle: '',
      authorizerName: '',
      authorizerPhone: '',
      authorizerEmail: '',
      authorizerIdNo: '',
      authorizerTitle: '',
      authorizerDate: null
    };
  /** Industry dropdown options (from res_industry), sort by name A–Z */
  industryOptions: Array<{ label: string; value: string; code?: string }> = [];
  customerDialogLoading = false;

  /** Translation key for customer dialog header (add vs edit payer/beneficiary/vehicleOwner). */
  get customerDialogHeaderKey(): string {
    if (this.customerDialogMode === 'create') {
      return 'Policy::Policy:AddCustomer';
    }
    switch (this.customerDialogTargetField) {
      case 'payer':
        return 'Policy::Policy:UpdatePayerInfo';
      case 'beneficiary':
        return 'Policy::Policy:UpdateBeneficiaryInfo';
      case 'vehicleOwner':
        return 'Policy::Policy:UpdateVehicleOwnerInfo';
      default:
        return 'Policy::Policy:EditCustomer';
    }
  }

  /** Default title when editing payer/beneficiary/vehicleOwner (used if key not localized). */
  get customerDialogHeaderDefault(): string | undefined {
    if (this.customerDialogMode === 'create') return undefined;
    switch (this.customerDialogTargetField) {
      case 'payer':
        return 'Cập nhật thông tin người thanh toán';
      case 'beneficiary':
        return 'Cập nhật thông tin người thụ hưởng';
      case 'vehicleOwner':
        return 'Cập nhật thông tin chủ xe';
      default:
        return undefined;
    }
  }

  organizationTypeOptions: Array<{ label: string; value: string }> = [];
  /** Map organization type id -> type (CN/TC) for customer dialog visibility */
  organizationTypeIdToType: Map<string, OrganizationTypeType> = new Map();
  /** Default organization type id for "Cá nhân" (CN), used when opening customer create modal */
  defaultOrganizationTypeIdCn: string | null = null;
  /**
   * Listen for Enter key events globally to trigger search
   */
  @HostListener('window:keydown.enter', ['$event'])
  onWindowKeyDown(event: KeyboardEvent): void {
    // Only trigger search if focus is not in a dialog
    if (
      !this.dialogVisible &&
      !this.customerDialogVisible &&
      !this.contractDocsPreviewVisible &&
      !this.policyDocsPreviewVisible &&
      !this.carPhotosVisible &&
      !this.carPhotoPreviewVisible &&
      !this.ocrUploadVisible &&
      !this.terminationModal?.visible &&
      !this.paymentUpdateModal?.visible &&
      !this.importPolicyModal?.visible
    ) {
      this.search();
    }
  }

  sexOptions: Array<{ label: string; value: ResCustomerSex }> = [];

  // Search form
  showAdvancedSearch = false;
  quickSearchKeyword: string | null = null;
  quickSearchSelectedType: 'carPlate' | 'carVin' | 'carEngineNumber' | 'certificateNo' | null = null;
  quickSearchSuggestions: Array<{
    type: 'carPlate' | 'carVin' | 'carEngineNumber' | 'certificateNo';
    prefix: string;
    keyword: string;
    display: string;
  }> = [];
  searchActionMenuItems: MenuItem[] = [];

  private buildSearchActionMenuItems(): void {
    this.searchActionMenuItems = [
      {
        label: 'Import đơn',
        icon: 'pi pi-upload',
        command: () => this.importFile()
      },
      {
        label: 'Cập nhật thanh toán',
        icon: 'pi pi-money-bill',
        command: () => this.updatePayment()
      },
      {
        label: 'Export',
        icon: 'pi pi-download',
        disabled: this.loadingExport,
        command: () => this.exportFile()
      }
    ];
  }

  toggleAdvancedSearch(): void {
    const next = !this.showAdvancedSearch;
    this.showAdvancedSearch = next;
    if (next) {
      this.quickSearchKeyword = null;
      this.quickSearchSelectedType = null;
      this.quickSearchSuggestions = [];
    }
  }

  onQuickSearchKeywordChange(value: string | null): void {
    this.quickSearchKeyword = value;
  }

  onQuickSearchComplete(event: AutoCompleteCompleteEvent): void {
    const keyword = (event.query || '').trim();
    this.rebuildQuickSearchSuggestions(keyword);
  }

  onQuickSearchFocus(): void {
    const keyword = (this.quickSearchKeyword || '').trim();
    if (!keyword) {
      return;
    }
    this.rebuildQuickSearchSuggestions(keyword);
    this.cdr.detectChanges();
  }

  private rebuildQuickSearchSuggestions(keyword: string): void {
    if (!keyword) {
      this.quickSearchSuggestions = [];
      return;
    }
    this.quickSearchSuggestions = [
      { type: 'carPlate', prefix: 'Biển số xe', keyword, display: `Biển số xe: ${keyword}` },
      { type: 'carVin', prefix: 'Số khung', keyword, display: `Số khung: ${keyword}` },
      { type: 'carEngineNumber', prefix: 'Số máy', keyword, display: `Số máy: ${keyword}` },
      { type: 'certificateNo', prefix: 'Số giấy chứng nhận', keyword, display: `Số giấy chứng nhận: ${keyword}` }
    ];
  }

  onQuickSearchSuggestionSelect(event: { value: { type: 'carPlate' | 'carVin' | 'carEngineNumber' | 'certificateNo'; keyword: string } }): void {
    const selected = event?.value;
    this.quickSearchSelectedType = selected?.type || null;
    this.quickSearchKeyword = selected?.keyword?.trim() || null;
  }

  get quickSearchPrefixLabel(): string {
    if (this.quickSearchSelectedType === 'carPlate') return 'Biển số xe';
    if (this.quickSearchSelectedType === 'carVin') return 'Số khung';
    if (this.quickSearchSelectedType === 'carEngineNumber') return 'Số máy';
    if (this.quickSearchSelectedType === 'certificateNo') return 'Số giấy chứng nhận';
    return '';
  }

  toggleSearchActionMenu(menu: { toggle: (event: Event) => void }, event: Event): void {
    this.buildSearchActionMenuItems();
    menu.toggle(event);
  }

  searchForm: PolicySearchForm = {
    policyNo: null,
    contractId: null,
    policyTypeId: null,
    partnerId: null,
    status: null,
    channelId: null,
    customerId: null,
    contractType: null,
    contractStatus: null,
    implementerId: null,
    certificateNo: null,
    effectiveDateFrom: null,
    effectiveDateTo: null,
    expiryDateFrom: null,
    expiryDateTo: null,
    carPlate: null,
    carVin: null,
    carEngineNumber: null,
    primaryInsurancePartnerId: null,
    importLotNumber: null,
    paymentStatus: null
  };

  // Status options
  statusOptions: Array<{ label: string; value: PolicyStatus }> = [];

  /** Trạng thái thanh toán (policy_amount.payment_status) */
  paymentStatusOptions: Array<{ label: string; value: string }> = [];

  // Sell Type options
  sellTypeOptions: Array<{ label: string; value: PolicySellType }> = [];

  // Contract Type options
  contractTypeOptions: Array<{ label: string; value: PolicyContractType }> = [];

  // Contract Status options
  contractStatusOptions: Array<{ label: string; value: PolicyContractStatus }> = [];

  // New dropdown options for wireframe implementation
  channelOptions: Array<{ label: string; value: string; code?: string }> = [];
  /** Id of channel with code DIRECT (trực tiếp), used when seller has no channel. */
  directChannelId: string | null = null;
  /** Lock channel selector when seller resolves to a concrete non-DIRECT partner channel. */
  channelLockedBySeller = false;
  /**
   * User cleared "Kênh khai thác" on purpose — skip auto-resolve (employee/partner/insurer → DIRECT)
   * inside loadProducts so the field stays empty until they pick a channel again.
   */
  userClearedFormChannel = false;
  customerOptions: Array<{ label: string; value: string; phone?: string; email?: string; idNo?: string; address?: string }> = [];
  readonly customerColumns = [
    { field: 'label', header: 'Tên KH', width: '25' },
    { field: 'phone', header: 'Điện thoại', width: '15' },
    { field: 'email', header: 'Email', width: '20' },
    { field: 'idNo', header: 'CMND/CCCD', width: '15' },
    { field: 'address', header: 'Địa chỉ', width: '25' },
  ];
  contractOptions: Array<{ label: string; value: string }> = [];
  /**
   * Danh sách "Số hợp đồng" trên form cấp đơn: chỉ hợp đồng bao (Group), cộng hợp đồng đang gắn với đơn khi sửa/xem.
   */
  formContractOptions: Array<{ label: string; value: string }> = [];
  /** Cache hợp đồng sau lọc trạng thái (dùng build lại formContractOptions sau populate). */
  private policyContractsFilteredCache: any[] = [];
  partnerOptions: Array<{ label: string; value: string }> = [];
  implementerOptions: Array<{ label: string; value: string; employeeCode?: string }> = [];
  primaryInsurancePartnerOptions: Array<{ label: string; value: string }> = [];
  lobOptions: Array<{ label: string; value: string }> = [];
  defaultLobId: string | null = null; // Default LOB (resolved from admin_config DEFAULT_LOB / DEFAULT_LOB_MOTORBIKE)
  /**
   * LOB gửi ngầm xuống API list (không hiển thị trên form tìm kiếm).
   * Mặc định: admin_config DEFAULT_LOB (đơn thường) hoặc DEFAULT_LOB_MOTORBIKE (đơn xe máy).
   */
  private implicitSearchLobId: string | null = null;
  /** Giá trị implicit sau Reset (cùng nguồn với lần áp mặc định đầu tiên). */
  private defaultImplicitSearchLobId: string | null = null;
  private searchLobDefaultApplied = false;
  /**
   * LOB gửi ngầm giống loadData — dùng khi tải file mẫu cập nhật thanh toán (cùng điều kiện tìm đơn không nhập gì thêm).
   */
  get exportPaymentTemplateLobId(): string | null {
    return this.implicitSearchLobId?.trim() || null;
  }

  /**
   * Chỉ mount bảng danh sách sau khi đã đọc LOB + admin_config và gán implicitSearchLobId (nếu có).
   * Tránh p-table lazyLoad chạy trước khi có default LOB, và tránh hai request đua (không lob / có lob).
   */
  lobSearchDefaultsResolved = false;
  /** Tăng mỗi lần gọi getList; bỏ qua callback nếu không còn là request mới nhất. */
  private policyListRequestGeneration = 0;
  /** Days before/after current date: show Renewal when policy orgExpireDate <= today + this (from admin_config NOTIFY_RENEWAL_DAYS). */
  notifyRenewalDays: number | null = null;
  policyTypeOptions: Array<{ label: string; value: string }> = [];
  /** Default policy type id for "hình thức cấp đơn" (code BHG). Used when creating policy. */
  defaultPolicyTypeId: string | null = null;
  currencyOptions: Array<{ label: string; value: string; code?: string }> = [];
  sellerOptions: Array<{ label: string; value: string; employeeCode?: string }> = [];
  /** Full seller list before lọc theo kênh khai thác */
  sellerOptionsAll: Array<{ label: string; value: string; employeeCode?: string }> = [];
  employeeData: Array<{
    id: string;
    userId?: string;
    partnerId?: string;
    /** Kênh từ ResPartner.channelId của đối tác gắn nhân viên */
    partnerChannelId?: string | null;
    /** Mã kênh chuẩn hóa (UPPERCASE) từ ResChannel.code */
    partnerChannelCode?: string | null;
  }> = [];
  provinceOptions: Array<{ label: string; value: string }> = [];
  wardOptions: Array<{ label: string; value: string }> = [];
  beneficiaryProvinceOptions: Array<{ label: string; value: string }> = [];
  beneficiaryWardOptions: Array<{ label: string; value: string }> = [];

  // New dropdown options
  payerOptions: Array<{ label: string; value: string; phone?: string; email?: string; idNo?: string; address?: string }> = [];
  beneficiaryOptions: Array<{ label: string; value: string; phone?: string; email?: string; idNo?: string; address?: string }> = [];
  vehicleOwnerOptions: Array<{ label: string; value: string; phone?: string; email?: string; idNo?: string; address?: string }> = [];
  businessPurposeOptions: Array<{ label: string; value: string }> = [
    { label: 'Có kinh doanh', value: 'KDVT' },
    { label: 'Không kinh doanh', value: 'KKDVT' }
  ];
  carBrandOptions: Array<{ label: string; value: string }> = [];
  carModelOptions: Array<{ label: string; value: string }> = [];
  carLineOptions: Array<{ label: string; value: string }> = [];
  carGroupOptions: Array<{ label: string; value: string }> = [];
  carTypeOptions: Array<{ label: string; value: string }> = [];
  carColorOptions: Array<{ label: string; value: string }> = [];
  carPlateTypeOptions: Array<{ label: string; value: string }> = [];
  carOriginOptions: Array<{ label: string; value: string }> = [];

  // Coverage data
  vehicleMaterialCoverages: CoverageItem[] = [];
  mandatoryCivilLiabilityCoverages: CoverageItem[] = [];
  voluntaryCivilLiabilityCoverages: CoverageItem[] = [];

  // Product data for dynamic switches
  products: ProProductDto[] = [];
  selectedProducts: { [productId: string]: boolean } = {};
  productCoverages: { [productId: string]: CoverageItem[] } = {};
  /** Product IDs for which we have already loaded deductible options (chỉ load lần đầu khi chọn sản phẩm). */
  private deductibleLoadedForProductIds = new Set<string>();
  /** Parameter names (from get-attribute-required-spec API) that are required for đối tượng bảo hiểm. If not in this set, the field is not required. */
  requiredAttributeParameterNames = new Set<string>();

  // Lookup maps (id -> code) for motor payload
  private carBrandCodeById: Record<string, string> = {};
  private carModelCodeById: Record<string, string> = {};
  private carCategorySeatNumberById: Record<string, number> = {};
  private carLineCodeById: Record<string, string> = {};
  private carGroupCodeById: Record<string, string> = {};
  private carLineIdByCarGroupId: Record<string, string> = {};
  /** Map carLineId -> carGroupIds[] (nhóm xe thuộc dòng xe đó) */
  private carGroupIdsByCarLineId: Record<string, string[]> = {};
  private carGroupItems: any[] = [];
  private carTypeCodeById: Record<string, string> = {};
  private contractTypeByContractId: Record<string, PolicyContractType> = {};
  private previousCarGroupId: string | null = null;

  // Document upload state (ResDocument)
  uploadedDocuments: ResDocumentDto[] = [];
  uploadedDocumentIds: string[] = [];
  uploadedContractDocuments: ResDocumentDto[] = [];
  uploadedContractDocumentIds: string[] = [];
  documentUploadLoading = false;
  contractDocsPreviewVisible = false;
  policyDocsPreviewVisible = false;
  carPhotosVisible = false;
  carPhotoPreviewVisible = false;
  carPhotoPreviewDoc: ResDocumentDto | null = null;
  /**
   * Car photos / video grouped by slot.
   * Each slot can now hold multiple ResDocumentDto (nhiều ảnh cùng loại).
   */
  carPhotos: {
    front: ResDocumentDto[];
    back: ResDocumentDto[];
    left: ResDocumentDto[];
    right: ResDocumentDto[];
    video: ResDocumentDto[];
  } = {
    front: [],
    back: [],
    left: [],
    right: [],
    video: []
  };
  carPhotosUploading: Record<string, boolean> = {};
  // OCR upload (ảnh đăng ký / ảnh đăng kiểm)
  ocrUploadVisible = false;
  ocrUploadLoading = false;
  ocrUploadedDoc: ResDocumentDto | null = null;
  private uploadedPolicyFileKeys = new Set<string>();
  private uploadedContractFileKeys = new Set<string>();
  private defaultUploadDocumentTypeId: string | null = null;
  private contractDocumentTypeId: string | null = null;
  private productPremiumRecalcTimers: Record<string, any> = {};
  private productPremiumCalcSeqByProductId: Record<string, number> = {};
  private productPremiumCalcStateByProductId: Record<
    string,
    { seq: number; promise: Promise<void>; resolve: () => void } | undefined
  > = {};

  // When opening edit modal, we load detail first then patch selections after products load
  private pendingDetailForEdit: any | null = null;
  // Keep latest loaded detail for update id-mapping (policyVersion/product/coverage ids)
  protected currentPolicyDetail: any | null = null;

  // ObjectTypeId for RiskObject (code = CAR)
  protected carObjectTypeId: string | null = null;

  // Summary data
  summaryItems: SummaryItem[] = [];
  totalPremium = 0;
  totalDiscount = 0;
  totalMarkup = 0;
  /** Giảm phí theo từng sản phẩm (productId -> discount amount), lưu vào policy_product.discount */
  productDiscountByProductId: Record<string, number> = {};
  /** Per-product markup amount (policy_product.markup). */
  productMarkupByProductId: Record<string, number> = {};

  // Flag to prevent infinite loop when auto-calculating dates
  private isUpdatingInsurancePeriod = false;

  // Table
  columns: TableColumn[] = [];
  actions: TableAction<PolicyDto>[] = [];
  currentLazyLoadEvent?: TableLazyLoadEvent;

  // Termination modal
  @ViewChild(PolicyTerminationModalComponent) terminationModal!: PolicyTerminationModalComponent;
  @ViewChild(PaymentUpdateModalComponent) paymentUpdateModal!: PaymentUpdateModalComponent;
  @ViewChild('importPolicyModal') importPolicyModal?: ImportPolicyModalComponent;

  constructor(
    protected policyService: PolicyService,
    protected messageService: MessageService,
    protected confirmationService: ConfirmationService,
    private reportTemplateService: ReportTemplateService,
    private permissionService: PermissionService,
    protected localizationService: LocalizationService,
    protected router: Router,
    private partnerService: ResPartnerService,
    private lobService: ProLineOfBusinessService,
    private customerService: ResCustomerService,
    private currencyService: ResCurrencyService,
    private channelService: ResChannelService,
    private employeeService: HrEmployeeService,
    private provinceService: ResProvinceService,
    private wardService: ResWardService,
    private policyTypeService: PolicyTypeService,
    private policyContractService: PolicyContractService,
    private carBrandService: ResCarBrandService,
    private carCategoryService: ResCarCategoryService,
    private carLineService: ResCarLineService,
    private carGroupService: ResCarGroupService,
    private carTypeService: ResCarTypeService,
    protected adminConfigService: AdminConfigService,
    private organizationTypeService: ResOrganizationTypeService,
    private industryService: ResIndustryService,
    private productService: ProProductService,
    private restService: RestService,
    private configState: ConfigStateService,
    private resDocumentService: ResDocumentService,
    private resDocumentTypeService: ResDocumentTypeService,
    private resObjectTypeService: ResObjectTypeService,
    private policyCertificateService: PolicyCertificateService,
    protected cdr: ChangeDetectorRef
  ) {
    this.initializeColumns();
    this.initializeActions();
    this.initializeStatusOptions();
    this.initializeSellTypeOptions();
    this.initializeContractTypeOptions();
    this.initializeContractStatusOptions();
    this.initializePaymentStatusOptions();
  }

  ngOnInit(): void {
    this.refreshMotorbikeIssuanceFlag();
    this.buildSearchActionMenuItems();
    // Load primary insurance partner options
    this.loadPrimaryInsurancePartnerOptions();
    // Load all dialog options
    this.loadDialogOptions();
    // Load admin config options
    this.loadAdminConfigOptions();
    // Load organization types for customer dialog
    this.loadOrganizationTypes();
    this.loadIndustryOptions();
    // Initialize sex options
    this.initializeSexOptions();
    // Initialize coverage data
    this.initializeCoverageData();
    // Initial load will be triggered by the table's onLazyLoad event
  }

  ngDoCheck(): void {
    // Watch for changes to carGroupId and auto-fill carLineId
    const currentCarGroupId = this.formData?.carGroupId;

    if (currentCarGroupId !== this.previousCarGroupId) {
      this.previousCarGroupId = currentCarGroupId;

      if (currentCarGroupId) {
        this.handleCarGroupChange(currentCarGroupId);
      }
    }
  }

  /**
   * Initialize coverage data with default values
   */
  private initializeCoverageData(): void {
    // Vehicle Material Insurance coverages
    this.vehicleMaterialCoverages = [
      {
        benefit: 'I. Vật chất xe',
        insuranceAmount: null,
        quantity: null,
        taxRate: 0.16,
        premiumRate: null,
        premium: 0,
        vat: 0,
        premiumWithVAT: 0,
        deductible: null,
        description: null
      },
      {
        benefit: 'II. Điều khoản bổ sung',
        insuranceAmount: null,
        quantity: null,
        taxRate: 0.02,
        premiumRate: null,
        premium: 0,
        vat: 0,
        premiumWithVAT: 0,
        deductible: null,
        description: null
      }
    ];

    // Mandatory Civil Liability coverages
    this.mandatoryCivilLiabilityCoverages = [
      {
        benefit: 'TNDS Bắt buộc',
        insuranceAmount: null,
        quantity: null,
        taxRate: null,
        premiumRate: null,
        premium: 470000,
        vat: 0,
        premiumWithVAT: 470000,
        deductible: null,
        description: null
      }
    ];

    // Voluntary Civil Liability coverages
    this.voluntaryCivilLiabilityCoverages = [
      {
        benefit: 'TNDS Tự nguyện',
        insuranceAmount: null,
        quantity: null,
        taxRate: null,
        premiumRate: null,
        premium: 0,
        vat: 0,
        premiumWithVAT: 0,
        deductible: null,
        description: null
      }
    ];

    // Summary items
    this.updateSummary();
  }

  /**
   * Update summary calculations
   */
  updateSummary(): void {
    // Build summary items from selected products
    const items: SummaryItem[] = [];
    let totalLiabilityAmount = 0; // Sum of MAIN coverage amount from all selected products

    // Add each selected product
    let sumDiscount = 0;
    let sumMarkup = 0;
    this.products.forEach(product => {
      if (product.id && this.isProductSelected(product.id)) {
        const totals = this.getProductTotals(product.id);
        const disc = this.productDiscountByProductId[product.id] ?? 0;
        const mk = this.productMarkupByProductId[product.id] ?? 0;
        sumDiscount += disc;
        sumMarkup += mk;
        items.push({
          category: product.name || '',
          premiumWithVAT: totals.premiumWithVAT,
          discount: disc,
          markup: mk,
          productId: product.id
        });

        // Calculate total liability amount: only MAIN coverage amount for this product
        const main = this.getMainCoverageItem(product.id);
        totalLiabilityAmount += (main?.insuranceAmount || 0);
      }
    });

    // Add "Tổng mức trách nhiệm" row - sum of insurance amounts from all selected products
    items.push({
      category: 'Tổng mức trách nhiệm',
      premiumWithVAT: totalLiabilityAmount,
      discount: 0,
      markup: 0
    });

    // Add "Tổng phí BH (có VAT)" row
    const totalPremiumWithVAT = items
      .filter(item => item.category !== 'Tổng mức trách nhiệm' && item.category !== 'Tổng phí BH (có VAT)')
      .reduce((sum, item) => sum + (item.premiumWithVAT || 0), 0);

    items.push({
      category: 'Tổng phí BH (có VAT)',
      premiumWithVAT: totalPremiumWithVAT,
      discount: sumDiscount,
      markup: sumMarkup
    });

    // Calculate premium and VAT totals (without VAT) for formData
    const selectedProductIds = this.getSelectedProductIds();
    const totals = this.getSelectedProductsTotals(selectedProductIds);

    // Update formData with calculated totals
    this.formData.premiumTotal = totals.premiumTotal;
    this.formData.premium = totals.premium;
    this.formData.vat = totals.vat;

    this.summaryItems = items;
    this.totalPremium = totalPremiumWithVAT;
    this.totalDiscount = sumDiscount;
    this.totalMarkup = sumMarkup;
  }

  getProductDiscount(productId: string): number {
    return this.productDiscountByProductId[productId] ?? 0;
  }

  setProductDiscount(productId: string, value: number | null): void {
    const num = value != null && !Number.isNaN(Number(value)) ? Number(value) : 0;
    this.productDiscountByProductId[productId] = num;
    this.refreshTotalDiscount();
  }

  getProductMarkup(productId: string): number {
    return this.productMarkupByProductId[productId] ?? 0;
  }

  setProductMarkup(productId: string, value: number | null): void {
    const num = value != null && !Number.isNaN(Number(value)) ? Number(value) : 0;
    this.productMarkupByProductId[productId] = num;
    this.refreshTotalMarkup();
  }

  /** Recompute totalDiscount from productDiscountByProductId without rebuilding summaryItems (avoids input focus loss). */
  private refreshTotalDiscount(): void {
    const ids = this.getSelectedProductIds();
    this.totalDiscount = ids.reduce((sum, id) => sum + (this.productDiscountByProductId[id] ?? 0), 0);
  }

  /** Recompute totalMarkup from productMarkupByProductId without rebuilding summaryItems (avoids input focus loss). */
  private refreshTotalMarkup(): void {
    const ids = this.getSelectedProductIds();
    this.totalMarkup = ids.reduce((sum, id) => sum + (this.productMarkupByProductId[id] ?? 0), 0);
  }

  /**
   * Get summary items for display (includes selected products and fixed rows)
   */
  getSummaryItems(): SummaryItem[] {
    return this.summaryItems;
  }

  /**
   * Initialize status options with localized labels
   */
  private initializeStatusOptions(): void {
    this.statusOptions = [
      { label: this.localizationService.localize('Policy::Policy:Quotation'), value: PolicyStatus.Quotation },
      { label: this.localizationService.localize('Policy::Policy:Draft'), value: PolicyStatus.Draft },
      { label: this.localizationService.localize('Policy::Policy:Active'), value: PolicyStatus.Active },
      { label: this.localizationService.localize('Policy::Policy:Expired'), value: PolicyStatus.Expired },
      { label: this.localizationService.localize('Policy::Policy:Terminated'), value: PolicyStatus.Terminated },
      { label: this.localizationService.localize('Policy::Policy:Cancelled'), value: PolicyStatus.Cancelled }
    ];
  }

  /**
   * Initialize sell type options with localized labels
   */
  private initializeSellTypeOptions(): void {
    this.sellTypeOptions = [
      { label: this.localizationService.localize('Policy::Policy:SellTypeAgency'), value: PolicySellType.Agency },
      { label: this.localizationService.localize('Policy::Policy:SellTypeDirect'), value: PolicySellType.Direct },
      { label: this.localizationService.localize('Policy::Policy:SellTypeIndirect'), value: PolicySellType.Indirect }
    ];
  }

  /**
   * Initialize contract type options with localized labels
   */
  private initializeContractTypeOptions(): void {
    this.contractTypeOptions = [
      { label: this.localizationService.localize('Policy::Policy:ContractTypeIndividual'), value: PolicyContractType.Individual },
      { label: this.localizationService.localize('Policy::Policy:ContractTypeGroup'), value: PolicyContractType.Group }
    ];
  }

  /**
   * Initialize contract status options with localized labels
   */
  private initializeContractStatusOptions(): void {
    this.contractStatusOptions = [
      { label: this.localizationService.localize('Policy::Policy:ContractStatusQuotation'), value: PolicyContractStatus.Quotation },
      { label: this.localizationService.localize('Policy::Policy:ContractStatusDraft'), value: PolicyContractStatus.Draft },
      { label: this.localizationService.localize('Policy::Policy:ContractStatusActive'), value: PolicyContractStatus.Active },
      { label: this.localizationService.localize('Policy::Policy:ContractStatusExpired'), value: PolicyContractStatus.Expired },
      { label: this.localizationService.localize('Policy::Policy:ContractStatusTerminated'), value: PolicyContractStatus.Terminated },
      { label: this.localizationService.localize('Policy::Policy:ContractStatusCancelled'), value: PolicyContractStatus.Cancelled }
    ];
  }

  /** Giá trị khớp policy_amount.payment_status (new, paid, partial). */
  private initializePaymentStatusOptions(): void {
    this.paymentStatusOptions = [
      { label: this.localizationService.localize('Policy::Policy:PaymentStatusNew'), value: 'new' },
      { label: this.localizationService.localize('Policy::Policy:PaymentStatusInProgress'), value: 'partial' },
      { label: this.localizationService.localize('Policy::Policy:PaymentStatusDone'), value: 'paid' }
    ];
  }

  /**
   * Load primary insurance partner options from ResPartner where ResPartnerType.code = 'INSURER'
   */
  private loadPrimaryInsurancePartnerOptions(): void {
    this.partnerService.getList({
      partnerTypeCode: 'INSURER',
      status: ResPartnerStatus.Active,
      maxResultCount: 1000,
      skipCount: 0,
      sorting: 'name asc'
    }).subscribe({
      next: (result) => {
        this.primaryInsurancePartnerOptions = (result.items || []).map(partner => ({
          label: partner.name || '',
          value: partner.id || '',
          code: partner.code || ''
        }));
      },
      error: () => {
        // Silently fail
      }
    });
  }

  /**
   * Load all dropdown options for the form
   */
  protected loadDialogOptions(): void {
    // Load Line of Business options
    this.lobService.getList({
      status: ProLineOfBusinessStatus.Active,
      maxResultCount: 1000,
      skipCount: 0,
      sorting: 'name asc'
    }).subscribe({
      next: (result) => {
        const lobs = result.items || [];
        this.lobOptions = lobs.map(lob => ({
          label: lob.name || '',
          value: lob.id || ''
        }));

        const byCode = new Map<string, string>();
        lobs.forEach(lob => {
          const code = (lob.code || '').toString().trim().toUpperCase();
          const id = (lob.id || '').toString().trim();
          if (code && id) {
            byCode.set(code, id);
          }
        });

        const motorbikeLobFallbackCodes = ['MOTOR'];

        const applyDefaultLobFromConfigValue = (defaultLobCode: string) => {
          const motorbikeFallbackId = motorbikeLobFallbackCodes.map(c => byCode.get(c)).find(v => !!v);
          const resolvedId =
            (defaultLobCode && byCode.get(defaultLobCode)) ||
            (this.isMotorbikeIssuanceUi ? motorbikeFallbackId : null) ||
            byCode.get('CAR') ||
            (lobs.length > 0 ? (lobs[0].id || null) : null);

          this.defaultLobId = resolvedId || null;

          if (this.dialogMode === 'create' && (!this.formData.lobId || this.formData.lobId === '')) {
            this.formData.lobId = this.defaultLobId || '';
          }
          this.applyImplicitSearchLobDefaultIfNeeded();
          this.lobSearchDefaultsResolved = true;
        };

        const adminConfigCode = this.isMotorbikeIssuanceUi ? 'DEFAULT_LOB_MOTORBIKE' : 'DEFAULT_LOB';
        this.adminConfigService
          .getList({
            code: adminConfigCode,
            status: AdminConfigStatus.Active,
            maxResultCount: 1,
            skipCount: 0,
            sorting: 'name asc'
          })
          .subscribe({
            next: (cfgResult) => {
              const cfg = (cfgResult.items || [])[0];
              const defaultLobCode = (cfg?.value || '').toString().trim().toUpperCase();
              applyDefaultLobFromConfigValue(defaultLobCode);
            },
            error: () => {
              const carLob = lobs.find(
                lob => (lob.code || '').toString().trim().toUpperCase() === 'CAR'
              );
              this.defaultLobId =
                (this.isMotorbikeIssuanceUi
                  ? motorbikeLobFallbackCodes.map(c => byCode.get(c)).find(v => !!v)
                  : null) ||
                (carLob?.id || (lobs[0]?.id ?? null)) ||
                null;
              if (this.dialogMode === 'create' && (!this.formData.lobId || this.formData.lobId === '')) {
                this.formData.lobId = this.defaultLobId || '';
              }
              this.applyImplicitSearchLobDefaultIfNeeded();
              this.lobSearchDefaultsResolved = true;
            }
          });
      },
      error: () => {
        this.lobSearchDefaultsResolved = true;
      }
    });

    // Load Customer options (initial 50; user gõ vào ô filter để tìm theo tên → onCustomerFilter)
    this.loadCustomers();

    // Load Currency options
    this.currencyService.getList({
      maxResultCount: 1000,
      skipCount: 0,
      sorting: 'name asc'
    }).subscribe({
      next: (result) => {
        this.currencyOptions = (result.items || []).map(currency => ({
          label: currency.name || '',
          value: currency.id || '',
          code: currency.code || ''
        }));

        // Set default currency to VND if not already set (only in create mode)
        if (this.dialogMode === 'create' && !this.formData.currencyId) {
          const vndCurrency = result.items?.find(c => c.code?.toUpperCase() === 'VND');
          if (vndCurrency?.id) {
            this.formData.currencyId = vndCurrency.id;
          }
        }
      },
      error: () => {
        // Silently fail
      }
    });

    // Load Channel options
    this.channelService.getList({
      status: ResChannelStatus.Active,
      maxResultCount: 1000,
      skipCount: 0,
      sorting: 'name asc'
    }).subscribe({
      next: (result) => {
        const items = result.items || [];
        this.channelOptions = items.map((channel: { id?: string; name?: string; code?: string }) => ({
          label: channel.name || '',
          value: channel.id || '',
          code: (channel.code || '').trim().toUpperCase() || undefined
        }));
        const direct = items.find((c: { code?: string }) => (c.code || '').trim().toUpperCase() === 'DIRECT');
        this.directChannelId = direct?.id?.trim() || null;
        // Kênh load sau employee: cần tính lại DIRECT vs kênh khác cho dropdown người khai thác.
        this.applySellerOptionsFilterForChannel();
      },
      error: () => {
        // Silently fail
      }
    });

    // Load Partner options
    this.partnerService.getList({
      status: ResPartnerStatus.Active,
      maxResultCount: 1000,
      skipCount: 0,
      sorting: 'name asc'
    }).subscribe({
      next: (result) => {
        this.partnerOptions = (result.items || []).map(partner => ({
          label: partner.name || '',
          value: partner.id || ''
        }));
      },
      error: () => {
        // Silently fail
      }
    });

    // Load Policy Type options
    this.policyTypeService.getList({
      status: PolicyTypeStatus.Active,
      maxResultCount: 1000,
      skipCount: 0,
      sorting: 'name asc'
    }).subscribe({
      next: (result) => {
        const items = result.items || [];
        this.policyTypeOptions = items.map(type => ({
          label: type.name || '',
          value: type.id || ''
        }));
        const defaultType = items.find((t: { code?: string }) => (t.code || '').trim().toUpperCase() === 'BHG');
        this.defaultPolicyTypeId = defaultType?.id ?? null;
        if (this.dialogMode === 'create' && this.formData && (!this.formData.policyTypeId || this.formData.policyTypeId === '') && this.defaultPolicyTypeId) {
          this.formData.policyTypeId = this.defaultPolicyTypeId;
        }
      },
      error: () => {
        // Silently fail
      }
    });

    // Load Document Type (for upload-multiple).
    this.resDocumentTypeService.getList({
      status: ResDocumentTypeStatus.Active,
      maxResultCount: 1000,
      skipCount: 0,
      sorting: 'name asc'
    }).subscribe({
      next: (result) => {
        const items = result.items || [];
        const first = items[0];
        this.defaultUploadDocumentTypeId = first?.id || null;

        // Specifically look for type with code 'CONTRACT' for contract documents
        const hdType = items.find(t => t.code?.trim().toUpperCase() === 'CONTRACT');
        this.contractDocumentTypeId = hdType?.id || this.defaultUploadDocumentTypeId;
      },
      error: () => {
        this.defaultUploadDocumentTypeId = null;
        this.contractDocumentTypeId = null;
      }
    });

    // Load CAR object type id for riskObject.objectTypeId
    this.resObjectTypeService.getList({
      code: 'CAR',
      status: ResObjectTypeStatus.Active,
      maxResultCount: 1,
      skipCount: 0
    }).subscribe({
      next: (result) => {
        const item = (result.items || [])[0];
        this.carObjectTypeId = item?.id || null;
      },
      error: () => {
        this.carObjectTypeId = null;
      }
    });

    // Load Employee options for Seller and Implementer (only active employees); map partner → channel để lọc người khai thác theo kênh
    this.employeeService
      .getList({
        maxResultCount: 1000,
        skipCount: 0,
        status: HrEmployeeStatus.Active,
        sorting: 'fullName asc'
      })
      .pipe(
        switchMap((result) => {
          const employees = result.items || [];
          const uniquePartnerIds = [
            ...new Set(
              employees
                .map((e) => (e.partnerId || '').toString().trim())
                .filter((x) => !!x)
            )
          ];
          if (uniquePartnerIds.length === 0) {
            return of({ employees, channelByPartner: new Map<string, string>(), channelCodeById: new Map<string, string>() });
          }
          return forkJoin(
            uniquePartnerIds.map((pid) =>
              this.partnerService.get(pid).pipe(catchError(() => of(null)))
            )
          ).pipe(
            switchMap((partners) => {
              const channelByPartner = new Map<string, string>();
              for (const p of partners) {
                if (p?.id && p.channelId) {
                  channelByPartner.set(p.id, p.channelId.trim());
                }
              }

              const uniqueChannelIds = [...new Set([...channelByPartner.values()].filter((id) => !!id))];
              if (uniqueChannelIds.length === 0) {
                return of({ employees, channelByPartner, channelCodeById: new Map<string, string>() });
              }

              return forkJoin(
                uniqueChannelIds.map((id) =>
                  this.channelService.get(id).pipe(catchError(() => of(null)))
                )
              ).pipe(
                map((channels) => {
                  const channelCodeById = new Map<string, string>();
                  for (const channel of channels) {
                    const channelId = (channel?.id || '').toString().trim();
                    if (!channelId) continue;
                    const channelCode = (channel?.code || '').toString().trim().toUpperCase();
                    if (channelCode) {
                      channelCodeById.set(channelId, channelCode);
                    }
                  }
                  return { employees, channelByPartner, channelCodeById };
                })
              );
            })
          );
        })
      )
      .subscribe({
        next: ({ employees, channelByPartner, channelCodeById }) => {
          const employeeOptions = employees.map((emp) => ({
            label: emp.fullName || '',
            value: emp.id || '',
            employeeCode: emp.code || ''
          }));
          this.sellerOptionsAll = employeeOptions;
          this.implementerOptions = employeeOptions;

          this.employeeData = employees.map((emp) => {
            const pid = (emp.partnerId || '').toString().trim();
            return {
              id: emp.id || '',
              userId: emp.userId,
              partnerId: emp.partnerId,
              partnerChannelId: pid ? channelByPartner.get(pid) ?? null : null,
              partnerChannelCode: pid ? channelCodeById.get(channelByPartner.get(pid) ?? '') ?? null : null
            };
          });

          this.applySellerOptionsFilterForChannel();

          // Set default values for implementer and seller based on current user
          this.setDefaultEmployeeValues();
          // Auto-fill channel from current user's HrEmployee → ResPartner.channelId
          this.setDefaultChannelFromCurrentUser();
          // Re-fetch customer list now that employeeData is ready — needed when restrictCustomerListToCurrentSale
          // is true and the initial loadCustomers() ran before employeeData was populated.
          if (this.restrictCustomerListToCurrentSale && this.dialogMode === 'create') {
            this.loadCustomers();
          }
          // Re-run change detection so table re-evaluates Approve action visibility
          this.cdr.markForCheck();
        },
        error: () => {
          this.sellerOptionsAll = [];
          this.sellerOptions = [];
          this.implementerOptions = [];
          this.employeeData = [];
        }
      });

    // Load Province options
    this.provinceService.getList({
      status: ResProvinceStatus.Active,
      maxResultCount: 1000,
      skipCount: 0,
      sorting: 'name asc'
    }).subscribe({
      next: (result) => {
        const provinceOptions = (result.items || []).map(province => ({
          label: province.name || '',
          value: province.id || ''
        }));
        this.provinceOptions = provinceOptions;
        this.beneficiaryProvinceOptions = provinceOptions;
      },
      error: () => {
        // Silently fail
      }
    });

    // Load Contract options: search/import = full filtered list; form = Group + linked contract only
    this.policyContractService.getList({
      maxResultCount: 1000,
      skipCount: 0,
      sorting: 'creationTime desc'
    }).subscribe({
      next: (result) => {
        this.applyPolicyContractListResult(result.items || []);
      },
      error: () => {
        // Silently fail
      }
    });


    // Load Car Brand options
    this.carBrandService.getList({
      status: ResCarBrandStatus.Active,
      maxResultCount: 1000,
      skipCount: 0,
      sorting: 'name asc'
    }).subscribe({
      next: (result) => {
        const items = result.items || [];
        this.carBrandCodeById = {};
        items.forEach(b => {
          if (b.id && b.code) this.carBrandCodeById[b.id] = b.code;
        });
        this.carBrandOptions = items.map(brand => ({
          label: brand.name || '',
          value: brand.id || ''
        }));
      },
      error: () => {
        // Silently fail
      }
    });

    // Load Car Line options
    this.carLineService.getList({
      status: ResCarLineStatus.Active,
      maxResultCount: 1000,
      skipCount: 0,
      sorting: 'name asc'
    }).subscribe({
      next: (result) => {
        const items = result.items || [];
        this.carLineCodeById = {};
        items.forEach(l => {
          if (l.id && l.code) this.carLineCodeById[l.id] = l.code;
        });
        this.carLineOptions = items.map(line => ({
          label: line.name || '',
          value: line.id || ''
        }));
      },
      error: () => {
        // Silently fail
      }
    });

    // Load Car Group options
    this.carGroupService.getList({
      status: ResCarGroupStatus.Active,
      maxResultCount: 1000,
      skipCount: 0,
      sorting: 'name asc'
    }).subscribe({
      next: (result) => {
        const items = result.items || [];
        this.carGroupItems = items;
        this.carGroupCodeById = {};
        this.carLineIdByCarGroupId = {};
        this.carGroupIdsByCarLineId = {};
        items.forEach(g => {
          if (g.id && g.code) this.carGroupCodeById[g.id] = g.code;
          const cLineId = (g as any).carLineId || (g as any).CarLineId;
          if (g.id && cLineId) {
            this.carLineIdByCarGroupId[g.id] = cLineId;
            if (!this.carGroupIdsByCarLineId[cLineId]) this.carGroupIdsByCarLineId[cLineId] = [];
            this.carGroupIdsByCarLineId[cLineId].push(g.id);
          }
        });
        this.carGroupOptions = items.map(group => ({
          label: group.name || '',
          value: group.id || ''
        }));
      },
      error: () => {
        // Silently fail
      }
    });

    this.loadCarTypeOptions();
  }

  /**
   * Ô tô: ResCarType active. Xe máy (menu riêng): InsurerDictionary RES_MORTOR_TYPE + đối tác BH + hiệu lực.
   */
  protected loadCarTypeOptions(): void {
    if (this.isMotorbikeIssuanceUi) {
      const insurerId = this.formData.primaryInsurancePartnerId?.trim();
      if (!insurerId) {
        this.carTypeOptions = [];
        this.carTypeCodeById = {};
        return;
      }
      this.policyService.getMotorbikeVehicleTypeOptions(insurerId).subscribe({
        next: items => {
          const list = items || [];
          this.carTypeCodeById = {};
          list.forEach(o => {
            const v = (o.selectValue || '').trim();
            const code = (o.carTypeCode || '').trim();
            if (v && code) {
              this.carTypeCodeById[v] = code;
            }
          });
          let options = list.map(o => ({
            label: o.label || o.carTypeCode || '',
            value: (o.selectValue || '').trim()
          }));

          const savedVt = (this.formData?.vehicleTypeId || '').trim();
          if (savedVt && this.carTypeCodeById[savedVt] === undefined) {
            this.carTypeCodeById[savedVt] = savedVt;
            options = [{ label: savedVt, value: savedVt }, ...options];
          }

          this.carTypeOptions = options.filter(o => !!o.value);
        },
        error: () => {
          this.carTypeOptions = [];
          this.carTypeCodeById = {};
        }
      });
      return;
    }

    this.carTypeService.getList({
      status: ResCarTypeStatus.Active,
      maxResultCount: 1000,
      skipCount: 0,
      sorting: 'name asc'
    }).subscribe({
      next: (result) => {
        const items = result.items || [];
        this.carTypeCodeById = {};
        items.forEach(t => {
          if (t.id && t.code) this.carTypeCodeById[t.id] = t.code;
        });
        this.carTypeOptions = items.map(type => ({
          label: type.name || '',
          value: type.id || ''
        }));
      },
      error: () => {
        // Silently fail
      }
    });
  }

  /**
   * Load admin config options for car color, plate type, and origin
   */
  private loadAdminConfigOptions(): void {
    // Load Car Color options (CAR_COLOR)
    this.adminConfigService.getList({
      code: 'CAR_COLOR',
      status: AdminConfigStatus.Active,
      maxResultCount: 1000,
      skipCount: 0,
      sorting: 'name asc'
    }).subscribe({
      next: (result) => {
        this.carColorOptions = (result.items || []).map(config => ({
          label: config.name || '',
          value: config.value || ''
        }));
      },
      error: () => {
        // Silently fail
      }
    });

    // Load Car Plate Type options (CAR_PLATE_TYPE)
    this.adminConfigService.getList({
      code: 'CAR_PLATE_TYPE',
      status: AdminConfigStatus.Active,
      maxResultCount: 1000,
      skipCount: 0,
      sorting: 'name asc'
    }).subscribe({
      next: (result) => {
        this.carPlateTypeOptions = (result.items || []).map(config => ({
          label: config.name || '',
          value: config.value || ''
        }));
      },
      error: () => {
        // Silently fail
      }
    });

    // Load Car Origin options (CAR_ORG)
    this.adminConfigService.getList({
      code: 'CAR_ORG',
      status: AdminConfigStatus.Active,
      maxResultCount: 1000,
      skipCount: 0,
      sorting: 'name asc'
    }).subscribe({
      next: (result) => {
        this.carOriginOptions = (result.items || []).map(config => ({
          label: config.name || '',
          value: config.value || ''
        }));
      },
      error: () => {
        // Silently fail
      }
    });

    // Load NOTIFY_RENEWAL_DAYS for Renewal button visibility (expireDate <= today + n days)
    this.adminConfigService.getList({
      code: 'NOTIFY_RENEWAL_DAYS',
      status: AdminConfigStatus.Active,
      maxResultCount: 1,
      skipCount: 0,
      sorting: 'name asc'
    }).subscribe({
      next: (result) => {
        const item = result.items?.[0];
        const raw = item?.value?.trim();
        if (raw !== undefined && raw !== '') {
          const n = Number(raw);
          this.notifyRenewalDays = Number.isFinite(n) && n >= 0 ? n : 0;
        }
      },
      error: () => {
        // Silently fail; Renewal button will use status-only visibility when null
      }
    });
  }

  /**
   * Load organization types for customer dialog
   * Only show "cá nhân" (Individual - CN) and "tổ chức" (Corporation - TC)
   */
  private loadOrganizationTypes(): void {
    this.organizationTypeService.getList({
      skipCount: 0,
      maxResultCount: 1000,
      sorting: 'name asc'
    }).subscribe({
      next: (result) => {
        const items = (result.items || []).filter(item => item.type === OrganizationTypeType.CN || item.type === OrganizationTypeType.TC);
        this.organizationTypeIdToType.clear();
        items.forEach(item => { if (item.id && item.type != null) this.organizationTypeIdToType.set(item.id, item.type); });
        this.organizationTypeOptions = items.map(item => ({
          label: item.name || '',
          value: item.id || ''
        }));
        const cnItem = items.find(item => item.type === OrganizationTypeType.CN);
        this.defaultOrganizationTypeIdCn = cnItem?.id || null;
      },
      error: () => {
        // Silently fail
      }
    });
  }

  /**
   * True when customer dialog organization type is Cá nhân (Individual – CN). Used to show Sex, DOB, CCCD and hide Industry, BusinessNo, TIN.
   */
  isCustomerOrgTypeIndividual(): boolean {
    if (this.customerDialogTargetField !== 'customer' || !this.customerFormData.organizationTypeId) return true;
    const type = this.organizationTypeIdToType.get(this.customerFormData.organizationTypeId);
    return type !== OrganizationTypeType.TC;
  }

  /**
   * True when customer dialog organization type is Tổ chức (Organization – TC). Used to show Industry, BusinessNo, TIN and hide Sex, DOB, CCCD.
   */
  isCustomerOrgTypeOrganization(): boolean {
    if (this.customerDialogTargetField !== 'customer' || !this.customerFormData.organizationTypeId) return false;
    return this.organizationTypeIdToType.get(this.customerFormData.organizationTypeId) === OrganizationTypeType.TC;
  }

  /**
   * Load industry options for customer dialog (res_industry, sort by name A–Z)
   */
  private loadIndustryOptions(): void {
    this.industryService.getList({
      status: ResIndustryStatus.Active,
      sorting: 'name asc',
      maxResultCount: 500,
      skipCount: 0
    }).subscribe({
      next: (result) => {
        this.industryOptions = (result.items || []).map(item => ({
          label: item.name || '',
          value: item.id || '',
          code: item.code || ''
        }));
      },
      error: () => {
        this.industryOptions = [];
      }
    });
  }

  /**
   * Initialize sex options
   */
  private initializeSexOptions(): void {
    this.sexOptions = [
      { label: this.localizationService.localize('Customer::ResCustomer:Male'), value: ResCustomerSex.Male },
      { label: this.localizationService.localize('Customer::ResCustomer:Female'), value: ResCustomerSex.Female }
    ];
  }

  /**
   * Load Car Category options filtered by Car Brand
   * NOTE: In policy form, "Hiệu xe" uses ResCarCategory (not ResCarModel).
   */
  loadCarModels(carBrandId?: string | null): void {
    if (!carBrandId) {
      this.carModelOptions = [];
      this.carModelCodeById = {};
      this.carCategorySeatNumberById = {};
      this.formData.carCategoryId = null;
      return;
    }

    this.carCategoryService.getList({
      carBrandId: carBrandId,
      status: ResCarCategoryStatus.Active,
      maxResultCount: 1000,
      skipCount: 0,
      sorting: 'name asc'
    }).subscribe({
      next: (result) => {
        const items = result.items || [];
        // overwrite model map for current brand
        this.carModelCodeById = {};
        this.carCategorySeatNumberById = {};
        items.forEach(m => {
          if (m.id && m.code) this.carModelCodeById[m.id] = m.code;
          if (m.id && typeof m.seatNumber === 'number') this.carCategorySeatNumberById[m.id] = m.seatNumber;
        });
        this.carModelOptions = items.map(model => ({
          label: model.name || '',
          value: model.id || ''
        }));
      },
      error: () => {
        this.carModelOptions = [];
        this.carModelCodeById = {};
        this.carCategorySeatNumberById = {};
      }
    });
  }

  onCarCategoryChanged(categoryId: string | null): void {
    // Auto-fill seat number from selected ResCarCategory; re-fill whenever category changes
    if (categoryId && typeof this.carCategorySeatNumberById[categoryId] === 'number') {
      this.formData.seatingCapacity = this.carCategorySeatNumberById[categoryId];
    }

    // Any car selection change should trigger pricing recalculation
    this.onInsuredObjectChange();
  }

  /**
   * Áp kết quả getList hợp đồng: cache + contractOptions (tìm kiếm) + formContractOptions (chỉ bao + đơn đang gắn).
   */
  private applyPolicyContractListResult(items: any[]): void {
    const selectedId = (this.formData?.contractId || '').toString().trim();
    const excluded = new Set<PolicyContractStatus>([
      PolicyContractStatus.Expired,
      PolicyContractStatus.Terminated,
      PolicyContractStatus.Cancelled
    ]);
    const filtered = items.filter(c => {
      const status = c?.status as any;
      const id = (c?.id || '').toString().trim();
      if (selectedId && id === selectedId) {
        return true;
      }
      return !excluded.has(status);
    });
    this.policyContractsFilteredCache = filtered;

    this.contractTypeByContractId = {};
    filtered.forEach(c => {
      if (c.id && c.type !== undefined && c.type !== null) {
        this.contractTypeByContractId[c.id] = c.type as any;
      }
    });

    this.contractOptions = filtered.map(contract => ({
      label: contract.code || '',
      value: contract.id || ''
    }));

    this.rebuildFormContractOptionsInternal();
  }

  /** Form cấp đơn: chỉ hợp đồng bao; luôn giữ hợp đồng đang gắn với đơn (sửa/xem, kể cả lẻ). */
  private rebuildFormContractOptionsInternal(): void {
    const selectedId = (this.formData?.contractId || '').toString().trim();
    const src = this.policyContractsFilteredCache || [];
    const rows = src.filter(c => {
      const id = (c?.id || '').toString().trim();
      if (selectedId && id === selectedId) {
        return true;
      }
      return (c?.type as PolicyContractType) === PolicyContractType.Group;
    });
    this.formContractOptions = rows.map(contract => ({
      label: contract.code || '',
      value: contract.id || ''
    }));
  }

  /**
   * Đồng bộ dropdown số hợp đồng trên form sau khi đã biết contractId (tránh race với getList khi mở sửa đơn).
   */
  private syncFormContractOptionsWithLinkedPolicy(): void {
    this.rebuildFormContractOptionsInternal();
    const cid = (this.formData.contractId || '').toString().trim();
    if (!cid || this.formContractOptions.some(o => o.value === cid)) {
      return;
    }
    this.policyContractService.get(cid).subscribe({
      next: contract => {
        if (!contract?.id) {
          return;
        }
        if (!this.policyContractsFilteredCache.some(x => (x?.id || '').toString().trim() === contract.id)) {
          this.policyContractsFilteredCache = [...this.policyContractsFilteredCache, contract as any];
        }
        if (contract.type != null && contract.id) {
          this.contractTypeByContractId[contract.id] = contract.type as any;
        }
        this.rebuildFormContractOptionsInternal();
      },
      error: () => {
        // ignore
      }
    });
  }

  /** Khi chọn "Hợp đồng lẻ", bỏ gắn hợp đồng bao (số hợp đồng). */
  onContractTypeChanged(contractType: PolicyContractType | null | undefined): void {
    if (contractType === PolicyContractType.Individual || contractType == null) {
      this.formData.contractId = null;
      this.formData.contractNo = null;
      this.formData.contractName = null;
      this.rebuildFormContractOptionsInternal();
    }
  }

  onContractIdChanged(contractId: string | null): void {
    const id = (contractId || '').toString().trim();
    if (!id) {
      this.formData.contractType = PolicyContractType.Individual;
      this.formData.contractNo = null;
      this.formData.contractName = null;
      this.rebuildFormContractOptionsInternal();
      return;
    }
    const contractType = this.contractTypeByContractId[id];
    if (contractType !== undefined && contractType !== null) {
      this.formData.contractType = contractType;
    }
    // Auto-fill customer (and related fields) from the selected contract
    this.policyContractService.get(id).subscribe({
      next: (contract) => {
        const customerId = contract.customerId || null;
        this.formData.customerId = customerId;
        this.formData.contractNo = contract.code || this.formData.contractNo;
        this.formData.contractType = (contract.type as PolicyContractType) ?? this.formData.contractType;
        if (contract.id && contract.type != null) {
          this.contractTypeByContractId[contract.id] = contract.type as any;
        }
        if (contract.id && !this.policyContractsFilteredCache.some(c => (c?.id || '').toString().trim() === contract.id)) {
          this.policyContractsFilteredCache = [...this.policyContractsFilteredCache, contract as any];
        }
        this.rebuildFormContractOptionsInternal();
        // Default payer, vehicle owner, beneficiary to contract's customer (same as when opening from contract detail)
        if (customerId) {
          if (!(this.formData.payerId ?? '').trim()) this.formData.payerId = customerId;
          if (!(this.formData.vehicleOwnerId ?? '').trim()) this.formData.vehicleOwnerId = customerId;
          if (!(this.formData.beneficiaryId ?? '').trim()) this.formData.beneficiaryId = customerId;
        }
      },
      error: () => {
        // Silently ignore (e.g. contract was removed); contract type from cache may still be set above
      }
    });
  }

  /**
   * Load wards for insured person
   */
  loadInsuredWards(provinceId?: string | null): void {
    if (!provinceId) {
      this.wardOptions = [];
      this.formData.insuredWardId = null;
      return;
    }

    this.wardService.getList({
      provinceId: provinceId,
      status: ResWardStatus.Active,
      maxResultCount: 1000,
      skipCount: 0,
      sorting: 'name asc'
    }).subscribe({
      next: (result) => {
        this.wardOptions = (result.items || []).map(ward => ({
          label: ward.name || '',
          value: ward.id || ''
        }));
      },
      error: () => {
        this.wardOptions = [];
      }
    });
  }

  /**
   * Load wards for beneficiary
   */
  loadBeneficiaryWards(provinceId?: string | null): void {
    if (!provinceId) {
      this.beneficiaryWardOptions = [];
      this.formData.beneficiaryWardId = null;
      return;
    }

    this.wardService.getList({
      provinceId: provinceId,
      status: ResWardStatus.Active,
      maxResultCount: 1000,
      skipCount: 0,
      sorting: 'name asc'
    }).subscribe({
      next: (result) => {
        this.beneficiaryWardOptions = (result.items || []).map(ward => ({
          label: ward.name || '',
          value: ward.id || ''
        }));
      },
      error: () => {
        this.beneficiaryWardOptions = [];
      }
    });
  }

  /**
   * Compute full address from address, ward, and province
   */
  computeFullAddress(address: string | null, wardId: string | null, provinceId: string | null, isInsured: boolean = true): void {
    // This is a simplified version - in production, you might want to fetch ward and province names
    // For now, we'll just use the address as full address
    if (isInsured) {
      this.formData.insuredFullAddress = address || '';
    } else {
      this.formData.beneficiaryFullAddress = address || '';
    }
  }

  /**
   * Initialize table columns with localized headers (25 columns for wireframe)
   */
  private initializeColumns(): void {
    this.columns = [
      {
        field: 'contractNo',
        header: this.localizationService.localize('Policy::Policy:ContractNo'),
        sortable: true,
        width: '150px',
        freeze: 'left',
        linkUrl: (row: any) => row?.contractId ? `/pages/policy/contract/${row.contractId}` : null,
        linkClass: 'text-black font-semibold hover:underline cursor-pointer'
      },
      {
        field: 'policyNo',
        header: this.localizationService.localize('Policy::Policy:PolicyNo'),
        sortable: true,
        width: '150px',
        linkUrl: (row: any) => this.getPolicyViewUrl(row),
        linkClass: 'text-black font-semibold hover:underline cursor-pointer'
      },
      {
        field: 'certificateNo',
        header: this.localizationService.localize('Policy::Policy:CertificateNo'),
        sortable: false,
        width: '150px',
        formatter: (value: any, row: any) =>
          value ?? (row as { policyCertificate?: { certificateNo?: string } }).policyCertificate?.certificateNo ?? ''
      },
      {
        field: 'insurerPolicyNo',
        header: this.localizationService.localize('Policy::Policy:RootPolicyNo'),
        sortable: false,
        width: '150px'
      },
      {
        field: 'productName',
        header: this.localizationService.localize('Policy::Policy:ProductName'),
        sortable: false,
        width: '200px'
      },
      {
        field: 'rootInsuranceName',
        header: this.localizationService.localize('Policy::Policy:RootInsuranceName'),
        sortable: false,
        width: '150px'
      },
      {
        field: 'customerName',
        header: this.localizationService.localize('Policy::Policy:CustomerName'),
        sortable: false,
        width: '200px'
      },
      {
        field: 'orgEffectDate',
        header: this.localizationService.localize('Policy::Policy:OrgEffectDate'),
        sortable: true,
        type: 'date',
        dateFormat: 'dd/MM/yyyy HH:mm:ss',
        width: '160px'
      },
      {
        field: 'orgExpireDate',
        header: this.localizationService.localize('Policy::Policy:OrgExpireDate'),
        sortable: true,
        type: 'date',
        dateFormat: 'dd/MM/yyyy HH:mm:ss',
        width: '160px'
      },
      {
        field: 'vehiclePlate',
        header: this.localizationService.localize('Policy::Policy:VehiclePlate'),
        sortable: false,
        width: '120px'
      },
      {
        field: 'chassisEngine',
        header: this.localizationService.localize('Policy::Policy:ChassisEngine'),
        sortable: false,
        width: '180px',
        formatter: (value: any, row: any) => {
          const chassis = row.chassisNumber || '';
          const engine = row.engineNumber || '';
          return chassis && engine ? `${chassis}/${engine}` : chassis || engine || '';
        }
      },
      {
        field: 'premiumTotal',
        header: this.localizationService.localize('Policy::Policy:PremiumTotal'),
        sortable: false,
        width: '150px',
        type: 'number',
        formatter: (value: any) => {
          return value ? new Intl.NumberFormat('vi-VN').format(value) : '';
        }
      },
      {
        field: 'channelName',
        header: this.localizationService.localize('Policy::Policy:ChannelName'),
        sortable: false,
        width: '150px'
      },
      {
        field: 'implementerName',
        header: this.localizationService.localize('Policy::Policy:ImplementerName'),
        sortable: false,
        width: '150px'
      },
      {
        field: 'policyIssuerName',
        header: this.localizationService.localize('Policy::Policy:PolicyIssuerName'),
        sortable: false,
        width: '150px'
      },
      {
        field: 'version',
        header: this.localizationService.localize('Policy::Policy:Version'),
        sortable: false,
        width: '100px',
        align: 'center'
      },
      {
        field: 'isRootPolicy',
        header: this.localizationService.localize('Policy::Policy:IsRootPolicy'),
        sortable: false,
        width: '120px',
        align: 'center',
        type: 'boolean'
      },
      {
        field: 'status',
        header: this.localizationService.localize('Policy::Policy:Status'),
        sortable: false,
        type: 'text',
        width: '120px',
        freeze: 'right',
        align: 'center',
        formatter: (value: any) => this.formatStatus(value),
        cellClass: (value: any) => {
          const statusValue = typeof value === 'number' ? value : 0;
          if (statusValue === PolicyStatus.Active) {
            return 'px-2 py-1 rounded text-xs font-semibold bg-green-100 text-green-800';
          } else if (statusValue === PolicyStatus.Expired || statusValue === PolicyStatus.Cancelled || statusValue === PolicyStatus.Terminated) {
            return 'px-2 py-1 rounded text-xs font-semibold bg-red-100 text-red-800';
          } else {
            return 'px-2 py-1 rounded text-xs font-semibold bg-gray-100 text-gray-800';
          }
        }
      },
      {
        field: 'approvalStatus',
        header: this.localizationService.localize('Policy::Policy:ApprovalStatus'),
        sortable: false,
        width: '150px',
        freeze: 'right',
        align: 'center',
        formatter: (value: any, row: any) => this.getApprovalStatusDisplayLabel(row?.terminationStatus, value),
        cellClass: (value: any, row: any) => this.getApprovalStatusCellClass(row?.terminationStatus, value)
      },
      {
        field: 'paymentStatus',
        header: this.localizationService.localize('Policy::Policy:PaymentStatus'),
        sortable: false,
        width: '150px',
        formatter: (value: any) => this.formatPaymentStatus(value)
      },
      {
        field: 'approvalDate',
        header: this.localizationService.localize('Policy::Policy:ApprovalDate'),
        sortable: false,
        type: 'date',
        width: '130px'
      },
      {
        field: 'approverName',
        header: this.localizationService.localize('Policy::Policy:ApproverName'),
        sortable: false,
        width: '150px'
      },
      {
        field: 'creationTime',
        header: this.localizationService.localize('AbpIdentity::CreationTime'),
        sortable: true,
        type: 'date',
        width: '130px'
      },
      {
        field: 'creatorName',
        header: this.localizationService.localize('Policy::Policy:CreatorName'),
        sortable: false,
        width: '150px'
      }
    ];
  }

  /**
   * Renewal button is visible when policy expiration date <= current date + n days
   * (n = NOTIFY_RENEWAL_DAYS from admin_config).
   * If NOTIFY_RENEWAL_DAYS is not configured, button is shown.
   */
  private isRenewalButtonVisible(row: any): boolean {
    const n = this.notifyRenewalDays;
    if (n === null || n === undefined) return true; // config not loaded: show button

    const expireStr = row?.orgExpireDate;
    if (!expireStr) return false;

    const expireDate = new Date(expireStr);
    if (isNaN(expireDate.getTime())) return false;

    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const cutoff = new Date(today);
    cutoff.setDate(cutoff.getDate() + n);
    expireDate.setHours(0, 0, 0, 0);
    return expireDate.getTime() <= cutoff.getTime();
  }

  /** Approval status normalized to lowercase for comparison. */
  private getApprovalStatusLower(row: any): string {
    return (row?.approvalStatus ?? '').toString().trim().toLowerCase();
  }

  /** Draft + (trạng thái duyệt trống hoặc từ chối). */
  private isPolicyDraftNotPending(row: any): boolean {
    if (row?.status !== PolicyStatus.Draft) return false;
    const a = this.getApprovalStatusLower(row);
    return a !== 'pending';
  }

  /** Draft + chờ duyệt. */
  private isPolicyDraftPending(row: any): boolean {
    return row?.status === PolicyStatus.Draft && this.getApprovalStatusLower(row) === 'pending';
  }

  /** Current user's HrEmployee id (from configState + employeeData); null if not found. */
  private getCurrentUserEmployeeId(): string | null {
    const currentUser = this.configState.getOne('currentUser');
    if (!currentUser?.id) return null;
    const userIdLower = (currentUser.id ?? '').toString().toLowerCase();
    return this.employeeData.find(emp => (emp.userId ?? '').toString().toLowerCase() === userIdLower)?.id ?? null;
  }

  /** Current user's HrEmployee row (from configState + employeeData); null if not found. */
  private getCurrentUserEmployeeData():
    | { id: string; userId?: string; partnerId?: string; partnerChannelId?: string | null; partnerChannelCode?: string | null }
    | null {
    const currentUser = this.configState.getOne('currentUser');
    if (!currentUser?.id) return null;
    const userIdLower = (currentUser.id ?? '').toString().toLowerCase();
    return this.employeeData.find(emp => (emp.userId ?? '').toString().toLowerCase() === userIdLower) ?? null;
  }

  /**
   * Apply saleId filter only when create-mode user has a partner channel and that channel is not DIRECT.
   * DIRECT or unassigned partner/channel means show all customers.
   */
  private shouldApplySaleIdFilterForCustomerList(): boolean {
    if (!this.restrictCustomerListToCurrentSale || this.dialogMode !== 'create') return false;
    const currentEmp = this.getCurrentUserEmployeeData();
    if (!currentEmp) return false;

    const partnerId = (currentEmp.partnerId || '').toString().trim();
    const partnerChannelId = (currentEmp.partnerChannelId || '').toString().trim();
    if (!partnerId || !partnerChannelId) return false;

    return (currentEmp.partnerChannelCode || '').toString().trim().toUpperCase() !== 'DIRECT';
  }

  /** True when the row's current work-task assignee is the current user (Approve action only for assignee). */
  private isCurrentUserAssigneeForApproval(row: any): boolean {
    const assigneeId = (row?.currentWorkTaskAssigneeId ?? '').toString().trim().toLowerCase();
    const currentEmpId = (this.getCurrentUserEmployeeId() ?? '').toString().toLowerCase();
    if (!assigneeId || !currentEmpId) return false;
    return assigneeId === currentEmpId;
  }

  /** True when "TT Duyệt" shows "Chờ duyệt chấm dứt" (termination pending). */
  private isTerminationPending(row: any): boolean {
    return this.normalizeTerminationStatus(row?.terminationStatus) === PolicyTerminationStatus.Pending;
  }

  /** Đơn đã hết hạn theo ngày (orgExpireDate < today). */
  private isPolicyExpiredByDate(row: any): boolean {
    const expireStr = row?.orgExpireDate;
    if (!expireStr) return false;
    const expireDate = new Date(expireStr);
    if (isNaN(expireDate.getTime())) return false;
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    expireDate.setHours(0, 0, 0, 0);
    return expireDate.getTime() < today.getTime();
  }

  /** Active và chưa hết hạn (theo ngày). */
  private isPolicyActiveNotExpired(row: any): boolean {
    return this.isPolicyStatusActive(row?.status) && !this.isPolicyExpiredByDate(row);
  }

  /** Kiểm tra status = Active (chấp nhận cả enum number và string từ API). */
  private isPolicyStatusActive(status: unknown): boolean {
    if (status === PolicyStatus.Active) return true;
    if (typeof status === 'string' && status.toString().toLowerCase() === 'active') return true;
    return false;
  }

  /** Row là version lớn nhất (đơn gắn với policy qua last_version_id). */
  private isLatestVersion(row: any): boolean {
    return row?.isLatestVersion === true;
  }

  /** Hiển thị / bật nút SĐBS (endorsement) – cùng logic với nút SĐBS trên danh sách: Active + chưa hết hạn + là version mới nhất + không đang chờ duyệt chấm dứt. */
  protected get detailSdbsEnabled(): boolean {
    if (this.formData == null || this.currentPolicyDetail == null) return false;
    return this.isPolicyActiveNotExpired(this.formData)
      && !this.isTerminationPending(this.currentPolicyDetail)
      && this.isLatestVersion(this.currentPolicyDetail);
  }

  /** Hiển thị hàng Tổng tăng/giảm phí trong Summary khi xem đơn SĐBS (view mode, có endorsementType hoặc endorsementAdjustmentAmount). */
  protected get showEndorsementTotalPremiumChange(): boolean {
    if (!this.isViewMode || !this.currentPolicyDetail) return false;
    const vd = this.currentPolicyDetail.versionDetail;
    const amt = this.currentPolicyDetail.amount;
    return !!(vd?.endorsementType != null || (amt != null && amt.endorsementAdjustmentAmount !== undefined && amt.endorsementAdjustmentAmount !== null));
  }

  /** Giá trị Tổng tăng/giảm phí khi xem đơn SĐBS (từ policy_amount.endorsementAdjustmentAmount). */
  protected get displayTotalPremiumChangeForView(): number {
    const amt = this.currentPolicyDetail?.amount;
    if (amt == null || amt.endorsementAdjustmentAmount === undefined || amt.endorsementAdjustmentAmount === null)
      return 0;
    return Number(amt.endorsementAdjustmentAmount);
  }

  /** Trạng thái Hủy hoặc Đã chấm dứt. */
  private isPolicyCancelledOrTerminated(row: any): boolean {
    return row?.status === PolicyStatus.Cancelled || row?.status === PolicyStatus.Terminated;
  }

  /** Trạng thái Expired (đơn đã hết hạn). */
  private isPolicyExpiredStatus(row: any): boolean {
    return row?.status === PolicyStatus.Expired;
  }

  private isUnpaidWithProviderPayment(row: any): boolean {
    const paymentStatus = (row?.paymentStatus ?? '').toString().trim().toLowerCase();
    const providerPayment = (row?.paymentProvider ?? '').toString().trim();
    return paymentStatus === 'new' && providerPayment.length > 0;
  }

  /**
   * Initialize actions based on permissions.
   * Visibility rules: (1) Draft + empty/rejected → View, Edit, Cancel; (2) Draft + pending → View, Approve;
   * (3) Active → View, Terminate, SĐBS, Cancel, Update payment, Download cert, Sync; (4) Cancelled/Terminated → View, Renew, Download cert;
   * (5) Active expiring soon → (3) + Renew; (6) Expired → View, Renew, Download cert.
   */
  private initializeActions(): void {
    // View – always
    this.actions.push({
      label: this.localizationService.localize('Policy::Policy:View'),
      icon: 'pi pi-eye',
      command: (row) => this.openViewPage(row)
    });

    // Edit – (1) Draft + (empty or rejected); hide when termination pending
    if (this.permissionService.isGranted(this.PERMISSIONS.UPDATE)) {
      this.actions.push({
        label: this.localizationService.localize('Policy::Edit'),
        icon: 'pi pi-pencil',
        visible: (row) => !this.isTerminationPending(row) && this.isPolicyDraftNotPending(row),
        command: (row) => this.openEditPage(row)
      });
    }

    // Approve – (2) Draft + pending or termination pending, and current user is the latest work_task assignee
    if (this.permissionService.isGranted('PolicyPolicy.RequestApproval.Approve')) {
      this.actions.push({
        label: this.localizationService.localize('Policy::Policy:Approve'),
        icon: 'pi pi-check',
        visible: (row) => (this.isPolicyDraftPending(row) || this.isTerminationPending(row)) && this.isCurrentUserAssigneeForApproval(row),
        command: (row) => {
          const id = (row?.id || '').trim();
          if (!id) return;
          const workTaskId = row?.currentWorkTaskId?.trim();
          if (workTaskId) {
            window.open(`/pages/policy/policy-request-approval/detail/${workTaskId}`, '_blank');
          } else {
            window.open(`/pages/policy/request-approval?policyId=${id}`, '_blank');
          }
        }
      });
    }

    // Terminate – (3) Active and not expired + chỉ version lớn nhất (last_version_id); hide when termination pending
    if (this.permissionService.isGranted(this.PERMISSIONS.UPDATE)) {
      this.actions.push({
        label: this.localizationService.localize('Policy::Policy:TerminatePolicy'),
        icon: 'pi pi-stop',
        visible: (row) => !this.isTerminationPending(row) && this.isPolicyActiveNotExpired(row) && this.isLatestVersion(row),
        command: (row) => this.openTerminationPage(row)
      });
    }

    // Submit for approval – (1) Draft + (empty or rejected); hide when termination pending
    this.actions.push({
      label: this.localizationService.localize('Policy::Policy:SubmitForApproval'),
      icon: 'pi pi-check',
      visible: (row) => !this.isTerminationPending(row) && this.isPolicyDraftNotPending(row),
      command: (row) => this.openApprovalModalForRow(row)
    });

    // Separator – hide when termination pending so menu does not show a lone separator
    this.actions.push({
      separator: true,
      visible: (row) => !this.isTerminationPending(row)
    });

    // Endorsement (SĐBS) – Active + chưa hết hạn + chỉ version lớn nhất (last_version_id); hide when termination pending
    this.actions.push({
      label: this.localizationService.localize('Policy::Policy:Endorsement'),
      icon: 'pi pi-file-edit',
      visible: (row) => !this.isTerminationPending(row) && this.isPolicyActiveNotExpired(row) && this.isLatestVersion(row),
      command: (row) => this.openEndorsementPage(row)
    });

    // Cancel – (1) Draft not pending only; hidden when status is Active (Hiệu lực) or termination pending
    this.actions.push({
      label: this.localizationService.localize('Policy::Policy:Cancel'),
      icon: 'pi pi-times',
      visible: (row) => !this.isTerminationPending(row) && this.isPolicyDraftNotPending(row),
      command: (row) => {
        const id = (row?.id || '').trim();
        if (!id) return;

        this.confirmationService.confirm({
          message: this.localizationService.localize('Policy::Policy:CancelConfirm'),
          header: this.localizationService.localize('Policy::Policy:Cancel'),
          icon: 'pi pi-exclamation-triangle',
          acceptButtonStyleClass: 'p-button-danger',
          accept: () => {
            this.loading = true;
            this.restService.request<any, void>({
              method: 'POST',
              url: `/api/policy/policies/${id}/cancel`,
              body: {}
            }).subscribe({
              next: () => {
                this.messageService.add({
                  severity: 'success',
                  summary: this.localizationService.localize('Policy::Success'),
                  detail: this.localizationService.localize('Policy::Policy:CancelledSuccessfully')
                });
                this.search();
              },
              error: (error) => {
                const errorMessage = error.error?.error?.message ||
                  error.error?.error?.details ||
                  this.localizationService.localize('AbpUi::InternalServerErrorMessage');
                this.messageService.add({
                  severity: 'error',
                  summary: this.localizationService.localize('Policy::Error'),
                  detail: errorMessage
                });
                this.loading = false;
              }
            });
          }
        });
      }
    });

    // Update payment – (3) Active; hide when termination pending
    this.actions.push({
      label: this.localizationService.localize('Policy::Policy:UpdatePayment'),
      icon: 'pi pi-money-bill',
      visible: (row) => !this.isTerminationPending(row) && row?.status === PolicyStatus.Active,
      command: (row) => {
        const id = (row?.id || '').trim();
        if (id) {
          this.paymentUpdateModal?.open(id, 'single');
        }
      }
    });

    this.actions.push({
      label: 'Check thanh toán',
      icon: 'pi pi-wallet',
      visible: (row) =>
        !this.isTerminationPending(row) &&
        row?.status === PolicyStatus.Active &&
        this.isUnpaidWithProviderPayment(row),
      command: (row) => {
        const paymentId = (row?.paymentId || '').toString().trim();
        if (!paymentId) {
          this.messageService.add({
            severity: 'warn',
            summary: this.localizationService.localize('Policy::Error'),
            detail: 'Không tìm thấy paymentId để kiểm tra giao dịch'
          });
          return;
        }

        this.loading = true;
        this.restService.request<{ paymentId: string }, void>({
          method: 'POST',
          url: '/api/payment/payment-inquiry-by-id',
          body: { paymentId }
        }).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: this.localizationService.localize('Policy::Success'),
              detail: 'Đã chạy kiểm tra thanh toán'
            });
            this.search();
          },
          error: (error) => {
            const errorMessage = error?.error?.error?.message ||
              error?.error?.error?.details ||
              this.localizationService.localize('AbpUi::InternalServerErrorMessage');
            this.messageService.add({
              severity: 'error',
              summary: this.localizationService.localize('Policy::Error'),
              detail: errorMessage
            });
            this.loading = false;
          }
        });
      }
    });

    // Renewal – (4) Cancelled/Terminated, (5) Active expiring soon, (6) Expired; hide when termination pending
    this.actions.push({
      label: this.localizationService.localize('Policy::Policy:Renewal'),
      icon: 'pi pi-refresh',
      visible: (row) =>
        !this.isTerminationPending(row) &&
        (this.isPolicyCancelledOrTerminated(row) ||
          this.isRenewalButtonVisible(row) ||
          this.isPolicyExpiredStatus(row)),
      command: (row) => {
        const id = (row?.id || '').trim();
        if (!id) return;
        this.openInNewTab([...this.getPolicyRoutePrefix(), 'new'], { renewFrom: id });
      }
    });

    // Download certificate – (3), (4), (5), (6); hide when termination pending
    this.actions.push({
      label: this.localizationService.localize('Policy::Policy:DownloadCertificate'),
      icon: 'pi pi-download',
      visible: (row) =>
        !this.isTerminationPending(row) &&
        (row?.status === PolicyStatus.Active ||
          this.isPolicyCancelledOrTerminated(row) ||
          this.isPolicyExpiredStatus(row)),
      command: (row) => {
        const id = (row?.id || '').toString().trim();
        if (!id) return;

        this.loading = true;
        this.policyCertificateService
          .getUrlByPolicyId(id, (row?.lastVersionId || '').toString().trim())
          .pipe(finalize(() => (this.loading = false)))
          .subscribe({
            next: (result) => {
              const url = result?.url;
              if (url) {
                this.loading = true;
                fetch(url)
                  .then((response) => {
                    if (!response.ok) throw new Error('Download failed');
                    return response.blob();
                  })
                  .then((blob) => {
                    const fileName = `GCN_${result.certificateNo || row.policyNo || id}.pdf`;
                    saveAs(blob, fileName);
                    this.loading = false;
                    this.cdr.detectChanges();
                    this.messageService.add({
                      severity: 'success',
                      summary: this.localizationService.localize('Policy::Success'),
                      detail: 'Tải giấy chứng nhận thành công'
                    });
                  })
                  .catch((error) => {
                    console.error('Download error:', error);
                    this.loading = false;
                    this.cdr.detectChanges();
                    // Fallback to window.open if fetch fails (e.g. CORS)
                    window.open(url, '_blank');
                    this.messageService.add({
                      severity: 'warn',
                      summary: this.localizationService.localize('Policy::Success'),
                      detail: 'Đang mở giấy chứng nhận trong tab mới...'
                    });
                  });
              } else {
                this.messageService.add({
                  severity: 'warn',
                  summary: this.localizationService.localize('Policy::Error'),
                  detail: 'Không tìm thấy URL giấy chứng nhận cho đơn này'
                });
              }
            },
            error: (error) => {
              const errorMessage =
                error?.error?.error?.message ||
                error?.error?.error?.details ||
                this.localizationService.localize('AbpUi::InternalServerErrorMessage');
              this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('Policy::Error'),
                detail: errorMessage
              });
            }
          });
      }
    });

    // Sync with root Insurer – (3) Active and not expired; hide when termination pending
    this.actions.push({
      label: this.localizationService.localize('Policy::Policy:SyncWithRootInsurer'),
      icon: 'pi pi-sync',
      visible: (row) => !this.isTerminationPending(row) && this.isPolicyActiveNotExpired(row),
      command: (row) => {
        const id = (row?.id || '').toString().trim();
        if (!id) {
          return;
        }
        this.loading = true;
        this.restService
          .request<unknown, unknown>({
            method: 'GET',
            url: '/api/policy/partner-policy-inquiry',
            params: { policyId: id }
          })
          .pipe(finalize(() => (this.loading = false)))
          .subscribe({
            next: () => {
              this.messageService.add({
                severity: 'success',
                summary: this.localizationService.localize('Policy::Success'),
                detail: this.localizationService.localize('Policy::Policy:SyncWithRootInsurerSuccess')
              });
            },
            error: (error) => {
              const errorMessage =
                error?.error?.error?.message ||
                error?.error?.error?.details ||
                this.localizationService.localize('AbpUi::InternalServerErrorMessage');
              this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('Policy::Error'),
                detail: errorMessage
              });
            }
          });
      }
    });
  }

  openCreatePage(): void {
    this.openInNewTab([...this.getPolicyRoutePrefix(), 'new']);
  }

  private openInNewTab(commands: any[], queryParams?: Record<string, any>): void {
    const tree = this.router.createUrlTree(commands, {
      queryParams: queryParams && Object.keys(queryParams).length ? queryParams : undefined,
    });
    const url = this.router.serializeUrl(tree);
    window.open(url, '_blank');
  }

  private getPolicyViewUrl(policy: any): string | null {
    const id = (policy?.id || '').toString().trim();
    if (!id) {
      return null;
    }
    const versionId = (policy?.lastVersionId || '').toString().trim();
    const isEndorsement = policy?.isRootPolicy === false;
    const path = isEndorsement ? 'endorsement' : 'view';
    const queryParams: Record<string, string> = {};
    if (versionId) {
      queryParams['versionId'] = versionId;
    }
    if (isEndorsement) {
      queryParams['view'] = '1';
    }
    const tree = this.router.createUrlTree([...this.getPolicyRoutePrefix(), id, path], {
      queryParams: Object.keys(queryParams).length ? queryParams : undefined,
    });
    return this.router.serializeUrl(tree);
  }

  openEditPage(policy: PolicyDto): void {
    const id = (policy?.id || '').trim();
    if (!id) {
      return;
    }
    // policy_version.type = "A" (SĐBS) → isRootPolicy = false → đi vào màn sửa đổi bổ sung
    const isEndorsement = policy?.isRootPolicy === false;
    const path = isEndorsement ? 'endorsement' : 'edit';
    const versionId = (policy?.lastVersionId as string)?.trim() || null;
    this.router.navigate([...this.getPolicyRoutePrefix(), id, path], {
      queryParams: versionId ? { versionId } : {},
    });
  }

  openViewPage(policy: PolicyDto): void {
    const id = (policy?.id || '').trim();
    if (!id) {
      return;
    }
    const versionId = (policy?.lastVersionId as string)?.trim() || null;
    // Đơn SĐBS (endorsement): xem chi tiết về màn endorsement read-only
    const isEndorsement = policy?.isRootPolicy === false;
    const path = isEndorsement ? 'endorsement' : 'view';
    const queryParams: Record<string, string> = versionId ? { versionId } : {};
    if (isEndorsement) {
      queryParams['view'] = '1';
    }
    this.openInNewTab([...this.getPolicyRoutePrefix(), id, path], queryParams);
  }

  openTerminationPage(policy: PolicyDto): void {
    const id = (policy?.id || '').trim();
    if (!id) {
      return;
    }
    this.openInNewTab([...this.getPolicyRoutePrefix(), id, 'termination']);
  }

  openEndorsementPage(policy: PolicyDto): void {
    const id = (policy?.id || '').trim();
    if (!id) {
      return;
    }
    const versionId = (policy?.lastVersionId as string)?.trim() || null;
    const queryParams: Record<string, string> = versionId ? { versionId } : {};
    this.openInNewTab([...this.getPolicyRoutePrefix(), id, 'endorsement'], queryParams);
  }

  /** Điều hướng sang màn SĐBS (endorsement) từ màn chi tiết. */
  navigateToEndorsement(): void {
    const id = (this.selectedPolicy?.id || '').trim();
    if (!id) return;
    const versionId = (this.selectedPolicy?.lastVersionId as string)?.trim() || null;
    this.router.navigate([...this.getPolicyRoutePrefix(), id, 'endorsement'], {
      queryParams: versionId ? { versionId } : {},
    });
  }

  /**
   * Gán LOB gửi ngầm khi tìm kiếm danh sách (một lần), từ DEFAULT_LOB / DEFAULT_LOB_MOTORBIKE.
   * Không gọi loadData ở đây — bảng chỉ mount sau `lobSearchDefaultsResolved` nên lazyLoad đầu đã có lobId.
   */
  private applyImplicitSearchLobDefaultIfNeeded(): void {
    if (this.searchLobDefaultApplied || !this.defaultLobId) {
      return;
    }
    this.implicitSearchLobId = this.defaultLobId;
    this.defaultImplicitSearchLobId = this.defaultLobId;
    this.searchLobDefaultApplied = true;
  }

  /**
   * Load policies with lazy loading
   */
  loadData(event: TableLazyLoadEvent): void {
    this.currentLazyLoadEvent = event;
    const generation = ++this.policyListRequestGeneration;
    this.loading = true;

    const baseInput: GetPoliciesInput = {
      skipCount: event.first || 0,
      maxResultCount: event.rows || 10,
      sorting: this.getSortingString(event),
      policyNo: this.searchForm.policyNo?.trim() || undefined,
      contractId: this.searchForm.contractId || undefined,
      lobId: this.implicitSearchLobId?.trim() || undefined,
      policyTypeId: this.searchForm.policyTypeId || undefined,
      partnerId: this.searchForm.partnerId || undefined,
      channelId: this.searchForm.channelId || undefined,
      customerId: this.searchForm.customerId || undefined,
      contractType: this.searchForm.contractType != null ? String(this.searchForm.contractType) : undefined,
      contractStatus: this.searchForm.contractStatus != null ? String(this.searchForm.contractStatus) : undefined,
      certificateNo: this.searchForm.certificateNo?.trim() || undefined,
      primaryInsurancePartnerId: this.searchForm.primaryInsurancePartnerId || undefined,
      carPlate: this.searchForm.carPlate?.trim() || undefined,
      carVin: this.searchForm.carVin?.trim() || undefined,
      carEngineNumber: this.searchForm.carEngineNumber?.trim() || undefined,
      effectiveDateFrom: this.toLocalIsoDate(this.ensureDate(this.searchForm.effectiveDateFrom as any)) || undefined,
      effectiveDateTo: this.toLocalIsoDate(this.ensureDate(this.searchForm.effectiveDateTo as any)) || undefined,
      expiryDateFrom: this.toLocalIsoDate(this.ensureDate(this.searchForm.expiryDateFrom as any)) || undefined,
      expiryDateTo: this.toLocalIsoDate(this.ensureDate(this.searchForm.expiryDateTo as any)) || undefined,
      status: this.searchForm.status ?? undefined,
      implementerId: this.searchForm.implementerId || undefined,
      paymentStatus: this.searchForm.paymentStatus?.trim() || undefined,
      importLotNumber: this.searchForm.importLotNumber?.trim() || undefined
    };

    const mapPolicyRows = (rows: any[]) =>
      (rows || []).map((row: any) => {
        const cert = row.policyCertificate ?? row.policyCertificates?.[0];
        const certNo = row.certificateNo ?? cert?.certificateNo ?? cert?.CertificateNo ?? '';
        return {
          ...row,
          certificateNo: certNo,
          insurerPolicyNo: row.insurerPolicyNo ?? ''
        };
      });

    const keywordRaw = this.quickSearchKeyword?.trim() || '';
    let input: GetPoliciesInput = { ...baseInput };
    if (keywordRaw && this.quickSearchSelectedType) {
      input = {
        ...baseInput,
        carPlate: undefined,
        carVin: undefined,
        carEngineNumber: undefined,
        certificateNo: undefined,
      };
      if (this.quickSearchSelectedType === 'carPlate') {
        input.carPlate = keywordRaw;
      } else if (this.quickSearchSelectedType === 'carVin') {
        input.carVin = keywordRaw;
      } else if (this.quickSearchSelectedType === 'carEngineNumber') {
        input.carEngineNumber = keywordRaw;
      } else if (this.quickSearchSelectedType === 'certificateNo') {
        input.certificateNo = keywordRaw;
      }
    }

    this.policyService.getList(input).subscribe({
      next: (result) => {
        if (generation !== this.policyListRequestGeneration) {
          return;
        }
        this.policies = mapPolicyRows(result.items || []);
        this.totalCount = result.totalCount || 0;
        this.loading = false;
      },
      error: (error) => {
        if (generation !== this.policyListRequestGeneration) {
          return;
        }
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Policy::Error'),
          detail: this.localizationService.localize('Policy::InternalServerErrorMessage')
        });
        this.loading = false;
      }
    });
  }

  /** Selected Đối tác BH gốc label in search form (for tooltip when truncated) */
  get primaryInsurancePartnerSearchSelectedLabel(): string | null {
    const id = this.searchForm?.primaryInsurancePartnerId ?? '';
    if (!id) return null;
    const opt = this.primaryInsurancePartnerOptions.find(o => (o.value || '') === (id || ''));
    return opt?.label ?? null;
  }

  /** Selected Customer label in search form (for tooltip when truncated) */
  get customerSearchSelectedLabel(): string | null {
    const id = this.searchForm?.customerId ?? '';
    if (!id) return null;
    const opt = this.customerOptions.find(o => (o.value || '') === (id || ''));
    return opt?.label ?? null;
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
    const keywordRaw = this.quickSearchKeyword?.trim() || '';
    if (keywordRaw && !this.quickSearchSelectedType) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Chưa chọn loại tìm kiếm',
        detail: 'Vui lòng chọn gợi ý tìm kiếm (Biển số / Số khung / Số máy / Số GCN).'
      });
      return;
    }
    if (this.currentLazyLoadEvent) {
      this.currentLazyLoadEvent.first = 0;
      this.loadData(this.currentLazyLoadEvent);
    }
  }

  /**
   * Reset search form and reload data
   */
  resetSearch(): void {
    this.implicitSearchLobId = this.defaultImplicitSearchLobId;
    this.quickSearchKeyword = null;
    this.quickSearchSelectedType = null;
    this.quickSearchSuggestions = [];
    this.searchForm = {
      policyNo: null,
      contractId: null,
      policyTypeId: null,
      partnerId: null,
      status: null,
      channelId: null,
      customerId: null,
      contractType: null,
      contractStatus: null,
      implementerId: null,
      certificateNo: null,
      effectiveDateFrom: null,
      effectiveDateTo: null,
      expiryDateFrom: null,
      expiryDateTo: null,
      carPlate: null,
      carVin: null,
      carEngineNumber: null,
      primaryInsurancePartnerId: null,
      importLotNumber: null,
      paymentStatus: null
    };
    this.search();
  }

  /**
   * Check if dialog is in view mode
   */
  get isViewMode(): boolean {
    return this.dialogMode === 'view';
  }

  /** Kênh khai thác DIRECT: được chọn người khai thác; kênh khác thì khóa chọn. */
  isDirectChannelSelected(): boolean {
    const channelId = (this.formData.channelId ?? '').toString().trim();
    if (!channelId) {
      return false;
    }

    const directIdTrim = (this.directChannelId ?? '').toString().trim();
    if (directIdTrim && channelId === directIdTrim) {
      return true;
    }

    const selectedChannel = this.channelOptions.find(o => (o.value ?? '').toString().trim() === channelId);
    const codeFromOpt = (selectedChannel?.code ?? '').trim().toUpperCase();
    return codeFromOpt === 'DIRECT';
  }

  /**
   * Open create dialog
   * @param contractId - When provided (e.g. from contract detail "Cấp đơn"), load contract and pre-fill form
   */
  openCreateDialog(contractId?: string | null): void {
    this.dialogMode = 'create';
    this.channelLockedBySeller = false;
    this.userClearedFormChannel = false;
    this.formData = this.getEmptyForm();
    // Create-mode default: version starts at 0
    this.formData.versionNo = 0;
    this.selectedPolicy = undefined;
    this.currentPolicyDetail = null;
    this.uploadedDocuments = [];
    this.uploadedDocumentIds = [];
    this.uploadedContractDocuments = [];
    this.uploadedContractDocumentIds = [];
    this.carPhotosVisible = false;
    this.carPhotos = { front: [], back: [], left: [], right: [], video: [] };
    this.uploadedPolicyFileKeys.clear();
    this.uploadedContractFileKeys.clear();
    this.productDiscountByProductId = {};
    this.productMarkupByProductId = {};
    this.loadDialogOptions();
    this.setDefaultEmployeeValues();

    const id = (contractId || '').trim();
    if (id) {
      this.loading = true;
      this.policyContractService.get(id).subscribe({
        next: (contract) => {
          this.populateFormFromContract(contract);
          this.loadProducts(); // Load products based on primaryInsurancePartnerId & lobId
          this.dialogVisible = true;
          this.loading = false;
        },
        error: (err) => {
          this.messageService.add({
            severity: 'error',
            summary: this.localizationService.localize('AbpUi::Error'),
            detail: err?.error?.error?.message || this.localizationService.localize('Policy::Policy:ContractLoadFailed') || 'Failed to load contract'
          });
          this.dialogVisible = true;
          this.loading = false;
        }
      });
    } else {
      this.dialogVisible = true;
    }
  }

  /**
   * Pre-fill policy form from contract data (when opening "Cấp đơn" from contract detail)
   */
  private populateFormFromContract(contract: PolicyContractDto): void {
    if (!contract) return;

    const empty = this.getEmptyForm();
    const customerId = contract.customerId || null;

    // Contract info
    this.formData.contractId = contract.id || null;
    this.formData.contractNo = contract.code || null;
    this.formData.contractName = contract.name || null;
    this.formData.primaryInsurancePartnerId = contract.insurerId || null;
    this.formData.lobId = contract.lobId || empty.lobId;
    this.formData.contractType = (contract.type as PolicyContractType) ?? empty.contractType;

    // Customer & payer
    this.formData.customerId = customerId;
    this.formData.payerId = customerId; // Default: payer = customer
    this.formData.payerName = contract.payerName || null;
    this.formData.payerPhone = contract.payerPhone || null;
    this.formData.payerEmail = contract.payerEmail || null;
    this.formData.payerAddress = contract.payerAddress || null;
    this.formData.payerFullAddress = contract.payerFullAddress || null;
    this.formData.payerProvinceId = contract.payerProvinceId || null;
    this.formData.payerWardId = contract.payerWardId || null;

    // Seller / implementer from contract employee (keep default from setDefaultEmployeeValues if contract has none)
    if (contract.employeeId) {
      this.formData.sellerId = contract.employeeId;
      this.formData.implementerId = contract.employeeId;
    }

    // Invoice
    this.formData.isReceiveInvoice = (contract.isReciveInvoice || 'N').toUpperCase() === 'Y';

    // Insurance period from contract effect/expire dates
    const effectDate = this.ensureDate(contract.effectDate as any);
    const expireDate = this.ensureDate(contract.expireDate as any);
    this.formData.insurancePeriodFrom = effectDate || this.getDefaultInsurancePeriodFrom();
    this.formData.insurancePeriodTo = expireDate || this.getDefaultInsurancePeriodTo();

    // Vehicle owner & beneficiary default to customer
    this.formData.vehicleOwnerId = customerId;
    this.formData.beneficiaryId = customerId;

    // Store contract type for onContractIdChanged
    if (contract.id && contract.type != null) {
      this.contractTypeByContractId[contract.id] = contract.type as any;
    }

    // Ensure loaded contract is in search + form dropdowns (if list load missed it)
    if (contract.id && contract.code && !this.contractOptions.some(o => o.value === contract.id)) {
      this.contractOptions = [{ label: contract.code, value: contract.id }, ...this.contractOptions];
    }
    if (contract.id && !this.policyContractsFilteredCache.some(c => (c?.id || '').toString().trim() === contract.id)) {
      this.policyContractsFilteredCache = [contract as any, ...this.policyContractsFilteredCache];
    }
    if (contract.id && contract.code && !this.formContractOptions.some(o => o.value === contract.id)) {
      this.formContractOptions = [{ label: contract.code, value: contract.id }, ...this.formContractOptions];
    }
  }

  /**
   * When customer is chosen, default payer + vehicle owner + beneficiary from customer
   * if they have not been selected yet.
   */
  onCustomerChanged(customerId: string | null | undefined): void {
    const id = (customerId || '').trim();
    if (!id) {
      return;
    }

    const payerId = (this.formData.payerId || '').trim();
    if (!payerId) {
      this.formData.payerId = id;
    }

    const vehicleOwnerId = (this.formData.vehicleOwnerId || '').trim();
    if (!vehicleOwnerId) {
      this.formData.vehicleOwnerId = id;
    }

    const beneficiaryId = (this.formData.beneficiaryId || '').trim();
    if (!beneficiaryId) {
      this.formData.beneficiaryId = id;
    }
  }

  /**
   * Đảm bảo các id khách (Khách hàng / Người thanh toán / Chủ xe / Người thụ hưởng) có trong mảng option của p-select.
   * getList chỉ trả tối đa 50 Active nên khi sửa đơn, khách có thể không nằm trong danh sách → select trống dù form đã có id.
   */
  private ensurePartyCustomersInSelectOptions(): void {
    const ids = [
      this.formData.customerId,
      this.formData.payerId,
      this.formData.vehicleOwnerId,
      this.formData.beneficiaryId
    ]
      .map(x => (x || '').trim())
      .filter(Boolean);
    const unique = [...new Set(ids)];
    const existing = new Set(
      (this.customerOptions || []).map(o => (o.value || '').trim()).filter(Boolean)
    );
    const missing = unique.filter(id => !existing.has(id));
    if (missing.length === 0) {
      return;
    }

    forkJoin(
      missing.map(id =>
        this.customerService.get(id).pipe(
          catchError(() => of(null))
        )
      )
    ).subscribe({
      next: (customers) => {
        const additions = (customers || [])
          .filter((c): c is Record<string, unknown> => !!c && !!(c as { id?: string }).id)
          .map(c => {
            const row = c as { id?: string; name?: string; phone?: string; email?: string; idNo?: string; address?: string; fullAddress?: string };
            return {
              label: row.name || '',
              value: row.id || '',
              phone: row.phone || '',
              email: row.email || '',
              idNo: row.idNo || '',
              address: row.fullAddress || row.address || ''
            };
          })
          .filter(o => o.value);

        if (additions.length === 0) {
          return;
        }

        const seen = new Set(additions.map(a => a.value));
        const base = this.customerOptions || [];
        const rest = base.filter(o => !seen.has(o.value));
        const merged = [...additions, ...rest];
        this.customerOptions = merged;
        this.payerOptions = merged;
        this.vehicleOwnerOptions = merged;
        this.beneficiaryOptions = merged;
        this.cdr.markForCheck();
      }
    });
  }

  /**
   * Load customer options (dùng chung cho Khách hàng, Người thanh toán, Chủ xe, Người thụ hưởng).
   * Có filter text thì gọi API với filter để tìm theo mã, tên, điện thoại, email; không thì load 50 bản ghi đầu.
   * Khi restrictCustomerListToCurrentSale = true và đang ở mode create, chỉ load khách hàng có saleId = nhân viên hiện tại.
   */
  loadCustomers(filterText?: string): void {
    const params: { status: ResCustomerStatus; maxResultCount: number; skipCount: number; sorting: string; filter?: string; saleId?: string } = {
      status: ResCustomerStatus.Active,
      maxResultCount: 50,
      skipCount: 0,
      sorting: 'name asc'
    };
    if (filterText != null && filterText.trim() !== '') {
      params.filter = filterText.trim();
    }
    if (this.shouldApplySaleIdFilterForCustomerList()) {
      const empId = this.getCurrentUserEmployeeId();
      if (empId) {
        params.saleId = empId;
      }
    }
    this.customerService.getList(params as any).subscribe({
      next: (result) => {
        const customerOptions = (result.items || []).map(customer => ({
          label: customer.name || '',
          value: customer.id || '',
          phone: customer.phone || '',
          email: customer.email || '',
          idNo: customer.idNo || '',
          address: customer.fullAddress || customer.address || ''
        }));
        this.customerOptions = customerOptions;
        this.payerOptions = customerOptions;
        this.vehicleOwnerOptions = customerOptions;
        this.beneficiaryOptions = customerOptions;
        this.ensurePartyCustomersInSelectOptions();
      },
      error: () => {
        // Silently fail
      }
    });
  }

  /**
   * Khi user gõ vào ô filter của select Khách hàng / Người thanh toán / ... → gọi API với filter để lọc theo mã, tên, điện thoại, email.
   */
  onCustomerFilter(event: { filter?: string; value?: string }): void {
    const filterText = (event?.filter ?? event?.value ?? '') as string;
    this.loadCustomers(filterText);
  }

  /**
   * Open edit dialog
   */
  openEditDialog(policy: PolicyDto): void {
    this.dialogMode = 'edit';
    this.channelLockedBySeller = false;
    this.userClearedFormChannel = false;
    this.selectedPolicy = policy;
    this.currentPolicyDetail = null;
    this.loadDialogOptions();
    this.uploadedDocuments = [];
    this.uploadedDocumentIds = [];

    const id = (policy?.id || '').trim();
    if (!id) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: 'Missing policy id'
      });
      return;
    }

    this.loading = true;
    const versionId = (policy?.lastVersionId as string)?.trim() || undefined;
    this.policyService.get(id, versionId).subscribe({
      next: (detail) => {
        this.currentPolicyDetail = detail as any;
        this.populateFormFromDetail(detail as any);
        this.dialogVisible = true;
        this.loading = false;
      },
      error: (error) => {
        console.error('Failed to load policy detail:', error);
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: 'Failed to load policy details'
        });
        this.loading = false;
      }
    });
  }

  openViewDialog(policy: PolicyDto): void {
    this.dialogMode = 'view';
    this.channelLockedBySeller = false;
    this.userClearedFormChannel = false;
    this.selectedPolicy = policy;
    this.currentPolicyDetail = null;
    this.loadDialogOptions();
    this.uploadedDocuments = [];
    this.uploadedDocumentIds = [];

    const id = (policy?.id || '').trim();
    if (!id) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: 'Missing policy id'
      });
      return;
    }

    this.loading = true;
    const versionId = (policy?.lastVersionId as string)?.trim() || undefined;
    this.policyService.get(id, versionId).subscribe({
      next: (detail) => {
        this.currentPolicyDetail = detail as any;
        this.populateFormFromDetail(detail as any);
        this.dialogVisible = true;
        this.loading = false;
      },
      error: (error) => {
        console.error('Failed to load policy detail:', error);
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: 'Failed to load policy details'
        });
        this.loading = false;
      }
    });
  }

  protected populateFormFromDetail(detail: any): void {
    const empty = this.getEmptyForm();

    const contract = detail?.contract;
    const versionDetail = detail?.versionDetail;
    const riskObject = detail?.riskObject;
    const riskMotor = riskObject?.riskObjectMotor;
    const motorCarTypeCode = (riskMotor?.carTypeCode ?? riskMotor?.CarTypeCode ?? '')
      .toString()
      .trim();

    const customerId = contract?.customerId || detail?.customerId || null;

    this.formData = {
      ...empty,
      // ContractNo select should use contract.id from detail (fallback to policy.contractId)
      contractId: contract?.id || detail?.contractId || null,
      // Optional text mirror
      contractNo: contract?.code || detail?.contractNo || null,
      contractName: contract?.name || null,
      lobId: detail?.lobId || empty.lobId,
      policyNo: detail?.policyNo || '',
      lastVersionId: detail?.lastVersionId || '0',
      versionNo: typeof versionDetail?.version === 'number' ? versionDetail.version : (versionDetail?.version ? Number(versionDetail.version) : null),
      sellType: detail?.sellType ?? empty.sellType,
      insurerPolicyNo: detail?.insurerPolicyNo || null,
      policyTypeId: detail?.policyTypeId || '',
      partnerId: detail?.partnerId || '',
      sellerId: detail?.sellerId || '',
      implementerId: detail?.implementerId || '',
      currencyId: detail?.currencyId || '',
      exchangeRate: detail?.exchangeRate ?? 1,
      status: detail?.status ?? empty.status,
      issueDate: detail?.issueDate || null,
      approvalStatus: detail?.approvalStatus || null,
      paymentStatus: detail?.paymentStatus || null,
      orgEffectDate: detail?.orgEffectDate || '',
      orgExpireDate: detail?.orgExpireDate || '',
      isRenewal: detail?.isRenewal === 'Y',
      isGift: detail?.isGift || 'N',
      premiumTotal: detail?.premiumTotal ?? 0,
      premium: detail?.premium ?? 0,
      vat: detail?.vat ?? 0,
      discount: detail?.discount ?? null,
      discountRate: detail?.discountRate ?? null,
      isBankLoan: detail?.isBankLoan === 'Y',
      channelId: detail?.channelId || null,
      certificateNo: detail?.certificateNo ?? null,

      // Contract-derived fields used by UI
      primaryInsurancePartnerId: contract?.insurerId || detail?.partnerId || null,
      contractType: contract?.type ?? empty.contractType,
      customerId: customerId,
      // For update: use values from API detail if available.
      // If they are missing, onCustomerChanged(customerId) will auto-fill only the empty ones.
      payerId: detail?.payerId || null,
      payerName: contract?.payerName || null,
      payerPhone: contract?.payerPhone || null,
      payerEmail: contract?.payerEmail || null,
      payerAddress: contract?.payerAddress || null,
      payerFullAddress: contract?.payerFullAddress || null,
      payerProvinceId: contract?.payerProvinceId || null,
      payerWardId: contract?.payerWardId || null,

      vehicleOwnerId: detail?.vehicleOwnerId || null,
      vehicleOwnerName: riskObject?.repName || null,
      vehicleOwnerPhone: riskObject?.repPhone || null,
      vehicleOwnerEmail: riskObject?.repEmail || null,
      vehicleOwnerAddress: riskObject?.repAddress || null,
      vehicleOwnerFullAddress: riskObject?.repFullAddress || null,
      vehicleOwnerProvinceId: riskObject?.repProvinceId || null,
      vehicleOwnerWardId: riskObject?.repWardId || null,
      vehicleOwnerIdNo: riskObject?.repIdNo || null,

      beneficiaryId: detail?.beneficiaryId || null,
      isReceiveInvoice: (contract?.isReciveInvoice || 'N') === 'Y',

      insuredName: detail?.insuredName || null,
      insuredIdNo: detail?.insuredIdNo || null,
      insuredPhone: detail?.insuredPhone || null,
      insuredEmail: detail?.insuredEmail || null,
      insuredProvinceId: detail?.insuredProvinceId || null,
      insuredWardId: detail?.insuredWardId || null,
      insuredAddress: detail?.insuredAddress || null,
      insuredFullAddress: detail?.insuredFullAddress || null,

      beneficiaryName: detail?.beneficiaryName || null,
      beneficiaryIdNo: detail?.beneficiaryIdNo || null,
      beneficiaryPhone: detail?.beneficiaryPhone || null,
      beneficiaryEmail: detail?.beneficiaryEmail || null,
      beneficiaryProvinceId: detail?.beneficiaryProvinceId || null,
      beneficiaryWardId: detail?.beneficiaryWardId || null,
      beneficiaryAddress: detail?.beneficiaryAddress || null,
      beneficiaryFullAddress: detail?.beneficiaryFullAddress || null,

      vehiclePlate: riskMotor?.carPlate || detail?.vehiclePlate || null,
      vehiclePlateType: riskMotor?.carPlateType ?? null,
      engineNumber: riskMotor?.carEngineNumber || detail?.engineNumber || null,
      chassisNumber: riskMotor?.carVin || detail?.chassisNumber || null,
      seatingCapacity: riskMotor?.carSeatNumber ?? null,
      weight: riskMotor?.carPayloadCapacity ?? null,
      carValue: riskMotor?.riskObjectValue ?? null,
      carColor: riskMotor?.carColor ?? null,
      carUsage: riskMotor?.carUsage || null,

      // Car-related ID fields from API detail (for dropdown mapping)
      carBrandId: riskMotor?.carBrandId || null,
      carModelId: riskMotor?.carModelId || null,
      carCategoryId: riskMotor?.carCategoryId || null,
      // Loại xe: ô tô bind ResCarType id; xe máy bind car_type_code (own_code / SelectValue) đã lưu policy_risk_motor
      vehicleTypeId: this.isMotorbikeIssuanceUi
        ? motorCarTypeCode || riskMotor?.carTypeId || riskMotor?.CarTypeId || null
        : riskMotor?.carTypeId || riskMotor?.CarTypeId || null,
      carLineId: riskMotor?.carLineId || null,
      carGroupId: riskMotor?.carGroupId || null,

      // Additional car fields
      isNewCar:
        (riskMotor?.carNew ?? riskMotor?.isNewCar) === 'Y' ||
        (riskMotor?.carNew ?? riskMotor?.isNewCar) === true,
      productionYear: riskMotor?.carProductionYear ? new Date(riskMotor.carProductionYear) : null,
      origin: riskMotor?.carOrigin ?? null,

      // Thời hạn bảo hiểm (từ)/(đến) lấy theo policy version: effectDate, expireDate
      insurancePeriodFrom: versionDetail?.effectDate
        ? new Date(versionDetail.effectDate)
        : (detail?.orgEffectDate ? new Date(detail.orgEffectDate) : this.getDefaultInsurancePeriodFrom()),
      insurancePeriodTo: versionDetail?.expireDate
        ? new Date(versionDetail.expireDate)
        : (detail?.orgExpireDate ? new Date(detail.orgExpireDate) : this.getDefaultInsurancePeriodTo()),

      customerNotes: versionDetail?.customerNote || null,
      internalNotes: versionDetail?.internalNote || null
    };

    // Documents (IDs)
    this.uploadedDocumentIds = Array.isArray(detail?.documents)
      ? detail.documents.map((d: any) => d?.documentId).filter((x: any) => !!x)
      : [];

    this.uploadedContractDocumentIds = Array.isArray(detail?.contract?.documents)
      ? detail.contract.documents.map((d: any) => d?.documentId).filter((x: any) => !!x)
      : [];

    // Load policy document URLs so user can preview/download in UI
    this.refreshPolicyDocumentLinks();

    // Load contract document URLs so user can preview/download in UI
    this.refreshContractDocumentLinks();

    // Load risk object documents into "Ảnh xe" modal slots (best-effort).
    // Note: policy_risk_object_document has no slot metadata, so we map by mimeType/order.
    this.carPhotos = { front: [], back: [], left: [], right: [], video: [] };
    const roDocIds = Array.isArray(riskObject?.documents)
      ? riskObject.documents.map((d: any) => d?.documentId).filter((x: any) => !!x)
      : [];
    if (roDocIds.length > 0) {
      this.resDocumentService.getMultipleFiles(roDocIds).subscribe({
        next: (files) => {
          const list = (files || []).filter(f => !!f?.id);
          const videos = list.filter(f => (f.mimeType || '').toLowerCase().startsWith('video/'));
          const images = list.filter(f => (f.mimeType || '').toLowerCase().startsWith('image/'));

          // Assign first 4 images to slots (legacy mapping from existing dữ liệu)
          const slots: Array<'front' | 'back' | 'left' | 'right'> = ['front', 'back', 'left', 'right'];
          slots.forEach((s, idx) => {
            const f = images[idx];
            if (f?.id) {
              this.carPhotos[s].push({
                id: f.id,
                fileName: f.fileName,
                url: f.url,
                fileSize: f.fileSize,
                mimeType: f.mimeType
              } as any);
            }
          });

          // Assign first video (if any)
          const v = videos[0];
          if (v?.id) {
            this.carPhotos.video.push({
              id: v.id,
              fileName: v.fileName,
              url: v.url,
              fileSize: v.fileSize,
              mimeType: v.mimeType
            } as any);
          }
        },
        error: () => {
          // ignore
        }
      });
    }

    // Load wards (if any)
    if (this.formData.insuredProvinceId) {
      this.loadInsuredWards(this.formData.insuredProvinceId);
    }
    if (this.formData.beneficiaryProvinceId) {
      this.loadBeneficiaryWards(this.formData.beneficiaryProvinceId);
    }

    // Load car categories if carBrandId is set (needed for "Hiệu xe" dropdown)
    if (this.formData.carBrandId) {
      this.loadCarModels(this.formData.carBrandId);
    }

    // Products + coverages: we must first load available products by lob/partner,
    // then patch the selection + insuranceAmount/quantity from detail.
    this.pendingDetailForEdit = detail;
    this.applySellerOptionsFilterForChannel();
    this.loadProducts();

    this.ensurePartyCustomersInSelectOptions();
    this.syncFormContractOptionsWithLinkedPolicy();
  }

  /**
   * Pre-fill form from policy detail for renewal: new policy (no id).
   * Start date: if source policy end date (calendar day) <= today → now; if still in force → source end datetime.
   * End date: start + 1 year. User can then save to call create API.
   */
  protected populateFormFromDetailForRenewal(detail: any): void {
    this.populateFormFromDetail(detail);
    this.formData.policyNo = '';
    this.formData.lastVersionId = '0';
    this.formData.versionNo = 0;
    this.formData.contractId = null;
    this.formData.contractNo = null;
    this.formData.contractName = null;

    const versionDetail = detail?.versionDetail;
    const sourceExpireDate = versionDetail?.expireDate
      ? this.ensureDate(versionDetail.expireDate)
      : this.ensureDate(detail?.orgExpireDate);

    const now = new Date();
    let periodFrom = now;
    if (sourceExpireDate) {
      const expireDay = this.toLocalIsoDate(sourceExpireDate);
      const todayDay = this.toLocalIsoDate(now);
      if (expireDay && todayDay && expireDay > todayDay) {
        periodFrom = new Date(sourceExpireDate.getTime());
      }
    }

    this.formData.insurancePeriodFrom = periodFrom;
    const periodTo = new Date(periodFrom);
    periodTo.setFullYear(periodTo.getFullYear() + 1);
    this.formData.insurancePeriodTo = periodTo;
    this.formData.isRenewal = true;
    // Đơn tái tục là đơn mới (nháp): luôn Draft và chưa gửi duyệt để hiển thị nút Lưu nháp / Lưu & T.Duyệt
    this.formData.status = PolicyStatus.Draft;
    this.formData.approvalStatus = null;
    this.selectedPolicy = undefined;
    this.currentPolicyDetail = null;

    // Clear documents khi tái tục: không mang theo tài liệu đính kèm từ đơn cũ
    this.uploadedDocuments = [];
    this.uploadedDocumentIds = [];
    this.uploadedContractDocuments = [];
    this.uploadedContractDocumentIds = [];
    this.uploadedPolicyFileKeys.clear();
    this.uploadedContractFileKeys.clear();
    this.carPhotos = { front: [], back: [], left: [], right: [], video: [] };
    this.carPhotosVisible = false;
  }

  private applyDetailToLoadedProducts(detail: any): void {
    const products = Array.isArray(detail?.products) ? detail.products : [];

    products.forEach((p: any) => {
      const productId = p?.productId;
      if (!productId || this.selectedProducts[productId] === undefined) {
        return;
      }

      this.selectedProducts[productId] = true;
      const discountVal = p?.discount;
      if (discountVal != null && !Number.isNaN(Number(discountVal))) {
        this.productDiscountByProductId[productId] = Number(discountVal);
      }
      const markupVal = p?.markup;
      if (markupVal != null && !Number.isNaN(Number(markupVal))) {
        this.productMarkupByProductId[productId] = Number(markupVal);
      }

      const coverages = Array.isArray(p?.coverages) ? p.coverages : [];
      const byCoverageId = new Map<string, any>(
        coverages
          .filter((c: any) => !!c?.coverageId)
          .map((c: any) => [String(c.coverageId), c])
      );

      const items = this.productCoverages[productId] || [];
      items.forEach((item: any) => {
        const covId = item?.coverageId;
        if (!covId) return;
        const dc = byCoverageId.get(String(covId));
        if (!dc) return;

        if (dc.amountLiability !== undefined && dc.amountLiability !== null) {
          item.insuranceAmount = dc.amountLiability;
          this.normalizeInsuranceAmountToLiabilityOptions(item);
        }

        if (dc.quantity !== undefined && dc.quantity !== null) {
          item.quantity = dc.quantity;
        }

        // Map pricing fields back to UI (premiumOrigin + vatOrigin = snapshot for Tăng/Giảm phí)
        if (dc.premium !== undefined && dc.premium !== null) {
          item.premium = dc.premium;
          item.premiumOrigin = dc.premium;
          item.vatOrigin = dc.vat ?? 0;
          // Use API's per-coverage premiumChange when present (endorsement view/update: computed from previous version)
          item.premiumChange = (dc as any).premiumChange !== undefined && (dc as any).premiumChange !== null
            ? (dc as any).premiumChange
            : 0;
        }
        if (dc.vat !== undefined && dc.vat !== null) {
          item.vat = dc.vat;
        }
        if (dc.premiumTotal !== undefined && dc.premiumTotal !== null) {
          item.premiumWithVAT = dc.premiumTotal;
        } else if (dc.premium !== undefined && dc.premium !== null) {
          item.premiumWithVAT = (dc.premium || 0) + (dc.vat || 0);
        }
        // When API sends premiumChange (endorsement), set origin so that origin + change = current (for display consistency)
        const apiPremiumChange = (dc as any).premiumChange;
        if (apiPremiumChange !== undefined && apiPremiumChange !== null && typeof apiPremiumChange === 'number') {
          const currentTotal = (item.premium ?? 0) + (item.vat ?? 0);
          const originTotal = currentTotal - apiPremiumChange;
          if (currentTotal > 0) {
            item.premiumOrigin = (item.premium ?? 0) * (originTotal / currentTotal);
            item.vatOrigin = originTotal - (item.premiumOrigin ?? 0);
          }
        }

        // Persisted rate metadata
        item.tableRateLineId = dc.tableRateLineId || item.tableRateLineId;
        item.baseRate = dc.baseRate ?? null;
        item.flatRate = dc.flatRate ?? null;

        // Display rate in disabled "Phí BH" input: baseRate -> flatRate -> premiumRate
        const displayRate = (dc.baseRate ?? dc.flatRate ?? dc.premiumRate);
        if (displayRate !== undefined && displayRate !== null) {
          item.premiumRate = displayRate;
        }

        // Map deductible from persisted policy_coverage_level (if present) back to UI selection.
        // Option value is now term Id; find term Id in dedMap that matches saved level (typeId, basisId, fromAmount).
        const dcLevels = Array.isArray(dc.coverageLevels) ? dc.coverageLevels : [];
        const dedMap = item?.deductibleTermByValue || undefined;
        if (dcLevels.length > 0 && dedMap) {
          for (const l of dcLevels) {
            const typeId = String(l?.coverageLevelTypeId ?? l?.CoverageLevelTypeId ?? '');
            const basisId = String(l?.coverageLevelBasisId ?? l?.CoverageLevelBasisId ?? '');
            const fromVal = Number(l?.fromAmount ?? l?.FromAmount);
            if (!isFinite(fromVal)) continue;
            const termIdKey = Object.keys(dedMap).find((k) => {
              const meta = dedMap[k];
              return meta && String(meta.coverageLevelTypeId) === typeId && String(meta.coverageLevelBasisId) === basisId && Number(meta.fromAmount) === fromVal;
            });
            if (termIdKey) {
              item.deductible = termIdKey;
              break;
            }
          }
        }

        if (item.coverageType !== 0) {
          item.selected = (dc.quantity ?? 0) > 0;
        }
      });

      // Re-apply interactions after patching selections from detail
      this.applyProductCoverageInteractionRules(productId, undefined);
    });

    this.updateSummary();
  }

  /**
   * Map saved coverageLevels from policy detail to item.deductible. Used after deductible options API has returned (so deductibleTermByValue is set).
   * Fixes case: user saved deductible 500, reopen detail showed 1000 because applyDetailToLoadedProducts ran before deductibleTermByValue was available.
   */
  private applySavedDeductibleFromDetail(item: any, productId: string, detail: any): void {
    const products = Array.isArray(detail?.products) ? detail.products : [];
    const p = products.find((x: any) => String(x?.productId) === String(productId));
    const coverages = Array.isArray(p?.coverages) ? p.coverages : [];
    const dc = coverages.find((c: any) => String(c?.coverageId) === String(item?.coverageId));
    if (!dc) return;
    const dcLevels = Array.isArray(dc.coverageLevels) ? dc.coverageLevels : [];
    const dedMap = item?.deductibleTermByValue || undefined;
    if (dcLevels.length === 0 || !dedMap) return;
    for (const l of dcLevels) {
      const typeId = String(l?.coverageLevelTypeId ?? l?.CoverageLevelTypeId ?? '');
      const basisId = String(l?.coverageLevelBasisId ?? l?.CoverageLevelBasisId ?? '');
      const fromVal = Number(l?.fromAmount ?? l?.FromAmount);
      if (!isFinite(fromVal)) continue;
      const termIdKey = Object.keys(dedMap).find((k) => {
        const meta = dedMap[k];
        return meta && String(meta.coverageLevelTypeId) === typeId && String(meta.coverageLevelBasisId) === basisId && Number(meta.fromAmount) === fromVal;
      });
      if (termIdKey) {
        item.deductible = termIdKey;
        return;
      }
    }
  }

  /**
   * Save (create or update)
   */
  async save(): Promise<void> {
    if (!this.validateForm()) {
      return;
    }

    // On the form page, `loading` drives the Save button spinner.
    // Always turn it on here and turn it off on both success/error.
    this.loading = true;

    // Ensure the latest premium calculation results are applied before building payload.
    // This prevents saving 0 values when there is a pending debounce timer or in-flight request.
    try {
      await this.flushSelectedProductsPremiumRecalc();
    } catch {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Policy::Error'),
        detail: 'Failed to calculate premium'
      });
      this.loading = false;
      return;
    }

    // Validate at least one product is selected (only for create mode)
    if (this.dialogMode === 'create') {
      const selectedProductIds = this.getSelectedProductIds();
      if (selectedProductIds.length === 0) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: 'Bạn phải tham gia ít nhất một sản phẩm bảo hiểm'
        });
        this.loading = false;
        return;
      }
      this.create();
    } else {
      this.update();
    }
  }

  private beginProductPremiumCalc(productId: string): number {
    const nextSeq = (this.productPremiumCalcSeqByProductId[productId] || 0) + 1;
    this.productPremiumCalcSeqByProductId[productId] = nextSeq;

    let resolveFn: (() => void) | null = null;
    const promise = new Promise<void>((resolve) => {
      resolveFn = resolve;
    });
    this.productPremiumCalcStateByProductId[productId] = {
      seq: nextSeq,
      promise,
      resolve: () => resolveFn && resolveFn()
    };
    return nextSeq;
  }

  private finishProductPremiumCalc(productId: string, seq: number): void {
    const st = this.productPremiumCalcStateByProductId[productId];
    if (!st || st.seq !== seq) {
      return; // outdated response
    }
    try {
      st.resolve();
    } finally {
      delete this.productPremiumCalcStateByProductId[productId];
    }
  }

  protected async flushSelectedProductsPremiumRecalc(timeoutMs: number = 20000): Promise<void> {
    const selected = this.getSelectedProductIds();
    if (selected.length === 0) return;

    const waits = selected.map(productId => {
      // Flush debounce timer (if any) and force an immediate calculation
      const t = this.productPremiumRecalcTimers[productId];
      if (t) {
        clearTimeout(t);
        delete this.productPremiumRecalcTimers[productId];
      }

      // Force a fresh calculation and wait for its completion
      this.calculateProductPremium(productId);
      const p = this.productPremiumCalcStateByProductId[productId]?.promise;
      if (!p) return Promise.resolve();

      // Protect save from hanging forever
      return Promise.race([
        p,
        new Promise<void>((_, reject) => setTimeout(() => reject(new Error('premium-timeout')), timeoutMs))
      ]);
    });

    await Promise.all(waits);
  }

  /**
   * Hook for post-save navigation/refresh.
   * Default behavior (modal): close dialog and refresh listing.
   * PolicyFormPageComponent overrides this to navigate back to list.
   */
  /** Hook called after a policy detail has been fully applied to the loaded products/coverages. Override in subclasses to react. */
  protected onDetailAppliedToProducts(): void {}

  protected afterSaveSuccess(): void {
    this.dialogVisible = false;
    this.search();
  }

  /**
   * Save then submit for approval (trigger Elsa workflow).
   * Saves then calls submit-for-approval with approverId "" (no modal).
   */
  saveAndApprove(): void {
    this.confirmationService.confirm({
      message: this.localizationService.localize('Policy::Policy:SaveAndApproveConfirm'),
      header: this.localizationService.localize('AbpUi::Warning'),
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: this.localizationService.localize('AbpUi::Yes'),
      rejectLabel: this.localizationService.localize('AbpUi::No'),
      accept: () => {
        this.pendingApproverId = '';
        this.willSubmitForApprovalAfterSave = true;
        this.save();
      }
    });
  }

  /** Submit for approval from listing row (no modal; calls API with approverId ""). */
  openApprovalModalForRow(row: { id?: string }): void {
    const id = (row?.id || '').trim();
    if (!id) return;
    this.callSubmitForApproval(id, '');
  }

  /**
   * Call submit-for-approval API for a draft policy. Used after save when user chose "Save and approve" or from listing Approve action.
   * @param approverId Optional HrEmployee id; send empty string if not selected.
   */
  protected callSubmitForApproval(policyId: string, approverId?: string): void {
    const body = { approverId: approverId ?? '' };
    this.restService
      .request<any, void>({
        method: 'POST',
        url: `/api/policy/policies/${policyId}/submit-for-approval`,
        body
      }, { apiName: 'Policy' })
      .subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: this.localizationService.localize('Policy::Success'),
            detail: this.localizationService.localize('Policy::Policy:Policy:SubmittedForApproval')
          });
          this.pendingApproverId = '';
          this.afterSaveSuccess();
        },
        error: (err) => {
          const errorMessage = this.getFriendlyErrorMessage(err);
          this.messageService.add({
            severity: 'error',
            summary: this.localizationService.localize('Policy::Error'),
            detail: errorMessage
          });
          this.loading = false;
        }
      });
  }

  /**
   * Validate form
   */
  protected validateForm(): boolean {
    const localizeOrMessage = (keyOrMessage: string): string => {
      const translated = this.localizationService.localize(keyOrMessage);
      return translated === keyOrMessage ? keyOrMessage : translated;
    };

    // Define validation rules: [fieldId, fieldValue, errorMessageKey, focusElementId]
    const validations: Array<[string, any, string, string]> = [];

    // Policy No is generated by BE in create mode; not validated on client

    // Contract Type (Loại hợp đồng) - required (value can be 0 = Individual)
    validations.push([
      'formContractType',
      this.formData.contractType != null ? 1 : null,
      'Policy::Policy:ContractTypeRequired',
      'formContractType'
    ]);

    // Contract No (required when contract type is Group)
    if (this.formData.contractType === PolicyContractType.Group) {
      validations.push([
        'formContractId',
        this.formData.contractId?.trim(),
        'Policy::Policy:ContractNoRequired',
        'formContractId'
      ]);
    }

    // LOB
    validations.push([
      'formLobId',
      this.formData.lobId?.trim(),
      'Policy::Policy:LobIdRequired',
      'formLobId'
    ]);

    // Customer
    validations.push([
      'formCustomerId',
      this.formData.customerId?.trim(),
      'Policy::Policy:CustomerRequired',
      'formCustomerId'
    ]);

    // Policy Type
    validations.push([
      'formPolicyTypeId',
      this.formData.policyTypeId?.trim(),
      'Policy::Policy:PolicyTypeIdRequired',
      'formPolicyTypeId'
    ]);

    // Beneficiary — bắt buộc trừ khi có sản phẩm TNDSBB tham gia
    validations.push([
      'formBeneficiaryId',
      this.hasSelectedTndsbbProduct() ||
        this.formData.beneficiaryId?.trim() ||
        this.formData.beneficiaryName?.trim(),
      'Policy::Policy:BeneficiaryRequired',
      'formBeneficiaryId'
    ]);

    // Currency
    validations.push([
      'formCurrencyId',
      this.formData.currencyId?.trim(),
      'Policy::Policy:CurrencyRequired',
      'formCurrencyId'
    ]);

    // Exchange rate (Tỷ giá) - required
    validations.push([
      'formExchangeRate',
      (this.formData.exchangeRate != null && Number(this.formData.exchangeRate) > 0) ? 1 : null,
      'Policy::Policy:ExchangeRateRequired',
      'formExchangeRate'
    ]);

    // Sell type (Loại kênh khai thác) - required (value can be 0 = Agency)
    validations.push([
      'formSellType',
      this.formData.sellType != null ? 1 : null,
      'Policy::Policy:SellTypeRequired',
      'formSellType'
    ]);

    // Channel (Kênh khai thác) - required
    validations.push([
      'formChannelId',
      this.formData.channelId?.trim(),
      'Policy::Policy:ChannelRequired',
      'formChannelId'
    ]);

    // Seller (Người khai thác) - required
    validations.push([
      'formSellerId',
      this.formData.sellerId?.trim(),
      'Policy::Policy:SellerRequired',
      'formSellerId'
    ]);

    // Implementer (Người cấp đơn) - required
    validations.push([
      'formImplementerId',
      this.formData.implementerId?.trim(),
      'Policy::Policy:ImplementerRequired',
      'formImplementerId'
    ]);

    // Exchange rate: when currency is not VND, rate cannot be 1
    const selectedCurrency = this.currencyOptions.find(o => o.value === this.formData.currencyId?.trim());
    const currencyCode = selectedCurrency?.code?.trim().toUpperCase();
    const exchangeRateNum = Number(this.formData.exchangeRate);
    if (currencyCode && currencyCode !== 'VND' && exchangeRateNum === 1) {
      validations.push([
        'formExchangeRate',
        null,
        'Policy::Policy:ExchangeRateMustNotBeOneForNonVND',
        'formExchangeRate'
      ]);
    }

    // Khi tạo đơn: bắt buộc chọn Chủ xe từ selectbox; tái tục dùng snapshot (tên) nên chấp nhận vehicleOwnerName.
    if (this.dialogMode === 'create') {
      const vehicleOwnerOk =
        !!this.formData.vehicleOwnerId?.trim() ||
        (!!this.formData.isRenewal && !!this.formData.vehicleOwnerName?.trim());
      validations.push([
        'formVehicleOwnerId',
        vehicleOwnerOk || null,
        'Policy::Policy:VehicleOwnerRequired',
        'formVehicleOwnerId'
      ]);
    }

    // Ít nhất một trong ba (biển số, số khung, số máy) — giống form cấp đơn ô tô (kể cả màn xe máy chỉ hiện vài trường).
    const hasPlateOrChassisOrEngine = !!(
      this.formData.vehiclePlate?.trim() ||
      this.formData.chassisNumber?.trim() ||
      this.formData.engineNumber?.trim()
    );
    validations.push([
      'formVehiclePlate',
      hasPlateOrChassisOrEngine ? 1 : null,
      'Policy::Policy:VehiclePlateOrChassisOrEngineRequired',
      'formVehiclePlate'
    ]);

    // --- Insured Object Information: only validate required when get-attribute-required-spec API says so ---
    if (this.isInsuredObjectFieldRequired('carUsage')) {
      validations.push([
        'formCarUsage',
        (this.formData.carUsage || '').trim(),
        `${localizeOrMessage('Policy::Policy:BusinessPurpose')} bắt buộc phải nhập`,
        'formCarUsage'
      ]);
    }
    if (this.isInsuredObjectFieldRequired('carBrandCode')) {
      validations.push([
        'formCarBrandId',
        this.formData.carBrandId?.trim(),
        `${localizeOrMessage('Policy::Policy:CarBrand')} bắt buộc phải nhập`,
        'formCarBrandId'
      ]);
    }
    if (this.isInsuredObjectFieldRequired('carModelCode') || this.isInsuredObjectFieldRequired('carCategoryCode')) {
      validations.push([
        'formCarModelId',
        this.formData.carCategoryId?.trim(),
        `${localizeOrMessage('Policy::Policy:CarModel')} bắt buộc phải nhập`,
        'formCarModelId'
      ]);
    }
    if (this.isInsuredObjectFieldRequired('carSeatNumber')) {
      validations.push([
        'formSeatingCapacity',
        (this.formData.seatingCapacity ?? 0) > 0 ? this.formData.seatingCapacity : null,
        `${localizeOrMessage('Policy::Policy:SeatingCapacity')} bắt buộc phải nhập`,
        'formSeatingCapacity'
      ]);
    }
    if (this.isInsuredObjectFieldRequired('carPayloadCapacity')) {
      validations.push([
        'formWeight',
        (this.formData.weight ?? 0) > 0 ? this.formData.weight : null,
        `${localizeOrMessage('Policy::Policy:Weight')} bắt buộc phải nhập`,
        'formWeight'
      ]);
    }
    if (this.isInsuredObjectFieldRequired('riskObjectValue')) {
      validations.push([
        'formCarValue',
        (this.formData.carValue ?? 0) > 0 ? this.formData.carValue : null,
        `${localizeOrMessage('Policy::Policy:CarValue')} bắt buộc phải nhập`,
        'formCarValue'
      ]);
    }
    if (this.isInsuredObjectFieldRequired('carLineCode')) {
      validations.push([
        'formCarLineId',
        this.formData.carLineId?.trim(),
        `${localizeOrMessage('Policy::Policy:CarLineCode')} bắt buộc phải nhập`,
        'formCarLineId'
      ]);
    }
    if (this.isInsuredObjectFieldRequired('carGroupCode')) {
      validations.push([
        'formCarGroupId',
        this.formData.carGroupId?.trim(),
        `${localizeOrMessage('Policy::Policy:CarGroupCode')} bắt buộc phải nhập`,
        'formCarGroupId'
      ]);
    }
    if (this.isInsuredObjectFieldRequired('carTypeCode')) {
      validations.push([
        'formVehicleTypeId',
        this.formData.vehicleTypeId?.trim(),
        `${localizeOrMessage('Policy::Policy:VehicleType')} bắt buộc phải nhập`,
        'formVehicleTypeId'
      ]);
    }
    if (this.isInsuredObjectFieldRequired('carPlate')) {
      validations.push([
        'formVehiclePlate',
        (this.formData.vehiclePlate || '').trim(),
        `${localizeOrMessage('Policy::Policy:VehiclePlate')} bắt buộc phải nhập`,
        'formVehiclePlate'
      ]);
    }
    if (this.isInsuredObjectFieldRequired('carVin')) {
      validations.push([
        'formChassisNumber',
        (this.formData.chassisNumber || '').trim(),
        `${localizeOrMessage('Policy::Policy:ChassisNumber')} bắt buộc phải nhập`,
        'formChassisNumber'
      ]);
    }
    if (this.isInsuredObjectFieldRequired('carEngineNumber')) {
      validations.push([
        'formEngineNumber',
        (this.formData.engineNumber || '').trim(),
        `${localizeOrMessage('Policy::Policy:EngineNumber')} bắt buộc phải nhập`,
        'formEngineNumber'
      ]);
    }
    if (this.isInsuredObjectFieldRequired('carProductionYear')) {
      validations.push([
        'formProductionYear',
        this.formData.productionYear ? 1 : null,
        `${localizeOrMessage('Policy::Policy:ProductionYear')} bắt buộc phải nhập`,
        'formProductionYear'
      ]);
    }
    if (this.isInsuredObjectFieldRequired('carColor')) {
      validations.push([
        'formCarColor',
        this.formData.carColor?.trim(),
        `${localizeOrMessage('Policy::Policy:CarColor')} bắt buộc phải nhập`,
        'formCarColor'
      ]);
    }
    if (this.isInsuredObjectFieldRequired('carOrigin')) {
      validations.push([
        'formOrigin',
        this.formData.origin?.trim(),
        `${localizeOrMessage('Policy::Policy:Origin')} bắt buộc phải nhập`,
        'formOrigin'
      ]);
    }

    // Production year (Năm sản xuất): only allow dates not in the future
    const productionYearDate = this.formData.productionYear ? this.ensureDate(this.formData.productionYear) : null;
    const now = new Date();
    const productionYearInvalid = productionYearDate
      ? productionYearDate > now
      : false;
    if (productionYearInvalid) {
      validations.push([
        'formProductionYear',
        null,
        `${localizeOrMessage('Policy::Policy:ProductionYear')} không được lớn hơn hiện tại`,
        'formProductionYear'
      ]);
    }

    // Insurance Period From
    const from = this.ensureDate(this.formData.insurancePeriodFrom);
    validations.push([
      'formInsurancePeriodFrom',
      from,
      'Policy::Policy:OrgEffectDateRequired',
      'formInsurancePeriodFrom'
    ]);

    // effectiveDate must be >= today (only in create mode)
    if (this.dialogMode === 'create' && from) {
      const today = new Date();
      today.setHours(0, 0, 0, 0);
      const fromDateOnly = new Date(from);
      fromDateOnly.setHours(0, 0, 0, 0);

      if (fromDateOnly < today) {
        validations.push([
          'formInsurancePeriodFrom',
          null, // Mark as invalid
          'Policy::Policy:EffectiveDateMustBeGreaterOrEqualToToday',
          'formInsurancePeriodFrom'
        ]);
      }
    }

    // Insurance Period To
    const to = this.ensureDate(this.formData.insurancePeriodTo);
    validations.push([
      'formInsurancePeriodTo',
      to,
      'Policy::Policy:OrgExpireDateRequired',
      'formInsurancePeriodTo'
    ]);

    // Effective date must be <= expire date
    if (from && to && from.getTime() > to.getTime()) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: localizeOrMessage('Policy::Policy:EffectiveDateMustBeLessOrEqualExpireDate') || 'Ngày hiệu lực phải nhỏ hơn hoặc bằng ngày hết hạn.'
      });
      setTimeout(() => {
        const element = document.getElementById('formInsurancePeriodFrom');
        if (element) {
          element.scrollIntoView({ behavior: 'smooth', block: 'center' });
          const inputElement = element.querySelector('input') as HTMLInputElement;
          if (inputElement) inputElement.focus();
        }
      }, 150);
      return false;
    }

    // Find first invalid field
    for (const [fieldId, value, errorKey, focusId] of validations) {
      if (!value) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: localizeOrMessage(errorKey)
        });

        // Focus on the first invalid field
        setTimeout(() => {
          const element = document.getElementById(focusId);
          if (element) {
            // Scroll to element first
            element.scrollIntoView({ behavior: 'smooth', block: 'center' });

            // Try to find and focus the actual input element
            // For PrimeNG components, the input is usually nested
            const inputElement = element.querySelector('input') as HTMLInputElement;
            if (inputElement) {
              inputElement.focus();
              // For select/datepicker, click to open dropdown/calendar
              if (focusId.includes('select') || focusId.includes('date')) {
                inputElement.click();
              }
            } else {
              // Fallback: try to focus the element itself
              (element as HTMLElement).focus();
            }
          }
        }, 150);

        return false;
      }
    }

    // Total amount validation (create/update): TotalPolicyAmount must be > 0
    // In this form, "TotalPolicyAmount" is represented by `premiumTotal`.
    // const totalAmount = Number(this.formData.premiumTotal ?? 0);
    // if (!(totalAmount > 0)) {
    //   this.messageService.add({
    //     severity: 'error',
    //     summary: this.localizationService.localize('AbpUi::Error'),
    //     detail: `${localizeOrMessage('Policy::Policy:TotalPolicyAmount')} phải lớn hơn 0`
    //   });
    //   return false;
    // }

    if (!this.validateCoverageAvailabilityForSave(localizeOrMessage)) {
      return false;
    }

    return true;
  }

  /**
   * Parse ProProductCoverageAvailabilityType from CoverageItem (from by-lob-partner / edit detail).
   */
  protected parseCoverageAvailabilityTypeNum(item: CoverageItem): number | undefined {
    const raw = item.availabilityType as unknown;
    if (raw === undefined || raw === null || raw === '') {
      return undefined;
    }
    if (typeof raw === 'number' && !Number.isNaN(raw)) {
      return raw;
    }
    const s = String(raw).trim();
    const n = Number.parseInt(s, 10);
    if (!Number.isNaN(n) && s === String(n) && n >= 0 && n <= 3) {
      return n;
    }
    const low = s.toLowerCase();
    if (low === 'required') {
      return 0;
    }
    if (low === 'standard') {
      return 1;
    }
    if (low === 'optional') {
      return 2;
    }
    if (low === 'selectable') {
      return 3;
    }
    return undefined;
  }

  /**
   * Required: each non-main Required coverage must be selected.
   * Selectable: as a group — if the product has any Selectable coverages, at least one must be selected.
   */
  private validateCoverageAvailabilityForSave(localizeOrMessage: (k: string) => string): boolean {
    const selectedProductIds = this.getSelectedProductIds();
    for (const productId of selectedProductIds) {
      const rows = this.productCoverages[productId] || [];
      const productName =
        (this.products.find(p => p.id === productId)?.name as string | undefined)?.trim() || productId;

      for (const item of rows) {
        if (item.isSectionHeader || item.coverageType === 0) {
          continue;
        }
        const at = this.parseCoverageAvailabilityTypeNum(item);
        const label = (item.benefit || item.coverageCode || '').trim() || '—';

        if (at === 0 && item.selected !== true) {
          this.messageService.add({
            severity: 'error',
            summary: this.localizationService.localize('AbpUi::Error'),
            detail: localizeOrMessage('Policy::Policy:CoverageRequiredMustBeSelected').replace(/\{0\}/g, label)
          });
          return false;
        }
      }

      const selectableItems = rows.filter(
        item =>
          !item.isSectionHeader &&
          item.coverageType !== 0 &&
          this.parseCoverageAvailabilityTypeNum(item) === 3
      );
      if (selectableItems.length > 0 && !selectableItems.some(i => i.selected === true)) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: localizeOrMessage('Policy::Policy:SelectableCoveragesAtLeastOneRequired').replace(
            /\{0\}/g,
            productName
          )
        });
        return false;
      }
    }
    return true;
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
      if (message.startsWith('Policy:')) {
        message = message.replace('Policy:', 'Policy::');
      }
      // Policy module localization contains some keys for PolicyContract validations
      // e.g. "PolicyContract:PayerNameMaxLength"
      else if (message.startsWith('PolicyContract:')) {
        message = `Policy::${message}`;
      }
    }

    // Try to translate it
    let translated = this.localizationService.localize(message);

    // If translation returns the same key, it means it wasn't found, use original message
    if (translated === message) {
      translated = message;
    }

    // Replace placeholders if they exist in the translated message
    if (translated.includes('{PolicyNo}')) {
      const policyNoValue = error?.value || error?.error?.data?.PolicyNo || '';
      translated = translated.replace(/{PolicyNo}/g, policyNoValue);
    }

    if (translated.includes('{0}')) {
      const value = error?.value || '';
      translated = translated.replace(/\{0\}/g, value);
    }

    return translated;
  }

  protected getFriendlyErrorMessage(error: any): string {
    const info = error?.error?.error;
    const validationErrors = info?.validationErrors;
    if (Array.isArray(validationErrors) && validationErrors.length > 0) {
      // Prefer showing localized validation error(s); skip PolicyNoRequired (policyNo is generated by BE in create mode)
      const lines = validationErrors
        .filter((ve: any) => {
          const msg = String(ve?.message || '').trim();
          return msg && !/PolicyNoRequired/i.test(msg);
        })
        .map((ve: any) => this.translateErrorMessage(String(ve?.message || '').trim(), error))
        .filter((x: string) => !!x);
      if (lines.length > 0) {
        return lines.join('\n');
      }
    }

    let msg =
      info?.message ||
      info?.details ||
      error?.message ||
      this.localizationService.localize('AbpUi::InternalServerErrorMessage');

    msg = this.translateErrorMessage(String(msg || '').trim(), error);
    return msg;
  }

  /**
   * Create new policy
   */
  private create(): void {
    this.loading = true;

    // Build payload based on latest mapping plan rules
    const beneficiaryId = this.formData.beneficiaryId?.trim() || null;
    const payerId = this.formData.payerId?.trim() || null;
    const vehicleOwnerId = this.formData.vehicleOwnerId?.trim() || null;

    const beneficiary$ = beneficiaryId ? this.customerService.get(beneficiaryId) : of(null);
    const payer$ = payerId ? this.customerService.get(payerId) : of(null);
    const vehicleOwner$ = vehicleOwnerId ? this.customerService.get(vehicleOwnerId) : of(null);

    forkJoin({ beneficiary: beneficiary$, payer: payer$, vehicleOwner: vehicleOwner$ }).subscribe({
      next: ({ beneficiary, payer, vehicleOwner }) => {
        const insurancePeriodFrom = this.ensureDate(this.formData.insurancePeriodFrom);
        const insurancePeriodTo = this.ensureDate(this.formData.insurancePeriodTo);

        const selectedProductIds = this.getSelectedProductIds();
        const productsPayload = selectedProductIds.map(productId => this.buildPolicyProductPayload(productId));

        // Totals from summary section (computed from selected products)
        const totals = this.getSelectedProductsTotals(selectedProductIds);

        // Contract logic: when user chose an existing contract from the selectbox, always use it
        // (send contractId). Only create a new contract when no contract is selected and type is Individual.
        const isIndividualContract = this.formData.contractType === PolicyContractType.Individual;
        const selectedContractId = (this.formData.contractId ?? '').toString().trim() || null;

        let contractId: string | null;
        let contractPayload: any = undefined;

        if (selectedContractId) {
          // User selected an existing contract (Group or Individual) -> use it, do not create new one
          contractId = selectedContractId;
          if (this.uploadedContractDocumentIds.length > 0) {
            contractPayload = {
              documents: this.uploadedContractDocumentIds.map(id => ({ documentId: id }))
            };
          }
        } else {
          // No contract selected: Individual = create new contract; Group = no contract payload
          contractId = null;
          if (isIndividualContract) {
            contractPayload = {
              insurerId: this.formData.primaryInsurancePartnerId?.trim() || null,
              lobId: this.formData.lobId?.trim() || null,
              type: this.formData.contractType,
              customerId: this.formData.customerId?.trim() || null,
              effectDate: this.toLocalIsoDateTime(insurancePeriodFrom),
              expireDate: this.toLocalIsoDateTime(insurancePeriodTo),
              isReciveInvoice: this.formData.isReceiveInvoice ? 'Y' : 'N',
              payerName: payer?.name || undefined,
              payerEmail: payer?.email || undefined,
              payerPhone: payer?.phone || undefined,
              payerProvinceId: payer?.provinceId || undefined,
              payerWardId: payer?.wardId || undefined,
              payerAddress: payer?.address || undefined,
              payerFullAddress: payer?.fullAddress || undefined,
              documents: this.uploadedContractDocumentIds.map(id => ({ documentId: id })),
            };
          } else if (this.uploadedContractDocumentIds.length > 0) {
            contractPayload = {
              documents: this.uploadedContractDocumentIds.map(id => ({ documentId: id }))
            };
          }
        }

        // RiskObject from Vehicle Owner customer (not beneficiary). Renewal may have rep* snapshot only (no vehicleOwnerId).
        const carPhotoDocIds = this.getCarPhotoDocumentIds();
        const hasRiskObjectSource =
          !!vehicleOwner ||
          !!vehicleOwnerId ||
          (!!this.formData.isRenewal && !!this.formData.vehicleOwnerName?.trim());
        const riskObjectPayload = hasRiskObjectSource
          ? ({
            // objectTypeId comes from ResObjectType where code = CAR
            objectTypeId: this.carObjectTypeId,
            repName: (vehicleOwner?.repName ?? vehicleOwner?.name ?? this.formData.vehicleOwnerName) || undefined,
            repIdNo: (vehicleOwner?.repIdNo ?? vehicleOwner?.idNo) || undefined,
            repPassport: vehicleOwner?.passportNo || undefined,
            repPhone: (vehicleOwner?.repPhone ?? vehicleOwner?.phone ?? this.formData.vehicleOwnerPhone) || undefined,
            repEmail: (vehicleOwner?.repEmail ?? vehicleOwner?.email ?? this.formData.vehicleOwnerEmail) || undefined,
            repProvinceId: (vehicleOwner?.provinceId ?? this.formData.vehicleOwnerProvinceId) || undefined,
            repWardId: (vehicleOwner?.wardId ?? this.formData.vehicleOwnerWardId) || undefined,
            repAddress: (vehicleOwner?.address ?? this.formData.vehicleOwnerAddress) || undefined,
            repFullAddress: (vehicleOwner?.fullAddress ?? this.formData.vehicleOwnerFullAddress) || undefined,
            riskObjectProvinceId: (vehicleOwner?.provinceId ?? this.formData.vehicleOwnerProvinceId) || undefined,
            riskObjectWardId: (vehicleOwner?.wardId ?? this.formData.vehicleOwnerWardId) || undefined,
            riskObjectAddress: (vehicleOwner?.address ?? this.formData.vehicleOwnerAddress) || undefined,
            riskObjectFullAddress: (vehicleOwner?.fullAddress ?? this.formData.vehicleOwnerFullAddress) || undefined,
            documents: carPhotoDocIds.length ? carPhotoDocIds.map(id => ({ documentId: id })) : undefined,
            riskObjectMotor: this.buildRiskMotorPayload(),
          } as any)
          : undefined;

        const createDto: any = {
          // Contract handling
          contractId: contractId,
          contract: contractPayload,

          // Required root fields
          lobId: this.formData.lobId?.trim() || null,
          policyNo: this.formData.policyNo?.trim() || null,

          // lastVersionId: Empty GUID on create
          lastVersionId: '00000000-0000-0000-0000-000000000000',

          // sellType comes from UI dropdown (Agency/Direct/Indirect)
          sellType: this.formData.sellType,
          channelId: this.formData.channelId?.trim() || undefined,

          insurerPolicyNo: this.formData.insurerPolicyNo?.trim() || undefined,
          certificateNo: this.formData.certificateNo?.trim() || undefined,
          sellerId: this.formData.sellerId?.trim() || null,
          implementerId: this.formData.implementerId?.trim() || null,
          currencyId: this.formData.currencyId?.trim() || null,
          exchangeRate: this.formData.exchangeRate,

          // Policy type from UI
          policyTypeId: this.formData.policyTypeId?.trim() || undefined,

          // partnerId: use Primary Insurance Partner selection
          partnerId: this.formData.primaryInsurancePartnerId?.trim() || undefined,

          approvalStatus: this.formData.approvalStatus?.trim() || undefined,

          // Dates from insurance period
          orgEffectDate: this.toLocalIsoDateTime(insurancePeriodFrom),
          orgExpireDate: this.toLocalIsoDateTime(insurancePeriodTo),

          isRenewal: this.formData.isRenewal ? 'Y' : 'N',
          isGift: this.formData.isGift || 'N',
          isBankLoan: this.formData.isBankLoan ? 'Y' : 'N',

          // Beneficiary info: save from selected beneficiary customer (or manual inputs)
          beneficiaryName: (beneficiary?.name || this.formData.beneficiaryName || undefined),
          beneficiaryIdNo: (beneficiary?.idNo || this.formData.beneficiaryIdNo || undefined),
          beneficiaryPhone: (beneficiary?.phone || this.formData.beneficiaryPhone || undefined),
          beneficiaryEmail: (beneficiary?.email || this.formData.beneficiaryEmail || undefined),
          beneficiaryProvinceId: (beneficiary?.provinceId || this.formData.beneficiaryProvinceId || undefined),
          beneficiaryWardId: (beneficiary?.wardId || this.formData.beneficiaryWardId || undefined),
          beneficiaryAddress: (beneficiary?.address || this.formData.beneficiaryAddress || undefined),
          beneficiaryFullAddress: (beneficiary?.fullAddress || this.formData.beneficiaryFullAddress || undefined),

          // Totals (also build amount object from summary totals)
          premiumTotal: totals.premiumTotal,
          premium: totals.premium,
          vat: totals.vat,
          discount: 0,
          discountRate: 0,
          markup: 0,
          amount: {
            premiumTotal: totals.premiumTotal,
            premium: totals.premium,
            vat: totals.vat,
            discount: 0,
            discountRate: 0,
            markup: 0,
          },

          // Version payload (default version = 0 on create)
          version: {
            version: 0,
            effectDate: this.toLocalIsoDateTime(insurancePeriodFrom),
            expireDate: this.toLocalIsoDateTime(insurancePeriodTo),
            orgEffectDate: this.toLocalIsoDateTime(insurancePeriodFrom),
            orgExpireDate: this.toLocalIsoDateTime(insurancePeriodTo),
            internalNote: this.formData.internalNotes || undefined,
            customerNote: this.formData.customerNotes || undefined,
            premiumTotal: totals.premiumTotal,
            premium: totals.premium,
            vat: totals.vat,
            discount: 0,
            discountRate: 0,
            markup: 0,
          },

          // Products + coverages
          products: productsPayload,

          // RiskObject from vehicle owner
          riskObject: riskObjectPayload,

          // Documents: upload-multiple returns ResDocument ids
          documents: this.uploadedDocumentIds.length
            ? this.uploadedDocumentIds.map(id => ({ documentId: id }))
            : undefined,
        };

        this.policyService.create(createDto as CreatePolicyDto).subscribe({
          next: (res: PolicyDto) => {
            this.loading = false;
            const policyId = res?.id;
            if (policyId && this.willSubmitForApprovalAfterSave) {
              this.willSubmitForApprovalAfterSave = false;
              this.messageService.add({
                severity: 'success',
                summary: this.localizationService.localize('Policy::Success'),
                detail: this.localizationService.localize('Policy::Policy:CreatedSuccessfully')
              });
              this.callSubmitForApproval(policyId, this.pendingApproverId || '');
              this.pendingApproverId = '';
              return;
            }
            this.messageService.add({
              severity: 'success',
              summary: this.localizationService.localize('Policy::Success'),
              detail: this.localizationService.localize('Policy::Policy:CreatedSuccessfully')
            });
            this.afterSaveSuccess();
          },
          error: (error) => {
            const errorMessage = this.getFriendlyErrorMessage(error);
            this.messageService.add({
              severity: 'error',
              summary: this.localizationService.localize('Policy::Error'),
              detail: errorMessage
            });
            this.loading = false;
          }
        });
      },
      error: (error) => {
        const errorMessage = this.getFriendlyErrorMessage(error);
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Policy::Error'),
          detail: errorMessage
        });
        this.loading = false;
      }
    });
  }

  protected ensureDate(value: string | Date | null | undefined): Date | null {
    if (!value) {
      return null;
    }
    if (value instanceof Date) {
      return value;
    }
    const parsed = new Date(value);
    return isNaN(parsed.getTime()) ? null : parsed;
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
  protected toLocalIsoDateTime(date: Date | null | undefined): string | null {
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

  protected getSelectedProductIds(): string[] {
    return Object.keys(this.selectedProducts || {}).filter(id => this.selectedProducts[id]);
  }

  protected getSelectedProductsTotals(productIds: string[]): { premiumTotal: number; premium: number; vat: number } {
    let premium = 0;
    let vat = 0;
    let premiumTotal = 0;

    productIds.forEach(productId => {
      const totals = this.getProductTotals(productId);
      premium += totals.premium || 0;
      vat += totals.vat || 0;
      premiumTotal += totals.premiumWithVAT || 0;
    });

    return { premiumTotal, premium, vat };
  }

  /**
   * Build attributes map (code -> value) from formData for policy create/update and validation.
   */
  protected getAttributesMap(): Record<string, any> {
    const attrs: Record<string, any> = {};
    if (this.formData.carBrandId && this.carBrandCodeById[this.formData.carBrandId]) {
      attrs['carBrand'] = this.carBrandCodeById[this.formData.carBrandId];
    }
    if (this.formData.carCategoryId && this.carModelCodeById[this.formData.carCategoryId]) {
      const catCode = this.carModelCodeById[this.formData.carCategoryId];
      attrs['carModel'] = catCode;
      attrs['carCategory'] = catCode;
    }
    if (this.formData.carLineId && this.carLineCodeById[this.formData.carLineId]) {
      attrs['carLine'] = this.carLineCodeById[this.formData.carLineId];
    }
    if (this.formData.carGroupId && this.carGroupCodeById[this.formData.carGroupId]) {
      attrs['carGroup'] = this.carGroupCodeById[this.formData.carGroupId];
    }
    if (this.formData.vehicleTypeId && this.carTypeCodeById[this.formData.vehicleTypeId]) {
      attrs['carType'] = this.carTypeCodeById[this.formData.vehicleTypeId];
    }
    if (this.formData.carUsage) attrs['carUsage'] = this.formData.carUsage;
    if (this.formData.productionYear) {
      const productionDate = this.ensureDate(this.formData.productionYear);
      if (productionDate) {
        attrs['carProductionYear'] = productionDate.getFullYear().toString();
        const carYear = new Date().getFullYear() - productionDate.getFullYear();
        if (!Number.isNaN(carYear) && carYear >= 0) attrs['carYear'] = carYear;
      }
    }
    if (this.formData.vehiclePlate) attrs['carPlate'] = this.formData.vehiclePlate;
    if (this.formData.seatingCapacity) attrs['carSeatNumber'] = this.formData.seatingCapacity;
    if (this.formData.chassisNumber) attrs['carVin'] = this.formData.chassisNumber;
    if (this.formData.engineNumber) attrs['carEngineNumber'] = this.formData.engineNumber;
    if (this.formData.weight) attrs['carPayloadCapacity'] = this.formData.weight;
    if (this.formData.carColor) attrs['carColor'] = this.formData.carColor;
    if (this.formData.origin) attrs['carOrigin'] = this.formData.origin;
    if (this.formData.isNewCar !== null && this.formData.isNewCar !== undefined) {
      attrs['isNewCar'] = this.formData.isNewCar ? 'Y' : 'N';
      attrs['carNew'] = this.formData.isNewCar ? 'Y' : 'N';
    }
    return attrs;
  }

  protected buildPolicyProductPayload(productId: string): any {
    // Filter coverages: exclude section headers, and for non-main coverages only include if selected
    // Also ensure non-main coverages that are not selected don't have insuranceAmount
    const coverages = (this.productCoverages[productId] || [])
      .filter(x => {
        if (x.isSectionHeader) return false;
        // Main coverages (coverageType === 0) are always included
        if (x.coverageType === 0) return true;
        // Non-main coverages only if selected
        return x.selected === true;
      })
      .map(x => this.buildCoveragePayload(x));

    const totals = this.getProductTotals(productId);

    // Build attributes with isRequired from product's productAttributes (by-lob-partner API)
    const product = this.products.find(p => p.id === productId);
    const attrsMap = this.getAttributesMap();
    const amountLiability = this.getMainCoverageItem(productId)?.insuranceAmount ?? 0;
    const attributes = Array.isArray(product?.productAttributes)
      ? product.productAttributes.map((pa: any) => {
          const code = pa.attributeCode ?? pa.code ?? '';
          // amountLiability attribute: value comes from product's main coverage amount, not formData
          const raw =
            code === 'amountLiability'
              ? amountLiability
              : attrsMap[code] ?? attrsMap[pa.code ?? ''] ?? '';
          // Backend expects Value as string; avoid "could not be converted to System.String" when FE sends number (e.g. carYear, seatingCapacity, weight)
          const value =
            raw === null || raw === undefined ? '' : typeof raw === 'string' ? raw : String(raw);
          return {
            code,
            value,
            isRequired: pa.isRequired ?? 'N',
            amountLiability: amountLiability,
            attributeName: pa.attributeName ?? pa.name ?? undefined
          };
        })
      : [];

    return {
      productId,
      // numeric fields default to 0 if missing
      premiumTotal: totals.premiumWithVAT || 0,
      premium: totals.premium || 0,
      vat: totals.vat || 0,
      discount: this.productDiscountByProductId[productId] ?? 0,
      markup: this.productMarkupByProductId[productId] ?? 0,
      discountRate: 0,
      // Amount liability at product level: only MAIN coverage amount (ignore non-main)
      amountLiability: (this.getMainCoverageItem(productId)?.insuranceAmount || 0),
      attributes: attributes.length > 0 ? attributes : undefined,
      coverages,
    };
  }

  private buildCoveragePayload(item: CoverageItem): any {
    // For non-main coverages that are not selected, don't include them (should be filtered out already)
    // But ensure insuranceAmount is only set if coverage is main or selected
    const amountLiability = (item.coverageType === 0 || item.selected) ? (item.insuranceAmount || 0) : 0;

    const coverageLevels = this.buildDeductibleCoverageLevelsPayload(item);

    // Per plan: map coverageId; other numeric fields -> 0; skip coverageLevels
    return {
      coverageId: item.coverageId,
      coverageParentId: item.coverageParentId || undefined,
      insurerCoverageCode: item.insurerCoverageCode || undefined,
      uomId: item.uomId || '00000000-0000-0000-0000-000000000000',
      taxId: item.taxId || '00000000-0000-0000-0000-000000000000',
      tableRateLineId: item.tableRateLineId || undefined,
      amountLiability: amountLiability,
      quantity: item.quantity || 0,
      netRate: undefined,
      baseRate: item.baseRate ?? undefined,
      flatRate: item.flatRate ?? undefined,
      loading: undefined,
      premiumRate: item.premiumRate || 0,
      premiumTotal: item.premiumWithVAT || 0,
      premium: item.premium || 0,
      vat: item.vat || 0,
      discount: 0,
      discountRate: 0,
      // Create/update policy_coverage_level for deductible selection (if any)
      coverageLevels: coverageLevels
    };
  }

  private buildDeductibleCoverageLevelsPayload(item: CoverageItem): any[] {
    const key = item?.deductible ? String(item.deductible) : null;
    const map = item?.deductibleTermByValue || undefined;
    const meta = key && map ? map[key] : null;
    if (!meta) return [];

    return [
      {
        coverageLevelTypeId: meta.coverageLevelTypeId,
        coverageLevelBasisId: meta.coverageLevelBasisId,
        amountType: meta.amountType,
        fromAmount: meta.fromAmount,
        toAmount: meta.toAmount,
        conditionScript: meta.conditionScript ?? undefined,
        computeScript: meta.computeScript ?? undefined
      }
    ];
  }

  protected mergeCoverageLevels(existingLevels: any[], desiredLevels: any[]): any[] {
    const existing = Array.isArray(existingLevels) ? existingLevels.filter(x => !!x) : [];
    const desired = Array.isArray(desiredLevels) ? desiredLevels.filter(x => !!x) : [];
    if (desired.length === 0) return existing;

    const sameTypeBasis = (lvl: any, typeId: string, basisId: string): boolean => {
      const t = String(lvl?.coverageLevelTypeId ?? lvl?.CoverageLevelTypeId ?? '');
      const b = String(lvl?.coverageLevelBasisId ?? lvl?.CoverageLevelBasisId ?? '');
      return t === typeId && b === basisId;
    };

    let result = [...existing];

    for (const d of desired) {
      const typeId = String(d?.coverageLevelTypeId ?? d?.CoverageLevelTypeId ?? '');
      const basisId = String(d?.coverageLevelBasisId ?? d?.CoverageLevelBasisId ?? '');
      if (!typeId || !basisId) {
        result.push(d);
        continue;
      }

      const match = existing.find(l => sameTypeBasis(l, typeId, basisId));
      // Remove existing levels of the same type/basis, then upsert desired
      result = result.filter(l => !sameTypeBasis(l, typeId, basisId));

      const merged: any = { ...d };
      const existingId = match?.id ?? match?.Id;
      if (existingId) {
        merged.id = existingId;
      }
      result.push(merged);
    }

    return result;
  }

  protected buildRiskMotorPayload(): any {
    const carBrandCode = this.formData.carBrandId ? this.carBrandCodeById[this.formData.carBrandId] : undefined;
    const carCategoryCode = this.formData.carCategoryId ? this.carModelCodeById[this.formData.carCategoryId] : undefined;
    const carLineCode = this.formData.carLineId ? this.carLineCodeById[this.formData.carLineId] : undefined;
    const carGroupCode = this.formData.carGroupId ? this.carGroupCodeById[this.formData.carGroupId] : undefined;
    const carTypeCode = this.formData.vehicleTypeId ? this.carTypeCodeById[this.formData.vehicleTypeId] : undefined;

    return {
      riskObjectValue: this.formData.carValue ?? undefined,
      carBrandCode: carBrandCode,
      // Backend/pricing expects carModelCode; we source it from ResCarCategory.code (Hiệu xe)
      carModelCode: carCategoryCode,
      carCategoryCode: carCategoryCode,
      carLineCode: carLineCode,
      carGroupCode: carGroupCode,
      carTypeCode: carTypeCode,
      carUsage: this.formData.carUsage || undefined,
      // Date-only field: keep local date (avoid UTC conversion)
      carProductionYear: this.toLocalIsoDate(this.ensureDate(this.formData.productionYear)) || undefined,
      carPlate: this.formData.vehiclePlate || undefined,
      carPlateType: this.formData.vehiclePlateType || undefined,
      carSeatNumber: this.formData.seatingCapacity ?? undefined,
      carVin: this.formData.chassisNumber || undefined,
      carEngineNumber: this.formData.engineNumber || undefined,
      carPayloadCapacity: this.formData.weight ?? undefined,
      carColor: this.formData.carColor || undefined,
      carOrigin: this.formData.origin || undefined,
      carNew:
        this.formData.isNewCar === null || this.formData.isNewCar === undefined
          ? undefined
          : (this.formData.isNewCar ? 'Y' : 'N'),
    };
  }

  /**
   * Update existing policy
   */
  private update(): void {
    if (!this.selectedPolicy || !this.selectedPolicy.id) {
      return;
    }

    this.loading = true;

    // For snapshot editing, we rely on the formData names directly
    const insurancePeriodFrom = this.ensureDate(this.formData.insurancePeriodFrom);
    const insurancePeriodTo = this.ensureDate(this.formData.insurancePeriodTo);

    const selectedProductIds = this.getSelectedProductIds();
    const totals = this.getSelectedProductsTotals(selectedProductIds);

    // Build products payload from UI
    // ... logic continues ...

    // Build products payload from UI, then attach policy_product/policy_coverage IDs from loaded detail (if present)
    // Use the persisted loaded detail; pendingDetailForEdit is cleared after products load.
    const detail: any = this.currentPolicyDetail;
    const productIdToPolicyProductId: Record<string, string> = {};
    const coverageIdToPolicyCoverageIdByProductId: Record<string, Record<string, string>> = {};
    const coverageIdToCoverageLevelsByProductId: Record<string, Record<string, any[]>> = {};

    if (detail?.products?.length) {
      detail.products.forEach((p: any) => {
        if (p?.productId && p?.id) {
          productIdToPolicyProductId[p.productId] = p.id;
        }
        const covMap: Record<string, string> = {};
        const lvlMap: Record<string, any[]> = {};
        (p?.coverages || []).forEach((c: any) => {
          if (c?.coverageId && c?.id) {
            covMap[c.coverageId] = c.id;
          }
          if (c?.coverageId && Array.isArray(c?.coverageLevels)) {
            lvlMap[c.coverageId] = c.coverageLevels;
          }
        });
        if (p?.productId) {
          coverageIdToPolicyCoverageIdByProductId[p.productId] = covMap;
          coverageIdToCoverageLevelsByProductId[p.productId] = lvlMap;
        }
      });
    }

    const productsPayload = selectedProductIds.map(productId => {
      const payload = this.buildPolicyProductPayload(productId);
      payload.id = productIdToPolicyProductId[productId] || undefined;

      const covIdMap = coverageIdToPolicyCoverageIdByProductId[productId] || {};
      const lvlMap = coverageIdToCoverageLevelsByProductId[productId] || {};
      payload.coverages = (payload.coverages || []).map((c: any) => ({
        ...c,
        id: covIdMap[c.coverageId] || undefined,
        // Preserve existing coverageLevels, but upsert deductible level based on current UI selection.
        coverageLevels: this.mergeCoverageLevels(
          Array.isArray(lvlMap[c.coverageId]) ? lvlMap[c.coverageId] : [],
          Array.isArray(c.coverageLevels) ? c.coverageLevels : []
        ),
      }));

      return payload;
    });

    const riskMotorPayload = {
      ...this.buildRiskMotorPayload(),
      id: detail?.riskObject?.riskObjectMotor?.id || undefined,
    };

    const riskObjectPayload = {
      id: detail?.riskObject?.id || undefined,
      objectTypeId: this.carObjectTypeId,
      // Update RiskObject with vehicle owner data from form (snapshot)
      repName: this.formData.vehicleOwnerName || undefined,
      repIdNo: this.formData.vehicleOwnerIdNo || undefined,
      repPassport: undefined,
      repPhone: this.formData.vehicleOwnerPhone || undefined,
      repEmail: this.formData.vehicleOwnerEmail || undefined,
      repProvinceId: this.formData.vehicleOwnerProvinceId || undefined,
      repWardId: this.formData.vehicleOwnerWardId || undefined,
      repAddress: this.formData.vehicleOwnerAddress || undefined,
      repFullAddress: this.formData.vehicleOwnerFullAddress || undefined,
      riskObjectProvinceId: this.formData.vehicleOwnerProvinceId || undefined,
      riskObjectWardId: this.formData.vehicleOwnerWardId || undefined,
      riskObjectAddress: this.formData.vehicleOwnerAddress || undefined,
      riskObjectFullAddress: this.formData.vehicleOwnerFullAddress || undefined,
      riskObjectLat: detail?.riskObject?.riskObjectLat ?? undefined,
      riskObjectLong: detail?.riskObject?.riskObjectLong ?? undefined,
      // Always send current list so backend can sync Ảnh xe (add new, remove deleted).
      documents: this.getCarPhotoDocumentIds().map(id => ({ documentId: id })),
      riskObjectMotor: riskMotorPayload,
    };

    const isIndividualContract = this.formData.contractType === PolicyContractType.Individual;
    const hasContract = !!(this.formData.contractId?.trim());
    let contractPayload: any = undefined;
    if (isIndividualContract) {
      contractPayload = {
        // ⛔ IMMUTABLE: Code and Type are sent but ignored by server update
        code: this.formData.contractNo,
        type: this.formData.contractType,

        name: this.formData.contractName || undefined,
        insurerId: this.formData.primaryInsurancePartnerId?.trim() || null,
        lobId: this.formData.lobId?.trim() || null,
        customerId: this.formData.customerId?.trim() || null,
        effectDate: this.toLocalIsoDateTime(insurancePeriodFrom) || null,
        expireDate: this.toLocalIsoDateTime(insurancePeriodTo) || null,
        isReciveInvoice: this.formData.isReceiveInvoice ? 'Y' : 'N',
        // Payer details - prefer snapshot
        payerName: this.formData.payerName || undefined,
        payerEmail: this.formData.payerEmail || undefined,
        payerPhone: this.formData.payerPhone || undefined,
        payerProvinceId: this.formData.payerProvinceId || undefined,
        payerWardId: this.formData.payerWardId || undefined,
        payerAddress: this.formData.payerAddress || undefined,
        payerFullAddress: this.formData.payerFullAddress || undefined,
        documents: this.uploadedContractDocumentIds.map(id => ({ documentId: id })),
      };
    } else if (hasContract || this.uploadedContractDocumentIds.length > 0) {
      // When policy has a contract (or we have contract docs), always send documents so backend can sync (add/remove).
      // Sending empty array allows backend to remove all contract documents when user removed them all.
      contractPayload = {
        documents: this.uploadedContractDocumentIds.map(id => ({ documentId: id }))
      };
    }

    // NOTE: Update API allows updating contract and changing to another contract (contractId).
    const updateDto: any = {
      contractId: this.formData.contractId?.trim() || undefined,
      contract: contractPayload,
      lobId: this.formData.lobId?.trim() || null,
      sellType: this.formData.sellType,
      insurerPolicyNo: this.formData.insurerPolicyNo?.trim() || undefined,
      certificateNo: this.formData.certificateNo?.trim() || undefined,
      policyTypeId: this.formData.policyTypeId?.trim() || undefined,
      partnerId: this.formData.primaryInsurancePartnerId?.trim() || undefined,
      sellerId: this.formData.sellerId?.trim() || null,
      implementerId: this.formData.implementerId?.trim() || null,
      currencyId: this.formData.currencyId?.trim() || null,
      exchangeRate: this.formData.exchangeRate,
      status: this.formData.status,
      issueDate: this.formData.issueDate?.trim() || undefined,
      approvalStatus: this.formData.approvalStatus?.trim() || undefined,
      orgEffectDate: this.toLocalIsoDateTime(insurancePeriodFrom) || null,
      orgExpireDate: this.toLocalIsoDateTime(insurancePeriodTo) || null,
      isRenewal: this.formData.isRenewal ? 'Y' : 'N',
      isGift: this.formData.isGift || 'N',
      premiumTotal: totals.premiumTotal,
      premium: totals.premium,
      vat: totals.vat,
      discount: 0,
      discountRate: 0,
      markup: 0,
      isBankLoan: this.formData.isBankLoan ? 'Y' : 'N',
      channelId: this.formData.channelId?.trim() || undefined,

      insuredName: this.formData.insuredName?.trim() || undefined,
      insuredIdNo: this.formData.insuredIdNo?.trim() || undefined,
      insuredPhone: this.formData.insuredPhone?.trim() || undefined,
      insuredEmail: this.formData.insuredEmail?.trim() || undefined,
      insuredProvinceId: this.formData.insuredProvinceId?.trim() || undefined,
      insuredWardId: this.formData.insuredWardId?.trim() || undefined,
      insuredAddress: this.formData.insuredAddress?.trim() || undefined,
      insuredFullAddress: this.formData.insuredFullAddress?.trim() || undefined,

      beneficiaryName: this.formData.beneficiaryName?.trim() || undefined,
      beneficiaryIdNo: this.formData.beneficiaryIdNo?.trim() || undefined,
      beneficiaryPhone: this.formData.beneficiaryPhone?.trim() || undefined,
      beneficiaryEmail: this.formData.beneficiaryEmail?.trim() || undefined,
      beneficiaryProvinceId: this.formData.beneficiaryProvinceId?.trim() || undefined,
      beneficiaryWardId: this.formData.beneficiaryWardId?.trim() || undefined,
      beneficiaryAddress: this.formData.beneficiaryAddress?.trim() || undefined,
      beneficiaryFullAddress: this.formData.beneficiaryFullAddress?.trim() || undefined,

      version: {
        effectDate: this.toLocalIsoDateTime(insurancePeriodFrom) || undefined,
        expireDate: this.toLocalIsoDateTime(insurancePeriodTo) || undefined,
        orgEffectDate: this.toLocalIsoDateTime(insurancePeriodFrom) || undefined,
        orgExpireDate: this.toLocalIsoDateTime(insurancePeriodTo) || undefined,
        internalNote: this.formData.internalNotes || undefined,
        customerNote: this.formData.customerNotes || undefined,
        premiumTotal: totals.premiumTotal,
        premium: totals.premium,
        vat: totals.vat,
        discount: 0,
        discountRate: 0,
        markup: 0,
      },

      products: productsPayload,
      riskObject: riskObjectPayload,
      // Always send current list so backend can add new and remove deleted (sync policy_document).
      documents: (this.uploadedDocumentIds || []).map(id => ({ documentId: id })),
      amount: {
        premiumTotal: totals.premiumTotal,
        premium: totals.premium,
        vat: totals.vat,
        discount: 0,
        discountRate: 0,
        markup: 0,
      },
    };

    const updatePolicyId = this.selectedPolicy!.id as string;
    this.policyService.update(updatePolicyId, updateDto as UpdatePolicyDetailDto).subscribe({
      next: () => {
        this.loading = false;
        if (this.willSubmitForApprovalAfterSave) {
          this.willSubmitForApprovalAfterSave = false;
          this.messageService.add({
            severity: 'success',
            summary: this.localizationService.localize('Policy::Success'),
            detail: this.localizationService.localize('Policy::Policy:UpdatedSuccessfully')
          });
          this.callSubmitForApproval(updatePolicyId, this.pendingApproverId || '');
          this.pendingApproverId = '';
          return;
        }
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Policy::Success'),
          detail: this.localizationService.localize('Policy::Policy:UpdatedSuccessfully')
        });
        this.afterSaveSuccess();
      },
      error: (error) => {
        const errorMessage = this.getFriendlyErrorMessage(error);
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Policy::Error'),
          detail: errorMessage
        });
        this.loading = false;
      }
    });
  }





  /**
   * Delete policy with confirmation
   */
  delete(policy: PolicyDto): void {
    if (!policy.id) {
      return;
    }

    this.confirmationService.confirm({
      message: this.localizationService.localize('Policy::Policy:DeleteConfirm'),
      header: this.localizationService.localize('Policy::Policy:Delete'),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.loading = true;
        this.policyService.delete(policy.id!).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: this.localizationService.localize('Policy::Success'),
              detail: this.localizationService.localize('Policy::Policy:DeletedSuccessfully')
            });
            this.search();
          },
          error: (error) => {
            const errorMessage = error.error?.error?.message ||
              error.error?.error?.details ||
              this.localizationService.localize('AbpUi::InternalServerErrorMessage');
            this.messageService.add({
              severity: 'error',
              summary: this.localizationService.localize('Policy::Error'),
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
  private getEmptyForm(): PolicyFormData {
    return {
      contractId: null,
      primaryInsurancePartnerId: null,
      lobId: this.defaultLobId || '',
      contractType: PolicyContractType.Individual,
      customerId: null,
      payerId: null,
      payerName: null,
      payerPhone: null,
      payerEmail: null,
      payerAddress: null,
      payerFullAddress: null,
      payerProvinceId: null,
      payerWardId: null,
      invoiceRecipient: null,
      contractNo: null,
      contractName: null,
      rootPolicyNo: null,
      policyNo: '',
      lastVersionId: '0',
      versionNo: 0,
      sellType: PolicySellType.Agency,
      insurerPolicyNo: null,
      policyTypeId: this.defaultPolicyTypeId || '',
      partnerId: '',
      sellerId: '',
      implementerId: '',
      currencyId: '',
      exchangeRate: 1,
      status: PolicyStatus.Draft,
      issueDate: null,
      approvalStatus: null,
      terminationStatus: null,
      paymentStatus: null,
      orgEffectDate: '',
      orgExpireDate: '',
      isRenewal: false,
      isGift: 'N',
      premiumTotal: 0,
      premium: 0,
      vat: 0,
      discount: null,
      discountRate: null,
      isBankLoan: false,
      channelId: null,
      certificateNo: null,
      beneficiaryId: null,
      vehicleOwnerId: null,
      vehicleOwnerName: null,
      vehicleOwnerPhone: null,
      vehicleOwnerEmail: null,
      vehicleOwnerAddress: null,
      vehicleOwnerFullAddress: null,
      vehicleOwnerProvinceId: null,
      vehicleOwnerWardId: null,
      vehicleOwnerIdNo: null,
      carUsage: '',
      carBrandId: null,
      carModelId: null,
      carCategoryId: null,
      seatingCapacity: null,
      weight: null,
      isNewCar: null,
      carValue: null,
      carColor: null,
      vehiclePlate: null,
      vehiclePlateType: null,
      chassisNumber: null,
      engineNumber: null,
      vehicleTypeId: null,
      productionYear: null,
      origin: null,
      carLineId: null,
      carGroupId: null,

      isReceiveInvoice: false,
      insuredName: null,
      insuredIdNo: null,
      insuredPhone: null,
      insuredEmail: null,
      insuredProvinceId: null,
      insuredWardId: null,
      insuredAddress: null,
      insuredFullAddress: null,
      beneficiaryName: null,
      beneficiaryIdNo: null,
      beneficiaryPhone: null,
      beneficiaryEmail: null,
      beneficiaryProvinceId: null,
      beneficiaryWardId: null,
      beneficiaryAddress: null,
      beneficiaryFullAddress: null,
      insurancePeriodFrom: this.getDefaultInsurancePeriodFrom(),
      insurancePeriodTo: this.getDefaultInsurancePeriodTo(),
      isVehicleMaterialInsurance: false,
      isMandatoryCivilLiability: false,
      isVoluntaryCivilLiability: false,
      packageType: null,
      customerNotes: null,
      internalNotes: null
    };
  }

  /**
   * Get default insurance period from (today)
   */
  private getDefaultInsurancePeriodFrom(): Date {
    return new Date();
  }

  /**
   * Get default insurance period to (today + 12 months)
   */
  private getDefaultInsurancePeriodTo(): Date {
    const today = new Date();
    const nextYear = new Date(today);
    nextYear.setMonth(today.getMonth() + 12);
    return nextYear;
  }

  /**
   * Handle insurance period from date change - auto calculate expireDate (effectDate + 1 year)
   */
  onCarGroupChange(event: any): void {
    // If event is object (selection), extract value. If string, use directly.
    const carGroupId = (event && typeof event === 'object') ? event.value : event;

    if (carGroupId) {
      this.handleCarGroupChange(carGroupId);
    }
  }

  private handleCarGroupChange(carGroupId: string): void {
    // Find car line ID from our map or items
    let lineId = this.carLineIdByCarGroupId[carGroupId];

    if (!lineId && this.carGroupItems) {
      const group = this.carGroupItems.find(g => g.id === carGroupId);
      lineId = group?.carLineId || group?.CarLineId;
    }

    if (lineId) {
      this.formData.carLineId = lineId;
    }

    this.onInsuredObjectChange();
  }

  onCarLineChange(lineId: string | null): void {
    // Khi chọn dòng xe: chỉ cho chọn những nhóm xe có dòng xe này. Clear carGroupId nếu không thuộc dòng đã chọn.
    if (lineId && this.formData.carGroupId) {
      const allowedGroupIds = this.carGroupIdsByCarLineId[lineId] || [];
      if (!allowedGroupIds.includes(this.formData.carGroupId)) {
        this.formData.carGroupId = null;
      }
    }
    this.onInsuredObjectChange();
  }

  /** Nhóm xe đã lọc theo dòng xe đang chọn (nếu có). Khi chọn dòng xe mà không có nhóm nào thì trả về rỗng. */
  get carGroupOptionsFiltered(): Array<{ label: string; value: string }> {
    const lineId = this.formData?.carLineId?.trim();
    if (!lineId) return this.carGroupOptions;
    const allowedIds = this.carGroupIdsByCarLineId[lineId] || [];
    return this.carGroupOptions.filter(opt => allowedIds.includes(opt.value));
  }

  onInsurancePeriodFromChange(date: Date | null): void {
    if (this.isUpdatingInsurancePeriod) {
      return; // Prevent infinite loop
    }

    if (date && date instanceof Date) {
      // Auto calculate expireDate: effectDate + 1 year
      this.isUpdatingInsurancePeriod = true;
      const expireDate = new Date(date);
      expireDate.setFullYear(expireDate.getFullYear() + 1);
      this.formData.insurancePeriodTo = expireDate;
      this.isUpdatingInsurancePeriod = false;

      // Recalculate fee when effectiveDate changes (and expireDate auto-updated)
      this.onInsuredObjectChange();
    }
  }

  /**
   * Handle insurance period to date change.
   * Keep only display formatting; do not auto back-calculate insurancePeriodFrom.
   */
  onInsurancePeriodToChange(date: Date | null): void {
    if (this.isUpdatingInsurancePeriod) {
      return; // Prevent infinite loop
    }

    if (date && date instanceof Date) {
      // Recalculate fee when expireDate changes
      this.onInsuredObjectChange();
    }
  }

  /**
   * Format date/time for display as dd/MM/yyyy HH:mm:ss
   */
  private formatDateTimeDisplay(date: Date): string {
    if (!date) {
      return '';
    }
    const day = String(date.getDate()).padStart(2, '0');
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const year = date.getFullYear();
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');
    const seconds = String(date.getSeconds()).padStart(2, '0');
    return `${day}/${month}/${year} ${hours}:${minutes}:${seconds}`;
  }

  /**
   * Attach document handler
   */
  attachDocument(): void {
    // TODO: Implement document attachment functionality
    this.messageService.add({
      severity: 'info',
      summary: this.localizationService.localize('Policy::Info'),
      detail: this.localizationService.localize('Policy::Policy:AttachDocumentFeature')
    });
  }

  onUploadMultipleDocuments(event: any): void {
    // Robustly extract files regardless of PrimeNG event structure
    let files: File[] = [];
    if (event?.files) {
      files = event.files;
    } else if (Array.isArray(event)) {
      files = event;
    } else if (event?.currentFiles) {
      files = event.currentFiles;
    }

    if (!files || files.length === 0) {
      return;
    }

    // PrimeNG sometimes keeps previous file selections; prevent re-uploading the same file(s) again.
    const localKeys = new Set<string>();
    const toUpload = files.filter(f => {
      const key = `${f.name}|${f.size}|${f.lastModified}`;
      if (this.uploadedPolicyFileKeys.has(key)) return false;
      if (localKeys.has(key)) return false;
      localKeys.add(key);
      return true;
    });

    if (toUpload.length === 0) {
      // Clear file queue so next selection is clean
      try { event?.options?.clear?.(); } catch { }
      try { event?.clear?.(); } catch { }
      return;
    }

    if (!this.defaultUploadDocumentTypeId) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Policy::Error'),
        detail: 'No active Document Type found. Please configure Document Types first.'
      });
      return;
    }

    const groupCode = (this.formData.policyNo && this.formData.policyNo.trim() !== '')
      ? this.formData.policyNo.trim()
      : 'POLICY_UPLOAD';

    this.documentUploadLoading = true;
    this.resDocumentService.uploadMultipleFiles(this.defaultUploadDocumentTypeId, groupCode, toUpload).subscribe({
      next: (docs) => {
        const newDocs = docs || [];
        const mappedDocs = newDocs.map(d => {
          // Robust ID check
          const id = (d as any).id || (d as any).resDocumentId || '';
          return { ...d, id };
        }).filter(d => !!d.id);

        const merged = [...(this.uploadedDocuments || []), ...mappedDocs];
        const seen = new Set<string>();
        this.uploadedDocuments = merged.filter(d => {
          if (!d.id || seen.has(d.id)) return false;
          seen.add(d.id);
          return true;
        });

        this.uploadedDocumentIds = this.uploadedDocuments.map(d => d.id!).filter(Boolean);

        // Ensure we have previewable URLs for policy attachments
        this.refreshPolicyDocumentLinks();

        // Mark uploaded file keys only after success
        toUpload.forEach(f => this.uploadedPolicyFileKeys.add(`${f.name}|${f.size}|${f.lastModified}`));

        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Policy::Success'),
          detail: `Uploaded ${newDocs.length} file(s). Total attached: ${this.uploadedDocumentIds.length}.`
        });

        // Important: clear PrimeNG file queue so the next upload doesn't re-send previous files
        try { event?.options?.clear?.(); } catch { }
        try { event?.clear?.(); } catch { }

        this.documentUploadLoading = false;
      },
      error: (error) => {
        const errorMessage = error?.error?.error?.message ||
          error?.error?.error?.details ||
          this.localizationService.localize('AbpUi::InternalServerErrorMessage');
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Policy::Error'),
          detail: errorMessage
        });

        // Clear file queue on error as well
        try { event?.options?.clear?.(); } catch { }
        try { event?.clear?.(); } catch { }

        this.documentUploadLoading = false;
      }
    });
  }

  private refreshPolicyDocumentLinks(): void {
    const ids = (this.uploadedDocumentIds || []).filter(Boolean);
    if (ids.length === 0) {
      this.uploadedDocuments = [];
      return;
    }

    this.resDocumentService.getMultipleFiles(ids).subscribe({
      next: (files) => {
        const list = (files || [])
          .map((f: FileResponseDto) => ({
            id: f.id,
            fileName: f.fileName,
            url: f.url,
            mimeType: f.mimeType,
            fileSize: f.fileSize
          }) as any as ResDocumentDto)
          .filter(d => !!d.id);

        const byId = new Map(list.map(d => [String(d.id), d]));
        this.uploadedDocuments = ids.map(id => byId.get(String(id))!).filter(Boolean);
      },
      error: () => {
        // Best-effort only; keep existing list if any.
      }
    });
  }

  onUploadContractDocuments(event: any): void {
    // Robustly extract files regardless of PrimeNG event structure
    let files: File[] = [];
    if (event?.files) {
      files = event.files;
    } else if (Array.isArray(event)) {
      files = event;
    } else if (event?.currentFiles) {
      files = event.currentFiles;
    }

    if (!files || files.length === 0) {
      return;
    }

    // PrimeNG sometimes keeps previous file selections; prevent re-uploading the same file(s) again.
    const localKeys = new Set<string>();
    const toUpload = files.filter(f => {
      const key = `${f.name}|${f.size}|${f.lastModified}`;
      if (this.uploadedContractFileKeys.has(key)) return false;
      if (localKeys.has(key)) return false;
      localKeys.add(key);
      return true;
    });

    if (toUpload.length === 0) {
      try { event?.options?.clear?.(); } catch { }
      try { event?.clear?.(); } catch { }
      return;
    }

    const typeId = this.contractDocumentTypeId || this.defaultUploadDocumentTypeId;
    if (!typeId) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Policy::Error'),
        detail: 'No active Document Type found. Please configure Document Types first.'
      });
      return;
    }

    const groupCode = (this.formData.contractNo && this.formData.contractNo.trim() !== '')
      ? this.formData.contractNo.trim()
      : 'CONTRACT_UPLOAD';

    this.documentUploadLoading = true;
    this.resDocumentService.uploadMultipleFiles(typeId, groupCode, toUpload).subscribe({
      next: (docs) => {
        const newDocs = docs || [];
        const mappedDocs = newDocs.map(d => {
          // Robust ID check: handle 'id' or potential 'resDocumentId'
          const id = (d as any).id || (d as any).resDocumentId || '';
          return { ...d, id };
        }).filter(d => !!d.id);

        const merged = [...(this.uploadedContractDocuments || []), ...mappedDocs];
        const seen = new Set<string>();
        this.uploadedContractDocuments = merged.filter(d => {
          if (!d.id || seen.has(d.id)) return false;
          seen.add(d.id);
          return true;
        });

        this.uploadedContractDocumentIds = this.uploadedContractDocuments.map(d => d.id!).filter(Boolean);

        // Ensure we have previewable URLs (some APIs may return documents without url/thumbnailUrl on upload)
        this.refreshContractDocumentLinks();

        // Mark uploaded file keys only after success
        toUpload.forEach(f => this.uploadedContractFileKeys.add(`${f.name}|${f.size}|${f.lastModified}`));

        const msg = this.localizationService.localize('Policy::Policy:UploadContractDocumentsSuccess', 'Uploaded {0} contract document(s). Total attached: {1}.');
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Policy::Success'),
          detail: msg.replace('{0}', String(newDocs.length)).replace('{1}', String(this.uploadedContractDocumentIds.length))
        });

        // Important: clear PrimeNG file queue so the next upload doesn't re-send previous files
        try { event?.options?.clear?.(); } catch { }
        try { event?.clear?.(); } catch { }

        this.documentUploadLoading = false;
      },
      error: (error) => {
        const errorMessage = error?.error?.error?.message ||
          error?.error?.error?.details ||
          this.localizationService.localize('AbpUi::InternalServerErrorMessage');
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Policy::Error'),
          detail: errorMessage
        });

        // Clear file queue on error as well
        try { event?.options?.clear?.(); } catch { }
        try { event?.clear?.(); } catch { }

        this.documentUploadLoading = false;
      }
    });
  }

  openCarPhotos(): void {
    this.carPhotosVisible = true;
  }

  openOcrUploadModal(): void {
    this.ocrUploadVisible = true;
    this.ocrUploadedDoc = null;
  }

  private carPhotoGroupCode(): string {
    const policyNo = (this.formData?.policyNo || '').trim();
    return policyNo ? `${policyNo}_CAR_PHOTOS` : 'CAR_PHOTOS_UPLOAD';
  }

  private ocrGroupCode(): string {
    const policyNo = (this.formData?.policyNo || '').trim();
    return policyNo ? `${policyNo}_OCR` : 'OCR_UPLOAD';
  }

  protected getCarPhotoDocumentIds(): string[] {
    const ids = [
      ...this.carPhotos.front.map(d => d.id),
      ...this.carPhotos.back.map(d => d.id),
      ...this.carPhotos.left.map(d => d.id),
      ...this.carPhotos.right.map(d => d.id),
      ...this.carPhotos.video.map(d => d.id)
    ].filter((x): x is string => !!x && typeof x === 'string' && x.trim().length > 0);

    return Array.from(new Set(ids));
  }

  /** Called from native file input (change) for OCR modal tile layout */
  onOcrFileSelected(event: Event, type: 'registration' | 'inspection'): void {
    const input = event.target as HTMLInputElement;
    const file = input?.files?.[0];
    if (!file) return;
    this.onUploadOcrImage({ files: [file] }, type);
    input.value = '';
  }

  onUploadOcrImage(event: any, type: 'registration' | 'inspection'): void {
    let files: File[] = [];
    if (event?.files) {
      files = event.files;
    } else if (Array.isArray(event)) {
      files = event;
    } else if (event?.currentFiles) {
      files = event.currentFiles;
    }
    const file = files && files.length > 0 ? files[0] : null;
    if (!file) {
      return;
    }

    this.ocrUploadLoading = true;
    this.policyService.uploadOcrImage(file, type).subscribe({
      next: (result) => {
        if (this.formData) {
          if (result.vehiclePlate != null) this.formData.vehiclePlate = result.vehiclePlate;
          if (result.chassisNumber != null) this.formData.chassisNumber = result.chassisNumber;
          if (result.engineNumber != null) this.formData.engineNumber = result.engineNumber;
        }
        this.ocrUploadLoading = false;
        this.ocrUploadVisible = false;
        try { event?.options?.clear?.(); } catch { }
        try { event?.clear?.(); } catch { }
      },
      error: (error) => {
        const errorMessage = error?.error?.error?.message ||
          error?.error?.error?.details ||
          this.localizationService.localize('AbpUi::InternalServerErrorMessage');
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Policy::Error'),
          detail: errorMessage
        });
        this.ocrUploadLoading = false;
        try { event?.options?.clear?.(); } catch { }
        try { event?.clear?.(); } catch { }
      }
    });
  }

  onUploadCarPhoto(slot: 'front' | 'back' | 'left' | 'right' | 'video', event: any): void {
    // Robustly extract files regardless of PrimeNG event structure
    let files: File[] = [];
    if (event?.files) {
      files = event.files;
    } else if (Array.isArray(event)) {
      files = event;
    } else if (event?.currentFiles) {
      files = event.currentFiles;
    }

    if (!files || files.length === 0) {
      return;
    }

    const typeId = this.defaultUploadDocumentTypeId;
    if (!typeId) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Policy::Error'),
        detail: 'No active Document Type found. Please configure Document Types first.'
      });
      return;
    }

    this.carPhotosUploading[slot] = true;
    let remaining = files.length;
    files.forEach((file) => {
      this.resDocumentService.uploadSingleFile(typeId, this.carPhotoGroupCode(), file).subscribe({
        next: (doc) => {
          const nextId = (doc as any)?.id || (doc as any)?.resDocumentId || '';
          const nextDoc = ({ ...(doc as any), id: nextId } as any) as ResDocumentDto;
          this.carPhotos[slot] = [...this.carPhotos[slot], nextDoc];

          remaining--;
          if (remaining <= 0) {
            // NOTE: We don't attach immediately on upload.
            // These documents will be attached to the policy risk object on Save (policy_risk_object_document).

            // Clear queue so next selection is clean
            try { event?.options?.clear?.(); } catch { }
            try { event?.clear?.(); } catch { }

            this.carPhotosUploading[slot] = false;
          }
        },
        error: (error) => {
          const errorMessage = error?.error?.error?.message ||
            error?.error?.error?.details ||
            this.localizationService.localize('AbpUi::InternalServerErrorMessage');
          this.messageService.add({
            severity: 'error',
            summary: this.localizationService.localize('Policy::Error'),
            detail: errorMessage
          });

          remaining--;
          if (remaining <= 0) {
            try { event?.options?.clear?.(); } catch { }
            try { event?.clear?.(); } catch { }

            this.carPhotosUploading[slot] = false;
          }
        }
      });
    });
  }

  /**
   * Remove a contract document from the list (contract section upload).
   * Call only when the section is editable (template controls button visibility).
   */
  removeContractDocument(docId: string | undefined): void {
    if (!docId) return;
    this.uploadedContractDocumentIds = (this.uploadedContractDocumentIds || []).filter(id => id !== docId);
    this.uploadedContractDocuments = (this.uploadedContractDocuments || []).filter(d => d.id !== docId);
  }

  /**
   * Remove a policy document from the list (policy section, next to save button).
   * Call only when the section is editable (template controls button visibility).
   */
  removePolicyDocument(docId: string | undefined): void {
    if (!docId) return;
    this.uploadedDocumentIds = (this.uploadedDocumentIds || []).filter(id => id !== docId);
    this.uploadedDocuments = (this.uploadedDocuments || []).filter(d => d.id !== docId);
  }

  /**
   * Remove a car photo from the given slot (Ảnh xe: front, back, left, right, or video).
   * Call only when the section is editable (template controls button visibility).
   */
  removeCarPhoto(slot: 'front' | 'back' | 'left' | 'right' | 'video', docId: string | undefined): void {
    if (!docId) return;
    this.carPhotos[slot] = (this.carPhotos[slot] || []).filter(d => d.id !== docId);
  }

  /**
   * Preview car photo/video ngay trong màn hình (không mở tab mới).
   * Nếu doc chưa có url, sẽ gọi getMultipleFiles để lấy url trước rồi hiển thị trong dialog preview.
   */
  previewCarPhoto(doc: { id?: string; url?: string; fileName?: string; mimeType?: string }): void {
    if (!doc?.id) return;

    const openPreview = (fullDoc: { id?: string; url?: string; fileName?: string; mimeType?: string }) => {
      if (!fullDoc?.url) {
        this.messageService.add({
          severity: 'warn',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail:
            this.localizationService.localize('Policy::Policy:PreviewNotAvailable') ||
            'Không thể xem trước file.'
        });
        return;
      }
      this.carPhotoPreviewDoc = fullDoc as ResDocumentDto;
      this.carPhotoPreviewVisible = true;
    };

    const existingUrl = doc.url?.trim();
    if (existingUrl) {
      openPreview(doc);
      return;
    }

    const id = doc.id.trim();
    this.resDocumentService.getMultipleFiles([id]).subscribe({
      next: (files) => {
        const first = (files || [])[0] as { id?: string; url?: string; fileName?: string; mimeType?: string } | undefined;
        if (first?.url) {
          openPreview({
            ...doc,
            ...first
          });
        } else {
          this.messageService.add({
            severity: 'warn',
            summary: this.localizationService.localize('AbpUi::Error'),
            detail:
              this.localizationService.localize('Policy::Policy:PreviewNotAvailable') ||
              'Không thể xem trước file.'
          });
        }
      },
      error: () => {
        this.messageService.add({
          severity: 'warn',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail:
            this.localizationService.localize('Policy::Policy:PreviewNotAvailable') ||
            'Không thể xem trước file.'
        });
      }
    });
  }

  private refreshContractDocumentLinks(): void {
    const ids = (this.uploadedContractDocumentIds || []).filter(Boolean);
    if (ids.length === 0) {
      this.uploadedContractDocuments = [];
      return;
    }

    // Prefer using get-multiple to obtain stable preview URLs.
    this.resDocumentService.getMultipleFiles(ids).subscribe({
      next: (files) => {
        const list = (files || [])
          .map((f: FileResponseDto) => ({
            id: f.id,
            fileName: f.fileName,
            url: f.url,
            mimeType: f.mimeType,
            fileSize: f.fileSize
          }) as any as ResDocumentDto)
          .filter(d => !!d.id);

        // Keep order consistent with ids
        const byId = new Map(list.map(d => [String(d.id), d]));
        this.uploadedContractDocuments = ids.map(id => byId.get(String(id))!).filter(Boolean);
      },
      error: () => {
        // If this fails, keep whatever we already have from upload/detail.
      }
    });
  }

  /**
   * Import đơn: mở modal import (giống contract), chọn hợp đồng trong modal nếu mở từ màn tìm kiếm.
   */
  importFile(): void {
    this.importPolicyModal?.open();
  }

  /**
   * Sau khi import đơn xong (từ modal), refresh danh sách.
   */
  onImportPolicyDone(): void {
    this.search();
  }

  /**
   * Update payment handler - opens Excel import modal for bulk payment update
   */
  updatePayment(): void {
    this.paymentUpdateModal?.open(undefined, 'excel');
  }

  private escapeForLike(pattern: string | null | undefined): string {
    if (!pattern) return '';
    // Same escaping logic as backend EscapeForLike: escape \, %, _ for SQL ILIKE ... ESCAPE '\'
    return pattern
      .replace(/\\/g, '\\\\')
      .replace(/%/g, '\\%')
      .replace(/_/g, '\\_');
  }

  /**
   * Export file handler
   */
  exportFile(): void {
    if (this.loadingExport) return;
    this.loadingExport = true;

    const statusStr =
      this.searchForm.status != null
        ? String(PolicyStatus[this.searchForm.status as any] ?? '').trim().toLowerCase()
        : '';

    const effectiveDateFrom = this.toLocalIsoDate(this.ensureDate(this.searchForm.effectiveDateFrom as any)) ?? '';
    const effectiveDateTo = this.toLocalIsoDate(this.ensureDate(this.searchForm.effectiveDateTo as any)) ?? '';
    const expiryDateFrom = this.toLocalIsoDate(this.ensureDate(this.searchForm.expiryDateFrom as any)) ?? '';
    const expiryDateTo = this.toLocalIsoDate(this.ensureDate(this.searchForm.expiryDateTo as any)) ?? '';

    const carPlate = this.searchForm.carPlate?.trim() ?? '';

    // Important: parameter codes must match the template definition.
    // We send empty string for optional params so backend will treat as NULL.
    // Phạm vi xem đơn (ViewerEmployeeId, ViewerPartnerId, ViewerDepartmentId, ViewerIsManager, ManagedDeptIds)
    // do backend gắn theo user đăng nhập khi mã mẫu là POLICY_SEARCH — không gửi từ đây.
    const params: Record<string, string> = {
      PolicyNo: this.searchForm.policyNo?.trim() ?? '',
      ContractId: this.searchForm.contractId ?? '',
      // Align with list search: DEFAULT_LOB / DEFAULT_LOB_MOTORBIKE + route (car vs motorbike)
      LobId: this.implicitSearchLobId?.trim() ?? '',
      PolicyTypeId: this.searchForm.policyTypeId ?? '',
      PartnerId: this.searchForm.partnerId ?? '',
      SellerId: '', // not present in current search UI
      CurrencyId: '', // not present in current search UI
      SellType: '', // not present in current search UI
      StatusStr: statusStr,
      ApprovalStatus: '', // not present in current search UI
      ChannelId: this.searchForm.channelId ?? '',
      CustomerId: this.searchForm.customerId ?? '',
      ContractType: this.searchForm.contractType != null ? String(this.searchForm.contractType) : '',
      ContractStatus: this.searchForm.contractStatus != null ? String(this.searchForm.contractStatus) : '',
      CertificateNo: this.searchForm.certificateNo?.trim() ?? '',
      ImplementerId: this.searchForm.implementerId ?? '',
      EffectiveDateFrom: effectiveDateFrom,
      EffectiveDateTo: effectiveDateTo,
      ExpiryDateFrom: expiryDateFrom,
      ExpiryDateTo: expiryDateTo,
      CarPlate: carPlate,
      CarPlateEscaped: this.escapeForLike(carPlate),
      CarVin: this.searchForm.carVin?.trim() ?? '',
      CarEngineNumber: this.searchForm.carEngineNumber?.trim() ?? '',
      PrimaryInsurancePartnerId: this.searchForm.primaryInsurancePartnerId ?? '',
      ImportLotNumber: this.searchForm.importLotNumber?.trim() ?? '',
      PaymentStatus: this.searchForm.paymentStatus?.trim() ?? ''
    };

    this.reportTemplateService
      .genDynamicFileByCode(POLICY_SEARCH_REPORT_CODE, params as unknown as Record<string, object>)
      .subscribe({
        next: (data: Blob) => {
          const a = document.createElement('a');
          const objectUrl = URL.createObjectURL(data);
          a.href = objectUrl;
          const dateStr = new Date().toISOString().slice(0, 10);
          a.download = `policy_search_${dateStr}.xlsx`;
          a.click();
          URL.revokeObjectURL(objectUrl);
          this.loadingExport = false;
          this.messageService.add({
            severity: 'success',
            summary: this.localizationService.localize('Policy::Success'),
            detail: this.localizationService.localize('Policy::PolicyRequestApproval:ExportSuccess')
          });
        },
        error: () => {
          this.loadingExport = false;
          this.messageService.add({
            severity: 'error',
            summary: this.localizationService.localize('Policy::Error'),
            detail: this.localizationService.localize('Policy::PolicyRequestApproval:ExportFailed')
          });
        }
      });
  }


  /**
   * Open customer creation dialog
   * @param targetField - Which field should be set after customer creation (customer, payer, beneficiary, or vehicleOwner)
   */
  openCustomerDialog(targetField: 'customer' | 'payer' | 'beneficiary' | 'vehicleOwner' = 'customer', customerId?: string | null): void {
    this.customerDialogTargetField = targetField;
    this.selectedCustomerId = customerId ?? undefined;

    if (customerId && (targetField === 'customer' || targetField === 'payer')) {
      this.customerDialogMode = 'edit';
      this.customerDialogLoading = true;
      this.customerDialogVisible = true;
      this.customerService.get(customerId).subscribe({
        next: (customer) => {
          this.customerFormData = {
            code: customer.code || '',
            name: customer.name || '',
            phone: customer.phone || '',
            email: customer.email || null,
            organizationTypeId: customer.organizationTypeId || null,
            provinceId: customer.provinceId || null,
            wardId: customer.wardId || null,
            address: customer.address || '',
            fullAddress: customer.fullAddress || '',
            sex: customer.sex || null,
            dob: customer.dob ? new Date(customer.dob) : null,
            idNo: customer.idNo || null,
            status: customer.status || ResCustomerStatus.Active,
            industryId: customer.industryId || null,
            businessNo: customer.businessNo || '',
            tin: customer.tin || '',
            repName: customer.repName || '',
            repPhone: customer.repPhone || '',
            repEmail: customer.repEmail || '',
            repIdNo: customer.repIdNo || '',
            repTitle: customer.repTitle || '',
            authorizerName: customer.authorizer || '',
            authorizerPhone: customer.authorizerPhone || '',
            authorizerEmail: customer.authorizerEmail || '',
            authorizerIdNo: customer.authorizerNo || '',
            authorizerTitle: customer.authorizerTitle || '',
            authorizerDate: customer.authorizerDate ? new Date(customer.authorizerDate) : null
          };
          if (this.customerFormData.provinceId) {
            this.loadCustomerWards(this.customerFormData.provinceId);
          }
          this.customerDialogLoading = false;
        },
        error: (error) => {
          this.customerDialogLoading = false;
          this.messageService.add({
            severity: 'error',
            summary: this.localizationService.localize('AbpUi::Error'),
            detail: 'Failed to load customer details'
          });
        }
      });
      return;
    }

    this.customerFormData = {
      code: '',
      name: '',
      phone: '',
      email: null,
      organizationTypeId: null,
      provinceId: null,
      wardId: null,
      address: '',
      fullAddress: '',
      sex: null,
      dob: null,
      idNo: null,
      status: ResCustomerStatus.Active,
      industryId: null,
      businessNo: '',
      tin: '',
      repName: '',
      repPhone: '',
      repEmail: '',
      repIdNo: '',
      repTitle: '',
      authorizerName: '',
      authorizerPhone: '',
      authorizerEmail: '',
      authorizerIdNo: '',
      authorizerTitle: '',
      authorizerDate: null
    };

    // Chỉnh snapshot trên đơn (payer/beneficiary/chủ xe) khi sửa đơn, hoặc khi tái tục (create + isRenewal) — không mở form tạo KH mới rỗng.
    const useSnapshotCustomerDialog =
      targetField !== 'customer' &&
      (this.dialogMode === 'edit' || (this.dialogMode === 'create' && this.formData.isRenewal));

    if (useSnapshotCustomerDialog) {
      this.customerDialogMode = 'snapshot-edit';
      if (targetField === 'payer') {
        this.customerFormData.name = this.formData.payerName || '';
        this.customerFormData.phone = this.formData.payerPhone || '';
        this.customerFormData.email = this.formData.payerEmail;
        this.customerFormData.address = this.formData.payerAddress || '';
        this.customerFormData.provinceId = this.formData.payerProvinceId;
        this.customerFormData.wardId = this.formData.payerWardId;
        this.customerFormData.fullAddress = this.formData.payerFullAddress || '';
      } else if (targetField === 'beneficiary') {
        this.customerFormData.name = this.formData.beneficiaryName || '';
        this.customerFormData.phone = this.formData.beneficiaryPhone || '';
        this.customerFormData.email = this.formData.beneficiaryEmail;
        this.customerFormData.address = this.formData.beneficiaryAddress || '';
        this.customerFormData.provinceId = this.formData.beneficiaryProvinceId;
        this.customerFormData.wardId = this.formData.beneficiaryWardId;
        this.customerFormData.fullAddress = this.formData.beneficiaryFullAddress || '';
        this.customerFormData.idNo = this.formData.beneficiaryIdNo;
      } else if (targetField === 'vehicleOwner') {
        this.customerFormData.name = this.formData.vehicleOwnerName || '';
        this.customerFormData.phone = this.formData.vehicleOwnerPhone || '';
        this.customerFormData.email = this.formData.vehicleOwnerEmail;
        this.customerFormData.address = this.formData.vehicleOwnerAddress || '';
        this.customerFormData.provinceId = this.formData.vehicleOwnerProvinceId;
        this.customerFormData.wardId = this.formData.vehicleOwnerWardId;
        this.customerFormData.fullAddress = this.formData.vehicleOwnerFullAddress || '';
        this.customerFormData.idNo = this.formData.vehicleOwnerIdNo;
      }

      if (this.customerFormData.provinceId) {
        this.loadCustomerWards(this.customerFormData.provinceId);
      }
    } else if (!customerId) { // Only set to 'create' if no customerId is provided
      this.customerDialogMode = 'create';
      // Default Loại tổ chức to Cá nhân when creating customer, user can change it
      if (targetField === 'customer' && this.defaultOrganizationTypeIdCn) {
        this.customerFormData.organizationTypeId = this.defaultOrganizationTypeIdCn;
      }
    }

    this.customerDialogVisible = true;
  }

  /**
   * Close customer creation dialog
   */
  closeCustomerDialog(): void {
    this.customerDialogVisible = false;
  }

  /**
   * Show confirm dialog when user clicks Cancel in customer dialog; on accept, close the dialog.
   */
  confirmCloseCustomerDialog(): void {
    this.confirmationService.confirm({
      message: this.localizationService.localize('Customer::ResCustomer:CloseWithoutSaveConfirm'),
      header: this.localizationService.localize('Customer::Cancel'),
      icon: 'pi pi-exclamation-triangle',
      accept: () => this.closeCustomerDialog()
    });
  }

  /**
   * Load wards for customer creation dialog
   */
  loadCustomerWards(provinceId?: string | null): void {
    if (!provinceId) {
      this.wardOptions = [];
      this.customerFormData.wardId = null;
      this.updateCustomerFullAddress();
      return;
    }

    this.wardService.getList({
      provinceId: provinceId,
      status: ResWardStatus.Active,
      maxResultCount: 1000,
      skipCount: 0,
      sorting: 'name asc'
    }).subscribe({
      next: (result) => {
        this.wardOptions = (result.items || []).map(ward => ({
          label: ward.name || '',
          value: ward.id || ''
        }));
        this.updateCustomerFullAddress();
      },
      error: () => {
        this.wardOptions = [];
        this.updateCustomerFullAddress();
      }
    });
  }

  onCustomerProvinceChanged(provinceId: string | null): void {
    // Province changed -> reset ward selection + reload ward options
    this.customerFormData.wardId = null;
    this.wardOptions = [];
    this.loadCustomerWards(provinceId);
    this.updateCustomerFullAddress();
  }

  updateCustomerFullAddress(): void {
    const address = (this.customerFormData.address || '').trim();
    const wardLabel = this.getOptionLabel(this.wardOptions, this.customerFormData.wardId);
    const provinceLabel = this.getOptionLabel(this.provinceOptions, this.customerFormData.provinceId);

    const parts = [address, wardLabel, provinceLabel].filter(x => !!x && x.trim().length > 0);
    this.customerFormData.fullAddress = parts.join(', ');
  }

  private getOptionLabel(options: Array<{ label: string; value: string }>, value?: string | null): string {
    if (!value) return '';
    const found = options.find(o => String(o.value) === String(value));
    return (found?.label || '').trim();
  }

  /**
   * Prevent space key in input (phone/email: không cho nhập dấu cách)
   */
  preventSpaceKey(event: KeyboardEvent): void {
    if (event.key === ' ') {
      event.preventDefault();
    }
  }

  /**
   * Normalize phone: remove all spaces (handles paste; SĐT cắt hết dấu cách)
   */
  onCustomerPhoneChange(value: string | null): void {
    const v = (value ?? '').replace(/\D/g, '');
    this.customerFormData.phone = v;
  }

  /**
   * Normalize email: remove all spaces (handles paste)
   */
  onCustomerEmailChange(value: string | null): void {
    const v = (value ?? '').replace(/\s/g, '');
    this.customerFormData.email = v || null;
  }

  /** Email format validation - same rule as backend [EmailAddress]: when value present must be valid email, max 50 chars. */
  private isValidEmailFormat(value: string | null | undefined): boolean {
    const s = value == null ? '' : String(value).trim();
    if (s === '') return true;
    if (s.length > 50) return false;
    return /^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(s);
  }

  /** Phone validation: when value present must contain digits only, length 1–15 (same for Phone, RepPhone, AuthorizerPhone). */
  private isValidPhoneFormat(value: string | null | undefined): boolean {
    const s = value == null ? '' : String(value).trim();
    if (s === '') return true;
    if (s.length > 15) return false;
    return /^[0-9]+$/.test(s);
  }

  /**
   * Create or update customer
   */
  saveCustomer(): void {
    // Normalize phone and emails: strip spaces before validation and save
    this.customerFormData.phone = (this.customerFormData.phone || '').replace(/\s/g, '').trim();
    this.customerFormData.repPhone = (this.customerFormData.repPhone ?? '').replace(/\s/g, '').trim() || '';
    this.customerFormData.authorizerPhone = (this.customerFormData.authorizerPhone ?? '').replace(/\s/g, '').trim() || '';
    this.customerFormData.email = this.customerFormData.email != null && this.customerFormData.email !== ''
      ? (String(this.customerFormData.email).replace(/\s/g, '').trim() || null)
      : null;
    this.customerFormData.repEmail = (this.customerFormData.repEmail ?? '').replace(/\s/g, '').trim() || '';
    this.customerFormData.authorizerEmail = (this.customerFormData.authorizerEmail ?? '').replace(/\s/g, '').trim() || '';

    if (!this.customerFormData.name || this.customerFormData.name.trim() === '') {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Customer::ResCustomer:NameRequired')
      });
      return;
    }

    if (!this.customerFormData.phone || this.customerFormData.phone.trim() === '') {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Customer::ResCustomer:PhoneRequired')
      });
      return;
    }

    if (!this.isValidPhoneFormat(this.customerFormData.phone)) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Customer::ResCustomer:PhoneInvalid')
      });
      return;
    }

    // DOB must be strictly earlier than today (ignore time part)
    if (this.customerFormData.dob) {
      const dob = new Date(this.customerFormData.dob);
      dob.setHours(0, 0, 0, 0);
      const today = new Date();
      today.setHours(0, 0, 0, 0);
      if (dob.getTime() >= today.getTime()) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: 'Ngày sinh phải nhỏ hơn ngày hiện tại'
        });
        return;
      }
    }

    if (!this.customerFormData.address || this.customerFormData.address.trim() === '') {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Customer::ResCustomer:AddressRequired')
      });
      return;
    }

    // When customer modal: Loại tổ chức (Organization Type) is required
    if (this.customerDialogTargetField === 'customer' && !this.customerFormData.organizationTypeId?.trim()) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Customer::ResCustomer:OrganizationTypeRequired')
      });
      return;
    }

    // Email format validation - same logic as backend [EmailAddress] for Email, RepEmail, AuthorizerEmail
    if (this.customerFormData.email && !this.isValidEmailFormat(this.customerFormData.email)) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Customer::ResCustomer:EmailInvalid')
      });
      return;
    }
    if (this.customerDialogTargetField === 'customer' && !this.isCustomerOrgTypeIndividual()) {
      if (this.customerFormData.repEmail && !this.isValidEmailFormat(this.customerFormData.repEmail)) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: this.localizationService.localize('Customer::ResCustomer:RepEmail') + ': ' + this.localizationService.localize('Customer::ResCustomer:EmailInvalid')
        });
        return;
      }
      if (this.customerFormData.authorizerEmail && !this.isValidEmailFormat(this.customerFormData.authorizerEmail)) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: this.localizationService.localize('Customer::ResCustomer:AuthorizerEmail') + ': ' + this.localizationService.localize('Customer::ResCustomer:EmailInvalid')
        });
        return;
      }
      if (this.customerFormData.repPhone && !this.isValidPhoneFormat(this.customerFormData.repPhone)) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: this.localizationService.localize('Customer::ResCustomer:RepPhone') + ': ' + this.localizationService.localize('Customer::ResCustomer:PhoneInvalid')
        });
        return;
      }
      if (this.customerFormData.authorizerPhone && !this.isValidPhoneFormat(this.customerFormData.authorizerPhone)) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: this.localizationService.localize('Customer::ResCustomer:AuthorizerPhone') + ': ' + this.localizationService.localize('Customer::ResCustomer:PhoneInvalid')
        });
        return;
      }
    }

    // Handle snapshot-edit mode
    if (this.customerDialogMode === 'snapshot-edit') {
      if (this.customerDialogTargetField === 'payer') {
        this.formData.payerName = this.customerFormData.name;
        this.formData.payerPhone = this.customerFormData.phone;
        this.formData.payerEmail = this.customerFormData.email;
        this.formData.payerAddress = this.customerFormData.address;
        this.formData.payerFullAddress = this.customerFormData.fullAddress;
        this.formData.payerProvinceId = this.customerFormData.provinceId;
        this.formData.payerWardId = this.customerFormData.wardId;
      } else if (this.customerDialogTargetField === 'beneficiary') {
        this.formData.beneficiaryName = this.customerFormData.name;
        this.formData.beneficiaryPhone = this.customerFormData.phone;
        this.formData.beneficiaryEmail = this.customerFormData.email;
        this.formData.beneficiaryAddress = this.customerFormData.address;
        this.formData.beneficiaryFullAddress = this.customerFormData.fullAddress;
        this.formData.beneficiaryProvinceId = this.customerFormData.provinceId;
        this.formData.beneficiaryWardId = this.customerFormData.wardId;
        this.formData.beneficiaryIdNo = this.customerFormData.idNo;
      } else if (this.customerDialogTargetField === 'vehicleOwner') {
        this.formData.vehicleOwnerName = this.customerFormData.name;
        this.formData.vehicleOwnerPhone = this.customerFormData.phone;
        this.formData.vehicleOwnerEmail = this.customerFormData.email;
        this.formData.vehicleOwnerAddress = this.customerFormData.address;
        this.formData.vehicleOwnerFullAddress = this.customerFormData.fullAddress;
        this.formData.vehicleOwnerProvinceId = this.customerFormData.provinceId;
        this.formData.vehicleOwnerWardId = this.customerFormData.wardId;
        this.formData.vehicleOwnerIdNo = this.customerFormData.idNo;
      }
      this.closeCustomerDialog();
      return;
    }

    this.customerDialogLoading = true;

    if (this.customerDialogMode === 'edit' && this.selectedCustomerId) {
      const updateDto: UpdateResCustomerDto = {
        name: this.customerFormData.name.trim(),
        phone: this.customerFormData.phone.trim(),
        email: this.customerFormData.email?.trim() || undefined,
        organizationTypeId: this.customerFormData.organizationTypeId || undefined,
        provinceId: this.customerFormData.provinceId || undefined,
        wardId: this.customerFormData.wardId || undefined,
        address: this.customerFormData.address.trim(),
        idNo: this.customerFormData.idNo?.trim() || undefined,
        dob: this.customerFormData.dob ? this.formatDateForApi(this.customerFormData.dob) : undefined,
        sex: this.customerFormData.sex ?? undefined,
        status: this.customerFormData.status,
        industryId: this.customerFormData.industryId || undefined,
        businessNo: this.customerFormData.businessNo?.trim() || undefined,
        tin: this.customerFormData.tin?.trim() || undefined,
        repName: this.customerFormData.repName?.trim() || undefined,
        repPhone: this.customerFormData.repPhone?.trim() ? this.customerFormData.repPhone.trim().slice(0, 15) : undefined,
        repEmail: this.customerFormData.repEmail?.trim() || undefined,
        repIdNo: this.customerFormData.repIdNo?.trim() || undefined,
        repTitle: this.customerFormData.repTitle?.trim() || undefined,
        authorizer: this.customerFormData.authorizerName?.trim() || undefined,
        authorizerPhone: this.customerFormData.authorizerPhone?.trim() ? this.customerFormData.authorizerPhone.trim().slice(0, 15) : undefined,
        authorizerEmail: this.customerFormData.authorizerEmail?.trim() || undefined,
        authorizerNo: this.customerFormData.authorizerIdNo?.trim() || undefined,
        authorizerTitle: this.customerFormData.authorizerTitle?.trim() || undefined,
        authorizerDate: this.customerFormData.authorizerDate ? this.formatDateForApi(this.customerFormData.authorizerDate) : undefined
      };

      this.customerService.update(this.selectedCustomerId, updateDto).subscribe({
        next: (updatedCustomer) => {
          this.messageService.add({
            severity: 'success',
            summary: this.localizationService.localize('AbpUi::Success'),
            detail: this.localizationService.localize('Customer::ResCustomer:UpdatedSuccessfully')
          });
          this.refreshCustomerOptionsAndSelect(updatedCustomer.id);
        },
        error: (error) => this.handleCustomerError(error)
      });
    } else {
      const createDto: CreateResCustomerDto = {
        code: this.customerFormData.code?.trim() || undefined,
        name: this.customerFormData.name.trim(),
        phone: this.customerFormData.phone.trim(),
        email: this.customerFormData.email?.trim() || undefined,
        organizationTypeId: this.customerFormData.organizationTypeId || undefined,
        provinceId: this.customerFormData.provinceId || undefined,
        wardId: this.customerFormData.wardId || undefined,
        address: this.customerFormData.address.trim(),
        idNo: this.customerFormData.idNo?.trim() || undefined,
        dob: this.customerFormData.dob ? this.formatDateForApi(this.customerFormData.dob) : undefined,
        sex: this.customerFormData.sex ?? undefined,
        status: this.customerFormData.status,
        industryId: this.customerFormData.industryId || undefined,
        businessNo: this.customerFormData.businessNo?.trim() || undefined,
        tin: this.customerFormData.tin?.trim() || undefined,
        repName: this.customerFormData.repName?.trim() || undefined,
        repPhone: this.customerFormData.repPhone?.trim() ? this.customerFormData.repPhone.trim().slice(0, 15) : undefined,
        repEmail: this.customerFormData.repEmail?.trim() || undefined,
        repIdNo: this.customerFormData.repIdNo?.trim() || undefined,
        repTitle: this.customerFormData.repTitle?.trim() || undefined,
        authorizer: this.customerFormData.authorizerName?.trim() || undefined,
        authorizerPhone: this.customerFormData.authorizerPhone?.trim() ? this.customerFormData.authorizerPhone.trim().slice(0, 15) : undefined,
        authorizerEmail: this.customerFormData.authorizerEmail?.trim() || undefined,
        authorizerNo: this.customerFormData.authorizerIdNo?.trim() || undefined,
        authorizerTitle: this.customerFormData.authorizerTitle?.trim() || undefined,
        authorizerDate: this.customerFormData.authorizerDate ? this.formatDateForApi(this.customerFormData.authorizerDate) : undefined,
        // Cấp đơn: NV sale (res_customer.sale_id) = người cấp đơn trên đơn
        saleId: this.formData.implementerId?.trim() || undefined
      };

      this.customerService.create(createDto).subscribe({
        next: (newCustomer) => {
          this.messageService.add({
            severity: 'success',
            summary: this.localizationService.localize('AbpUi::Success'),
            detail: this.localizationService.localize('Customer::ResCustomer:CreatedSuccessfully')
          });
          this.refreshCustomerOptionsAndSelect(newCustomer.id);
        },
        error: (error) => this.handleCustomerError(error)
      });
    }
  }

  private refreshCustomerOptionsAndSelect(selectedId?: string): void {
    const refreshParams: { status: ResCustomerStatus; maxResultCount: number; skipCount: number; sorting: string; saleId?: string } = {
      status: ResCustomerStatus.Active,
      maxResultCount: 1000,
      skipCount: 0,
      sorting: 'name asc'
    };
    if (this.shouldApplySaleIdFilterForCustomerList()) {
      const empId = this.getCurrentUserEmployeeId();
      if (empId) {
        refreshParams.saleId = empId;
      }
    }
    this.customerService.getList(refreshParams as any).subscribe({
      next: (result) => {
        const customerOptions = (result.items || []).map(customer => ({
          label: customer.name || '',
          value: customer.id || '',
          phone: customer.phone || '',
          email: customer.email || '',
          idNo: customer.idNo || '',
          address: customer.fullAddress || customer.address || ''
        }));
        this.customerOptions = customerOptions;
        this.payerOptions = customerOptions;
        this.vehicleOwnerOptions = customerOptions;
        this.beneficiaryOptions = customerOptions;
        this.ensurePartyCustomersInSelectOptions();

        if (selectedId) {
          switch (this.customerDialogTargetField) {
            case 'customer':
              this.formData.customerId = selectedId;
              this.onCustomerChanged(selectedId);
              break;
            case 'payer':
              this.formData.payerId = selectedId;
              break;
            case 'beneficiary':
              this.formData.beneficiaryId = selectedId;
              break;
            case 'vehicleOwner':
              this.formData.vehicleOwnerId = selectedId;
              break;
          }
        }
        this.customerDialogLoading = false;
        this.closeCustomerDialog();
      },
      error: () => {
        this.customerDialogLoading = false;
      }
    });
  }

  private handleCustomerError(error: any): void {
    const err = error?.error?.error;
    let errorMessage = '';

    const validationErrors = err?.validationErrors;
    if (Array.isArray(validationErrors) && validationErrors.length > 0) {
      const emailInvalidKey = 'Customer::ResCustomer:EmailInvalid';
      const phoneInvalidKey = 'Customer::ResCustomer:PhoneInvalid';
      const lines = validationErrors.map((ve: { message?: string; members?: string[] }) => {
        const members = (ve.members || []).map((m: string) => m.toLowerCase());
        if (members.includes('repemail')) {
          return this.localizationService.localize('Customer::ResCustomer:RepEmail') + ': ' + this.localizationService.localize(emailInvalidKey);
        }
        if (members.includes('authorizeremail')) {
          return this.localizationService.localize('Customer::ResCustomer:AuthorizerEmail') + ': ' + this.localizationService.localize(emailInvalidKey);
        }
        if (members.includes('email')) {
          return this.localizationService.localize(emailInvalidKey);
        }
        if (members.includes('repphone')) {
          return this.localizationService.localize('Customer::ResCustomer:RepPhone') + ': ' + this.localizationService.localize(phoneInvalidKey);
        }
        if (members.includes('authorizerphone')) {
          return this.localizationService.localize('Customer::ResCustomer:AuthorizerPhone') + ': ' + this.localizationService.localize(phoneInvalidKey);
        }
        if (members.includes('phone')) {
          return this.localizationService.localize(phoneInvalidKey);
        }
        return (ve?.message || '').trim();
      }).filter(Boolean);
      errorMessage = lines.join('\n');
    }
    if (!errorMessage) {
      errorMessage = err?.message || err?.details || this.localizationService.localize('AbpUi::InternalServerErrorMessage');
    }

    this.messageService.add({
      severity: 'error',
      summary: this.localizationService.localize('AbpUi::Error'),
      detail: errorMessage
    });
    this.customerDialogLoading = false;
  }

  /**
   * API by-lob-partner requires channelId. Resolve from employee → partner → insurer → DIRECT before calling.
   */
  private ensureChannelIdForProductLoad(done: () => void): void {
    if (this.formData.channelId?.trim()) {
      done();
      return;
    }

    const empId = this.formData.implementerId || this.formData.sellerId;
    if (empId) {
      this.fillChannelFromEmployee(empId, done);
      return;
    }

    const insurerId = this.formData.primaryInsurancePartnerId?.trim();
    if (insurerId) {
      this.partnerService.get(insurerId).subscribe({
        next: (partner) => {
          const rawCh = partner?.channelId as string | null | undefined;
          const ch =
            rawCh != null && String(rawCh).trim() !== '' ? String(rawCh).trim() : '';
          if (ch) {
            this.formData.channelId = ch;
            this.userClearedFormChannel = false;
          } else {
            this.setChannelToDirectIfAvailable();
          }
          this.applySellerOptionsFilterForChannel();
          this.syncChannelLockForResolvedFormChannel();
          this.cdr.markForCheck();
          done();
        },
        error: () => {
          this.setChannelToDirectIfAvailable();
          this.applySellerOptionsFilterForChannel();
          this.syncChannelLockForResolvedFormChannel();
          this.cdr.markForCheck();
          done();
        }
      });
      return;
    }

    this.setChannelToDirectIfAvailable();
    this.applySellerOptionsFilterForChannel();
    done();
  }

  /**
   * Load products based on lobId and partnerId
   */
  loadProducts(): void {
    if (this.isMotorbikeIssuanceUi) {
      this.loadCarTypeOptions();
    }

    if (!this.formData.lobId || !this.formData.primaryInsurancePartnerId) {
      this.products = [];
      this.selectedProducts = {};
      this.productCoverages = {};
      this.deductibleLoadedForProductIds.clear();
      return;
    }

    const executeLoadProductsRequest = (): void => {
      if (!this.formData.channelId?.trim()) {
        this.products = [];
        this.selectedProducts = {};
        this.productCoverages = {};
        this.deductibleLoadedForProductIds.clear();
        return;
      }

    // Use RestService directly to call the correct endpoint
    this.restService.request<any, ProProductDto[]>({
      method: 'GET',
      url: '/api/product/pro-products/by-lob-partner',
      params: {
        lobId: this.formData.lobId,
        partnerId: this.formData.primaryInsurancePartnerId,
        channelId: this.formData.channelId || undefined,
        appChannelId: undefined
      }
    }).subscribe({
      next: (result) => {
        // Handle both single product and array response
        const productsList = Array.isArray(result) ? result : (result ? [result] : []);
        this.products = productsList;

        // Initialize selected products and coverages
        this.selectedProducts = {};
        this.productCoverages = {};
        this.deductibleLoadedForProductIds.clear();

        productsList.forEach(product => {
          if (product.id) {
            this.selectedProducts[product.id] = false;
            // Initialize coverage items from product coverages
            if (product.productCoverages && product.productCoverages.length > 0) {
              // Sort coverages by seqNumber (ascending) in FE
              const sortedProductCoverages = product.productCoverages
                .map((pc, idx) => ({ pc, idx }))
                .sort((a, b) => {
                  const sa = ((a.pc as any).seqNumber ?? 0) as number;
                  const sb = ((b.pc as any).seqNumber ?? 0) as number;
                  if (sa !== sb) return sa - sb;
                  return a.idx - b.idx;
                })
                .map(x => x.pc);

              // Compute hierarchy levels (parent/child) in FE
              const parentRefById = new Map<string, string | undefined>();
              const pcIdByCoverageId = new Map<string, string>();
              sortedProductCoverages.forEach((pc: any) => {
                if (pc?.id) {
                  const parentRef = pc.parentId || pc.coverageParentId || undefined;
                  parentRefById.set(String(pc.id), parentRef ? String(parentRef) : undefined);
                }
                if (pc?.coverageId && pc?.id) {
                  pcIdByCoverageId.set(String(pc.coverageId), String(pc.id));
                }
              });

              const levelCache = new Map<string, number>();
              const getLevel = (pcId: string, visiting: Set<string>): number => {
                if (levelCache.has(pcId)) return levelCache.get(pcId) as number;
                if (visiting.has(pcId)) return 0; // break cycles
                visiting.add(pcId);

                const rawParent = parentRefById.get(pcId);
                if (!rawParent) {
                  levelCache.set(pcId, 0);
                  visiting.delete(pcId);
                  return 0;
                }

                // parentId might reference productCoverage.id OR coverageId; resolve if needed
                const resolvedParentPcId =
                  parentRefById.has(rawParent) ? rawParent : (pcIdByCoverageId.get(rawParent) || undefined);

                if (!resolvedParentPcId || resolvedParentPcId === pcId) {
                  levelCache.set(pcId, 0);
                  visiting.delete(pcId);
                  return 0;
                }

                const parentLevel = getLevel(resolvedParentPcId, visiting);
                const nextLevel = Math.min(parentLevel + 1, 10); // cap to avoid huge indentation
                levelCache.set(pcId, nextLevel);
                visiting.delete(pcId);
                return nextLevel;
              };

              const issueDateForRules = this.getIssueDateForCoverageLevelRules();

              this.productCoverages[product.id] = sortedProductCoverages.map(pc => {
                // Access coverage property safely (may not exist in frontend model yet)
                const coverage = (pc as any).coverage;
                const coverageType = coverage?.type ?? undefined;
                const availabilityTypeRaw = pc.availabilityType; // ProProductCoverageAvailabilityType enum: 0=Required, 1=Standard, 2=Optional, 3=Selectable

                // Convert availabilityType to number for comparison (enum values: 0=Required, 1=Standard, 2=Optional, 3=Selectable)
                let availabilityTypeNum: number | undefined = undefined;
                if (typeof availabilityTypeRaw === 'number') {
                  availabilityTypeNum = availabilityTypeRaw;
                } else if (availabilityTypeRaw !== undefined && availabilityTypeRaw !== null) {
                  // Handle string values if API returns them
                  const str = String(availabilityTypeRaw).toLowerCase();
                  if (str === 'required') availabilityTypeNum = 0;
                  else if (str === 'standard') availabilityTypeNum = 1;
                  else if (str === 'optional') availabilityTypeNum = 2;
                  else if (str === 'selectable') availabilityTypeNum = 3;
                }

                // Determine checkbox state based on availabilityType
                let selected: boolean | undefined = undefined;
                let isCheckboxDisabled = false;
                let initialQuantity = 1; // Default quantity

                if (coverageType === 0) {
                  // Main coverage: quantity always = 1
                  initialQuantity = 1;
                } else {
                  // For non-main coverages, set checkbox state based on availabilityType
                  // availabilityType is numeric enum: 0=Required, 1=Standard, 2=Optional, 3=Selectable
                  if (availabilityTypeNum === 0) {
                    // Required: tick checkbox and disable it
                    selected = true;
                    isCheckboxDisabled = true;
                    initialQuantity = 1; // Checkbox is ticked, so quantity = 1
                  } else if (availabilityTypeNum === 1) {
                    // Standard: tick checkbox by default but allow change
                    selected = true;
                    isCheckboxDisabled = false;
                    initialQuantity = 1; // Checkbox is ticked, so quantity = 1
                  } else if (availabilityTypeNum === 2) {
                    // Optional: don't tick by default but allow change
                    selected = false;
                    isCheckboxDisabled = false;
                    initialQuantity = 0; // Checkbox is not ticked, so quantity = 0
                  } else if (availabilityTypeNum === 3) {
                    // Selectable: user chọn ít nhất 1 trong nhóm khi lưu; mặc định chưa tick
                    selected = false;
                    isCheckboxDisabled = false;
                    initialQuantity = 0;
                  } else {
                    // Undefined / unknown: treat like optional
                    selected = false;
                    isCheckboxDisabled = false;
                    initialQuantity = 0;
                  }
                }

                // Liability Amount constraints (from ProductCoverageLevels.Terms where coverageLevelTypeCode == 'LIABILITY_AMOUNT')
                const liability = this.getLiabilityAmountConstraintFromProductCoverage(pc as any, issueDateForRules);
                const liabilityFixed = liability?.fixed ?? null;
                const liabilityMin = liability?.min ?? null;
                const liabilityMax = liability?.max ?? null;
                const liabilityOptions =
                  liability?.options && liability.options.length >= 2 ? liability.options : undefined;

                // Deductible options: loaded via separate API when product is selected (ensureDeductibleOptionsForProducts)
                const deductibleOptions: Array<{ label: string; value: string }> | undefined = undefined;
                const deductibleTermByValue: CoverageItem['deductibleTermByValue'] = undefined;

                // For fixed / discrete list: auto-fill when coverage is main OR selected by default
                let initialInsuranceAmount: number | null = null;
                if (liabilityOptions?.length) {
                  if (coverageType === 0 || selected === true) {
                    initialInsuranceAmount = this.getDefaultLiabilityValueFromOptions(liabilityOptions);
                  }
                } else if (liabilityFixed !== null && liabilityFixed !== undefined) {
                  if (coverageType === 0 || selected === true) {
                    initialInsuranceAmount = liabilityFixed;
                  }
                }

                // Initial deductible set when deductible options are loaded (deductible-options API)
                const initialDeductible: string | null = null;

                // Interactions (dependency / incompatible / exclusive) from by-lob API
                const rawInteractions = (pc as any).productCoverageInteractions || (pc as any).interactions || [];
                const productCoverageInteractions = Array.isArray(rawInteractions) ? rawInteractions : [];

                return {
                  productCoverageId: pc.id, // Store ProProductCoverage.Id
                  coverageId: pc.coverageId,
                  coverageCode: coverage?.code ?? (pc as any).coverage?.code ?? undefined,
                  coverageParentId: (pc as any).parentId || (pc as any).coverageParentId || undefined,
                  hierarchyLevel: pc?.id ? getLevel(String(pc.id), new Set<string>()) : 0,
                  uomId: pc.uomId,
                  taxId: pc.taxId,
                  taxCode: (pc as any)?.tax?.code || null,
                  enableQuantity: (pc as any).enableQuantity || undefined,
                  insurerCoverageCode: pc.insurerCoverageCode,
                  benefit: coverage?.name || pc.coverageId || '',
                  insuranceAmount: initialInsuranceAmount,
                  liabilityAmountFixed: liabilityFixed,
                  liabilityAmountMin: liabilityMin,
                  liabilityAmountMax: liabilityMax,
                  liabilityAmountOptions: liabilityOptions,
                  quantity: initialQuantity, // Set based on coverage type and checkbox state
                  taxRate: null,
                  premiumRate: null,
                  premium: 0,
                  vat: 0,
                  premiumWithVAT: 0,
                  deductible: initialDeductible,
                  deductibleOptions,
                  deductibleTermByValue,
                  premiumChange: null,
                  description: coverage?.description || null,
                  productCoverageInteractions,
                  coverageType: coverageType, // Store coverage type: 0 = Main, others = disabled
                  selected: selected, // Initialize checkbox state based on availabilityType
                  availabilityType: availabilityTypeNum !== undefined ? String(availabilityTypeNum) : undefined,
                  isCheckboxDisabled: isCheckboxDisabled
                };
              });
            } else {
              this.productCoverages[product.id] = [];
            }
          }
        });

        // Không áp dụng logic interaction khi load form; chỉ áp dụng khi user click coverage

        // Update summary after products are loaded
        this.updateSummary();

        // If we are opening edit modal, patch selections from detail after products exist
        let detailForDeductible: any = null;
        if (this.pendingDetailForEdit) {
          const detail = this.pendingDetailForEdit;
          this.pendingDetailForEdit = null;
          detailForDeductible = detail;
          this.applyDetailToLoadedProducts(detail);
          this.onDetailAppliedToProducts();
        }
        // Load deductible options for currently selected products (edit/view: ưu tiên giá trị đã lưu).
        // Pass detailForDeductible so that after options load we can map saved coverageLevels -> deductible (deductibleTermByValue is only available after this API returns).
        this.ensureDeductibleOptionsForProducts(this.getSelectedProductIds(), true, detailForDeductible);
        // Refresh required attribute spec for đối tượng bảo hiểm based on selected products
        this.fetchAndLogAttributeRequiredSpec();
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Policy::Error'),
          detail: this.localizationService.localize('Policy::FailedToLoadProducts') || 'Failed to load products'
        });
        this.products = [];
        this.selectedProducts = {};
        this.productCoverages = {};
        this.requiredAttributeParameterNames.clear();
        // Update summary even on error to clear it
        this.updateSummary();
      }
    });
    };

    if (!this.formData.channelId?.trim()) {
      if (this.userClearedFormChannel) {
        this.cdr.markForCheck();
        executeLoadProductsRequest();
        return;
      }
      this.ensureChannelIdForProductLoad(() => {
        this.cdr.markForCheck();
        executeLoadProductsRequest();
      });
      return;
    }

    executeLoadProductsRequest();
  }

  /**
   * Get coverage items for a product
   */
  getProductCoverages(productId: string): CoverageItem[] {
    return this.productCoverages[productId] || [];
  }

  /**
   * Get main coverage items (type === 0) for a product
   */
  getMainCoverages(productId: string): CoverageItem[] {
    const coverages = this.productCoverages[productId] || [];
    return coverages.filter(item => item.coverageType === 0);
  }

  /**
   * Get non-main coverage items (type !== 0) for a product
   */
  getNonMainCoverages(productId: string): CoverageItem[] {
    const coverages = this.productCoverages[productId] || [];
    return coverages.filter(item => item.coverageType !== 0);
  }

  /**
   * Get all coverages with section headers interleaved
   */
  getAllCoveragesWithSections(productId: string): CoverageItem[] {
    const mainCoverages = this.getMainCoverages(productId);
    const nonMainCoverages = this.getNonMainCoverages(productId);
    const result: CoverageItem[] = [];

    // Add main coverages section if it has items
    if (mainCoverages.length > 0) {
      result.push({
        benefit: '1. Phạm vi chính',
        insuranceAmount: null,
        quantity: null,
        taxRate: null,
        premiumRate: null,
        premium: 0,
        vat: 0,
        premiumWithVAT: 0,
        deductible: null,
        description: null,
        isSectionHeader: true
      });
      result.push(...mainCoverages);
    }

    // Add non-main coverages section if it has items
    if (nonMainCoverages.length > 0) {
      result.push({
        benefit: '2. Phạm vi bổ sung',
        insuranceAmount: null,
        quantity: null,
        taxRate: null,
        premiumRate: null,
        premium: 0,
        vat: 0,
        premiumWithVAT: 0,
        deductible: null,
        description: null,
        isSectionHeader: true
      });
      result.push(...nonMainCoverages);
    }

    return result;
  }

  /**
   * Check if a product is selected
   */
  isProductSelected(productId: string): boolean {
    return this.selectedProducts[productId] || false;
  }

  /**
   * Toggle product selection
   */
  toggleProduct(productId: string): void {
    if (this.selectedProducts[productId] !== undefined) {
      this.selectedProducts[productId] = !this.selectedProducts[productId];
      if (this.selectedProducts[productId]) {
        // Load deductible options for this product (chỉ load lần đầu, cache theo deductibleLoadedForProductIds)
        this.ensureDeductibleOptionsForProducts([productId]);
        // When enabling a product, immediately recalculate its fee
        this.applyCarValueToMainCoverageIfVcx(productId);
        this.syncVcxNonMainCoverageAmounts(productId);
        this.scheduleProductPremiumRecalc(productId);
      }
      // Update summary when product selection changes
      this.updateSummary();
      // Check attribute required spec for selected products (đối tượng bảo hiểm) and log to console
      this.fetchAndLogAttributeRequiredSpec();
    }
  }

  /**
   * Normalize parameter name to camelCase so API "CarProductionYear" matches FE "carProductionYear".
   */
  private normalizeAttributeParameterName(name: string): string {
    if (!name || typeof name !== 'string') return '';
    const t = name.trim();
    if (t.length === 0) return '';
    return t.charAt(0).toLowerCase() + t.slice(1);
  }

  /**
   * When product selection changes, call get-attribute-required-spec for each selected product
   * and update requiredAttributeParameterNames so đối tượng bảo hiểm inputs show required only when API says so.
   * If a parameter appears multiple times in the response, it is required when at least one occurrence has isRequired true.
   */
  private fetchAndLogAttributeRequiredSpec(): void {
    const selectedIds = this.products
      .filter(p => p.id && this.selectedProducts[p.id])
      .map(p => p.id as string);

    if (selectedIds.length === 0) {
      this.requiredAttributeParameterNames.clear();
      return;
    }

    const motorPayload = this.buildRiskMotorPayload();
    const requests = selectedIds.map(productId => {
      const product = this.products.find(p => p.id === productId);
      if (!product?.id) return of({ items: [] as { parameterName?: string; isRequired?: boolean }[] });
      const productAttributeDefs = this.buildProductAttributeDefinitionsForExtract(product);
      const extractReqBody: ExtractPolicyAttributeParametersInputDto = {
        amountLiability: 0,
        riskObject: { riskObjectMotor: motorPayload } as any,
        product: {
          productId: productId,
          attributes: productAttributeDefs as any
        } as any
      };
      return this.policyService.getAttributeRequiredSpec(extractReqBody);
    });

    forkJoin(requests).subscribe({
      next: (responses) => {
        const required = new Set<string>();
        responses.forEach(res => {
          const items = (res?.items ?? []) as { parameterName?: string; isRequired?: boolean }[];
          items.forEach(item => {
            // Parameter is required if this item is required; same parameter can appear multiple times (e.g. CarProductionYear for carAge and carYear)
            if (item.isRequired && item.parameterName) {
              required.add(this.normalizeAttributeParameterName(item.parameterName));
            }
          });
        });
        this.requiredAttributeParameterNames = required;
      },
      error: () => {
        this.requiredAttributeParameterNames.clear();
      }
    });
  }

  /**
   * Whether the đối tượng bảo hiểm field for the given API parameter name is required (from get-attribute-required-spec).
   * If the parameter appears multiple times in the response, required when at least one has isRequired true.
   * Parameter name is normalized (camelCase) so API "CarProductionYear" matches "carProductionYear".
   */
  isInsuredObjectFieldRequired(parameterName: string): boolean {
    const n = this.normalizeAttributeParameterName(parameterName);
    if (this.isMotorbikeReducedInsuredObjectSection) {
      const suppressedForMotorbikeMinimal = new Set([
        'carUsage',
        'carBrandCode',
        'carModelCode',
        'carCategoryCode',
        'carSeatNumber',
        'carPayloadCapacity',
        'riskObjectValue',
        'carColor',
        'carProductionYear',
        'carOrigin',
        'carLineCode',
        'carGroupCode'
      ]);
      if (suppressedForMotorbikeMinimal.has(n)) {
        return false;
      }
    }
    const vcxOnly = new Set(['riskObjectValue', 'carColor', 'carOrigin', 'carNew']);
    if (vcxOnly.has(n) && !this.hasSelectedVcxProduct()) {
      return false;
    }
    return this.requiredAttributeParameterNames.has(n);
  }

  private scheduleProductPremiumRecalc(productId: string, delayMs: number = 300): void {
    if (!productId) return;
    try {
      const prev = this.productPremiumRecalcTimers[productId];
      if (prev) {
        clearTimeout(prev);
      }
      this.productPremiumRecalcTimers[productId] = setTimeout(() => {
        this.calculateProductPremium(productId);
      }, delayMs);
    } catch {
      // Fallback: just calculate without debounce
      this.calculateProductPremium(productId);
    }
  }

  private clearProductFeeData(productId: string): void {
    const coverages = this.productCoverages[productId] || [];
    coverages.forEach(item => {
      if (!item || item.isSectionHeader) return;
      item.premium = 0;
      item.vat = 0;
      item.premiumWithVAT = 0;
      item.premiumRate = null;
      // Pricing metadata (set by get-price response)
      item.tableRateLineId = undefined;
      item.baseRate = null;
      item.flatRate = null;
      // taxRate may be set by pricing response; keep taxCode label but clear numeric value
      item.taxRate = null;
    });
  }

  /** So sánh theo productTypeCode (pro_product_type.code) = VCX */
  public isVcxProductCode(productOrCode: { productTypeCode?: string | null } | string | undefined | null): boolean {
    if (productOrCode == null) return false;
    const code = typeof productOrCode === 'string' ? productOrCode : (productOrCode?.productTypeCode ?? '');
    return (code || '').toString().trim().toUpperCase() === 'VCX';
  }

  /** Có ít nhất một sản phẩm đang tham gia (toggle bật) là TNDSBB — đối tượng thụ hưởng không bắt buộc. */
  public hasSelectedTndsbbProduct(): boolean {
    return this.products.some(p => {
      if (!p?.id || !this.isProductSelected(p.id)) {
        return false;
      }
      const code = (p.productTypeCode ?? '').toString().trim().toUpperCase();
      return code === 'TNDSBB';
    });
  }

  /** Đã chọn sản phẩm VCX — dùng hiển thị/validate nhóm trường giá trị xe, màu, nguồn gốc, xe mới, ảnh xe. */
  public hasSelectedVcxProduct(): boolean {
    return this.products.some(
      p => !!p?.id && this.isProductSelected(p.id) && this.isVcxProductCode(p)
    );
  }

  private applyCarValueToMainCoverageIfVcx(productId: string): void {
    if (!productId) return;
    const product = this.products.find(p => p.id === productId);
    if (!product || !this.isVcxProductCode(product)) return;

    const carValue = this.formData?.carValue;
    if (carValue === null || carValue === undefined) return;

    const coverages = this.productCoverages[productId] || [];
    const main = this.getMainCoverageItem(productId) || coverages.find(c => !!c && !c.isSectionHeader);
    if (!main) return;

    // Auto-fill: VCX main coverage amount = car value (discrete liability list → snap to nearest mức)
    if (this.hasLiabilityAmountSelect(main)) {
      const cv = Number(carValue);
      const fallback = this.getDefaultLiabilityOptionValue(main) ?? 0;
      main.insuranceAmount = this.snapInsuranceAmountToNearestLiabilityOption(
        main,
        isFinite(cv) ? cv : fallback
      );
      return;
    }
    main.insuranceAmount = carValue;
  }

  private isVcxProduct(productId: string): boolean {
    if (!productId) return false;
    const product = this.products.find(p => p.id === productId);
    return !!product && this.isVcxProductCode(product);
  }

  private getMainCoverageItem(productId: string): CoverageItem | null {
    const coverages = (this.productCoverages[productId] || []).filter(c => !!c && !c.isSectionHeader);
    if (coverages.length === 0) return null;

    // Prefer explicit Main term type (ProCoverageTermType.Main == 0)
    const mainByType = coverages.find(c => c.coverageType === 0);
    if (mainByType) return mainByType;

    // Fallback: first top-level coverage (no parent)
    const topLevel = coverages.find(c => !c.coverageParentId);
    return topLevel || coverages[0] || null;
  }

  private getMainCoverageAmount(productId: string): number | null {
    const main = this.getMainCoverageItem(productId);
    const n = Number(main?.insuranceAmount);
    return isFinite(n) && n > 0 ? n : null;
  }

  private syncVcxNonMainCoverageAmounts(productId: string): void {
    if (!this.isVcxProduct(productId)) return;
    const mainItem = this.getMainCoverageItem(productId);
    const mainAmount = this.getMainCoverageAmount(productId);
    if (mainAmount === null) return;

    const coverages = this.productCoverages[productId] || [];
    coverages.forEach(c => {
      if (!c || c.isSectionHeader) return;
      if (mainItem?.productCoverageId && c.productCoverageId === mainItem.productCoverageId) return; // main
      if (!c.selected) return; // only ticked
      // Do not override fixed liability or discrete-option rows (user / product defines amount)
      if (c.liabilityAmountFixed !== null && c.liabilityAmountFixed !== undefined) return;
      if (this.hasLiabilityAmountSelect(c)) return;
      c.insuranceAmount = mainAmount;
    });
  }

  private getIssueDateForCoverageLevelRules(): Date {
    // Create: issueDate is "now" (server sets it too)
    if (this.dialogMode === 'create') {
      return new Date();
    }

    // Update/View: use issueDate from policy detail if possible, else fallback to now
    const d = this.ensureDate(this.formData?.issueDate as any);
    return d || new Date();
  }

  /**
   * Load deductible (mức miễn thường) options for given product IDs via dedicated API.
   * Chỉ gọi API cho những productId chưa load (deductibleLoadedForProductIds); tick/untick không load lại.
   * @param preserveSavedValue true khi mở form update/view lần đầu: ưu tiên giá trị đã lưu. false (mặc định): logic cũ, luôn set theo option đầu tiên từ BE.
   * @param savedDetail when provided (edit/view), after options load we map coverageLevels from this detail to item.deductible so saved value (e.g. 500) is shown instead of first option (e.g. 1000).
   */
  ensureDeductibleOptionsForProducts(productIds: string[], preserveSavedValue = false, savedDetail?: any): void {
    const toLoad = productIds.filter(id => id && !this.deductibleLoadedForProductIds.has(id));
    if (toLoad.length === 0) return;

    const issueDate = this.getIssueDateForCoverageLevelRules();
    const carGroup = this.formData?.carGroupId && this.carGroupCodeById[this.formData.carGroupId] != null
      ? this.carGroupCodeById[this.formData.carGroupId]
      : undefined;
    const carType = this.formData?.vehicleTypeId && this.carTypeCodeById[this.formData.vehicleTypeId] != null
      ? this.carTypeCodeById[this.formData.vehicleTypeId]
      : undefined;
    const seatingCapacity = this.formData?.seatingCapacity != null ? Number(this.formData.seatingCapacity) : undefined;
    const weight = this.formData?.weight != null ? Number(this.formData.weight) : undefined;
    const carPurpose = (this.formData?.carUsage || '').trim() || undefined;

    this.restService.request<any, any[]>({
      method: 'POST',
      url: '/api/product/pro-products/deductible-options',
      body: {
        productIds: toLoad.map(id => id),
        issueDate: issueDate.toISOString(),
        carGroup: carGroup ?? null,
        carType: carType ?? null,
        seatingCapacity: seatingCapacity ?? null,
        weight: weight ?? null,
        carPurpose: carPurpose ?? null
      }
    }).subscribe({
      next: (items: any[]) => {
        if (!Array.isArray(items)) return;
        for (const row of items) {
          const productId = row.productId ? String(row.productId) : null;
          const productCoverageId = row.productCoverageId != null ? String(row.productCoverageId) : null;
          if (!productId || !productCoverageId) continue;

          const coverages = this.productCoverages[productId];
          if (!coverages) continue;

          const options = Array.isArray(row.options) ? row.options : [];
          const termByValue = row.termByValue && typeof row.termByValue === 'object' ? row.termByValue : {};
          const firstValue = options.length > 0 && options[0].value != null ? String(options[0].value) : null;

          for (const item of coverages) {
            if (String(item.productCoverageId) !== productCoverageId) continue;
            // Trước khi ghi đè termByValue: lưu fromAmount đang chọn (để khi reload options vẫn ưu tiên cùng mức, dù term id đổi)
            const prevTermByValue = item.deductibleTermByValue;
            const prevDeductible = item.deductible != null && item.deductible !== '' ? String(item.deductible) : null;
            const preservedFromAmount = preserveSavedValue && prevTermByValue && prevDeductible
              ? (prevTermByValue[prevDeductible]?.fromAmount ?? null)
              : null;

            item.deductibleOptions = options.map((o: any) => {
              const val = o.value != null ? Number(o.value) : NaN;
              const apiLabel = o.label != null ? String(o.label).trim() : '';
              const label = apiLabel !== '' ? apiLabel : (isFinite(val) ? this.formatMoneyLabel(val) : String(o.value));
              return { label, value: String(o.value) };
            });
            item.deductibleTermByValue = Object.keys(termByValue).length > 0 ? termByValue : undefined;
            if (preserveSavedValue && savedDetail) {
              // Edit/view: map saved coverageLevels -> deductible now that deductibleTermByValue is set (API vừa trả về).
              this.applySavedDeductibleFromDetail(item, productId, savedDetail);
            }
            if (!preserveSavedValue) {
              // onchange (nhóm xe, loại xe, số chỗ, trọng tải): luôn lấy option đầu tiên
              item.deductible = firstValue ?? null;
              if (typeof console !== 'undefined' && console.log) {
                console.log('[ensureDeductibleOptions] preserveSavedValue=false → set firstValue', { productId, productCoverageId: item.productCoverageId, firstValue, optionsCount: options.length });
              }
            } else if (item.deductible == null || item.deductible === '') {
              // preserveSavedValue=true nhưng chưa có giá trị: ưu tiên từ savedDetail hoặc fromAmount, không thì firstValue
              if (preservedFromAmount != null && typeof preservedFromAmount === 'number' && isFinite(preservedFromAmount)) {
                const newKeySameAmount = Object.keys(termByValue).find(
                  (k) => Number((termByValue as any)[k]?.fromAmount) === preservedFromAmount
                );
                if (newKeySameAmount != null) {
                  item.deductible = newKeySameAmount;
                } else {
                  const optionValues = options.map((o: any) => o.value != null ? String(o.value) : '');
                  item.deductible = optionValues.includes(prevDeductible ?? '') ? (prevDeductible ?? firstValue) : (firstValue ?? null);
                }
              } else {
                const optionValues = options.map((o: any) => o.value != null ? String(o.value) : '');
                const currentDeductible = prevDeductible;
                if (currentDeductible != null && optionValues.includes(currentDeductible)) {
                  item.deductible = currentDeductible;
                } else {
                  item.deductible = firstValue ?? null;
                }
              }
            } else if (preserveSavedValue && preservedFromAmount != null && isFinite(Number(preservedFromAmount))) {
              // preserveSavedValue=true và đã có deductible: khi reload, giữ cùng fromAmount nếu có trong options mới
              const newKeySameAmount = Object.keys(termByValue).find(
                (k) => Number((termByValue as any)[k]?.fromAmount) === Number(preservedFromAmount)
              );
              if (newKeySameAmount != null && newKeySameAmount !== item.deductible) {
                item.deductible = newKeySameAmount;
              }
            }
            break;
          }
        }
        toLoad.forEach(id => this.deductibleLoadedForProductIds.add(id));
      },
      error: () => {
        this.messageService.add({
          severity: 'warn',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: this.localizationService.localize('Policy::Policy:LoadDeductibleOptionsFailed') ?? 'Không tải được mức miễn thường.'
        });
      }
    });
  }

  private parseDate(value: any): Date | null {
    if (!value) return null;
    if (value instanceof Date) return isNaN(value.getTime()) ? null : value;
    const d = new Date(value);
    return isNaN(d.getTime()) ? null : d;
  }

  private isIssueDateInLevelRange(issueDate: Date, level: any): boolean {
    const effect = this.parseDate(level?.effectDate ?? level?.EffectDate);
    const expire = this.parseDate(level?.expireDate ?? level?.ExpireDate);
    if (!effect) return false;
    const t = issueDate.getTime();
    const from = effect.getTime();
    const to = expire ? expire.getTime() : Number.POSITIVE_INFINITY;
    return t >= from && t <= to;
  }

  private getTermsByCoverageLevelTypeCode(pc: any, typeCode: string, issueDate: Date): any[] {
    const normalizedType = (typeCode || '').trim().toUpperCase();
    if (!normalizedType) return [];

    const levels = pc?.productCoverageLevels || pc?.coverageLevels || pc?.CoverageLevels || [];
    if (!Array.isArray(levels) || levels.length === 0) {
      return [];
    }

    const result: any[] = [];
    for (const level of levels) {
      if (!this.isIssueDateInLevelRange(issueDate, level)) {
        continue;
      }

      const terms = level?.terms || level?.Terms || [];
      if (!Array.isArray(terms) || terms.length === 0) {
        continue;
      }

      for (const term of terms) {
        const rawCode =
          term?.coverageLevelTypeCode ||
          term?.CoverageLevelTypeCode ||
          term?.coverageLevelType?.code ||
          term?.CoverageLevelType?.Code;
        const code = rawCode ? String(rawCode).trim().toUpperCase() : '';
        if (code === normalizedType) {
          result.push(term);
        }
      }
    }

    return result;
  }

  /**
   * True when số tiền BH must be chosen from a discrete list (2+ LIABILITY_AMOUNT point terms).
   */
  protected hasLiabilityAmountSelect(item: CoverageItem | undefined | null): boolean {
    const n = item?.liabilityAmountOptions?.length ?? 0;
    return n >= 2;
  }

  /** pro_product_coverage_level_term.is_default — 'Y' / truthy treated as default for that term. */
  private termLiabilityIsDefault(term: any): boolean {
    const v = (term?.isDefault ?? term?.IsDefault ?? '').toString().trim().toUpperCase();
    return v === 'Y';
  }

  /**
   * Selected value for liability select: first option marked isDefault, else first option.
   */
  protected getDefaultLiabilityOptionValue(item: CoverageItem | null | undefined): number | null {
    const opts = item?.liabilityAmountOptions;
    if (!opts?.length) return null;
    return this.getDefaultLiabilityValueFromOptions(opts);
  }

  private getDefaultLiabilityValueFromOptions(
    options: Array<{ label: string; value: number; isDefault?: boolean }>
  ): number {
    const marked = options.find(o => o.isDefault === true);
    return marked?.value ?? options[0].value;
  }

  private getLiabilityAmountConstraintFromProductCoverage(
    pc: any,
    issueDate: Date
  ): {
    fixed?: number;
    min?: number;
    max?: number;
    options?: Array<{ label: string; value: number; isDefault?: boolean }>;
  } | null {
    const terms = this.getTermsByCoverageLevelTypeCode(pc, 'LIABILITY_AMOUNT', issueDate);
    if (!terms.length) return null;

    let firstDefaultPointAmount: number | undefined;
    for (const term of terms) {
      const fromRaw = term?.fromAmount ?? term?.FromAmount;
      const toRaw = term?.toAmount ?? term?.ToAmount;
      const from = Number(fromRaw);
      const to = Number(toRaw);
      if (!isFinite(from) || !isFinite(to) || from !== to) continue;
      if (this.termLiabilityIsDefault(term)) {
        firstDefaultPointAmount = from;
        break;
      }
    }

    const pointValuesOrdered: number[] = [];
    const seen = new Set<number>();
    for (const term of terms) {
      const fromRaw = term?.fromAmount ?? term?.FromAmount;
      const toRaw = term?.toAmount ?? term?.ToAmount;
      const from = Number(fromRaw);
      const to = Number(toRaw);
      if (!isFinite(from) || !isFinite(to)) continue;
      if (from === to && !seen.has(from)) {
        seen.add(from);
        pointValuesOrdered.push(from);
      }
    }

    if (pointValuesOrdered.length >= 2) {
      return {
        options: pointValuesOrdered.map(v => ({
          label: this.formatMoneyLabel(v),
          value: v,
          ...(firstDefaultPointAmount === v ? { isDefault: true } : {})
        }))
      };
    }

    if (pointValuesOrdered.length === 1) {
      return { fixed: pointValuesOrdered[0] };
    }

    const term = terms[0];
    const fromRaw = term?.fromAmount ?? term?.FromAmount;
    const toRaw = term?.toAmount ?? term?.ToAmount;
    const from = Number(fromRaw);
    const to = Number(toRaw);
    if (!isFinite(from) || !isFinite(to)) return null;

    if (from === to) return { fixed: from };
    if (from < to) return { min: from, max: to };
    return null;
  }

  /** Snap desired amount to the nearest option (e.g. VCX main vs giá trị xe). */
  private snapInsuranceAmountToNearestLiabilityOption(item: CoverageItem, desired: number): number {
    const opts = item.liabilityAmountOptions;
    if (!opts?.length) return desired;
    const defVal = this.getDefaultLiabilityOptionValue(item);
    let best = defVal ?? opts[0].value;
    let bestDiff = Math.abs(best - desired);
    for (const o of opts) {
      const d = Math.abs(o.value - desired);
      if (d < bestDiff) {
        bestDiff = d;
        best = o.value;
      } else if (d === bestDiff && defVal != null && o.value === defVal) {
        best = o.value;
      }
    }
    return best;
  }

  /** If current insuranceAmount is not one of the allowed options, reset to default option (is_default) or first. */
  private normalizeInsuranceAmountToLiabilityOptions(item: CoverageItem): void {
    const opts = item.liabilityAmountOptions;
    if (!opts?.length) return;
    const allowed = new Set(opts.map(o => o.value));
    const cur = item.insuranceAmount;
    if (cur == null || !allowed.has(Number(cur))) {
      item.insuranceAmount = this.getDefaultLiabilityValueFromOptions(opts);
    }
  }

  private formatMoneyLabel(value: number): string {
    const n = Number(value);
    if (!isFinite(n)) return '';
    try {
      // Use Vietnamese-style grouping by default; adjust locale later if needed.
      return new Intl.NumberFormat('vi-VN', { maximumFractionDigits: 0 }).format(n);
    } catch {
      // Fallback
      return String(Math.round(n));
    }
  }

  private getDeductibleTermByValueFromProductCoverage(
    pc: any,
    issueDate: Date
  ): NonNullable<CoverageItem['deductibleTermByValue']> {
    const terms = this.getTermsByCoverageLevelTypeCode(pc, 'DEDUCTIBLE', issueDate);
    if (!terms.length) return {};

    const map: NonNullable<CoverageItem['deductibleTermByValue']> = {};

    for (const t of terms) {
      const fromRaw = t?.fromAmount ?? t?.FromAmount;
      const toRaw = t?.toAmount ?? t?.ToAmount;
      const fromAmount = Number(fromRaw);
      const toAmount = isFinite(Number(toRaw)) ? Number(toRaw) : fromAmount;
      if (!isFinite(fromAmount)) continue;

      const coverageLevelTypeId =
        t?.coverageLevelTypeId ??
        t?.CoverageLevelTypeId ??
        t?.coverageLevelType?.id ??
        t?.CoverageLevelType?.Id;

      const coverageLevelBasisId =
        t?.coverageLevelBasisId ??
        t?.CoverageLevelBasisId ??
        t?.coverageLevelBasis?.id ??
        t?.CoverageLevelBasis?.Id;

      const amountTypeRaw = t?.amountType ?? t?.AmountType ?? 'FIXED';
      const amountType = String(amountTypeRaw || 'FIXED');

      if (!coverageLevelTypeId || !coverageLevelBasisId) continue;

      const key = String(fromAmount);
      // Keep first occurrence to avoid unexpected overwrites
      if (map[key]) continue;

      map[key] = {
        coverageLevelTypeId: String(coverageLevelTypeId),
        coverageLevelBasisId: String(coverageLevelBasisId),
        amountType,
        fromAmount,
        toAmount: isFinite(toAmount) ? toAmount : fromAmount,
        conditionScript: (t?.conditionScript ?? t?.ConditionScript ?? null) as any,
        computeScript: (t?.computeScript ?? t?.ComputeScript ?? null) as any
      };
    }

    return map;
  }

  /**
   * Resolves the deductible value to send to the product price API.
   * The API expects the deductible amount (fromAmount) for rate-line condition matching, not the term id.
   * When the selection is a term id (UUID), we look up fromAmount from deductibleTermByValue.
   */
  private getDeductibleValueForPriceApi(item: CoverageItem): string | undefined {
    if (item.deductible == null || item.deductible === '') return undefined;
    const meta = item.deductibleTermByValue?.[item.deductible];
    if (meta != null) return String(meta.fromAmount);
    return item.deductible;
  }

  private normalizeInteractionTypeCode(interaction: any): 'dependency' | 'incompatible' | 'exclusive' | null {
    const codeRaw = interaction?.interactionTypeCode ?? interaction?.InteractionTypeCode;
    if (codeRaw !== undefined && codeRaw !== null && String(codeRaw).trim() !== '') {
      const c = String(codeRaw).trim().toLowerCase();
      if (c === 'dependency') return 'dependency';
      if (c === 'incompatible') return 'incompatible';
      if (c === 'exclusive') return 'exclusive';
    }

    const t = interaction?.interactionType ?? interaction?.InteractionType;
    if (typeof t === 'number') {
      if (t === 0) return 'dependency';
      if (t === 1) return 'incompatible';
      if (t === 2) return 'exclusive';
      return null;
    }
    if (typeof t === 'string') {
      const s = t.trim().toLowerCase();
      if (s === 'dependency') return 'dependency';
      if (s === 'incompatible') return 'incompatible';
      if (s === 'exclusive') return 'exclusive';
    }
    return null;
  }

  private isCoverageSelected(item: CoverageItem | undefined): boolean {
    if (!item) return false;
    if (item.isSectionHeader) return false;
    // Main coverage considered always selected
    if (item.coverageType === 0) return true;
    return item.selected === true;
  }

  private canToggleCoverageSelection(item: CoverageItem | undefined): boolean {
    if (!item) return false;
    if (item.isSectionHeader) return false;
    // Main coverage cannot be toggled in UI
    if (item.coverageType === 0) return false;
    // Required checkbox disabled -> cannot toggle
    if (item.isCheckboxDisabled) return false;
    return true;
  }

  private setCoverageSelectedInternal(productId: string, item: CoverageItem, selected: boolean): { changed: boolean; fixedAmountSelected: boolean } {
    if (item.isSectionHeader) return { changed: false, fixedAmountSelected: false };
    if (item.coverageType === 0) {
      // main coverage is always selected
      return { changed: false, fixedAmountSelected: false };
    }
    if (item.isCheckboxDisabled) {
      // required coverages cannot be toggled
      return { changed: false, fixedAmountSelected: false };
    }

    const prev = item.selected === true;
    if (prev === selected) {
      return { changed: false, fixedAmountSelected: false };
    }

    item.selected = selected;
    if (selected) {
      item.quantity = 1;
      if (this.hasLiabilityAmountSelect(item)) {
        item.insuranceAmount = this.getDefaultLiabilityOptionValue(item)!;
        return { changed: true, fixedAmountSelected: true };
      }
      // If fixed liability amount applies, auto-fill amount
      if (item.liabilityAmountFixed !== null && item.liabilityAmountFixed !== undefined) {
        item.insuranceAmount = item.liabilityAmountFixed;
        return { changed: true, fixedAmountSelected: true };
      }
      return { changed: true, fixedAmountSelected: false };
    }

    // Deselected: clear amount and premium values
    item.quantity = 0;
    item.insuranceAmount = null;
    item.premium = 0;
    item.vat = 0;
    item.premiumWithVAT = 0;
    return { changed: true, fixedAmountSelected: false };
  }

  private applyProductCoverageInteractionRules(productId: string, changedItem?: CoverageItem): CoverageItem | null {
    if (!productId) return null;

    const issueDate = this.getIssueDateForCoverageLevelRules();
    const items = (this.productCoverages[productId] || []).filter(x => !x.isSectionHeader);

    const byId = new Map<string, CoverageItem>();
    for (const it of items) {
      if (it.productCoverageId) {
        byId.set(String(it.productCoverageId), it);
      }
    }

    // Build adjacency maps (date-filtered)
    const depAdj = new Map<string, Set<string>>();
    const incAdj = new Map<string, Set<string>>();
    const excAdj = new Map<string, Set<string>>();

    const addEdge = (map: Map<string, Set<string>>, a: string, b: string) => {
      if (!map.has(a)) map.set(a, new Set<string>());
      map.get(a)!.add(b);
    };

    for (const it of items) {
      const ints = it.productCoverageInteractions || [];
      for (const raw of ints) {
        if (!raw) continue;
        if (!this.isIssueDateInLevelRange(issueDate, raw)) continue;

        const type = this.normalizeInteractionTypeCode(raw);
        if (!type) continue;

        const a = (raw as any).productCoverageId || it.productCoverageId;
        const b = (raw as any).interactionCoverageId;
        if (!a || !b) continue;

        const aId = String(a);
        const bId = String(b);
        if (!byId.has(aId) || !byId.has(bId)) continue;

        if (type === 'dependency') {
          addEdge(depAdj, aId, bId);
          addEdge(depAdj, bId, aId);
        } else if (type === 'incompatible') {
          addEdge(incAdj, aId, bId);
          addEdge(incAdj, bId, aId);
        } else if (type === 'exclusive') {
          addEdge(excAdj, aId, bId);
          addEdge(excAdj, bId, aId);
        }
      }
    }

    // Build dependency components (Union-Find)
    const parent = new Map<string, string>();
    const find = (x: string): string => {
      const p = parent.get(x);
      if (!p || p === x) {
        parent.set(x, x);
        return x;
      }
      const root = find(p);
      parent.set(x, root);
      return root;
    };
    const union = (a: string, b: string) => {
      const ra = find(a);
      const rb = find(b);
      if (ra !== rb) parent.set(ra, rb);
    };
    for (const [a, ns] of depAdj) {
      for (const b of ns) union(a, b);
    }
    const depGroups = new Map<string, string[]>();
    for (const id of byId.keys()) {
      const r = find(id);
      if (!depGroups.has(r)) depGroups.set(r, []);
      depGroups.get(r)!.push(id);
    }
    const depGroupById = new Map<string, string[]>();
    for (const ids of depGroups.values()) {
      for (const id of ids) depGroupById.set(id, ids);
    }

    // Queue-based propagation
    type QItem = { id: string; desired: boolean };
    const q: QItem[] = [];
    let pricingCandidate: CoverageItem | null = null;

    const enqueue = (id: string, desired: boolean) => {
      q.push({ id, desired });
    };

    // Chỉ áp dụng khi user click coverage (không seed exclusive khi load)
    if (changedItem?.productCoverageId) {
      enqueue(String(changedItem.productCoverageId), this.isCoverageSelected(changedItem));
    }

    const seen = new Set<string>();
    while (q.length > 0) {
      const cur = q.shift()!;
      const key = `${cur.id}:${cur.desired ? '1' : '0'}`;
      if (seen.has(key)) continue;
      seen.add(key);

      const item = byId.get(cur.id);
      if (!item) continue;

      // 1) Dependency: toggle whole group to same desired state
      const group = depGroupById.get(cur.id) || [cur.id];
      let desired = cur.desired;
      // If any member is not toggleable (main/required) and is selected -> cannot set group false
      if (!desired) {
        for (const gid of group) {
          const gi = byId.get(gid);
          if (!gi) continue;
          const lockedSelected = (gi.coverageType === 0) || (gi.isCheckboxDisabled === true);
          if (lockedSelected && this.isCoverageSelected(gi)) {
            desired = true;
            break;
          }
        }
      }
      for (const gid of group) {
        const gi = byId.get(gid);
        if (!gi) continue;
        const r = this.setCoverageSelectedInternal(productId, gi, desired);
        if (r.changed) {
          enqueue(gid, desired);
          if (r.fixedAmountSelected) pricingCandidate = pricingCandidate || gi;
        }
      }

      // 2) Incompatible: cannot both selected
      if (desired) {
        const neighbors = incAdj.get(cur.id);
        if (neighbors) {
          for (const nid of neighbors) {
            const other = byId.get(nid);
            if (!other) continue;
            if (!this.isCoverageSelected(other)) continue;

            // Try to deselect the other; if not possible, deselect current
            if (this.canToggleCoverageSelection(other)) {
              const r = this.setCoverageSelectedInternal(productId, other, false);
              if (r.changed) enqueue(nid, false);
            } else if (this.canToggleCoverageSelection(item)) {
              const r = this.setCoverageSelectedInternal(productId, item, false);
              if (r.changed) enqueue(cur.id, false);
              this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpUi::Error'),
                detail: this.localizationService.localize('Policy::Policy:CoverageIncompatibleCannotSelectTogether')
              });
            }
          }
        }
      }

      // 3) Exclusive: chỉ được chọn một trong cặp; khi tick coverage mà đối tác loại trừ đang chọn → báo lỗi, không cho chọn. Bỏ chọn thì để trống, không ép chọn bên kia.
      if (desired) {
        const excNeighbors = excAdj.get(cur.id);
        if (excNeighbors) {
          for (const nid of excNeighbors) {
            const other = byId.get(nid);
            if (!other) continue;
            if (this.isCoverageSelected(other)) {
              if (this.canToggleCoverageSelection(item)) {
                const r = this.setCoverageSelectedInternal(productId, item, false);
                if (r.changed) enqueue(cur.id, false);
                this.messageService.add({
                  severity: 'error',
                  summary: this.localizationService.localize('AbpUi::Error'),
                  detail: this.localizationService.localize('Policy::Policy:CoverageExclusiveOnlyOne')
                });
              }
              break;
            }
          }
        }
      }
    }

    return pricingCandidate;
  }

  /**
   * Handle coverage checkbox change - update quantity based on checkbox state.
   * Apply interaction rules (exclusive/incompatible/dependency) first so violating selection is reverted before UI/amount updates.
   */
  onCoverageCheckboxChange(productId: string, item: CoverageItem): void {
    // 1) Apply interaction rules first: may revert item.selected (e.g. exclusive → cannot tick when partner already selected)
    const pricingCandidate = this.applyProductCoverageInteractionRules(productId, item);

    // 2) Update quantity/amount from current state (after rules may have reverted the click)
    if (item.coverageType !== 0) {
      if (item.selected) {
        item.quantity = 1;
        if (item.liabilityAmountFixed !== null && item.liabilityAmountFixed !== undefined) {
          item.insuranceAmount = item.liabilityAmountFixed;
        } else if (this.hasLiabilityAmountSelect(item)) {
          const mainAmount = this.getMainCoverageAmount(productId);
          if (mainAmount !== null) {
            item.insuranceAmount = this.snapInsuranceAmountToNearestLiabilityOption(item, mainAmount);
          } else {
            item.insuranceAmount = this.getDefaultLiabilityOptionValue(item)!;
          }
        } else if (productId && this.isVcxProduct(productId)) {
          const mainAmount = this.getMainCoverageAmount(productId);
          if (mainAmount !== null) {
            item.insuranceAmount = mainAmount;
          }
        }
      } else {
        item.quantity = 0;
        item.insuranceAmount = null;
        item.premium = 0;
        item.vat = 0;
        item.premiumWithVAT = 0;
      }
    }

    if (pricingCandidate && productId) {
      this.onInsuranceAmountChange(productId, pricingCandidate);
    }
    if (productId) {
      this.syncVcxNonMainCoverageAmounts(productId);
      this.scheduleProductPremiumRecalc(productId);
    }
    this.updateSummary();

    // 3) Next tick: force p-checkbox to sync with model (reverted state); same-tick detectChanges often not enough
    const finalSelected = item.selected;
    setTimeout(() => {
      item.selected = !finalSelected;
      this.cdr.detectChanges();
      item.selected = finalSelected;
      this.cdr.detectChanges();
    }, 0);
  }

  onCoverageQuantityChange(productId: string, item: CoverageItem): void {
    if (!productId || !item || item.isSectionHeader) return;

    // For non-main coverages, only react when selected
    if (item.coverageType !== 0 && !item.selected) return;

    // Only applies if quantity is enabled
    if (((item.enableQuantity || '').toUpperCase() !== 'Y')) return;

    this.scheduleProductPremiumRecalc(productId);
  }

  /**
   * Handle insurance amount change - calculate premium for the product
   */
  /**
   * When Mục đích kinh doanh = KDVT, default Xe biển vàng (CAR_PLATE_TYPE code = YELLOW).
   */
  onCarUsageChange(carUsage: string | null): void {
    const v = (carUsage || '').trim();
    if (v === 'KDVT') {
      const yellow = this.carPlateTypeOptions.find(
        o => (o.value || '').toUpperCase() === 'YELLOW'
      );
      this.formData.vehiclePlateType = yellow ? yellow.value : 'YELLOW';
    }
  }

  /**
   * Recalculate premiums for all selected products based on current Insured Object data
   */
  /** Chuẩn hóa biển số xe: chỉ giữ số và chữ in hoa, loại bỏ ký tự đặc biệt */
  protected readonly normalizeVehiclePlate = normalizeVehiclePlate;

  onInsuredObjectChange(): void {
    // Khi Nhóm xe, Loại xe, Số chỗ ngồi, Trọng tải, Hãng xe, Hiệu xe... thay đổi -> gọi lại API mức miễn thường và tính lại phí.
    // Không ghi đè main coverage = carValue ở đây (chỉ khi user đổi "Giá trị xe" hoặc bật sản phẩm VCX mới gán).
    this.deductibleLoadedForProductIds.clear();
    const selectedProductIds = this.getSelectedProductIds();
    this.ensureDeductibleOptionsForProducts(selectedProductIds);

    selectedProductIds.forEach(productId => {
      this.syncVcxNonMainCoverageAmounts(productId);
      this.scheduleProductPremiumRecalc(productId);
    });
  }

  /** Gọi khi user thay đổi "Giá trị xe" (carValue): VCX main coverage = carValue và sync non-main. */
  onCarValueChange(): void {
    const selectedProductIds = this.getSelectedProductIds();
    selectedProductIds.forEach(productId => {
      this.applyCarValueToMainCoverageIfVcx(productId);
      this.syncVcxNonMainCoverageAmounts(productId);
      this.scheduleProductPremiumRecalc(productId);
    });
  }

  /**
   * Handle insurance amount change - calculate premium for the product
   */
  /** Khi đổi mức miễn thường -> gọi lại API tính phí (giống đổi đối tượng bảo hiểm). */
  onDeductibleChange(productId: string): void {
    this.scheduleProductPremiumRecalc(productId);
  }

  onInsuranceAmountChange(productId: string, item: CoverageItem): void {
    // For non-main coverages, only allow calculation if checkbox is selected
    if (item.coverageType !== 0 && !item.selected) {
      // Clear amount and premium values if checkbox is not selected
      item.insuranceAmount = null;
      item.premium = 0;
      item.vat = 0;
      item.premiumWithVAT = 0;
      this.updateSummary();
      return;
    }

    // Enforce LIABILITY_AMOUNT constraints (if present)
    if (this.hasLiabilityAmountSelect(item)) {
      this.normalizeInsuranceAmountToLiabilityOptions(item);
    } else if (item.liabilityAmountFixed !== null && item.liabilityAmountFixed !== undefined) {
      item.insuranceAmount = item.liabilityAmountFixed;
    } else if (
      item.insuranceAmount !== null &&
      item.insuranceAmount !== undefined &&
      item.liabilityAmountMin !== null &&
      item.liabilityAmountMin !== undefined &&
      item.liabilityAmountMax !== null &&
      item.liabilityAmountMax !== undefined
    ) {
      if (item.insuranceAmount < item.liabilityAmountMin) {
        item.insuranceAmount = item.liabilityAmountMin;
      } else if (item.insuranceAmount > item.liabilityAmountMax) {
        item.insuranceAmount = item.liabilityAmountMax;
      }
    }

    // Reset premium values if amount is cleared
    if (!item.insuranceAmount || item.insuranceAmount <= 0) {
      item.premium = 0;
      item.vat = 0;
      item.premiumWithVAT = 0;
      this.updateSummary();
    }

    // VCX: if main coverage amount changes, force all selected non-main amounts to match
    if (productId && item.coverageType === 0) {
      this.syncVcxNonMainCoverageAmounts(productId);
    }

    // Trigger calculation for the whole product
    this.calculateProductPremium(productId);
  }

  /**
   * Calculate premium for a specific product based on current form data and coverage inputs
   */
  calculateProductPremium(productId: string): void {
    if (!productId) {
      return;
    }

    const calcSeq = this.beginProductPremiumCalc(productId);

    // Find the product to get its code
    const product = this.products.find(p => p.id === productId);
    if (!product || !product.code) {
      this.finishProductPremiumCalc(productId, calcSeq);
      return;
    }

    // VCX: sync non-main coverage amounts from main (do NOT overwrite main with carValue here –
    // use current main.insuranceAmount so manual changes to số tiền bảo hiểm are respected when recalculating premium)
    if (this.isVcxProductCode(product)) {
      this.syncVcxNonMainCoverageAmounts(productId);
    }

    // Clear previous fee data before recalculation (avoid stale UI)
    this.clearProductFeeData(productId);
    this.updateSummary();

    const mainItem = this.getMainCoverageItem(productId);
    const coverageItems = (this.productCoverages[productId] || [])
      .filter(c => {
        // Exclude section headers
        if (c.isSectionHeader) return false;

        // Always include the main coverage item (even if not selected/amount missing)
        if (mainItem?.productCoverageId && c.productCoverageId === mainItem.productCoverageId) {
          // ok
        } else {
          // Non-main coverages: Must be selected
          if (!c.selected) return false;
        }

        // Must have required IDs
        if (!c.productCoverageId || !c.coverageId) return false;

        return true;
      });

    const attributes = this.getAttributesMap();
    // API expects deductible value (fromAmount) for rate-line condition matching; resolve from deductibleTermByValue when selection is term id.
    const itemWithDeductible = mainItem?.deductible != null && mainItem.deductible !== ''
      ? mainItem
      : coverageItems.find(c => c.deductible != null && c.deductible !== '');
    const deductibleValueForAttributes = itemWithDeductible ? this.getDeductibleValueForPriceApi(itemWithDeductible) : undefined;
    if (deductibleValueForAttributes != null && deductibleValueForAttributes !== '') {
      attributes['deductible'] = deductibleValueForAttributes;
    }

    const productCoverages = coverageItems.map(c => ({
      productCoverageId: c.productCoverageId!,
      coverageId: c.coverageId!,
      coverageCode: c.coverageCode || undefined,
      amountLiability: c.insuranceAmount || 0,
      quantity: c.enableQuantity === 'Y' ? (c.quantity ?? 1) : 1,
      coverageType: c.coverageType ?? 0, // Include coverageType for mock calculation
      deductible: this.getDeductibleValueForPriceApi(c) ?? c.deductible ?? undefined
    }));

    // If no coverages with amounts, return
    if (productCoverages.length === 0) {
      this.finishProductPremiumCalc(productId, calcSeq);
      return;
    }

    const url = `/api/product/price/${encodeURIComponent(product.code)}`;
    const productAttributeDefs = this.buildProductAttributeDefinitionsForExtract(product);
    const motorPayload = this.buildRiskMotorPayload();

    // Provide a single scalar amountLiability at root for attribute definitions that reference it (dataPath: "amountLiability").
    // Here we use the sum across selected coverages for this product.
    const amountLiability = productCoverages.reduce((sum, pc) => sum + (pc.amountLiability || 0), 0);

    const extractReqBody: ExtractPolicyAttributeParametersInputDto = {
      amountLiability,
      riskObject: {
        riskObjectMotor: motorPayload as any
      } as any,
      product: {
        productId: productId,
        attributes: productAttributeDefs as any
      } as any
    };

    const callGetPrice = (attrsToUse: Record<string, any>) => {
      const eff = this.ensureDate(this.formData.insurancePeriodFrom as any);
      const exp = this.ensureDate(this.formData.insurancePeriodTo as any);
      const body = {
        // Keep request key as productId, but send product code as the value (backend supports code or guid).
        productId: product.code,
        // IMPORTANT: do NOT use toISOString() here (local -> UTC) or you'll see -7h in VN
        effectiveDate: this.toLocalIsoDateTime(eff) || undefined,
        expireDate: this.toLocalIsoDateTime(exp) || undefined,
        attributes: attrsToUse,
        productCoverages: productCoverages
      };

      this.restService.request<any, any>({
        method: 'POST',
        url,
        body
      }).subscribe({
        next: (response) => {
          if (response && response.productCoverages && Array.isArray(response.productCoverages)) {
            // Create a map of productCoverageId -> premium response
            const premiumMap = new Map<string, any>();
            response.productCoverages.forEach((pc: any) => {
              if (pc.productCoverageId) {
                premiumMap.set(pc.productCoverageId, pc);
              }
            });

            // Update premium values for all coverages in this product
            const coverages = this.productCoverages[productId] || [];
            coverages.forEach(coverage => {
              if (!coverage.isSectionHeader && coverage.productCoverageId) {
                const premiumData = premiumMap.get(coverage.productCoverageId);
                if (premiumData) {
                  const premium = premiumData.premium ?? 0;
                  const premiumVat = premiumData.premiumVat; // may be null
                  coverage.premium = premium;
                  coverage.vat = premiumVat !== null && premiumVat !== undefined ? (premiumVat - premium) : 0;
                  coverage.premiumWithVAT = premiumVat ?? premium;
                  // Tăng/Giảm phí = premiumVat - premiumVat snapshot (not premium - premiumOrigin)
                  const currentPremiumVat = (coverage.premium ?? 0) + (coverage.vat ?? 0);
                  const originPremiumVat = (coverage.premiumOrigin ?? 0) + (coverage.vatOrigin ?? 0);
                  coverage.premiumChange = currentPremiumVat - originPremiumVat;

                  // Update rate information if available
                  if (premiumData.rate) {
                    // Phí BH (premiumRate): prefer baseRate; if null then flatRate (keep existing if both null)
                    const baseRate = premiumData.rate.baseRate;
                    const flatRate = premiumData.rate.flatRate;
                    const chosenRate = baseRate ?? flatRate ?? null;
                    if (chosenRate !== null && chosenRate !== undefined) {
                      coverage.premiumRate = chosenRate;
                    }

                    // Persist metadata for create/update policy
                    coverage.baseRate = baseRate ?? null;
                    coverage.flatRate = flatRate ?? null;
                    // MatchedRateDto.RateId -> JSON: rateId
                    if (premiumData.rate.rateId) {
                      coverage.tableRateLineId = premiumData.rate.rateId;
                    }

                    // Tax rate: keep existing if null/undefined (allow 0)
                    const taxValue = premiumData.rate.taxValue;
                    if (taxValue !== null && taxValue !== undefined) {
                      coverage.taxRate = taxValue;
                    }
                  }
                } else {
                  // If no premium data found, reset to 0
                  coverage.premium = 0;
                  coverage.vat = 0;
                  coverage.premiumWithVAT = 0;
                  const originPremiumVat = (coverage.premiumOrigin ?? 0) + (coverage.vatOrigin ?? 0);
                  coverage.premiumChange = 0 - originPremiumVat;
                }
              }
            });

            // Update summary after premium calculation
            this.updateSummary();
          }
          this.finishProductPremiumCalc(productId, calcSeq);
        },
        error: () => {
          // On error, reset premium values
          const coverages = this.productCoverages[productId] || [];
          coverages.forEach(coverage => {
            if (!coverage.isSectionHeader) {
              coverage.premium = 0;
              coverage.vat = 0;
              coverage.premiumWithVAT = 0;
              const originPremiumVat = (coverage.premiumOrigin ?? 0) + (coverage.vatOrigin ?? 0);
              coverage.premiumChange = 0 - originPremiumVat;
            }
          });
          this.updateSummary();
          this.finishProductPremiumCalc(productId, calcSeq);
        }
      });
    };

    // 1) Extract parameter attributes from riskObject using product attribute definitions
    // 2) Merge with current UI-built attributes (fallback) and call get price
    if (productAttributeDefs.length > 0) {
      this.policyService.extractAttributeParameters(extractReqBody).subscribe({
        next: (respList) => {
          const first = Array.isArray(respList) && respList.length > 0 ? respList[0] : {};
          const extracted = (first && typeof first === 'object') ? first : {};
          const merged = { ...attributes, ...(extracted as any) };

          // Ensure carYear is present for pricing if tables expect it.
          // If extract didn't provide it, keep the computed value from attributes.
          if (merged['carYear'] === undefined && attributes['carYear'] !== undefined) {
            merged['carYear'] = attributes['carYear'];
          }

          callGetPrice(merged);
        },
        error: () => {
          // Fallback to current formData-built attributes
          callGetPrice(attributes);
        }
      });
    } else {
      // No attribute definitions available; fallback to current formData-built attributes
      callGetPrice(attributes);
    }
  }

  private buildProductAttributeDefinitionsForExtract(product: any): any[] {
    const productAttributes = Array.isArray(product?.productAttributes) ? product.productAttributes : [];

    const defs = productAttributes
      .map((pa: any) => {
        // Old/new shapes supported:
        // - pa.attribute (nested ProAttributeDto)
        // - pa.proAttribute (legacy nested)
        // - flattened fields: attributeCode, attributeName, attributeDataPath, ...
        const nested = pa?.attribute || pa?.proAttribute || null;
        const code = (nested?.code ?? pa?.attributeCode ?? pa?.code ?? '').toString().trim() || undefined;
        const name = (nested?.name ?? pa?.attributeName ?? pa?.name ?? undefined) as any;
        const status = (nested?.status ?? pa?.attributeStatus ?? pa?.status ?? undefined) as any;
        const spec = (nested?.spec ?? pa?.attributeSpec ?? pa?.spec ?? undefined) as any;
        const description = (nested?.description ?? pa?.attributeDescription ?? pa?.description ?? undefined) as any;
        const dataPath = (nested?.dataPath ?? pa?.attributeDataPath ?? pa?.dataPath ?? undefined) as any;
        const dataType = (nested?.dataType ?? pa?.attributeDataType ?? pa?.dataType ?? undefined) as any;
        const computeScript = (nested?.computeScript ?? pa?.attributeComputeScript ?? pa?.computeScript ?? undefined) as any;
        const clearDataScript = (nested?.clearDataScript ?? pa?.attributeClearDataScript ?? pa?.clearDataScript ?? undefined) as any;

        // Some attributes are "computed-only" (no dataPath) so we must not drop them.
        const hasAny =
          !!code ||
          !!name ||
          !!dataPath ||
          dataType !== undefined ||
          !!computeScript ||
          !!clearDataScript;

        if (hasAny) {
          return {
            code,
            name,
            status,
            spec,
            description,
            dataPath,
            dataType,
            computeScript,
            clearDataScript
          };
        }

        return null;
      })
      .filter((x: any) => {
        // Backend extraction uses `code` as the output key; without it the entry is not useful.
        if (!x) return false;
        if (typeof x.code === 'string' && x.code.trim().length > 0) return true;
        return false;
      });

    return defs;
  }

  /**
   * Calculate totals for a specific product
   */
  /**
   * Calculate totals for a specific product
   */
  getProductTotals(productId: string): { premium: number; vat: number; premiumWithVAT: number } {
    const coverages = this.productCoverages[productId] || [];
    let totalPremium = 0;
    let totalVAT = 0;
    let totalPremiumWithVAT = 0;

    coverages.forEach(item => {
      // Only sum non-section header items
      // For non-main coverages, only include if selected
      if (!item.isSectionHeader && (item.coverageType === 0 || item.selected)) {
        totalPremium += item.premium || 0;
        totalVAT += item.vat || 0;
        totalPremiumWithVAT += item.premiumWithVAT || 0;
      }
    });

    return {
      premium: totalPremium,
      vat: totalVAT,
      premiumWithVAT: totalPremiumWithVAT
    };
  }

  /**
   * Khớp `HrEmployee.userId` với `currentUser.id` (ABP/Guid có thể khác kiểu hoặc casing) — tránh mất NV → fallback DIRECT.
   */
  private findEmployeeRowForLoggedInUser():
    | (typeof this.employeeData)[number]
    | undefined {
    const currentUser = this.configState.getOne('currentUser');
    if (!currentUser?.id) {
      return undefined;
    }
    const uid = (currentUser.id ?? '').toString().trim().toLowerCase();
    return this.employeeData.find(
      e => (e.userId ?? '').toString().trim().toLowerCase() === uid
    );
  }

  /**
   * Set default values for implementer and seller based on current user's HrEmployee
   */
  private setDefaultEmployeeValues(): void {
    // Only set defaults in create mode
    if (this.dialogMode !== 'create') {
      return;
    }

    const currentUser = this.configState.getOne('currentUser');
    if (!currentUser || !currentUser.id) {
      return;
    }

    const employeeForCurrentUser = this.findEmployeeRowForLoggedInUser();

    if (employeeForCurrentUser && employeeForCurrentUser.id) {
      // Set default values only if fields are empty
      if (!this.formData.implementerId || this.formData.implementerId === '') {
        this.formData.implementerId = employeeForCurrentUser.id;
      }
      if (!this.formData.sellerId || this.formData.sellerId === '') {
        this.formData.sellerId = employeeForCurrentUser.id;
      }
    }
  }

  /**
   * Khi đã chọn kênh:
   * - Kênh trực tiếp (DIRECT): chỉ NV không gắn đối tác.
   * - Kênh khác: chỉ NV có đối tác thuộc đúng kênh (partnerChannelId === channelId).
   * Chưa chọn kênh: full danh sách.
   */
  applySellerOptionsFilterForChannel(): void {
    const all = this.sellerOptionsAll;
    if (!all.length) {
      this.sellerOptions = [];
      return;
    }

    const ch = (this.formData.channelId ?? '').toString().trim();
    if (!ch) {
      this.sellerOptions = [...all];
      return;
    }

    const directIdTrim = (this.directChannelId ?? '').toString().trim();
    const selectedChannel = this.channelOptions.find(o => (o.value ?? '').toString().trim() === ch);
    const codeFromOpt = (selectedChannel?.code ?? '').trim().toUpperCase();
    const isDirectChannel = codeFromOpt === 'DIRECT' || (!!directIdTrim && ch === directIdTrim);

    const currentUser = this.configState.getOne('currentUser');
    const currentUserEmpId = (() => {
      const uid = (currentUser?.id ?? '').toString().trim().toLowerCase();
      if (!uid) return '';
      const row = this.employeeData.find(
        e => (e.userId ?? '').toString().trim().toLowerCase() === uid
      );
      return (row?.id ?? '').toString().trim();
    })();

    const allowedIds = new Set(
      this.employeeData
        .filter((emp) => {
          const pid = (emp.partnerId ?? '').toString().trim();
          if (isDirectChannel) {
            // Kênh trực tiếp: NV không đối tác, hoặc luôn cho phép NV đang đăng nhập (có thể vẫn gắn partner).
            if (!pid) {
              return true;
            }
            return !!currentUserEmpId && emp.id === currentUserEmpId;
          }
          if (!pid) {
            return false;
          }
          const pc = (emp.partnerChannelId ?? '').toString().trim();
          return pc === ch;
        })
        .map((e) => e.id)
    );

    const filtered = all.filter((o) => allowedIds.has(o.value));
    this.sellerOptions = filtered;

    const sid = (this.formData.sellerId ?? '').toString().trim();
    if (sid && !this.sellerOptions.some((o) => o.value === sid)) {
      this.formData.sellerId = '';
    }
    this.ensureDefaultSellerForDirectChannel();
    this.cdr.markForCheck();
  }

  /**
   * Form cấp đơn (create): nếu kênh là DIRECT và chưa có người khai thác, mặc định = nhân viên của user đăng nhập.
   */
  private ensureDefaultSellerForDirectChannel(): void {
    if (this.dialogMode !== 'create') {
      return;
    }
    if (!this.isDirectChannelSelected()) {
      return;
    }
    const currentUser = this.configState.getOne('currentUser');
    if (!currentUser?.id) {
      return;
    }
    const uid = (currentUser.id ?? '').toString().trim().toLowerCase();
    const emp = this.employeeData.find(
      e => (e.userId ?? '').toString().trim().toLowerCase() === uid
    );
    if (!emp?.id) {
      return;
    }
    const sid = (this.formData.sellerId ?? '').toString().trim();
    if (sid) {
      return;
    }
    this.formData.sellerId = emp.id;
  }

  /** Đổi kênh khai thác trên form đơn: tải lại sản phẩm + lọc người khai thác theo kênh. */
  onFormChannelChanged(value: string | null | undefined): void {
    this.channelLockedBySeller = false;
    const trimmed = (value ?? '').toString().trim();
    this.userClearedFormChannel = !trimmed;
    this.loadProducts();
    this.applySellerOptionsFilterForChannel();
  }

  /** Đổi người cấp đơn: cập nhật danh sách người khai thác theo kênh hiện tại. */
  onFormImplementerChangedForSellerSync(_value: string | null | undefined): void {
    this.applySellerOptionsFilterForChannel();
  }

  /**
   * When user changes seller (người khai thác), auto-fill channel from Employee → Partner → Channel.
   * If employee has no partner, partner has no channelId, or API fails, set channel to DIRECT (code DIRECT).
   */
  fillChannelFromEmployee(employeeId: string | null | undefined, onSettled?: () => void): void {
    const isDirect = this.isDirectChannelSelected();
    if (isDirect) {
      this.channelLockedBySeller = false;
    }

    const id = (employeeId ?? '').toString().trim();
    if (!id) {
      if (isDirect) {
        this.setChannelToDirectIfAvailable();
      }
      this.channelLockedBySeller = false;
      this.applySellerOptionsFilterForChannel();
      this.cdr.markForCheck();
      onSettled?.();
      return;
    }

    const emp = this.employeeData.find(e => e.id === id);
    const partnerId = emp?.partnerId?.trim();
    if (!partnerId) {
      if (isDirect) {
        this.setChannelToDirectIfAvailable();
      }
      this.channelLockedBySeller = false;
      this.applySellerOptionsFilterForChannel();
      this.cdr.markForCheck();
      onSettled?.();
      return;
    }

    this.partnerService.get(partnerId).subscribe({
      next: (partner) => {
        const rawCh = partner?.channelId as string | null | undefined;
        const channelId =
          rawCh != null && String(rawCh).trim() !== '' ? String(rawCh).trim() : '';
        if (channelId) {
          this.formData.channelId = channelId;
          this.userClearedFormChannel = false;
          this.syncChannelLockForResolvedFormChannel();
        } else {
          if (isDirect) {
            this.setChannelToDirectIfAvailable();
          }
          this.syncChannelLockForResolvedFormChannel();
        }
        this.applySellerOptionsFilterForChannel();
        this.cdr.markForCheck();
        onSettled?.();
      },
      error: () => {
        if (isDirect) {
          this.setChannelToDirectIfAvailable();
        }
        this.syncChannelLockForResolvedFormChannel();
        this.applySellerOptionsFilterForChannel();
        this.cdr.markForCheck();
        onSettled?.();
      }
    });
  }

  /** Set formData.channelId to DIRECT channel (code DIRECT) when available. */
  private setChannelToDirectIfAvailable(): void {
    if (this.directChannelId) {
      this.formData.channelId = this.directChannelId;
    }
  }

  /**
   * Sau khi kênh đã được gán (partner / nhân viên): khóa dropdown kênh nếu không phải DIRECT; DIRECT thì mở khóa.
   */
  private syncChannelLockForResolvedFormChannel(): void {
    const ch = (this.formData.channelId ?? '').toString().trim();
    if (!ch) {
      this.channelLockedBySeller = false;
      return;
    }
    this.channelLockedBySeller = !this.isDirectChannelSelected();
  }

  /**
   * Auto-fill channel from current user: HrEmployee (userId) → ResPartner (partnerId) → channelId.
   * If partner is missing, has no channel, or API fails, fall back to DIRECT when present in master data.
   * In create mode only; in edit/view always keep the value from policy detail (never overwrite).
   */
  private setDefaultChannelFromCurrentUser(): void {
    // In update/view mode always prefer policy detail value; never apply auto-fill
    if (this.dialogMode === 'edit' || this.dialogMode === 'view') {
      return;
    }

    const currentUser = this.configState.getOne('currentUser');
    if (!currentUser?.id) return;

    // Only auto-fill when channel is empty (create mode)
    if (this.formData.channelId?.trim()) return;

    const employeeForCurrentUser = this.findEmployeeRowForLoggedInUser();
    const partnerId = employeeForCurrentUser?.partnerId?.trim();
    if (!partnerId) {
      this.setChannelToDirectIfAvailable();
      this.applySellerOptionsFilterForChannel();
      this.syncChannelLockForResolvedFormChannel();
      this.cdr.markForCheck();
      this.maybeLoadProductsAfterDefaultUserChannel();
      return;
    }

    this.partnerService.get(partnerId).subscribe({
      next: (partner) => {
        const rawCh = partner?.channelId as string | null | undefined;
        const channelId =
          rawCh != null && String(rawCh).trim() !== '' ? String(rawCh).trim() : '';
        if (channelId && (!this.formData.channelId || !this.formData.channelId.trim())) {
          this.formData.channelId = channelId;
          this.userClearedFormChannel = false;
        } else if (!this.formData.channelId?.trim()) {
          this.setChannelToDirectIfAvailable();
        }
        this.applySellerOptionsFilterForChannel();
        this.syncChannelLockForResolvedFormChannel();
        this.cdr.markForCheck();
        this.maybeLoadProductsAfterDefaultUserChannel();
      },
      error: () => {
        this.setChannelToDirectIfAvailable();
        this.applySellerOptionsFilterForChannel();
        this.syncChannelLockForResolvedFormChannel();
        this.cdr.markForCheck();
        this.maybeLoadProductsAfterDefaultUserChannel();
      }
    });
  }

  /** After resolving channel from current user's partner (or DIRECT fallback), refresh products if LOB + insurer are set. */
  private maybeLoadProductsAfterDefaultUserChannel(): void {
    if (this.formData.lobId?.trim() && this.formData.primaryInsurancePartnerId?.trim()) {
      this.loadProducts();
    }
  }


  /**
   * Format date for API (YYYY-MM-DD)
   */
  private formatDateForApi(date: Date): string {
    if (!date) {
      return '';
    }
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  /**
   * Format payment status for display (policy_amount.payment_status: new, paid, partial, cancelled)
   */
  formatPaymentStatus(status: string | undefined | null): string {
    if (!status) return '-';
    const s = (status || '').toLowerCase();
    if (s === 'new') return this.localizationService.localize('Policy::Policy:PaymentStatusNew', 'Chưa thanh toán');
    if (s === 'inprogress' || s === 'partial') return this.localizationService.localize('Policy::Policy:PaymentStatusInProgress', 'Thanh toán 1 phần');
    if (s === 'done' || s === 'paid') return this.localizationService.localize('Policy::Policy:PaymentStatusDone', 'Đã thanh toán toàn bộ');
    if (s === 'cancelled') return this.localizationService.localize('Policy::Policy:PaymentStatusCancelled', 'Đã hủy');
    return status;
  }

  /**
   * Format approval status for display (localized)
   */
  formatApprovalStatus(status: string | undefined | null): string {
    if (status == null || status === '') return '-';
    const s = (status || '').toString().toLowerCase();
    if (s === 'approved') return this.localizationService.localize('Policy::Policy:ApprovalStatusApproved');
    if (s === 'rejected') return this.localizationService.localize('Policy::Policy:ApprovalStatusRejected');
    if (s === 'pending') return this.localizationService.localize('Policy::Policy:ApprovalStatusPendingApproval');
    return status;
  }

  /**
   * Display label for approval column: when terminationStatus is set, show termination-specific labels; otherwise use approvalStatus.
   */
  getApprovalStatusDisplayLabel(terminationStatus: PolicyTerminationStatus | number | string | undefined | null, approvalStatus: string | undefined | null): string {
    const term = this.normalizeTerminationStatus(terminationStatus);
    if (term === PolicyTerminationStatus.Pending) return this.localizationService.localize('Policy::Policy:TerminationStatusPending', 'Chờ duyệt chấm dứt');
    if (term === PolicyTerminationStatus.Approved) return this.localizationService.localize('Policy::Policy:TerminationStatusApproved', 'Đã duyệt chấm dứt');
    if (term === PolicyTerminationStatus.Rejected) return this.localizationService.localize('Policy::Policy:TerminationStatusRejected', 'Từ chối chấm dứt');
    return this.formatApprovalStatus(approvalStatus);
  }

  /**
   * Cell CSS class for approval column: same badge colors for termination states as for approval states.
   */
  getApprovalStatusCellClass(terminationStatus: PolicyTerminationStatus | number | string | undefined | null, approvalStatus: string | undefined | null): string {
    const base = 'px-2 py-1 rounded text-xs font-semibold ';
    const term = this.normalizeTerminationStatus(terminationStatus);
    if (term === PolicyTerminationStatus.Pending) return base + 'bg-amber-100 text-amber-800';
    if (term === PolicyTerminationStatus.Approved) return base + 'bg-green-100 text-green-800';
    if (term === PolicyTerminationStatus.Rejected) return base + 'bg-red-100 text-red-800';
    const s = (approvalStatus ?? '').toString().toLowerCase();
    if (s === 'approved') return base + 'bg-green-100 text-green-800';
    if (s === 'rejected') return base + 'bg-red-100 text-red-800';
    if (s === 'pending') return base + 'bg-amber-100 text-amber-800';
    return base + 'bg-gray-100 text-gray-800';
  }

  private normalizeTerminationStatus(v: PolicyTerminationStatus | number | string | undefined | null): PolicyTerminationStatus | null {
    if (v === undefined || v === null || v === '') return null;
    if (typeof v === 'number' && (v === 0 || v === 1 || v === 2)) return v as PolicyTerminationStatus;
    const s = String(v).trim().toLowerCase();
    if (s === 'pending' || s === '0') return PolicyTerminationStatus.Pending;
    if (s === 'approved' || s === '1') return PolicyTerminationStatus.Approved;
    if (s === 'rejected' || s === '2') return PolicyTerminationStatus.Rejected;
    return null;
  }

  /**
   * Approval status for detail footer: termination-aware (same logic as grid column).
   */
  formatApprovalStatusForDetail(terminationStatus: PolicyTerminationStatus | number | string | undefined | null, approvalStatus: string | undefined | null): string {
    return this.getApprovalStatusDisplayLabel(terminationStatus, approvalStatus);
  }

  /** CSS classes for policy approval status in footer: green approved, red rejected, amber pending, gray other. */
  getPolicyApprovalStatusFooterClass(approvalStatus: string | null | undefined): string {
    const s = (approvalStatus ?? '').toString().trim().toLowerCase();
    if (s === 'approved') return 'px-2 py-0.5 rounded text-sm font-semibold bg-green-100 text-green-800';
    if (s === 'rejected') return 'px-2 py-0.5 rounded text-sm font-semibold bg-red-100 text-red-800';
    if (s === 'pending') return 'px-2 py-0.5 rounded text-sm font-semibold bg-amber-100 text-amber-800';
    return 'px-2 py-0.5 rounded text-sm font-semibold bg-gray-100 text-gray-800';
  }

  /**
   * Format status for display
   */
  formatStatus(status: PolicyStatus | number | string | undefined | null): string {
    if (status === undefined || status === null) {
      return '';
    }

    let statusValue: number;

    if (typeof status === 'number') {
      statusValue = status;
    } else if (typeof status === 'string') {
      statusValue = parseInt(status, 10);
    } else {
      statusValue = status;
    }

    switch (statusValue) {
      case PolicyStatus.Quotation:
        return this.localizationService.localize('Policy::Policy:Quotation');
      case PolicyStatus.Draft:
        return this.localizationService.localize('Policy::Policy:Draft');
      case PolicyStatus.Active:
        return this.localizationService.localize('Policy::Policy:Active');
      case PolicyStatus.Expired:
        return this.localizationService.localize('Policy::Policy:Expired');
      case PolicyStatus.Terminated:
        return this.localizationService.localize('Policy::Policy:Terminated');
      case PolicyStatus.Cancelled:
        return this.localizationService.localize('Policy::Policy:Cancelled');
      default:
        return '';
    }
  }
}
