import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpModule } from '@angular/http';

import { AppComponent, MenuComponent, DemoComponent, MarkerOverlayComponent, MeasurementComponent } from './components/index';
import { MarkerProvider, SocketIO } from './services/index';

import { routing } from './routes';

@NgModule({
  imports:      [ BrowserModule, HttpModule, routing ],
  declarations: [ AppComponent, MenuComponent, DemoComponent, MarkerOverlayComponent, MeasurementComponent ],
  bootstrap:    [ AppComponent ],
  providers:    [ SocketIO, MarkerProvider ]
})
export class AppModule { }
