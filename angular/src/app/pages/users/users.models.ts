import type { IdentityUserDto } from '@/proxy/identity/models';

export interface UserSearchForm {
  filter: string | null;
}

export interface UserFormData {
  userName: string;
  name: string;
  surname: string;
  email: string;
  phoneNumber: string;
  isActive: boolean;
  lockoutEnabled: boolean;
  password: string;
}

export type UserDialogMode = 'create' | 'edit';

export interface UserDialogData {
  mode: UserDialogMode;
  user?: IdentityUserDto;
}

