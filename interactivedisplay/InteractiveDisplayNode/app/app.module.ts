import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { AppComponent, MenuComponent } from './components/index';

import { routing } from './routes';

@NgModule({
  imports:      [ BrowserModule, routing ],
  declarations: [ AppComponent, MenuComponent ],
  bootstrap:    [ AppComponent ],
  providers:    [ ]
})
export class AppModule { }
