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

import { VTable } from '@/shared/components/v-table/v-table';
import { TableColumn } from '@/shared/models/table-column.model';
import { TableAction } from '@/shared/models/table-action.model';
import { HrDepartmentService } from '@/proxy/hr/controllers/hr-department.service';
import { HrDepartmentDto, CreateHrDepartmentDto, UpdateHrDepartmentDto, GetHrDepartmentsInput, HrDepartmentTreeDto } from '@/proxy/hr/hr-departments/models';
import { HrDepartmentStatus } from '@/proxy/hr-departments/hr-department-status.enum';
import { HrDepartmentLevel } from '@/proxy/hr-departments/hr-department-level.enum';
import { DepartmentSearchForm, DepartmentFormData } from './departments.models';
import { PermissionPipe } from '@/core/pipes/permission.pipe';
import { PermissionService } from '@/core/services/permission.service';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';
import { HrDepartmentTypeService } from '@/proxy/hr/controllers/hr-department-type.service';
import { ResProvinceService } from '@/proxy/master/controllers/res-province.service';
import { ResWardService } from '@/proxy/master/controllers/res-ward.service';
import { ResBankService } from '@/proxy/master/controllers/res-bank.service';
import { HrDepartmentTypeStatus } from '@/proxy/hr-department-types/hr-department-type-status.enum';
import { ResBankStatus } from '@/proxy/res-banks/res-bank-status.enum';
import { ResProvinceStatus } from '@/proxy/res-provinces/res-province-status.enum';
import { ResWardStatus } from '@/proxy/res-wards/res-ward-status.enum';

@Component({
  selector: 'app-departments',
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
    VTable,
    PermissionPipe,
    TranslatePipe
  ],
  templateUrl: './departments.component.html',
  styleUrl: './departments.component.scss',
  providers: [MessageService, ConfirmationService]
})
export class DepartmentsComponent implements OnInit {
  // Permissions
  readonly PERMISSIONS = {
    CREATE: 'HrDepartment.Create',
    UPDATE: 'HrDepartment.Edit',
    DELETE: 'HrDepartment.Delete',
    VIEW: 'HrDepartment.View'
  };

  // Data
  departments: HrDepartmentDto[] = [];
  totalCount = 0;
  loading = true;
  pageSize = 10;

  // Tree
  treeNodes: TreeNode[] = [];
  selectedTreeNode: TreeNode | null = null;
  treeLoading = false;

  // Dialog
  dialogVisible = false;
  dialogMode: 'create' | 'edit' = 'create';
  formData: DepartmentFormData = this.getEmptyForm();
  selectedDepartment?: HrDepartmentDto;
  /**
   * Listen for Enter key events globally to trigger search
   */
  @HostListener('window:keydown.enter', ['$event'])
  onWindowKeyDown(event: KeyboardEvent): void {
    if (!this.dialogVisible) {
      this.search();
    }
  }

  // Search form
  searchForm: DepartmentSearchForm = {
    code: null,
    name: null,
    status: null,
    deptLevel: null,
    parentId: null,
    orgId: null,
    typeId: null,
    provinceId: null,
    wardId: null,
    bankId: null
  };

  // Options
  statusOptions: Array<{ label: string; value: HrDepartmentStatus }> = [];
  deptLevelOptions: Array<{ label: string; value: HrDepartmentLevel }> = [];
  departmentOptions: Array<{ label: string; value: string }> = [];
  typeOptions: Array<{ label: string; value: string }> = [];
  provinceOptions: Array<{ label: string; value: string }> = [];
  wardOptions: Array<{ label: string; value: string }> = [];
  bankOptions: Array<{ label: string; value: string }> = [];

  // Table
  columns: TableColumn[] = [];
  actions: TableAction<HrDepartmentDto>[] = [];
  currentLazyLoadEvent?: TableLazyLoadEvent;

  constructor(
    private departmentService: HrDepartmentService,
    private departmentTypeService: HrDepartmentTypeService,
    private provinceService: ResProvinceService,
    private wardService: ResWardService,
    private bankService: ResBankService,
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
      { label: this.localizationService.localize('Hr::HrDepartment:Active'), value: HrDepartmentStatus.Active },
      { label: this.localizationService.localize('Hr::HrDepartment:Deactive'), value: HrDepartmentStatus.Deactive }
    ];

    this.deptLevelOptions = [
      { label: this.localizationService.localize('Hr::HrDepartment:Unit'), value: HrDepartmentLevel.Unit },
      { label: this.localizationService.localize('Hr::HrDepartment:Dept'), value: HrDepartmentLevel.Dept }
    ];
  }

  /**
   * Initialize table columns with localized headers
   */
  private initializeColumns(): void {
    this.columns = [
      {
        field: 'code',
        header: this.localizationService.localize('Hr::HrDepartment:Code'),
        sortable: true,
        width: '150px'
      },
      {
        field: 'name',
        header: this.localizationService.localize('Hr::HrDepartment:Name'),
        sortable: true,
        width: '250px'
      },
      {
        field: 'parentName',
        header: this.localizationService.localize('Hr::HrDepartment:ParentName'),
        sortable: false,
        width: '200px'
      },
      {
        field: 'orgName',
        header: this.localizationService.localize('Hr::HrDepartment:OrgName'),
        sortable: false,
        width: '200px'
      },
      {
        field: 'typeName',
        header: this.localizationService.localize('Hr::HrDepartment:TypeName'),
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
        header: this.localizationService.localize('Hr::HrDepartment:Status'),
        sortable: true,
        type: 'text',
        width: '120px',
        freeze: 'right',
        align: 'center',
        formatter: (value: any) => this.formatStatus(value),
        cellClass: (value: any) => {
          const statusValue = typeof value === 'number' ? value : 
            (value === HrDepartmentStatus.Active ? 0 : 1);
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
      this.searchForm.parentId = department.id || null;
      this.search();
    }
  }

  /**
   * Handle tree node unselect
   */
  onTreeNodeUnselect(): void {
    this.selectedTreeNode = null;
    this.searchForm.parentId = null;
    this.search();
  }

  /**
   * Load departments with lazy loading
   */
  loadData(event: TableLazyLoadEvent): void {
    this.currentLazyLoadEvent = event;
    this.loading = true;

    const input: GetHrDepartmentsInput = {
      skipCount: event.first || 0,
      maxResultCount: event.rows || 10,
      sorting: this.getSortingString(event),
      code: this.searchForm.code || undefined,
      name: this.searchForm.name || undefined,
      status: this.searchForm.status ?? undefined,
      deptLevel: this.searchForm.deptLevel ?? undefined,
      parentId: this.searchForm.parentId || undefined,
      orgId: this.searchForm.orgId || undefined,
      typeId: this.searchForm.typeId || undefined,
      provinceId: this.searchForm.provinceId || undefined,
      wardId: this.searchForm.wardId || undefined,
      bankId: this.searchForm.bankId || undefined
    };

    this.departmentService.getList(input).subscribe({
      next: (result) => {
        this.departments = result.items || [];
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
   * Reset search form and reload data
   */
  resetSearch(): void {
    this.searchForm = {
      code: null,
      name: null,
      status: null,
      deptLevel: null,
      parentId: null,
      orgId: null,
      typeId: null,
      provinceId: null,
      wardId: null,
      bankId: null
    };
    this.selectedTreeNode = null;
    this.search();
  }

  /**
   * Open create dialog
   */
  openCreateDialog(): void {
    this.dialogMode = 'create';
    this.formData = this.getEmptyForm();
    this.selectedDepartment = undefined;
    this.loadDialogOptions();
    // Wait for options to load, then update full address
    setTimeout(() => {
      this.updateFullAddress();
    }, 500);
    this.dialogVisible = true;
  }

  /**
   * Open edit dialog
   * ⚠️ QUAN TRỌNG: Code không được phép sửa khi edit
   */
  openEditDialog(department: HrDepartmentDto): void {
    this.dialogMode = 'edit';
    this.selectedDepartment = department;
    this.formData = {
      code: department.code || '',
      name: department.name || '',
      description: department.description || null,
      status: department.status ?? HrDepartmentStatus.Active,
      deptLevel: department.deptLevel ?? HrDepartmentLevel.Dept,
      parentId: department.parentId || null,
      orgId: department.orgId || null,
      typeId: department.typeId || null,
      provinceId: department.provinceId || null,
      wardId: department.wardId || null,
      address: department.address || null,
      fullAddress: null, // Will be auto-generated
      bankId: department.bankId || null,
      bankNo: department.bankNo || null
    };
    this.loadDialogOptions();
    // Wait for options to load, then update full address
    setTimeout(() => {
      this.updateFullAddress();
    }, 500);
    this.dialogVisible = true;
  }

  /**
   * Load options for dialog dropdowns
   */
  private loadDialogOptions(): void {
    // Load departments for ParentId and OrgId
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

    // Load department types (only active)
    this.departmentTypeService.getList({ 
      maxResultCount: 1000, 
      skipCount: 0,
      status: HrDepartmentTypeStatus.Active
    }).subscribe({
      next: (result) => {
        this.typeOptions = (result.items || []).map(t => ({
          label: `${t.code} - ${t.name}`,
          value: t.id || ''
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

    // Load banks (only active)
    this.bankService.getList({ 
      maxResultCount: 1000, 
      skipCount: 0,
      status: ResBankStatus.Active
    }).subscribe({
      next: (result) => {
        this.bankOptions = (result.items || []).map(b => ({
          label: `${b.code} - ${b.name}`,
          value: b.id || ''
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
          detail: this.localizationService.localize('Hr::HrDepartment:CodeRequired')
        });
        return false;
      }

      if (this.formData.code.length > 25) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: this.localizationService.localize('Hr::HrDepartment:CodeMaxLength')
        });
        return false;
      }

      if (!this.validateCode(this.formData.code)) {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: this.localizationService.localize('Hr::HrDepartment:CodeInvalid')
        });
        return false;
      }
    }

    if (!this.formData.name || this.formData.name.trim() === '') {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Hr::HrDepartment:NameRequired')
      });
      return false;
    }

    if (this.formData.name.length > 250) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Hr::HrDepartment:NameMaxLength')
      });
      return false;
    }

    // OrgId is optional, no validation needed

    // Validate ProvinceId is required
    if (!this.formData.provinceId || this.formData.provinceId.trim() === '') {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Hr::HrDepartment:ProvinceIdRequired')
      });
      return false;
    }

    // Validate WardId is required
    if (!this.formData.wardId || this.formData.wardId.trim() === '') {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Hr::HrDepartment:WardIdRequired')
      });
      return false;
    }

    // Validate Address is required
    if (!this.formData.address || this.formData.address.trim() === '') {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('AbpUi::Error'),
        detail: this.localizationService.localize('Hr::HrDepartment:AddressRequired')
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
   * Create new department
   */
  private create(): void {
    this.loading = true;

    // Ensure deptLevel is explicitly set from formData (not default)
    const deptLevel = this.formData.deptLevel !== undefined && this.formData.deptLevel !== null
      ? this.formData.deptLevel
      : HrDepartmentLevel.Dept;

    const createDto: CreateHrDepartmentDto = {
      code: this.formData.code.trim(),
      name: this.formData.name.trim(),
      description: this.formData.description?.trim() || undefined,
      status: this.formData.status,
      deptLevel: deptLevel,
      parentId: this.formData.parentId || undefined,
      orgId: this.formData.orgId || undefined,
      typeId: this.formData.typeId || undefined,
      provinceId: this.formData.provinceId || undefined,
      wardId: this.formData.wardId || undefined,
      address: this.formData.address?.trim() || undefined,
      fullAddress: this.formData.fullAddress?.trim() || undefined,
      bankId: this.formData.bankId || undefined,
      bankNo: this.formData.bankNo?.trim() || undefined
    };

    this.departmentService.create(createDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Hr::Success'),
          detail: this.localizationService.localize('Hr::HrDepartment:CreatedSuccessfully')
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
   * Update existing department
   * ⚠️ QUAN TRỌNG: Chỉ update các fields được phép, không update Code
   */
  private update(): void {
    if (!this.selectedDepartment || !this.selectedDepartment.id) {
      return;
    }

    this.loading = true;

    // Ensure deptLevel is explicitly set from formData (not default)
    const deptLevel = this.formData.deptLevel !== undefined && this.formData.deptLevel !== null
      ? this.formData.deptLevel
      : (this.selectedDepartment.deptLevel ?? HrDepartmentLevel.Dept);

    const updateDto: UpdateHrDepartmentDto = {
      name: this.formData.name.trim(),
      description: this.formData.description?.trim() || undefined,
      status: this.formData.status,
      deptLevel: deptLevel,
      parentId: this.formData.parentId || undefined,
      orgId: this.formData.orgId || undefined,
      typeId: this.formData.typeId || undefined,
      provinceId: this.formData.provinceId || undefined,
      wardId: this.formData.wardId || undefined,
      address: this.formData.address?.trim() || undefined,
      fullAddress: this.formData.fullAddress?.trim() || undefined,
      bankId: this.formData.bankId || undefined,
      bankNo: this.formData.bankNo?.trim() || undefined
    };

    this.departmentService.update(this.selectedDepartment.id, updateDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Hr::Success'),
          detail: this.localizationService.localize('Hr::HrDepartment:UpdatedSuccessfully')
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
   * Delete department with confirmation
   * ⚠️ QUAN TRỌNG: Soft delete - set Status to Deactive
   */
  delete(department: HrDepartmentDto): void {
    if (!department.id) {
      return;
    }

    this.confirmationService.confirm({
      message: this.localizationService.localize('Hr::HrDepartment:DeleteConfirm'),
      header: this.localizationService.localize('Hr::HrDepartment:Delete'),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.loading = true;
        this.departmentService.delete(department.id!).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: this.localizationService.localize('Hr::Success'),
              detail: this.localizationService.localize('Hr::HrDepartment:DeletedSuccessfully')
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
  private getEmptyForm(): DepartmentFormData {
    return {
      code: '',
      name: '',
      description: null,
      status: HrDepartmentStatus.Active,
      deptLevel: HrDepartmentLevel.Dept,
      parentId: null,
      orgId: null,
      typeId: null,
      provinceId: null,
      wardId: null,
      address: null,
      fullAddress: null,
      bankId: null,
      bankNo: null
    };
  }

  /**
   * Format status for display
   */
  formatStatus(status: HrDepartmentStatus | number | string | undefined | null): string {
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
      statusValue = status === HrDepartmentStatus.Active ? 0 : 1;
    }
    
    return statusValue === 0
      ? this.localizationService.localize('Hr::HrDepartment:Active')
      : this.localizationService.localize('Hr::HrDepartment:Deactive');
  }
}

