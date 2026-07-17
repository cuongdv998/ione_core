import {
  Component,
  Input,
  Output,
  EventEmitter,
  forwardRef,
  ChangeDetectionStrategy,
  ChangeDetectorRef,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';
import { SelectModule } from 'primeng/select';

export interface ComboboxColumn {
  field: string;
  header: string;
  /** Numeric percentage width, e.g. 35 → "35%" */
  width: number | string;
}

export interface ComboboxOption {
  label: string;
  value: string;
  [key: string]: unknown;
}

@Component({
  selector: 'app-multicolumn-combobox',
  standalone: true,
  host: { style: 'display: contents' },
  imports: [CommonModule, FormsModule, SelectModule],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => MulticolumnComboboxComponent),
      multi: true,
    },
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <p-select
      [(ngModel)]="internalValue"
      (ngModelChange)="onInternalChange($event)"
      [options]="options"
      [placeholder]="placeholder"
      [showClear]="showClear"
      [filter]="filter"
      [filterBy]="filterBy"
      (onFilter)="onFilter.emit($event)"
      [disabled]="isDisabled"
      optionLabel="label"
      optionValue="value"
      [styleClass]="styleClass"
      [appendTo]="appendTo"
      [required]="required"
      [panelStyle]="resolvedPanelStyle">
      <ng-template #header>
        <div class="flex px-3 py-2 border-b border-surface-200 dark:border-surface-700 text-xs font-semibold text-surface-500 dark:text-surface-400">
          <span *ngFor="let col of columns" [style.width.%]="col.width" class="truncate pr-2">{{ col.header }}</span>
        </div>
      </ng-template>
      <ng-template #item let-option>
        <div class="flex w-full text-sm">
          <span *ngFor="let col of columns" [style.width.%]="col.width" class="truncate pr-2">{{ option[col.field] }}</span>
        </div>
      </ng-template>
    </p-select>
  `,
})
export class MulticolumnComboboxComponent implements ControlValueAccessor {
  @Input() options: ComboboxOption[] = [];
  @Input() columns: ComboboxColumn[] = [];
  @Input() placeholder = '';
  @Input() showClear = false;
  @Input() filter = true;
  @Input() filterBy = 'label';
  @Input() styleClass = '';
  @Input() appendTo: string | undefined = undefined;
  @Input() required = false;
  @Input() panelMinWidth = '680px';
  @Input() set panelStyle(val: Record<string, string> | undefined) {
    this._customPanelStyle = val;
  }

  @Output() onFilter = new EventEmitter<{ filter?: string; value?: string }>();

  internalValue: string | null = null;
  isDisabled = false;

  private _customPanelStyle: Record<string, string> | undefined;
  private _onChange: (value: string | null) => void = () => {};
  private _onTouched: () => void = () => {};

  constructor(private cdr: ChangeDetectorRef) {}

  get resolvedPanelStyle(): Record<string, string> {
    return this._customPanelStyle ?? { 'min-width': this.panelMinWidth };
  }

  onInternalChange(value: string | null): void {
    this._onChange(value);
    this._onTouched();
  }

  writeValue(value: string | null): void {
    this.internalValue = value;
    this.cdr.markForCheck();
  }

  registerOnChange(fn: (value: string | null) => void): void {
    this._onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this._onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.isDisabled = isDisabled;
    this.cdr.markForCheck();
  }
}
