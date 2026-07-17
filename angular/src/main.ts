import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app.config';
import { AppComponent } from './app.component';
import { registerLocaleData } from '@angular/common';
import localeVi from '@angular/common/locales/vi';
import localeEn from '@angular/common/locales/en';
import localeViExtra from '@angular/common/locales/extra/vi';
import localeEnExtra from '@angular/common/locales/extra/en';

// Register locale data with extra data
registerLocaleData(localeVi, 'vi', localeViExtra);
registerLocaleData(localeEn, 'en', localeEnExtra);

bootstrapApplication(AppComponent, appConfig).catch((err) => console.error(err));
