import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LivrosListComponent } from './components/livros/livros-list.component';
import { LivroFormComponent } from './components/livros/livro-form.component';
import { AutoresComponent } from './components/autores/autores.component';
import { AssuntosComponent } from './components/assuntos/assuntos.component';
import { RelatoriosComponent } from './components/relatorios/relatorios.component';

const routes: Routes = [
  { path: '', redirectTo: '/livros', pathMatch: 'full' },
  { path: 'livros', component: LivrosListComponent },
  { path: 'livros/novo', component: LivroFormComponent },
  // Rota para detalhes simples do livro (utiliza o mesmo formulário em modo de edição)
  { path: 'livros/:id', component: LivroFormComponent },
  { path: 'livros/editar/:id', component: LivroFormComponent },
  { path: 'autores', component: AutoresComponent },
  { path: 'assuntos', component: AssuntosComponent },
  { path: 'relatorios', component: RelatoriosComponent }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
