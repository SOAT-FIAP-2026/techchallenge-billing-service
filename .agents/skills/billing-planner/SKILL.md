---
name: billing-planner
description: >-
  Use this skill before starting any coding task to thoroughly explore the codebase, understand cross-service
  dependencies, gather accurate context from related repositories and documentation, and formulate a step-by-step execution plan.
---

# Billing Planner & Context Explorer Skill

Este guia orienta o agente de planejamento na exploração ativa de contexto, investigação de dependências entre microsserviços e estruturação de planos de ação antes de qualquer linha de código ser escrita ou alterada.

## 1. Rotina de Descoberta e Exploração de Contexto

Antes de iniciar qualquer tarefa de codificação, execute os seguintes passos de investigação:

### Passo 1: Leitura das Diretrizes de Arquitetura e Contexto
- Consultar [`PLANO_DESACOPLAMENTO_SERVICO_PAGAMENTO.md`](file:///home/gabriel/Desktop/FIAP%20-%20TC/PLANO_DESACOPLAMENTO_SERVICO_PAGAMENTO.md) para identificar em qual fase a demanda se insere.
- Consultar [`CONTEXTO_COMPLETO_PROJETO_SOAT.md`](file:///home/gabriel/Desktop/FIAP%20-%20TC/CONTEXTO_COMPLETO_PROJETO_SOAT.md) para entender os requisitos do edital da Fase 4 da pós-graduação.

### Passo 2: Mapeamento de Dependências e Contratos Multi-Repositório
- **OS Service ([`fase1-tech-challenge`](file:///home/gabriel/Desktop/FIAP%20-%20TC/fase1-tech-challenge))**:
  - Verificar modelos de dados relacionados (`OrdemServico`, `Orcamento`).
  - Auditar nomes de eventos, payloads JSON e filas/tópicos da Saga.
- **Auth Gateway ([`lambda-auth-function`](file:///home/gabriel/Desktop/FIAP%20-%20TC/lambda-auth-function))**:
  - Verificar estrutura do token JWT, secret utilizado e claims (`sub`, CPF).
- **Infraestrutura em Nuvem ([`soat-infra`](file:///home/gabriel/Desktop/FIAP%20-%20TC/soat-infra))**:
  - Verificar caminhos dos parâmetros no AWS SSM Parameter Store (`/techchallenge/prod/*`).
  - Mapear Target Groups e portas NodePort do ALB.

### Passo 3: Identificação de Pontos Críticos e Edge Cases
- **Idempotência**: Requisições de cobrança duplicadas ou notificações repetidas do Mercado Pago.
- **Saga Rollback**: Cenários em que o pagamento expira ou é rejeitado e requer emissão de evento de compensação.
- **Segurança**: Validação de HMAC SHA-256 no webhook e proteção contra vazamento de dados em logs.
- **Disponibilidade**: Comportamento da API caso o MongoDB ou a mensageria estejam temporariamente indisponíveis (readiness probe).

---

## 2. Template Padrão do Plano de Ação

Ao concluir a exploração, o `@planner` deve produzir um resumo estruturado no formato:

```markdown
### 📋 Plano de Execução Técnica

#### 1. Objetivo & Escopo
[Resumo conciso da tarefa e valor gerado]

#### 2. Contexto Descoberto & Arquivos Envolvidos
- [Arquivo 1](file:///caminho/arquivo1) (Descrição do papel)
- [Arquivo 2](file:///caminho/arquivo2) (Descrição do papel)

#### 3. Roteiro Passo a Passo & Atribuição aos Agentes
1. [ ] **Etapa 1 — @architect**: [Ação específica de arquitetura/domínio]
2. [ ] **Etapa 2 — @security-integrator**: [Ação de integração ou segurança]
3. [ ] **Etapa 3 — @qa-tester**: [Testes unitários e cenários BDD Reqnroll]
4. [ ] **Etapa 4 — @cloud-ops**: [Docker, K8s, SSM ou CI/CD]
5. [ ] **Etapa 5 — @docs**: [Atualização de Swagger, ADRs e README]

#### 4. Critérios de Aceite & Comandos de Validação
- `dotnet build`: Compilação com 0 erros e 0 warnings.
- `dotnet test`: Todos os testes unitários e BDD passando com cobertura >= 80%.
- Validação manual de endpoints via curl/Swagger.
```

---

## 3. Checklist de Validação do Planejamento

- [ ] Todos os arquivos alvo foram inspecionados antes de propor alterações?
- [ ] O impacto nos outros microsserviços do ecossistema SOAT foi considerado?
- [ ] Os edge cases de compensação da Saga e idempotência foram mapeados?
- [ ] Os agentes especializados adequados foram convocados para cada etapa?
- [ ] Comandos reproduzíveis de validação foram especificados?
