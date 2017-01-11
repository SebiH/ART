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
    Components.DemoComponent,
    Components.MarkerComponent,
    Components.MarkerOverlayComponent,
    Components.MeasurementComponent,
    Components.PanZoomComponent,
    Components.MapOverlayComponent,
    Components.ScrollerComponent,
    Components.GraphContainerComponent,
    Components.GraphSectionComponent,
    Components.GraphCreateFormComponent,
    Components.GraphDataSelectionComponent,
    Components.GraphDetailComponent,
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
    Services.GraphDataProvider
  ]
})
export class AppModule { }
