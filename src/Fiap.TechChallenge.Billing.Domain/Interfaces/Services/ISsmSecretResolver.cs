namespace Fiap.TechChallenge.Billing.Domain.Interfaces.Services
{
    public interface ISsmSecretResolver
    {
        Task<string?> ObterParametroAsync(string nomeParametro, bool descriptografar = true, CancellationToken cancellationToken = default);
        Task<IDictionary<string, string>> ObterParametrosPorCaminhoAsync(string caminhoBase, bool descriptografar = true, CancellationToken cancellationToken = default);
    }
}
