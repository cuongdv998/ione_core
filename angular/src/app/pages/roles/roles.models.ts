import type { PermissionGrantInfoDto } from '@/proxy/permissionManagement/models';

export interface RoleSearchForm {
  filter: string | null;
}

export interface RoleFormData {
  name: string;
  isDefault: boolean;
  isPublic: boolean;
}

export interface PermissionTreeNode {
  permission: PermissionGrantInfoDto;
  children: PermissionTreeNode[];
}

export interface PermissionTreeGroup {
  name: string;
  displayName: string;
  rootNodes: PermissionTreeNode[];
}

