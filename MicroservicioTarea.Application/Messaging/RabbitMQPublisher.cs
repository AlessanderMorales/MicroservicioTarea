using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using MicroservicioTarea.Domain.Events;

namespace MicroservicioTarea.Application.Messaging
{
    public class RabbitMQPublisher : IEventPublisher, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IChannel _channel;
        private readonly string _exchangeName;
        private readonly ILogger<RabbitMQPublisher> _logger;

        public RabbitMQPublisher(IConfiguration configuration, ILogger<RabbitMQPublisher> logger)
        {
            _logger = logger;

            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = configuration["RabbitMQ:HostName"] ?? "localhost",
                    Port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672"),
                    UserName = configuration["RabbitMQ:UserName"] ?? "guest",
                    Password = configuration["RabbitMQ:Password"] ?? "guest",
                    VirtualHost = configuration["RabbitMQ:VirtualHost"] ?? "/"
                };

                _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
                _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
                _exchangeName = configuration["RabbitMQ:ExchangeName"] ?? "tareas.events";

                // Declarar el exchange (tipo topic para routing flexible)
                _channel.ExchangeDeclareAsync(
                    exchange: _exchangeName,
                    type: ExchangeType.Topic,
                    durable: true,
                    autoDelete: false
                ).GetAwaiter().GetResult();

                _logger.LogInformation("? RabbitMQ Publisher conectado exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError($"? Error al conectar con RabbitMQ: {ex.Message}");
                throw;
            }
        }

        public void PublishTareaAsignada(TareaAsignadaEvent evento)
        {
            try
            {
                var message = JsonSerializer.Serialize(evento);
                var body = Encoding.UTF8.GetBytes(message);

                var properties = new BasicProperties
                {
                    Persistent = true,
                    ContentType = "application/json",
                    Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                };

                _channel.BasicPublishAsync(
                    exchange: _exchangeName,
                    routingKey: "tarea.asignada",
                    mandatory: false,
                    basicProperties: properties,
                    body: body
                ).GetAwaiter().GetResult();

                _logger.LogInformation($"?? Evento publicado: TareaAsignada (TareaId: {evento.TareaId}, Empleados: {evento.EmpleadosIds.Count})");
            }
            catch (Exception ex)
            {
                _logger.LogError($"? Error al publicar evento: {ex.Message}");
                throw;
            }
        }

        public void Dispose()
        {
            _channel?.CloseAsync().GetAwaiter().GetResult();
            _connection?.CloseAsync().GetAwaiter().GetResult();
            _logger.LogInformation("?? RabbitMQ Publisher desconectado");
        }
    }
}

