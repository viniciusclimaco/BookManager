-- ====================================================================
-- BookManager Database - Initial Script
-- SQL Server 2022
-- ====================================================================

-- Drop database if exists (for fresh setup)
IF EXISTS(SELECT * FROM sys.databases WHERE name = 'BookManager')
BEGIN
    ALTER DATABASE [BookManager] SET SINGLE_USER WITH ROLLBACK IMMEDIATE
    DROP DATABASE [BookManager]
END
GO

-- Create database
CREATE DATABASE [BookManager]
GO

USE [BookManager]
GO

-- ====================================================================
-- Table: Autor
-- ====================================================================
CREATE TABLE [dbo].[Autor]
(
    [IdAutor] INT NOT NULL PRIMARY KEY IDENTITY(1,1),
    [Nome] NVARCHAR(255) NOT NULL,
    [DataCadastro] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [Ativo] BIT NOT NULL DEFAULT 1
)
GO

-- ====================================================================
-- Table: Assunto
-- ====================================================================
CREATE TABLE [dbo].[Assunto]
(
    [IdAssunto] INT NOT NULL PRIMARY KEY IDENTITY(1,1),
    [Descricao] NVARCHAR(255) NOT NULL,
    [DataCadastro] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [Ativo] BIT NOT NULL DEFAULT 1
)
GO

-- ====================================================================
-- Table: Livro
-- ====================================================================
CREATE TABLE [dbo].[Livro]
(
    [IdLivro] INT NOT NULL PRIMARY KEY IDENTITY(1,1),
    [Titulo] NVARCHAR(255) NOT NULL,
    [Editora] NVARCHAR(255) NULL,
    [AnoPublicacao] INT NULL,
    [ISBN] NVARCHAR(20) UNIQUE NULL,
    [IdAssunto] INT NOT NULL,
    [DataCadastro] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [Ativo] BIT NOT NULL DEFAULT 1,
    CONSTRAINT [FK_Livro_Assunto] FOREIGN KEY ([IdAssunto]) 
        REFERENCES [dbo].[Assunto]([IdAssunto])
)
GO

-- ====================================================================
-- Table: LivroAutor (Junction table - Many to Many)
-- ====================================================================
CREATE TABLE [dbo].[LivroAutor]
(
    [IdLivroAutor] INT NOT NULL PRIMARY KEY IDENTITY(1,1),
    [IdLivro] INT NOT NULL,
    [IdAutor] INT NOT NULL,
    [Ordem] INT NOT NULL DEFAULT 1,
    [DataCadastro] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [FK_LivroAutor_Livro] FOREIGN KEY ([IdLivro]) 
        REFERENCES [dbo].[Livro]([IdLivro]) ON DELETE CASCADE,
    CONSTRAINT [FK_LivroAutor_Autor] FOREIGN KEY ([IdAutor]) 
        REFERENCES [dbo].[Autor]([IdAutor]) ON DELETE CASCADE,
    CONSTRAINT [UQ_LivroAutor] UNIQUE ([IdLivro], [IdAutor])
)
GO

-- ====================================================================
-- Table: FormaPagamento (Nova tabela para formas de pagamento)
-- ====================================================================
CREATE TABLE [dbo].[FormaPagamento]
(
    [IdFormaPagamento] INT NOT NULL PRIMARY KEY IDENTITY(1,1),
    [Nome] NVARCHAR(100) NOT NULL UNIQUE,
    [Descricao] NVARCHAR(255) NULL,
    [Ativo] BIT NOT NULL DEFAULT 1
)
GO

-- ====================================================================
-- Table: LivroPreco (Preços por forma de pagamento)
-- ====================================================================
CREATE TABLE [dbo].[LivroPreco]
(
    [IdLivroPreco] INT NOT NULL PRIMARY KEY IDENTITY(1,1),
    [IdLivro] INT NOT NULL,
    [IdFormaPagamento] INT NOT NULL,
    [Valor] DECIMAL(10, 2) NOT NULL,
    [DataCadastro] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [DataAtualizacao] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [FK_LivroPreco_Livro] FOREIGN KEY ([IdLivro]) 
        REFERENCES [dbo].[Livro]([IdLivro]) ON DELETE CASCADE,
    CONSTRAINT [FK_LivroPreco_FormaPagamento] FOREIGN KEY ([IdFormaPagamento]) 
        REFERENCES [dbo].[FormaPagamento]([IdFormaPagamento]),
    CONSTRAINT [UQ_LivroPreco] UNIQUE ([IdLivro], [IdFormaPagamento])
)
GO

-- ====================================================================
-- Indices
-- ====================================================================
CREATE INDEX [IX_Livro_Titulo] ON [dbo].[Livro]([Titulo])
GO

CREATE INDEX [IX_Livro_ISBN] ON [dbo].[Livro]([ISBN])
GO

CREATE INDEX [IX_Livro_IdAssunto] ON [dbo].[Livro]([IdAssunto])
GO

CREATE INDEX [IX_LivroAutor_IdLivro] ON [dbo].[LivroAutor]([IdLivro])
GO

CREATE INDEX [IX_LivroAutor_IdAutor] ON [dbo].[LivroAutor]([IdAutor])
GO

CREATE INDEX [IX_LivroPreco_IdLivro] ON [dbo].[LivroPreco]([IdLivro])
GO

-- ====================================================================
-- View: vw_LivroDetalhado
-- Para relatórios com informações agregadas
-- ====================================================================
CREATE VIEW [dbo].[vw_LivroDetalhado]
AS
SELECT 
    L.[IdLivro],
    L.[Titulo],
    L.[Editora],
    L.[AnoPublicacao],
    L.[ISBN],
    A.[IdAutor],
    A.[Nome] AS [AutorNome],
    LA.[Ordem] AS [AutorOrdem],
    AS2.[IdAssunto],
    AS2.[Descricao] AS [AssuntoDescricao],
    FP.[IdFormaPagamento],
    FP.[Nome] AS [FormaPagamentoNome],
    LP.[Valor] AS [Preco],
    L.[DataCadastro],
    L.[Ativo]
FROM [dbo].[Livro] L
INNER JOIN [dbo].[Assunto] AS2 ON L.[IdAssunto] = AS2.[IdAssunto]
LEFT JOIN [dbo].[LivroAutor] LA ON L.[IdLivro] = LA.[IdLivro]
LEFT JOIN [dbo].[Autor] A ON LA.[IdAutor] = A.[IdAutor]
LEFT JOIN [dbo].[LivroPreco] LP ON L.[IdLivro] = LP.[IdLivro]
LEFT JOIN [dbo].[FormaPagamento] FP ON LP.[IdFormaPagamento] = FP.[IdFormaPagamento]
GO

-- ====================================================================
-- Insert formas de pagamento padrão
-- ====================================================================
INSERT INTO [dbo].[FormaPagamento] ([Nome], [Descricao])
VALUES 
    ('Balcão', 'Compra no balcão da loja'),
    ('Self-Service', 'Compra via self-service'),
    ('Internet', 'Compra via internet'),
    ('Evento', 'Compra em evento'),
    ('Fornecedor', 'Compra para fornecedor')
GO

-- ====================================================================
-- Sample data para testes
-- ====================================================================
INSERT INTO [dbo].[Autor] ([Nome])
VALUES 
    ('J.K. Rowling'),
    ('George R.R. Martin'),
    ('J.R.R. Tolkien')
GO

INSERT INTO [dbo].[Assunto] ([Descricao])
VALUES 
    ('Ficção Científica'),
    ('Fantasia'),
    ('Mistério'),
    ('Romance'),
    ('Suspense')
GO

INSERT INTO [dbo].[Livro] ([Titulo], [Editora], [AnoPublicacao], [ISBN], [IdAssunto])
VALUES 
    ('Harry Potter and the Sorcerer''s Stone', 'Bloomsbury', 1997, '978-0439708180', 2),
    ('A Song of Ice and Fire', 'Bantam Spectra', 1996, '978-0553103540', 2),
    ('The Lord of the Rings', 'Allen & Unwin', 1954, '978-0544003415', 2)
GO

INSERT INTO [dbo].[LivroAutor] ([IdLivro], [IdAutor], [Ordem])
VALUES 
    (1, 1, 1),
    (2, 2, 1),
    (3, 3, 1)
GO

INSERT INTO [dbo].[LivroPreco] ([IdLivro], [IdFormaPagamento], [Valor])
VALUES 
    (1, 1, 45.90),
    (1, 2, 42.90),
    (1, 3, 39.90),
    (1, 4, 44.90),
    (2, 1, 89.90),
    (2, 2, 85.90),
    (2, 3, 79.90),
    (3, 1, 125.90),
    (3, 2, 120.90),
    (3, 3, 115.90)
GO

PRINT 'Database BookManager created successfully!'
