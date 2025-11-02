import { Component, OnInit } from '@angular/core';
import { AssuntosService } from '../../services/assuntos.service';
import { AssuntoDto, CreateAssuntoDto, UpdateAssuntoDto } from '../../models/models';

@Component({
  selector: 'app-assuntos',
  templateUrl: './assuntos.component.html',
  styleUrls: ['./assuntos.component.css']
})
export class AssuntosComponent implements OnInit {
  assuntos: AssuntoDto[] = [];
  loading = false;
  error: string | null = null;
  
  showForm = false;
  editingId: number | null = null;
  assuntoDescricao = '';

  constructor(private assuntosService: AssuntosService) {}

  ngOnInit(): void {
    this.loadAssuntos();
  }

  loadAssuntos(): void {
    this.loading = true;
    this.assuntosService.getAll().subscribe({
      next: (data) => {
        this.assuntos = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Erro ao carregar assuntos';
        this.loading = false;
        console.error(err);
      }
    });
  }

  openForm(assunto?: AssuntoDto): void {
    this.showForm = true;
    if (assunto) {
      this.editingId = assunto.idAssunto;
      this.assuntoDescricao = assunto.descricao;
    } else {
      this.editingId = null;
      this.assuntoDescricao = '';
    }
  }

  closeForm(): void {
    this.showForm = false;
    this.editingId = null;
    this.assuntoDescricao = '';
  }

  saveAssunto(): void {
    if (!this.assuntoDescricao.trim()) {
      return;
    }

    if (this.editingId) {
      const updateDto: UpdateAssuntoDto = {
        descricao: this.assuntoDescricao,
        ativo: true
      };
      this.assuntosService.update(this.editingId, updateDto).subscribe({
        next: () => {
          this.loadAssuntos();
          this.closeForm();
        },
        error: (err) => {
          this.error = 'Erro ao atualizar assunto';
          console.error(err);
        }
      });
    } else {
      const createDto: CreateAssuntoDto = {
        descricao: this.assuntoDescricao
      };
      this.assuntosService.create(createDto).subscribe({
        next: () => {
          this.loadAssuntos();
          this.closeForm();
        },
        error: (err) => {
          this.error = 'Erro ao criar assunto';
          console.error(err);
        }
      });
    }
  }

  toggleStatus(assunto: AssuntoDto): void {
    const updateDto: UpdateAssuntoDto = {
      descricao: assunto.descricao,
      ativo: !assunto.ativo
    };
    this.assuntosService.update(assunto.idAssunto, updateDto).subscribe({
      next: () => {
        this.loadAssuntos();
      },
      error: (err) => {
        this.error = 'Erro ao atualizar status';
        console.error(err);
      }
    });
  }

  deleteAssunto(id: number): void {
    if (confirm('Tem certeza que deseja excluir este assunto?')) {
      this.assuntosService.delete(id).subscribe({
        next: () => {
          this.loadAssuntos();
        },
        error: (err) => {
          this.error = 'Erro ao excluir assunto';
          console.error(err);
        }
      });
    }
  }
}
