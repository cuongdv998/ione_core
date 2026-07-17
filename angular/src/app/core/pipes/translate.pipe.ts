import { Pipe, PipeTransform, inject, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { LocalizationService } from '../services/localization.service';
import { Subscription } from 'rxjs';

/**
 * Pipe để translate trong templates
 * 
 * Usage:
 * {{ 'Welcome' | translate }}
 * {{ 'AbpAccount::Welcome' | translate }}
 * {{ 'Welcome' | translate:'Hello' }} // với defaultValue
 */
@Pipe({
  name: 'translate',
  standalone: true,
  pure: false, // Cần pure: false để pipe reactive với changes
})
export class TranslatePipe implements PipeTransform, OnDestroy {
  private localizationService = inject(LocalizationService);
  private cdr = inject(ChangeDetectorRef);
  private subscription?: Subscription;
  private lastKey: string | null = null;
  private lastDefaultValue: string | undefined = undefined;
  private result: string = '';

  transform(key: string, defaultValue?: string): string {
    // Nếu giá trị không thay đổi, trả về kết quả cached
    if (
      this.lastKey === key &&
      this.lastDefaultValue === defaultValue &&
      this.subscription
    ) {
      return this.result;
    }

    // Unsubscribe subscription cũ
    if (this.subscription) {
      this.subscription.unsubscribe();
    }

    this.lastKey = key;
    this.lastDefaultValue = defaultValue;

    // Lấy translation ngay lập tức
    this.result = this.localizationService.localize(key, defaultValue);

    // Subscribe để lắng nghe changes (khi culture thay đổi)
    this.subscription = this.localizationService
      .localize$(key, defaultValue)
      .subscribe((translation) => {
        this.result = translation;
        this.cdr.markForCheck();
      });

    return this.result;
  }

  ngOnDestroy() {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }
}
