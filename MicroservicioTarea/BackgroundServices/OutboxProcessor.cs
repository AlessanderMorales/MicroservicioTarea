using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MicroservicioTarea.Application.Messaging;
using MicroservicioTarea.Infrastructure.Repository;
using MicroservicioTarea.Domain.Events;

namespace MicroservicioTarea.BackgroundServices
{
    public class OutboxProcessor : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OutboxProcessor> _logger;

        public OutboxProcessor(IServiceProvider serviceProvider, ILogger<OutboxProcessor> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("?? OutboxProcessor iniciado");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var outboxRepository = scope.ServiceProvider.GetRequiredService<OutboxRepository>();
                        var rabbitMQPublisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

                        var pendingEvents = outboxRepository.GetPendingEvents(50);

                        foreach (var outboxEvent in pendingEvents)
                        {
                            try
                            {
                                var evento = JsonSerializer.Deserialize<TareaAsignadaEvent>(outboxEvent.Payload);

                                if (evento != null)
                                {
                                    rabbitMQPublisher.PublishTareaAsignada(evento);
                                    outboxRepository.MarkAsProcessed(outboxEvent.Id);
                                    _logger.LogInformation($"? Evento procesado desde Outbox: {outboxEvent.EventId}");
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"? Error al procesar evento {outboxEvent.EventId}");
                                outboxRepository.MarkAsFailed(outboxEvent.Id, ex.Message);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "? Error en OutboxProcessor");
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }

            _logger.LogInformation("?? OutboxProcessor detenido");
        }
    }
}
