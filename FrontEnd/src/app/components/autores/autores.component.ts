import { Component, OnInit } from '@angular/core';
import { AutoresService } from '../../services/autores.service';
import { AutorDto, CreateAutorDto, UpdateAutorDto } from '../../models/models';

@Component({
  selector: 'app-autores',
  templateUrl: './autores.component.html',
  styleUrls: ['./autores.component.css']
})
export class AutoresComponent implements OnInit {
  autores: AutorDto[] = [];
  loading = false;
  error: string | null = null;
  
  showForm = false;
  editingId: number | null = null;
  autorNome = '';

  constructor(private autoresService: AutoresService) {}

  ngOnInit(): void {
    this.loadAutores();
  }

  loadAutores(): void {
    this.loading = true;
    this.autoresService.getAll().subscribe({
      next: (data) => {
        this.autores = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Erro ao carregar autores';
        this.loading = false;
        console.error(err);
      }
    });
  }

  openForm(autor?: AutorDto): void {
    this.showForm = true;
    if (autor) {
      this.editingId = autor.idAutor;
      this.autorNome = autor.nome;
    } else {
      this.editingId = null;
      this.autorNome = '';
    }
  }

  closeForm(): void {
    this.showForm = false;
    this.editingId = null;
    this.autorNome = '';
  }

  saveAutor(): void {
    if (!this.autorNome.trim()) {
      return;
    }

    if (this.editingId) {
      const updateDto: UpdateAutorDto = {
        nome: this.autorNome,
        ativo: true
      };
      this.autoresService.update(this.editingId, updateDto).subscribe({
        next: () => {
          this.loadAutores();
          this.closeForm();
        },
        error: (err) => {
          this.error = 'Erro ao atualizar autor';
          console.error(err);
        }
      });
    } else {
      const createDto: CreateAutorDto = {
        nome: this.autorNome
      };
      this.autoresService.create(createDto).subscribe({
        next: () => {
          this.loadAutores();
          this.closeForm();
        },
        error: (err) => {
          this.error = 'Erro ao criar autor';
          console.error(err);
        }
      });
    }
  }

  toggleStatus(autor: AutorDto): void {
    const updateDto: UpdateAutorDto = {
      nome: autor.nome,
      ativo: !autor.ativo
    };
    this.autoresService.update(autor.idAutor, updateDto).subscribe({
      next: () => {
        this.loadAutores();
      },
      error: (err) => {
        this.error = 'Erro ao atualizar status';
        console.error(err);
      }
    });
  }

  deleteAutor(id: number): void {
    if (confirm('Tem certeza que deseja excluir este autor?')) {
      this.autoresService.delete(id).subscribe({
        next: () => {
          this.loadAutores();
        },
        error: (err) => {
          this.error = 'Erro ao excluir autor';
          console.error(err);
        }
      });
    }
  }
}
