import { Component, HostListener, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { CheckboxModule } from 'primeng/checkbox';
import { TableLazyLoadEvent } from 'primeng/table';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { DialogModule } from 'primeng/dialog';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TabsModule } from 'primeng/tabs';
import { AccordionModule } from 'primeng/accordion';

import { VTable } from '@/shared/components/v-table/v-table';
import { TableColumn } from '@/shared/models/table-column.model';
import { TableAction } from '@/shared/models/table-action.model';
import { IdentityRoleService } from '@/proxy/volo/abp/identity/identity-role.service';
import { IdentityRoleDto, GetIdentityRolesInput } from '@/proxy/identity/models';
import { RoleSearchForm, RoleFormData, PermissionTreeNode, PermissionTreeGroup } from './roles.models';
import { PermissionPipe } from '@/core/pipes/permission.pipe';
import { PermissionService } from '@/core/services/permission.service';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';
import { PermissionManagementService } from '@/proxy/volo/abp/permission-management/permission.service';
import { GetPermissionListResultDto, PermissionGroupDto, PermissionGrantInfoDto, UpdatePermissionsDto } from '@/proxy/permissionManagement/models';

@Component({
  selector: 'app-roles',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    PanelModule,
    ButtonModule,
    InputTextModule,
    CheckboxModule,
    ToastModule,
    DialogModule,
    ConfirmDialogModule,
    TabsModule,
    AccordionModule,
    VTable,
    PermissionPipe,
    TranslatePipe
  ],
  templateUrl: './roles.component.html',
  styleUrl: './roles.component.scss',
  providers: [MessageService, ConfirmationService]
})
export class RolesComponent implements OnInit {
  // Permissions
  readonly PERMISSIONS = {
    CREATE: 'AbpIdentity.Roles.Create',
    UPDATE: 'AbpIdentity.Roles.Update',
    DELETE: 'AbpIdentity.Roles.Delete',
    MANAGE_PERMISSIONS: 'AbpIdentity.Roles.ManagePermissions'
  };

  // Data properties
  roles: IdentityRoleDto[] = [];
  totalCount = 0;
  loading = true;
  pageSize = 10;

  // Dialog properties
  roleDialogVisible = false;
  roleDialogMode: 'create' | 'edit' = 'create';
  roleFormData: RoleFormData = this.getEmptyRoleForm();
  selectedRole?: IdentityRoleDto;

  // Permission management
  permissionData?: GetPermissionListResultDto;
  permissionsLoading = false;
  selectedPermissions: { [key: string]: boolean } = {};
  permissionTreeGroups: PermissionTreeGroup[] = [];
  filteredPermissionTreeGroups: PermissionTreeGroup[] = [];
  /** Lọc theo tên/hiển thị nhóm quyền (accordion); trong nhóm hiển thị đủ cây quyền con. */
  groupSearchText = '';
  /** Lọc theo tên kỹ thuật hoặc tên hiển thị của từng quyền. */
  permissionDetailSearchText = '';

  // Search form
  searchForm: RoleSearchForm = {
    filter: null
  };

  // Lazy load event
  currentLazyLoadEvent?: TableLazyLoadEvent;

  // Table configuration
  columns: TableColumn[] = [];
  actions: TableAction<IdentityRoleDto>[] = [];

  tab = '0'

  constructor(
    private roleService: IdentityRoleService,
    private permissionManagementService: PermissionManagementService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private permissionService: PermissionService,
    private localizationService: LocalizationService
  ) {
    this.initializeColumns();
    this.initializeActions();
  }

  ngOnInit(): void {
    // Initial load will be triggered by the table's onLazyLoad event
  }

  /**
   * Initialize table columns with localized headers
   */
  private initializeColumns(): void {
    this.columns = [
      {
        field: 'name',
        header: this.localizationService.localize('AbpIdentity::RoleName'),
        sortable: true,
        width: '200px'
      },
      {
        field: 'isDefault',
        header: this.localizationService.localize('AbpIdentity::DisplayName:IsDefault'),
        sortable: true,
        type: 'text',
        width: '120px'
      },
      {
        field: 'isPublic',
        header: this.localizationService.localize('AbpIdentity::DisplayName:IsPublic'),
        sortable: true,
        type: 'text',
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
   * Initialize actions based on permissions
   */
  private initializeActions(): void {
    if (this.permissionService.isGranted(this.PERMISSIONS.UPDATE)) {
      this.actions.push({
        label: this.localizationService.localize('AbpUi::Edit'),
        icon: 'pi pi-pencil',
        command: (row) => this.openEditDialog(row)
      });
    }

    if (this.permissionService.isGranted(this.PERMISSIONS.DELETE)) {
      this.actions.push({
        label: this.localizationService.localize('AbpUi::Delete'),
        icon: 'pi pi-trash',
        command: (row) => this.confirmDelete(row)
      });
    }
  }

  /**
   * Load roles with lazy loading
   */
  loadRoles(event: TableLazyLoadEvent): void {
    this.currentLazyLoadEvent = event;
    this.loading = true;

    const input: GetIdentityRolesInput = {
      skipCount: event.first || 0,
      maxResultCount: event.rows || 10,
      sorting: this.getSortingString(event),
      filter: this.searchForm.filter || undefined
    };

    this.roleService.getList(input).subscribe({
      next: (result) => {
        this.roles = result.items || [];
        this.totalCount = result.totalCount || 0;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading roles:', error);
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: this.localizationService.localize('AbpUi::InternalServerErrorMessage')
        });
        this.loading = false;
      }
    });
  }

  /**
   * Execute search with current filters
   */
  search(): void {
    if (this.currentLazyLoadEvent) {
      this.loadRoles(this.currentLazyLoadEvent);
    }
  }

  /**
   * Listen for Enter key globally to trigger search when no dialog is open
   */
  @HostListener('window:keydown.enter', ['$event'])
  onWindowKeyDown(event: KeyboardEvent): void {
    if (!this.roleDialogVisible) {
      this.search();
    }
  }

  /**
   * Reset search form and reload data
   */
  resetSearch(): void {
    this.searchForm = {
      filter: null
    };
    this.search();
  }

  /**
   * Open create dialog
   */
  openCreateDialog(): void {
    this.roleDialogMode = 'create';
    this.roleFormData = this.getEmptyRoleForm();
    this.selectedRole = undefined;
    this.permissionData = undefined;
    this.selectedPermissions = {};
    this.permissionTreeGroups = [];
    this.filteredPermissionTreeGroups = [];
    this.groupSearchText = '';
    this.permissionDetailSearchText = '';
    this.roleDialogVisible = true;
    this.tab = '0';
  }

  /**
   * Open edit dialog
   */
  openEditDialog(role: IdentityRoleDto): void {
    this.roleDialogMode = 'edit';
    this.selectedRole = role;
    this.roleFormData = {
      name: role.name || '',
      isDefault: role.isDefault ?? false,
      isPublic: role.isPublic ?? false
    };
    this.roleDialogVisible = true;

    // Load permissions for the role
    if (this.permissionService.isGranted(this.PERMISSIONS.MANAGE_PERMISSIONS) && role.name) {
      this.loadPermissions(role.name);
    }
  }

  /**
   * Handle dialog hide event
   */
  onDialogHide(): void {
    this.tab = '0';
  }

  /**
   * Load permissions for a role
   */
  loadPermissions(roleName: string): void {
    this.permissionsLoading = true;
    this.permissionManagementService.get('R', roleName).subscribe({
      next: (result) => {
        this.permissionData = result;
        this.selectedPermissions = {};

        // Build selected permissions map
        result.groups?.forEach(group => {
          group.permissions?.forEach(permission => {
            if (permission.name) {
              this.selectedPermissions[permission.name] = permission.isGranted ?? false;
            }
          });
        });

        // Build permission tree for display
        this.permissionTreeGroups = this.buildPermissionTree(result);
        this.filteredPermissionTreeGroups = this.permissionTreeGroups;
        this.groupSearchText = '';
        this.permissionDetailSearchText = '';

        this.permissionsLoading = false;
      },
      error: (error) => {
        console.error('Error loading permissions:', error);
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpIdentity::Error'),
          detail: this.localizationService.localize('AbpIdentity::CouldNotLoadPermissions'),
        });
        this.permissionsLoading = false;
      },
    });
  }

  /**
   * Build permission tree from flat list (recursive structure)
   */
  private buildPermissionTree(result: GetPermissionListResultDto): PermissionTreeGroup[] {
    if (!result.groups) {
      return [];
    }

    return result.groups.map(group => {
      // Build tree for this group
      const permissionMap = new Map<string, PermissionTreeNode>();
      const rootNodes: PermissionTreeNode[] = [];

      // First pass: create all nodes
      group.permissions?.forEach(permission => {
        if (permission.name) {
          permissionMap.set(permission.name, {
            permission,
            children: []
          });
        }
      });

      // Second pass: build parent-child relationships
      group.permissions?.forEach(permission => {
        if (!permission.name) return;
        const node = permissionMap.get(permission.name);
        if (!node) return;

        if (permission.parentName) {
          // Has parent, add to parent's children
          const parentNode = permissionMap.get(permission.parentName);
          if (parentNode) {
            parentNode.children.push(node);
          } else {
            // Parent not found, treat as root
            rootNodes.push(node);
          }
        } else {
          // No parent, it's a root node
          rootNodes.push(node);
        }
      });

      // Sort children recursively
      const sortNodes = (nodes: PermissionTreeNode[]) => {
        nodes.sort((a, b) => (a.permission.displayName || '').localeCompare(b.permission.displayName || ''));
        nodes.forEach(node => {
          if (node.children.length > 0) {
            sortNodes(node.children);
          }
        });
      };
      sortNodes(rootNodes);

      return {
        name: group.name || '',
        displayName: group.displayName || '',
        rootNodes
      };
    });
  }

  /**
   * Toggle permission
   */
  onPermissionChange(permissionName: string, isGranted: boolean): void {
    this.selectedPermissions[permissionName] = isGranted;

    // Auto-grant parent permissions if child is granted
    if (isGranted) {
      this.grantParentPermissions(permissionName);
    }
    // Auto-revoke child permissions if parent is revoked
    else {
      this.revokeChildPermissions(permissionName);
    }
  }

  /**
   * Ô 1: lọc theo nhóm (accordion); mở nhóm thấy đủ quyền con.
   * Ô 2: lọc theo tên kỹ thuật / tên hiển thị từng quyền.
   */
  filterPermissions(): void {
    const groupQ = this.groupSearchText?.toLowerCase().trim() || '';
    const detailQ = this.permissionDetailSearchText?.toLowerCase().trim() || '';

    if (!groupQ && !detailQ) {
      this.filteredPermissionTreeGroups = this.permissionTreeGroups;
      return;
    }

    this.filteredPermissionTreeGroups = this.permissionTreeGroups.map(group => {
      const groupMatches =
        !groupQ ||
        (group.displayName || '').toLowerCase().includes(groupQ) ||
        (group.name || '').toLowerCase().includes(groupQ);

      if (!groupMatches) {
        return null;
      }

      if (!detailQ) {
        return { ...group, rootNodes: group.rootNodes };
      }

      const filterNode = (node: PermissionTreeNode): PermissionTreeNode | null => {
        const detailMatches =
          (node.permission.name || '').toLowerCase().includes(detailQ) ||
          (node.permission.displayName || '').toLowerCase().includes(detailQ);

        const filteredChildren = node.children
          .map(child => filterNode(child))
          .filter((child): child is PermissionTreeNode => child !== null);

        if (detailMatches || filteredChildren.length > 0) {
          return { ...node, children: filteredChildren };
        }

        return null;
      };

      const filteredRootNodes = group.rootNodes
        .map(node => filterNode(node))
        .filter((node): node is PermissionTreeNode => node !== null);

      if (filteredRootNodes.length === 0) {
        return null;
      }

      return { ...group, rootNodes: filteredRootNodes };
    }).filter((group): group is PermissionTreeGroup => group !== null);
  }

  /**
   * Select all permissions
   */
  selectAllPermissions(): void {
    if (!this.permissionData) return;

    this.permissionData.groups?.forEach(group => {
      group.permissions?.forEach(permission => {
        if (permission.name) {
          this.selectedPermissions[permission.name] = true;
        }
      });
    });
  }

  /**
   * Deselect all permissions
   */
  deselectAllPermissions(): void {
    if (!this.permissionData) return;

    this.permissionData.groups?.forEach(group => {
      group.permissions?.forEach(permission => {
        if (permission.name) {
          this.selectedPermissions[permission.name] = false;
        }
      });
    });
  }

  /**
   * Select all permissions in a group
   */
  selectAllInGroup(groupName: string): void {
    if (!this.permissionData) return;

    const group = this.permissionData.groups?.find(g => g.name === groupName);
    if (!group) return;

    group.permissions?.forEach(permission => {
      if (permission.name) {
        this.selectedPermissions[permission.name] = true;
      }
    });
  }

  /**
   * Deselect all permissions in a group
   */
  deselectAllInGroup(groupName: string): void {
    if (!this.permissionData) return;

    const group = this.permissionData.groups?.find(g => g.name === groupName);
    if (!group) return;

    group.permissions?.forEach(permission => {
      if (permission.name) {
        this.selectedPermissions[permission.name] = false;
      }
    });
  }

  /**
   * Check if all permissions in a group are selected
   */
  isGroupFullySelected(groupName: string): boolean {
    if (!this.permissionData) return false;

    const group = this.permissionData.groups?.find(g => g.name === groupName);
    if (!group || !group.permissions || group.permissions.length === 0) return false;

    return group.permissions.every(permission => {
      if (!permission.name) return true;
      return this.selectedPermissions[permission.name] === true;
    });
  }

  /**
   * Check if any permission in a group is selected
   */
  isGroupPartiallySelected(groupName: string): boolean {
    if (!this.permissionData) return false;

    const group = this.permissionData.groups?.find(g => g.name === groupName);
    if (!group || !group.permissions || group.permissions.length === 0) return false;

    const hasSelected = group.permissions.some(permission => {
      if (!permission.name) return false;
      return this.selectedPermissions[permission.name] === true;
    });

    const allSelected = this.isGroupFullySelected(groupName);

    return hasSelected && !allSelected;
  }

  /**
   * Get count of selected permissions in a group
   */
  getSelectedCountInGroup(groupName: string): { selected: number; total: number } {
    if (!this.permissionData) return { selected: 0, total: 0 };

    const group = this.permissionData.groups?.find(g => g.name === groupName);
    if (!group || !group.permissions) return { selected: 0, total: 0 };

    const total = group.permissions.filter(p => p.name).length;
    const selected = group.permissions.filter(p => {
      if (!p.name) return false;
      return this.selectedPermissions[p.name] === true;
    }).length;

    return { selected, total };
  }

  /**
   * Get total count of selected permissions
   */
  getTotalSelectedCount(): { selected: number; total: number } {
    if (!this.permissionData) return { selected: 0, total: 0 };

    let total = 0;
    let selected = 0;

    this.permissionData.groups?.forEach(group => {
      group.permissions?.forEach(permission => {
        if (permission.name) {
          total++;
          if (this.selectedPermissions[permission.name] === true) {
            selected++;
          }
        }
      });
    });

    return { selected, total };
  }

  /**
   * Grant parent permissions recursively
   */
  private grantParentPermissions(permissionName: string): void {
    this.permissionData?.groups?.forEach(group => {
      const permission = group.permissions?.find(p => p.name === permissionName);
      if (permission?.parentName) {
        this.selectedPermissions[permission.parentName] = true;
        this.grantParentPermissions(permission.parentName);
      }
    });
  }

  /**
   * Revoke child permissions recursively
   */
  private revokeChildPermissions(permissionName: string): void {
    this.permissionData?.groups?.forEach(group => {
      group.permissions?.forEach(permission => {
        if (permission.parentName === permissionName && permission.name) {
          this.selectedPermissions[permission.name] = false;
          this.revokeChildPermissions(permission.name);
        }
      });
    });
  }

  /**
   * Save based on current tab
   */
  save(): void {
    if (this.tab === '1') {
      this.savePermissions();
    } else {
      this.saveRole();
    }
  }

  /**
   * Save role (create or update)
   */
  saveRole(): void {
    if (this.roleDialogMode === 'create') {
      this.createRole();
    } else {
      this.updateRole();
    }
    this.tab = '0';
  }

  /**
   * Save permissions for the role
   */
  savePermissions(): void {
    if (!this.selectedRole) {
      return;
    }
    console.log('selectedPermissions', this.selectedPermissions);
    this.permissionsLoading = true;

    const permissions: UpdatePermissionsDto = {
      permissions: Object.keys(this.selectedPermissions).map(name => ({
        name,
        isGranted: this.selectedPermissions[name]
      }))
    };

    if (!this.selectedRole?.name) return;

    this.permissionManagementService.update('R', this.selectedRole.name, permissions).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('AbpIdentity::Success'),
          detail: this.localizationService.localize('AbpIdentity::PermissionsUpdatedSuccessfully'),
        });
        this.permissionsLoading = false;
        this.roleDialogVisible = false;
        this.search();
      },
      error: (error) => {
        console.error('Error updating permissions:', error);
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpIdentity::Error'),
          detail: error.error?.error?.message || this.localizationService.localize('AbpIdentity::CouldNotUpdatePermissions'),
        });
        this.permissionsLoading = false;
      },
    });
  }

  /**
   * Create new role
   */
  private createRole(): void {
    this.loading = true;

    this.roleService.create(this.roleFormData).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('AbpUi::Success'),
          detail: this.localizationService.localize('AbpIdentity::RoleCreatedMessage')
        });
        this.roleDialogVisible = false;
        this.search();
      },
      error: (error) => {
        console.error('Error creating role:', error);
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: error.error?.error?.message || this.localizationService.localize('AbpUi::InternalServerErrorMessage')
        });
        this.loading = false;
      }
    });
  }

  /**
   * Update existing role
   */
  private updateRole(): void {
    if (!this.selectedRole || !this.selectedRole.id) return;

    this.loading = true;

    const updateDto = {
      ...this.roleFormData,
      concurrencyStamp: this.selectedRole.concurrencyStamp
    };
    console.log('updateDto', updateDto);

    this.roleService.update(this.selectedRole.id!, updateDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('AbpUi::Success'),
          detail: this.localizationService.localize('AbpIdentity::RoleUpdatedMessage')
        });
        this.roleDialogVisible = false;
        this.search();
      },
      error: (error) => {
        console.error('Error updating role:', error);
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: error.error?.error?.message || this.localizationService.localize('AbpUi::InternalServerErrorMessage')
        });
        this.loading = false;
      }
    });
  }

  /**
   * Confirm delete role
   */
  confirmDelete(role: IdentityRoleDto): void {
    if (role.isStatic) {
      this.messageService.add({
        severity: 'warn',
        summary: this.localizationService.localize('AbpUi::Warning'),
        detail: this.localizationService.localize('AbpIdentity::StaticRolesDeletionErrorMessage')
      });
      return;
    }

    this.confirmationService.confirm({
      message: this.localizationService.localize('AbpIdentity::RoleDeletionConfirmationMessage', role.name),
      header: this.localizationService.localize('AbpUi::AreYouSure'),
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: this.localizationService.localize('AbpUi::Yes'),
      rejectLabel: this.localizationService.localize('AbpUi::No'),
      accept: () => {
        this.deleteRole(role);
      }
    });
  }

  /**
   * Delete role
   */
  private deleteRole(role: IdentityRoleDto): void {
    if (!role.id) return;

    this.loading = true;

    this.roleService.delete(role.id!).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('AbpUi::Success'),
          detail: this.localizationService.localize('AbpIdentity::RoleDeletedMessage')
        });
        this.search();
      },
      error: (error) => {
        console.error('Error deleting role:', error);
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: error.error?.error?.message || this.localizationService.localize('AbpUi::InternalServerErrorMessage')
        });
        this.loading = false;
      }
    });
  }

  /**
   * Get empty role form
   */
  private getEmptyRoleForm(): RoleFormData {
    return {
      name: '',
      isDefault: false,
      isPublic: false
    };
  }

  /**
   * Get sorting string from table event
   */
  private getSortingString(event: TableLazyLoadEvent): string {
    if (event.sortField) {
      const direction = event.sortOrder === 1 ? 'ASC' : 'DESC';
      return `${event.sortField} ${direction}`;
    }
    return '';
  }
}

