using Npgsql;
using Studium_Audit.Services.Handlers;
using Wolverine;
using Wolverine.ErrorHandling;
using Wolverine.RabbitMQ;

namespace Studium_Audit.Api.Extensions;

public static class MessagingExtensions
{
    public static void AddStudiumMessaging(this WebApplicationBuilder builder)
    {
        builder.Host.UseWolverine(opts =>
        {
            opts.Discovery.IncludeAssembly(typeof(AuditIntegrationEventHandler).Assembly);

            var rabbitConnectionString = builder.Configuration["RabbitMQ:ConnectionString"]
                ?? builder.Configuration["RABBITMQ_URL"]
                ?? "amqp://studium:StudiumRabbitMQ2026!@studium-rabbitmq-service.studium.svc.cluster.local:5672";

            opts.UseRabbitMq(new Uri(rabbitConnectionString)).AutoProvision();

            var auditQueueName = builder.Configuration["RabbitMQ:AuditQueueName"] ?? "studium-audit-events-queue";
            opts.ListenToRabbitQueue(auditQueueName);

            opts.Policies.AutoApplyTransactions();

            opts.OnException<NpgsqlException>()
                .RetryWithCooldown(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(5));

            opts.OnException<TimeoutException>()
                .RetryWithCooldown(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(5));

            opts.OnException<HttpRequestException>()
                .RetryWithCooldown(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(5));

            opts.OnException<Exception>()
                .MoveToErrorQueue();
        });
    }
}
