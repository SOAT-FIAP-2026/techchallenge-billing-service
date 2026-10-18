---
name: billing-docs
description: >-
  Use this skill when creating, updating, or reviewing technical documentation, Architecture Decision Records (ADRs),
  OpenAPI/Swagger specs, Mermaid diagrams, Postman collections, or developer guides for the Billing & Payment microservice.
---

# Billing Docs & Technical Writing Skill

Este guia orienta o agente na produção, revisão e padronização da documentação técnica e arquitetural do microsserviço `techchallenge-billing-service`.

## 1. Escopos de Atuação

1. **Documentação de API (OpenAPI / Swagger)**:
   - Garantir que todos os endpoints dos controllers (`OrcamentoController`, `PagamentoController`) estejam devidamente anotados com summaries, tags e status codes.
   - Manter exemplos realistas de payloads de request e response.
2. **Architecture Decision Records (ADRs)**:
   - Redigir e manter registros de decisão em `docs/adr/`.
   - ADRs essenciais do repositório:
     - `ADR-001`: Adoção do MongoDB 7.0 para persistência de webhooks dinâmicos e snapshots imutáveis.
     - `ADR-002`: Implementação da Saga Coreografada com compensação para transações financeiras.
     - `ADR-003`: Resolução dinâmica de segredos via AWS SSM Parameter Store e Fallback Local.
     - `ADR-004`: Validação de Webhooks Mercado Pago com assinatura criptográfica HMAC SHA-256.
3. **Diagramas Visuais (Mermaid)**:
   - Manter diagramas de fluxo de pagamento, rollback de compensação e topologia de componentes sincronizados com o código.
4. **README.md e Guias de Execução**:
   - Manter comandos de Docker, variáveis de ambiente e endpoints públicos atualizados.

## 2. Template Padrão para ADRs

```markdown
# ADR-XXX: [Título Sucinto da Decisão]

- **Data**: AAAA-MM-DD
- **Status**: [Proposto | Aceito | Depreciado | Substituído]
- **Autores**: Equipe SOAT FIAP
- **Contexto**: [Explicação do problema técnico e restrições do projeto]

## Decisão
[Descrição da alternativa escolhida e justificativa técnica]

## Consequências
### Positivas:
- [Benefício 1]
- [Benefício 2]

### Negativas / Trade-offs:
- [Trade-off ou complexidade introduzida]
```

## 3. Checklist de Validação da Documentação

- [ ] Todos os novos endpoints possuem summaries XML e `ProducesResponseType`?
- [ ] O `README.md` reflete as variáveis de ambiente necessárias?
- [ ] As decisões de alto impacto possuem ADR correspondente em `docs/adr/`?
- [ ] Os diagramas Mermaid renderizam sem erros de sintaxe?
- [ ] Links relativos entre arquivos markdown estão válidos?
