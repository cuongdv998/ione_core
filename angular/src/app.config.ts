import { provideHttpClient, withFetch } from '@angular/common/http';
import { ApplicationConfig } from '@angular/core';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideRouter, withEnabledBlockingInitialNavigation, withInMemoryScrolling } from '@angular/router';
import Aura from '@primeuix/themes/aura';
import { providePrimeNG } from 'primeng/config';
import { appRoutes } from './app.routes';
import { provideAbpCore, withOptions } from '@abp/ng.core';
import { provideAbpOAuth } from '@abp/ng.oauth';
import { provideAccountConfig } from '@abp/ng.account/config';
import { registerLocaleForEsBuild } from '@abp/ng.core/locale';
import { environment } from '@environments/environment';

// Create locale function that returns a Promise
const localeFn = registerLocaleForEsBuild 
  ? (() => {
      try {
        const originalFn = registerLocaleForEsBuild();
        if (originalFn && typeof originalFn === 'function') {
          return (locale?: string) => {
            try {
              const result = originalFn(locale || 'en');
              return result && typeof result?.then === 'function' ? result : Promise.resolve();
            } catch {
              return Promise.resolve();
            }
          };
        }
      } catch {
        // Fallback if registerLocaleForEsBuild fails
      }
      return () => Promise.resolve();
    })()
  : () => Promise.resolve();

export const appConfig: ApplicationConfig = {
    providers: [
        provideRouter(appRoutes, withInMemoryScrolling({ anchorScrolling: 'enabled', scrollPositionRestoration: 'enabled' }), withEnabledBlockingInitialNavigation()),
        provideHttpClient(withFetch()),
        provideAnimationsAsync(),
        providePrimeNG({ theme: { preset: Aura, options: { darkModeSelector: '.app-dark' } } }),
        provideAbpCore(
            withOptions({
                environment,
                registerLocaleFn: localeFn,
            }),
        ),
        provideAbpOAuth(),
        provideAccountConfig(),
    ]
};
