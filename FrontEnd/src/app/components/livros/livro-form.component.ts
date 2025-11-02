import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormArray, AbstractControl, ValidationErrors } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { LivrosService } from '../../services/livros.service';
import { AssuntosService } from '../../services/assuntos.service';
import { AutoresService } from '../../services/autores.service';
import { FormasPagamentoService } from '../../services/formas-pagamento.service';
import { AssuntoDto, AutorDto, FormaPagamentoDto, LivroDto } from '../../models/models';

@Component({
  selector: 'app-livro-form',
  templateUrl: './livro-form.component.html',
  styleUrls: ['./livro-form.component.css']
})
export class LivroFormComponent implements OnInit {
  livroForm!: FormGroup;
  isEditMode = false;
  // Quando acessado por /livros/:id, o formulário fica somente leitura
  readOnly = false;
  livroId?: number;
  loading = false;
  error: string | null = null;
  backendErrors: string[] = [];

  assuntos: AssuntoDto[] = [];
  autores: AutorDto[] = [];
  formasPagamento: FormaPagamentoDto[] = [];

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private livrosService: LivrosService,
    private assuntosService: AssuntosService,
    private autoresService: AutoresService,
    private formasPagamentoService: FormasPagamentoService
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.loadDependencies();

    const id = this.route.snapshot.paramMap.get('id');
    const routePath = this.route.snapshot.routeConfig?.path ?? '';

    // Define os modos conforme a rota
    if (routePath.includes('editar')) {
      this.isEditMode = true;
    } else if (routePath.includes(':id')) {
      this.readOnly = true;
    }

    if (id) {
      this.livroId = +id;
      this.loadLivro(this.livroId);
    }
  }

  initForm(): void {
    this.livroForm = this.fb.group({
      titulo: ['', [Validators.required, this.requiredTrimValidator]],
      editora: ['', [Validators.required, this.requiredTrimValidator]],
      anoPublicacao: [null],
      // Aceita 10 ou 13 dígitos, com ou sem hífens/espaços
      isbn: ['', [Validators.required, Validators.pattern(/^(?:\d{10}|\d{13}|(?:\d[ -]?){9}[\dxX]|(?:\d[ -]?){13})$/)]],
      idAssunto: [null, Validators.required],
      ativo: [true],
      idAutores: [[], Validators.required],
      precos: this.fb.array([], [this.minLengthArray(1)])
    });
  }

  get precos(): FormArray {
    return this.livroForm.get('precos') as FormArray;
  }

  // Helpers para exibição em modo somente leitura
  get assuntoDescricao(): string {
    const id = this.livroForm?.get('idAssunto')?.value as number | null;
    const assunto = this.assuntos.find(a => a.idAssunto === id);
    return assunto?.descricao ?? '-';
  }

  get autoresNomes(): string {
    const ids = (this.livroForm?.get('idAutores')?.value as number[] | null) ?? [];
    if (!Array.isArray(ids)) return '';
    return this.autores
      .filter(a => ids.includes(a.idAutor))
      .map(a => a.nome)
      .join(', ');
  }

  formaPagamentoNome(id?: number | null): string {
    if (id === undefined || id === null) return '-';
    const fp = this.formasPagamento.find(f => f.idFormaPagamento === id);
    return fp?.nome ?? '-';
  }

  loadDependencies(): void {
    this.assuntosService.getAtivos().subscribe(data => this.assuntos = data);
    this.autoresService.getAtivos().subscribe(data => this.autores = data);
    this.formasPagamentoService.getAtivos().subscribe(data => {
      this.formasPagamento = data;
      if (!this.isEditMode) {
        this.addPrecoField();
      }
    });
  }

  loadLivro(id: number): void {
    this.loading = true;
    this.livrosService.getById(id).subscribe({
      next: (livro) => {
        this.livroForm.patchValue({
          titulo: livro.titulo,
          editora: livro.editora,
          anoPublicacao: livro.anoPublicacao,
          isbn: livro.isbn,
          idAssunto: livro.idAssunto,
          ativo: livro.ativo,
          idAutores: livro.autores.map(a => a.idAutor)
        });

        livro.precos.forEach(preco => {
          this.precos.push(this.fb.group({
            idFormaPagamento: [preco.idFormaPagamento, Validators.required],
            valor: [preco.valor, [Validators.required, Validators.min(0)]]
          }));
        });
        // Em modo somente leitura, desabilita o formulário após carregar os dados
        if (this.readOnly) {
          this.livroForm.disable({ emitEvent: false });
        }

        this.loading = false;
      },
      error: (err) => {
        this.error = 'Erro ao carregar livro';
        this.loading = false;
        console.error(err);
      }
    });
  }

  addPrecoField(): void {
    this.precos.push(this.fb.group({
      idFormaPagamento: [null, Validators.required],
      valor: [null, [Validators.required, Validators.min(0.01)]]
    }));
  }

  removePrecoField(index: number): void {
    this.precos.removeAt(index);
  }

  onSubmit(): void {
    if (this.livroForm.invalid) {
      this.livroForm.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.error = null;
    this.backendErrors = [];

    const formData = this.livroForm.value;

    if (this.isEditMode && this.livroId) {
      this.livrosService.update(this.livroId, formData).subscribe({
        next: () => {
          this.router.navigate(['/livros']);
        },
        error: (err) => {
          const { mensagem, mensagens } = this.parseBackendErrors(err);
          this.error = mensagem ?? 'Erro ao atualizar livro';
          this.backendErrors = mensagens;
          this.applyServerErrorsToControls(mensagens);
          this.loading = false;
          console.error(err);
        }
      });
    } else {
      this.livrosService.create(formData).subscribe({
        next: () => {
          this.router.navigate(['/livros']);
        },
        error: (err) => {
          const { mensagem, mensagens } = this.parseBackendErrors(err);
          this.error = mensagem ?? 'Erro ao criar livro';
          this.backendErrors = mensagens;
          this.applyServerErrorsToControls(mensagens);
          this.loading = false;
          console.error(err);
        }
      });
    }
  }

  cancel(): void {
    this.router.navigate(['/livros']);
  }

  /** Extrai mensagem geral e lista de mensagens do backend em diferentes formatos */
  private parseBackendErrors(err: any): { mensagem: string | null; mensagens: string[] } {
    try {
      const e = err?.error ?? err;
      let mensagem: string | null = null;
      let mensagens: string[] = [];

      if (!e) return { mensagem: null, mensagens: [] };

      if (typeof e === 'string') {
        mensagens = [e];
      } else if (Array.isArray(e)) {
        mensagens = e.filter((x) => typeof x === 'string');
      } else if (typeof e === 'object') {
        mensagem = e.message || e.mensagem || null;
        if (Array.isArray(e.erros)) mensagens = e.erros;
        else if (Array.isArray(e.errors)) mensagens = e.errors;
        else if (e.errors && typeof e.errors === 'object') {
          // Padrão ASP.NET: errors: { Field: ["msg1", "msg2"] }
          for (const key of Object.keys(e.errors)) {
            const arr = e.errors[key];
            if (Array.isArray(arr)) mensagens.push(...arr);
          }
        }
      }

      return { mensagem, mensagens };
    } catch {
      return { mensagem: null, mensagens: [] };
    }
  }

  /** Marca os controles relacionados como inválidos com a mensagem do servidor */
  private applyServerErrorsToControls(mensagens: string[]): void {
    if (!mensagens?.length) return;
    const map: { control: string; keywords: string[] }[] = [
      { control: 'isbn', keywords: ['isbn'] },
      { control: 'titulo', keywords: ['titulo', 'título'] },
      { control: 'editora', keywords: ['editora'] },
      { control: 'anoPublicacao', keywords: ['ano'] },
      { control: 'idAssunto', keywords: ['assunto'] },
      { control: 'idAutores', keywords: ['autor', 'autores'] }
    ];

    const lower = mensagens.map(m => m.toLowerCase());
    for (const m of map) {
      const hit = lower.find(msg => m.keywords.some(k => msg.includes(k)));
      if (hit) {
        const ctrl = this.livroForm.get(m.control);
        if (ctrl) {
          ctrl.setErrors({ ...(ctrl.errors || {}), server: mensagens[lower.indexOf(hit)] });
          ctrl.markAsTouched();
        }
      }
    }

    // Heurística: tratar violações de UNIQUE KEY/duplicate para ISBN
    const duplicate = mensagens.find(ms => /unique|duplicat(e|o)|constraint/i.test(ms));
    if (duplicate) {
      const ctrl = this.livroForm.get('isbn');
      if (ctrl) {
        const match = duplicate.match(/(\d{10,13})/);
        const isbnVal = match?.[1] ?? this.livroForm.get('isbn')?.value;
        const msg = `Este ISBN já está cadastrado${isbnVal ? `: ${isbnVal}` : ''}.`;
        ctrl.setErrors({ ...(ctrl.errors || {}), server: msg });
        ctrl.markAsTouched();
      }
    }
  }

  // -------- Validators auxiliares --------
  private requiredTrimValidator(control: AbstractControl): ValidationErrors | null {
    const v = (control.value ?? '').toString();
    return v.trim().length ? null : { requiredTrim: true };
  }

  private minLengthArray(min: number) {
    return (control: AbstractControl): ValidationErrors | null => {
      const arr = control as FormArray;
      if (!arr || !Array.isArray(arr.controls)) return { minLengthArray: { requiredLength: min } };
      return arr.length >= min ? null : { minLengthArray: { requiredLength: min } };
    };
  }

  // Formata o campo de valor para sempre exibir 2 casas decimais no input (sem alterar o tipo number do form)
  formatValor(index: number, event: Event): void {
    const input = event.target as HTMLInputElement;
    const ctrl = this.precos.at(index).get('valor');
    if (!ctrl) return;
    const value = parseFloat(input.value.replace(',', '.'));
    if (isNaN(value)) {
      input.value = '';
      ctrl.setValue(null);
      return;
    }
    // mantém no formulário como número, mas exibe com 2 casas no input
    ctrl.setValue(value);
    input.value = value.toFixed(2);
  }
}
