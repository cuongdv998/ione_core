import { Component, OnInit, OnChanges, SimpleChanges, Input, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SelectModule } from 'primeng/select';
import { LocalizationService } from '../../services/localization.service';
import type { LanguageInfo } from '../../../proxy/volo/abp/localization/models';
import { ApplicationLanguageSwitchService } from '../../services/application-language-switch.service';

interface LanguageOption {
  label: string;
  value: string;
  cultureName: string;
}

/**
 * Component để chọn ngôn ngữ
 * Tương tự ABP LanguageSelector
 */
@Component({
  selector: 'app-language-selector',
  standalone: true,
  imports: [CommonModule, FormsModule, SelectModule],
  template: `
    <p-select
      [options]="languageOptions()"
      [(ngModel)]="selectedLanguage"
      optionLabel="label"
      optionValue="value"
      (onChange)="onLanguageChange($event)"
      [style]="style()"
      [showClear]="false"
      [placeholder]="placeholderSignal()"
      [fluid]="fluidSignal()"
      [size]="sizeSignal()"
    ></p-select>
  `,
})
export class LanguageSelectorComponent implements OnInit, OnChanges {
  @Input() width: string = '150px';
  @Input() placeholder: string = 'Select Language';
  @Input() fluid: boolean = false;
  @Input() size: 'small' | 'large' = 'large';
  @Input() reloadFullConfig: boolean = true; // Reload full application configuration khi đổi ngôn ngữ

  private localizationService = inject(LocalizationService);
  private languageSwitch = inject(ApplicationLanguageSwitchService);

  languages = signal<LanguageInfo[]>([]);
  languageOptions = signal<LanguageOption[]>([]);
  selectedLanguage: string = '';

  style = signal<{ width?: string }>({});
  placeholderSignal = signal<string>('Select Language');
  fluidSignal = signal<boolean>(false);
  sizeSignal = signal<'small' | 'large'>('large');

  ngOnChanges(changes: SimpleChanges) {
    // Update signals when inputs change
    if (changes['width']) {
      this.style.set({ width: this.width });
    }
    if (changes['placeholder']) {
      this.placeholderSignal.set(this.placeholder || 'Select Language');
    }
    if (changes['fluid']) {
      this.fluidSignal.set(this.fluid);
    }
    if (changes['size']) {
      this.sizeSignal.set(this.size);
    }
  }

  ngOnInit() {
    // Set initial values
    this.style.set({ width: this.width });
    this.placeholderSignal.set(this.placeholder || 'Select Language');
    this.fluidSignal.set(this.fluid);
    this.sizeSignal.set(this.size);

    // Load languages
    this.localizationService.getLanguages$().subscribe((languages) => {
      this.languages.set(languages);
      this.languageOptions.set(
        languages.map((lang) => ({
          label: lang.displayName || lang.cultureName || '',
          value: lang.cultureName || '',
          cultureName: lang.cultureName || '',
        }))
      );
    });

    // Set current language
    this.localizationService.getCurrentCulture$().subscribe((culture) => {
      this.selectedLanguage = culture;
    });
  }

  onLanguageChange(event: { value: string }) {
    const cultureName = event.value;
    if (!cultureName) {
      return;
    }
    this.languageSwitch.switchToCulture(cultureName, this.reloadFullConfig);
  }
}
