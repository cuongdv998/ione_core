import { Directive, Input, ElementRef, OnInit, OnDestroy, inject } from '@angular/core';
import { Subscription } from 'rxjs';
import { LocalizationService } from '../services/localization.service';

/**
 * Directive để translate text content của element
 * 
 * Usage:
 * <span [appTranslate]="'Welcome'"></span>
 * <span [appTranslate]="'AbpAccount::Welcome'"></span>
 * <span [appTranslate]="'Welcome'" [defaultValue]="'Hello'"></span>
 */
@Directive({
  selector: '[appTranslate]',
  standalone: true,
})
export class TranslateDirective implements OnInit, OnDestroy {
  private el = inject(ElementRef);
  private localizationService = inject(LocalizationService);
  
  private subscription?: Subscription;
  private key: string = '';
  private _defaultValue?: string;

  @Input() set appTranslate(key: string) {
    this.key = key;
    this.updateTranslation();
  }

  @Input() set defaultValue(value: string | undefined) {
    this._defaultValue = value;
    this.updateTranslation();
  }

  ngOnInit() {
    this.updateTranslation();
  }

  ngOnDestroy() {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  private updateTranslation() {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }

    if (!this.key) {
      return;
    }

    // Set translation ngay lập tức
    const translation = this.localizationService.localize(this.key, this._defaultValue);
    this.el.nativeElement.textContent = translation;

    // Subscribe để update khi culture thay đổi
    this.subscription = this.localizationService
      .localize$(this.key, this._defaultValue)
      .subscribe((translation) => {
        this.el.nativeElement.textContent = translation;
      });
  }
}
