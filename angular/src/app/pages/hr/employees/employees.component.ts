import { Component, HostListener, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { TreeModule } from 'primeng/tree';
import { TreeNode } from 'primeng/api';
import { TableLazyLoadEvent } from 'primeng/table';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { DialogModule } from 'primeng/dialog';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TextareaModule } from 'primeng/textarea';
import { SplitterModule } from 'primeng/splitter';
import { CheckboxModule } from 'primeng/checkbox';
import { DatePickerModule } from 'primeng/datepicker';
import { TabsModule } from 'primeng/tabs';
import { TableModule } from 'primeng/table';
import { TooltipModule } from 'primeng/tooltip';

import { VTable } from '@/shared/components/v-table/v-table';
import { TableColumn } from '@/shared/models/table-column.model';
import { TableAction } from '@/shared/models/table-action.model';
import { HrEmployeeService } from '@/proxy/hr/controllers/hr-employee.service';
import { HrEmployeeDto, CreateHrEmployeeDto, UpdateHrEmployeeDto, GetHrEmployeesInput, HrEmployeeRoleRelDto, CreateHrEmployeeRoleRelDto, UpdateHrEmployeeRoleRelDto } from '@/proxy/hr/hr-employees/models';
import { HrEmployeeStatus } from '@/proxy/hr-employees/hr-employee-status.enum';
import { EmployeeSearchForm, EmployeeFormData, EmployeeRoleFormData } from './employees.models';
import { PermissionPipe } from '@/core/pipes/permission.pipe';
import { PermissionService } from '@/core/services/permission.service';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';
import { HrDepartmentService } from '@/proxy/hr/controllers/hr-department.service';
import { HrDepartmentTreeDto } from '@/proxy/hr/hr-departments/models';
import { HrDepartmentLevel } from '@/proxy/hr-departments/hr-department-level.enum';
import { HrEmployeePositionService } from '@/proxy/hr/controllers/hr-employee-position.service';
import { HrEmployeeLevelService } from '@/proxy/hr/controllers/hr-employee-level.service';
import { HrEmployeeRoleService } from '@/proxy/hr/controllers/hr-employee-role.service';
import { ResPartnerService } from '@/proxy/partner/controllers/res-partner.service';
import { ResProvinceService } from '@/proxy/master/controllers/res-province.service';
import { ResWardService } from '@/proxy/master/controllers/res-ward.service';
import { IdentityUserService } from '@/proxy/volo/abp/identity/identity-user.service';
import { HrEmployeePositionStatus } from '@/proxy/hr-employee-positions/hr-employee-position-status.enum';
import { HrEmployeeLevelStatus } from '@/proxy/hr-employee-levels/hr-employee-level-status.enum';
import { HrEmployeeRoleStatus } from '@/proxy/hr-employee-roles/hr-employee-role-status.enum';
import { ResPartnerStatus } from '@/proxy/res-partners/res-partner-status.enum';
import { ResProvinceStatus } from '@/proxy/res-provinces/res-province-status.enum';
import { ResWardStatus } from '@/proxy/res-wards/res-ward-status.enum';

@Component({
  selector: 'app-employees',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    PanelModule,
    ButtonModule,
    InputTextModule,
    SelectModule,
    TreeModule,
    ToastModule,
    DialogModule,
    ConfirmDialogModule,
    TextareaModule,
    SplitterModule,
    CheckboxModule,
    DatePickerModule,
    TabsModule,
    TableModule,
    TooltipModule,
    VTable,
    PermissionPipe,
    TranslatePipe
  ],
  templateUrl: './employees.component.html',
  styleUrl: './employees.component.scss',
  providers: [MessageService, ConfirmationService]
})
export class EmployeesComponent implements OnInit {
  // Permissions
  readonly PERMISSIONS = {
    CREATE: 'HrEmployee.Create',
    UPDATE: 'HrEmployee.Edit',
    DELETE: 'HrEmployee.Delete',
    VIEW: 'HrEmployee.View'
  };

  // Data
  employees: HrEmployeeDto[] = [];
  totalCount = 0;
  loading = true;
  pageSize = 10;

  // Tree
  treeNodes: TreeNode[] = [];
  selectedTreeNode: TreeNode | null = null;
  treeLoading = false;
  selectedDepartmentId: string | null = null;

  // Dialog
  dialogVisible = false;
  dialogMode: 'create' | 'edit' = 'create';
  formData: EmployeeFormData = this.getEmptyForm();
  selectedEmployee?: HrEmployeeDto;

  // Role Management Dialog
  roleDialogVisible = false;
  roleDialogMode: 'create' | 'edit' = 'create';
  roleFormData: EmployeeRoleFormData = this.getEmptyRoleForm();
  selectedRoleRel?: HrEmployeeRoleRelDto;
  employeeRoles: HrEmployeeRoleRelDto[] = [];
  currentEmployeeId?: string;

  // Search form
  searchForm: EmployeeSearchForm = {
    code: null,
    fullName: null,
    status: null,
    departmentId: null,
    orgId: null,
    partnerId: null,
    positionId: null,
    levelId: null
  };

  // Options
  statusOptions: Array<{ label: string; value: HrEmployeeStatus }> = [];
  departmentOptions: Array<{ label: string; value: string }> = [];
  orgOptions: Array<{ label: string; value: string }> = [];
  partnerOptions: Array<{ label: string; value: string }> = [];
  positionOptions: Array<{ label: string; value: string }> = [];
  levelOptions: Array<{ label: string; value: string }> = [];
  roleOptions: Array<{ label: string; value: string }> = [];
  managerOptions: Array<{ label: string; value: string }> = [];
  provinceOptions: Array<{ label: string; value: string }> = [];
  wardOptions: Array<{ label: string; value: string }> = [];
  userOptions: Array<{ label: string; value: string }> = [];

  // Table
  columns: TableColumn[] = [];
  actions: TableAction<HrEmployeeDto>[] = [];
  currentLazyLoadEvent?: TableLazyLoadEvent;

  constructor(
    private employeeService: HrEmployeeService,
    private departmentService: HrDepartmentService,
    private positionService: HrEmployeePositionService,
    private levelService: HrEmployeeLevelService,
    private roleService: HrEmployeeRoleService,
    private partnerService: ResPartnerService,
    private provinceService: ResProvinceService,
    private wardService: ResWardService,
    private userService: IdentityUserService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private permissionService: PermissionService,
    private localizationService: LocalizationService
  ) {
    this.initializeColumns();
    this.initializeActions();
    this.initializeOptions();
  }

  ngOnInit(): void {
    this.loadTree();
  }

  /**
   * Initialize options with localized labels
   */
  private initializeOptions(): void {
    this.statusOptions = [
      { label: this.localizationService.localize('Hr::HrEmployee:Active'), value: HrEmployeeStatus.Active },
      { label: this.localizationService.localize('Hr::HrEmployee:Deactive'), value: HrEmployeeStatus.Deactive }
    ];
  }

  /**
   * Initialize table columns with localized headers
   */
  private initializeColumns(): void {
    this.columns = [
      {
        field: 'code',
        header: this.localizationService.localize('Hr::HrEmployee:Code'),
        sortable: true,
        width: '120px'
      },
      {
        field: 'fullName',
        header: this.localizationService.localize('Hr::HrEmployee:FullName'),
        sortable: true,
        width: '200px'
      },
      {
        field: 'departmentName',
        header: this.localizationService.localize('Hr::HrEmployee:DepartmentName'),
        sortable: false,
        width: '200px'
      },
      {
        field: 'orgName',
        header: this.localizationService.localize('Hr::HrEmployee:OrgName'),
        sortable: false,
        width: '150px'
      },
      {
        field: 'positionName',
        header: this.localizationService.localize('Hr::HrEmployee:PositionName'),
        sortable: false,
        width: '150px'
      },
      {
        field: 'levelName',
        header: this.localizationService.localize('Hr::HrEmployee:LevelName'),
        sortable: false,
        width: '150px'
      },
      {
        field: 'phone',
        header: this.localizationService.localize('Hr::HrEmployee:Phone'),
        sortable: false,
        width: '120px'
      },
      {
        field: 'email',
        header: this.localizationService.localize('Hr::HrEmployee:Email'),
        sortable: false,
        width: '180px'
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
        header: this.localizationService.localize('Hr::HrEmployee:Status'),
        sortable: true,
        type: 'text',
        width: '120px',
        freeze: 'right',
        align: 'center',
        formatter: (value: any) => this.formatStatus(value),
        cellClass: (value: any) => {
          const statusValue = typeof value === 'number' ? value : 
            (value === HrEmployeeStatus.Active ? 0 : 1);
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
        label: this.localizationService.localize('Hr::Edit'),
        icon: 'pi pi-pencil',
        command: (row) => this.openEditDialog(row)
      });
    }

    if (this.permissionService.isGranted(this.PERMISSIONS.DELETE)) {
      this.actions.push({
        label: this.localizationService.localize('Hr::Delete'),
        icon: 'pi pi-trash',
        command: (row) => this.delete(row)
      });
    }

    // Add role management action
    this.actions.push({
      label: this.localizationService.localize('Hr::HrEmployee:ManageRoles'),
      icon: 'pi pi-users',
      command: (row) => this.openRoleManagementDialog(row)
    });
  }

  /**
   * Load tree data
   */
  loadTree(): void {
    this.treeLoading = true;
    this.departmentService.getTree().subscribe({
      next: (result) => {
        this.treeNodes = this.convertTreeDtoToTreeNode(result.items || []);
        this.treeLoading = false;
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Hr::Error'),
          detail: this.localizationService.localize('Hr::InternalServerErrorMessage')
        });
        this.treeLoading = false;
      }
    });
  }

  /**
   * Convert HrDepartmentTreeDto to TreeNode
   */
  private convertTreeDtoToTreeNode(dtos: HrDepartmentTreeDto[]): TreeNode[] {
    return dtos.map(dto => ({
      key: dto.id || '',
      label: `${dto.code} - ${dto.name}`,
      data: dto,
      children: dto.children && dto.children.length > 0 
        ? this.convertTreeDtoToTreeNode(dto.children) 
        : undefined,
      expanded: false,
      leaf: !dto.children || dto.children.length === 0
    }));
  }

  /**
   * Handle tree node selection
   */
  onTreeNodeSelect(event: any): void {
    if (event.node?.data) {
      const department = event.node.data as HrDepartmentTreeDto;
      this.selectedDepartmentId = department.id || null;
      this.searchForm.departmentId = department.id || null;
      this.search();
    }
  }

  /**
   * Handle tree node unselect
   */
  onTreeNodeUnselect(): void {
    this.selectedTreeNode = null;
    this.selectedDepartmentId = null;
    this.searchForm.departmentId = null;
    this.search();
  }

  /**
   * Load employees with lazy loading
   */
  loadData(event: TableLazyLoadEvent): void {
    this.currentLazyLoadEvent = event;
    this.loading = true;

    const input: GetHrEmployeesInput = {
      skipCount: event.first || 0,
      maxResultCount: event.rows || 10,
      sorting: this.getSortingString(event),
      code: this.searchForm.code || undefined,
      fullName: this.searchForm.fullName || undefined,
      status: this.searchForm.status ?? undefined,
      departmentId: this.searchForm.departmentId || undefined,
      orgId: this.searchForm.orgId || undefined,
      partnerId: this.searchForm.partnerId || undefined,
      positionId: this.searchForm.positionId || undefined,
      levelId: this.searchForm.levelId || undefined
    };

    this.employeeService.getList(input).subscribe({
      next: (result) => {
        this.employees = result.items || [];
        this.totalCount = result.totalCount || 0;
        this.loading = false;
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Hr::Error'),
          detail: this.localizationService.localize('Hr::InternalServerErrorMessage')
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
   * Listen for Enter key globally to trigger search when no dialog is open
   */
  @HostListener('window:keydown.enter', ['$event'])
  onWindowKeyDown(event: KeyboardEvent): void {
    if (!this.dialogVisible && !this.roleDialogVisible) {
      this.search();
    }
  }

  /**
   * Reset search form and reload data
   */
  resetSearch(): void {
    this.searchForm = {
      code: null,
      fullName: null,
      status: null,
      departmentId: null,
      orgId: null,
      partnerId: null,
      positionId: null,
      levelId: null
    };
    this.selectedTreeNode = null;
    this.selectedDepartmentId = null;
    this.search();
  }

  /**
   * Open create dialog
   */
  openCreateDialog(): void {
    this.dialogMode = 'create';
    this.formData = this.getEmptyForm();
    this.selectedEmployee = undefined;
    this.loadDialogOptions();
    setTimeout(() => {
      this.updateFullAddress();
    }, 500);
    this.dialogVisible = true;
  }

  /**
   * Open edit dialog
   * ⚠️ QUAN TRỌNG: Code không được phép sửa khi edit
   */
  openEditDialog(employee: HrEmployeeDto): void {
    this.dialogMode = 'edit';
    this.selectedEmployee = employee;
    this.formData = {
      code: employee.code || '',
      fullName: employee.fullName || '',
      status: employee.status ?? HrEmployeeStatus.Active,
      positionId: employee.positionId || null,
      levelId: employee.levelId || null,
      partnerId: employee.partnerId || null,
      orgId: employee.orgId || null,
      departmentId: employee.departmentId || '',
      isManager: employee.isManager ?? null,
      managerId: employee.managerId || null,
      provinceId: employee.provinceId || null,
      wardId: employee.wardId || null,
      address: employee.address || null,
      fullAddress: employee.fullAddress || null,
      phone: employee.phone || null,
      email: employee.email || null,
      userId: employee.userId || null
    };
    this.loadDialogOptions();
    setTimeout(() => {
      this.updateFullAddress();
    }, 500);
    this.dialogVisible = true;
  }

  /**
   * Load options for dialog dropdowns
   */
  private loadDialogOptions(): void {
    // Load departments (all)
    this.departmentService.getList({ maxResultCount: 1000, skipCount: 0 }).subscribe({
      next: (result) => {
        this.departmentOptions = (result.items || []).map(d => ({
          label: `${d.code} - ${d.name}`,
          value: d.id || ''
        }));
      },
      error: () => {
        // Silently fail
      }
    });

    // Load organizations (departments with DeptLevel = Unit)
    this.departmentService.getList({ 
      maxResultCount: 1000, 
      skipCount: 0,
      deptLevel: HrDepartmentLevel.Unit
    }).subscribe({
      next: (result) => {
        this.orgOptions = (result.items || []).map(d => ({
          label: `${d.code} - ${d.name}`,
          value: d.id || ''
        }));
      },
      error: () => {
        // Silently fail
      }
    });

    // Load partners (only active)
    this.partnerService.getList({ 
      maxResultCount: 1000, 
      skipCount: 0,
      status: ResPartnerStatus.Active
    }).subscribe({
      next: (result) => {
        this.partnerOptions = (result.items || []).map(p => ({
          label: `${p.code} - ${p.name}`,
          value: p.id || ''
        }));
      },
      error: () => {
        // Silently fail
      }
    });

    // Load positions (only active)
    this.positionService.getList({ 
      maxResultCount: 1000, 
      skipCount: 0,
      status: HrEmployeePositionStatus.Active
    }).subscribe({
      next: (result) => {
        this.positionOptions = (result.items || []).map(p => ({
          label: `${p.code} - ${p.name}`,
          value: p.id || ''
        }));
      },
      error: () => {
        // Silently fail
      }
    });

    // Load levels (only active)
    this.levelService.getList({ 
      maxResultCount: 1000, 
      skipCount: 0,
      status: HrEmployeeLevelStatus.Active
    }).subscribe({
      next: (result) => {
        this.levelOptions = (result.items || []).map(l => ({
          label: `${l.code} - ${l.name}`,
          value: l.id || ''
        }));
      },
      error: () => {
        // Silently fail
      }
    });

    // Load managers (only active employees)
    this.employeeService.getList({ 
      maxResultCount: 1000, 
      skipCount: 0,
      status: HrEmployeeStatus.Active
    }).subscribe({
      next: (result) => {
        this.managerOptions = (result.items || []).map(e => ({
          label: `${e.code} - ${e.fullName}`,
          value: e.id || ''
        }));
      },
      error: () => {
        // Silently fail
      }
    });

    // Load provinces (only active)
    this.provinceService.getList({ 
      maxResultCount: 1000, 
      skipCount: 0,
      status: ResProvinceStatus.Active
    }).subscribe({
      next: (result) => {
        this.provinceOptions = (result.items || []).map(p => ({
          label: p.name || '',
          value: p.id || ''
        }));
      },
      error: () => {
        // Silently fail
      }
    });

    // Load wards (only active)
    this.wardService.getList({ 
      maxResultCount: 1000, 
      skipCount: 0,
      status: ResWardStatus.Active
    }).subscribe({
      next: (result) => {
        this.wardOptions = (result.items || []).map(w => ({
          label: w.name || '',
          value: w.id || ''
        }));
      },
      error: () => {
        // Silently fail
      }
    });

    // Load users (all active users)
    this.userService.getList({ 
      maxResultCount: 1000, 
      skipCount: 0
    }).subscribe({
      next: (result) => {
        this.userOptions = (result.items || []).map(u => ({
          label: `${u.userName} - ${u.name || ''} ${u.surname || ''}`.trim(),
          value: u.id || ''
        }));
      },
      error: () => {
        // Silently fail
      }
    });
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
          detail: this.localizationService.localize('Hr::HrEmployee:CodeRequired')
        });
        return false;
      }

      if (this.formData.code.length > 50) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: this.localizationService.localize('Hr::HrEmployee:CodeMaxLength')
        });
        return false;
      }

      if (!this.validateCode(this.formData.code)) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: this.localizationService.localize('Hr::HrEmployee:CodeInvalid')
        });
        return false;
      }
    }

    if (!this.formData.fullName || this.formData.fullName.trim() === '') {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Hr::HrEmployee:FullNameRequired')
      });
      return false;
    }

    if (this.formData.fullName.length > 50) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Hr::HrEmployee:FullNameMaxLength')
      });
      return false;
    }

    if (!this.formData.departmentId || this.formData.departmentId.trim() === '') {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Hr::HrEmployee:DepartmentIdRequired')
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
   * Update full address automatically from address + ward + province
   */
  updateFullAddress(): void {
    const parts: string[] = [];

    // Add address
    if (this.formData.address?.trim()) {
      parts.push(this.formData.address.trim());
    }

    // Add ward name
    if (this.formData.wardId) {
      const ward = this.wardOptions.find(w => w.value === this.formData.wardId);
      if (ward) {
        parts.push(ward.label);
      }
    }

    // Add province name
    if (this.formData.provinceId) {
      const province = this.provinceOptions.find(p => p.value === this.formData.provinceId);
      if (province) {
        parts.push(province.label);
      }
    }

    // Join all parts with comma and space
    this.formData.fullAddress = parts.length > 0 ? parts.join(', ') : null;
  }

  /**
   * Create new employee
   */
  private create(): void {
    this.loading = true;

    const createDto: CreateHrEmployeeDto = {
      code: this.formData.code.trim(),
      fullName: this.formData.fullName.trim(),
      status: this.formData.status,
      positionId: this.formData.positionId || undefined,
      levelId: this.formData.levelId || undefined,
      partnerId: this.formData.partnerId || undefined,
      orgId: this.formData.orgId || undefined,
      departmentId: this.formData.departmentId,
      isManager: this.formData.isManager ?? undefined,
      managerId: this.formData.managerId || undefined,
      provinceId: this.formData.provinceId || undefined,
      wardId: this.formData.wardId || undefined,
      address: this.formData.address?.trim() || undefined,
      fullAddress: this.formData.fullAddress?.trim() || undefined,
      phone: this.formData.phone?.trim() || undefined,
      email: this.formData.email?.trim() || undefined,
      userId: this.formData.userId || undefined
    };

    this.employeeService.create(createDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Hr::Success'),
          detail: this.localizationService.localize('Hr::HrEmployee:CreatedSuccessfully')
        });
        this.dialogVisible = false;
        this.loadTree();
        this.search();
      },
      error: (error) => {
        const errorMessage = error.error?.error?.message || 
                            error.error?.error?.details || 
                            this.localizationService.localize('AbpUi::InternalServerErrorMessage');
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Hr::Error'),
          detail: errorMessage
        });
        this.loading = false;
      }
    });
  }

  /**
   * Update existing employee
   * ⚠️ QUAN TRỌNG: Chỉ update các fields được phép, không update Code
   */
  private update(): void {
    if (!this.selectedEmployee || !this.selectedEmployee.id) {
      return;
    }

    this.loading = true;

    const updateDto: UpdateHrEmployeeDto = {
      fullName: this.formData.fullName.trim(),
      status: this.formData.status,
      positionId: this.formData.positionId || undefined,
      levelId: this.formData.levelId || undefined,
      partnerId: this.formData.partnerId || undefined,
      orgId: this.formData.orgId || undefined,
      departmentId: this.formData.departmentId,
      isManager: this.formData.isManager ?? undefined,
      managerId: this.formData.managerId || undefined,
      provinceId: this.formData.provinceId || undefined,
      wardId: this.formData.wardId || undefined,
      address: this.formData.address?.trim() || undefined,
      fullAddress: this.formData.fullAddress?.trim() || undefined,
      phone: this.formData.phone?.trim() || undefined,
      email: this.formData.email?.trim() || undefined,
      userId: this.formData.userId || undefined
    };

    this.employeeService.update(this.selectedEmployee.id, updateDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Hr::Success'),
          detail: this.localizationService.localize('Hr::HrEmployee:UpdatedSuccessfully')
        });
        this.dialogVisible = false;
        this.loadTree();
        this.search();
      },
      error: (error) => {
        const errorMessage = error.error?.error?.message || 
                            error.error?.error?.details || 
                            this.localizationService.localize('AbpUi::InternalServerErrorMessage');
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Hr::Error'),
          detail: errorMessage
        });
        this.loading = false;
      }
    });
  }

  /**
   * Delete employee with confirmation
   * ⚠️ QUAN TRỌNG: Soft delete - set Status to Deactive
   */
  delete(employee: HrEmployeeDto): void {
    if (!employee.id) {
      return;
    }

    this.confirmationService.confirm({
      message: this.localizationService.localize('Hr::HrEmployee:DeleteConfirm'),
      header: this.localizationService.localize('Hr::HrEmployee:Delete'),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.loading = true;
        this.employeeService.delete(employee.id!).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: this.localizationService.localize('Hr::Success'),
              detail: this.localizationService.localize('Hr::HrEmployee:DeletedSuccessfully')
            });
            this.loadTree();
            this.search();
          },
          error: (error) => {
            const errorMessage = error.error?.error?.message || 
                                error.error?.error?.details || 
                                this.localizationService.localize('AbpUi::InternalServerErrorMessage');
            this.messageService.add({
              severity: 'error',
              summary: this.localizationService.localize('Hr::Error'),
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
  private getEmptyForm(): EmployeeFormData {
    return {
      code: '',
      fullName: '',
      status: HrEmployeeStatus.Active,
      positionId: null,
      levelId: null,
      partnerId: null,
      orgId: null,
      departmentId: '',
      isManager: null,
      managerId: null,
      provinceId: null,
      wardId: null,
      address: null,
      fullAddress: null,
      phone: null,
      email: null,
      userId: null
    };
  }

  /**
   * Format status for display
   */
  formatStatus(status: HrEmployeeStatus | number | string | undefined | null): string {
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
      statusValue = status === HrEmployeeStatus.Active ? 0 : 1;
    }
    
    return statusValue === 0
      ? this.localizationService.localize('Hr::HrEmployee:Active')
      : this.localizationService.localize('Hr::HrEmployee:Deactive');
  }

  // ========== Role Management Methods ==========

  /**
   * Open role management dialog
   */
  openRoleManagementDialog(employee: HrEmployeeDto): void {
    if (!employee.id) {
      return;
    }

    this.currentEmployeeId = employee.id;
    // Reset form and mode when opening dialog
    this.roleDialogMode = 'create';
    this.roleFormData = this.getEmptyRoleForm();
    this.selectedRoleRel = undefined;
    this.loadEmployeeRoles(employee.id);
    this.loadRoleOptions();
    this.roleDialogVisible = true;
  }

  /**
   * Load employee roles
   */
  loadEmployeeRoles(employeeId: string): void {
    this.employeeService.getRoles(employeeId).subscribe({
      next: (result) => {
        this.employeeRoles = result || [];
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Hr::Error'),
          detail: this.localizationService.localize('Hr::InternalServerErrorMessage')
        });
      }
    });
  }

  /**
   * Load role options
   */
  private loadRoleOptions(): void {
    this.roleService.getList({ 
      maxResultCount: 1000, 
      skipCount: 0,
      status: HrEmployeeRoleStatus.Active
    }).subscribe({
      next: (result) => {
        this.roleOptions = (result.items || []).map(r => ({
          label: `${r.code} - ${r.name}`,
          value: r.id || ''
        }));
      },
      error: () => {
        // Silently fail
      }
    });
  }

  /**
   * Open add role dialog
   */
  openAddRoleDialog(): void {
    this.roleDialogMode = 'create';
    this.roleFormData = this.getEmptyRoleForm();
    this.selectedRoleRel = undefined;
  }

  /**
   * Open edit role dialog
   */
  openEditRoleDialog(roleRel: HrEmployeeRoleRelDto): void {
    this.roleDialogMode = 'edit';
    this.selectedRoleRel = roleRel;
    this.roleFormData = {
      roleId: roleRel.roleId || '',
      effectDate: this.isoStringToDate(roleRel.effectDate) || null,
      expireDate: this.isoStringToDate(roleRel.expireDate)
    };
  }

  /**
   * Save role (create or update)
   */
  saveRole(): void {
    if (!this.validateRoleForm()) {
      return;
    }

    if (!this.currentEmployeeId) {
      return;
    }

    // Always clear selectedRoleRel before determining action
    // Only update if explicitly in edit mode AND have valid selectedRoleRel with id
    const shouldUpdate = this.roleDialogMode === 'edit' && 
                         this.selectedRoleRel && 
                         this.selectedRoleRel.id;

    if (shouldUpdate) {
      this.updateRole();
    } else {
      // Force create mode - clear any leftover selectedRoleRel
      this.roleDialogMode = 'create';
      this.selectedRoleRel = undefined;
      this.createRole();
    }
  }

  /**
   * Validate role form
   */
  private validateRoleForm(): boolean {
    if (!this.roleFormData.roleId || this.roleFormData.roleId.trim() === '') {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Hr::HrEmployeeRoleRel:RoleIdRequired')
      });
      return false;
    }

    if (!this.roleFormData.effectDate) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Hr::HrEmployeeRoleRel:EffectDateRequired')
      });
      return false;
    }

    // Validate date is valid
    const effectDate = this.roleFormData.effectDate instanceof Date ? this.roleFormData.effectDate : new Date(this.roleFormData.effectDate);
    if (isNaN(effectDate.getTime())) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Hr::HrEmployeeRoleRel:EffectDateRequired')
      });
      return false;
    }

    return true;
  }

  /**
   * Create new role
   */
  private createRole(): void {
    if (!this.currentEmployeeId) {
      return;
    }

    const effectDateStr = this.dateToISOString(this.roleFormData.effectDate);
    if (!effectDateStr) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Hr::HrEmployeeRoleRel:EffectDateRequired')
      });
      return;
    }

    const createDto: CreateHrEmployeeRoleRelDto = {
      roleId: this.roleFormData.roleId,
      effectDate: effectDateStr,
      expireDate: this.dateToISOString(this.roleFormData.expireDate) || undefined
    };

    this.employeeService.addRole(this.currentEmployeeId, createDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Hr::Success'),
          detail: this.localizationService.localize('Hr::HrEmployeeRoleRel:CreatedSuccessfully')
        });
        this.roleFormData = this.getEmptyRoleForm();
        this.selectedRoleRel = undefined;
        this.roleDialogMode = 'create';
        this.loadEmployeeRoles(this.currentEmployeeId!);
      },
      error: (error) => {
        const errorMessage = error.error?.error?.message || 
                            error.error?.error?.details || 
                            this.localizationService.localize('AbpUi::InternalServerErrorMessage');
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Hr::Error'),
          detail: errorMessage
        });
      }
    });
  }

  /**
   * Update existing role
   */
  private updateRole(): void {
    if (!this.currentEmployeeId || !this.selectedRoleRel || !this.selectedRoleRel.id) {
      return;
    }

    const effectDateStr = this.dateToISOString(this.roleFormData.effectDate);
    if (!effectDateStr) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Hr::HrEmployeeRoleRel:EffectDateRequired')
      });
      return;
    }

    const updateDto: UpdateHrEmployeeRoleRelDto = {
      roleId: this.roleFormData.roleId,
      effectDate: effectDateStr,
      expireDate: this.dateToISOString(this.roleFormData.expireDate) || undefined
    };

    this.employeeService.updateRole(this.currentEmployeeId, this.selectedRoleRel.id, updateDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Hr::Success'),
          detail: this.localizationService.localize('Hr::HrEmployeeRoleRel:UpdatedSuccessfully')
        });
        this.roleFormData = this.getEmptyRoleForm();
        this.selectedRoleRel = undefined;
        this.roleDialogMode = 'create';
        if (this.currentEmployeeId) {
          this.loadEmployeeRoles(this.currentEmployeeId);
        }
      },
      error: (error) => {
        const errorMessage = error.error?.error?.message || 
                            error.error?.error?.details || 
                            this.localizationService.localize('AbpUi::InternalServerErrorMessage');
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Hr::Error'),
          detail: errorMessage
        });
      }
    });
  }

  /**
   * Delete role
   */
  deleteRole(roleRel: HrEmployeeRoleRelDto): void {
    if (!this.currentEmployeeId || !roleRel.id) {
      return;
    }

    this.confirmationService.confirm({
      message: this.localizationService.localize('Hr::HrEmployeeRoleRel:DeleteConfirm'),
      header: this.localizationService.localize('Hr::HrEmployeeRoleRel:Delete'),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.employeeService.removeRole(this.currentEmployeeId!, roleRel.id!).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: this.localizationService.localize('Hr::Success'),
              detail: this.localizationService.localize('Hr::HrEmployeeRoleRel:DeletedSuccessfully')
            });
            this.loadEmployeeRoles(this.currentEmployeeId!);
          },
          error: (error) => {
            const errorMessage = error.error?.error?.message || 
                                error.error?.error?.details || 
                                this.localizationService.localize('AbpUi::InternalServerErrorMessage');
            this.messageService.add({
              severity: 'error',
              summary: this.localizationService.localize('Hr::Error'),
              detail: errorMessage
            });
          }
        });
      }
    });
  }

  /**
   * Get empty role form data
   */
  getEmptyRoleForm(): EmployeeRoleFormData {
    return {
      roleId: '',
      effectDate: null,
      expireDate: null
    };
  }

  /**
   * Format date for display (dd/MM/yyyy)
   */
  formatDate(date: string | null | undefined): string {
    if (!date) {
      return '';
    }
    try {
      const d = new Date(date);
      if (isNaN(d.getTime())) {
        return date;
      }
      const day = String(d.getDate()).padStart(2, '0');
      const month = String(d.getMonth() + 1).padStart(2, '0');
      const year = d.getFullYear();
      return `${day}/${month}/${year}`;
    } catch {
      return date;
    }
  }

  /**
   * Convert Date object to ISO string (yyyy-MM-dd)
   */
  private dateToISOString(date: Date | string | null | undefined): string | undefined {
    if (!date) {
      return undefined;
    }
    try {
      const d = date instanceof Date ? date : new Date(date);
      if (isNaN(d.getTime())) {
        return undefined;
      }
      const year = d.getFullYear();
      const month = String(d.getMonth() + 1).padStart(2, '0');
      const day = String(d.getDate()).padStart(2, '0');
      return `${year}-${month}-${day}`;
    } catch {
      return undefined;
    }
  }

  /**
   * Convert ISO string to Date object
   */
  private isoStringToDate(dateStr: string | null | undefined): Date | null {
    if (!dateStr) {
      return null;
    }
    try {
      const d = new Date(dateStr);
      if (isNaN(d.getTime())) {
        return null;
      }
      return d;
    } catch {
      return null;
    }
  }
}

