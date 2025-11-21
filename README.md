🚀 API Caixa Invest — Simulador de Investimentos com Perfil de Risco Dinâmico

A API Caixa Invest, desenvolvida em .NET 9.0, implementa todo o ecossistema necessário para simulação de investimentos, recomendação inteligente de produtos e cálculo automático de perfil de risco, atendendo integralmente ao desafio proposto.

A aplicação foi construída com Clean Architecture, banco local SQLite, autenticação JWT, Redis como suporte a segurança/performance, e cobertura com testes unitários e de integração.

---

# 🧩 1. Arquitetura da Aplicação

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
│   └── Services/                    → Serviços concretos:
│                                      Simulação, PerfilRisco,
│                                      Investimentos, Produtos,
│                                      Telemetria, RedisTokenStore
│
├── Dockerfile
├── docker-compose.yml
└── README.md


---
🧠 2. Redis — Resumo de Uso na API

O Redis está presente na solução de forma leve e estratégica:

🔹 Finalidade

Armazenar refresh tokens com expiração controlada

Aumentar segurança, evitando reuso de tokens antigos

Minimizar acessos ao SQLite em operações repetitivas

Suporte a mecanismos de autenticação mais eficientes

🔹 Onde é utilizado

AuthController → grava e valida refresh tokens

PerfilRiscoService → pode armazenar último cálculo (cache leve)

ProdutosService → usa cache em consultas de produtos por risco

O Redis sobe automaticamente pelo docker-compose sem configuração adicional.

---

# 🔍 3. Perfil de Risco Dinâmico — Coração da Aplicação

O motor calcula automaticamente o perfil com base em:

- Volume total investido  
- Frequência de movimentações  
- Liquidez dos produtos  
- Rentabilidade média  
- Exposição a ativos de alto risco  

Classificação:

| Pontuação Total | Perfil |
|-----------------|--------|
| 0–80            | Conservador |
| 81–140          | Moderado |
| ≥ 141           | Agressivo |

Além do cálculo básico, a API oferece:

### ✔️ Perfil Detalhado  
Inclui tendência Markoviana e próximo perfil provável.

### ✔️ Perfil com IA  
Gera explicações em linguagem natural, com resumo, ações recomendadas e alertas personalizados.

---

# 📡 4. Endpoints da API (Tabela Completa)

### 🔐 **Autenticação**
| Método | Endpoint | Descrição |
|--------|----------|-----------|
| POST | `/api/auth/login` | Login + token JWT + refresh token |
| POST | `/api/auth/refresh-token` | Renova token |
| GET | `/api/auth/me` | Teste de autenticação |

### 👤 **Clientes**
| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/api/clientes` | Lista clientes existentes |
| GET | `/api/clientes/{id}` | Consulta cliente específico |

### 💼 **Investimentos**
| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/api/investimentos/{clienteId}` | Histórico do cliente |
| POST | `/api/investimentos/efetivar` | Efetiva simulações e recalcula perfil |

### 📊 **Perfil de Risco**
| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/api/perfil-risco/{clienteId}` | Perfil básico |
| GET | `/api/perfil-risco/detalhado/{clienteId}` | Perfil detalhado (Liquidez, Tendência, etc.) |
| GET | `/api/perfil-risco-ia/{clienteId}` | Explicação em linguagem natural |

### 🏦 **Produtos**
| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/api/produtos` | Lista todos os produtos |
| GET | `/api/produtos/{id}` | Consulta por ID |
| GET | `/api/produtos/risco/{risco}` | Filtra por risco |

### 🧠 **Recomendações**
| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/api/recomendacoes/produtos/{perfil}` | Recomenda por perfil informado |
| GET | `/api/recomendacoes/cliente/{clienteId}` | Recomenda com base no perfil real do cliente |

### 🧮 **Simulações**
| Método | Endpoint | Descrição |
|--------|----------|-----------|
| POST | `/api/simular-investimento` | Simula investimento |
| POST | `/api/simular-e-contratar-investimento` | Simula e efetiva em uma única operação |
| GET | `/api/simulacoes` | Histórico completo |
| GET | `/api/simulacoes/por-produto-dia` | Análise agrupada por produto e dia |

### 📈 **Telemetria**
| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/api/telemetria?inicio=AAAA-MM-DD&fim=AAAA-MM-DD` | Volume + tempo médio por serviço |

---

# 🧪 5. Testes Automatizados

A API possui **cobertura completa de testes**, incluindo:

## ✔️ Testes Unitários  
- Motor de Perfil de Risco  
- Simulações  
- Efetivação  
- Recomendações  
- Produtos  
- Telemetria  
- Autenticação (mock Redis)

## ✔️ Testes de Integração  
Executados contra o servidor real em memória:

- Autenticação real (Login)  
- Simular + Efetivar + Consultar perfil  
- Ciclo completo de investimentos  
- Recomendações baseadas no comportamento real  

Os testes garantem **confiabilidade**, **regressão zero** e **aderência ao enunciado**.

---

# 🐳 6. Executando com Docker

### Requisitos
- Docker  
- Docker Compose  

### Comando único:

```bash
docker compose up --build


Serviços iniciados:

Serviço	Porta	Função
API	http://localhost:8080	Endpoints REST
Redis	6379	Armazenamento de tokens

Swagger disponível em:

👉 http://localhost:8080/swagger

🏁 Conclusão

A API Caixa Invest entrega:

✔ Arquitetura limpa
✔ Cálculo inteligente de perfil
✔ IA explicativa
✔ Redis para segurança
✔ Testes completos
✔ Docker pronto
✔ Documentação Swagger
✔ Total aderência ao desafio

Pronto para produção, avaliação ou extensão.

Se quiser, posso gerar também:
🔥 versão curta,
🔥 versão para apresentação,
🔥 versão corporativa,
🔥 versão com badges e shields para GitHub.