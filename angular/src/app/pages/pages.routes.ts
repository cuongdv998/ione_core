import { Routes } from '@angular/router';
import { AuditLogsComponent } from './audit-logs/audit-logs.component';
import { RolesComponent } from './roles/roles.component';
import { UsersComponent } from './users/users.component';
import { DepartmentTypesComponent } from './hr/department-types/department-types.component';
import { DepartmentsComponent } from './hr/departments/departments.component';
import { EmployeeRolesComponent } from './hr/employee-roles/employee-roles.component';
import { EmployeeLevelsComponent } from './hr/employee-levels/employee-levels.component';
import { EmployeePositionsComponent } from './hr/employee-positions/employee-positions.component';
import { EmployeesComponent } from './hr/employees/employees.component';
import { PartnerTypesComponent } from './partner/partner-types/partner-types.component';
import { ResOrganizationTypesComponent } from './partner/res-organization-types/res-organization-types.component';
import { ChannelsComponent } from './partner/channels/channels.component';
import { AgreementTermsComponent } from './partner/agreement-terms/agreement-terms.component';
import { ResPartnersComponent } from './partner/res-partners/res-partners.component';
import { CountriesComponent } from './master/countries/countries.component';
import { ProvincesComponent } from './master/provinces/provinces.component';
import { WardsComponent } from './master/wards/wards.component';
import { DocumentTypesComponent } from './master/document-types/document-types.component';
import { AdminConfigsComponent } from './master/admin-configs/admin-configs.component';
import { CarBrandsComponent } from './master/car-brands/car-brands.component';
import { CarLinesComponent } from './master/car-lines/car-lines.component';
import { CarGroupsComponent } from './master/car-groups/car-groups.component';
import { CarModelsComponent } from './master/car-models/car-models.component';
import { CarTypesComponent } from './master/car-types/car-types.component';
import { MotorClassesComponent } from './master/motor-classes/motor-classes.component';
import { CarCategoriesComponent } from './master/car-categories/car-categories.component';
import { BanksComponent } from './master/banks/banks.component';
import { ObjectTypesComponent } from './master/object-types/object-types.component';
import { RisksComponent } from './master/risks/risks.component';
import { InsurerDictionariesComponent } from './master/insurer-dictionaries/insurer-dictionaries.component';
import { ResReasonGroupsComponent } from './master/res-reason-groups/res-reason-groups.component';
import { ResFeeItemsComponent } from './master/res-fee-items/res-fee-items.component';
import { ResPaymentMethodsComponent } from './master/res-payment-methods/res-payment-methods.component';
import { ResReasonsComponent } from './master/res-reasons/res-reasons.component';
import { ResObjectItemTypesComponent } from './master/res-object-item-types/res-object-item-types.component';
import { ResObjectTypeItemsComponent } from './master/res-object-type-items/res-object-type-items.component';
import { ResObjectItemDepreciationsComponent } from './master/res-object-item-depreciations/res-object-item-depreciations.component';
import { DamageLevelsComponent } from './master/damage-levels/damage-levels.component';
import { UomClassesComponent } from './master/uom-classes/uom-classes.component';
import { ResUomsComponent } from './master/res-uoms/res-uoms.component';
import { ResSequencesComponent } from './master/res-sequences/res-sequences.component';
import { ResCurrencyComponent } from './master/currencies/currencies.component';
import { ResAppChannelsComponent } from './master/res-app-channels/res-app-channels.component';
import { EventsComponent } from './master/events/events.component';
import { UserDevicesComponent } from './master/user-devices/user-devices.component';
import { SystemEventNotifiesComponent } from './master/system-event-notifies/system-event-notifies.component';
import { ResBusinessAuthoritiesComponent } from './master/res-business-authorities/res-business-authorities.component';
import { ResBusinessAssigneesComponent } from './master/res-business-assignees/res-business-assignees.component';
import { ResTaskCategoriesComponent } from './master/res-task-categories/res-task-categories.component';
import { BusinessFlowsComponent } from './master/business-flows/business-flows.component';
import { IndustriesComponent } from './customer/industries/industries.component';
import { ResCustomersComponent } from './customer/res-customers/res-customers.component';
import { PolicyTypesComponent } from './policy/policy-types/policy-types.component';
import { PoliciesComponent } from './policy/policies/policies.component';
import { PolicyRequestApprovalComponent } from './policy/policy-request-approval/policy-request-approval.component';
import { permissionGuard } from '@/core/guards/permission.guard';
import { ReportComponent } from './report/report.component';
import { ClaimListComponent } from './claim/claim-list/claim-list.component';
import { ClaimTaskListComponent } from './claim/claim-task-list/claim-task-list.component';
import { ClaimCreateComponent } from './claim/claim-create/claim-create.component';
import { ClaimDetailComponent } from './claim/claim-detail/claim-detail.component';
import { ClaimTaskDetailComponent } from './claim/claim-task-detail/claim-task-detail.component';
import { PolicyContractListComponent } from './policy/policy-contract/policy-contract-list/policy-contract-list.component';
import { PolicyContractCreateComponent } from './policy/policy-contract/policy-contract-create/policy-contract-create.component';
import { PolicyContractDetailComponent } from './policy/policy-contract/policy-contract-detail/policy-contract-detail.component';
import { ApiKeysComponent } from './api-keys';
import { DashboardComponent } from './dashboard/dashboard.component';
import { SystemSettingsComponent } from './system-settings/system-settings.component';

export default [
    {
        path: 'dashboard',
        component: DashboardComponent,
        data: {
            breadcrumb: 'iOne::Menu:Dashboard'
        }
    },
    {
        path: 'system-settings',
        component: SystemSettingsComponent,
        canActivate: [permissionGuard],
        data: {
            requiredPermission: 'iOne.AppSetting.View',
            breadcrumb: 'iOne::Menu:SystemSettings'
        }
    },
    {
        path: 'audit-logs',
        component: AuditLogsComponent,
        canActivate: [permissionGuard],
        data: {
            requiredPermission: 'AbpAuditLogging.AuditLogs',
            breadcrumb: 'iOne::Menu:AuditLogs'
        }
    },
    {
        path: 'roles',
        component: RolesComponent,
        canActivate: [permissionGuard],
        data: {
            requiredPermission: 'AbpIdentity.Roles',
            breadcrumb: 'iOne::Menu:Roles'
        }
    },
    {
        path: 'users',
        component: UsersComponent,
        canActivate: [permissionGuard],
        data: {
            requiredPermission: 'AbpIdentity.Users',
            breadcrumb: 'iOne::Menu:Users'
        }
    },
    {
        path: 'api-keys',
        component: ApiKeysComponent,
        data: {
            breadcrumb: 'API Keys'
        }
    },
    {
        path: 'hr/department-types',
        component: DepartmentTypesComponent,
        canActivate: [permissionGuard],
        data: {
            // Permission name là "HrDepartmentType" (vì Default = GroupName)
            requiredPermission: 'HrDepartmentType',
            breadcrumb: 'Hr::Menu:DepartmentTypes'
        }
    },
    {
        path: 'hr/departments',
        component: DepartmentsComponent,
        canActivate: [permissionGuard],
        data: {
            // Permission name là "HrDepartment" (vì Default = GroupName)
            requiredPermission: 'HrDepartment',
            breadcrumb: 'Hr::Menu:Departments'
        }
    },
    {
        path: 'hr/employee-roles',
        component: EmployeeRolesComponent,
        canActivate: [permissionGuard],
        data: {
            // Permission name là "HrEmployeeRole" (vì Default = GroupName)
            requiredPermission: 'HrEmployeeRole',
            breadcrumb: 'Hr::Menu:EmployeeRoles'
        }
    },
    {
        path: 'hr/employee-levels',
        component: EmployeeLevelsComponent,
        canActivate: [permissionGuard],
        data: {
            // Permission name là "HrEmployeeLevel" (vì Default = GroupName)
            requiredPermission: 'HrEmployeeLevel',
            breadcrumb: 'Hr::Menu:EmployeeLevels'
        }
    },
    {
        path: 'hr/employee-positions',
        component: EmployeePositionsComponent,
        canActivate: [permissionGuard],
        data: {
            // Permission name là "HrEmployeePosition" (vì Default = GroupName)
            requiredPermission: 'HrEmployeePosition',
            breadcrumb: 'Hr::Menu:EmployeePositions'
        }
    },
    {
        path: 'hr/employees',
        component: EmployeesComponent,
        canActivate: [permissionGuard],
        data: {
            // Permission name là "HrEmployee" (vì Default = GroupName)
            requiredPermission: 'HrEmployee',
            breadcrumb: 'Hr::Menu:Employees'
        }
    },
    {
        path: 'product/pro-coverage-groups',
        loadComponent: () => import('./product/pro-coverage-groups/pro-coverage-groups.component').then(m => m.ProCoverageGroupsComponent),
        canActivate: [permissionGuard],
        data: {
            requiredPermission: 'Product.ProCoverageGroup',
            breadcrumb: 'Product::Menu:ProCoverageGroup'
        }
    },
    {
        path: 'product/line-of-businesses',
        loadComponent: () => import('./product/pro-line-of-businesses/pro-line-of-businesses.component').then(m => m.ProLineOfBusinessesComponent),
        canActivate: [permissionGuard],
        data: {
            requiredPermission: 'Product.ProLineOfBusiness',
            breadcrumb: 'Product::Menu:ProLineOfBusiness'
        }
    },
    {
        path: 'product/pro-product-categories',
        loadComponent: () => import('./product/pro-product-categories/pro-product-categories.component').then(m => m.ProProductCategoriesComponent),
        canActivate: [permissionGuard],
        data: {
            requiredPermission: 'Product.ProProductCategory',
            breadcrumb: 'Product::Menu:ProProductCategory'
        }
    },
    {
        path: 'product/coverage-types',
        loadComponent: () => import('./product/pro-coverage-types/pro-coverage-types.component').then(m => m.ProCoverageTypesComponent),
        canActivate: [permissionGuard],
        data: {
            requiredPermission: 'Product.ProCoverageType',
            breadcrumb: 'Product::Menu:ProCoverageType'
        }
    },
    {
        path: 'product/pro-product-types',
        loadComponent: () => import('./product/pro-product-types/pro-product-types.component').then(m => m.ProProductTypesComponent),
        canActivate: [permissionGuard],
        data: {
            requiredPermission: 'Product.ProProductType',
            breadcrumb: 'Product::Menu:ProProductType'
        }
    },
    {
        path: 'product/pro-attributes',
        loadComponent: () => import('./product/pro-attributes/pro-attributes.component').then(m => m.ProAttributesComponent),
        canActivate: [permissionGuard],
        data: {
            requiredPermission: 'Product.ProAttribute',
            breadcrumb: 'Product::Menu:ProAttribute'
        }
    },
    {
        path: 'product/res-taxes',
        loadComponent: () => import('./product/res-taxes/res-taxes.component').then(m => m.ResTaxesComponent),
        canActivate: [permissionGuard],
        data: {
            requiredPermission: 'Product.ResTax',
            breadcrumb: 'Product::Menu:ResTax'
        }
    },
    {
        path: 'partner/partner-types',
        component: PartnerTypesComponent,
        canActivate: [permissionGuard],
        data: {
            // Permission name là "PartnerResPartnerType" (vì Default = GroupName)
            requiredPermission: 'PartnerResPartnerType',
            breadcrumb: 'Partner::Menu:ResPartnerType'
        }
    },
    {
        path: 'partner/res-organization-types',
        component: ResOrganizationTypesComponent,
        canActivate: [permissionGuard],
        data: {
            // Permission name là "PartnerResOrganizationType" (vì Default = GroupName)
            requiredPermission: 'PartnerResOrganizationType',
            breadcrumb: 'Partner::Menu:ResOrganizationType'
        }
    },
    {
        path: 'partner/channels',
        component: ChannelsComponent,
        canActivate: [permissionGuard],
        data: {
            // Permission name là "PartnerResChannel" (vì Default = GroupName)
            requiredPermission: 'PartnerResChannel',
            breadcrumb: 'Partner::Menu:ResChannel'
        }
    },
    {
        path: 'partner/agreement-terms',
        component: AgreementTermsComponent,
        canActivate: [permissionGuard],
        data: {
            // Permission name là "PartnerResAgreementTerm" (vì Default = GroupName)
            requiredPermission: 'PartnerResAgreementTerm',
            breadcrumb: 'Partner::Menu:ResAgreementTerm'
        }
    },
    {
        path: 'partner/res-partners',
        component: ResPartnersComponent,
        canActivate: [permissionGuard],
        data: {
            // Permission name là "PartnerResPartner" (vì Default = GroupName)
            requiredPermission: 'PartnerResPartner',
            breadcrumb: 'Partner::Menu:ResPartner'
        }
    },
    {
        path: 'master/countries',
        component: CountriesComponent,
        canActivate: [permissionGuard],
        data: {
            // Permission name là "MasterResCountry" (vì Default = GroupName)
            requiredPermission: 'MasterResCountry',
            breadcrumb: 'Master::Menu:ResCountry'
        }
    },
    {
        path: 'master/provinces',
        component: ProvincesComponent,
        canActivate: [permissionGuard],
        data: {
            // Permission name là "MasterResProvince" (vì Default = GroupName)
            requiredPermission: 'MasterResProvince',
            breadcrumb: 'Master::Menu:ResProvince'
        }
    },
    {
        path: 'master/wards',
        component: WardsComponent,
        canActivate: [permissionGuard],
        data: {
            // Permission name là "MasterResWard" (vì Default = GroupName)
            requiredPermission: 'MasterResWard',
            breadcrumb: 'Master::Menu:ResWard'
        }
    },
    {
        path: 'master/document-types',
        component: DocumentTypesComponent,
        canActivate: [permissionGuard],
        data: {
            // Permission name là "MasterResDocumentType" (vì Default = GroupName)
            requiredPermission: 'MasterResDocumentType',
            breadcrumb: 'Master::Menu:ResDocumentType'
        }
    },
    {
        path: 'master/admin-configs',
        component: AdminConfigsComponent,
        canActivate: [permissionGuard],
        data: {
            // Permission name là "MasterAdminConfig" (vì Default = GroupName)
            requiredPermission: 'MasterAdminConfig',
            breadcrumb: 'Master::Menu:AdminConfig'
        }
    },
    {
        path: 'master/car-brands',
        component: CarBrandsComponent,
        canActivate: [permissionGuard],
        data: {
            // Permission name là "MasterResCarBrand" (vì Default = GroupName)
            requiredPermission: 'MasterResCarBrand',
            breadcrumb: 'Master::Menu:ResCarBrand'
        }
    },
    {
        path: 'master/car-lines',
        component: CarLinesComponent,
        canActivate: [permissionGuard],
        data: {
            // Permission name là "MasterResCarLine" (vì Default = GroupName)
            requiredPermission: 'MasterResCarLine',
            breadcrumb: 'Master::Menu:ResCarLine'
        }
    },
    {
        path: 'master/car-groups',
        component: CarGroupsComponent,
        canActivate: [permissionGuard],
        data: {
            // Permission name là "MasterResCarGroup" (vì Default = GroupName)
            requiredPermission: 'MasterResCarGroup',
            breadcrumb: 'Master::Menu:ResCarGroup'
        }
    },
    {
        path: 'master/car-models',
        component: CarModelsComponent,
        canActivate: [permissionGuard],
        data: {
            // Permission name là "MasterResCarModel" (vì Default = GroupName)
            requiredPermission: 'MasterResCarModel',
            breadcrumb: 'Master::Menu:ResCarModel'
        }
    },
    {
        path: 'master/car-types',
        component: CarTypesComponent,
        canActivate: [permissionGuard],
        data: {
            // Permission name là "MasterResCarType" (vì Default = GroupName)
            requiredPermission: 'MasterResCarType',
            breadcrumb: 'Master::Menu:ResCarType'
        }
    },
    {
        path: 'master/motor-classes',
        component: MotorClassesComponent,
        canActivate: [permissionGuard],
        data: {
            // Permission name là "MasterResMotorClass" (vì Default = GroupName)
            requiredPermission: 'MasterResMotorClass',
            breadcrumb: 'Master::Menu:ResMotorClass'
        }
    },
    {
        path: 'master/car-categories',
        component: CarCategoriesComponent,
        canActivate: [permissionGuard],
        data: {
            // Permission name là "MasterResCarCategory" (vì Default = GroupName)
            requiredPermission: 'MasterResCarCategory',
            breadcrumb: 'Master::Menu:ResCarCategory'
        }
    },
    {
        path: 'master/banks',
        component: BanksComponent,
        canActivate: [permissionGuard],
        data: {
            // Permission name là "MasterResBank" (vì Default = GroupName)
            requiredPermission: 'MasterResBank',
            breadcrumb: 'Master::Menu:ResBank'
        }
    },
    {
        path: 'master/object-types',
        component: ObjectTypesComponent,
        canActivate: [permissionGuard],
        data: {
            // Permission name là "MasterResObjectType" (vì Default = GroupName)
            requiredPermission: 'MasterResObjectType',
            breadcrumb: 'Master::Menu:ResObjectType'
        }
    },
    {
        path: 'master/risks',
        component: RisksComponent,
        canActivate: [permissionGuard],
        data: {
            // Permission name là "MasterResRisk" (vì Default = GroupName)
            requiredPermission: 'MasterResRisk',
            breadcrumb: 'Master::Menu:ResRisk'
        }
    },
    {
        path: 'master/insurer-dictionaries',
        component: InsurerDictionariesComponent,
        canActivate: [permissionGuard],
        data: {
            // Permission name là "MasterInsurerDictionary" (vì Default = GroupName)
            requiredPermission: 'MasterInsurerDictionary',
            breadcrumb: 'Master::Menu:InsurerDictionary'
        }
    },
    {
        path: 'master/res-reason-groups',
        component: ResReasonGroupsComponent,
        canActivate: [permissionGuard],
        data: {
            // Permission name là "MasterResReasonGroup" (vì Default = GroupName)
            requiredPermission: 'MasterResReasonGroup',
            breadcrumb: 'Master::Menu:ResReasonGroup'
        }
    },
    {
        path: 'master/res-reasons',
        component: ResReasonsComponent,
        canActivate: [permissionGuard],
        data: {
            // Permission name là "MasterResReason" (vì Default = GroupName)
            requiredPermission: 'MasterResReason',
            breadcrumb: 'Master::Menu:ResReason'
        }
    },
    {
        path: 'master/res-app-channels',
        component: ResAppChannelsComponent,
        canActivate: [permissionGuard],
        data: {
            // Permission name là "MasterResAppChannel" (vì Default = GroupName)
            requiredPermission: 'MasterResAppChannel',
            breadcrumb: 'Master::Menu:ResAppChannel'
        }
    },
    {
        path: 'master/events',
        component: EventsComponent,
        canActivate: [permissionGuard],
        data: {
            // Permission name là "MasterResEvent" (vì Default = GroupName)
            requiredPermission: 'MasterResEvent',
            breadcrumb: 'Master::Menu:ResEvent'
        }
    },
    {
        path: 'master/user-devices',
        component: UserDevicesComponent,
        canActivate: [permissionGuard],
        data: {
            requiredPermission: 'MasterResUserDevice',
            breadcrumb: 'Master::Menu:ResUserDevice'
        }
    },
    {
        path: 'master/system-event-notifies',
        component: SystemEventNotifiesComponent,
        canActivate: [permissionGuard],
        data: {
            requiredPermission: 'MasterSystemEventNotify',
            breadcrumb: 'Master::Menu:SystemEventNotify'
        }
    },
    {
        path: 'master/res-business-authorities',
        component: ResBusinessAuthoritiesComponent,
        canActivate: [permissionGuard],
        data: {
            requiredPermission: 'MasterResBusinessAuthority',
            breadcrumb: 'Master::Menu:ResBusinessAuthority'
        }
    },
    {
        path: 'master/res-business-assignees',
        component: ResBusinessAssigneesComponent,
        canActivate: [permissionGuard],
        data: {
            requiredPermission: 'MasterResBusinessAssignee',
            breadcrumb: 'Master::Menu:ResBusinessAssignee'
        }
    },
    {
        path: 'master/res-task-categories',
        component: ResTaskCategoriesComponent,
        canActivate: [permissionGuard],
        data: {
            requiredPermission: 'MasterResTaskCategory',
            breadcrumb: 'Master::Menu:ResTaskCategory'
        }
    },
    {
        path: 'master/business-flows',
        component: BusinessFlowsComponent,
        canActivate: [permissionGuard],
        data: {
            requiredPermission: 'MasterBusinessFlow',
            breadcrumb: 'Master::Menu:BusinessFlow'
        }
    },
    {
        path: 'master/damage-levels',
        component: DamageLevelsComponent,
        canActivate: [permissionGuard],
        data: {
            // Permission name là "MasterResDamageLevel" (vì Default = GroupName)
            requiredPermission: 'MasterResDamageLevel',
            breadcrumb: 'Master::Menu:ResDamageLevel'
        }
    },
    {
        path: 'master/uom-classes',
        component: UomClassesComponent,
        canActivate: [permissionGuard],
        data: {
            // Permission name là "MasterResUomClass" (vì Default = GroupName)
            requiredPermission: 'MasterResUomClass',
            breadcrumb: 'Master::Menu:ResUomClass'
        }
    },
    {
        path: 'master/res-uoms',
        component: ResUomsComponent,
        canActivate: [permissionGuard],
        data: {
            // Permission name là "MasterResUom" (vì Default = GroupName)
            requiredPermission: 'MasterResUom',
            breadcrumb: 'Master::Menu:ResUom'
        }
    },
    {
        path: 'master/res-sequences',
        component: ResSequencesComponent,
        canActivate: [permissionGuard],
        data: {
            // Permission name là "MasterResSequence" (vì Default = GroupName)
            requiredPermission: 'MasterResSequence',
            breadcrumb: 'Master::Menu:ResSequence'
        }
    },
    {
        path: 'master/res-currencies',
        component: ResCurrencyComponent,
        canActivate: [permissionGuard],
        data: {
            // Permission name là "MasterResCurrency" (vì Default = GroupName)
            requiredPermission: 'MasterResCurrency',
            breadcrumb: 'Master::Menu:ResCurrency'
        }
    },
    {
        path: 'master/res-fee-items',
        component: ResFeeItemsComponent,
        canActivate: [permissionGuard],
        data: {
            // Permission name là "MasterResFeeItem" (vì Default = GroupName)
            requiredPermission: 'MasterResFeeItem',
            breadcrumb: 'Master::Menu:ResFeeItem'
        }
    },
    {
        path: 'master/res-payment-methods',
        component: ResPaymentMethodsComponent,
        canActivate: [permissionGuard],
        data: {
            // Permission name là "MasterResPaymentMethod" (vì Default = GroupName)
            requiredPermission: 'MasterResPaymentMethod',
            breadcrumb: 'Master::Menu:ResPaymentMethod'
        }
    },
    {
        path: 'master/res-object-item-types',
        component: ResObjectItemTypesComponent,
        canActivate: [permissionGuard],
        data: {
            // Permission name là "MasterResObjectItemType" (vì Default = GroupName)
            requiredPermission: 'MasterResObjectItemType',
            breadcrumb: 'Master::Menu:ResObjectItemType'
        }
    },
    {
        path: 'master/res-object-type-items',
        component: ResObjectTypeItemsComponent,
        canActivate: [permissionGuard],
        data: {
            // Permission name là "MasterResObjectTypeItem" (vì Default = GroupName)
            requiredPermission: 'MasterResObjectTypeItem',
            breadcrumb: 'Master::Menu:ResObjectTypeItem'
        }
    },
    {
        path: 'master/res-object-item-depreciations',
        component: ResObjectItemDepreciationsComponent,
        canActivate: [permissionGuard],
        data: {
            // Permission name là "MasterResObjectItemDepreciation" (vì Default = GroupName)
            requiredPermission: 'MasterResObjectItemDepreciation',
            breadcrumb: 'Master::Menu:ResObjectItemDepreciation'
        }
    },
    {
        path: 'customer/industries',
        component: IndustriesComponent,
        canActivate: [permissionGuard],
        data: {
            // Permission name là "ResIndustry" (vì Default = GroupName)
            requiredPermission: 'ResIndustry',
            breadcrumb: 'Customer::Menu:Industries'
        }
    },
    {
        path: 'customer/res-customers',
        component: ResCustomersComponent,
        canActivate: [permissionGuard],
        data: {
            // Permission name là "CustomerResCustomer" (vì Default = GroupName)
            requiredPermission: 'CustomerResCustomer',
            breadcrumb: 'Customer::Menu:ResCustomer'
        }
    },
    {
        path: 'product/pro-coverages',
        loadComponent: () => import('./product/pro-coverages/pro-coverages.component').then(m => m.ProCoveragesComponent),
        canActivate: [permissionGuard],
        data: {
            requiredPermission: 'Product.ProCoverage',
            breadcrumb: 'Product::Menu:ProCoverage'
        }
    },
    {
        path: 'product/pro-table-rates',
        loadComponent: () => import('./product/pro-table-rates/pro-table-rates.component').then(m => m.ProTableRatesComponent),
        canActivate: [permissionGuard],
        data: {
            requiredPermission: 'Product.ProTableRate',
            breadcrumb: 'Product::Menu:ProTableRate'
        }
    },
    {
        path: 'product/pro-products',
        loadComponent: () => import('./product/pro-products/pro-products.component').then(m => m.ProProductsComponent),
        canActivate: [permissionGuard],
        data: {
            requiredPermission: 'Product.ProProduct',
            breadcrumb: 'Product::Menu:ProProduct'
        }
    },
    {
        path: 'policy/contract-list',
        component: PolicyContractListComponent,
        canActivate: [permissionGuard],
        data: {
            requiredPermission: 'PolicyPolicyContract',
            breadcrumb: 'Policy::Menu:ContractList'
        }
    },
    {
        path: 'policy/contract/create',
        component: PolicyContractCreateComponent,
        canActivate: [permissionGuard],
        data: {
            requiredPermission: 'PolicyPolicyContract.Create',
            breadcrumb: 'Policy::Policy:AddContract'
        }
    },
    {
        path: 'policy/contract/edit/:id',
        component: PolicyContractCreateComponent,
        canActivate: [permissionGuard],
        data: {
            requiredPermission: 'PolicyPolicyContract.Edit',
            breadcrumb: 'Policy::Policy:EditContract'
        }
    },
    {
        path: 'policy/contract/:id',
        component: PolicyContractDetailComponent,
        canActivate: [permissionGuard],
        data: {
            requiredPermission: 'PolicyPolicyContract',
            breadcrumb: 'Policy::Policy:ContractDetail'
        }
    },
    {
        path: 'policy/policy-types',
        component: PolicyTypesComponent,
        canActivate: [permissionGuard],
        data: {
            requiredPermission: 'PolicyPolicyType',
            breadcrumb: 'Policy::Menu:PolicyType'
        }
    },
    {
        path: 'policy/policies',
        component: PoliciesComponent,
        canActivate: [permissionGuard],
        data: {
            requiredPermission: 'PolicyPolicy',
            breadcrumb: 'Policy::Menu:Policy'
        }
    },
    {
        path: 'policy/motorbike-policies',
        component: PoliciesComponent,
        canActivate: [permissionGuard],
        data: {
            requiredPermission: 'PolicyPolicy',
            breadcrumb: 'Policy::Menu:MotorbikePolicy'
        }
    },
    {
        path: 'policy/request-approval',
        component: PolicyRequestApprovalComponent,
        canActivate: [permissionGuard],
        data: {
            requiredPermission: 'PolicyPolicy.RequestApproval.View',
            breadcrumb: 'Policy::Menu:RequestApproval'
        }
    },
    {
        path: 'policy/terminate-approval',
        component: PolicyRequestApprovalComponent,
        canActivate: [permissionGuard],
        data: {
            requiredPermission: 'PolicyPolicy.TerminateApproval.View',
            breadcrumb: 'Policy::Menu:TerminateApproval',
            approvalType: 'terminate'
        }
    },
    {
        path: 'policy/endorsement-approval',
        component: PolicyRequestApprovalComponent,
        canActivate: [permissionGuard],
        data: {
            requiredPermission: 'PolicyPolicy.EndorsementApproval.View',
            breadcrumb: 'Policy::Menu:EndorsementApproval',
            approvalType: 'endorsement'
        }
    },
    {
        path: 'policy/policy-request-approval/detail/:id',
        loadComponent: () =>
            import('./policy/policy-request-approval/policy-request-approval-detail/policy-request-approval-detail.component').then(
                m => m.PolicyRequestApprovalDetailComponent
            ),
        canActivate: [permissionGuard],
        data: {
            requiredPermission: 'PolicyPolicy.RequestApproval.View',
            breadcrumb: 'Policy::Policy:View'
        }
    },
    {
        path: 'policy/policies/new',
        loadComponent: () =>
            import('./policy/policies/policy-form-page/policy-form-page.component').then(
                m => m.PolicyFormPageComponent
            ),
        canActivate: [permissionGuard],
        data: {
            requiredPermission: 'PolicyPolicy.Create',
            breadcrumb: 'Policy::Policy:New',
            mode: 'create'
        }
    },
    {
        path: 'policy/policies/:id/edit',
        loadComponent: () =>
            import('./policy/policies/policy-form-page/policy-form-page.component').then(
                m => m.PolicyFormPageComponent
            ),
        canActivate: [permissionGuard],
        data: {
            requiredPermission: 'PolicyPolicy.Edit',
            breadcrumb: 'Policy::Policy:Edit',
            mode: 'edit'
        }
    },
    {
        path: 'policy/policies/:id/view',
        loadComponent: () =>
            import('./policy/policies/policy-form-page/policy-form-page.component').then(
                m => m.PolicyFormPageComponent
            ),
        canActivate: [permissionGuard],
        data: {
            requiredPermission: 'PolicyPolicy.View',
            breadcrumb: 'Policy::Policy:View',
            mode: 'view'
        }
    },
    {
        path: 'policy/policies/:id/termination',
        loadComponent: () =>
            import('./policy/policies/termination-page/policy-termination-page.component').then(
                m => m.PolicyTerminationPageComponent
            ),
        canActivate: [permissionGuard],
        data: {
            requiredPermission: 'PolicyPolicy.Edit',
            breadcrumb: 'Policy::Policy:TerminatePolicy',
            mode: 'termination'
        }
    },
    {
        path: 'policy/policies/:id/endorsement',
        loadComponent: () =>
            import('./policy/policies/policy-endorsement-page/policy-endorsement-page.component').then(
                m => m.PolicyEndorsementPageComponent
            ),
        canActivate: [permissionGuard],
        data: {
            requiredPermission: 'PolicyPolicy.Edit',
            breadcrumb: 'Policy::Policy:Endorsement'
        }
    },
    {
        path: 'policy/motorbike-policies/new',
        loadComponent: () =>
            import('./policy/policies/policy-form-page/policy-form-page.component').then(
                m => m.PolicyFormPageComponent
            ),
        canActivate: [permissionGuard],
        data: {
            requiredPermission: 'PolicyPolicy.Create',
            breadcrumb: 'Policy::Policy:NewMotorbike',
            mode: 'create'
        }
    },
    {
        path: 'policy/motorbike-policies/:id/edit',
        loadComponent: () =>
            import('./policy/policies/policy-form-page/policy-form-page.component').then(
                m => m.PolicyFormPageComponent
            ),
        canActivate: [permissionGuard],
        data: {
            requiredPermission: 'PolicyPolicy.Edit',
            breadcrumb: 'Policy::Policy:EditMotorbike',
            mode: 'edit'
        }
    },
    {
        path: 'policy/motorbike-policies/:id/view',
        loadComponent: () =>
            import('./policy/policies/policy-form-page/policy-form-page.component').then(
                m => m.PolicyFormPageComponent
            ),
        canActivate: [permissionGuard],
        data: {
            requiredPermission: 'PolicyPolicy.View',
            breadcrumb: 'Policy::Policy:ViewMotorbike',
            mode: 'view'
        }
    },
    {
        path: 'policy/motorbike-policies/:id/termination',
        loadComponent: () =>
            import('./policy/policies/termination-page/policy-termination-page.component').then(
                m => m.PolicyTerminationPageComponent
            ),
        canActivate: [permissionGuard],
        data: {
            requiredPermission: 'PolicyPolicy.Edit',
            breadcrumb: 'Policy::Policy:TerminatePolicy',
            mode: 'termination'
        }
    },
    {
        path: 'policy/motorbike-policies/:id/endorsement',
        loadComponent: () =>
            import('./policy/policies/policy-endorsement-page/policy-endorsement-page.component').then(
                m => m.PolicyEndorsementPageComponent
            ),
        canActivate: [permissionGuard],
        data: {
            requiredPermission: 'PolicyPolicy.Edit',
            breadcrumb: 'Policy::Policy:Endorsement'
        }
    },
    {
        path: 'report/template',
        component: ReportComponent,
        canActivate: [permissionGuard],
        data: {
            requiredPermission: 'ReportReportTemplate',
            breadcrumb: 'Report::Menu:ReportTemplate'
        }
    },
    {
        path: 'claim/list',
        component: ClaimListComponent,
        canActivate: [permissionGuard],
        data: {
            requiredPermission: 'ClaimClaim',
            breadcrumb: 'Claim::Menu:ClaimList'
        }
    },
    {
        path: 'claim/task-list',
        component: ClaimTaskListComponent,
        canActivate: [permissionGuard],
        data: {
            requiredPermission: 'ClaimClaim',
            breadcrumb: 'Claim::Menu:ClaimTaskList'
        }
    },
    {
        path: 'claim/onsite-assessment-list',
        loadComponent: () =>
            import('./claim/onsite-assessment-list/onsite-assessment-list.component').then(
                m => m.OnsiteAssessmentListComponent
            ),
        canActivate: [permissionGuard],
        data: {
            requiredPermission: 'ClaimClaim',
            breadcrumb: 'Claim::Menu:OnsiteAssessmentTaskList'
        }
    },
    {
        path: 'claim/detail-assessment-list',
        loadComponent: () =>
            import('./claim/detail-assessment-list/detail-assessment-list.component').then(
                m => m.DetailAssessmentListComponent
            ),
        canActivate: [permissionGuard],
        data: {
            requiredPermission: 'ClaimClaim',
            breadcrumb: 'Claim::Menu:DetailAssessmentTaskList'
        }
    },
    {
        path: 'claim/quotation-approval-list',
        loadComponent: () =>
            import('./claim/quotation-approval-list').then(m => m.QuotationApprovalListComponent),
        canActivate: [permissionGuard],
        data: {
            requiredPermission: 'ClaimClaim.QuotationApprovalList',
            breadcrumb: 'Claim::Menu:QuotationApprovalList'
        }
    },
    {
        path: 'claim/quotation-approval-detail/:claimId/:workTaskId',
        loadComponent: () =>
            import('./claim/quotation-approval-list/quotation-approval-detail.component').then(
                m => m.QuotationApprovalDetailComponent
            ),
        canActivate: [permissionGuard],
        data: {
            requiredPermission: 'ClaimClaim.QuotationApprovalList',
            breadcrumb: 'Claim::Menu:QuotationApprovalList'
        }
    },
    {
        path: 'claim/onsite-assessment-detail/:id/:workTaskId',
        loadComponent: () =>
            import('./claim/onsite-assessment-detail/onsite-assessment-detail.component').then(
                m => m.OnsiteAssessmentDetailComponent
            ),
        canActivate: [permissionGuard],
        data: {
            requiredPermission: 'ClaimClaim',
            breadcrumb: 'Claim::ViewClaimTask'
        }
    },
    {
        path: 'claim/onsite-assessment-detail/:id/:workTaskId/view',
        loadComponent: () =>
            import('./claim/onsite-assessment-detail/onsite-assessment-detail.component').then(
                m => m.OnsiteAssessmentDetailComponent
            ),
        canActivate: [permissionGuard],
        data: {
            requiredPermission: 'ClaimClaim',
            breadcrumb: 'Claim::OnsiteAssessmentDetailView'
        }
    },
    {
        path: 'claim/onsite-assessment-detail/:id/:workTaskId/process',
        loadComponent: () =>
            import('./claim/onsite-assessment-detail/onsite-assessment-detail.component').then(
                m => m.OnsiteAssessmentDetailComponent
            ),
        canActivate: [permissionGuard],
        data: {
            requiredPermission: 'ClaimClaim',
            breadcrumb: 'Claim::OnsiteAssessmentDetailProcess'
        }
    },
    {
        path: 'claim/task-detail/:id/:workTaskId',
        component: ClaimTaskDetailComponent,
        canActivate: [permissionGuard],
        data: {
            requiredPermission: 'ClaimClaim',
            breadcrumb: 'Claim::ViewClaimTask'
        }
    },
    {
        path: 'claim/create',
        component: ClaimCreateComponent,
        canActivate: [permissionGuard],
        data: {
            requiredPermission: 'ClaimClaim.Create',
            breadcrumb: 'Claim::AddClaim'
        }
    },
    {
        path: 'claim/detail/:id',
        component: ClaimDetailComponent,
        canActivate: [permissionGuard],
        data: {
            requiredPermission: 'ClaimClaim',
            breadcrumb: 'Claim::ViewClaim'
        }
    },
    {
        path: 'claim/edit/:id',
        component: ClaimCreateComponent,
        canActivate: [permissionGuard],
        data: {
            requiredPermission: 'ClaimClaim',
            breadcrumb: 'Claim::EditClaim'
        }
    },
    { path: '**', redirectTo: '/notfound' }
] as Routes;
