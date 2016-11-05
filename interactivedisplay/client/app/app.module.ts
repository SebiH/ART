import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpModule } from '@angular/http';
import { AppComponent, MenuComponent, DemoComponent } from './components/index';

import { routing } from './routes';

@NgModule({
  imports:      [ BrowserModule, HttpModule, routing ],
  declarations: [ AppComponent, MenuComponent, DemoComponent ],
  bootstrap:    [ AppComponent ],
  providers:    [ ]
})
export class AppModule { }
