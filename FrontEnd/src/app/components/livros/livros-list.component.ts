import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { LivrosService } from '../../services/livros.service';
import { LivroDto } from '../../models/models';

@Component({
  selector: 'app-livros-list',
  templateUrl: './livros-list.component.html',
  styleUrls: ['./livros-list.component.css']
})
export class LivrosListComponent implements OnInit {
  livros: LivroDto[] = [];
  loading = false;
  error: string | null = null;

  constructor(
    private livrosService: LivrosService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadLivros();
  }

  loadLivros(): void {
    this.loading = true;
    this.error = null;
    
    this.livrosService.getAll().subscribe({
      next: (data) => {
        this.livros = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Erro ao carregar livros';
        this.loading = false;
        console.error(err);
      }
    });
  }

  viewDetails(id: number): void {
    this.router.navigate(['/livros', id]);
  }

  editLivro(id: number): void {
    this.router.navigate(['/livros/editar', id]);
  }

  deleteLivro(id: number): void {
    if (confirm('Tem certeza que deseja excluir este livro?')) {
      this.livrosService.delete(id).subscribe({
        next: () => {
          this.loadLivros();
        },
        error: (err) => {
          this.error = 'Erro ao excluir livro';
          console.error(err);
        }
      });
    }
  }

  createNew(): void {
    this.router.navigate(['/livros/novo']);
  }
}
