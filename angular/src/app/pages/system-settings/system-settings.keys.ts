/** Khớp Volo.Abp.Identity.Settings.IdentitySettingNames */
export const IdentitySettingKeys = {
  SignInRequireConfirmedEmail: 'Abp.Identity.SignIn.RequireConfirmedEmail',
  SignInRequireEmailVerificationToRegister: 'Abp.Identity.SignIn.RequireEmailVerificationToRegister',
  SignInEnablePhoneNumberConfirmation: 'Abp.Identity.SignIn.EnablePhoneNumberConfirmation',
  SignInRequireConfirmedPhoneNumber: 'Abp.Identity.SignIn.RequireConfirmedPhoneNumber',
  UserIsUserNameUpdateEnabled: 'Abp.Identity.User.IsUserNameUpdateEnabled',
  UserIsEmailUpdateEnabled: 'Abp.Identity.User.IsEmailUpdateEnabled',

  PasswordRequiredLength: 'Abp.Identity.Password.RequiredLength',
  PasswordRequiredUniqueChars: 'Abp.Identity.Password.RequiredUniqueChars',
  PasswordRequireNonAlphanumeric: 'Abp.Identity.Password.RequireNonAlphanumeric',
  PasswordRequireLowercase: 'Abp.Identity.Password.RequireLowercase',
  PasswordRequireUppercase: 'Abp.Identity.Password.RequireUppercase',
  PasswordRequireDigit: 'Abp.Identity.Password.RequireDigit',
  PasswordForcePeriodicallyChange: 'Abp.Identity.Password.ForceUsersToPeriodicallyChangePassword',
  PasswordChangePeriodDays: 'Abp.Identity.Password.PasswordChangePeriodDays',

  LockoutAllowedForNewUsers: 'Abp.Identity.Lockout.AllowedForNewUsers',
  LockoutDurationSeconds: 'Abp.Identity.Lockout.LockoutDuration',
  LockoutMaxFailedAttempts: 'Abp.Identity.Lockout.MaxFailedAccessAttempts',
} as const;

/** Khớp Volo.Abp.Emailing.EmailSettingNames */
export const EmailSettingKeys = {
  DefaultFromAddress: 'Abp.Mailing.DefaultFromAddress',
  DefaultFromDisplayName: 'Abp.Mailing.DefaultFromDisplayName',
  SmtpHost: 'Abp.Mailing.Smtp.Host',
  SmtpPort: 'Abp.Mailing.Smtp.Port',
  SmtpUserName: 'Abp.Mailing.Smtp.UserName',
  SmtpPassword: 'Abp.Mailing.Smtp.Password',
  SmtpEnableSsl: 'Abp.Mailing.Smtp.EnableSsl',
  SmtpUseDefaultCredentials: 'Abp.Mailing.Smtp.UseDefaultCredentials',
} as const;

/** Tab Thiết lập tài khoản: chỉ khóa tài khoản (Identity Lockout). */
export const ACCOUNT_TAB_KEYS: readonly string[] = [
  IdentitySettingKeys.LockoutAllowedForNewUsers,
  IdentitySettingKeys.LockoutDurationSeconds,
  IdentitySettingKeys.LockoutMaxFailedAttempts,
];

export const PASSWORD_TAB_KEYS: readonly string[] = [
  IdentitySettingKeys.PasswordRequiredLength,
  IdentitySettingKeys.PasswordRequiredUniqueChars,
  IdentitySettingKeys.PasswordRequireNonAlphanumeric,
  IdentitySettingKeys.PasswordRequireLowercase,
  IdentitySettingKeys.PasswordRequireUppercase,
  IdentitySettingKeys.PasswordRequireDigit,
  IdentitySettingKeys.PasswordForcePeriodicallyChange,
  IdentitySettingKeys.PasswordChangePeriodDays,
];

/** Đọc API (không đọc mật khẩu SMTP qua GET). */
export const EMAIL_TAB_KEYS_GET: readonly string[] = [
  EmailSettingKeys.DefaultFromAddress,
  EmailSettingKeys.DefaultFromDisplayName,
  EmailSettingKeys.SmtpHost,
  EmailSettingKeys.SmtpPort,
  EmailSettingKeys.SmtpUserName,
  EmailSettingKeys.SmtpEnableSsl,
  EmailSettingKeys.SmtpUseDefaultCredentials,
];

/** Ghi API (SMTP password chỉ khi người dùng nhập mới). */
export const EMAIL_TAB_KEYS_SAVE_BASE: readonly string[] = [...EMAIL_TAB_KEYS_GET];
