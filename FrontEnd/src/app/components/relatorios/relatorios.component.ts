import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { RelatoriosService } from '../../services/relatorios.service';
import { AssuntosService } from '../../services/assuntos.service';
import { AutoresService } from '../../services/autores.service';
import { FormasPagamentoService } from '../../services/formas-pagamento.service';
import { AssuntoDto, AutorDto, FormaPagamentoDto } from '../../models/models';

@Component({
  selector: 'app-relatorios',
  templateUrl: './relatorios.component.html',
  styleUrls: ['./relatorios.component.css']
})
export class RelatoriosComponent implements OnInit {
  // Formulários para cada relatório
  formRelatorio1!: FormGroup;
  formRelatorio2!: FormGroup;
  formRelatorio3!: FormGroup;

  // Dados para selects
  assuntos: AssuntoDto[] = [];
  autores: AutorDto[] = [];
  formasPagamento: FormaPagamentoDto[] = [];

  // Estados de loading
  loadingRelatorio1 = false;
  loadingRelatorio2 = false;
  loadingRelatorio3 = false;

  // Mensagens de erro
  errorRelatorio1: string | null = null;
  errorRelatorio2: string | null = null;
  errorRelatorio3: string | null = null;

  // Mensagens de sucesso
  successRelatorio1: string | null = null;
  successRelatorio2: string | null = null;
  successRelatorio3: string | null = null;

  constructor(
    private fb: FormBuilder,
    private relatoriosService: RelatoriosService,
    private assuntosService: AssuntosService,
    private autoresService: AutoresService,
    private formasPagamentoService: FormasPagamentoService
  ) {}

  ngOnInit(): void {
    this.initForms();
    this.loadDependencies();
  }

  initForms(): void {
    // Formulário Relatório 1: Livros por Assunto
    this.formRelatorio1 = this.fb.group({
      formato: ['PDF'],
      idAssunto: [null],
      dataInicio: [null],
      dataFim: [null],
      apenasAtivos: [true]
    });

    // Formulário Relatório 2: Autores por Livro
    this.formRelatorio2 = this.fb.group({
      formato: ['PDF'],
      idAutor: [null],
      ordenarPor: ['autor']
    });

    // Formulário Relatório 3: Livros com Preço
    this.formRelatorio3 = this.fb.group({
      formato: ['PDF'],
      valorMinimo: [null],
      valorMaximo: [null],
      idFormaPagamento: [null],
      apenasAtivos: [true]
    });
  }

  loadDependencies(): void {
    this.assuntosService.getAtivos().subscribe({
      next: (data) => this.assuntos = data,
      error: (err) => console.error('Erro ao carregar assuntos', err)
    });

    this.autoresService.getAtivos().subscribe({
      next: (data) => this.autores = data,
      error: (err) => console.error('Erro ao carregar autores', err)
    });

    this.formasPagamentoService.getAtivos().subscribe({
      next: (data) => this.formasPagamento = data,
      error: (err) => console.error('Erro ao carregar formas de pagamento', err)
    });
  }

  // Gerar Relatório 1: Livros por Assunto
  gerarRelatorio1(): void {
    this.loadingRelatorio1 = true;
    this.errorRelatorio1 = null;
    this.successRelatorio1 = null;

    const params = this.formRelatorio1.value;
    // Força o formato para PDF independentemente da seleção (outros formatos indisponíveis)
    params.formato = 'PDF';

    this.relatoriosService.gerarRelatorioLivrosPorAssunto(params).subscribe({
      next: (blob) => {
        const nomeArquivo = this.relatoriosService.obterNomeArquivo(
          'Relatorio_Livros_Por_Assunto',
          params.formato
        );
        this.relatoriosService.downloadArquivo(blob, nomeArquivo);
        this.successRelatorio1 = 'Relatório gerado com sucesso!';
        this.loadingRelatorio1 = false;

        setTimeout(() => this.successRelatorio1 = null, 5000);
      },
      error: (err) => {
        this.errorRelatorio1 = 'Erro ao gerar relatório. Verifique se o backend está rodando.';
        this.loadingRelatorio1 = false;
        console.error(err);
      }
    });
  }

  // Gerar Relatório 2: Autores por Livro
  gerarRelatorio2(): void {
    this.loadingRelatorio2 = true;
    this.errorRelatorio2 = null;
    this.successRelatorio2 = null;

    const params = this.formRelatorio2.value;
    // Força o formato para PDF independentemente da seleção (outros formatos indisponíveis)
    params.formato = 'PDF';

    this.relatoriosService.gerarRelatorioAutoresPorLivro(params).subscribe({
      next: (blob) => {
        const nomeArquivo = this.relatoriosService.obterNomeArquivo(
          'Relatorio_Autores_Por_Livro',
          params.formato
        );
        this.relatoriosService.downloadArquivo(blob, nomeArquivo);
        this.successRelatorio2 = 'Relatório gerado com sucesso!';
        this.loadingRelatorio2 = false;

        setTimeout(() => this.successRelatorio2 = null, 5000);
      },
      error: (err) => {
        this.errorRelatorio2 = 'Erro ao gerar relatório. Verifique se o backend está rodando.';
        this.loadingRelatorio2 = false;
        console.error(err);
      }
    });
  }

  // Gerar Relatório 3: Livros com Preço
  gerarRelatorio3(): void {
    this.loadingRelatorio3 = true;
    this.errorRelatorio3 = null;
    this.successRelatorio3 = null;

    const params = this.formRelatorio3.value;
    // Força o formato para PDF independentemente da seleção (outros formatos indisponíveis)
    params.formato = 'PDF';

    this.relatoriosService.gerarRelatorioLivrosComPreco(params).subscribe({
      next: (blob) => {
        const nomeArquivo = this.relatoriosService.obterNomeArquivo(
          'Relatorio_Livros_Com_Preco',
          params.formato
        );
        this.relatoriosService.downloadArquivo(blob, nomeArquivo);
        this.successRelatorio3 = 'Relatório gerado com sucesso!';
        this.loadingRelatorio3 = false;

        setTimeout(() => this.successRelatorio3 = null, 5000);
      },
      error: (err) => {
        this.errorRelatorio3 = 'Erro ao gerar relatório. Verifique se o backend está rodando.';
        this.loadingRelatorio3 = false;
        console.error(err);
      }
    });
  }

  // Limpar formulários
  limparRelatorio1(): void {
    this.formRelatorio1.reset({ formato: 'PDF', apenasAtivos: true });
    this.errorRelatorio1 = null;
    this.successRelatorio1 = null;
  }

  limparRelatorio2(): void {
    this.formRelatorio2.reset({ formato: 'PDF', ordenarPor: 'autor' });
    this.errorRelatorio2 = null;
    this.successRelatorio2 = null;
  }

  limparRelatorio3(): void {
    this.formRelatorio3.reset({ formato: 'PDF', apenasAtivos: true });
    this.errorRelatorio3 = null;
    this.successRelatorio3 = null;
  }
}
