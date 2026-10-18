using Fiap.TechChallenge.Billing.Api.Configurations;
using Fiap.TechChallenge.Billing.Api.Middlewares;
using Fiap.TechChallenge.Billing.Api.Observability;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Fiap.TechChallenge.Billing.Api
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddDependencyInjection(Configuration);
            services.AddJWTConfig(Configuration);
            services.AddSwaggerConfig();
            services.AddObservability(Configuration);
        }

        public void Configure(WebApplication app, IWebHostEnvironment env)
        {
            app.UseMiddleware<CorrelationIdMiddleware>();
            app.UseMiddleware<RequestObservabilityMiddleware>();
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "TechChallenge Billing API v1");
                c.RoutePrefix = "swagger";
            });

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = HealthCheckResponseWriter.WriteAsync
            });

            app.MapHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = _ => false,
                ResponseWriter = HealthCheckResponseWriter.WriteAsync
            });

            app.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("ready"),
                ResponseWriter = HealthCheckResponseWriter.WriteAsync
            });

            app.MapPrometheusScrapingEndpoint("/metrics");
        }
    }
}
