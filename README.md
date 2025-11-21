# 🚀 API Caixa Invest — Simulador de Investimentos com Perfil de Risco Dinâmico

![.NET 9](https://img.shields.io/badge/.NET-9.0-blueviolet)
![Docker](https://img.shields.io/badge/Docker-Ready-blue)
![Redis](https://img.shields.io/badge/Redis-Enabled-red)
![License](https://img.shields.io/badge/License-MIT-green)
![Status](https://img.shields.io/badge/Status-Production%20Ready-success)
![Tests](https://img.shields.io/badge/Tests-Unit%20%2B%20Integration-brightgreen)

A API Caixa Invest, desenvolvida em **.NET 9.0**, implementa todo o ecossistema necessário para simulação de investimentos, recomendação inteligente de produtos e cálculo automático de perfil de risco, atendendo integralmente ao desafio proposto.

A aplicação foi construída com **Clean Architecture**, banco local **SQLite**, autenticação **JWT**, **Redis** para otimização e segurança, além de testes **unitários** e **de integração** abrangentes.

---

# 🧩 1. Arquitetura da Aplicação

```
ApiCaixaInvest/
├── api/                             → Camada Web (Presentation Layer)
│   ├── controllers/                 → Endpoints REST
│   ├── extensions/                  → DI, Swagger, Auth, Redis
│   ├── middleware/                  → Telemetria, erros
│   └── swaggerexamples/             → Exemplos para Swagger
│
├── application/                     → Camada de aplicação (use cases)
│   ├── Dtos/                        → Objetos de transferência
│   ├── Interfaces/                  → Contratos (ports)
│   └── Options/                     → Configurações (JWT, Redis)
│
├── DataBase/                        → Banco SQLite (.db)
│
├── Domain/                          → Regras de domínio
│   ├── Enuns/
│   └── Models/
│
├── Infraesctrutura/                 → Implementações (adapters)
│   ├── Data/                        → DbContext e EF Core
│   └── Services/                    → Serviços:
│                                      Simulação, PerfilRisco,
│                                      Investimentos, Produtos,
│                                      Telemetria, RedisTokenStore
│
├── Dockerfile
├── docker-compose.yml
└── README.md
```

---

# 🧠 2. Redis — Resumo de Uso na API

O Redis está presente na solução de forma leve e estratégica:

### 🔹 Finalidade
- Armazenamento de refresh tokens com expiração controlada  
- Aumentar segurança evitando reuso de tokens antigos  
- Reduzir leitura do SQLite em operações repetitivas  
- Acelerar respostas de endpoints sensíveis  

### 🔹 Onde é utilizado
- **AuthController** → grava e valida refresh tokens  
- **PerfilRiscoService** → cache leve do último cálculo  
- **ProdutosService** → cache de produtos por risco  

O Redis sobe automaticamente pelo **docker-compose**.

---

# 🔍 3. Perfil de Risco Dinâmico — Coração da Aplicação

O motor calcula automaticamente o perfil com base em:

- Volume total investido  
- Frequência de movimentações  
- Liquidez dos produtos  
- Rentabilidade média  
- Exposição a ativos de alto risco  

### Classificação:

| Pontuação Total | Perfil |
|-----------------|--------|
| 0–80            | Conservador |
| 81–140          | Moderado |
| ≥ 141           | Agressivo |

A API entrega também:

### ✔️ Perfil Detalhado  
Inclui tendência Markoviana e próximo perfil provável.

### ✔️ Perfil com IA  
Explicação personalizada em linguagem natural.

---

# 📡 4. Endpoints da API

| Categoria | Método | Endpoint | Descrição |
|----------|--------|----------|-----------|
| **Auth** | POST | `/api/auth/login` | Login (JWT + RefreshToken) |
| **Auth** | POST | `/api/auth/refresh-token` | Renova token |
| **Auth** | GET | `/api/auth/me` | Teste de autenticação |
| **Clientes** | GET | `/api/clientes` | Lista clientes |
| **Clientes** | GET | `/api/clientes/{id}` | Cliente por ID |
| **Investimentos** | GET | `/api/investimentos/{clienteId}` | Histórico |
| **Investimentos** | POST | `/api/investimentos/efetivar` | Efetiva simulações |
| **Perfil** | GET | `/api/perfil-risco/{clienteId}` | Perfil básico |
| **Perfil** | GET | `/api/perfil-risco/detalhado/{clienteId}` | Perfil detalhado |
| **Perfil** | GET | `/api/perfil-risco-ia/{clienteId}` | Perfil IA |
| **Produtos** | GET | `/api/produtos` | Lista produtos |
| **Produtos** | GET | `/api/produtos/{id}` | Produto por ID |
| **Produtos** | GET | `/api/produtos/risco/{risco}` | Produtos por risco |
| **Recomendações** | GET | `/api/recomendacoes/produtos/{perfil}` | Recomendação por perfil |
| **Recomendações** | GET | `/api/recomendacoes/cliente/{clienteId}` | Recomendação automática |
| **Simulações** | POST | `/api/simular-investimento` | Simula investimento |
| **Simulações** | POST | `/api/simular-e-contratar-investimento` | Simula e efetiva |
| **Simulações** | GET | `/api/simulacoes` | Histórico |
| **Simulações** | GET | `/api/simulacoes/por-produto-dia` | Agrupado por dia |
| **Telemetria** | GET | `/api/telemetria` | Métricas da API |

---

# 🧪 5. Testes Automatizados

### ✔️ Unit Tests
- Perfil de risco  
- Simulações  
- Recomendações  
- Produtos  
- Autenticação (mock Redis)

### ✔️ Integration Tests
- Login real  
- Simular + efetivar  
- Recomendações completas  
- Telemetria real  
- Contexto EF com SQLite em memória  

---

# 🐳 6. Executando com Docker

### Comando único:

```bash
docker compose up --build
```

### Serviços iniciados:

| Serviço | Porta | Função |
|--------|--------|--------|
| API | http://localhost:8080 | Endpoints REST |
| Redis | 6379 | Cache / Tokens / Otimizações |

Swagger:
👉 **http://localhost:8080/swagger**

---

# 🏁 Conclusão

A API Caixa Invest entrega:

✔ Arquitetura limpa  
✔ Cálculo inteligente de perfil  
✔ IA explicativa  
✔ Redis para segurança e performance  
✔ Testes completos  
✔ Docker pronto para uso  
✔ Documentação limpa e objetiva  

Pronta para produção, análise técnica ou apresentação.

