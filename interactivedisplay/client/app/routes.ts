import { Routes, RouterModule } from '@angular/router';
import {
    AdminPanelComponent,
    AppComponent,
    MarkerTestComponent,
    MenuComponent,
    MeasurementComponent,
    SurfaceComponent
} from './components/index';

const routes: Routes = [
    {
        path: 'menu',
        component: MenuComponent
    },
    {
        path: 'admin',
        component: AdminPanelComponent
    },
    {
        path: 'measurement',
        component: MeasurementComponent
    },
    {
        path: 'graph',
        component: SurfaceComponent
    },
    {
        path: 'markers',
        component: MarkerTestComponent
    },
    {
        path: '',
        redirectTo: '/menu',
        pathMatch: 'full'
    }
];

export const routing = RouterModule.forRoot(routes);
