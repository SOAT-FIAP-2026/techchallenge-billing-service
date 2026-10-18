namespace Fiap.TechChallenge.Billing.Api.Configurations
{
    public static class JwtSecretResolver
    {
        public const string EnvironmentVariableName = "JWT_SECRET";
        public const string ConfigurationKey = "Jwt:Secret";
        public const int MinimumLength = 32;

        public static string Resolve(IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            string? secret = Environment.GetEnvironmentVariable(EnvironmentVariableName);

            if (string.IsNullOrWhiteSpace(secret))
                secret = configuration[ConfigurationKey];

            if (string.IsNullOrWhiteSpace(secret))
            {
                // Fallback de desenvolvimento padrão se nenhum configurado
                return "chave-secreta-jwt-techchallenge-fiap-2026-segura";
            }

            if (secret.Length < MinimumLength)
            {
                throw new InvalidOperationException(
                    $"A chave de assinatura do JWT precisa ter pelo menos {MinimumLength} caracteres.");
            }

            return secret;
        }
    }
}
