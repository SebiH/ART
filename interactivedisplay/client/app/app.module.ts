import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpModule } from '@angular/http';

import * as Components from './components/index';
import * as Directives from './directives/index';
import * as Services from './services/index';

import { routing } from './routes';

@NgModule({
  imports:      [ BrowserModule, HttpModule, routing ],
  declarations: [
    Components.AppComponent,
    Components.MenuComponent,
    Components.MarkerComponent,
    Components.MeasurementComponent,
    Components.GraphContainerComponent,
    Components.GraphSectionComponent,
    Components.GraphDataSelectionComponent,
    Components.GraphDetailComponent,
    Components.ScatterPlotComponent,
    Components.GraphOverviewComponent,
    Directives.MoveableDirective
  ],
  bootstrap:    [ Components.AppComponent ],
  providers:    [
    Services.SocketIO,
    Services.MarkerProvider,
    Services.MapProvider,
    Services.Logger,
    Services.InteractionManager,
    Services.GraphProvider,
    Services.GraphDataProvider,
    Services.SurfaceProvider
  ]
})
export class AppModule { }
