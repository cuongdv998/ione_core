import { Component, Input, Output, EventEmitter, forwardRef, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { ListboxModule } from 'primeng/listbox';
import { InputTextModule } from 'primeng/inputtext';
import { TooltipModule } from 'primeng/tooltip';

export interface DualListboxOption {
  label: string;
  value: string;
  disabled?: boolean;
}

@Component({
  selector: 'app-v-dual-listbox',
  standalone: true,
  imports: [CommonModule, FormsModule, ButtonModule, ListboxModule, InputTextModule, TooltipModule],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => VDualListboxComponent),
      multi: true
    }
  ],
  templateUrl: './v-dual-listbox.component.html',
  styleUrl: './v-dual-listbox.component.scss'
})
export class VDualListboxComponent implements ControlValueAccessor, OnInit, OnChanges {
  @Input() availableOptions: DualListboxOption[] = [];
  @Input() selectedOptions: DualListboxOption[] = [];
  @Input() options: DualListboxOption[] = []; // Alternative: pass all options and let component manage separation
  @Input() availableLabel: string = 'Available';
  @Input() selectedLabel: string = 'Selected';
  @Input() disabled: boolean = false;
  @Input() filter: boolean = false;
  @Input() styleClass: string = '';
  @Input() listStyle: { [key: string]: string | undefined } = { height: '300px' };

  @Output() selectionChange = new EventEmitter<DualListboxOption[]>();

  availableFilter: string = '';
  selectedFilter: string = '';
  // PrimeNG listbox with optionValue returns string[] not DualListboxOption[]
  selectedAvailableItems: (DualListboxOption | string)[] = [];
  selectedSelectedItems: (DualListboxOption | string)[] = [];

  private onChange = (value: string[]) => {};
  private onTouched = () => {};
  private _allOptions: DualListboxOption[] = [];
  private _currentValue: string[] = [];

  get filteredAvailableOptions(): DualListboxOption[] {
    if (!this.availableFilter) {
      return this.availableOptions;
    }
    const filter = this.availableFilter.toLowerCase();
    return this.availableOptions.filter(opt => 
      opt.label.toLowerCase().includes(filter)
    );
  }

  get filteredSelectedOptions(): DualListboxOption[] {
    if (!this.selectedFilter) {
      return this.selectedOptions;
    }
    const filter = this.selectedFilter.toLowerCase();
    return this.selectedOptions.filter(opt => 
      opt.label.toLowerCase().includes(filter)
    );
  }

  moveToSelected(): void {
    if (this.selectedAvailableItems.length === 0) return;
    
    // PrimeNG listbox with optionValue returns string[] not DualListboxOption[]
    // Convert selected values to option objects
    const selectedValues = this.selectedAvailableItems.map(item => 
      typeof item === 'string' ? item : item.value
    );
    const itemsToMove = this.availableOptions.filter(opt => 
      selectedValues.includes(opt.value)
    );
    
    if (itemsToMove.length === 0) return;
    
    this.selectedOptions.push(...itemsToMove);
    this.availableOptions = this.availableOptions.filter(
      opt => !selectedValues.includes(opt.value)
    );
    this.selectedAvailableItems = [];
    this.emitChange();
  }

  moveToAvailable(): void {
    if (this.selectedSelectedItems.length === 0) return;
    
    // PrimeNG listbox with optionValue returns string[] not DualListboxOption[]
    // Convert selected values to option objects
    const selectedValues = this.selectedSelectedItems.map(item => 
      typeof item === 'string' ? item : item.value
    );
    const itemsToMove = this.selectedOptions.filter(opt => 
      selectedValues.includes(opt.value)
    );
    
    if (itemsToMove.length === 0) return;
    
    this.availableOptions.push(...itemsToMove);
    this.selectedOptions = this.selectedOptions.filter(
      opt => !selectedValues.includes(opt.value)
    );
    this.selectedSelectedItems = [];
    this.emitChange();
  }

  moveAllToSelected(): void {
    if (this.availableOptions.length === 0) return;
    
    this.selectedOptions.push(...this.availableOptions);
    this.availableOptions = [];
    this.selectedAvailableItems = [];
    this.emitChange();
  }

  moveAllToAvailable(): void {
    if (this.selectedOptions.length === 0) return;
    
    this.availableOptions.push(...this.selectedOptions);
    this.selectedOptions = [];
    this.selectedSelectedItems = [];
    this.emitChange();
  }

  private emitChange(): void {
    const values = this.selectedOptions.map(opt => opt.value);
    this.onChange(values);
    this.selectionChange.emit(this.selectedOptions);
  }

  ngOnInit(): void {
    this.initializeOptions();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['availableOptions'] || changes['selectedOptions'] || changes['options']) {
      this.initializeOptions();
    }
  }

  private initializeOptions(): void {
    // If options input is provided, use it; otherwise combine available and selected
    let allOptions: DualListboxOption[] = [];
    if (this.options && this.options.length > 0) {
      allOptions = [...this.options];
    } else {
      allOptions = [...this.availableOptions, ...this.selectedOptions];
    }
    
    if (allOptions.length > 0) {
      this._allOptions = allOptions;
      // If we have a current value, apply it
      if (this._currentValue && this._currentValue.length > 0) {
        this.applyValue(this._currentValue);
      } else {
        // Otherwise, use the current selectedOptions to determine what's selected
        const selectedValues = this.selectedOptions.map(opt => opt.value);
        if (selectedValues.length > 0) {
          this.applyValue(selectedValues);
        }
      }
    }
  }

  private applyValue(value: string[]): void {
    if (!this._allOptions || this._allOptions.length === 0) return;
    
    // Separate available and selected based on value array
    this.selectedOptions = this._allOptions.filter(opt => 
      value.includes(opt.value)
    );
    this.availableOptions = this._allOptions.filter(opt => 
      !value.includes(opt.value)
    );
  }

  // ControlValueAccessor implementation
  writeValue(value: string[]): void {
    this._currentValue = value || [];
    // If options are already loaded, apply the value immediately
    if (this._allOptions && this._allOptions.length > 0) {
      this.applyValue(this._currentValue);
    } else {
      // Otherwise, wait for options to be loaded via initializeOptions
      const allOptions = [...this.availableOptions, ...this.selectedOptions];
      if (allOptions.length > 0) {
        this._allOptions = allOptions;
        this.applyValue(this._currentValue);
      }
    }
  }

  registerOnChange(fn: (value: string[]) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }
}
