import { Component, OnInit, Input, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LocalizationService } from '../../services/localization.service';
import type { LanguageInfo } from '../../../proxy/volo/abp/localization/models';
import { ApplicationLanguageSwitchService } from '../../services/application-language-switch.service';

/**
 * Component để chọn ngôn ngữ trong menu (compact version)
 * Sử dụng trong user menu hoặc dropdown menu
 */
@Component({
  selector: 'app-language-selector-menu',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="language-selector-menu">
      <div class="language-menu-header">
        <i class="pi pi-globe"></i>
        <span>Language</span>
      </div>
      <div 
        *ngFor="let lang of languages()" 
        class="language-menu-item"
        [class.active]="lang.cultureName === currentCulture()"
        (click)="onLanguageSelect(lang.cultureName || '')"
      >
        <i class="pi pi-check" *ngIf="lang.cultureName === currentCulture()"></i>
        <span>{{ lang.displayName || lang.cultureName }}</span>
      </div>
    </div>
  `,
  styles: [`
    .language-selector-menu {
      display: flex;
      flex-direction: column;
    }
    
    .language-menu-header {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      padding: 0.5rem 1rem;
      font-weight: 600;
      color: var(--text-color-secondary);
      font-size: 0.875rem;
      text-transform: uppercase;
      letter-spacing: 0.5px;
    }
    
    .language-menu-header i {
      font-size: 0.875rem;
    }
    
    .language-menu-item {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      padding: 0.75rem 1rem;
      cursor: pointer;
      transition: background-color 0.2s;
      color: var(--text-color);
    }
    
    .language-menu-item:hover {
      background-color: var(--surface-hover);
    }
    
    .language-menu-item.active {
      background-color: var(--primary-color);
      color: var(--primary-contrast-color);
      font-weight: 500;
    }
    
    .language-menu-item i {
      font-size: 0.875rem;
      width: 1rem;
      display: flex;
      align-items: center;
      justify-content: center;
    }
    
    .language-menu-item span {
      flex: 1;
    }
  `]
})
export class LanguageSelectorMenuComponent implements OnInit {
  @Input() reloadFullConfig: boolean = true;

  private localizationService = inject(LocalizationService);
  private languageSwitch = inject(ApplicationLanguageSwitchService);

  languages = signal<LanguageInfo[]>([]);
  currentCulture = signal<string>('');

  ngOnInit() {
    // Load languages
    this.localizationService.getLanguages$().subscribe((languages) => {
      this.languages.set(languages);
    });

    // Set current language
    this.localizationService.getCurrentCulture$().subscribe((culture) => {
      this.currentCulture.set(culture);
    });
  }

  onLanguageSelect(cultureName: string) {
    if (!cultureName || cultureName === this.currentCulture()) {
      return;
    }
    this.languageSwitch.switchToCulture(cultureName, this.reloadFullConfig);
  }
}
