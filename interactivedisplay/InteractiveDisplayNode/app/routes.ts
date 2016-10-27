import { Routes, RouterModule } from '@angular/router';
import { AppComponent, MenuComponent } from './components/index';

const routes: Routes = [
    {
        path: 'menu',
        component: MenuComponent
    },
    {
        path: '',
        redirectTo: '/menu',
        pathMatch: 'full'
    }
];

export const routing = RouterModule.forRoot(routes);
