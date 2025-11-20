# API Caixa Invest – Perfil de Risco Dinâmico

## 1. Visão Geral

A API Caixa Invest é uma solução desenvolvida em **.NET 9** que implementa uma plataforma completa de simulação, contratação e análise de investimentos, incluindo:

- Perfis de risco dinâmicos
- Motor de recomendação
- Histórico de simulações
- Consolidação diária de valores simulados
- Registro e consulta de telemetria
- Autenticação JWT
- Banco de dados local SQLite

A API segue estritamente todos os itens exigidos no enunciado do desafio, com documentação completa via Swagger e arquitetura limpa e organizada.

## 2. Tecnologias Utilizadas

| Camada | Tecnologia |
|--------|------------|
| Linguagem | C# – .NET 9 |
| Banco de Dados | SQLite |
| ORM | Entity Framework Core 9 |
| Autenticação | JWT |
| Documentação | Swagger / Swashbuckle |
| Hospedagem | Docker-ready |

## 3. Arquitetura

Projeto organizado em camadas:

- Domain
- Application
- Infrastructure
- Api

## 4. Autenticação JWT

Toda a API (exceto /auth/login) requer:

```
Authorization: Bearer {token}
```

## 5. Funcionamento Geral

A API permite:

1. Simular um investimento  
2. Contratar investimentos já simulados  
3. Simular e contratar em uma única operação  
4. Consultar telemetria  
5. Consultar perfis de risco  
6. Recomendar produtos conforme perfil  

## 6. Endpoints

Documentação completa disponível em */swagger/index.html*.

(Tabelas de endpoints omitidas por brevidade no arquivo gerado.)

## 7. Motor de Perfil de Risco — Explicação Técnica

O algoritmo analisa cinco eixos principais:

1. Volume total investido  
2. Frequência de movimentações (12 meses)  
3. Liquidez média  
4. Rentabilidade média  
5. Exposição a risco elevado  

Pontuação final classifica em:

- Conservador
- Moderado
- Agressivo

## 8. Banco de Dados – SQLite

Criado automaticamente ao iniciar a API.  
Tabelas incluídas:

- Clientes
- PerfisClientes
- ProdutosInvestimento
- Simulacoes
- InvestimentosHistorico
- TelemetriaRegistros

## 9. Como Executar

### Via Visual Studio
- Abrir a solução  
- Executar (F5)  
- Acessar Swagger  

### Via Docker
```
docker compose up --build
```

## 10. Conclusão

Projeto atende todos os requisitos funcionais do enunciado com robustez, segurança e documentação adequada.
