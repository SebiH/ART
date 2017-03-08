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
    Components.AdminPanelComponent,
    Components.AppComponent,
    Components.Chart1dComponent,
    Components.Chart2dComponent,
    Components.GraphCreateButtonComponent,
    Components.GraphDataSelectionComponent,
    Components.GraphDetailComponent,
    Components.GraphListComponent,
    Components.GraphListItemComponent,
    Components.GraphSectionComponent,
    Components.MarkerComponent,
    Components.MeasurementComponent,
    Components.MenuComponent,
    Components.ScatterPlotComponent,
    Components.SurfaceComponent,
    Directives.ChartDirective,
    Directives.MoveableDirective,
    Directives.TouchButtonDirective
  ],
  bootstrap:    [ Components.AppComponent ],
  providers:    [
    Services.DataFilter,
    Services.GraphDataProvider,
    Services.GraphProvider,
    Services.InteractionManager,
    Services.Logger,
    Services.MapProvider,
    Services.MarkerProvider,
    Services.SocketIO,
    Services.SurfaceProvider
  ]
})
export class AppModule { }
