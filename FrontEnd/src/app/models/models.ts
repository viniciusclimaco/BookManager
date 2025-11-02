// DTOs de Assunto
export interface AssuntoDto {
  idAssunto: number;
  descricao: string;
  ativo: boolean;
}

export interface CreateAssuntoDto {
  descricao: string;
}

export interface UpdateAssuntoDto {
  descricao: string;
  ativo: boolean;
}

// DTOs de Autor
export interface AutorDto {
  idAutor: number;
  nome: string;
  ativo: boolean;
}

export interface CreateAutorDto {
  nome: string;
}

export interface UpdateAutorDto {
  nome: string;
  ativo: boolean;
}

// DTOs de Forma de Pagamento
export interface FormaPagamentoDto {
  idFormaPagamento: number;
  nome: string;
  descricao: string;
  ativo: boolean;
}

// DTOs de Livro
export interface LivroAutorDto {
  idAutor: number;
  nome: string;
  ordem: number;
}

export interface LivroPrecoDto {
  idLivroPreco: number;
  idFormaPagamento: number;
  formaPagamentoNome: string;
  valor: number;
}

export interface CreateLivroPrecoDto {
  idFormaPagamento: number;
  valor: number;
}

export interface LivroDto {
  idLivro: number;
  titulo: string;
  editora: string;
  anoPublicacao?: number;
  isbn: string;
  idAssunto: number;
  assuntoDescricao: string;
  ativo: boolean;
  autores: LivroAutorDto[];
  precos: LivroPrecoDto[];
}

export interface CreateLivroDto {
  titulo: string;
  editora: string;
  anoPublicacao?: number;
  isbn: string;
  idAssunto: number;
  idAutores: number[];
  precos: CreateLivroPrecoDto[];
}

export interface UpdateLivroDto {
  titulo: string;
  editora: string;
  anoPublicacao?: number;
  isbn: string;
  idAssunto: number;
  ativo: boolean;
  idAutores: number[];
  precos: CreateLivroPrecoDto[];
}

export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
}
