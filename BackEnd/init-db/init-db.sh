#!/bin/bash
set -e

SERVER="sqlserver,1433"
USER="sa"
PASSWORD="Climaco@123"
SCRIPTS_DIR="/app/init-db"
MAX_RETRIES=50
RETRY_INTERVAL=3

echo "==========================================="
echo "Inicializando Banco de Dados BookManager"
echo "==========================================="

# Procura pelo sqlcmd nas versões mais recentes e antigas
if [ -x "/opt/mssql-tools18/bin/sqlcmd" ]; then
  SQLCMD="/opt/mssql-tools18/bin/sqlcmd"
  # Versão 18 requer -C para confiar no certificado
  SQLCMD_OPTS="-C"
  echo "✓ Usando sqlcmd versão 18"
elif [ -x "/opt/mssql-tools/bin/sqlcmd" ]; then
  SQLCMD="/opt/mssql-tools/bin/sqlcmd"
  SQLCMD_OPTS=""
  echo "✓ Usando sqlcmd versão legacy"
else
  echo "✗ ERRO: sqlcmd não encontrado no container"
  echo "Caminhos verificados:"
  echo "  - /opt/mssql-tools18/bin/sqlcmd"
  echo "  - /opt/mssql-tools/bin/sqlcmd"
  exit 127
fi

echo ""
echo "Aguardando SQL Server aceitar conexões em $SERVER..."
echo "Tentativas máximas: $MAX_RETRIES (intervalo: ${RETRY_INTERVAL}s)"
echo ""

# Tenta conectar até MAX_RETRIES vezes
CONNECTED=0
for i in $(seq 1 $MAX_RETRIES); do
  echo -n "[$i/$MAX_RETRIES] Tentando conectar... "
  
  if $SQLCMD $SQLCMD_OPTS -S "$SERVER" -U "$USER" -P "$PASSWORD" -Q "SELECT 1" -d master &>/dev/null; then
    CONNECTED=1
    echo "✓ SUCESSO!"
    echo ""
    echo "✓ Conexão estabelecida com sucesso!"
    break
  fi
  
  echo "✗ Falhou"
  
  if [ $i -lt $MAX_RETRIES ]; then
    echo "   Aguardando ${RETRY_INTERVAL}s antes da próxima tentativa..."
    sleep $RETRY_INTERVAL
  fi
done

if [ $CONNECTED -eq 0 ]; then
  echo ""
  echo "==========================================="
  echo "✗ ERRO CRÍTICO"
  echo "==========================================="
  echo "Não foi possível conectar ao SQL Server"
  echo "após $MAX_RETRIES tentativas."
  echo ""
  echo "Verifique:"
  echo "  - Se o container do SQL Server está rodando"
  echo "  - Se as credenciais estão corretas"
  echo "  - Os logs do container sqlserver"
  echo "==========================================="
  exit 1
fi

echo "==========================================="
echo "Executando Scripts SQL"
echo "==========================================="
echo ""

# Contador de scripts executados
SCRIPTS_EXECUTED=0
SCRIPTS_FAILED=0

# Executa todos os scripts .sql na ordem alfabética
for SCRIPT in $(ls $SCRIPTS_DIR/*.sql 2>/dev/null | sort); do
  SCRIPT_NAME=$(basename "$SCRIPT")
  echo "-------------------------------------------"
  echo "Executando: $SCRIPT_NAME"
  echo "-------------------------------------------"

  # Tenta executar o script com retry
  SCRIPT_SUCCESS=0
  for attempt in $(seq 1 20); do
    if [ $attempt -gt 1 ]; then
      echo "Tentativa $attempt/20..."
    fi

    if $SQLCMD $SQLCMD_OPTS \
      -S "$SERVER" \
      -U "$USER" \
      -P "$PASSWORD" \
      -i "$SCRIPT" \
      -b 2>&1; then
      SCRIPT_SUCCESS=1
      break
    fi
    
    if [ $attempt -lt 20 ]; then
      echo "Falha na execução, tentando novamente em 2s..."
      sleep 20
    fi
  done

  if [ $SCRIPT_SUCCESS -eq 1 ]; then
    echo "✓ $SCRIPT_NAME executado com sucesso"
    SCRIPTS_EXECUTED=$((SCRIPTS_EXECUTED + 1))
  else
    echo "✗ ERRO: Falha ao executar $SCRIPT_NAME após 20 tentativas"
    SCRIPTS_FAILED=$((SCRIPTS_FAILED + 1))
  fi
  
  echo ""
done

echo "==========================================="
if [ $SCRIPTS_FAILED -eq 0 ]; then
  echo "✓ SUCESSO!"
  echo "==========================================="
  echo "Banco de dados inicializado com sucesso!"
  echo ""
  echo "Scripts executados: $SCRIPTS_EXECUTED"
  echo ""
  echo "Lista de scripts:"
  ls $SCRIPTS_DIR/*.sql 2>/dev/null | sort | while read script; do
    echo "  ✓ $(basename "$script")"
  done
  echo ""
  echo "✓ Banco BookManager está pronto para uso!"
  echo "==========================================="
  exit 0
else
  echo "✗ FALHA PARCIAL"
  echo "==========================================="
  echo "Scripts executados: $SCRIPTS_EXECUTED"
  echo "Scripts com erro: $SCRIPTS_FAILED"
  echo ""
  echo "Verifique os logs acima para mais detalhes"
  echo "==========================================="
  exit 1
fi