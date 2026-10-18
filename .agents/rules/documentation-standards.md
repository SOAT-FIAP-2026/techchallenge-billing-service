# Regras de Padrões de Documentação Técnica

Diretrizes obrigatórias de documentação técnica para o `techchallenge-billing-service`.

## 1. Padrões de Documentação de API (OpenAPI / Swagger)
- Todo endpoint de controller deve possuir:
  - Comentários XML com `<summary>` claro e objetivo.
  - Atributos `[ProducesResponseType]` para todos os status HTTP possíveis (200, 201, 400, 404, 500, etc.) com seus respectivos tipos de retorno.
  - Parâmetros obrigatórios e validações declaradas via DataAnnotations nos DTOs.
- Headers especiais (`X-Correlation-ID`, `x-signature`, `Authorization: Bearer <token>`) devem ser explicitamente descritos.

## 2. Padrões de Architecture Decision Records (ADRs)
- Decisões arquiteturais relevantes devem ser registradas em `docs/adr/ADR-XXX-<titulo>.md`.
- Estrutura obrigatória:
  - **Título**: Número e descrição curta da decisão.
  - **Status**: `Proposto`, `Aceito`, `Depreciado` ou `Substituído`.
  - **Contexto**: Motivação técnica, requisitos de negócio e restrições (ex: Edital Fase 4 SOAT).
  - **Decisão**: A alternativa técnica selecionada e fundamentação.
  - **Consequências**: Pontos positivos, trade-offs e impactos operacionais/financeiros.

## 3. Padrões de Diagramas
- Utilizar **Mermaid** para diagramas de sequência, topologia e fluxo de transição de estados.
- Evitar imagens estáticas de difícil versionamento; diagramas devem estar sempre como código legível no repositório.

## 4. Manutenção Contínua do README.md
- Todo novo endpoint, variável de ambiente ou segredo no SSM deve ser imediatamente refletido no `README.md` raiz.
- As instruções de execução local e comandos de teste devem ser reproduzíveis por qualquer desenvolvedor da equipe sem dependências ocultas.
