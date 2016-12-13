import { Routes, RouterModule } from '@angular/router';
import {
    AppComponent,
    MenuComponent,
    DemoComponent,
    MeasurementComponent,
    PanZoomComponent,
    ScrollerComponent,
    GraphContainerComponent
} from './components/index';

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
        path: 'measurement',
        component: MeasurementComponent
    },
    {
        path: 'panzoom',
        component: PanZoomComponent
    },
    {
        path: 'scroll',
        component: ScrollerComponent
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
