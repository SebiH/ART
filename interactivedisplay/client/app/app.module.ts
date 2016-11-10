import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpModule } from '@angular/http';

import { AppComponent, MenuComponent, DemoComponent } from './components/index';
import { SocketIO } from './services/index';

import { routing } from './routes';

@NgModule({
  imports:      [ BrowserModule, HttpModule, routing ],
  declarations: [ AppComponent, MenuComponent, DemoComponent ],
  bootstrap:    [ AppComponent ],
  providers:    [ SocketIO ]
})
export class AppModule { }
