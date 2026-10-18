using System.Collections.Concurrent;
using Amazon;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Fiap.TechChallenge.Billing.Domain.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fiap.TechChallenge.Billing.Infrastructure.Aws
{
    public class AwsSsmSecretResolver : ISsmSecretResolver, IDisposable
    {
        private readonly AwsSsmSettings _settings;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AwsSsmSecretResolver> _logger;
        private readonly IAmazonSimpleSystemsManagement? _ssmClient;
        private readonly ConcurrentDictionary<string, string> _cache = new();
        private bool _disposed;

        public AwsSsmSecretResolver(
            IOptions<AwsSsmSettings> settings,
            IConfiguration configuration,
            ILogger<AwsSsmSecretResolver> logger,
            IAmazonSimpleSystemsManagement? ssmClient = null)
        {
            _settings = settings?.Value ?? new AwsSsmSettings();
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (_settings.Enabled)
            {
                try
                {
                    var region = RegionEndpoint.GetBySystemName(_settings.Region ?? "sa-east-1");
                    _ssmClient = ssmClient ?? new AmazonSimpleSystemsManagementClient(region);
                    _logger.LogInformation("AWS SSM Client inicializado para a região {Region}.", region.SystemName);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Falha ao inicializar o cliente AWS SSM. Fallback para variáveis de ambiente/IConfiguration será utilizado.");
                    _ssmClient = null;
                }
            }
            else
            {
                _logger.LogInformation("AWS SSM desabilitado via configuração. Utilizando variáveis locais/IConfiguration.");
            }
        }

        public async Task<string?> ObterParametroAsync(string nomeParametro, bool descriptografar = true, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(nomeParametro))
                return null;

            if (_cache.TryGetValue(nomeParametro, out var cachedValue))
                return cachedValue;

            // 1. Tentar buscar no AWS SSM Parameter Store se habilitado
            if (_settings.Enabled && _ssmClient != null)
            {
                try
                {
                    var request = new GetParameterRequest
                    {
                        Name = nomeParametro,
                        WithDecryption = descriptografar
                    };

                    var response = await _ssmClient.GetParameterAsync(request, cancellationToken);
                    if (response?.Parameter?.Value != null)
                    {
                        _logger.LogInformation("Parâmetro '{ParamName}' obtido com sucesso do AWS SSM.", nomeParametro);
                        _cache[nomeParametro] = response.Parameter.Value;
                        return response.Parameter.Value;
                    }
                }
                catch (ParameterNotFoundException)
                {
                    _logger.LogWarning("Parâmetro '{ParamName}' não encontrado no AWS SSM.", nomeParametro);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Erro ao obter parâmetro '{ParamName}' do AWS SSM. Tentando fallback local.", nomeParametro);
                }
            }

            // 2. Fallback: Variáveis de Ambiente e IConfiguration
            var valorFallback = ResolverFallbackLocal(nomeParametro);
            if (!string.IsNullOrWhiteSpace(valorFallback))
            {
                _cache[nomeParametro] = valorFallback;
                return valorFallback;
            }

            return null;
        }

        public async Task<IDictionary<string, string>> ObterParametrosPorCaminhoAsync(string caminhoBase, bool descriptografar = true, CancellationToken cancellationToken = default)
        {
            var resultado = new Dictionary<string, string>();

            if (_settings.Enabled && _ssmClient != null)
            {
                try
                {
                    string? nextToken = null;
                    do
                    {
                        var request = new GetParametersByPathRequest
                        {
                            Path = caminhoBase,
                            Recursive = true,
                            WithDecryption = descriptografar,
                            NextToken = nextToken
                        };

                        var response = await _ssmClient.GetParametersByPathAsync(request, cancellationToken);
                        foreach (var parameter in response.Parameters)
                        {
                            resultado[parameter.Name] = parameter.Value;
                            _cache[parameter.Name] = parameter.Value;
                        }

                        nextToken = response.NextToken;
                    } while (!string.IsNullOrWhiteSpace(nextToken));

                    return resultado;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Erro ao listar parâmetros por caminho '{Path}' no AWS SSM.", caminhoBase);
                }
            }

            return resultado;
        }

        private string? ResolverFallbackLocal(string nomeParametro)
        {
            // Mapeia parâmetros SSM conhecidos para variáveis de ambiente comuns
            var chaveNormalizada = nomeParametro.TrimStart('/').Replace('/', '_').ToUpperInvariant();
            var envValue = Environment.GetEnvironmentVariable(chaveNormalizada);
            if (!string.IsNullOrWhiteSpace(envValue)) return envValue;

            // Extrair o nome da folha final do parâmetro (ex: mongodb_connection_string)
            var nomeFolha = nomeParametro.Split('/').LastOrDefault()?.ToUpperInvariant();
            if (!string.IsNullOrWhiteSpace(nomeFolha))
            {
                envValue = Environment.GetEnvironmentVariable(nomeFolha);
                if (!string.IsNullOrWhiteSpace(envValue)) return envValue;
            }

            // Mapeamentos específicos por convenção
            if (nomeParametro.Contains("mongodb_connection_string", StringComparison.OrdinalIgnoreCase))
            {
                return _configuration.GetConnectionString("MongoDb")
                    ?? _configuration["MongoDbSettings:ConnectionString"]
                    ?? Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING");
            }

            if (nomeParametro.Contains("mercadopago_access_token", StringComparison.OrdinalIgnoreCase))
            {
                return _configuration["MercadoPagoSettings:AccessToken"]
                    ?? Environment.GetEnvironmentVariable("MERCADOPAGO_ACCESS_TOKEN");
            }

            if (nomeParametro.Contains("mercadopago_webhook_secret", StringComparison.OrdinalIgnoreCase))
            {
                return _configuration["MercadoPagoSettings:WebhookSecret"]
                    ?? Environment.GetEnvironmentVariable("MERCADOPAGO_WEBHOOK_SECRET");
            }

            if (nomeParametro.Contains("jwt_secret", StringComparison.OrdinalIgnoreCase))
            {
                return _configuration["JwtSettings:Secret"]
                    ?? Environment.GetEnvironmentVariable("JWT_SECRET");
            }

            // Fallback genérico no IConfiguration
            return _configuration[nomeParametro] ?? _configuration[chaveNormalizada];
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _ssmClient?.Dispose();
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }
    }
}
