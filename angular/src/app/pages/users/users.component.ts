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

import { VTable } from '@/shared/components/v-table/v-table';
import { TableColumn } from '@/shared/models/table-column.model';
import { TableAction } from '@/shared/models/table-action.model';
import { IdentityUserService } from '@/proxy/volo/abp/identity/identity-user.service';
import { IdentityUserDto, GetIdentityUsersInput, IdentityRoleDto } from '@/proxy/identity/models';
import { UserSearchForm, UserFormData } from './users.models';
import { PermissionPipe } from '@/core/pipes/permission.pipe';
import { PermissionService } from '@/core/services/permission.service';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';

@Component({
  selector: 'app-users',
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
    VTable,
    PermissionPipe,
    TranslatePipe
  ],
  templateUrl: './users.component.html',
  styleUrl: './users.component.scss',
  providers: [MessageService, ConfirmationService]
})
export class UsersComponent implements OnInit {
  // Permissions
  readonly PERMISSIONS = {
    CREATE: 'AbpIdentity.Users.Create',
    UPDATE: 'AbpIdentity.Users.Update',
    DELETE: 'AbpIdentity.Users.Delete',
    MANAGE_ROLES: 'AbpIdentity.Users.Update.ManageRoles'
  };

  // Data properties
  users: IdentityUserDto[] = [];
  totalCount = 0;
  loading = true;
  pageSize = 10;

  // Dialog properties
  userDialogVisible = false;
  userDialogMode: 'create' | 'edit' = 'create';
  userFormData: UserFormData = this.getEmptyUserForm();
  selectedUser?: IdentityUserDto;
  activeTab = '0';

  // Role management
  availableRoles: IdentityRoleDto[] = [];
  userRoles: IdentityRoleDto[] = [];
  selectedRoleNames: string[] = [];
  rolesLoading = false;

  // Search form
  searchForm: UserSearchForm = {
    filter: null
  };

  // Lazy load event
  currentLazyLoadEvent?: TableLazyLoadEvent;

  // Table configuration
  columns: TableColumn[] = [];
  actions: TableAction<IdentityUserDto>[] = [];

  constructor(
    private userService: IdentityUserService,
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
        field: 'userName',
        header: this.localizationService.localize('AbpIdentity::UserName'),
        sortable: true,
        width: '150px'
      },
      {
        field: 'email',
        header: this.localizationService.localize('AbpIdentity::EmailAddress'),
        sortable: true,
        width: '200px'
      },
      {
        field: 'phoneNumber',
        header: this.localizationService.localize('AbpIdentity::PhoneNumber'),
        sortable: true,
        width: '150px'
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
        command: (row) => this.deleteUser(row)
      });
    }
  }

  /**
   * Load users with pagination and filtering
   */
  loadUsers(event: TableLazyLoadEvent): void {
    this.currentLazyLoadEvent = event;
    this.loading = true;

    const input: GetIdentityUsersInput = {
      skipCount: event.first || 0,
      maxResultCount: event.rows || this.pageSize,
      sorting: this.getSortingString(event),
      filter: this.searchForm.filter || undefined
    };

    this.userService.getList(input).subscribe({
      next: (result) => {
        this.users = result.items || [];
        this.totalCount = result.totalCount || 0;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading users:', error);
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: this.localizationService.localize('AbpIdentity::CouldNotLoadUsers')
        });
        this.loading = false;
      }
    });
  }

  /**
   * Search users
   */
  search(): void {
    if (this.currentLazyLoadEvent) {
      this.loadUsers(this.currentLazyLoadEvent);
    }
  }

  /**
   * Listen for Enter key globally to trigger search when no dialog is open
   */
  @HostListener('window:keydown.enter', ['$event'])
  onWindowKeyDown(event: KeyboardEvent): void {
    if (!this.userDialogVisible) {
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
    this.userDialogMode = 'create';
    this.userFormData = this.getEmptyUserForm();
    this.selectedUser = undefined;
    this.selectedRoleNames = [];
    this.loadAvailableRoles();
    this.userDialogVisible = true;
  }

  /**
   * Open edit dialog
   */
  openEditDialog(user: IdentityUserDto): void {
    this.userDialogMode = 'edit';
    this.selectedUser = user;
    this.userFormData = {
      userName: user.userName || '',
      name: user.name || '',
      surname: user.surname || '',
      email: user.email || '',
      phoneNumber: user.phoneNumber || '',
      isActive: user.isActive ?? false,
      lockoutEnabled: user.lockoutEnabled ?? false,
      password: ''
    };
    this.userDialogVisible = true;

    // Load roles for the user
    if (this.permissionService.isGranted(this.PERMISSIONS.MANAGE_ROLES)) {
      this.loadUserRoles(user.id!);
      this.loadAvailableRoles();
    }
  }

  /**
   * Load available roles
   */
  loadAvailableRoles(): void {
    this.rolesLoading = true;
    this.userService.getAssignableRoles().subscribe({
      next: (result) => {
        this.availableRoles = result.items || [];
        // Match MVC user create: pre-select default roles so the form reflects intended assignments.
        if (this.userDialogMode === 'create') {
          this.selectedRoleNames = this.availableRoles
            .filter((r) => r.isDefault && r.name)
            .map((r) => r.name as string);
        }
        this.rolesLoading = false;
      },
      error: (error) => {
        console.error('Error loading available roles:', error);
        this.rolesLoading = false;
      }
    });
  }

  /**
   * Load user roles
   */
  loadUserRoles(userId: string): void {
    this.rolesLoading = true;
    this.userService.getRoles(userId).subscribe({
      next: (result) => {
        this.userRoles = result.items || [];
        this.selectedRoleNames = this.userRoles.map(r => r.name || '').filter(name => name !== '');
        this.rolesLoading = false;
      },
      error: (error) => {
        console.error('Error loading user roles:', error);
        this.rolesLoading = false;
      }
    });
  }

  /**
   * Save user (create or update)
   */
  saveUser(): void {
    if (this.userDialogMode === 'create') {
      this.createUser();
    } else {
      this.updateUser();
    }
  }

  /**
   * Create new user
   */
  private createUser(): void {
    this.loading = true;

    const createDto = {
      ...this.userFormData,
      roleNames: this.selectedRoleNames
    };

    this.userService.create(createDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('AbpUi::Success'),
          detail: this.localizationService.localize('AbpIdentity::UserCreatedSuccessfully')
        });
        this.userDialogVisible = false;
        this.search();
      },
      error: (error) => {
        console.error('Error creating user:', error);
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: error.error?.error?.message || this.localizationService.localize('AbpIdentity::CouldNotCreateUser')
        });
        this.loading = false;
      }
    });
  }

  /**
   * Update existing user
   */
  private updateUser(): void {
    if (!this.selectedUser || !this.selectedUser.id) return;

    this.loading = true;

    const updateDto = {
      ...this.userFormData,
      roleNames: this.selectedRoleNames,
      concurrencyStamp: this.selectedUser.concurrencyStamp
    };

    this.userService.update(this.selectedUser.id!, updateDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('AbpUi::Success'),
          detail: this.localizationService.localize('AbpIdentity::UserUpdatedSuccessfully')
        });
        this.userDialogVisible = false;
        this.search();
      },
      error: (error) => {
        console.error('Error updating user:', error);
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: error.error?.error?.message || this.localizationService.localize('AbpIdentity::CouldNotUpdateUser')
        });
        this.loading = false;
      }
    });
  }

  /**
   * Delete user
   */
  deleteUser(user: IdentityUserDto): void {
    this.confirmationService.confirm({
      message: this.localizationService.localize('AbpIdentity::DeleteUserConfirmation', user.userName),
      header: this.localizationService.localize('AbpUi::AreYouSure'),
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.loading = true;
        this.userService.delete(user.id!).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: this.localizationService.localize('AbpUi::Success'),
              detail: this.localizationService.localize('AbpIdentity::UserDeletedSuccessfully')
            });
            this.search();
          },
          error: (error) => {
            console.error('Error deleting user:', error);
            this.messageService.add({
              severity: 'error',
              summary: this.localizationService.localize('AbpUi::Error'),
              detail: error.error?.error?.message || this.localizationService.localize('AbpIdentity::CouldNotDeleteUser')
            });
            this.loading = false;
          }
        });
      }
    });
  }

  /**
   * Get empty user form
   */
  private getEmptyUserForm(): UserFormData {
    return {
      userName: '',
      name: '',
      surname: '',
      email: '',
      phoneNumber: '',
      isActive: true,
      lockoutEnabled: true,
      password: ''
    };
  }

  /**
   * Get sorting string for API
   */
  private getSortingString(event: TableLazyLoadEvent): string {
    if (event.sortField) {
      const direction = event.sortOrder === 1 ? 'ASC' : 'DESC';
      return `${event.sortField} ${direction}`;
    }
    return '';
  }
}

