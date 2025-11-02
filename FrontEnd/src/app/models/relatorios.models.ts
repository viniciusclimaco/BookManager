// Modelos para parâmetros de relatórios

export interface RelatorioParametros {
  formato: 'PDF' | 'EXCEL' | 'WORD';
}

export interface RelatorioLivrosPorAssuntoParams extends RelatorioParametros {
  idAssunto?: number;
  dataInicio?: string;
  dataFim?: string;
  apenasAtivos?: boolean;
}

export interface RelatorioAutoresPorLivroParams extends RelatorioParametros {
  idAutor?: number;
  ordenarPor?: 'autor' | 'livro' | 'ano';
}

export interface RelatorioLivrosComPrecoParams extends RelatorioParametros {
  valorMinimo?: number;
  valorMaximo?: number;
  idFormaPagamento?: number;
  apenasAtivos?: boolean;
}

export interface RelatorioResponse {
  nomeArquivo: string;
  conteudo: string; // Base64
  contentType: string;
}
