import { Component, ElementRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { LayoutService } from '../service/layout.service';
import { AppMenu } from './app.menu';

@Component({
    selector: 'app-sidebar',
    standalone: true,
    imports: [AppMenu, CommonModule, RouterModule],
    template: ` <div class="layout-sidebar">
        <div class="layout-sidebar-header" *ngIf="!layoutService.isOverlay()">
            <button class="layout-sidebar-anchor layout-topbar-action" (click)="layoutService.onMenuToggle()">
                <i class="pi pi-bars"></i>
            </button>
            <a class="layout-sidebar-logo" routerLink="/">
                <img src="/assets/images/ione-logo-login.png" alt="iOne Logo" />
            </a>
        </div>
        <div class="layout-menu-container">
            <app-menu></app-menu>
        </div>
    </div>`
})
export class AppSidebar {
    layoutService = inject(LayoutService);
    constructor(public el: ElementRef) {}
}
