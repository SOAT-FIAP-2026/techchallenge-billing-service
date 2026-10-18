using Fiap.TechChallenge.Billing.Domain.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;

namespace Fiap.TechChallenge.Billing.Infrastructure.Data
{
    public static class BsonClassMappings
    {
        private static bool _isInitialized;
        private static readonly object _lock = new();

        public static void RegisterMappings()
        {
            if (_isInitialized) return;

            lock (_lock)
            {
                if (_isInitialized) return;

                // 1. Convenções Globais: camelCase e ignorar elementos extras no JSON/BSON
                var conventionPack = new ConventionPack
                {
                    new CamelCaseElementNameConvention(),
                    new IgnoreExtraElementsConvention(true),
                    new EnumRepresentationConvention(BsonType.String)
                };
                ConventionRegistry.Register("BillingConventions", conventionPack, _ => true);

                // 2. Serializadores Padrão para Guid e Decimal
                try
                {
                    BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
                }
                catch (BsonSerializationException) { /* Já registrado */ }

                try
                {
                    BsonSerializer.RegisterSerializer(new DecimalSerializer(BsonType.Decimal128));
                }
                catch (BsonSerializationException) { /* Já registrado */ }

                // 3. Mapeamento EntidadeBase
                if (!BsonClassMap.IsClassMapRegistered(typeof(EntidadeBase)))
                {
                    BsonClassMap.RegisterClassMap<EntidadeBase>(cm =>
                    {
                        cm.AutoMap();
                        cm.SetIsRootClass(true);
                        cm.MapIdProperty(c => c.Id);
                    });
                }

                // 4. Mapeamento ItemOrcamento
                if (!BsonClassMap.IsClassMapRegistered(typeof(ItemOrcamento)))
                {
                    BsonClassMap.RegisterClassMap<ItemOrcamento>(cm =>
                    {
                        cm.AutoMap();
                        cm.MapCreator(i => new ItemOrcamento(i.Descricao, i.Tipo, i.Quantidade, i.ValorUnitario, i.ItemCatalogoId));
                    });
                }

                // 5. Mapeamento DadosPagador
                if (!BsonClassMap.IsClassMapRegistered(typeof(DadosPagador)))
                {
                    BsonClassMap.RegisterClassMap<DadosPagador>(cm =>
                    {
                        cm.AutoMap();
                        cm.MapCreator(p => new DadosPagador(p.Cpf, p.Nome, p.Email));
                    });
                }

                // 6. Mapeamento RegistroWebhook
                if (!BsonClassMap.IsClassMapRegistered(typeof(RegistroWebhook)))
                {
                    BsonClassMap.RegisterClassMap<RegistroWebhook>(cm =>
                    {
                        cm.AutoMap();
                    });
                }

                // 7. Mapeamento Orcamento
                if (!BsonClassMap.IsClassMapRegistered(typeof(Orcamento)))
                {
                    BsonClassMap.RegisterClassMap<Orcamento>(cm =>
                    {
                        cm.AutoMap();
                    });
                }

                // 8. Mapeamento Pagamento
                if (!BsonClassMap.IsClassMapRegistered(typeof(Pagamento)))
                {
                    BsonClassMap.RegisterClassMap<Pagamento>(cm =>
                    {
                        cm.AutoMap();
                    });
                }

                _isInitialized = true;
            }
        }
    }
}
