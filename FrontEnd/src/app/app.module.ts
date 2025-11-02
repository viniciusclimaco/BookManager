import { NgModule, LOCALE_ID } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from '@angular/common/http';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { registerLocaleData } from '@angular/common';
import localePt from '@angular/common/locales/pt';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { LivrosListComponent } from './components/livros/livros-list.component';
import { LivroFormComponent } from './components/livros/livro-form.component';
import { AutoresComponent } from './components/autores/autores.component';
import { AssuntosComponent } from './components/assuntos/assuntos.component';
import { RelatoriosComponent } from './components/relatorios/relatorios.component';

@NgModule({
  declarations: [
    AppComponent,
    LivrosListComponent,
    LivroFormComponent,
    AutoresComponent,
    AssuntosComponent,
    RelatoriosComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule
  ],
  providers: [
    { provide: LOCALE_ID, useValue: 'pt-BR' }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }

// Registra o locale pt-BR para pipes como currency, date, etc.
registerLocaleData(localePt, 'pt-BR');
