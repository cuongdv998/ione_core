import {Component, computed, inject, input} from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { StyleClassModule } from 'primeng/styleclass';
import { AppConfigurator } from './app.configurator';
import { LayoutService } from '../service/layout.service';
import {CommonModule} from "@angular/common";

@Component({
    selector: 'app-floating-configurator',
    imports: [CommonModule, ButtonModule, StyleClassModule, AppConfigurator],
    template: `
        @if (hideUi()) {
            <!-- Chỉ ẩn UI: vẫn mount AppConfigurator để onPresetChange / theme khớp app (tránh mặc định xanh Aura) -->
            <div class="sr-only" aria-hidden="true">
                <app-configurator />
            </div>
        } @else {
            <div class="flex gap-4 top-8 right-8" [ngClass]="{ fixed: float() }">
                <p-button type="button" (onClick)="toggleDarkMode()" [rounded]="true" [icon]="isDarkTheme() ? 'pi pi-moon' : 'pi pi-sun'" severity="secondary" />
                <div class="relative">
                    <p-button icon="pi pi-palette" pStyleClass="@next" enterFromClass="hidden" enterActiveClass="animate-scalein" leaveToClass="hidden" leaveActiveClass="animate-fadeout" [hideOnOutsideClick]="true" type="button" rounded />
                    <app-configurator />
                </div>
            </div>
        }
    `
})
export class AppFloatingConfigurator {
    LayoutService = inject(LayoutService);

    float = input<boolean>(true);

    /** true: không hiện nút sáng/tối & palette; vẫn chạy logic theme (dùng trên login) */
    hideUi = input(false);

    isDarkTheme = computed(() => this.LayoutService.layoutConfig().darkTheme);

    toggleDarkMode() {
        this.LayoutService.layoutConfig.update((state) => ({ ...state, darkTheme: !state.darkTheme }));
    }

}
