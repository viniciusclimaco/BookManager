#!/bin/bash

echo "========================================="
echo "BookManager - Executando Testes Unitários"
echo "========================================="
echo ""

# Cores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Navegar para o diretório do projeto de testes
cd "$(dirname "$0")"

echo -e "${YELLOW}Restaurando pacotes...${NC}"
dotnet restore
if [ $? -ne 0 ]; then
    echo -e "${RED}Erro ao restaurar pacotes${NC}"
    exit 1
fi
echo -e "${GREEN}Pacotes restaurados com sucesso${NC}"
echo ""

echo -e "${YELLOW}🏗️  Compilando projeto de testes...${NC}"
dotnet build --no-restore
if [ $? -ne 0 ]; then
    echo -e "${RED}Erro na compilação${NC}"
    exit 1
fi
echo -e "${GREEN}Compilação concluída com sucesso${NC}"
echo ""

echo -e "${YELLOW}Executando testes...${NC}"
echo ""
dotnet test --no-build --logger "console;verbosity=normal"

TEST_EXIT_CODE=$?

echo ""
if [ $TEST_EXIT_CODE -eq 0 ]; then
    echo -e "${GREEN}Todos os testes passaram!${NC}"
else
    echo -e "${RED}Alguns testes falharam${NC}"
fi
echo ""

echo -e "${YELLOW}Gerando relatório de cobertura...${NC}"
dotnet test --no-build /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput=./coverage/

echo ""
echo -e "${GREEN}Relatório de cobertura gerado em: ./coverage/coverage.opencover.xml${NC}"
echo ""

echo "========================================="
echo "Resumo:"
echo "========================================="
if [ $TEST_EXIT_CODE -eq 0 ]; then
    echo -e "Status: ${GREEN}SUCESSO ✓${NC}"
else
    echo -e "Status: ${RED}FALHOU ✗${NC}"
fi
echo ""
echo "Para ver o relatório detalhado de cobertura:"
echo "  - Abra o arquivo coverage/coverage.opencover.xml"
echo "  - Ou use ferramentas como ReportGenerator para HTML"
echo ""

exit $TEST_EXIT_CODE
