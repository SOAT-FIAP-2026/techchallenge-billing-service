# Regras de Nuvem, Kubernetes e Observabilidade

Diretrizes para operação em nuvem AWS, Kubernetes e observabilidade unificada.

## 1. Infraestrutura Efêmera e Custo Zero
- A infraestrutura em nuvem no AWS EKS e RDS opera sob o princípio de destruição automatizada via `terraform destroy` ao final das avaliações.
- O Parameter Store do AWS SSM (`/techchallenge/prod/*`) é utilizado para manter segredos permanentes sem custos.

## 2. Resolução Dinâmica de Segredos
- Em ambiente de produção/EKS, a aplicação deve ler seus parâmetros diretamente do AWS SSM Parameter Store.
- Em desenvolvimento local ou testes em container, a aplicação deve executar com fallback gracioso para `appsettings.json` ou variáveis de ambiente.

## 3. Padrões Kubernetes
- Services devem ser expostos via `NodePort` para evitar criação descontrolada de Classic Load Balancers pela AWS (o Terraform gerencia o ALB).
- Probes obrigatórias:
  - Liveness: `/health/live` (valida se o processo .NET está em execução).
  - Readiness: `/health/ready` (valida se a conexão com MongoDB e mensageria está operacional).
- O deployment deve possuir resource requests e limits declarados para suporte ao HPA (Horizontal Pod Autoscaler).

## 4. Observabilidade e Telemetria
- Propagação obrigatória do cabeçalho `X-Correlation-ID` em todas as requisições para rastreabilidade ponta a ponta.
- Traces distribuídos via OTLP enviados para o OTel Collector.
- Métricas expostas no endpoint padrão `/metrics` no formato Prometheus.
