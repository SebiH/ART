import { Routes, RouterModule } from '@angular/router';
import { AppComponent, MenuComponent, DemoComponent } from './components/index';

const routes: Routes = [
    {
        path: 'menu',
        component: MenuComponent
    },
    {
        path: 'demo',
        component: DemoComponent
    },
    {
        path: '',
        redirectTo: '/menu',
        pathMatch: 'full'
    }
];

export const routing = RouterModule.forRoot(routes);
