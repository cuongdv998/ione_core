/**
 * Types và interfaces cho permission system
 */

/**
 * Permission name pattern
 * Format: {Module}.{Feature}.{Action}
 * Example: 'Admin.Users.Create', 'Admin.Users.Edit', 'Admin.Users.Delete'
 */
export type PermissionName = string;

/**
 * Multiple permission names
 */
export type PermissionNames = string | string[];

/**
 * Permission check options
 */
export interface PermissionCheckOptions {
  /**
   * Nếu true, cần có tất cả permissions. Nếu false, chỉ cần một trong số đó
   * Mặc định: false
   */
  requireAll?: boolean;
}
