import { Component, Input, Output, EventEmitter, OnChanges, SimpleChanges, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TreeModule, TreeNodeSelectEvent } from 'primeng/tree';
import { ContextMenu, ContextMenuModule } from 'primeng/contextmenu';
import { InputTextModule } from 'primeng/inputtext';
import { TreeNode, MenuItem } from 'primeng/api';
import { ProProductCategoryDto } from '@/proxy/product/pro-product-categorys/models';
import { ProProductListDto } from '@/proxy/product/pro-products/models';
import { ProProductCategoryStatus } from '@/proxy/pro-product-categorys';
import { ProProductStatus } from '@/proxy/pro-products';
import { LocalizationService } from '@/core/services/localization.service';

export interface ProductTreeNode extends TreeNode {
    nodeType: 'category' | 'product';
    data: ProProductCategoryDto | ProProductListDto;
    isActive: boolean;
}

@Component({
    selector: 'app-v-product-tree',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        TreeModule,
        ContextMenuModule,
        InputTextModule,
    ],
    templateUrl: './v-product-tree.component.html',
    styleUrl: './v-product-tree.component.scss'
})
export class VProductTreeComponent implements OnChanges {
    @Input() categories: ProProductCategoryDto[] = [];
    @Input() products: ProProductListDto[] = [];
    @Input() loading: boolean = false;
    @Input() filterText: string = '';
    /** When true (e.g. search panel has criteria), hide category branches with no descendant products. */
    @Input() searchFilterActive = false;

    @Output() nodeSelect = new EventEmitter<ProductTreeNode>();
    @Output() addProduct = new EventEmitter<{ parentCategoryId?: string; parentProductId?: string }>();
    @Output() editProduct = new EventEmitter<ProProductListDto>();
    @Output() deleteProduct = new EventEmitter<ProProductListDto>();
    @Output() makeActive = new EventEmitter<ProProductListDto>();
    @Output() makeInactive = new EventEmitter<ProProductListDto>();

    @ViewChild('contextMenu') contextMenu!: ContextMenu;

    treeNodes: ProductTreeNode[] = [];
    selectedNode: ProductTreeNode | null = null;
    contextMenuItems: MenuItem[] = [];
    filteredNodes: ProductTreeNode[] = [];

    constructor(private localizationService: LocalizationService) { }

    ngOnChanges(changes: SimpleChanges): void {
        if (changes['categories'] || changes['products'] || changes['searchFilterActive']) {
            this.buildTree();
        }
        if (changes['filterText']) {
            this.applyFilter();
        }
    }

    /**
     * Build the tree structure from categories and products
     */
    private buildTree(): void {
        const categoryMap = new Map<string, ProductTreeNode>();
        const productMap = new Map<string, ProductTreeNode>();

        // First, create all category nodes
        this.categories.forEach(category => {
            const isActive = category.status === ProProductCategoryStatus.Active;
            const node: ProductTreeNode = {
                key: `cat_${category.id}`,
                label: category.name || '',
                icon: '', // Don't set icon property - we render it in template to avoid duplication
                nodeType: 'category',
                data: category,
                isActive: isActive,
                children: [],
                expanded: true,
                styleClass: isActive ? '' : 'inactive-node'
            };
            // Store icon separately in a custom property for template use
            (node as any).iconClass = 'pi pi-folder';
            categoryMap.set(category.id!, node);
        });

        // Build category hierarchy
        this.categories.forEach(category => {
            if (category.parentId && categoryMap.has(category.parentId)) {
                const parentNode = categoryMap.get(category.parentId)!;
                const childNode = categoryMap.get(category.id!)!;
                parentNode.children = parentNode.children || [];
                parentNode.children.push(childNode);
            }
        });

        // Create product nodes
        this.products.forEach(product => {
            const isActive = product.status === ProProductStatus.Active;
            const hasPartner = !!product.partnerId;
            const node: ProductTreeNode = {
                key: `prod_${product.id}`,
                label: product.name || '',
                icon: '', // Don't set icon property - we render it in template to avoid duplication
                nodeType: 'product',
                data: product,
                isActive: isActive,
                children: [],
                expanded: true,
                styleClass: isActive ? '' : 'inactive-node'
            };
            // Store icon separately in a custom property for template use
            (node as any).iconClass = hasPartner ? 'pi pi-tag' : 'pi pi-box';
            productMap.set(product.id!, node);
        });

        // Build product hierarchy (products can have parent products).
        // Track which products were attached under a parent product or category so that
        // rows whose parent is missing from the filtered API result (e.g. partner filter)
        // still appear as top-level nodes instead of being dropped.
        const attachedIds = new Set<string>();
        this.products.forEach(product => {
            const productNode = productMap.get(product.id!)!;

            if (product.rootProductId && productMap.has(product.rootProductId)) {
                const parentProductNode = productMap.get(product.rootProductId)!;
                parentProductNode.children = parentProductNode.children || [];
                parentProductNode.children.push(productNode);
                attachedIds.add(product.id!);
            } else if (product.productCategoryId && categoryMap.has(product.productCategoryId)) {
                const categoryNode = categoryMap.get(product.productCategoryId)!;
                categoryNode.children = categoryNode.children || [];
                categoryNode.children.push(productNode);
                attachedIds.add(product.id!);
            }
        });

        const rootCategories = this.categories
            .filter(c => !c.parentId)
            .map(c => categoryMap.get(c.id!)!)
            .filter(n => !!n);

        const floatingProductRoots = this.products
            .filter(p => p.id && !attachedIds.has(p.id))
            .map(p => productMap.get(p.id!)!)
            .filter(n => !!n);

        let roots: ProductTreeNode[] = [...rootCategories, ...floatingProductRoots];
        if (this.searchFilterActive) {
            roots = this.pruneCategoryNodesWithoutProductDescendants(roots);
        }
        this.treeNodes = roots;
        this.applyFilter();
    }

    /**
     * Remove category nodes that have no descendant product nodes (panel search filter active).
     */
    private pruneCategoryNodesWithoutProductDescendants(nodes: ProductTreeNode[]): ProductTreeNode[] {
        const result: ProductTreeNode[] = [];
        for (const node of nodes) {
            const pruned = this.pruneNodeForPanelFilter(node);
            if (pruned) {
                result.push(pruned);
            }
        }
        return result;
    }

    private pruneNodeForPanelFilter(node: ProductTreeNode): ProductTreeNode | null {
        const children = (node.children || []) as ProductTreeNode[];
        const prunedChildren = children
            .map(child => this.pruneNodeForPanelFilter(child))
            .filter((n): n is ProductTreeNode => n !== null);

        if (node.nodeType === 'product') {
            return { ...node, children: prunedChildren };
        }
        if (prunedChildren.length === 0) {
            return null;
        }
        return { ...node, children: prunedChildren };
    }

    /**
     * Apply filter to tree nodes
     */
    applyFilter(): void {
        if (!this.filterText || this.filterText.trim() === '') {
            this.filteredNodes = [...this.treeNodes];
        } else {
            const filter = this.filterText.toLowerCase();
            this.filteredNodes = this.filterNodes(this.treeNodes, filter);
        }
    }

    /**
     * Recursively filter nodes
     */
    private filterNodes(nodes: ProductTreeNode[], filter: string): ProductTreeNode[] {
        const result: ProductTreeNode[] = [];

        nodes.forEach(node => {
            const nodeMatches = node.label?.toLowerCase().includes(filter);
            const filteredChildren = node.children
                ? this.filterNodes(node.children as ProductTreeNode[], filter)
                : [];

            if (nodeMatches || filteredChildren.length > 0) {
                const clonedNode: ProductTreeNode = {
                    ...node,
                    children: filteredChildren.length > 0 ? filteredChildren : node.children,
                    expanded: filteredChildren.length > 0 // Expand if children match
                };
                result.push(clonedNode);
            }
        });

        return result;
    }

    /**
     * Handle node selection
     */
    onNodeSelect(event: TreeNodeSelectEvent): void {
        this.selectedNode = event.node as ProductTreeNode;
        this.nodeSelect.emit(this.selectedNode);
    }

    /**
     * Handle right-click context menu
     */
    onContextMenu(event: MouseEvent, node: ProductTreeNode): void {
        event.preventDefault();
        this.selectedNode = node;
        this.buildContextMenu(node);
        this.contextMenu.show(event);
    }

    /**
     * Build context menu items based on node type and status
     */
    private buildContextMenu(node: ProductTreeNode): void {
        const items: MenuItem[] = [];

        // Add Product action - available for categories and products
        items.push({
            label: this.localizationService.localize('Product::ProProduct:AddProduct'),
            icon: 'pi pi-plus',
            command: () => this.onAddProduct(node)
        });

        if (node.nodeType === 'product') {
            const product = node.data as ProProductListDto;

            items.push({
                label: this.localizationService.localize('Product::ProProduct:EditProduct'),
                icon: 'pi pi-pencil',
                command: () => this.editProduct.emit(product)
            });

            items.push({
                label: this.localizationService.localize('Product::ProProduct:DeleteProduct'),
                icon: 'pi pi-trash',
                command: () => this.deleteProduct.emit(product)
            });

            items.push({ separator: true });

            if (node.isActive) {
                items.push({
                    label: this.localizationService.localize('Product::ProProduct:MakeInactive'),
                    icon: 'pi pi-times-circle',
                    command: () => this.makeInactive.emit(product)
                });
            } else {
                items.push({
                    label: this.localizationService.localize('Product::ProProduct:MakeActive'),
                    icon: 'pi pi-check-circle',
                    command: () => this.makeActive.emit(product)
                });
            }
        }

        this.contextMenuItems = items;
    }

    /**
     * Handle add product action
     */
    private onAddProduct(node: ProductTreeNode): void {
        if (node.nodeType === 'category') {
            const category = node.data as ProProductCategoryDto;
            this.addProduct.emit({ parentCategoryId: category.id });
        } else {
            const product = node.data as ProProductListDto;
            this.addProduct.emit({ parentProductId: product.id });
        }
    }

    /**
     * Get icon class for a node
     */
    getNodeIconClass(node: ProductTreeNode): string {
        const nodeWithIcon = node as any;
        return nodeWithIcon.iconClass || node.icon || 'pi pi-box';
    }
}
