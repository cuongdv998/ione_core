import { Component, OnInit, inject } from '@angular/core';
import { RouterModule } from '@angular/router';
import { ConfigStateService } from '@abp/ng.core';
import { LanguagePreferenceService } from './app/core/services/language-preference.service';
import { ApplicationLanguageSwitchService } from './app/core/services/application-language-switch.service';
import { filter } from 'rxjs/operators';

/** Trùng Volo.Abp.Localization.LocalizationSettingNames.DefaultLanguage — phải isVisibleToClients trên backend. */
const DEFAULT_LANGUAGE_SETTING_KEY = 'Abp.Localization.DefaultLanguage';

@Component({
    selector: 'app-root',
    standalone: true,
    imports: [RouterModule],
    template: `<router-outlet></router-outlet>`,
})
export class AppComponent implements OnInit {
    private configState = inject(ConfigStateService);
    private languagePreferenceService = inject(LanguagePreferenceService);
    private applicationLanguageSwitch = inject(ApplicationLanguageSwitchService);
    private hasInitializedLanguage = false;
    /** Tránh gọi switch+reload liên tục khi values vẫn rỗng. */
    private triedHydrateFromEmpty = false;

    ngOnInit() {
        // Đợi application configuration được load lần đầu
        // Lần bootstrap ABP thường gọi application-configuration với includeLocalizationResources=false
        // → currentCulture có thể đúng (vi-VN) nhưng localization.values rỗng → login vẫn thấy tiếng Anh nếu chỉ refreshAppState().
        this.configState
            .getOne$('localization')
            .pipe(filter((localization) => !!localization))
            .subscribe({
                next: (localization) => {
                    const savedLanguage = this.languagePreferenceService.getSavedLanguagePreference();
                    const currentLanguage = localization.currentCulture?.cultureName;

                    if (savedLanguage) {
                        if (
                            savedLanguage !== currentLanguage &&
                            currentLanguage
                        ) {
                            // Chỉ đồng bộ localStorage không cập nhật bản dịch trong ConfigState.
                            this.loadLocalizationForCulture(currentLanguage);
                        }
                    } else if (!this.hasInitializedLanguage) {
                        const setting = this.configState.getOne('setting') as
                            | { values?: Record<string, string> }
                            | undefined;
                        const defaultFromSetting = setting?.values?.[DEFAULT_LANGUAGE_SETTING_KEY]?.trim();
                        const values = localization.values;
                        const hasResourceValues = !!(values && Object.keys(values).length > 0);

                        if (defaultFromSetting && defaultFromSetting !== currentLanguage) {
                            this.loadLocalizationForCulture(defaultFromSetting);
                        } else if (
                            !hasResourceValues &&
                            currentLanguage &&
                            !this.triedHydrateFromEmpty
                        ) {
                            this.triedHydrateFromEmpty = true;
                            this.loadLocalizationForCulture(currentLanguage);
                        } else if (currentLanguage) {
                            this.languagePreferenceService.setLanguagePreference(currentLanguage);
                        }

                        this.hasInitializedLanguage = true;
                    }
                },
            });
    }

    /**
     * Cùng luồng đổi ngôn ngữ trong app: application-configuration (includeLocalizationResources: true),
     * /application-localization, rồi reload — đủ texts cho TranslatePipe (trang login).
     */
    private loadLocalizationForCulture(cultureName: string): void {
        if (!cultureName) {
            return;
        }
        this.applicationLanguageSwitch.switchToCulture(cultureName, true);
    }
}
