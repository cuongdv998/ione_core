import { Routes } from '@angular/router';
import { AppLayout } from './app/layout/component/app.layout';
import { Notfound } from './app/pages/notfound/notfound';
import { authGuard } from './app/core/guards/auth.guard';
import { ResetPassword } from './app/pages/auth/reset-password';

export const appRoutes: Routes = [
    {
        path: '',
        component: AppLayout,
        canActivate: [authGuard],
        children: [
            { path: '', redirectTo: '/pages/dashboard', pathMatch: 'full' },
            { 
                path: 'pages', 
                loadChildren: () => import('./app/pages/pages.routes'),
                data: {
                    breadcrumb: 'iOne::Menu:Administration'
                }
            }
        ]
    },
    { path: 'notfound', component: Notfound },
    { path: 'account/reset-password', component: ResetPassword },
    { path: 'auth', loadChildren: () => import('./app/pages/auth/auth.routes') },
    { path: '**', redirectTo: '/notfound' }
];
