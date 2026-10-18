# Regras de Planejamento e Exploração de Contexto Pré-Codificação

Diretrizes obrigatórias para o agente de planejamento antes do início de qualquer ciclo de desenvolvimento.

## 1. Princípio Fundamental: "Explorar e Entender Antes de Codificar"
- Nenhuma alteração estrutural ou implementação de feature deve iniciar sem antes:
  1. Mapear os arquivos relevantes no repositório local e nos repositórios irmãos (`fase1-tech-challenge`, `soat-infra`, etc.).
  2. Consultar os documentos mestres de arquitetura:
     - [`PLANO_DESACOPLAMENTO_SERVICO_PAGAMENTO.md`](file:///home/gabriel/Desktop/FIAP%20-%20TC/PLANO_DESACOPLAMENTO_SERVICO_PAGAMENTO.md)
     - [`CONTEXTO_COMPLETO_PROJETO_SOAT.md`](file:///home/gabriel/Desktop/FIAP%20-%20TC/CONTEXTO_COMPLETO_PROJETO_SOAT.md)
     - [`FASE_4.md`](file:///home/gabriel/Desktop/FIAP%20-%20TC/FASE_4.md)
  3. Identificar os contratos de dados (DTOs, eventos de mensageria, coleções MongoDB) e dependências de infraestrutura (SSM, Docker, K8s).

## 2. Análise de Impacto Multi-Serviço
- O `techchallenge-billing-service` é desacoplado mas se comunica diretamente com o ecossistema:
  - **OS Service (`fase1-tech-challenge`)**: Eventos de Saga (`DiagnosticoFinalizadoEvent`, `PagamentoConfirmadoEvent`, `PagamentoCanceladoCompensacaoEvent`).
  - **Auth Gateway (`lambda-auth-function`)**: Validação de tokens JWT emitidos com claims do cliente.
  - **Cloud/Infra (`soat-infra` e `soat-db`)**: Parâmetros no AWS SSM Parameter Store (`/techchallenge/prod/*`) e regras de roteamento no ALB.
- O planejamento deve explicitamente auditar o impacto em cada um desses pontos.

## 3. Estrutura Obrigatória do Plano de Ação
Todo plano elaborado deve conter:
- **Objetivo Claro**: Descrição do que será resolvido/criado.
- **Contexto & Diagnóstico Atual**: O que já existe e quais são as lacunas.
- **Arquivos & Componentes Alvo**: Caminhos absolutos e métodos impactados.
- **Passo a Passo de Execução**: Tarefas atômicas atribuídas aos agentes executores (`@architect`, `@security-integrator`, `@qa-tester`, `@cloud-ops`, `@docs`).
- **Critérios de Aceite & Validação**: Comandos específicos (`dotnet build`, `dotnet test`, `curl`, probes) para comprovar a eficácia da implementação.
