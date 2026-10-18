---
name: billing-architect
description: >-
  Use this skill when designing, refactoring, or reviewing the Clean Architecture structure,
  MongoDB NoSQL schemas, Saga choreography events, or domain rules of the Billing & Payment microservice.
---

# Billing Architecture & Domain Governance Skill

Este guia orienta o agente na aplicação e preservação dos padrões de Clean Architecture, Domain-Driven Design (DDD), modelagem NoSQL e padrão Saga no microsserviço `techchallenge-billing-service`.

## 1. Princípios Arquiteturais Obrigatórios

1. **Pureza do Domínio**:
   - `Fiap.TechChallenge.Billing.Domain` não deve referenciar projetos externos nem bibliotecas de infraestrutura.
   - Entidades ricas (`Orcamento`, `Pagamento`) devem encapsular suas próprias mutações através de métodos explícitos (`Aprovar`, `Cancelar`, `ConfirmarPagamento`, `Estornar`).
2. **Governança NoSQL (MongoDB)**:
   - Coleções: `orcamentos` e `pagamentos`.
   - Registrar mapeamentos em `BsonClassMappings.cs` com convenções padrão (`camelCase`, `IgnoreExtraElements`, `GuidRepresentation.Standard`).
   - Mapear criação de índices em `MongoDbContext.EnsureIndexesAsync()`.
3. **Coreografia da Saga**:
   - Evento de entrada: `DiagnosticoFinalizadoEvent` (consumido do serviço de OS).
   - Evento de sucesso: `PagamentoConfirmadoEvent` (publicado para liberar produção).
   - Evento compensatório: `PagamentoCanceladoCompensacaoEvent` (publicado em timeout/falha).

## 2. Checklist de Validação Arquitetural

Ao propor alterações de código, execute:
```bash
# 1. Validar referências e integridade da compilação
dotnet build Fiap.TechChallenge.Billing.sln

# 2. Verificar que Domain não possui referências proibidas
grep -rn "ProjectReference" src/Fiap.TechChallenge.Billing.Domain/Fiap.TechChallenge.Billing.Domain.csproj || echo "Domain está isolado (OK)"
```

## 3. Padrões de DTOs e Mapeamentos
- DTOs devem residir em `Fiap.TechChallenge.Billing.Application/DTOs/`.
- Nunca expor entidades de domínio diretamente nos controllers REST.
- As respostas de API devem sempre retornar DTOs tipados e documentados para Swagger.
