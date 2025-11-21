# API Caixa Invest – Simulador com Perfil de Risco Dinâmico

## Visão Geral

A **API Caixa Invest** é uma solução desenvolvida em **.NET 9.0**, com arquitetura limpa, documentação abrangente e autenticação JWT.  
Seu objetivo é fornecer uma infraestrutura completa para:

- Simulação de investimentos  
- Efetivação de simulações  
- Cálculo dinâmico do perfil de risco  
- Recomendação de produtos adequados  
- Registro de telemetria  
- Histórico de investimentos e simulações  

A plataforma atende integralmente todos os requisitos do enunciado do desafio.

---

# 1. Arquitetura da Aplicação

A API segue uma arquitetura organizada em camadas:

```
ApiCaixaInvest/
 ├── api/                              → Camada de apresentação (Web API)
 │    ├── controllers/                 → Controllers HTTP (endpoints REST)
 │    ├── extensions/                  → Métodos de extensão (DI, Swagger, Auth, etc.)
 │    ├── middleware/                  → Middlewares customizados (telemetria, erros, etc.)
 │    └── swaggerexamples/             → Exemplos de request/response para Swagger (Swashbuckle)
 │
 ├── application/                      → Camada de aplicação (orquestra regras de negócio)
 │    ├── Dtos/                        → DTOs de entrada/saída da API
 │    ├── Interfaces/                  → Contratos dos serviços (Ports)
 │    └── Options/                     → Bindings de configurações (ex.: JWT, Redis, etc.)
 │
 ├── DataBase/                         → Banco de dados SQLite (arquivo .db e scripts auxiliares)
 │
 ├── Domain/                           → Núcleo de domínio (regras puras)
 │    ├── Enuns/                       → Enums de domínio (Perfil de risco, tipos, etc.)
 │    └── Models/                      → Entidades de domínio (Cliente, Produto, Simulação, Investimento...)
 │
 ├── Infraesctrutura/                  → Implementações concretas (Adapters)
 │    ├── Data/                        → DbContext, mapeamentos EF Core, repositórios
 │    └── Services/                    → Serviços concretos (Simulação, PerfilRisco, Investimentos,
 │                                      Produtos, Telemetria, RedisTokenStore, etc.)
 │
 ├── Dockerfile                        → Build da imagem da API (.NET 9 + SQLite)
 ├── docker-compose.yml                → Orquestração da API + Redis
 └── README.md                         → Documentação do projeto e instruções de execução

```

---

# 2. Fluxo Geral da Solução

### 2.1. Simulação de Investimento
1. Cliente envia:
   ```json
   { "clienteId": 123, "valor": 10000, "prazoMeses": 12, "tipoProduto": "CDB" }
   ```
2. API valida parâmetros.
3. Busca produtos no banco compatíveis.
4. Seleciona o melhor produto (maior rentabilidade).
5. Calcula valor final.
6. Registra a simulação no banco.
7. Retorna envelope JSON conforme enunciado.

### 2.2. Efetivação da Simulação
- Marca simulações como efetivadas.
- Movimenta o histórico de investimentos.
- Recalcula automaticamente o perfil de risco do cliente.

### 2.3. Cálculo de Perfil de Risco Dinâmico
Baseado em:
- Volume total investido
- Frequência de movimentações (últimos 12 meses)
- Preferência por liquidez
- Preferência por rentabilidade
- Exposição a produtos de maior risco

### 2.4. Recomendação de Produtos
- Baseada no perfil de risco atualizado.
- Alinha risco do produto com perfil atual.

---

# 3. Perfil de Risco Dinâmico (Explicação Detalhada)

A seguir está a explicação completa do algoritmo implementado.

---

## 3.1. Etapas do Cálculo do Perfil

### **1) Carregamento do Histórico**
Busca todos investimentos efetivados para o cliente.

- Se não houver histórico → **Conservador (20 pontos)**.

---

### **2) Volume Total Investido**
Quanto maior o volume, maior a tolerância ao risco.

### Pontuação por Volume Total Investido

| Faixa de Volume (R$)         | Pontos |
|------------------------------|--------|
| **0 a 4.999,99**             | 10     |
| **5.000,00 a 19.999,99**     | 20     |
| **20.000,00 a 99.999,99**    | 30     |
| **≥ 100.000,00**             | 40     |

---

### **3) Frequência de Movimentações (12 meses)**
Mensura comportamento ativo do cliente.

| Movimentações | Pontos |
|---------------|--------|
| 0             | 10     |
| 1             | 20     |
| 2 a 6         | 30     |
| > 6           | 40     |

---

### **4) Exposição a Ativos de Alto Risco**
Percentual da carteira em produtos como  
fundos, ações, multimercado, cripto.

Pontos = `% de exposição * 40`

---

### **5) Liquidez Média dos Produtos**
Quanto menor a liquidez, mais aversão a ser identificado.

### Pontuação por Liquidez

| Faixa de Liquidez (dias) | Pontos |
|--------------------------|--------|
| **0 a 30**               | 40     |
| **31 a 90**              | 25     |
| **> 90**                 | 10     |

---

### **6) Rentabilidade Média dos Produtos**
Avalia se o cliente busca maior retorno.

### Pontuação por Rentabilidade

| Rentabilidade Anual (a.a.) | Pontos |
|----------------------------|--------|
| **0% até 7,99%**           | 10     |
| **8,0% até 11,99%**         | 20     |
| **12,0% até 19,99%**        | 30     |
| **≥ 20%**                  | 40     |


---

## 3.2. Pontuação Final e Classificação

| Pontuação Total | Perfil Resultante |
|-----------------|-------------------|
| 0 – 80          | **Conservador**   |
| 81 – 140        | **Moderado**      |
| 141+            | **Agressivo**     |

---

## 3.3. Descrição Inteligente
O algoritmo monta automaticamente uma explicação detalhada considerando:

- Volume investido
- Frequência de movimentações
- Exposição ao risco
- Rentabilidade média
- Liquidez

### Exemplo:
> “Perfil moderado: equilíbrio entre segurança e rentabilidade, com alguma exposição a ativos de maior risco.  
> Pontuação 96. Total investido: R$ 52.000,00. Movimentações nos últimos 12 meses: 5. Liquidez média: 45 dias. Rentabilidade média: 11%. Exposição a risco alto: 22%.”


---

# 4. Endpoints Disponíveis

Todos estão documentados via **Swagger/OpenAPI**.

## 📌 Tabela de Endpoints da API

| Categoria | Método | Endpoint | Descrição |
|----------|--------|----------|-----------|
| **Auth** | POST | `/api/auth/login` | Autentica o usuário e retorna **JWT + RefreshToken**. |
| **Auth** | POST | `/api/auth/refresh-token` | Renova o token usando um refresh token válido. |
| **Auth** | GET | `/api/auth/me` | Retorna dados do usuário autenticado (teste do JWT). |
| **Clientes** | GET | `/api/clientes` | Lista todos os clientes registrados. |
| **Clientes** | GET | `/api/clientes/{id}` | Retorna dados de um cliente específico. |
| **Investimentos** | GET | `/api/investimentos/{clienteId}` | Histórico de investimentos efetivados do cliente. |
| **Investimentos** | POST | `/api/investimentos/efetivar` | Efetiva simulações e recalcula o perfil de risco. |
| **Perfil de Risco** | GET | `/api/perfil-risco/{clienteId}` | Perfil de risco **básico**, conforme o enunciado. |
| **Perfil de Risco** | GET | `/api/perfil-risco/detalhado/{clienteId}` | Perfil **detalhado**, com liquidez, frequência, carteira e tendência Markoviana. |
| **Perfil de Risco** | GET | `/api/perfil-risco-ia/{clienteId}` | Explicação em linguagem natural (IA). |
| **Produtos** | GET | `/api/produtos` | Lista todos os produtos de investimento. |
| **Produtos** | GET | `/api/produtos/{id}` | Consulta um produto específico. |
| **Produtos** | GET | `/api/produtos/risco/{risco}` | Filtra produtos por risco (`Baixo`, `Médio/Medio`, `Alto`). |
| **Recomendações** | GET | `/api/recomendacoes/produtos/{perfil}` | Recomenda produtos para um perfil informado. |
| **Recomendações** | GET | `/api/recomendacoes/cliente/{clienteId}` | Recomenda produtos com base no **perfil real** do cliente. |
| **Simulações** | POST | `/api/simular-investimento` | Simula um investimento e retorna produto validado + resultado. |
| **Simulações** | POST | `/api/simular-e-contratar-investimento` | Simula **e efetiva** o investimento em uma única operação. |
| **Simulações** | GET | `/api/simulacoes` | Histórico de todas as simulações realizadas. |
| **Simulações** | GET | `/api/simulacoes/por-produto-dia` | Resumo de simulações agrupadas por produto e dia. |
| **Telemetria** | GET | `/api/telemetria?inicio=yyyy-MM-dd&fim=yyyy-MM-dd` | Retorna métricas de uso: volume de chamadas e tempo médio por serviço. |

---

# 5. Segurança

A API utiliza:

- **JWT Bearer Token**  
- Políticas de autorização  
- Erros padronizados  
- Validação rigorosa de entrada  
- Telemetria para monitoramento de comportamento

---

# 6. Como Executar a Aplicação

### **1. Restaurar pacotes**
```
dotnet restore
```

### **2. Aplicar migrations**
```
dotnet ef database update
```

### **3. Rodar a API**
```
dotnet run
```

### **4. Acessar documentação Swagger**
```
http://localhost:5000/swagger
```

---

# 7. Execução via Docker

A aplicação já possui:

- Dockerfile
- docker-compose.yml

Execução:

```
docker compose up --build
```

---

# 8. Banco de Dados

A API usa **Entity Framework Core** com migrations automáticas.  
O banco é criado e atualizado automaticamente com:

```
dotnet ef database update
```

---

# 9. Testes Propostos para Avaliação

Segue um checklist de testes para validar todas funcionalidades:

### ✔️ **Simulação**
- Simular valores válidos
- Simular com cliente inexistente (criação automática)
- Simular produto inexistente
- Simular com rentabilidade diferente
- Validar JSON de resposta conforme enunciado

### ✔️ **Efetivação**
- Efetivar várias simulações
- Efetivar simulando erro
- Efetivar e verificar histórico
- Recalcular perfil após efetivação

### ✔️ **Perfil de risco dinâmico**
- Cliente sem histórico → conservador
- Cliente com investimentos pequenos → conservador
- Cliente com frequência alta → moderado/agressivo
- Cliente com produtos agressivos → agressivo
- Após novas simulações → perfil muda

### ✔️ **Recomendações**
- Testar para cada perfil (Conservador | Moderado | Agressivo)

### ✔️ **Produtos**
- Listagem completa
- Filtro por risco
- Filtro inexistente

### ✔️ **Telemetria**
- Chamar endpoints e validar contadores

### ✔️ **Autenticação**
- Token inválido deve retornar 401
- Token expirado deve retornar 401
- Endpoint `/me` deve validar corretamente

---


