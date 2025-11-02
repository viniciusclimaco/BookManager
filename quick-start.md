# GUIA DE INÍCIO RÁPIDO - Docker

### Opção 1: Full Stack (Recomendado)

```bash
# Na raiz do projeto
docker-compose up -d --build

# Aguarde ~2 minutos para todos os serviços iniciarem
# Acesse: http://localhost:4200
```

### Opção 2: Backend Only

```bash
cd Backend
docker-compose up -d --build
cd ..

# Backend disponível em: http://localhost:5000
# Rode frontend localmente: cd FrontEnd && npm start
```

---

## Pré-requisitos

Antes de começar, certifique-se de ter instalado:

- ✅ [Docker Desktop](https://www.docker.com/products/docker-desktop) (Windows/Mac)
- ✅ Docker Engine + Docker Compose (Linux)
- ✅ 8GB RAM mínimo (SQL Server requer ~4GB)
- ✅ 10GB espaço em disco livre

**Verificar instalação:**
```bash
docker --version
docker-compose --version
```

---

## Estrutura do Projeto

```
BookManager/
│
├── docker-compose.yml              # Full Stack
├── docker-manager.sh               # Script helper (Linux/Mac)
├── docker-manager.bat              # Script helper (Windows)
├── .env.example                    # Variáveis de ambiente
│
├── Backend/
│   ├── Dockerfile                  # Build do .NET 8
│   ├── docker-compose.yml          # Backend Only
│   └── init-db/                    # Scripts SQL
│
└── FrontEnd/
    ├── Dockerfile                  # Build do Angular 16
    └── nginx.conf                  # Config Nginx
```

---


## Método 2: Comandos Manuais

### Subir Full Stack

```bash
# Build e start
docker-compose up -d --build

# Verificar status
docker-compose ps

# Ver logs
docker-compose logs -f

# Parar
docker-compose down
```

### Subir Backend Only

```bash
cd Backend
docker-compose up -d --build
docker-compose logs -f
```

---

## Verificar se Está Funcionando

### 1. Verificar Status dos Containers

```bash
docker ps
```

**Deve mostrar 4 containers rodando (Full Stack):**
- bookmanager-sqlserver (healthy)
- bookmanager-db-init (exited)
- bookmanager-api (healthy)
- bookmanager-frontend (healthy)

### 2. Testar Endpoints

```bash
# Healthcheck da API
curl http://localhost:5000/health

# Frontend
curl http://localhost:4200

# SQL Server (via cliente SQL)
# Server: localhost,1433
# User: sa
# Password: Climaco@123
```

### 3. Abrir no Navegador

- **Frontend:** http://localhost:4200
- **Swagger:** http://localhost:5000/swagger
- **API:** http://localhost:5000/api

---

## Tempo de Inicialização

| Serviço | Tempo Aproximado |
|---------|------------------|
| SQL Server | 60-90 segundos |
| DB Init | 20-30 segundos |
| API | 30-50 segundos |
| Frontend | 20-40 segundos |
| **TOTAL** | **~4 minutos** |

**Aguarde o healthcheck de todos os serviços antes de usar!**

---

## 🔍 Troubleshooting Rápido

### Container não inicia

```bash
# Ver logs detalhados
docker-compose logs <service>

# Exemplo
docker-compose logs api
docker-compose logs sqlserver
```

### Porta já em uso

```bash
# Windows
netstat -ano | findstr :5000
taskkill /PID <PID> /F

# Linux/Mac
lsof -i :5000
kill -9 <PID>
```

### Rebuild limpo

```bash
# Parar tudo
docker-compose down -v

# Remover imagens
docker rmi bookmanager-api bookmanager-frontend

# Rebuild
docker-compose up -d --build --force-recreate
```

### SQL Server não aceita conexão

```bash
# Verificar se está healthy
docker ps

# Entrar no container e testar
docker exec -it bookmanager-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "Climaco@123"
```

---

## Configuração Personalizada

### 1. Usar arquivo .env

```bash
# Copiar exemplo
cp .env.example .env

# Editar configurações
nano .env  # ou vim, code, notepad, etc

# Usar no docker-compose
docker-compose --env-file .env up -d
```

### 2. Alterar portas

Editar `docker-compose.yml`:

```yaml
services:
  api:
    ports:
      - "8080:5000"  # Mude 5000 para 8080
  
  frontend:
    ports:
      - "8081:80"    # Mude 80 para 8081
```

### 3. Mudar senha SQL Server

Em `docker-compose.yml`:

```yaml
services:
  sqlserver:
    environment:
      MSSQL_SA_PASSWORD: "MinhaNewSenha123!"
  
  api:
    environment:
      ConnectionStrings__DefaultConnection: "Server=sqlserver,1433;Database=BookManager;User Id=sa;Password=MinhaNewSenha123!;..."
```

---

## Gerenciar Banco de Dados

### Conectar ao SQL Server

```bash
# Via Docker
docker exec -it bookmanager-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "Climaco@123"

# Via cliente externo (SSMS, Azure Data Studio)
Server: localhost,1433
User: sa
Password: Climaco@123
Database: BookManager
```

### Fazer Backup

```bash
# Usar script
./docker-manager.sh  # Opção 17

# Ou manualmente
docker exec bookmanager-sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P "Climaco@123" \
  -Q "BACKUP DATABASE BookManager TO DISK='/var/opt/mssql/backup/BookManager.bak'"

docker cp bookmanager-sqlserver:/var/opt/mssql/backup/BookManager.bak ./backup.bak
```

### Resetar Banco (Limpar tudo)

```bash
# Usar script
./docker-manager.sh  # Opção 18

# Ou manualmente
docker-compose down
docker volume rm bookmanager-sqlserver-data
docker-compose up -d
```

---

## Próximos Passos

Após subir o ambiente:

1. ✅ Acessar Swagger: http://localhost:5000/swagger
2. ✅ Testar endpoints da API
3. ✅ Acessar Frontend: http://localhost:4200
4. ✅ Cadastrar livros, autores e assuntos
5. ✅ Gerar relatórios

---

## Checklist de Sucesso

- [ ] Docker e Docker Compose instalados
- [ ] Projeto clonado/baixado
- [ ] Executou `docker-compose up -d --build`
- [ ] Aguardou ~2 minutos
- [ ] Todos os containers healthy (`docker ps`)
- [ ] Frontend acessível em http://localhost:4200
- [ ] API acessível em http://localhost:5000
- [ ] Swagger acessível em http://localhost:5000/swagger
- [ ] Consegue cadastrar dados
- [ ] Consegue gerar relatório

---

**🎉 Tudo funcionando? Parabéns!**