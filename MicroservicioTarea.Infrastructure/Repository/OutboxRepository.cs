using System;
using System.Collections.Generic;
using System.Data;
using Dapper;
using MicroservicioTarea.Domain.Entities;
using MicroservicioTarea.Infrastructure.Persistence;
using MySql.Data.MySqlClient;

namespace MicroservicioTarea.Infrastructure.Repository
{
    public class OutboxRepository
    {
        private readonly MySqlConnectionSingleton _connection;

        public OutboxRepository(MySqlConnectionSingleton connection)
        {
            _connection = connection;
        }

        public void Save(OutboxEvent outboxEvent)
        {
            using var conn = _connection.CreateConnection();
            const string sql = @"
                INSERT INTO Outbox (event_id, event_type, payload, created_at, processed, retry_count)
                VALUES (@EventId, @EventType, @Payload, @CreatedAt, 0, 0)";
            
            conn.Execute(sql, outboxEvent);
        }

        public void SaveWithTransaction(OutboxEvent outboxEvent, IDbConnection connection, IDbTransaction transaction)
        {
            const string sql = @"
                INSERT INTO Outbox (event_id, event_type, payload, created_at, processed, retry_count)
                VALUES (@EventId, @EventType, @Payload, @CreatedAt, 0, 0)";
            
            connection.Execute(sql, outboxEvent, transaction);
        }

        public IEnumerable<OutboxEvent> GetPendingEvents(int limit = 100)
        {
            using var conn = _connection.CreateConnection();
            const string sql = @"
                SELECT id AS Id, event_id AS EventId, event_type AS EventType, 
                       payload AS Payload, created_at AS CreatedAt, processed AS Processed,
                       processed_at AS ProcessedAt, retry_count AS RetryCount, error_message AS ErrorMessage
                FROM Outbox
                WHERE processed = 0 AND retry_count < 5
                ORDER BY created_at ASC
                LIMIT @Limit";
            
            return conn.Query<OutboxEvent>(sql, new { Limit = limit });
        }

        public void MarkAsProcessed(long id)
        {
            using var conn = _connection.CreateConnection();
            const string sql = @"
                UPDATE Outbox
                SET processed = 1, processed_at = @ProcessedAt
                WHERE id = @Id";
            
            conn.Execute(sql, new { Id = id, ProcessedAt = DateTime.Now });
        }

        public void MarkAsFailed(long id, string errorMessage)
        {
            using var conn = _connection.CreateConnection();
            const string sql = @"
                UPDATE Outbox
                SET retry_count = retry_count + 1, error_message = @ErrorMessage
                WHERE id = @Id";
            
            conn.Execute(sql, new { Id = id, ErrorMessage = errorMessage });
        }
    }
}
