import { Injectable, inject } from '@angular/core';
import { Rest, RestService } from '@abp/ng.core';

/** Khớp Volo.Abp.Account.ChangePasswordInput — giữ ngoài proxy để không ghi đè khi generate lại. */
export interface MyProfileChangePasswordInput {
  currentPassword?: string;
  newPassword: string;
}

/**
 * Gọi API profile của ABP (/api/account/my-profile/change-password).
 * Dùng RestService + cùng apiName với proxy account để không sửa file generate.
 */
@Injectable({ providedIn: 'root' })
export class MyProfilePasswordService {
  private readonly restService = inject(RestService);
  /** Phải trùng proxy account khi backend dùng nhóm API này */
  private readonly apiName = 'AbpAccount';

  changePassword(input: MyProfileChangePasswordInput, config?: Partial<Rest.Config>) {
    return this.restService.request<any, void>(
      {
        method: 'POST',
        url: '/api/account/my-profile/change-password',
        body: input,
      },
      { apiName: this.apiName, ...config },
    );
  }
}
