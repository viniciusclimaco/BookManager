USE [BookManager]
GO

-- ====================================================================
-- View 1: Livros Agrupados por Assunto
-- Para o relatório de livros organizados por categoria
-- ====================================================================
IF OBJECT_ID('dbo.vw_RelatorioLivrosPorAssunto', 'V') IS NOT NULL
    DROP VIEW dbo.vw_RelatorioLivrosPorAssunto;
GO

CREATE VIEW dbo.vw_RelatorioLivrosPorAssunto
AS
SELECT 
    A.IdAssunto,
    A.Descricao AS Assunto,
    L.IdLivro,
    L.Titulo,
    L.Editora,
    L.ISBN,
    L.AnoPublicacao,
    STUFF((
        SELECT ', ' + Au.Nome
        FROM LivroAutor LA2
        INNER JOIN Autor Au ON LA2.IdAutor = Au.IdAutor
        WHERE LA2.IdLivro = L.IdLivro
        ORDER BY LA2.Ordem
        FOR XML PATH(''), TYPE
    ).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS Autores,
    L.Ativo
FROM Livro L
INNER JOIN Assunto A ON L.IdAssunto = A.IdAssunto
GO

-- ====================================================================
-- View 2: Autores com seus Livros
-- Para o relatório de autores e suas obras
-- ====================================================================
IF OBJECT_ID('dbo.vw_RelatorioAutoresPorLivro', 'V') IS NOT NULL
    DROP VIEW dbo.vw_RelatorioAutoresPorLivro;
GO

CREATE VIEW dbo.vw_RelatorioAutoresPorLivro
AS
SELECT 
    Au.IdAutor,
    Au.Nome AS Autor,
    L.IdLivro,
    L.Titulo,
    L.Editora,
    A.Descricao AS Assunto,
    L.AnoPublicacao,
    L.ISBN,
    LA.Ordem AS OrdemAutor
FROM Autor Au
LEFT JOIN LivroAutor LA ON Au.IdAutor = LA.IdAutor
LEFT JOIN Livro L ON LA.IdLivro = L.IdLivro
LEFT JOIN Assunto A ON L.IdAssunto = A.IdAssunto
WHERE L.Ativo = 1 OR L.Ativo IS NULL
GO

-- ====================================================================
-- View 3: Livros com Preços por Forma de Pagamento
-- Para o relatório de livros com seus preços
-- ====================================================================
IF OBJECT_ID('dbo.vw_RelatorioLivrosComPreco', 'V') IS NOT NULL
    DROP VIEW dbo.vw_RelatorioLivrosComPreco;
GO

CREATE VIEW dbo.vw_RelatorioLivrosComPreco
AS
SELECT 
    L.IdLivro,
    L.Titulo,
    L.Editora,
    L.ISBN,
    L.AnoPublicacao,
    A.Descricao AS Assunto,
    STUFF((
        SELECT ', ' + Au.Nome
        FROM LivroAutor LA2
        INNER JOIN Autor Au ON LA2.IdAutor = Au.IdAutor
        WHERE LA2.IdLivro = L.IdLivro
        ORDER BY LA2.Ordem
        FOR XML PATH(''), TYPE
    ).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS Autores,
    FP.IdFormaPagamento,
    FP.Nome AS FormaPagamento,
    LP.Valor AS Preco
FROM Livro L
INNER JOIN Assunto A ON L.IdAssunto = A.IdAssunto
INNER JOIN LivroPreco LP ON L.IdLivro = LP.IdLivro
INNER JOIN FormaPagamento FP ON LP.IdFormaPagamento = FP.IdFormaPagamento
WHERE L.Ativo = 1
GO

PRINT '✅ Views para relatórios criadas com sucesso!'
PRINT ''
PRINT 'Views criadas:'
PRINT '  - vw_RelatorioLivrosPorAssunto'
PRINT '  - vw_RelatorioAutoresPorLivro'
PRINT '  - vw_RelatorioLivrosComPreco'
