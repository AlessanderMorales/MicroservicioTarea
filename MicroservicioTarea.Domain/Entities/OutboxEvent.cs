using System;

namespace MicroservicioTarea.Domain.Entities
{
    public class OutboxEvent
    {
        public long Id { get; set; }
        public string EventId { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool Processed { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public int RetryCount { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
