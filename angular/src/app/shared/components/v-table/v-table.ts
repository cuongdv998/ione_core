import {
  Component,
  Input,
  Output,
  EventEmitter,
  OnInit,
  OnChanges,
  OnDestroy,
  SimpleChanges,
  AfterViewInit,
  NgZone,
  QueryList,
  ViewChild,
  ViewChildren,
  Inject,
  PLATFORM_ID
} from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { Table, TableLazyLoadEvent, TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { MenuItem } from 'primeng/api';
import { Menu, MenuModule } from 'primeng/menu';
import { TooltipModule } from 'primeng/tooltip';
import { MultiSelectModule } from 'primeng/multiselect';
import { Popover } from 'primeng/popover';
import { CheckboxModule } from 'primeng/checkbox';
import { FormsModule } from '@angular/forms';
import { LocalizationService } from '@/core/services/localization.service';

import { TableColumn } from '../../models/table-column.model';
import { TableAction } from '../../models/table-action.model';

@Component({
  selector: 'app-v-table',
  imports: [
    CommonModule,
    TableModule,
    ButtonModule,
    MenuModule,
    TooltipModule,
    MultiSelectModule,
    Popover,
    CheckboxModule,
    FormsModule
  ],
  templateUrl: './v-table.html',
  styleUrl: './v-table.scss'
})
export class VTable implements OnInit, OnChanges, AfterViewInit, OnDestroy {
  constructor(
    private localizationService: LocalizationService,
    private ngZone: NgZone,
    @Inject(PLATFORM_ID) private platformId: object
  ) {}

  @ViewChild('dt') private dtRef?: Table;

  /** Debounce nhiều thay đổi cùng tick (loading + data + lazyLoad). */
  private scrollLayoutRefreshTimer: ReturnType<typeof setTimeout> | null = null;

  selectedColumns: TableColumn[] = [];
  columnOptions: TableColumn[] = [];

  @Input() columns: TableColumn[] = [];
  @Input() data: any[] = [];
  @Input() actions: TableAction[] | null = null;
  /** 'menu' = dropdown ellipsis, 'buttons' = separate icon buttons per action */
  @Input() actionDisplay: 'menu' | 'buttons' = 'menu';

  @Input() totalCount = 0;
  @Input() loading = true;
  @Input() rows = 10;
  @Input() rowsPerPageOptions = [10, 20, 50, 100];
  @Input() showCurrentPageReport = true;
  @Input() currentPageReportTemplate = 'Hiển thị {first} đến {last} trong tổng số {totalRecords} bản ghi';
  @Input() showGridlines = true;
  @Input() stripedRows = true;
  @Input() rowHover = true;
  @Input() size: 'small' | 'normal' | 'large' = 'small';
  @Input() actionLabelKey = 'iOne::Actions';
  @Input() scrollable = true;
  /**
   * Chiều cao vùng cuộn dọc (vd. '400px', 'flex'). Để trống = cố định ~10 dòng dữ liệu (theo rem, khớp header 3rem trong SCSS).
   */
  @Input() scrollHeight: string | undefined = undefined;
  /** Số dòng body dùng khi tính chiều cao mặc định (khi scrollHeight không set). Mặc định 10. */
  @Input() bodyViewportRows = 10;
  @Input() tableMinWidth = '1200px';
  @Input() tableLayout: 'auto' | 'fixed' = 'auto';
  @Input() showIndex = true;      
  @Input() indexHeader = 'STT';    
  @Input() indexWidth = '60px';
  @Input() showColumnToggle = true;
  @Input() columnTogglePlaceholder = 'Chọn cột hiển thị';
  @Input() columnToggleTooltip = 'Tùy chỉnh hiển thị cột';
  @Input() resizableColumns = true;
  @Input() columnResizeMode: 'fit' | 'expand' = 'expand';
  @Input() rowClickable = false;
  @Input() selectionMode: 'single' | 'multiple' | null = null;
  @Input() metaKeySelection = false;
  /** Selected row(s); use with selectionMode. Two-way: [(selection)]="selectedRows" */
  @Input() selection: any;
  @Output() selectionChange = new EventEmitter<any>();
  /** When true, show custom checkbox column instead of index (STT); only selectable rows are enabled. Do not use with selectionMode. */
  @Input() useCustomSelectionColumn = false;
  /** Used with useCustomSelectionColumn: row is selectable (checkbox enabled) when this returns true. */
  @Input() isRowSelectable: ((row: any) => boolean) | null = null;

  @Output() lazyLoad = new EventEmitter<TableLazyLoadEvent>();
  @Output() edit = new EventEmitter<any>();
  @Output() delete = new EventEmitter<any>();
  @Output() rowClick = new EventEmitter<any>();

  @ViewChildren(Menu) private actionMenus!: QueryList<Menu>;

  first = 0;

  ngOnInit() {
    // Initialize column options and selected columns
    this.columnOptions = [...this.columns];
    this.selectedColumns = [...this.columns];
  }

  ngAfterViewInit(): void {
    if (this.scrollable) {
      this.scheduleScrollableLayoutRefresh();
    }
  }

  ngOnDestroy(): void {
    if (this.scrollLayoutRefreshTimer != null) {
      clearTimeout(this.scrollLayoutRefreshTimer);
      this.scrollLayoutRefreshTimer = null;
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (!this.scrollable) {
      return;
    }
    const loadingTurnedOff =
      changes['loading'] &&
      changes['loading'].currentValue === false &&
      changes['loading'].previousValue !== changes['loading'].currentValue;
    const dataChanged =
      changes['data'] &&
      changes['data'].previousValue !== changes['data'].currentValue &&
      this.loading === false;

    if (loadingTurnedOff || dataChanged) {
      this.scheduleScrollableLayoutRefresh();
    }
  }

  /**
   * PrimeNG scrollable + pFrozenColumn đôi khi tính lại vị trí cột đông trễ sau khi có dữ liệu.
   * Gợi ý layout bằng scroll/resize sau khi DOM ổn định (pattern tương tự issue virtual scroll + frozen).
   */
  private scheduleScrollableLayoutRefresh(): void {
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }
    if (this.scrollLayoutRefreshTimer != null) {
      clearTimeout(this.scrollLayoutRefreshTimer);
    }
    this.scrollLayoutRefreshTimer = setTimeout(() => {
      this.scrollLayoutRefreshTimer = null;
      requestAnimationFrame(() => {
        requestAnimationFrame(() => this.flushScrollableLayoutRefresh());
      });
    }, 0);
  }

  private flushScrollableLayoutRefresh(): void {
    const dt = this.dtRef;
    if (!dt?.scrollable || !isPlatformBrowser(this.platformId)) {
      return;
    }

    this.ngZone.runOutsideAngular(() => {
      try {
        const wrap = dt.wrapperViewChild?.nativeElement as HTMLElement | undefined;
        if (wrap) {
          const top = wrap.scrollTop;
          const left = wrap.scrollLeft;
          if (typeof wrap.scrollTo === 'function') {
            wrap.scrollTo({ top, left });
          }
          wrap.dispatchEvent(new Event('scroll'));
        } else {
          dt.scrollTo({ top: 0, left: 0 });
        }
      } catch {
        // ignore
      }
      window.dispatchEvent(new Event('resize'));
    });
  }

  onLazyLoad(event: TableLazyLoadEvent) {
    this.first = event.first ?? 0;
    this.lazyLoad.emit(event);
    if (this.scrollable) {
      this.scheduleScrollableLayoutRefresh();
    }
  }

  onRowSelect(event: any) {
    if (this.selection !== undefined) {
      this.selectionChange.emit(this.selection);
    }
    const row = event?.data || event;
    if (row) {
      this.rowClick.emit(row);
    }
  }

  onSelectionChange(value: any): void {
    this.selectionChange.emit(value);
  }

  get actionLabel(): string {
    return this.localizationService.localize(this.actionLabelKey);
  }

  /** Close any other row action menu before toggling this one (only one overlay visible). */
  toggleRowActionMenu(menu: Menu, event: Event): void {
    this.actionMenus?.forEach((m) => {
      if (m !== menu) {
        m.hide();
      }
    });
    menu.toggle(event);
  }

  /**
   * Handle menu item activation as early as possible (mousedown),
   * because in some layouts the click event can be consumed by overlay hide,
   * row selection, or document listeners - leading to "must click twice".
   */
  onMenuItemPointerDown(menu: Menu, item: any, event: Event): void {
    try {
      (event as any)?.preventDefault?.();
      (event as any)?.stopPropagation?.();
    } catch {
      // ignore
    }

    const row = item?.__row;
    const action: TableAction | undefined = item?.__action;

    if (!row || !action) {
      return;
    }

    // Skip if this is a separator (no command)
    if (action.separator || !action.command) {
      return;
    }

    const isDisabled = action.disabled ? action.disabled(row) : false;
    if (isDisabled) {
      return;
    }

    // Close menu immediately on selection
    menu?.hide();

    // Defer to next tick to let overlay finish closing cleanly
    setTimeout(() => {
      this.ngZone.run(() => action.command!(row));
    }, 0);
  }

  get leftFrozenColumns(): TableColumn[] {
    return this.columns.filter(c => 
      c.freeze === 'left' && this.isColumnSelected(c)
    );
  }

  get normalColumns(): TableColumn[] {
    return this.columns.filter(c => 
      !c.freeze && this.isColumnSelected(c)
    );
  }

  get rightFrozenColumns(): TableColumn[] {
    return this.columns.filter(c => 
      c.freeze === 'right' && this.isColumnSelected(c)
    );
  }

  isColumnSelected(column: TableColumn): boolean {
    return this.selectedColumns.some(c => c.field === column.field);
  }

  buildMenuItems(row: any): MenuItem[] {
    if (!this.actions || !row) {
      return [];
    }

    // We attach the original TableAction onto the MenuItem so the template can
    // handle click reliably (PrimeNG command sometimes doesn't fire on first click
    // due to overlay close/focus handling).
    return this.actions
      .filter(a => (a.visible ? a.visible(row) : true))
      .map(a => {
        // Handle separator
        if (a.separator) {
          return {
            separator: true
          } as any;
        }
        
        return {
          label: a.label,
          icon: a.icon,
          disabled: a.disabled ? a.disabled(row) : false,
          __action: a,
          __row: row
        } as any;
      });
  }

  getColumnAlign(col: TableColumn): 'left' | 'center' | 'right' {
    if (col.align) {
      return col.align;
    }
  
    switch (col.type) {
      case 'number':
        return 'right';
      case 'date':
      case 'boolean':
        return 'center';
      default:
        return 'left';
    }
  }

  /** Chiều cao gửi xuống p-table: mặc định cố định theo `bodyViewportRows` (header 3rem + N × 2.25rem/dòng, sm). */
  get effectiveScrollHeight(): string | undefined {
    if (!this.scrollable) {
      return undefined;
    }
    const custom = this.scrollHeight;
    if (custom != null && String(custom).trim() !== '') {
      return custom;
    }
    const n = Math.max(1, Math.min(100, Math.floor(this.bodyViewportRows)));
    return `calc(3rem + ${n} * 2.25rem)`;
  }

  /** Effective selection mode passed to p-table: null when using custom selection column. */
  get effectiveSelectionMode(): 'single' | 'multiple' | null {
    return this.useCustomSelectionColumn ? null : this.selectionMode;
  }

  get selectableRowsOnPage(): any[] {
    if (!this.data || !this.isRowSelectable) return [];
    return this.data.filter((row: any) => this.isRowSelectable!(row));
  }

  isRowSelected(row: any): boolean {
    const sel = this.selection;
    if (!Array.isArray(sel)) return false;
    return sel.includes(row);
  }

  get customHeaderCheckboxChecked(): boolean {
    const selectable = this.selectableRowsOnPage;
    return selectable.length > 0 && selectable.every((r: any) => this.isRowSelected(r));
  }

  get customHeaderCheckboxIndeterminate(): boolean {
    const selectable = this.selectableRowsOnPage;
    if (selectable.length === 0) return false;
    const selectedCount = selectable.filter((r: any) => this.isRowSelected(r)).length;
    return selectedCount > 0 && selectedCount < selectable.length;
  }

  onCustomHeaderCheckboxChange(checked: boolean): void {
    const selectable = this.selectableRowsOnPage;
    const current = Array.isArray(this.selection) ? [...this.selection] : [];
    if (checked) {
      const merged = [...current];
      for (const row of selectable) {
        if (!merged.includes(row)) merged.push(row);
      }
      this.selectionChange.emit(merged);
    } else {
      const set = new Set(selectable);
      this.selectionChange.emit(current.filter((r: any) => !set.has(r)));
    }
  }

  onCustomRowCheckboxChange(row: any, checked: boolean): void {
    if (!this.isRowSelectable?.(row)) return;
    const current = Array.isArray(this.selection) ? [...this.selection] : [];
    if (checked) {
      if (!current.includes(row)) this.selectionChange.emit([...current, row]);
    } else {
      this.selectionChange.emit(current.filter((r: any) => r !== row));
    }
  }
}
