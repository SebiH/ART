import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpModule } from '@angular/http';

import { AppComponent, MenuComponent, DemoComponent, MarkerOverlayComponent } from './components/index';
import { SocketIO } from './services/index';

import { routing } from './routes';

@NgModule({
  imports:      [ BrowserModule, HttpModule, routing ],
  declarations: [ AppComponent, MenuComponent, DemoComponent, MarkerOverlayComponent ],
  bootstrap:    [ AppComponent ],
  providers:    [ SocketIO ]
})
export class AppModule { }
