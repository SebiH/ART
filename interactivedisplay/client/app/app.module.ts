import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpModule } from '@angular/http';

import * as Components from './components/index';
import * as Services from './services/index';

import { routing } from './routes';

@NgModule({
  imports:      [ BrowserModule, HttpModule, routing ],
  declarations: [
    Components.AppComponent,
    Components.MenuComponent,
    Components.DemoComponent,
    Components.MarkerOverlayComponent,
    Components.MeasurementComponent,
    Components.PanZoomComponent,
    Components.MapOverlayComponent,
    Components.ScrollerComponent,
    Components.GraphContainerComponent,
    Components.GraphSectionComponent
  ],
  bootstrap:    [ Components.AppComponent ],
  providers:    [
    Services.SocketIO,
    Services.MarkerProvider,
    Services.MapProvider,
    Services.Logger,
    Services.InteractionManager,
    Services.GraphProvider
  ]
})
export class AppModule { }
