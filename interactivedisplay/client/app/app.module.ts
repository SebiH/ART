import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { HttpModule } from '@angular/http';

import * as Components from './components/index';
import * as Directives from './directives/index';
import * as Services from './services/index';

import { routing } from './routes';

@NgModule({
  imports:      [ BrowserModule, HttpModule, FormsModule, routing ],
  declarations: [
    Components.AdminCalibrationComponent,
    Components.AdminCameraComponent,
    Components.AdminChangeMonitorComponent,
    Components.AdminDataComponent,
    Components.AdminPanelComponent,
    Components.AdminTableSetupComponent,
    Components.AppComponent,
    Components.CategoryOverviewChartComponent,
    Components.Chart1dComponent,
    Components.Chart2dComponent,
    Components.FilterMenuComponent,
    Components.GraphCreateButtonComponent,
    Components.GraphDataSelectionComponent,
    Components.GraphDetailComponent,
    Components.GraphDimensionSelectorComponent,
    Components.GraphListComponent,
    Components.GraphListItemComponent,
    Components.GraphOverviewChartComponent,
    Components.GraphOverviewChartComponent,
    Components.GraphSectionComponent,
    Components.MarkerComponent,
    Components.MarkerOverlayComponent,
    Components.MarkerTestComponent,
    Components.MeasurementComponent,
    Components.MenuComponent,
    Components.MetricOverviewChartComponent,
    Components.SurfaceComponent,
    Directives.ChartDirective,
    Directives.MoveableDirective,
    Directives.TouchButtonDirective
  ],
  bootstrap:    [ Components.AppComponent ],
  providers:    [
    Services.DataProvider,
    Services.FilterProvider,
    Services.GlobalFilterProvider,
    Services.GraphProvider,
    Services.InteractionManager,
    Services.Logger,
    Services.MapProvider,
    Services.MarkerProvider,
    Services.ServiceLoader,
    Services.SettingsProvider,
    Services.SocketIO,
    Services.SurfaceProvider
  ]
})
export class AppModule { }
