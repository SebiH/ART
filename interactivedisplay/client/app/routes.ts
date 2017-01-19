import { Routes, RouterModule } from '@angular/router';
import {
    AppComponent,
    MenuComponent,
    MeasurementComponent,
    GraphContainerComponent
} from './components/index';

const routes: Routes = [
    {
        path: 'menu',
        component: MenuComponent
    },
    {
        path: 'measurement',
        component: MeasurementComponent
    },
    {
        path: 'graph',
        component: GraphContainerComponent
    },
    {
        path: '',
        redirectTo: '/menu',
        pathMatch: 'full'
    }
];

export const routing = RouterModule.forRoot(routes);
