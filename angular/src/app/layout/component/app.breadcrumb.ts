import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, NavigationEnd, ActivatedRoute, RouterModule } from '@angular/router';
import { filter } from 'rxjs/operators';
import { LocalizationService } from '../../core/services/localization.service';
import { NavigationService, ApplicationMenuItem } from '../../core/services/navigation.service';

interface BreadcrumbItem {
    label: string;
    url: string;
    clickable?: boolean;
}

@Component({
    selector: 'app-breadcrumb',
    standalone: true,
    imports: [CommonModule, RouterModule],
    template: `
        <div class="layout-breadcrumb">
            <div class="breadcrumb-content">
                <!-- Home icon -->
                <a [routerLink]="['/']" class="breadcrumb-home" title="Home">
                    <i class="pi pi-home"></i>
                </a>
                <!-- Breadcrumb items -->
                <ng-container *ngFor="let item of breadcrumbs; let last = last">
                    <span class="breadcrumb-separator">/</span>
                    <a *ngIf="item.clickable && !last" [routerLink]="item.url" class="breadcrumb-link" [title]="item.label">
                        {{ item.label }}
                    </a>
                    <span *ngIf="!item.clickable && !last" style="color: var(--text-color-secondary); font-size: 0.875rem;" [title]="item.label">
                        {{ item.label }}
                    </span>
                    <span *ngIf="last" class="breadcrumb-link breadcrumb-current" [title]="item.label" style="cursor: default;">
                        {{ item.label }}
                    </span>
                </ng-container>
            </div>
        </div>
    `
})
export class AppBreadcrumb implements OnInit {
    private router = inject(Router);
    private activatedRoute = inject(ActivatedRoute);
    private localizationService = inject(LocalizationService);
    private navigationService = inject(NavigationService);

    breadcrumbs: BreadcrumbItem[] = [];
    showHome = true;
    menuItems: ApplicationMenuItem[] = [];
    private readonly routeParentMap: Record<string, string> = {
        '/pages/claim/task-detail': '/pages/claim/task-list',
        '/pages/claim/onsite-assessment-detail': '/pages/claim/onsite-assessment-list',
        '/pages/claim/detail': '/pages/claim/list',
        '/pages/claim/edit': '/pages/claim/list',
        '/pages/claim/create': '/pages/claim/list',
        '/pages/policy/contract': '/pages/policy/contract-list',
        '/pages/policy/policies': '/pages/policy/policies',
        '/pages/policy/motorbike-policies': '/pages/policy/motorbike-policies'
    };
    
    ngOnInit() {
        // Load menu items from server
        this.navigationService.getMenuItems().subscribe({
            next: (items) => {
                this.menuItems = items;
                // Build breadcrumb after menu is loaded
                this.buildBreadcrumb();
            },
            error: (error) => {
                console.error('Error loading menu items:', error);
                this.menuItems = [];
                this.buildBreadcrumb();
            }
        });

        // Listen to route changes
        this.router.events
            .pipe(filter(event => event instanceof NavigationEnd))
            .subscribe(() => {
                this.buildBreadcrumb();
            });

        // Build initial breadcrumb 
        // (may not have menu items yet)
        this.buildBreadcrumb();
    }

    private buildBreadcrumb() {
        const breadcrumbs: BreadcrumbItem[] = [];
        let currentRoute: ActivatedRoute | null = this.activatedRoute.root;
        let url = '';
        const processedUrls = new Set<string>();

        // Traverse route tree to get final URL
        while (currentRoute?.firstChild) {
            currentRoute = currentRoute.firstChild;
            const routeSnapshot = currentRoute.snapshot;
            const routeUrl = routeSnapshot.url.map(segment => segment.path).join('/');

            if (routeUrl) {
                url += `/${routeUrl}`;
            }

            // Skip "pages" segment - it's just a container
            if (routeUrl === 'pages' || routeSnapshot.routeConfig?.path === 'pages') {
                continue;
            }
        }

        // Build breadcrumb from menu structure - completely dynamic based on server menu
        if (url && this.menuItems && this.menuItems.length > 0) {
            // Map some detail routes to their logical list route for menu hierarchy
            // Ví dụ: /pages/claim/create nên kế thừa cha từ /pages/claim/list
            const menuHierarchyUrl = this.mapUrlForMenuHierarchy(url);

            // Find menu item for current URL (hoặc URL đã map) và tất cả cha của nó
            const result = this.findMenuItemWithParents(menuHierarchyUrl, this.menuItems, []);
            
            if (result) {
                // Add all parent menu items to breadcrumb
                // Parents are in order from root to direct parent
                for (let i = 0; i < result.parents.length; i++) {
                    const parent = result.parents[i];
                    // Determine URL for parent menu item
                    let parentUrl = this.normalizeUrl(parent.url || '');
                    
                    // If parent doesn't have URL, try to find the closest child with URL
                    // to construct a reasonable URL for navigation
                    if (!parentUrl) {
                        // Find first child with URL in this parent's subtree
                        const childWithUrl = this.findFirstChildWithUrl(parent);
                        if (childWithUrl && childWithUrl.url) {
                            // Use the child's URL path but go up one level
                            const childUrl = this.normalizeUrl(childWithUrl.url);
                            const pathSegments = childUrl.split('/').filter(s => s && s !== 'pages');
                            if (pathSegments.length > 0) {
                                // Construct parent URL by removing last segment
                                parentUrl = '/pages/' + pathSegments.slice(0, -1).join('/');
                            } else {
                                parentUrl = '/pages';
                            }
                        } else {
                            // No child with URL found, use /pages as placeholder
                            // But make it unique for each parent level
                            const pathSegments = url.split('/').filter(s => s && s !== 'pages');
                            if (i === 0 && pathSegments.length > 0) {
                                // First level parent without URL - likely Administration
                                parentUrl = '/pages';
                            } else {
                                // For nested parents without URL, use /pages as placeholder
                                parentUrl = '/pages';
                            }
                        }
                    }
                    
                    // Create unique key for this breadcrumb item to avoid duplicates
                    // Use combination of parent name and index to ensure uniqueness
                    const breadcrumbKey = `${parent.name || parent.displayName}_${i}`;
                    
                    // Add parent to breadcrumb (even if URL is placeholder)
                    // Only check URL uniqueness if URL is not placeholder
                    if (parentUrl !== '/pages' || !processedUrls.has(breadcrumbKey)) {
                        if (parentUrl !== url) {
                            breadcrumbs.push({
                                label: parent.displayName || parent.name,
                                url: parentUrl,
                                clickable: !!parent.url
                            });
                            // Track both URL and unique key to avoid duplicates
                            if (parentUrl !== '/pages') {
                                processedUrls.add(parentUrl);
                            }
                            processedUrls.add(breadcrumbKey);
                        }
                    }
                }

                // Also add the direct menu item (ví dụ: "Tiếp nhận yêu cầu bồi thường")
                // nếu nó khác với URL hiện tại và chưa được thêm
                const item = result.item;
                if (item && item.url) {
                    const itemUrl = this.normalizeUrl(item.url);
                    if (itemUrl !== url && !processedUrls.has(itemUrl)) {
                        breadcrumbs.push({
                            label: item.displayName || item.name,
                            url: itemUrl,
                            clickable: true
                        });
                        processedUrls.add(itemUrl);
                    }
                }
            }
        }

        // Get label for current route
        const finalRoute = this.getFinalRoute();
        const routeData = finalRoute?.snapshot.data;
        let label = routeData?.['breadcrumb'] || routeData?.['title'];
        
        if (!label && url) {
            // Try to find menu item for current URL
            const currentMenuItem = this.findMenuItemByUrl(url, this.menuItems);
            if (currentMenuItem) {
                label = currentMenuItem.displayName || currentMenuItem.name;
            } else {
                // Fallback: generate from path
                const pathSegments = url.split('/').filter(s => s && s !== 'pages');
                if (pathSegments.length > 0) {
                    label = this.generateLabelFromPath(pathSegments[pathSegments.length - 1]);
                }
            }
        }

        // Localize label if it's a localization key
        if (label && label.includes('::')) {
            label = this.localizationService.localize(label, label);
        }

        // Add current route to breadcrumb
        if (label && url !== '/' && !processedUrls.has(url)) {
            breadcrumbs.push({
                label: label,
                url: url,
                clickable: true
            });
        }
        this.breadcrumbs = breadcrumbs;
    }

    private getFinalRoute(): ActivatedRoute | null {
        let currentRoute: ActivatedRoute | null = this.activatedRoute.root;
        while (currentRoute?.firstChild) {
            currentRoute = currentRoute.firstChild;
        }
        return currentRoute;
    }

    /**
     * Map URL chi tiết về URL list tương ứng để lấy đúng cây menu cha.
     * Ví dụ:
     *  - /pages/claim/create
     *  - /pages/claim/edit/123
     *  - /pages/claim/detail/123
     * đều sẽ dựa vào cây menu của module "claim" (thường là list).
     */
    private mapUrlForMenuHierarchy(url: string): string {
        const normalized = this.normalizeUrl(url); // vd: /pages/claim/create
        if (!normalized || !this.menuItems || this.menuItems.length === 0) {
            return normalized;
        }

        // Bước 1: cắt bớt các segment cuối kiểu action/id để ra URL cha logic
        const segments = normalized.split('/').filter(s => s); // ['pages', 'claim', 'create', '123', ...]
        if (segments.length <= 2) {
            // /pages hoặc /pages/claim -> giữ nguyên
            return normalized;
        }

        const actionLike = ['create', 'edit', 'view', 'detail', 'termination', 'endorsement'];
        const trimmed = [...segments];

        const last = trimmed[trimmed.length - 1];
        const looksLikeId = /^[0-9]+$/.test(last) || /^[0-9a-f-]{6,}$/i.test(last);

        if (actionLike.includes(last.toLowerCase()) || looksLikeId) {
            trimmed.pop();
        }

        if (trimmed.length > 3) {
            const last2 = trimmed[trimmed.length - 1];
            const looksLikeId2 = /^[0-9]+$/.test(last2) || /^[0-9a-f-]{6,}$/i.test(last2);
            if (actionLike.includes(last2.toLowerCase()) || looksLikeId2) {
                trimmed.pop();
            }
        }

        const baseUrl = '/' + trimmed.join('/'); // vd: /pages/claim
        const mappedParent = this.routeParentMap[baseUrl];
        if (mappedParent) {
            return mappedParent;
        }

        // Bước 2: nếu baseUrl trùng với URL của 1 menu item thì dùng luôn
        const exactMenuItem = this.findMenuItemByUrl(baseUrl, this.menuItems);
        if (exactMenuItem && exactMenuItem.url && this.normalizeUrl(exactMenuItem.url) === baseUrl) {
            return this.normalizeUrl(exactMenuItem.url);
        }

        // Bước 3: nếu không có exact match, tìm item "list" gần nhất trong cùng module
        const pathParts = baseUrl.split('/').filter(s => s);
        let moduleName: string | undefined;
        if (pathParts[0] === 'pages') {
            moduleName = pathParts[1];
        } else {
            moduleName = pathParts[0];
        }

        if (!moduleName) {
            return normalized;
        }

        const candidates: ApplicationMenuItem[] = [];

        const collectCandidates = (items: ApplicationMenuItem[]) => {
            for (const item of items) {
                if (item.url) {
                    const u = this.normalizeUrl(item.url);
                    const ps = u.split('/').filter(s => s);
                    if (ps.length >= 3) {
                        const mod = ps[0] === 'pages' ? ps[1] : ps[0];
                        if (mod === moduleName) {
                            candidates.push(item);
                        }
                    }
                }
                if (item.items && item.items.length > 0) {
                    collectCandidates(item.items);
                }
            }
        };

        collectCandidates(this.menuItems);

        if (candidates.length === 0) {
            return baseUrl;
        }

        // Ưu tiên URL ngắn nhất (thường là list) trong module
        candidates.sort((a, b) => {
            const ua = this.normalizeUrl(a.url || '');
            const ub = this.normalizeUrl(b.url || '');
            const sa = ua.split('/').length;
            const sb = ub.split('/').length;
            return sa - sb || ua.length - ub.length;
        });

        return this.normalizeUrl(candidates[0].url || baseUrl);
    }

    private findMenuItemWithParents(url: string, items: ApplicationMenuItem[], parents: ApplicationMenuItem[]): { item: ApplicationMenuItem; parents: ApplicationMenuItem[] } | null {
        const normalizedUrl = this.normalizeUrl(url);
        
        for (const item of items) {
            // Check if this item's URL matches exactly
            if (item.url) {
                const itemUrl = this.normalizeUrl(item.url);
                if (itemUrl === normalizedUrl) {
                    // Exact match found, return it with all parents
                    return { item, parents };
                }
            }
            
            // Check children recursively
            if (item.items && item.items.length > 0) {
                // Add current item to parents chain before searching children
                const newParents = [...parents, item];
                const found = this.findMenuItemWithParents(url, item.items, newParents);
                if (found) {
                    // Found in children, return with updated parents chain
                    return found;
                }
            }
        }
        
        return null;
    }

    private findMenuItemInPath(pathSegments: string[], items: ApplicationMenuItem[]): ApplicationMenuItem | null {
        if (pathSegments.length === 0) return null;
        
        const segment = pathSegments[0];
        for (const item of items) {
            // Check if item name or URL matches the segment
            const itemUrl = this.normalizeUrl(item.url || '');
            const urlSegments = itemUrl.split('/').filter(s => s && s !== 'pages');
            
            if (item.name === segment || 
                item.displayName === segment ||
                (urlSegments.length > 0 && urlSegments[urlSegments.length - 1] === segment)) {
                // Found matching item
                if (pathSegments.length === 1) {
                    return item;
                } else {
                    // Continue searching in children
                    if (item.items && item.items.length > 0) {
                        return this.findMenuItemInPath(pathSegments.slice(1), item.items);
                    }
                }
            }
            
            // Also check children recursively
            if (item.items && item.items.length > 0) {
                const found = this.findMenuItemInPath(pathSegments, item.items);
                if (found) return found;
            }
        }
        
        return null;
    }

    private findMenuItemByName(name: string, items: ApplicationMenuItem[]): ApplicationMenuItem | null {
        for (const item of items) {
            if (item.name === name) {
                return item;
            }
            if (item.items && item.items.length > 0) {
                const found = this.findMenuItemByName(name, item.items);
                if (found) return found;
            }
        }
        return null;
    }

    private findMenuItemByDisplayName(displayName: string, items: ApplicationMenuItem[]): ApplicationMenuItem | null {
        for (const item of items) {
            if (item.displayName === displayName || item.displayName?.includes(displayName)) {
                return item;
            }
            if (item.items && item.items.length > 0) {
                const found = this.findMenuItemByDisplayName(displayName, item.items);
                if (found) return found;
            }
        }
        return null;
    }

    private findFirstChildWithUrl(item: ApplicationMenuItem): ApplicationMenuItem | null {
        // If item itself has URL, return it
        if (item.url) {
            return item;
        }
        
        // Otherwise, search in children
        if (item.items && item.items.length > 0) {
            for (const child of item.items) {
                const found = this.findFirstChildWithUrl(child);
                if (found) return found;
            }
        }
        
        return null;
    }

    private findMenuItemByUrl(url: string, items: ApplicationMenuItem[]): ApplicationMenuItem | null {
        const normalizedUrl = this.normalizeUrl(url);
        let prefixMatch: ApplicationMenuItem | null = null;
        let prefixMatchLength = 0;

        const search = (list: ApplicationMenuItem[]): ApplicationMenuItem | null => {
            for (const item of list) {
                if (item.url) {
                    const itemUrl = this.normalizeUrl(item.url);
                    if (itemUrl === normalizedUrl) {
                        return item;
                    }
                    if (normalizedUrl.startsWith(itemUrl + '/') && itemUrl.length > prefixMatchLength) {
                        prefixMatch = item;
                        prefixMatchLength = itemUrl.length;
                    }
                }
                if (item.items && item.items.length > 0) {
                    const found = search(item.items);
                    if (found) return found;
                }
            }
            return null;
        };

        return search(items) || prefixMatch;
    }

    private findParentMenuItemByUrl(url: string, items: ApplicationMenuItem[], parent: ApplicationMenuItem | null): ApplicationMenuItem | null {
        const normalizedUrl = this.normalizeUrl(url);
        for (const item of items) {
            // Check if this item's URL matches
            if (item.url) {
                const itemUrl = this.normalizeUrl(item.url);
                if (itemUrl === normalizedUrl || normalizedUrl.startsWith(itemUrl + '/')) {
                    // This is the item itself, return its parent
                    return parent;
                }
            }
            
            // Check if any child matches the URL
            if (item.items && item.items.length > 0) {
                for (const child of item.items) {
                    if (child.url) {
                        const childUrl = this.normalizeUrl(child.url);
                        if (childUrl === normalizedUrl || normalizedUrl.startsWith(childUrl + '/')) {
                            // Found a child that matches, return the parent (item)
                            return item;
                        }
                    }
                }
                // Recursively check nested items
                const found = this.findParentMenuItemByUrl(url, item.items, item);
                if (found) return found;
            }
        }
        return null;
    }

    private normalizeUrl(url: string): string {
        if (!url) return '';
        // Use same logic as NavigationService
        let normalized = url.replace(/^~\//, '/');
        // Ensure starts with /
        if (!normalized.startsWith('/')) {
            normalized = '/' + normalized;
        }
        // Remove trailing / (except root)
        if (normalized.length > 1 && normalized.endsWith('/')) {
            normalized = normalized.slice(0, -1);
        }
        return normalized;
    }

    private getModuleLabel(moduleName: string): string {
        // Fallback: generate from module name
        // This is only used when menu item is not found
        return this.generateLabelFromPath(moduleName);
    }

    private generateLabelFromPath(path: string): string {
        if (!path) return '';

        // Map special paths to localization keys
        const pathMappings: Record<string, string> = {
            'hr': 'Hr::Menu:HR',
            'system-settings': 'iOne::Menu:SystemSettings',
            'audit-logs': 'iOne::Menu:AuditLogs',
            'roles': 'iOne::Menu:Roles',
            'users': 'iOne::Menu:Users'
        };

        // Check if path has a mapping
        if (pathMappings[path]) {
            const localized = this.localizationService.localize(pathMappings[path], path);
            return localized;
        }

        // Convert path to readable label
        // e.g., "audit-logs" -> "Audit Logs", "users" -> "Users"
        return path
            .split('-')
            .map(word => word.charAt(0).toUpperCase() + word.slice(1))
            .join(' ');
    }
}

