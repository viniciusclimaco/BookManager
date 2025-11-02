import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../config/api.config';
import {
  RelatorioLivrosPorAssuntoParams,
  RelatorioAutoresPorLivroParams,
  RelatorioLivrosComPrecoParams,
  RelatorioResponse
} from '../models/relatorios.models';

@Injectable({
  providedIn: 'root'
})
export class RelatoriosService {
  private apiUrl = `${environment.apiUrl}/Relatorios`;

  constructor(private http: HttpClient) {}

  /**
   * Relatório 1: Livros agrupados por Assunto
   * Mostra todos os livros organizados por assunto com suas informações básicas
   */
  gerarRelatorioLivrosPorAssunto(params: RelatorioLivrosPorAssuntoParams): Observable<Blob> {
    let httpParams = new HttpParams();
    httpParams = httpParams.set('formato', params.formato);

    if (params.idAssunto) {
      httpParams = httpParams.set('idAssunto', params.idAssunto.toString());
    }
    if (params.dataInicio) {
      httpParams = httpParams.set('dataInicio', params.dataInicio);
    }
    if (params.dataFim) {
      httpParams = httpParams.set('dataFim', params.dataFim);
    }
    if (params.apenasAtivos !== undefined) {
      httpParams = httpParams.set('apenasAtivos', params.apenasAtivos.toString());
    }

    return this.http.get(`${this.apiUrl}/livros-por-assunto`, {
      params: httpParams,
      responseType: 'blob'
    });
  }

  /**
   * Relatório 2: Autores com seus Livros
   * Lista autores e todos os livros que escreveram
   */
  gerarRelatorioAutoresPorLivro(params: RelatorioAutoresPorLivroParams): Observable<Blob> {
    let httpParams = new HttpParams();
    httpParams = httpParams.set('formato', params.formato);

    if (params.idAutor) {
      httpParams = httpParams.set('idAutor', params.idAutor.toString());
    }
    if (params.ordenarPor) {
      httpParams = httpParams.set('ordenarPor', params.ordenarPor);
    }

    return this.http.get(`${this.apiUrl}/autores-por-livro`, {
      params: httpParams,
      responseType: 'blob'
    });
  }

  /**
   * Relatório 3: Livros com Preços e Formas de Pagamento
   * Mostra livros com seus preços detalhados por forma de pagamento
   */
  gerarRelatorioLivrosComPreco(params: RelatorioLivrosComPrecoParams): Observable<Blob> {
    let httpParams = new HttpParams();
    httpParams = httpParams.set('formato', params.formato);

    // Envia apenas quando houver valor (diferente de null/undefined)
    if (params.valorMinimo != null) {
      httpParams = httpParams.set('valorMinimo', String(params.valorMinimo));
    }
    if (params.valorMaximo != null) {
      httpParams = httpParams.set('valorMaximo', String(params.valorMaximo));
    }
    if (params.idFormaPagamento != null) {
      httpParams = httpParams.set('idFormaPagamento', params.idFormaPagamento.toString());
    }
    if (params.apenasAtivos !== undefined) {
      httpParams = httpParams.set('apenasAtivos', params.apenasAtivos.toString());
    }

    return this.http.get(`${this.apiUrl}/livros-com-preco`, {
      params: httpParams,
      responseType: 'blob'
    });
  }

  /**
   * Método auxiliar para fazer download do blob
   */
  downloadArquivo(blob: Blob, nomeArquivo: string): void {
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = nomeArquivo;
    link.click();
    window.URL.revokeObjectURL(url);
  }

  /**
   * Método auxiliar para determinar o nome do arquivo baseado no formato
   */
  obterNomeArquivo(nomeRelatorio: string, formato: string): string {
    const timestamp = new Date().toISOString().slice(0, 19).replace(/:/g, '-');
    let extensao = 'pdf';

    switch (formato.toUpperCase()) {
      case 'EXCEL':
        extensao = 'xlsx';
        break;
      case 'WORD':
        extensao = 'docx';
        break;
      default:
        extensao = 'pdf';
    }

    return `${nomeRelatorio}_${timestamp}.${extensao}`;
  }
}
