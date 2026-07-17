import { Component, HostBinding, Input, OnInit, OnDestroy } from '@angular/core';
import { NavigationEnd, Router, RouterModule } from '@angular/router';
import { animate, state, style, transition, trigger } from '@angular/animations';
import { Subscription } from 'rxjs';
import { filter } from 'rxjs/operators';
import { CommonModule } from '@angular/common';
import { RippleModule } from 'primeng/ripple';
import { TooltipModule } from 'primeng/tooltip';
import { MenuItem } from 'primeng/api';
import { LayoutService } from '../service/layout.service';

@Component({
    // eslint-disable-next-line @angular-eslint/component-selector
    selector: '[app-menuitem]',
    imports: [CommonModule, RouterModule, RippleModule, TooltipModule],
    template: `
        <ng-container>
            <!-- Root menu with submenu: show as text header with toggle -->
            <div *ngIf="root && item.visible !== false && item.items" 
                 class="layout-menuitem-root-text" 
                 [class.active-menuitem]="active"
                 (click)="itemClick($event)"
                 [pTooltip]="item.label"
                 tooltipPosition="right"
                 [tooltipDisabled]="!layoutService.layoutState().staticMenuDesktopInactive">
                <div class="layout-menuitem-root-content">
                <i *ngIf="item.icon" [ngClass]="item.icon" class="layout-menuitem-icon"></i>
                <span class="layout-menuitem-text">{{ item.label }}</span>
                </div>
                <i class="pi pi-fw pi-angle-down layout-submenu-toggler" 
                   [ngClass]="{'rotated': expanded}"
                   [style.transform]="expanded ? 'rotate(180deg)' : 'rotate(0deg)'"></i>
            </div>
            
            <!-- Root menu without submenu: show as regular link -->
            <a *ngIf="root && item.visible !== false && !item.items && item.routerLink"
                (click)="itemClick($event)"
                [ngClass]="item.styleClass"
                [routerLink]="item.routerLink"
                routerLinkActive="active-route"
                [routerLinkActiveOptions]="item.routerLinkActiveOptions || { paths: 'exact', queryParams: 'ignored', matrixParams: 'ignored', fragment: 'ignored' }"
                [fragment]="item.fragment"
                [queryParamsHandling]="item.queryParamsHandling"
                [preserveFragment]="item.preserveFragment"
                [skipLocationChange]="item.skipLocationChange"
                [replaceUrl]="item.replaceUrl"
                [state]="item.state"
                [queryParams]="item.queryParams"
                [attr.target]="item.target"
                tabindex="0"
                pRipple
                [pTooltip]="item.label"
                tooltipPosition="right"
                [tooltipDisabled]="!layoutService.layoutState().staticMenuDesktopInactive"
            >
                <i [ngClass]="item.icon" class="layout-menuitem-icon"></i>
                <span class="layout-menuitem-text">{{ item.label }}</span>
            </a>
            
            <!-- Non-root menu with submenu or url: show as link with toggle -->
            <a *ngIf="!root && item.visible !== false && (!item.routerLink || item.items)" 
                [attr.href]="item.url" 
                (click)="itemClick($event)" 
                [ngClass]="item.styleClass" 
                [attr.target]="item.target" 
                tabindex="0" 
                pRipple
                [class.active-menuitem]="active"
                [pTooltip]="item.label"
                tooltipPosition="right"
                [tooltipDisabled]="!layoutService.layoutState().staticMenuDesktopInactive"
            >
                <span class="layout-menuitem-text">{{ item.label }}</span>
                <i *ngIf="item.items" 
                   class="pi pi-fw pi-angle-down layout-submenu-toggler"
                   [style.transform]="expanded ? 'rotate(180deg)' : 'rotate(0deg)'"></i>
            </a>
            
            <!-- Non-root menu without submenu: show as router link -->
            <a *ngIf="!root && item.visible !== false && item.routerLink && !item.items"
                (click)="itemClick($event)"
                [ngClass]="item.styleClass"
                [routerLink]="item.routerLink"
                routerLinkActive="active-route"
                [routerLinkActiveOptions]="item.routerLinkActiveOptions || { paths: 'exact', queryParams: 'ignored', matrixParams: 'ignored', fragment: 'ignored' }"
                [fragment]="item.fragment"
                [queryParamsHandling]="item.queryParamsHandling"
                [preserveFragment]="item.preserveFragment"
                [skipLocationChange]="item.skipLocationChange"
                [replaceUrl]="item.replaceUrl"
                [state]="item.state"
                [queryParams]="item.queryParams"
                [attr.target]="item.target"
                tabindex="0"
                pRipple
                [pTooltip]="item.label"
                tooltipPosition="right"
                [tooltipDisabled]="!layoutService.layoutState().staticMenuDesktopInactive"
            >
                <span class="layout-menuitem-text">{{ item.label }}</span>
            </a>

            <!-- Submenu list -->
            <ul *ngIf="item.items && item.visible !== false" [@children]="submenuAnimation" style="overflow: hidden;">
                <ng-template ngFor let-child let-i="index" [ngForOf]="item.items">
                    <li app-menuitem [item]="child" [index]="i" [parentKey]="key" [class]="child['badgeClass']"></li>
                </ng-template>
            </ul>
        </ng-container>
    `,
    animations: [
        trigger('children', [
            state(
                'collapsed',
                style({
                    height: '0px',
                    maxHeight: '0px',
                    overflow: 'hidden',
                    opacity: '0',
                    marginTop: '0px',
                    marginBottom: '0px',
                    paddingTop: '0px',
                    paddingBottom: '0px'
                })
            ),
            state(
                'expanded',
                style({
                    height: '*',
                    maxHeight: '5000px',
                    overflow: 'visible',
                    opacity: '1'
                })
            ),
            transition('collapsed => expanded', [
                animate('400ms cubic-bezier(0.86, 0, 0.07, 1)')
            ]),
            transition('expanded => collapsed', [
                animate('400ms cubic-bezier(0.86, 0, 0.07, 1)')
            ])
        ])
    ]
})
export class AppMenuitem implements OnInit, OnDestroy {
    @Input() item!: MenuItem;

    @Input() index!: number;

    @Input() @HostBinding('class.layout-root-menuitem') root!: boolean;

    @Input() parentKey!: string;

    active = false;

    expanded = false;

    menuSourceSubscription: Subscription;

    menuResetSubscription: Subscription;

    key: string = '';

    constructor(
        public router: Router,
        public layoutService: LayoutService
    ) {
        this.menuSourceSubscription = this.layoutService.menuSource$.subscribe((value) => {
            Promise.resolve(null).then(() => {
                if (value.routeEvent) {
                    this.active = value.key === this.key || value.key.startsWith(this.key + '-') ? true : false;
                    // Tự động mở danh mục cha nếu có con đang active route
                    if (this.active && this.item.items) {
                        this.expanded = true;
                    }
                    // Accordion: thu các nhóm root không chứa route hiện tại
                    if (this.root && this.item.items && !this.active) {
                        this.expanded = false;
                    }
                } else if (value.slimOpenExpandRoot) {
                    if (this.root && this.item.items) {
                        this.expanded = value.key === this.key;
                    }
                } else if (value.rootAccordionToggle) {
                    if (this.root && this.item.items && value.key !== this.key) {
                        this.expanded = false;
                    }
                }
            });
        });

        this.menuResetSubscription = this.layoutService.resetSource$.subscribe(() => {
            this.active = false;
            this.expanded = false;
        });

        this.router.events.pipe(filter((event) => event instanceof NavigationEnd)).subscribe((params) => {
            this.updateActiveStateFromRoute();
        });
    }

    ngOnInit() {
        this.key = this.parentKey ? this.parentKey + '-' + this.index : String(this.index);

        this.updateActiveStateFromRoute();
    }

    updateActiveStateFromRoute() {
        if (this.item.routerLink) {
            const isHome = this.item.routerLink[0] === '/';
            this.active = this.router.isActive(this.item.routerLink[0], { paths: isHome ? 'exact' : 'subset', queryParams: 'ignored', matrixParams: 'ignored', fragment: 'ignored' });
        } else {
            this.active = this.checkChildActive(this.item);
        }

        if (this.active) {
            this.expanded = true;
            this.layoutService.onMenuStateChange({ key: this.key, routeEvent: true });
        }
    }

    checkChildActive(item: MenuItem): boolean {
        if (item.routerLink) {
            const isHome = item.routerLink[0] === '/';
            return this.router.isActive(item.routerLink[0], { paths: isHome ? 'exact' : 'subset', queryParams: 'ignored', matrixParams: 'ignored', fragment: 'ignored' });
        }
        if (item.items) {
            return item.items.some((child) => this.checkChildActive(child));
        }
        return false;
    }

    itemClick(event: Event) {
        // avoid processing disabled items
        if (this.item.disabled) {
            event.preventDefault();
            return;
        }

        // Sidebar icon-only: mở rộng sidebar; nếu là nhóm có submenu thì sau đó mở đúng nhóm đó
        if (this.layoutService.layoutState().staticMenuDesktopInactive) {
            const rootKeyToExpand = this.root && this.item.items ? this.key : null;
            this.layoutService.onMenuToggle();
            event.preventDefault();
            if (rootKeyToExpand !== null) {
                Promise.resolve().then(() => {
                    this.layoutService.onMenuStateChange({
                        key: rootKeyToExpand,
                        slimOpenExpandRoot: true
                    });
                });
            }
            return;
        }

        // execute command
        if (this.item.command) {
            this.item.command({ originalEvent: event, item: this.item });
        }

        // toggle expanded state
        if (this.item.items) {
            this.expanded = !this.expanded;
        }

        this.layoutService.onMenuStateChange({
            key: this.key,
            rootAccordionToggle: !!(this.root && this.item.items)
        });
    }

    get submenuAnimation() {
        return this.expanded ? 'expanded' : 'collapsed';
    }

    @HostBinding('class.active-menuitem')
    get activeClass() {
        return this.active;
    }

    ngOnDestroy() {
        if (this.menuSourceSubscription) {
            this.menuSourceSubscription.unsubscribe();
        }

        if (this.menuResetSubscription) {
            this.menuResetSubscription.unsubscribe();
        }
    }
}
