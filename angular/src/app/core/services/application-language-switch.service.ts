import { inject, Injectable } from '@angular/core';
import { ConfigStateService } from '@abp/ng.core';
import { AbpApplicationConfigurationService } from '../../proxy/volo/abp/asp-net-core/mvc/application-configurations/abp-application-configuration.service';
import { AbpApplicationLocalizationService } from '../../proxy/volo/abp/asp-net-core/mvc/application-configurations/abp-application-localization.service';
import type { ApplicationLocalizationDto } from '../../proxy/volo/abp/asp-net-core/mvc/application-configurations/models';
import { LanguagePreferenceService } from './language-preference.service';

/**
 * Chuyển culture toàn app (lưu preference + tải localization + reload) — dùng chung cho login / selector.
 */
@Injectable({
    providedIn: 'root'
})
export class ApplicationLanguageSwitchService {
    private languagePreferenceService = inject(LanguagePreferenceService);
    private localizationApiService = inject(AbpApplicationLocalizationService);
    private configState = inject(ConfigStateService);
    private applicationConfigService = inject(AbpApplicationConfigurationService);

    switchToCulture(cultureName: string, reloadFullConfig = true): void {
        if (!cultureName) {
            return;
        }

        const run = () => this.loadLocalizationForCulture(cultureName);

        if (reloadFullConfig) {
            this.applicationConfigService
                .get({
                    includeLocalizationResources: true
                })
                .subscribe({
                    next: () => run(),
                    error: (error: unknown) => {
                        console.error('Error reloading application configuration:', error);
                        run();
                    }
                });
        } else {
            run();
        }
    }

    private loadLocalizationForCulture(cultureName: string): void {
        this.languagePreferenceService.setLanguagePreference(cultureName);

        this.localizationApiService
            .get({
                cultureName,
                onlyDynamics: false
            })
            .subscribe({
                next: (_localization: ApplicationLocalizationDto) => {
                    // Luôn refresh + reload: payload đôi khi không có resources đầy nhưng vẫn cần nạp lại app (login / bootstrap).
                    this.configState.refreshAppState().subscribe(() => {
                        window.location.reload();
                    });
                },
                error: (error: unknown) => {
                    console.error('Error loading localization:', error);
                }
            });
    }
}
