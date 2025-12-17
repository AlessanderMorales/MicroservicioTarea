using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Dapper;
using MicroservicioTarea.Domain.Entities;
using MicroservicioTarea.Infrastructure.Persistence;
using MicroservicioTarea.Infrastructure.Repository;
using MySql.Data.MySqlClient;

namespace MicroservicioTarea.Infrastructure.Repository
{
    public class TareaUsuarioRepository
    {
        private readonly MySqlConnectionSingleton _connection;
        private readonly OutboxRepository _outboxRepository;

        public TareaUsuarioRepository(MySqlConnectionSingleton connection, OutboxRepository outboxRepository)
        {
            _connection = connection;
            _outboxRepository = outboxRepository;
        }

        public IEnumerable<TareaUsuario> GetByTareaId(int idTarea)
        {
            using var conn = _connection.CreateConnection();
            const string sql = @"
                SELECT 
                    id, 
                    id_tarea AS IdTarea, 
                    id_usuario AS IdUsuario,
                    fecha_asignacion AS FechaAsignacion, 
                    estado
                FROM Tarea_Usuario
                WHERE id_tarea = @IdTarea AND estado = 1";
            return conn.Query<TareaUsuario>(sql, new { IdTarea = idTarea });
        }

        public void AssignUsers(int idTarea, IEnumerable<int> usuarios, string tareaTitulo, string usuarioNombre)
        {
            using var conn = _connection.CreateConnection();
            conn.Open();
            
            using var transaction = conn.BeginTransaction();
            
            try
            {
                var usuariosList = usuarios.ToList();
                var idsString = usuariosList.Any() ? string.Join(",", usuariosList) : string.Empty;
                
                const string sql = "CALL sp_asignar_usuarios_a_tarea(@IdTarea, @IdsUsuarios);";
                conn.Execute(sql, new { IdTarea = idTarea, IdsUsuarios = idsString }, transaction);

                if (usuariosList.Any())
                {
                    var outboxEvent = new OutboxEvent
                    {
                        EventId = Guid.NewGuid().ToString(),
                        EventType = "TareaAsignadaEvent",
                        Payload = JsonSerializer.Serialize(new
                        {
                            TareaId = idTarea,
                            EmpleadosIds = usuariosList,
                            UsuarioNombre = usuarioNombre,
                            FechaEvento = DateTime.Now,
                            TareaTitulo = tareaTitulo
                        }),
                        CreatedAt = DateTime.Now
                    };

                    _outboxRepository.SaveWithTransaction(outboxEvent, conn, transaction);
                }
                
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public IEnumerable<int> GetTareasByUsuario(int idUsuario)
        {
            using var conn = _connection.CreateConnection();
            const string sql = @"
                SELECT id_tarea
                FROM Tarea_Usuario
                WHERE id_usuario = @IdUsuario AND estado = 1";
            return conn.Query<int>(sql, new { IdUsuario = idUsuario });
        }
    }
}
